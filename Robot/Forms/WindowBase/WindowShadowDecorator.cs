using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vanara.Extensions;
using Vanara.PInvoke;
using Robot.Forms.BorderlessForm;
using static Vanara.PInvoke.Gdi32;
using static Vanara.PInvoke.User32;



namespace Robot.Forms
{

    /// <summary>
    /// 窗口阴影装饰器: 通过四个环绕目标窗口的透明子窗口绘制阴影, 并随窗口状态/激活/尺寸变化动态刷新。
    /// </summary>
    internal sealed class WindowShadowDecorator : NativeWindow, IDisposable
    {
        /// <summary>
        /// 各阴影效果对应的默认配置(偏移与模糊半径)。
        /// </summary>
        readonly Dictionary<ShadowEffect, ShadowEffectConfiguration> ShadowEffectConfiguration = new Dictionary<ShadowEffect, ShadowEffectConfiguration>()
        {
            [ShadowEffect.None] = new ShadowEffectConfiguration { OffsetX = 0, OffsetY = 0, Blur = 0 },
            [ShadowEffect.Glow] = new ShadowEffectConfiguration { OffsetX = 0, OffsetY = 0, Blur = 4 },
            [ShadowEffect.Small] = new ShadowEffectConfiguration { OffsetX = 1, OffsetY = 1, Blur = 7 },
            [ShadowEffect.Normal] = new ShadowEffectConfiguration { OffsetX = 1, OffsetY = 4, Blur = 10 },
            [ShadowEffect.Big] = new ShadowEffectConfiguration { OffsetX = 2, OffsetY = 4, Blur = 15 },
            [ShadowEffect.Huge] = new ShadowEffectConfiguration { OffsetX = 3, OffsetY = 5, Blur = 20 },
            [ShadowEffect.DropShadow] = new ShadowEffectConfiguration { OffsetX = 5, OffsetY = 15, Blur = 25 },
        };

        /// <summary>
        /// 混合模式: 源覆盖 (SRC_OVER)。
        /// </summary>
        const int AcSrcOver = 0x00;

        /// <summary>
        /// 混合模式: 源 alpha (SRC_ALPHA)。
        /// </summary>
        const int AcSrcAlpha = 0x01;

        /// <summary>
        /// 默认层叠混合函数: 使用源 alpha 覆盖, 不透明度 0xff。
        /// </summary>
        internal BLENDFUNCTION DefaultBlendFunciton { get; } = new BLENDFUNCTION
        {
            AlphaFormat = AcSrcAlpha,
            BlendOp = AcSrcOver,
            SourceConstantAlpha = 0xff,
            BlendFlags = 0x00
        };

        /// <summary>
        /// 当前阴影效果。
        /// </summary>
        private ShadowEffect _shadowEffcet = ShadowEffect.DropShadow;

        /// <summary>
        /// 窗口激活时的阴影颜色。
        /// </summary>
        private Color _shadowActiveColor = ColorTranslator.FromHtml("#99303030");

        /// <summary>
        /// 窗口非激活时的阴影颜色, 为 null 时按激活色 60% 透明度派生。
        /// </summary>
        private Color? _shadowInactiveColor = null;

        /// <summary>
        /// 阴影元素是否已初始化。
        /// </summary>
        private bool _isShadowInitialized = false;

        /// <summary>
        /// 阴影是否已启用。
        /// </summary>
        private bool _isShadowEnabled = false;




        /// <summary>
        /// 上一次记录的窗口状态, 用于判断窗口是否处于非普通状态。
        /// </summary>
        private FormWindowState? _lastWindowState = null;

        /// <summary>
        /// 上一次阴影元素状态, 用于判断是否需要更新位图。
        /// </summary>
        private ShadowElementState? _lastShadowState = null;

        /// <summary>
        /// 四个方向的阴影元素窗口集合。
        /// </summary>
        private List<ShadowElementWindow> ShadowElements { get; } = new List<ShadowElementWindow>();


        /// <summary>
        /// 顶部阴影元素窗口。
        /// </summary>
        private ShadowElementWindow? _topElementWindow;

        /// <summary>
        /// 右侧阴影元素窗口。
        /// </summary>
        private ShadowElementWindow? _rightElementWindow;

        /// <summary>
        /// 底部阴影元素窗口。
        /// </summary>
        private ShadowElementWindow? _bottomElmentWindow;

        /// <summary>
        /// 左侧阴影元素窗口。
        /// </summary>
        private ShadowElementWindow? _leftElementWindow;


        /// <summary>
        /// 阴影是否随窗口一同显示(即窗口处于非普通状态)。
        /// </summary>
        private bool IsShadowShownWithWindow => _lastWindowState.HasValue;

        /// <summary>
        /// 阴影动画取消源, 用于中断进行中的延迟显示任务。
        /// </summary>
        private CancellationTokenSource _shadowCancellationSource = new CancellationTokenSource();


        /// <summary>
        /// 阴影元素当前是否可见。
        /// </summary>
        public bool IsVisible { get; private set; }

        /// <summary>
        /// 当前阴影元素渲染器, 按效果与颜色生成阴影位图。
        /// </summary>
        public ShadowElementRender? ShadowElementRender { get; private set; }

        /// <summary>
        /// 窗口激活时的阴影颜色, 变化后若已初始化则重建渲染器。
        /// </summary>
        public Color ShadowActiveColor
        {
            get => _shadowActiveColor;
            set
            {
                if (value != _shadowActiveColor)
                {
                    _shadowActiveColor = value;

                    if (_isShadowInitialized) CreateShadowElementRender();
                }
            }
        }

        /// <summary>
        /// 窗口非激活时的阴影颜色; 未显式设置时按激活色 60% 透明度派生, 设为透明则回退为派生值。
        /// </summary>
        public Color ShadowInactiveColor
        {
            get => _shadowInactiveColor ?? Color.FromArgb(Convert.ToByte(_shadowActiveColor.A * 0.6f), _shadowActiveColor);
            set
            {
                if (value != _shadowInactiveColor)
                {


                    _shadowInactiveColor = value;

                    if (_shadowInactiveColor == Color.Transparent)
                    {
                        _shadowInactiveColor = null;
                    }

                    if (_isShadowInitialized) CreateShadowElementRender();
                }
            }
        }

        /// <summary>
        /// 窗口阴影效果, 变化后若已初始化则重建渲染器。
        /// </summary>
        public ShadowEffect WindowShadowEffect
        {
            get => _shadowEffcet;
            set
            {
                if (value != _shadowEffcet)
                {
                    _shadowEffcet = value;

                    if (_isShadowInitialized) CreateShadowElementRender();
                }
            }
        }

        /// <summary>
        /// 被装饰的目标窗口。
        /// </summary>
        public BorderlessWindow TargetWindow { get; }

        /// <summary>
        /// 目标窗口的所有者窗口。
        /// </summary>
        public Form? TargetWindowOwner => TargetWindow!.Owner;

        /// <summary>
        /// 装饰器自身句柄。
        /// </summary>
        public HWND WND { get; private set; }

        /// <summary>
        /// 当前阴影效果对应的配置。
        /// </summary>
        public ShadowEffectConfiguration ShadowElementConfiguration => ShadowEffectConfiguration[WindowShadowEffect];

        /// <summary>
        /// 初始化窗口阴影装饰器, 绑定目标窗口并订阅其事件。
        /// </summary>
        /// <param name="targetWindow">需要装饰阴影的目标窗口。</param>
        public WindowShadowDecorator(BorderlessWindow targetWindow)
        {
            TargetWindow = targetWindow;

            RegisterTargetWindowEvents();
        }

        /// <summary>
        /// 启用或禁用阴影: 启用时按当前状态刷新或延迟显示, 禁用时立即隐藏。
        /// </summary>
        /// <param name="enable">是否启用阴影。</param>
        public void EnableShadow(bool enable)
        {

            if (enable)
            {
                _isShadowEnabled = true;
                if (_lastShadowState != null)
                {
                    UpdateShadowElements(true);
                }
                else
                {
                    UpdateShadowElements(true, true, 100);
                }
            }
            else
            {
                UpdateShadowElements(false);
                _isShadowEnabled = false;
            }
        }

        /// <summary>
        /// 更新所有阴影元素的 Z 序, 必要时同步更新所有者窗口的阴影。
        /// </summary>
        /// <param name="updateOwner">是否同步更新所有者窗口的阴影 Z 序。</param>
        public void UpdateZOrder(bool updateOwner = false)
        {
            if (!_isShadowInitialized || !IsVisible) return;

            foreach (var element in ShadowElements)
            {
                element.UpdateZOrder();
            }

            if (TargetWindow.Owner != null && updateOwner)
            {

                if (TargetWindow.Owner is BorderlessWindow)
                {
                    var owner = TargetWindow.Owner as BorderlessWindow;

                    owner?.ShadowDecorator.UpdateZOrder(false);
                }
            }
        }

        #region TargetWindow Events
        /// <summary>
        /// 订阅目标窗口的句柄/显示/状态/激活/关闭事件。
        /// </summary>
        private void RegisterTargetWindowEvents()
        {
            TargetWindow.HandleCreated += TargetWindow_HandleCreated;
            TargetWindow.HandleDestroyed += TargetWindow_HandleDestroyed;
            TargetWindow.Shown += TargetWindow_Shown;
            TargetWindow.WindowStateChanged += TargetWindow_WindowStateChanged;
            TargetWindow.WindowActivated += TargetWindow_WindowActivated;
            TargetWindow.FormClosed += TargetWindow_FormClosed;
        }

        /// <summary>
        /// 取消订阅目标窗口的全部事件。
        /// </summary>
        private void UnregisterTargetWindowEvents()
        {
            TargetWindow.FormClosed -= TargetWindow_FormClosed;
            TargetWindow.WindowActivated -= TargetWindow_WindowActivated;
            TargetWindow.WindowStateChanged -= TargetWindow_WindowStateChanged;
            TargetWindow.Shown -= TargetWindow_Shown;
            TargetWindow.HandleDestroyed -= TargetWindow_HandleDestroyed;
            TargetWindow.HandleCreated -= TargetWindow_HandleCreated;
        }

        /// <summary>
        /// 目标窗口句柄创建: 接管句柄并创建阴影元素。
        /// </summary>
        private void TargetWindow_HandleCreated(object sender, EventArgs e)
        {
            AssignHandle(TargetWindow.Handle);

            WND = new HWND(TargetWindow.Handle);

            CreateShadowElements();
        }

        /// <summary>
        /// 目标窗口句柄销毁: 释放句柄。
        /// </summary>
        private void TargetWindow_HandleDestroyed(object sender, EventArgs e)
        {
            ReleaseHandle();
        }

        /// <summary>
        /// 目标窗口激活状态变化: 按激活与否更新阴影 Z 序。
        /// </summary>
        private void TargetWindow_WindowActivated(object sender, WindowActivatedEventArgs e)
        {
            UpdateZOrder(e.IsActivated);
        }

        /// <summary>
        /// 目标窗口状态变化: 记录状态并按还原/非普通状态刷新或隐藏阴影。
        /// </summary>
        private void TargetWindow_WindowStateChanged(object sender, WindowStateChangedEventArgs e)
        {
            if (TargetWindow.WindowState != FormWindowState.Normal)
            {
                _lastWindowState = TargetWindow.WindowState;
            }

            if (e.State == WindowChangeState.Restore)
            {
                if (_lastWindowState != FormWindowState.Normal)
                {
                    if (_lastWindowState == null)
                    {

                        UpdateShadowElements(true, true, 150);
                    }
                    else
                    {
                        UpdateShadowElements(true, true, 200);

                    }
                }
                else
                {
                    UpdateShadowElements(true);

                }
            }
            else
            {
                UpdateShadowElements(false);
            }
        }

        /// <summary>
        /// 目标窗口显示: 普通状态且效果非 None 时启用阴影, 否则禁用。
        /// </summary>
        private void TargetWindow_Shown(object sender, EventArgs e)
        {
            if (TargetWindow.WindowState == FormWindowState.Normal && WindowShadowEffect != ShadowEffect.None)
            {
                EnableShadow(true);

            }
            else
            {
                EnableShadow(false);
            }
        }

        /// <summary>
        /// 目标窗口关闭: 禁用阴影。
        /// </summary>
        private void TargetWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            EnableShadow(false);
        }





        #endregion





        #region ShadowElement operations
        /// <summary>
        /// 为所有阴影元素设置所有者窗口句柄。
        /// </summary>
        /// <param name="owner">所有者窗口句柄。</param>
        private void SetOwner(HWND owner)
        {
            foreach (var element in ShadowElements)
            {
                element.SetOwner(owner);
            }
        }

        /// <summary>
        /// 按当前效果与颜色重建阴影渲染器, 并按模板尺寸调整目标窗口最小尺寸。
        /// </summary>
        private void CreateShadowElementRender()
        {
            var shadowConfig = ShadowElementConfiguration;

            if (WindowShadowEffect == ShadowEffect.None)
            {
                ShadowElementRender = null;
                return;
            }

            ShadowElementRender = new ShadowElementRender(shadowConfig, ShadowActiveColor, ShadowInactiveColor);

            var setMinWidth = TargetWindow.MinimumSize.Width;
            var setMinHeight = TargetWindow.MinimumSize.Height;

            var templateMinWidth = ShadowElementRender.TemplateBoxSize.Width;
            var templateMinHeight = ShadowElementRender.TemplateBoxSize.Height;

            if (TargetWindow.MinimumSize == Size.Empty || setMinHeight < templateMinHeight && setMinWidth < templateMinWidth)
            {
                TargetWindow.MinimumSize = new Size(templateMinWidth, templateMinHeight);
            }
            else if (setMinHeight < templateMinHeight)
            {
                TargetWindow.MinimumSize = new Size(setMinWidth, templateMinHeight);
            }
            else if (setMinWidth < templateMinWidth)
            {
                TargetWindow.MinimumSize = new Size(templateMinWidth, setMinHeight);
            }

        }

        /// <summary>
        /// 创建四个方向的阴影元素窗口, 设置所有者并标记为已初始化。
        /// </summary>
        private void CreateShadowElements()
        {
            CreateShadowElementRender();

            if (_isShadowInitialized) return;

            _topElementWindow = new ShadowElementWindow(ShadowElementPosition.Top, this);
            _rightElementWindow = new ShadowElementWindow(ShadowElementPosition.Right, this);
            _bottomElmentWindow = new ShadowElementWindow(ShadowElementPosition.Bottom, this);
            _leftElementWindow = new ShadowElementWindow(ShadowElementPosition.Left, this);

            ShadowElements.Add(_topElementWindow);
            ShadowElements.Add(_rightElementWindow);
            ShadowElements.Add(_bottomElmentWindow);
            ShadowElements.Add(_leftElementWindow);

            if (TargetWindow.Owner != null)
            {
                SetOwner(TargetWindow.Owner.Handle);
            }


            _isShadowInitialized = true;

        }

        /// <summary>
        /// 关闭并销毁所有阴影元素窗口。
        /// </summary>
        private void DestroyShadowElements()
        {
            foreach (var element in ShadowElements)
            {
                element.Close();
            }
        }

        /// <summary>
        /// 用于周期性刷新阴影 Z 序的定时器。
        /// </summary>
        System.Timers.Timer? _updateZOrderTimer = null;

        /// <summary>
        /// 显示所有阴影元素, 并启动周期性 Z 序刷新定时器。
        /// </summary>
        private void ShowShadowElements()
        {
            if (!_isShadowInitialized || !_isShadowEnabled) return;

            ShowWindow(_topElementWindow!.WND, ShowWindowCommand.SW_SHOWNOACTIVATE);
            ShowWindow(_rightElementWindow!.WND, ShowWindowCommand.SW_SHOWNOACTIVATE);
            ShowWindow(_bottomElmentWindow!.WND, ShowWindowCommand.SW_SHOWNOACTIVATE);
            ShowWindow(_leftElementWindow!.WND, ShowWindowCommand.SW_SHOWNOACTIVATE);


            if (_updateZOrderTimer != null)
            {
                _updateZOrderTimer.Stop();
                _updateZOrderTimer.Close();
                _updateZOrderTimer.Dispose();
            }

            _updateZOrderTimer = new()
            {
                Interval = 500,
                AutoReset = true,
                Enabled = false
            };

            _updateZOrderTimer.Elapsed += (s, e) =>
            {
                UpdateZOrder();
            };

            _updateZOrderTimer.Start();






            IsVisible = true;
        }

        /// <summary>
        /// 隐藏所有阴影元素并停止 Z 序刷新定时器。
        /// </summary>
        private void HideShadowElements()
        {
            if (!_isShadowInitialized || !_isShadowEnabled) return;

            _updateZOrderTimer?.Stop();
            _updateZOrderTimer?.Close();

            ShowWindow(_topElementWindow!.WND, ShowWindowCommand.SW_HIDE);
            ShowWindow(_rightElementWindow!.WND, ShowWindowCommand.SW_HIDE);
            ShowWindow(_bottomElmentWindow!.WND, ShowWindowCommand.SW_HIDE);
            ShowWindow(_leftElementWindow!.WND, ShowWindowCommand.SW_HIDE);


            IsVisible = false;
        }

        /// <summary>
        /// 是否处于延迟动画阶段。
        /// </summary>
        private bool _isShadowAnimationDelayed = false;

        /// <summary>
        /// 是否正在执行延迟显示动画。
        /// </summary>
        private bool _isShadowShowing = false;

        /// <summary>
        /// 阴影当前是否已显示。
        /// </summary>
        private bool _isShadowShown = false;

        /// <summary>
        /// 显示或隐藏阴影元素, 支持延迟动画; 延迟期间可被取消任务中断。
        /// </summary>
        /// <param name="show">是否显示阴影。</param>
        /// <param name="delayed">是否使用延迟动画。</param>
        /// <param name="duration">延迟动画时长(毫秒)。</param>
        private void UpdateShadowElements(bool show, bool delayed = false, int duration = 150)
        {
            if (!_isShadowInitialized) return;

            if (show == _isShadowShown) return;

            if (WindowShadowEffect == ShadowEffect.None) return;

            var shadowCancellation = _shadowCancellationSource.Token;


            void SetShadowVisibleImmediately()
            {
                if (show)
                {
                    RefreshShadowElements();

                    ShowShadowElements();

                    UpdateZOrder();

                    RefreshShadowElements();

                    _lastWindowState = FormWindowState.Normal;
                }
                else
                {
                    HideShadowElements();
                }

                _isShadowShown = show;
            }

            if (_isShadowAnimationDelayed)
            {
                if (show == _isShadowShowing)
                {
                    return;
                }
                else
                {
                    _shadowCancellationSource.Cancel();
                }
            }



            if (show && delayed)
            {
                _isShadowAnimationDelayed = true;

                _isShadowShowing = show;

                Task.Run(async () =>
                {
                    if (shadowCancellation.IsCancellationRequested)
                    {
                        if (_isShadowAnimationDelayed)
                        {
                            SetShadowVisibleImmediately();
                        }
                    }
                    else
                    {
                        await Task.Delay(duration);

                        if (_isShadowAnimationDelayed)
                        {
                            SetShadowVisibleImmediately();
                        }
                    }

                    _isShadowAnimationDelayed = false;


                }, shadowCancellation);
            }
            else
            {
                SetShadowVisibleImmediately();
            }


        }

        /// <summary>
        /// 按窗口矩形刷新四个阴影元素的位图, 激活状态变化时重新生成位图。
        /// </summary>
        /// <param name="rect">目标窗口矩形, 为 null 时取当前窗口矩形。</param>
        /// <param name="syncActions">是否并行更新四个元素。</param>
        private void RefreshShadowElements(RECT? rect = null, bool syncActions = false)
        {

            if (!_isShadowInitialized) return;

            if (!_isShadowEnabled)
                return;

            if (IsIconic(WND) || IsZoomed(WND)) return;




            if (rect == null)
            {
                GetWindowRect(Handle, out var winRect);

                rect = winRect;
            }

            var windowRect = rect!.Value;

            var state = new ShadowElementState
            {
                Width = windowRect.Width,
                Height = windowRect.Height,
                IsActive = TargetWindow.IsWindowActivated
            };

            var shouldUpdateBitmap = false;

            if (_lastShadowState == null || _lastShadowState.IsActive != state.IsActive)
            {
                shouldUpdateBitmap = true;
            }



            if (syncActions)
            {
                Parallel.Invoke(
                    () => _rightElementWindow!.UpdateBitmap(windowRect, state.IsActive, shouldUpdateBitmap),
                    () => _topElementWindow!.UpdateBitmap(windowRect, state.IsActive, shouldUpdateBitmap),
                    () => _bottomElmentWindow!.UpdateBitmap(windowRect, state.IsActive, shouldUpdateBitmap),
                    () => _leftElementWindow!.UpdateBitmap(windowRect, state.IsActive, shouldUpdateBitmap));
            }
            else
            {
                _rightElementWindow!.UpdateBitmap(windowRect, state.IsActive, shouldUpdateBitmap);
                _topElementWindow!.UpdateBitmap(windowRect, state.IsActive, shouldUpdateBitmap);
                _bottomElmentWindow!.UpdateBitmap(windowRect, state.IsActive, shouldUpdateBitmap);
                _leftElementWindow!.UpdateBitmap(windowRect, state.IsActive, shouldUpdateBitmap);
            }

            _lastShadowState = state;
        }

        #endregion

        /// <summary>
        /// 是否正处于尺寸拖拽过程中。
        /// </summary>
        private bool _isSizing;

        /// <summary>
        /// 重写消息处理: 拦截显示/位置/激活/尺寸消息以刷新阴影元素。
        /// </summary>
        /// <param name="m">待处理的窗口消息。</param>
        protected override void WndProc(ref Message m)
        {

            if (!_isShadowEnabled)
            {
                base.WndProc(ref m);

                return;
            }

            var msg = (WindowMessage)m.Msg;

            if (msg == WindowMessage.WM_SHOWWINDOW)
            {
                var isShown = m.WParam != (nint)0;

                if (IsShadowShownWithWindow)
                {
                    UpdateShadowElements(isShown);
                }
            }

            if (msg == WindowMessage.WM_WINDOWPOSCHANGED && m.HWnd != (nint)0)
            {
                var windowpos = m.LParam.ToStructure<WINDOWPOS>();

                var rect = new RECT(windowpos.x, windowpos.y, windowpos.x + windowpos.cx, windowpos.y + windowpos.cy);


                if ((windowpos.flags & SetWindowPosFlags.SWP_NOMOVE) != SetWindowPosFlags.SWP_NOMOVE)
                {
                    RefreshShadowElements(rect);
                }
                else if ((windowpos.flags & SetWindowPosFlags.SWP_NOSIZE) != SetWindowPosFlags.SWP_NOSIZE)
                {
                    RefreshShadowElements(rect, true);
                    _isSizing = true;
                }
            }

            if (msg == WindowMessage.WM_ACTIVATEAPP)
            {
                RefreshShadowElements(syncActions: true);
            }

            if (msg == WindowMessage.WM_ACTIVATE)
            {
                RefreshShadowElements(syncActions: true);
                UpdateZOrder();
            }

            if (msg == WindowMessage.WM_ENTERSIZEMOVE)
            {

            }

            if (msg == WindowMessage.WM_EXITSIZEMOVE)
            {
                if (_isSizing)
                {
                    System.GC.Collect();

                    _isSizing = false;
                }
            }



            base.WndProc(ref m);
        }


        #region IDispose
        /// <summary>
        /// 是否已释放。
        /// </summary>
        bool _isDisposed = false;

        /// <summary>
        /// 释放装饰器占用的全部资源。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放资源: 销毁阴影元素并取消订阅目标窗口事件。
        /// </summary>
        /// <param name="disposing">是否为显式释放调用。</param>
        private void Dispose(bool disposing)
        {
            if (_isDisposed) return;


            if (disposing)
            {
                // release unmanaged resources
                DestroyShadowElements();
            }


            UnregisterTargetWindowEvents();

            _isDisposed = true;
        }
        #endregion




    }
}
