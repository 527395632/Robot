using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using Vanara.Extensions;
using Vanara.PInvoke;
using static Vanara.PInvoke.User32;
using static Vanara.PInvoke.Gdi32;

namespace Robot.Forms.BorderlessForm
{

    /// <summary>
    /// WM_NCCALCSIZE 消息参数结构 (来自 User32)。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct NCCALCSIZE_PARAMS
    {
        /// <summary>
        /// 窗口区域矩形数组。
        /// </summary>
        public RECT rgrc0, rgrc1, rgrc2;

        /// <summary>
        /// 窗口位置结构。
        /// </summary>
        public WINDOWPOS lppos;
    }


    /// <summary>
    /// 无边框窗口: 基于 GDI 渲染, 通过拦截 WM_NCCALCSIZE / WM_NCPAINT / WM_NCHITTEST 等消息实现
    /// 自定义边框、阴影与命中测试, 并对 WinForms 客户端尺寸计算做修正; 提供 DPI 适配、窗口居中、
    /// 激活/状态变化事件与非客户区重绘等通用能力, 并支持自定义阴影与消息拦截。
    /// </summary>
    internal class BorderlessWindow : Form
    {
        #region Fields

        /// <summary>
        /// 窗口句柄。
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal protected HWND WND { get; protected set; }

        /// <summary>
        /// 窗口 DPI 适配器。
        /// </summary>
        internal protected WindowDpiAdapter WindowDpiAdapter { get; }

        /// <summary>
        /// 无边框颜色 (用于透明键, 使该颜色区域透明)。
        /// </summary>
        protected Color NO_BORDER_COLOR { get; set; } = Color.FromArgb(0xFF, 0x01, 0x01, 0x01);

        /// <summary>
        /// 透明颜色标记。
        /// </summary>
        protected readonly Color TRANSPARENT_COLOR = Color.Empty;// Color.FromArgb(0x00, 0x00, 0x00, 0x01);

        /// <summary>
        /// 是否显示窗口边框。
        /// </summary>
        bool _showBorder = false;

        /// <summary>
        /// 窗口激活时的边框颜色。
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal protected Color BorderColor { get; set; } = Color.FromArgb(0xB2, 0xB2, blue: 0xB2);

        /// <summary>
        /// 未显式指定时的非激活边框颜色; 为 null 时由 <see cref="BorderColor"/> 自动派生。
        /// </summary>
        private Color? _inactiveBorderColor = null;

        /// <summary>
        /// 表示 TRUE 的指针常量 (1)。
        /// </summary>
        private readonly IntPtr TRUE = new IntPtr(1);

        /// <summary>
        /// 表示 FALSE 的指针常量 (0)。
        /// </summary>
        private readonly IntPtr FALSE = new IntPtr(0);

        /// <summary>
        /// 反射: Control._clientWidth 私有字段。
        /// </summary>
        private FieldInfo? _clientWidthField;

        /// <summary>
        /// 反射: Control._clientHeight 私有字段。
        /// </summary>
        private FieldInfo? _clientHeightField;

        /// <summary>
        /// 反射: Form.FormStateSetClientSize 私有静态字段 (BitVector32.Section)。
        /// </summary>
        private FieldInfo? _formStateSetClientSizeField;

        /// <summary>
        /// 反射: Form.formState 私有字段 (BitVector32)。
        /// </summary>
        private FieldInfo? _formStateField;

        /// <summary>
        /// 标记是否刚进入最大化状态, 用于在下次 SetBoundsCore 时保持窗口位置。
        /// </summary>
        private bool _shouldPerformMaximiazedState = false;

        /// <summary>
        /// 上一次窗口状态 (用于检测状态变化)。
        /// </summary>
        private WindowChangeState _lastWinState = WindowChangeState.Restore;

        /// <summary>
        /// 窗口是否处于激活状态。
        /// </summary>
        private bool _isWindowActivated = false;

        /// <summary>
        /// 需要触发重绘的非客户区鼠标消息列表。
        /// </summary>
        private readonly WindowMessage[] NC_MESSAGES = new[]
        {
            WindowMessage.WM_NCMOUSEMOVE,
            WindowMessage.WM_NCMOUSELEAVE,
            WindowMessage.WM_NCLBUTTONDOWN,
            WindowMessage.WM_NCRBUTTONDOWN,
            WindowMessage.WM_NCLBUTTONDBLCLK,
            WindowMessage.WM_NCRBUTTONDBLCLK,
        };

        /// <summary>
        /// 幽灵窗口 (ghosting) 相关消息 ID 列表。
        /// </summary>
        private readonly int[] GHOSTING_MESSAGES = new[]
        {
            0x00AE,
            0x00AF,
            0xC1BC
        };

        /// <summary>
        /// 是否启用命中测试 (鼠标可点击区域判定)。
        /// </summary>
        private bool _enableHitTest = true;

        #endregion


        #region Properties

        /// <summary>
        /// 窗口是否透明。
        /// </summary>
        protected bool IsWindowTransparent => false;

        /// <summary>
        /// 窗口矩形 (含非客户区)。
        /// </summary>
        internal protected Rectangle WindowRectangle
        {
            get
            {
                GetWindowRect(WND, out var windowRect);
                return System.Drawing.Rectangle.FromLTRB(windowRect.left, windowRect.top, windowRect.right, windowRect.bottom);
            }
        }

        /// <summary>
        /// 窗口缩放因子 (基于窗口所在屏幕 DPI)。
        /// </summary>
        public float WindowScaleFactor => SystemDpiManager.GetScaleFactorForWindow(WND);

        /// <summary>
        /// 窗口是否处于激活状态; 变化时触发 <see cref="OnWindowActivated"/>。
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal protected bool IsWindowActivated
        {
            get => _isWindowActivated;
            internal set
            {
                if (_isWindowActivated != value)
                {
                    _isWindowActivated = value;

                    OnWindowActivatedInternal();
                }
            }
        }

        /// <summary>
        /// 窗口最大化/最小化状态 (读取窗口样式得到)。
        /// </summary>
        internal protected FormWindowState MinMaxState
        {
            get
            {
                var retval = (WindowStyles)GetWindowLong(WND, WindowLongFlags.GWL_STYLE);

                var max = (retval & WindowStyles.WS_MAXIMIZE) > 0;
                if (max)
                    return FormWindowState.Maximized;
                var min = (retval & WindowStyles.WS_MINIMIZE) > 0;
                if (min)
                    return FormWindowState.Minimized;

                return FormWindowState.Normal;
            }
        }

        /// <summary>
        /// 窗口非激活时的边框颜色; 未显式设置时按 <see cref="BorderColor"/> 校正 -0.1 得到。
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal protected Color InactiveBorderColor
        {
            get => _inactiveBorderColor ?? ChangeColor(BorderColor, -0.1f);
            set
            {
                if (value != _inactiveBorderColor && value.A > 0)
                {
                    _inactiveBorderColor = value;

                    if (_inactiveBorderColor == TRANSPARENT_COLOR)
                    {
                        _inactiveBorderColor = null;
                    }
                }
            }
        }

        /// <summary>
        /// 是否启用命中测试 (由窗口决定是否允许鼠标命中边框/标题区域)。
        /// </summary>
        protected bool EnableHitTest => _enableHitTest;

        /// <summary>
        /// 是否显示窗口边框; 关闭时设置 <see cref="TransparencyKey"/> 为无边框颜色。
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        internal protected bool ShowBorder
        {
            get => _showBorder;
            set
            {
                if (value != _showBorder)
                {
                    _showBorder = value;

                    if (!_showBorder)
                    {
                        TransparencyKey = NO_BORDER_COLOR;
                    }
                }
            }
        }

        /// <summary>
        /// 自定义窗口消息 (WM_*) 处理委托; 返回 true 表示已处理, 不再调用基类。
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public WindowMessageDelegate? OnWndProc { get; set; }

        /// <summary>
        /// 默认窗口消息 (WM_NCHITTEST 等) 处理委托; 返回 true 表示已处理, 不再调用基类。
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public WindowMessageDelegate? OnDefWndProc { get; set; }

        /// <summary>
        /// 是否启用命中测试; 关闭时窗口边框样式切换为 <see cref="FormBorderStyle.FixedDialog"/>。
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsHitTestEnabled
        {
            get => _enableHitTest;
            set
            {
                _enableHitTest = value;

                if (!value)
                {
                    FormBorderStyle = FormBorderStyle.FixedDialog;
                }
            }
        }

        /// <summary>
        /// 窗口阴影装饰器。
        /// </summary>
        internal WindowShadowDecorator ShadowDecorator { get; }

        /// <summary>
        /// 窗口激活时的阴影颜色, 转发至阴影装饰器。
        /// </summary>
        protected Color ShadowColor { get => ShadowDecorator.ShadowActiveColor; set => ShadowDecorator.ShadowActiveColor = value; }

        /// <summary>
        /// 窗口非激活时的阴影颜色, 转发至阴影装饰器。
        /// </summary>
        protected Color InactiveShadowColor { get => ShadowDecorator.ShadowInactiveColor; set => ShadowDecorator.ShadowInactiveColor = value; }

        /// <summary>
        /// 窗口阴影效果, 转发至阴影装饰器。
        /// </summary>
        protected ShadowEffect WindowShadowEffect { get => ShadowDecorator.WindowShadowEffect; set => ShadowDecorator.WindowShadowEffect = value; }

        #endregion


        #region Events

        /// <summary>
        /// 窗口激活状态变化事件。
        /// </summary>
        public event EventHandler<WindowActivatedEventArgs>? WindowActivated;

        /// <summary>
        /// 窗口状态变化事件。
        /// </summary>
        public event EventHandler<WindowStateChangedEventArgs>? WindowStateChanged;

        #endregion


        #region Constructor

        /// <summary>
        /// 初始化无边框窗口: 设置 DPI 缩放模式、DPI 适配器与默认背景色,
        /// 初始化反射字段, 创建阴影装饰器并设置默认阴影颜色与效果。
        /// </summary>
        public BorderlessWindow()
        {
            AutoScaleMode = AutoScaleMode.Dpi;

            WindowDpiAdapter = new WindowDpiAdapter(this);

            BackColor = Color.White;

            InitializeReflectedFields();

            ShadowDecorator = new WindowShadowDecorator(this);

            // 默认无边框窗口阴影
            ShadowColor = ColorTranslator.FromHtml("#99303030");
            InactiveShadowColor = Color.Transparent;
            WindowShadowEffect = ShadowEffect.Normal;
        }

        #endregion


        #region Window lifecycle

        /// <summary>
        /// 窗口加载后: 居中窗口。
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            CenterWindow();
        }

        /// <summary>
        /// 创建窗口句柄 (不显示窗口), 用于提前初始化窗口相关资源。
        /// </summary>
        public void ShowInvisible()
        {
            if (!IsHandleCreated)
            {
                CreateHandle();
            }
        }

        /// <summary>
        /// 句柄创建后: 记录窗口句柄, 非无边框窗口时刷新边框, 交由基类,
        /// 再禁用主题/处理透明键, 最后禁用进程窗口幽灵 (ghosting)。
        /// </summary>
        protected override void OnHandleCreated(EventArgs e)
        {
            WND = new HWND(Handle);

            if (FormBorderStyle != FormBorderStyle.None)
            {
                SetWindowPos(WND, HWND.NULL, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_NOOWNERZORDER | SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_FRAMECHANGED);
            }

            base.OnHandleCreated(e);

            UxTheme.SetWindowTheme(WND, string.Empty, string.Empty);

            if (!ShowBorder)
            {
                TransparencyKey = NO_BORDER_COLOR;
            }

            DisableProcessWindowsGhosting();
        }

        /// <summary>
        /// 窗口关闭前: 取消关闭时清理透明键与无边框颜色。
        /// </summary>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (!e.Cancel)
            {
                NO_BORDER_COLOR = BorderColor;
                TransparencyKey = Color.Empty;
            }
        }

        #endregion


        #region Nonclient & centering

        /// <summary>
        /// 失效非客户区 (句柄已创建且窗口非透明时)。
        /// </summary>
        protected void InvalidateNonclient()
        {
            if (Handle == IntPtr.Zero) return;

            if (IsWindowTransparent) return;

            InvalidateNonclient(WND);
        }

        /// <summary>
        /// 居中窗口: 按 StartPosition (居中父窗口/居中屏幕) 计算位置, 支持 Per-Monitor-V2 缩放。
        /// </summary>
        protected void CenterWindow()
        {
            if (StartPosition == FormStartPosition.CenterParent && Owner != null)
            {
                Location = new Point(Owner.Location.X + Owner.Width / 2 - Width / 2,
                Owner.Location.Y + Owner.Height / 2 - Height / 2);
            }
            else if (StartPosition == FormStartPosition.CenterScreen || (StartPosition == FormStartPosition.CenterParent && Owner == null))
            {
                var currentScreen = Screen.FromPoint(MousePosition);

                var w = WindowRectangle.Width;
                var h = WindowRectangle.Height;

                var screenLeft = 0;
                var screenTop = 0;

                if (SystemDpiManager.IsPerMonitorV2Awareness)
                {
                    var screenDpi = SystemDpiManager.GetScreenDpiFromPoint(MousePosition);

                    var screenScaleFactor = screenDpi / 96f / WindowDpiAdapter.ScaleFactor;

                    var bounds = GetScaledBounds(WindowRectangle, new SizeF(screenScaleFactor, screenScaleFactor), BoundsSpecified.Size);

                    w = bounds.Width;
                    h = bounds.Height;
                }

                screenLeft += currentScreen.WorkingArea.X;
                screenTop += currentScreen.WorkingArea.Y;

                var location = default(Point);

                location.X = screenLeft + (currentScreen.WorkingArea.Width - w) / 2;
                location.Y = screenTop + (currentScreen.WorkingArea.Height - h) / 2;

                Location = location;
            }
        }

        /// <summary>
        /// 按命中区域设置窗口光标 (上下/左右/对角调整尺寸光标)。
        /// </summary>
        internal SafeHCURSOR? SetWindowCursor(HitTestValues mode)
        {
            SafeHCURSOR? handle = null;

            switch (mode)
            {
                case HitTestValues.HTTOP:
                case HitTestValues.HTBOTTOM:
                    handle = LoadCursor(lpCursorName: Macros.MAKEINTRESOURCE(32645));
                    break;
                case HitTestValues.HTLEFT:
                case HitTestValues.HTRIGHT:
                    handle = LoadCursor(lpCursorName: Macros.MAKEINTRESOURCE(32644));
                    break;
                case HitTestValues.HTTOPLEFT:
                case HitTestValues.HTBOTTOMRIGHT:
                    handle = LoadCursor(lpCursorName: Macros.MAKEINTRESOURCE(32642));

                    break;
                case HitTestValues.HTTOPRIGHT:
                case HitTestValues.HTBOTTOMLEFT:
                    handle = LoadCursor(lpCursorName: Macros.MAKEINTRESOURCE(32643));
                    break;
            }


            if (handle != null)
            {
                var oldCursor = SetCursor(handle);

                oldCursor.Close();
            }

            return handle;
        }

        /// <summary>
        /// 重绘非客户区: 重绘窗口边框并刷新窗口 (句柄已创建且未释放时)。
        /// </summary>
        private void InvalidateNonclient(HWND hWnd)
        {
            if (!IsHandleCreated || IsDisposed)
                return;

            RedrawWindow(hWnd, null, HRGN.NULL, RedrawWindowFlags.RDW_FRAME | RedrawWindowFlags.RDW_UPDATENOW | RedrawWindowFlags.RDW_VALIDATE);

            UpdateWindow(hWnd);

            SetWindowPos(hWnd, HWND.NULL, 0, 0, 0, 0, SetWindowPosFlags.SWP_FRAMECHANGED | SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_NOCOPYBITS | SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOOWNERZORDER | SetWindowPosFlags.SWP_NOREPOSITION | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOZORDER);
        }

        #endregion


        #region WndProc

        /// <summary>
        /// 拦截窗口消息: 先交由 <see cref="OnWndProc"/> 处理, 未处理时分发无边框窗口各消息
        /// (非客户区计算/绘制、命中测试、尺寸、幽灵窗口等), 再交由基类。
        /// </summary>
        protected override void WndProc(ref Message m)
        {
            var retval = OnWndProc?.Invoke(ref m) ?? false;

            if (retval) return;

            var msg = (WindowMessage)m.Msg;

            switch (msg)
            {
                case WindowMessage.WM_ACTIVATE:
                    {
                        InvalidateNonclient();
                    }
                    break;
                case WindowMessage.WM_NCCALCSIZE when m.WParam != IntPtr.Zero:
                    {
                        if (WmNCCalcSize(ref m)) return;
                    }
                    break;
                case WindowMessage.WM_NCACTIVATE:
                    {
                        if (WmNCActivate(ref m)) return;
                    }
                    break;
                case WindowMessage.WM_NCPAINT:
                    {
                        if (WmNCPaint(ref m)) return;
                    }
                    break;
                case WindowMessage.WM_PAINT:
                    {
                        WmPaint(ref m);
                    }
                    break;
                case WindowMessage.WM_NCHITTEST:
                    {
                        m.Result = TRUE;
                    }
                    return;
                case WindowMessage.WM_SIZE:
                    {
                        WmSize(ref m);
                        WmSizeState(ref m);
                    }
                    break;
                case WindowMessage.WM_SETCURSOR when EnableHitTest == true:
                    {
                        if (WmSetCursorWithHitTestEnabled(ref m)) return;
                    }
                    break;
            }

            if (WmGhostingHandler(ref m)) return;

            base.WndProc(ref m);
        }

        /// <summary>
        /// 处理默认窗口消息: 先交由 <see cref="OnDefWndProc"/> 处理, 未处理时调用基类。
        /// </summary>
        protected override void DefWndProc(ref Message m)
        {
            var retval = OnDefWndProc?.Invoke(ref m) ?? false;

            if (!retval)
            {
                base.DefWndProc(ref m);
            }
        }

        #endregion


        #region WindowMessage Handlers

        /// <summary>
        /// 处理 WM_SIZE: 记录最大化状态, 还原时失效非客户区/客户区并清理区域。
        /// </summary>
        private void WmSize(ref Message m)
        {
            const int SIZE_RESTORED = 0;
            const int SIZE_MAXIMIZED = 2;


            if (m.WParam == (nint)SIZE_MAXIMIZED)
            {
                _shouldPerformMaximiazedState = true;
            }

            if (m.WParam == (nint)SIZE_RESTORED)
            {
                InvalidateNonclient();
                Invalidate();

                if (!IsZoomed(m.HWnd))
                {
                    Region?.Dispose();
                    Region = null;
                }
            }

        }

        /// <summary>
        /// 处理 WM_SIZE: 检测最小化/最大化/还原状态变化并触发内部事件。
        /// </summary>
        private void WmSizeState(ref Message m)
        {
            const int SIZE_RESTORED = 0;
            const int SIZE_MINIMIZED = 1;
            const int SIZE_MAXIMIZED = 2;

            if (m.WParam == (nint)SIZE_MINIMIZED)
            {
                if (_lastWinState != WindowChangeState.Minimize)
                {
                    var state = _lastWinState = WindowChangeState.Minimize;

                    OnWindowStateChangedInternal(state);
                }
            }

            if (m.WParam == (nint)SIZE_MAXIMIZED)
            {
                if (_lastWinState != WindowChangeState.Maximize)
                {
                    var state = _lastWinState = WindowChangeState.Maximize;

                    OnWindowStateChangedInternal(state);

                }
            }

            if (m.WParam == (nint)SIZE_RESTORED)
            {
                if (_lastWinState != WindowChangeState.Restore)
                {
                    var state = _lastWinState = WindowChangeState.Restore;

                    OnWindowStateChangedInternal(state);
                }
            }

        }

        /// <summary>
        /// 处理 WM_NCCALCSIZE: 调整非客户区大小, 无边框窗口将非客户区压缩到 0。
        /// 返回 true 表示消息已完全处理 (不再调用基类)。
        /// </summary>
        private bool WmNCCalcSize(ref Message m)
        {
            if (FormBorderStyle == FormBorderStyle.None) return false;

            var nccsp = Marshal.PtrToStructure<NCCALCSIZE_PARAMS>(m.LParam);
            var borders = GetNonClientMetrics();

            if (IsZoomed(WND))
            {
                nccsp.rgrc0.top -= borders.Top;
                nccsp.rgrc0.top += borders.Bottom;

                Marshal.StructureToPtr(nccsp, m.LParam, false);
            }
            else
            {
                nccsp.rgrc0.top -= borders.Top;
                nccsp.rgrc0.bottom += borders.Bottom;
                nccsp.rgrc0.left -= borders.Left;
                nccsp.rgrc0.right += borders.Right;

                nccsp.rgrc0.top += NonclientFrameSize.Top;
                nccsp.rgrc0.bottom -= NonclientFrameSize.Bottom;
                nccsp.rgrc0.left += NonclientFrameSize.Left;
                nccsp.rgrc0.right -= NonclientFrameSize.Right;

                Marshal.StructureToPtr(nccsp, m.LParam, false);
            }

            m.Result = new IntPtr(0x0400);

            return false;
        }

        /// <summary>
        /// 启用命中测试时处理 WM_SETCURSOR: 根据命中区域设置光标 (如边框为调整尺寸光标)。
        /// 返回 true 表示消息已处理。
        /// </summary>
        private bool WmSetCursorWithHitTestEnabled(ref Message m)
        {
            var pos = GetMessagePos();
            var point = new POINT(Macros.LOWORD(pos), Macros.HIWORD(pos));
            ScreenToClient(WND, ref point);

            var mode = HitTest(point);

            if (mode == HitTestValues.HTNOWHERE)
            {
                return false;
            }

            if (mode != HitTestValues.HTCLIENT && WindowState == FormWindowState.Normal)
            {
                SetWindowCursor(mode);

                m.Result = TRUE;

                return true;
            }

            return false;
        }

        /// <summary>
        /// 处理 WM_PAINT (非 DWM): 最大化时根据屏幕范围设置窗口区域 (Region)。
        /// </summary>
        private void WmPaint(ref Message m)
        {
            if (Bounds.X == -32000 && Bounds.Y == -32000)
            {
                return;
            }


            if (IsZoomed(m.HWnd))
            {
                var screen = Screen.FromHandle(Handle);

                var bounds = FormBorderStyle == FormBorderStyle.None ? screen.Bounds : screen.WorkingArea;

                var regionBounds = new Rectangle(bounds.X - Bounds.X, bounds.Y - Bounds.Y, Bounds.Width - (Bounds.Width - bounds.Width), Bounds.Height - (Bounds.Height - bounds.Height));

                Region?.Dispose();
                Region = null;

                if (FormBorderStyle != FormBorderStyle.None)
                {
                    Region = new Region(regionBounds);
                }
                else
                {

                }


            }
            else
            {
                Region?.Dispose();
                Region = null;
            }

        }

        /// <summary>
        /// 处理 WM_NCPAINT (GDI 渲染): 用边框颜色 (激活/非激活) 填充非客户区边框。
        /// 返回 true 表示消息已处理。
        /// </summary>
        private bool WmNCPaint(ref Message m)
        {
            if (m.HWnd == IntPtr.Zero)
            {
                return false;
            };

            if (IsWindowTransparent) return true;

            GetWindowRect(m.HWnd, out var bounds);

            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return false;
            }

            var dcxFlags = DCX.DCX_WINDOW | DCX.DCX_CACHE | DCX.DCX_CLIPSIBLINGS | DCX.DCX_VALIDATE;

            var hRegion = IntPtr.Zero;

            if (m.WParam != TRUE)
            {
                dcxFlags |= DCX.DCX_INTERSECTRGN;
                hRegion = m.WParam;
            }

            var dc = GetDCEx(WND, hRegion, dcxFlags);

            try
            {
                if (dc != IntPtr.Zero)
                {
                    GetClientRect(m.HWnd, out var clientRect);

                    OffsetRect(ref clientRect, NonclientFrameSize.Left, NonclientFrameSize.Top);

                    OffsetRect(ref bounds, -bounds.left, -bounds.top);

                    if (IsZoomed(m.HWnd))
                    {
                        ExcludeClipRect(dc, bounds.left, bounds.top, bounds.right, bounds.bottom);
                    }
                    else
                    {
                        ExcludeClipRect(dc, clientRect.left, clientRect.top, clientRect.right, clientRect.bottom);
                    }

                    var borderColor = IsWindowActivated ?
                        BorderColor :
                        InactiveBorderColor;

                    using var brush = ShowBorder ? CreateSolidBrush(new COLORREF(borderColor)) : CreateSolidBrush(new COLORREF(NO_BORDER_COLOR));

                    FillRect(dc, bounds, brush);
                    DeleteObject(brush);
                    brush.Close();

                    SelectClipRgn(dc, HRGN.NULL);
                }
            }
            finally
            {
                ReleaseDC(m.HWnd, dc);
            }

            m.Result = TRUE;// IntPtr.Zero;

            return true;

        }

        /// <summary>
        /// 处理 WM_NCACTIVATE: 更新窗口激活状态, 非激活时重绘非客户区。
        /// </summary>
        private bool WmNCActivate(ref Message m)
        {
            if (m.HWnd == IntPtr.Zero) return false;

            IsWindowActivated = m.WParam != IntPtr.Zero;

            if (IsIconic(m.HWnd)) return false;

            m.Result = DefWindowProc(m.HWnd, (uint)m.Msg, m.WParam, new IntPtr(-1));


            if (m.WParam == IntPtr.Zero)
            {
                m.Result = TRUE;
                InvalidateNonclient();
                Invalidate();
            }

            return true;
        }

        /// <summary>
        /// 处理幽灵窗口与非客户区鼠标消息: 触发非客户区/客户区重绘。
        /// </summary>
        private bool WmGhostingHandler(ref Message m)
        {
            if (GHOSTING_MESSAGES.Contains(m.Msg))
            {
                m.Result = FALSE;
                InvalidateNonclient();
            }

            if (NC_MESSAGES.Contains((WindowMessage)m.Msg))
            {
                if (m.HWnd == IntPtr.Zero) return false;
                InvalidateNonclient();
                Invalidate();
            }

            return false;
        }

        #endregion


        #region Window state & activation events

        /// <summary>
        /// 窗口状态变化 (还原/最大化) 时失效客户区与非客户区以重绘, 并触发状态变化事件。
        /// </summary>
        protected void OnWindowStateChanged(WindowChangeState state)
        {
            switch (state)
            {
                case WindowChangeState.Restore:
                    Invalidate();
                    InvalidateNonclient();
                    break;
                case WindowChangeState.Maximize:
                    Invalidate();
                    InvalidateNonclient();
                    break;
                case WindowChangeState.Minimize:
                    break;
                default:
                    break;
            }

            WindowStateChanged?.Invoke(this, new WindowStateChangedEventArgs(state));
        }

        /// <summary>
        /// 触发窗口激活事件 (内部)。
        /// </summary>
        private void OnWindowActivatedInternal()
        {
            OnWindowActivated(_isWindowActivated);
        }

        /// <summary>
        /// 窗口激活状态变化。
        /// </summary>
        protected void OnWindowActivated(bool isActivated)
        {
            WindowActivated?.Invoke(this, new WindowActivatedEventArgs(_isWindowActivated));

        }

        /// <summary>
        /// 触发窗口状态变化事件 (内部)。
        /// </summary>
        private void OnWindowStateChangedInternal(WindowChangeState state)
        {
            OnWindowStateChanged(state);

        }

        #endregion


        #region Color

        /// <summary>
        /// 按校正系数调整颜色: 系数为负时向黑色收缩, 为正时向白色 (255) 靠近, 并对各通道做 0–255 钳制。
        /// </summary>
        /// <param name="color">待调整的颜色。</param>
        /// <param name="correctionFactor">校正系数。</param>
        /// <returns>调整后的颜色。</returns>
        Color ChangeColor(Color color, float correctionFactor)
        {
            float red = (float)color.R;
            float green = (float)color.G;
            float blue = (float)color.B;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }

            if (red < 0) red = 0;

            if (red > 255) red = 255;

            if (green < 0) green = 0;

            if (green > 255) green = 255;

            if (blue < 0) blue = 0;

            if (blue > 255) blue = 255;

            return Color.FromArgb(color.A, (int)red, (int)green, (int)blue);
        }

        #endregion


        #region WinForm Frameless Crack

        /// <summary>
        /// 非客户区边框尺寸 (上下左右)。
        /// </summary>
        protected Padding NonclientFrameSize => new Padding(1);

        /// <summary>
        /// 计算当前非客户区 (边框) 各边宽度, 通过 AdjustWindowRect 比较客户区与窗口矩形得到。
        /// </summary>
        protected Padding GetNonClientMetrics()
        {
            var rect = RECT.Empty;

            var screenRect = ClientRectangle;

            screenRect.Offset(-Bounds.Left, -Bounds.Top);

            rect.top = screenRect.Top;
            rect.left = screenRect.Left;
            rect.bottom = screenRect.Bottom;
            rect.right = screenRect.Right;

            AdjustWindowRect(ref rect, (WindowStyles)CreateParams.Style, (WindowStylesEx)CreateParams.ExStyle);

            return new Padding
            {
                Top = screenRect.Top - rect.top,
                Left = screenRect.Left - rect.left,
                Bottom = rect.bottom - screenRect.Bottom,
                Right = rect.right - screenRect.Right
            };
        }


        /// <summary>
        /// 通过反射初始化 WinForms 私有字段 (客户端尺寸/表单状态), 供后续修正尺寸使用。
        /// </summary>
        private void InitializeReflectedFields()
        {
            _clientWidthField = typeof(Control).GetField("_clientWidth", BindingFlags.NonPublic | BindingFlags.Instance) ?? typeof(Control).GetField("clientWidth", BindingFlags.NonPublic | BindingFlags.Instance);
            _clientHeightField = typeof(Control).GetField("_clientHeight", BindingFlags.NonPublic | BindingFlags.Instance) ?? typeof(Control).GetField("clientHeight", BindingFlags.NonPublic | BindingFlags.Instance);

            _formStateSetClientSizeField = typeof(Form).GetField("FormStateSetClientSize", BindingFlags.NonPublic | BindingFlags.Static);
            _formStateField = typeof(Form).GetField("formState", BindingFlags.NonPublic | BindingFlags.Instance) ?? typeof(Form).GetField("_formState", BindingFlags.NonPublic | BindingFlags.Instance);

        }

        /// <summary>
        /// 尺寸变化时修正客户端尺寸, 再交由基类处理。
        /// </summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            PatchClientSize();

            base.OnSizeChanged(e);
        }

        /// <summary>
        /// 设置客户端尺寸: 通过反射直接写入 WinForms 私有客户端尺寸字段并触发尺寸变化,
        /// 反射字段不可用时回退到基类实现。
        /// </summary>
        protected override void SetClientSizeCore(int x, int y)
        {
            if (_clientWidthField != null && _clientHeightField != null && _formStateField != null && _formStateSetClientSizeField != null)
            {
                _clientWidthField.SetValue(this, x);
                _clientHeightField.SetValue(this, y);

                var section = (BitVector32.Section)_formStateSetClientSizeField!.GetValue(this)!;

                var vector = (BitVector32)_formStateField!.GetValue(this)!;

                vector[section] = 1;

                _formStateField.SetValue(this, vector);

                OnClientSizeChanged(EventArgs.Empty);

                vector[section] = 0;

                _formStateField.SetValue(this, vector);

                Size = SizeFromClientSize(new Size(x, y));

            }
            else
            {
                base.SetClientSizeCore(x, y);
            }
        }

        /// <summary>
        /// 设置窗口边界: 处理 DPI 锁定、最大化位置保持, 并修正还原窗口边界, 再交由基类。
        /// </summary>
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            if (WindowDpiAdapter?.IsBoundsChangingLocked ?? false)
            {
                return;
            }

            if (_shouldPerformMaximiazedState && WindowState != FormWindowState.Minimized)
            {
                if (y != Top)
                    y = Top;

                if (x != Left)
                    x = Left;

                _shouldPerformMaximiazedState = false;

            }

            var size = PatchWindowSizeInRestoreWindowBoundsIfNecessary(width, height);

            base.SetBoundsCore(x, y, size.Width, size.Height, specified);

        }

        /// <summary>
        /// 命中测试: 根据鼠标位置返回对应的 <see cref="HitTestValues"/> (客户区/八边/八角)。
        /// </summary>
        internal protected HitTestValues HitTest(Point point)
        {
            var htSize = Convert.ToInt32(Math.Ceiling(4 * WindowDpiAdapter.ScaleFactor));
            GetWindowRect(WND, out var lpRect);



            var rect = new Rectangle(Point.Empty, lpRect.Size);

            var hitTestValue = HitTestValues.HTNOWHERE;

            if (rect.Contains(point))
            {
                hitTestValue = HitTestValues.HTCLIENT;

                var x = point.X;
                var y = point.Y;

                if (x < rect.Left + htSize * 2 && y < rect.Top + htSize * 2)
                {
                    hitTestValue = HitTestValues.HTTOPLEFT;
                }
                else if (x >= rect.Left + htSize * 2 && x <= rect.Right - htSize * 2 && y <= rect.Top + htSize)
                {
                    hitTestValue = HitTestValues.HTTOP;
                }
                else if (x > rect.Right - htSize * 2 && y <= rect.Top + htSize * 2)
                {
                    hitTestValue = HitTestValues.HTTOPRIGHT;
                }
                else if (x <= rect.Left + htSize && y >= rect.Top + htSize * 2 && y <= rect.Bottom - htSize * 2)
                {
                    hitTestValue = HitTestValues.HTLEFT;
                }
                else if (x >= rect.Right - htSize && y >= rect.Top * 2 + htSize && y <= rect.Bottom - htSize * 2)
                {
                    hitTestValue = HitTestValues.HTRIGHT;
                }
                else if (x <= rect.Left + htSize * 2 && y >= rect.Bottom - htSize * 2)
                {
                    hitTestValue = HitTestValues.HTBOTTOMLEFT;
                }
                else if (x > rect.Left + htSize * 2 && x < rect.Right - htSize * 2 && y >= rect.Bottom - htSize)
                {
                    hitTestValue = HitTestValues.HTBOTTOM;
                }
                else if (x >= rect.Right - htSize * 2 && y >= rect.Bottom - htSize * 2)
                {
                    hitTestValue = HitTestValues.HTBOTTOMRIGHT;
                }
            }

            return hitTestValue;
        }


        /// <summary>
        /// 按 DPI 缩放因子计算缩放后的边界, 对宽度/高度按固定尺寸约束分别取整。
        /// </summary>
        protected override Rectangle GetScaledBounds(Rectangle bounds, SizeF factor, BoundsSpecified specified)
        {
            var rect = base.GetScaledBounds(bounds, factor, specified);

            if (!GetStyle(ControlStyles.FixedWidth) && (specified & BoundsSpecified.Width) != BoundsSpecified.None)
            {
                var clientWidth = bounds.Width;// - sz.Width;
                rect.Width = (int)Math.Round((double)(clientWidth * factor.Width));// + sz.Width;
            }

            if (!GetStyle(ControlStyles.FixedHeight) && (specified & BoundsSpecified.Height) != BoundsSpecified.None)
            {
                var clientHeight = bounds.Height;// - sz.Height;
                rect.Height = (int)Math.Round((double)(clientHeight * factor.Height));// + sz.Height;
            }

            return rect;
        }

        /// <summary>
        /// 由客户端尺寸反推窗口尺寸: 加上非客户区边框尺寸。
        /// </summary>
        protected override Size SizeFromClientSize(Size clientSize)
        {
            clientSize.Width += NonclientFrameSize.Horizontal;
            clientSize.Height += NonclientFrameSize.Vertical;

            return clientSize;
        }


        /// <summary>
        /// 由窗口尺寸反推客户端尺寸: 减去非客户区边框, 最大化时额外补偿。
        /// </summary>
        private Size ClientSizeFromSize(Size size)
        {
            if (size.IsEmpty)
            {
                return Size.Empty;
            }

            var borders = GetNonClientMetrics();

            var sz = SizeFromClientSize(Size.Empty);

            var res = new Size(size.Width - sz.Width, size.Height - sz.Height);

            if (WindowState != FormWindowState.Maximized)
                return res;

            return new Size(res.Width - borders.Horizontal + sz.Width, res.Height - borders.Bottom * 2 + sz.Height);
        }


        /// <summary>
        /// 修正 WinForms 客户端尺寸字段, 使无边框窗口的客户端尺寸计算与边框一致。
        /// </summary>
        private void PatchClientSize()
        {
            if (FormBorderStyle != FormBorderStyle.None)
            {
                var size = ClientSizeFromSize(Size);

                _clientWidthField!.SetValue(this, size.Width);
                _clientHeightField!.SetValue(this, size.Height);
            }
        }

        /// <summary>
        /// 按窗口样式调整矩形以纳入边框: Per-Monitor-V2 感知时使用带 DPI 的 API, 否则使用常规 API。
        /// </summary>
        private void AdjustWindowRect(ref RECT rect, WindowStyles style, WindowStylesEx exStyle)
        {
            if (SystemDpiManager.IsPerMonitorV2Awareness)
            {
                AdjustWindowRectExForDpi(ref rect, style, false, exStyle, (uint)SystemDpiManager.GetDpiForWindow(WND));
            }
            else
            {
                AdjustWindowRectEx(ref rect, style, false, exStyle);
            }

        }

        /// <summary>
        /// 还原窗口时, 若 WinForms 以客户端尺寸记录了还原边界, 则按渲染类型补偿边框尺寸, 返回修正后的尺寸。
        /// </summary>
        private Size PatchWindowSizeInRestoreWindowBoundsIfNecessary(int width, int height)
        {
            if (WindowState == FormWindowState.Normal)
            {
                var restoredWindowBoundsSpecified = typeof(Form).GetField("restoredWindowBoundsSpecified", BindingFlags.NonPublic | BindingFlags.Instance) ?? typeof(Form).GetField("_restoredWindowBoundsSpecified", BindingFlags.NonPublic | BindingFlags.Instance);
                var restoredSpecified = (BoundsSpecified)restoredWindowBoundsSpecified!.GetValue(this)!;

                if ((restoredSpecified & BoundsSpecified.Size) != BoundsSpecified.None)
                {
                    var formStateExWindowBoundsFieldInfo = typeof(Form).GetField("FormStateExWindowBoundsWidthIsClientSize", BindingFlags.NonPublic | BindingFlags.Static);
                    var formStateExFieldInfo = typeof(Form).GetField("formStateEx", BindingFlags.NonPublic | BindingFlags.Instance) ?? typeof(Form).GetField("_formStateEx", BindingFlags.NonPublic | BindingFlags.Instance);
                    var restoredBoundsFieldInfo = typeof(Form).GetField("restoredWindowBounds", BindingFlags.NonPublic | BindingFlags.Instance) ?? typeof(Form).GetField("_restoredWindowBounds", BindingFlags.NonPublic | BindingFlags.Instance);

                    if (formStateExWindowBoundsFieldInfo != null && formStateExFieldInfo != null && restoredBoundsFieldInfo != null)
                    {
                        var restoredWindowBounds = (Rectangle)restoredBoundsFieldInfo.GetValue(this)!;
                        var section = (BitVector32.Section)formStateExWindowBoundsFieldInfo.GetValue(this)!;
                        var vector = (BitVector32)formStateExFieldInfo.GetValue(this)!;
                        if (vector[section] == 1)
                        {
                            width = restoredWindowBounds.Width + NonclientFrameSize.Horizontal;
                            height = restoredWindowBounds.Height + NonclientFrameSize.Vertical;
                        }
                    }
                }
            }

            return new Size(width, height);
        }

        #endregion

    }
}
