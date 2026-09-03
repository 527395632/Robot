using Robot.App.Forms;
using Robot.Browser;
using Robot.Forms;
using Robot.Forms.BorderlessForm;
using Robot.JavaScript;
using Vanara.PInvoke;
using Vortice.Direct2D1;
using Xilium.CefGlue;
using static Vanara.PInvoke.User32;
using Robot.Properties;
using System.Runtime.InteropServices;
using System.Text;

namespace Robot
{

    public class RobotWindow : IDisposable, IWin32Window
    {
        public RobotWindow()
        {
            WebView = new WebViewLifeSpan(this);

            CreateHostWindowCore();
            Url = "http://embedded.app.local/index.html";

            // 无边框窗体直接配置(原 ConfigureWindowStyle/FormStyle 体系已删)
            InitialSize = new Size(1280, 800);
            StartCentered = true;
            // 窗口四周及四角支持调整大小
            Sizable = true;
            // 最大化 / 还原
            Maximizable = true;
            // 最小化
            Minimizable = true;
            // 全屏
            AllowFullScreen = true;
        }

        #region Base

        /// <summary>
        /// 获取或设置一个值,指示该表单是否可以接受用户拖放到其上的数据。
        /// </summary>
        public bool AllowDrop { get; set; } = false;

        /// <summary>
        /// 获取或设置一个值,指示该表单是否可以全屏。
        /// </summary>
        public bool AllowFullScreen { get; set; } = true;

        /// <summary>
        /// 获取或设置是否启用启动画面。
        /// </summary>
        public bool EnableSplashScreen { get; set; } = true;

        // G 段:原 FormStyle 初始配置直接字段化
        /// <summary>
        /// 初始尺寸。
        /// </summary>
        public Size InitialSize { get; set; } = new Size(960, 640);
        /// <summary>
        /// 初始最小尺寸。
        /// </summary>
        public Size InitialMinimumSize { get; set; } = Size.Empty;
        /// <summary>
        /// 初始最大尺寸。
        /// </summary>
        public Size InitialMaximumSize { get; set; } = Size.Empty;
        /// <summary>
        /// 是否在启动时居中显示。
        /// </summary>
        public bool StartCentered { get; set; } = true;
        /// <summary>
        /// 初始位置。
        /// </summary>
        public Point? InitialLocation { get; set; }
        /// <summary>
        /// 是否在任务栏中显示。
        /// </summary>
        public bool ShowInTaskbar { get; set; } = true;
        /// <summary>
        /// 是否允许系统菜单。
        /// </summary>
        public bool AllowSystemMenu { get; set; } = true;
        /// <summary>
        /// 默认应用标题。
        /// </summary>
        public string DefaultAppTitle { get; set; } = "Robot";
        /// <summary>
        /// 是否使用浏览器命中测试。
        /// </summary>
        public bool UseBrowserHitTest { get; set; } = true;

        /// <summary>
        /// 获取或设置表单的尺寸。
        /// </summary>
        public Size Size
        {
            get => HostWindow?.Size ?? Size.Empty;
            set
            {
                if (HostWindow != null)
                {
                    HostWindow.Size = value;
                }
            }
        }

        /// <summary>
        /// 获取或设置一个值,表示表单在屏幕坐标中的左上角。
        /// </summary>
        public Point Location
        {
            get => HostWindow?.Location ?? new Point(0, 0);
            set
            {
                if (HostWindow != null)
                {
                    HostWindow.Location = value;
                }
            }
        }

        /// <summary>
        /// 获取或设置一个值,指示表单是最小化、最大化、全屏还是正常状态。
        /// </summary>
        public RobotFormWindowState WindowState
        {
            get
            {
                if (HostWindow == null) return RobotFormWindowState.Normal;

                if (IsFullscreen) return RobotFormWindowState.FullScreen;

                return (RobotFormWindowState)HostWindow.WindowState;
            }
            set
            {
                if (value == WindowState) return;

                if (value == RobotFormWindowState.FullScreen)
                {
                    SetFullscreenState(true);
                }
                else
                {
                    SetFullscreenState(false, (FormWindowState)value);
                }

            }
        }




        /// <summary>
        /// 获取或设置一个值,指示表单是否应显示为最顶层。
        /// </summary>
        public bool TopMost
        {
            get => HostWindow?.TopMost ?? false;
            set
            {
                if (HostWindow != null)
                {
                    HostWindow.TopMost = value;
                }
            }
        }

        /// <summary>
        /// 是否可最大化。
        /// </summary>
        private bool _maximizable = true;

        /// <summary>
        /// 获取或设置一个值,指示表单是否可以最大化。
        /// </summary>
        public bool Maximizable
        {
            get => HostWindow?.MaximizeBox ?? _maximizable;
            set
            {
                _maximizable = value;
                if (HostWindow != null)
                {
                    HostWindow.MaximizeBox = value;
                }
            }
        }
        /// <summary>
        /// 是否可最小化。
        /// </summary>
        private bool _minimizable = true;

        /// <summary>
        /// 获取或设置一个值,指示表单是否可以最小化到任务栏。
        /// </summary>
        public bool Minimizable
        {
            get => HostWindow?.MinimizeBox ?? _minimizable;
            set
            {
                _minimizable = value;
                if (HostWindow != null)
                {
                    HostWindow.MinimizeBox = value;
                }
            }
        }

        /// <summary>
        /// 表单图标。
        /// </summary>
        private Icon? _icon;

        /// <summary>
        /// 获取或设置表单的图标。
        /// </summary>
        public Icon? Icon
        {
            get => HostWindow?.Icon ?? _icon;
            set
            {
                _icon = value;
                if (HostWindow != null)
                {
                    HostWindow.Icon = value;
                }
            }
        }

        /// <summary>
        /// 是否可调整大小。
        /// </summary>
        private bool _sizable = true;

        /// <summary>
        /// 获取或设置一个值,指示表单是否可以被用户调整大小。
        /// </summary>
        /// <value>
        /// 若表单可被用户调整大小则为 true,否则为 false。默认为 true。
        /// </value>
        public bool Sizable
        {
            get => _sizable;
            set => _sizable = value;
        }

        /// <summary>
        /// 获取或设置一个值,指示表单是否使用页面标题作为表单标题。
        /// </summary>
        public bool UsePageTitle { get; set; } = false;

        /// <summary>
        /// 获取或设置表单的标题。
        /// </summary>
        public string AppTitle
        {
            get => _appTitle ?? string.Empty;
            set
            {
                _appTitle = value;

                if (HostWindow != null)
                {
                    InvokeOnUIThread(() => HostWindow.Text = BuildTitleString());
                }
            }
        }

        /// <summary>
        /// 获取或设置页面的标题。
        /// </summary>
        internal string PageTitle { get; set; } = string.Empty;

        /// <summary>
        /// 获取或设置表单的标题模式。
        /// </summary>
        public string TitlePattern { get; set; } = "{0} - {1}";

        /// <summary>
        /// 获取或设置表单左边缘与屏幕工作区左边缘之间的距离(像素)。
        /// </summary>
        public int Left
        {
            get => HostWindow?.Left ?? 0;
            set
            {
                if (HostWindow != null)
                {
                    InvokeOnUIThread(() => HostWindow.Left = value);
                }
            }
        }

        /// <summary>
        /// 获取或设置表单上边缘与屏幕工作区上边缘之间的距离(像素)。
        /// </summary>
        public int Top
        {
            get => HostWindow?.Top ?? 0;
            set
            {
                if (HostWindow != null)
                {
                    InvokeOnUIThread(() => HostWindow.Top = value);
                }
            }
        }

        /// <summary>
        /// 获取表单右边缘与屏幕工作区右边缘之间的距离(像素)。
        /// </summary>
        public int Right { get => HostWindow?.Right ?? 0; }

        /// <summary>
        /// 获取表单下边缘与屏幕工作区下边缘之间的距离(像素)。
        /// </summary>
        public int Bottom { get => HostWindow?.Bottom ?? 0; }

        /// <summary>
        /// 获取或设置表单的宽度。
        /// </summary>
        public int Width
        {
            get => HostWindow!.Width;
            set
            {
                if (HostWindow != null)
                {
                    InvokeOnUIThread(() => HostWindow.Width = value);
                }
            }
        }

        /// <summary>
        /// 获取或设置表单的高度。
        /// </summary>
        public int Height
        {
            get => HostWindow!.Height;
            set
            {
                if (HostWindow != null)
                {
                    InvokeOnUIThread(() => HostWindow.Height = value);
                }
            }
        }

        /// <summary>
        /// 获取或设置表单可指定的尺寸上限。
        /// </summary>
        public Size MaximumSize
        {
            get => HostWindow?.MaximumSize ?? Size.Empty;
            set
            {
                if (HostWindow != null)
                {
                    InvokeOnUIThread(() => HostWindow.MaximumSize = value);
                }
            }
        }

        /// <summary>
        /// 获取或设置表单可指定的尺寸下限。
        /// </summary>
        public Size MinimumSize
        {
            get => HostWindow?.MinimumSize ?? Size.Empty;
            set
            {
                if (HostWindow != null)
                {
                    InvokeOnUIThread(() => HostWindow.MinimumSize = value);
                }
            }
        }

        /// <summary>
        /// 获取一个值,指示该表单是否以模态方式显示。
        /// </summary>
        public bool Modal => HostWindow?.Modal ?? false;

        /// <summary>
        /// 获取或设置一个值,指示表单是否已显示。
        /// </summary>
        public bool Visible
        {
            get => HostWindow?.Visible ?? false;
            set
            {
                if (HostWindow != null)
                {
                    InvokeOnUIThread(() => HostWindow.Visible = value);
                }
            }
        }

        /// <summary>
        /// 获取表单在屏幕上的尺寸和位置(像素)。
        /// </summary>
        public Rectangle Bounds => new Rectangle(Left, Top, Width, Height);

        /// <summary>
        /// 获取表单绑定的窗口句柄。
        /// </summary>
        public IntPtr Handle => WindowHandle;

        /// <summary>
        /// 获取表单的所有者。
        /// </summary>
        public IWin32Window? Owner => HostWindow?.Owner;

        /// <summary>
        /// 获取一个值,指示表单是否处于全屏状态。
        /// </summary>
        public bool IsFullscreen { get; private set; }

        /// <summary>
        /// 获取或设置一个值,指示表单是否可以响应用户交互。
        /// </summary>
        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (value == _enabled) return;

                _enabled = value;

                OnEnabledChangedCore();
            }
        }
        /// <summary>
        /// 获取一个值,指示浏览器是否可以后退。
        /// </summary>
        public bool CanGoBack => Browser?.CanGoBack ?? false;

        /// <summary>
        /// 获取一个值,指示浏览器是否可以前进。
        /// </summary>
        public bool CanGoForward => Browser?.CanGoForward ?? false;

        /// <summary>
        /// 获取一个值,指示浏览器是否正在加载。
        /// </summary>
        public bool IsLoading => Browser?.IsLoading ?? false;

        /// <summary>
        /// 浏览器后退。
        /// </summary>
        public void GoBack() => Browser?.GoBack();

        /// <summary>
        /// 浏览器前进。
        /// </summary>
        public void GoForward() => Browser?.GoForward();

        /// <summary>
        /// 重新加载浏览器。
        /// </summary>
        /// <param name="igroneCache">
        /// 指示是否应忽略缓存的值。
        /// </param>
        public void Reload(bool igroneCache = false)
        {
            if (igroneCache)
            {
                Browser?.ReloadIgnoreCache();
            }
            else
            {
                Browser?.Reload();
            }
        }

        /// <summary>
        /// 获取一个值,指示浏览器是否有 DevTools 窗口。
        /// </summary>
        public bool HasDevTools => BrowserHost?.HasDevTools ?? false;

        /// <summary>
        /// 显示 DevTools 窗口。
        /// </summary>
        public void ShowDevTools()
        {
            WebView.ShowDevTools();
        }

        /// <summary>
        /// 隐藏 DevTools 窗口。
        /// </summary>
        public void CloseDevTools()
        {
            WebView.HideDevTools();
        }

        /// <summary>
        /// 将表单居中于父表单的边界内。
        /// </summary>
        public void CenterToParent()
        {
            if (OwnerHandle == null)
            {
                CenterToScreen();
                return;
            }

            GetWindowRect(OwnerHandle.Value, out var rect);

            InvokeOnUIThread(() => { Location = new Point(rect.Left + (rect.Width - Width) / 2, rect.Top + (rect.Height - Height) / 2); });
        }

        /// <summary>
        /// 将表单居中于当前屏幕。
        /// </summary>
        public void CenterToScreen()
        {
            var screen = Screen.FromHandle(Handle);

            if (screen == null) return;

            InvokeOnUIThread(() => { Location = new Point(screen.WorkingArea.Left + (screen.WorkingArea.Width - Width) / 2, screen.WorkingArea.Top + (screen.WorkingArea.Height - Height) / 2); });
        }



        /// <summary>
        /// 显示表单。
        /// </summary>
        public void Show()
        {
            InvokeOnUIThread(() =>
            {
                HostWindow?.Show();

            });
        }

        /// <summary>
        /// 以指定所有者显示表单。
        /// </summary>
        /// <param name="owner">任何实现了 <see cref="IWin32Window"/> 的对象,表示将拥有此表单的顶层窗口。</param>
        public void Show(IWin32Window owner)
        {
            InvokeOnUIThread(() =>
            {
                HostWindow?.Show(owner);

                HostWindow?.Activate();
            });

        }

        /// <summary>
        /// 以指定所有者显示表单。
        /// </summary>
        /// <param name="owner">任何实现了 <see cref="RobotWindow"/> 的对象,表示将拥有此表单的顶层窗口。</param>
        public void Show(RobotWindow owner)
        {
            InvokeOnUIThread(() =>
            {
                HostWindow?.Show(owner.HostWindow);

                HostWindow?.Activate();
            });
        }

        /// <summary>
        /// 创建表单但暂不显示。
        /// </summary>
        public void ShowInvisible()
        {
            InvokeOnUIThread(() =>
            {
                HostWindow?.ShowInvisible();
            });
        }

        //public void ShowInvisible(IWin32Window owner)
        //{
        //    InvokeOnUIThread(() =>
        //    {
        //        HostWindow?.ShowInvisible();
        //    });
        //}

        //public void ShowInvisible(RobotForm owner)
        //{
        //    InvokeOnUIThread(() =>
        //    {
        //        HostWindow?.ShowInvisible();
        //    });
        //}

        /// <summary>
        /// 以模态对话框方式显示表单。
        /// </summary>
        public DialogResult ShowDialog()
        {
            return InvokeOnUIThread(() =>
            {
                return HostWindow?.ShowDialog() ?? DialogResult.None;
            });
        }

        /// <summary>
        /// 以指定所有者的模态对话框方式显示表单。
        /// </summary>
        /// <param name="owner">任何实现了 <see cref="IWin32Window"/> 的对象,表示将拥有此表单的顶层窗口。</param>
        public DialogResult ShowDialog(IWin32Window owner)
        {
            return InvokeOnUIThread(() =>
            {
                return HostWindow?.ShowDialog(owner) ?? DialogResult.None;
            });
        }

        /// <summary>
        /// 以指定所有者的模态对话框方式显示表单。
        /// </summary>
        /// <param name="owner">
        /// 任何实现了 <see cref="RobotWindow"/> 的对象,表示将拥有此表单的顶层窗口。
        /// </param>
        public DialogResult ShowDialog(RobotWindow owner)
        {
            return InvokeOnUIThread(() =>
            {
                return HostWindow?.ShowDialog(owner.HostWindow) ?? DialogResult.None;
            });
        }

        /// <summary>
        /// 以指定所有者的模态对话框方式显示表单。
        /// </summary>
        /// <param name="handle">
        /// 表单所有者窗口的句柄。
        /// </param>
        public DialogResult ShowDialog(IntPtr handle)
        {
            return InvokeOnUIThread(() =>
            {
                var owner = new Win32WindowWrap(handle);

                return HostWindow?.ShowDialog(owner) ?? DialogResult.None;
            });

        }

        /// <summary>
        /// 以指定所有者显示表单。
        /// </summary>
        /// <param name="handle">
        /// 表单所有者窗口的句柄。
        /// </param>
        public void Show(IntPtr handle)
        {
            InvokeOnUIThread(() =>
            {
                var owner = new Win32WindowWrap(handle);

                HostWindow?.Show(owner);

                HostWindow?.Activate();
            });

        }

        /// <summary>
        /// 隐藏表单。
        /// </summary>
        public void Hide()
        {
            InvokeOnUIThread(() => HostWindow?.Hide());
        }

        /// <summary>
        /// 关闭表单。
        /// </summary>
        public void Close()
        {
            InvokeOnUIThread(() => HostWindow?.Close());
        }

        /// <summary>
        /// 获取一个值,指示表单是否已释放。
        /// </summary>
        public bool IsDisposed => HostWindow?.IsDisposed ?? true;

        #region InvokeOnUIThread
        /// <summary>
        /// 在 UI 线程上异步执行 Action,不阻塞调用线程的执行。
        /// </summary>
        /// <param name="action">要在表单上执行的操作。</param>
        public void InvokeOnUIThread(Action action)
        {
            if (HostWindow == null || HostWindow.IsDisposed) return;

            if (HostWindow!.InvokeRequired)
            {
                HostWindow!.Invoke(new System.Windows.Forms.MethodInvoker(action));
            }
            else
            {
                action.Invoke();
            }
        }

        /// <summary>
        /// 在 UI 线程上同步执行指定委托,阻塞调用线程直到 action 执行完毕。
        /// </summary>
        /// <param name="method">
        /// 包含要在 UI 线程上下文中调用的方法的委托。
        /// </param>
        /// <param name="args">
        /// 作为参数传递给指定方法的对象数组。若方法不接受参数,此参数可为 null。
        /// </param>
        /// <returns>
        /// 包含被调用委托返回值的对象;若委托无返回值则为 null。
        /// </returns>
        public object? InvokeOnUIThread(Delegate method, params object[] args)
        {
            if (HostWindow == null || HostWindow.IsDisposed) return default;

            if (HostWindow!.InvokeRequired)
            {
                var r = HostWindow!.Invoke(method, args);
                return r;
            }

            var rr = method.DynamicInvoke(args);
            return rr;
        }

        /// <summary>
        /// 在 UI 线程上同步执行指定委托,阻塞调用线程直到 action 执行完毕。
        /// </summary>
        /// <typeparam name="T">方法的返回类型。</typeparam>
        /// <param name="method">要在 UI 线程上下文中调用的函数。</param>
        /// <param name="args">
        /// 作为参数传递给指定方法的对象数组。若方法不接受参数,此参数可为 null。
        /// </param>
        /// <returns>被调用函数的返回值。</returns>
        public T? InvokeOnUIThread<T>(Delegate method, params object[] args)
        {
            if (HostWindow == null || HostWindow.IsDisposed) return default;

            if (HostWindow!.InvokeRequired)
            {
                var r = (T?)HostWindow!.Invoke(method, args);
                return r;
            }

            var rr = (T?)method.DynamicInvoke(args);
            return rr;

        }

        /// <summary>
        /// 在 UI 线程上同步执行指定委托,阻塞调用线程直到 action 执行完毕。
        /// </summary>
        /// <typeparam name="T">方法的返回类型。</typeparam>
        /// <param name="method">要在 UI 线程上下文中调用的函数。</param>
        /// <returns>被调用函数的返回值。</returns>
        public T? InvokeOnUIThread<T>(Func<T> method)
        {
            if (HostWindow == null || HostWindow.IsDisposed) return default;

            if (HostWindow!.InvokeRequired)
            {
                return (T)HostWindow!.Invoke((Func<T>)method);
            }


            return method.Invoke();
        }
        #endregion

        /// <summary>
        /// 激活表单并使其获得焦点。
        /// </summary>
        public void Activate()
        {
            InvokeOnUIThread(() =>
            {
                HostWindow?.Activate();
                HostWindow?.Focus();
            });
        }

        /// <summary>
        /// 使表单获得焦点。
        /// </summary>
        public void Focus()
        {
            InvokeOnUIThread(() =>
            {
                HostWindow?.Focus();
            });
            SetBrowserFocus();
        }

        /// <summary>
        /// 释放 <see cref="RobotWindow"/> 实例。
        /// </summary>
        public void Dispose()
        {
            WebView?.Dispose();
            HostWindow?.Dispose();
        }

        /// <summary>
        /// 执行 JavaScript 代码。
        /// </summary>
        /// <param name="code">
        /// 要执行的 JavaScript 代码。
        /// </param>
        /// <param name="url">
        /// 脚本所在的地址(若存在)。
        /// </param>
        /// <param name="line">
        /// 用于错误报告的基础行号。
        /// </param>
        public void ExecuteJavaScript(string code, string url = "", int line = 0)
        {
            WebView.ExecuteJavaScript(code, url, line);
        }

        /// <summary>
        /// 在指定帧上执行 JavaScript 代码。
        /// </summary>
        /// <param name="frame">
        /// 要执行 JavaScript 代码的帧。
        /// </param>
        /// <param name="code">
        /// 要执行的 JavaScript 代码。
        /// </param>
        /// <param name="url">
        /// 脚本所在的地址(若存在)。
        /// </param>
        /// <param name="line">
        /// 用于错误报告的基础行号。
        /// </param>
        public void ExecuteJavaScript(CefFrame frame, string code, string url = "", int line = 0)
        {
            WebView.ExecuteJavaScript(frame, code, url, line);
        }

        /// <summary>
        /// 异步评估 JavaScript 代码。
        /// </summary>
        /// <param name="code">
        /// 要执行的 JavaScript 代码。
        /// </param>
        /// <param name="url">
        /// 脚本所在的地址(若存在)。
        /// </param>
        /// <param name="line">
        /// 用于错误报告的基础行号。
        /// </param>
        /// <returns>
        /// 表示异步操作的 <see cref="Task{TResult}"/>。
        /// </returns>
        public Task<JavaScriptResult> EvaluateJavaScriptAsync(string code, string url = "", int line = 0)
        {
            return WebView.EvaluateJavaScriptAsync(code, url, line);
        }

        /// <summary>
        /// 在指定帧上异步评估 JavaScript 代码。
        /// </summary>
        /// <param name="frame">
        /// 要执行 JavaScript 代码的帧。
        /// </param>
        /// <param name="code">
        /// 要执行的 JavaScript 代码。
        /// </param>
        /// <param name="url">
        /// 脚本所在的地址(若存在)。
        /// </param>
        /// <param name="line">
        /// 用于错误报告的基础行号。
        /// </param>
        /// <returns>
        /// 表示异步操作的 <see cref="Task{TResult}"/>。
        /// </returns>
        public Task<JavaScriptResult> EvaluateJavaScriptAsync(CefFrame frame, string code, string url = "", int line = 0)
        {
            return WebView.EvaluateJavaScriptAsync(frame, code, url, line);
        }

        /// <summary>
        /// 开始新的 JavaScript 对象注册。
        /// </summary>
        /// <param name="frame">
        /// 要注册 JavaScript 对象的帧。
        /// </param>
        /// <returns>
        /// 表示该注册的 <see cref="JavaScriptObjectRegisterHandle"/>。
        /// </returns>
        public JavaScriptObjectRegisterHandle BeginRegisterJavaScriptObject(CefFrame frame)
        {
            return WebView.BeginRegisterJavaScriptObject(frame);
        }

        /// <summary>
        /// 结束 JavaScript 对象注册并在渲染进程上创建对象。
        /// </summary>
        /// <param name="handle">
        /// 注册的句柄。
        /// </param>
        public void EndRegisterJavaScriptObject(JavaScriptObjectRegisterHandle handle)
        {
            WebView.EndRegisterJavaScriptObject(handle);
        }

        /// <summary>
        /// 将 JavaScript 对象注册为窗口中的外部对象。
        /// </summary>
        /// <param name="handle">
        /// 注册的句柄。
        /// </param>
        /// <param name="name">
        /// 要注册的 JavaScript 对象的名称。
        /// </param>
        /// <param name="jsObject">
        /// 要注册的 JavaScript 对象。
        /// </param>
        /// <returns>
        /// 成功返回 true,否则返回 false。
        /// </returns>
        public bool RegisterJavaScriptObject(JavaScriptObjectRegisterHandle handle, string name, JavaScriptObject jsObject)
        {
            return WebView.RegisterJavaScriptObject(handle, name, jsObject);
        }

        /// <summary>
        /// 使用对象包装器将 JavaScript 对象注册为窗口中的外部对象。
        /// </summary>
        /// <param name="handle">
        /// 注册的句柄。
        /// </param>
        /// <param name="name">
        /// 要注册的 JavaScript 对象的名称。
        /// </param>
        /// <param name="jsHostObject">
        /// 要注册的 JavaScript 对象。
        /// </param>
        /// <returns>
        /// 成功返回 true,否则返回 false。
        /// </returns>

        public bool RegisterJavaScriptObject(JavaScriptObjectRegisterHandle handle, string name, JavaScriptObjectWrapper jsHostObject)
        {
            return WebView.RegisterJavaScriptObject(handle, name, jsHostObject);
        }

        /// <summary>
        /// 向前端环境发送带有或不带有 <see cref="JavaScriptValue"/> 的消息。
        /// </summary>
        /// <param name="message">
        /// 消息名。
        /// </param>
        /// <param name="args">
        /// 传递给前端环境的 <see cref="JavaScriptValue"/>。若无需数据可为 null。
        /// </param>
        public void PostJavaScriptMessage(string message, JavaScriptValue? args = null)
        {
            var frame = Browser?.GetMainFrame();

            if (frame == null || WebView.JavaScriptWindowBindingObject == null) return;

            WebView.JavaScriptWindowBindingObject.PostBrowserMessage(frame, message, args);
        }



        /// <summary>
        /// 获取宿主窗口实例。
        /// </summary>
        public Form GetHostWindow()
        {
            return HostWindow!;
        }
        #endregion

        #region Event
        #region Lifecycle Events
        /// <summary>
        /// Occurs when the form is activated in code or by the user.
        /// </summary>
        public event EventHandler<EventArgs>? Activated;
        /// <summary>
        /// Occurs when the browser is created.
        /// </summary>
        public event EventHandler<BrowserEventArgs>? BrowserCreated; //<-- OnRenderViewReady
        /// <summary>
        /// Occurs when the form and broswser is loaded and ready for interaction.
        /// </summary>
        public event EventHandler<BrowserEventArgs>? Loaded; //<-- OnRenderViewReady
        /// <summary>
        /// Occurs when the window.document is available.
        /// </summary>
        public event EventHandler<BrowserEventArgs>? DocumentAvailable; //<-- OnDocumentAvailableInMainFrame
        /// <summary>
        /// Occurs when the form is deactivated in code or by the user.
        /// </summary>
        public event EventHandler<EventArgs>? Deactivate;
        /// <summary>
        /// Occurs before the form is closed.
        /// </summary>
        public event EventHandler<ClosingEventArgs>? Closing;
        /// <summary>
        /// Occurs when the form is closed.
        /// </summary>
        public event EventHandler<EventArgs>? Closed;
        #endregion

        #region Window Events
        /// <summary>
        /// Occurs when a form enters resizing mode.
        /// </summary>
        public event EventHandler<EventArgs>? ResizeBegin;

        /// <summary>
        /// Occurs when the form is resized.
        /// </summary>
        public event EventHandler<EventArgs>? Resize;

        /// <summary>
        /// Occurs when a form exits resizing mode.
        /// </summary>
        public event EventHandler<EventArgs>? ResizeEnd;

        /// <summary>
        /// Occurs when the form is moved.
        /// </summary>
        public event EventHandler<EventArgs>? Move;



        /// <summary>
        /// Occurs whenever the form is first displayed.
        /// </summary>
        public event EventHandler<EventArgs>? Shown;

        /// <summary>
        /// Occurs when the <see cref="Visible"/> property value changes.
        /// </summary>
        public event EventHandler<EventArgs>? VisibleChanged;
        #endregion

        #region Browser Events

        // page load events


        /// <summary>
        /// Occurs when the page loading state has changed.
        /// </summary>
        public event EventHandler<PageLoadingStateChangeEventArgs>? PageLoadingStateChange;
        /// <summary>
        /// Occurs when the page load has started.
        /// </summary>
        public event EventHandler<PageLoadStartEventArgs>? PageLoadStart;
        /// <summary>
        /// Occurs when the page load has ended with one or more errors.
        /// </summary>
        public event EventHandler<PageLoadErrorEventArgs>? PageLoadError;
        /// <summary>
        /// Occurs when the page load has ended..
        /// </summary>
        public event EventHandler<PageLoadEndEventArgs>? PageLoadEnd;
        /// <summary>
        /// Occurs when the frame page load has started.
        /// </summary>
        public event EventHandler<FramePageLoadStartEventArgs>? FramePageLoadStart;
        /// <summary>
        /// Occurs when the frame page load has ended with one or more errors.
        /// </summary>
        public event EventHandler<FramePageLoadErrorEventArgs>? FramePageLoadError;
        /// <summary>
        /// Occurs when the frame page load has ended.
        /// </summary>
        public event EventHandler<FramePageLoadEndEventArgs>? FramePageLoadEnd;

        // focus events

        /// <summary>
        /// Occurs when the browser has received focus.
        /// </summary>
        public event EventHandler<EventArgs>? GotFocus;
        /// <summary>
        /// Occurs when the form loses focus.
        /// </summary>
        public event EventHandler<EventArgs>? TakeFocus;
        /// <summary>
        /// Occurs when the browser component is requesting focus.
        /// </summary>
        public event EventHandler<SetFocusEventArgs>? SetFocus;

        // drag events

        /// <summary>
        /// Occurs when an object is dragged into the form's bounds.
        /// </summary>
        public event EventHandler<DragEnterEventArgs>? DragEnter;

        // display events

        /// <summary>
        /// Occurs when the browser's title has changed.
        /// </summary>
        public event EventHandler<PageTitleChangeEventArgs>? PageTitleChange;
        /// <summary>
        /// Occurs when the browser's address has changed.
        /// </summary>
        public event EventHandler<PageAddressChangeEventArgs>? PageAddressChange;
        /// <summary>
        /// Occurs when one frame of the browser's address has changed.
        /// </summary>
        public event EventHandler<FramePageAddressChangeEventArgs>? FramePageAddressChange;
        /// <summary>
        /// Occurs when the browser's cursor has changed.
        /// </summary>
        public event EventHandler<CursorChangeEventArgs>? CursorChange;
        /// <summary>
        /// Occurs when the browser's loading progress has changed.
        /// </summary>
        public event EventHandler<PageLoadingProgressChangeEventArgs>? PageLoadingProgressChange;
        /// <summary>
        /// Occurs when the browser's favicon has changed.
        /// </summary>
        public event EventHandler<FaviconUrlChangeEventArgs>? FaviconUrlChange;
        /// <summary>
        /// Occurs when the browser's status message has changed.
        /// </summary>
        public event EventHandler<StatusMessageChangeEventArgs>? StatusMessageChange;
        /// <summary>
        /// Occurs when the console message has changed.
        /// </summary>
        public event EventHandler<ConsoleMessageEventArgs>? ConsoleMessage;
        /// <summary>
        /// Occurs when the browser's fullscreen mode has changed.
        /// </summary>
        public event EventHandler<FullscreenModeChangeEventArgs>? FullscreenModeChange;
        /// <summary>
        /// Occurs when the browser's access to an audio and/or video source has changed.
        /// </summary>
        public event EventHandler<MediaAccessChangeEventArgs>? MediaAccessChange;
        /// <summary>
        /// Occurs when the browser is about to display a tooltip.
        /// </summary>
        public event EventHandler<TooltipEventArgs>? Tooltip;

        //request events

        /// <summary>
        /// Occurs before <see cref="BeforeBrowse"/> in certain limited cases where navigating a new or different browser might be desirable.
        /// </summary>
        public event EventHandler<OpenUrlFromTabEventArgs>? OpenUrlFromTab;

        /// <summary>
        /// Occurs when the render process terminates unexpectedly.
        /// </summary>
        public event EventHandler<RenderProcessCrashedEventArgs>? RenderProcessCrashed;

        /// <summary>
        /// Occurs before browser navigation.
        /// </summary>
        public event EventHandler<BeforeBrowseEventArgs>? BeforeBrowse;

        /// <summary>
        /// Occurs when the browser needs credentials from the user.
        /// </summary>
        public event EventHandler<GetAuthCredentialsEventArgs>? AuthCredentialsRequested;

        // keyboard events

        /// <summary>
        /// Occurs before a keyboard event is sent to the browser.
        /// </summary>
        public event EventHandler<BeforeKeyEventEventArgs>? BeforeKeyEvent;

        /// <summary>
        /// Occurs when a keyboard event is sent to the browser.
        /// </summary>
        public event EventHandler<KeyEventEventArgs>? KeyEvent;

        // download events

        /// <summary>
        /// Occurs before a download begins in response to a user-initiated action such as alt + link clicking or link clicking.
        /// </summary>
        public event EventHandler<CanDownloadEventArgs>? DownloadPermissionRequested;

        /// <summary>
        /// Occurs before a download begins.
        /// </summary>
        public event EventHandler<BeforeDownloadEventArgs>? BeforeDownload;

        /// <summary>
        /// Occurs when a download's status or progress information has been updated.
        /// </summary>
        public event EventHandler<DownloadUpdatedEventArgs>? DownloadUpdated;

        #endregion

        #region Browser




        /// <summary>
        /// Raises the <see cref="DragEnter"/> event.
        /// </summary>
        /// <param name="args">The <see cref="DragEnterEventArgs"/> that contains the event data.</param>
        protected virtual void OnDragEnter(DragEnterEventArgs args)
        {
            DragEnter?.Invoke(this, args);
        }

        #region Display
        /// <summary>
        /// Raises the <see cref="Tooltip"/> event.
        /// </summary>
        /// <param name="args">The <see cref="TooltipEventArgs"/> that contains the event data.</param>
        protected virtual void OnToolTip(TooltipEventArgs args)
        {
            Tooltip?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="MediaAccessChange"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="MediaAccessChangeEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnMediaAccessChange(MediaAccessChangeEventArgs args)
        {
            MediaAccessChange?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="PageLoadingProgressChange"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="PageLoadingProgressChangeEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnPageLoadingProgressChange(PageLoadingProgressChangeEventArgs args)
        {
            PageLoadingProgressChange?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="OnFullscreenModeChange"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="FullscreenModeChangeEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnFullscreenModeChange(FullscreenModeChangeEventArgs args)
        {

            FullscreenModeChange?.Invoke(this, args);

            if (HostWindow == null || args.Cancel) return;

            SetFullscreenState(args.Fullscreen);

        }

        /// <summary>
        /// Raises the <see cref="FaviconUrlChange"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="FaviconUrlChangeEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnFaviconUrlChange(FaviconUrlChangeEventArgs args)
        {
            FaviconUrlChange?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="ConsoleMessage"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="ConsoleMessageEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnConsoleMessage(ConsoleMessageEventArgs args)
        {
            ConsoleMessage?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="StatusMessageChange"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="StatusMessageChangeEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnStatusMessageChange(StatusMessageChangeEventArgs args)
        {
            StatusMessageChange?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="PageTitleChange"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="PageTitleChangeEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnPageTitleChange(PageTitleChangeEventArgs args)
        {
            if (PageTitleChange == null)
            {
                PageTitle = args.Title;
                HostWindow!.Text = BuildTitleString();
                return;
            }
            PageTitleChange?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="PageAddressChange"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="PageAddressChangeEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnPageAddressChange(PageAddressChangeEventArgs args)
        {
            PageAddressChange?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="FramePageAddressChange"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="FramePageAddressChangeEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnFramePageAddressChange(FramePageAddressChangeEventArgs args)
        {
            FramePageAddressChange?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="CursorChange"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="CursorChangeEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnCursorChange(CursorChangeEventArgs args)
        {
            CursorChange?.Invoke(this, args);
        }
        #endregion

        #region Download
        /// <summary>
        /// Raises the <see cref="CanDownload"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="CanDownloadEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnCanDownload(CanDownloadEventArgs args)
        {
            DownloadPermissionRequested?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="BeforeDownload"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="BeforeDownloadEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnBeforeDownload(BeforeDownloadEventArgs args)
        {

            if (BeforeDownload != null)
                BeforeDownload.Invoke(this, args);
            else
            {
                args.Callback.Continue(string.Empty, true);
            }
        }

        /// <summary>
        /// Raises the <see cref="DownloadUpdated"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="DownloadUpdatedEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnDownloadUpdated(DownloadUpdatedEventArgs args)
        {
            DownloadUpdated?.Invoke(this, args);
        }
        #endregion

        #region Focus
        /// <summary>
        /// Raises the <see cref="TakeFocus"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="BrowserEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnTakeFocus(BrowserEventArgs args)
        {
            TakeFocus?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="GotFocus"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="BrowserEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnGotFocus(BrowserEventArgs args)
        {
            GotFocus?.Invoke(this, args);
        }


        /// <summary>
        /// Raises the <see cref="SetFocus"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="SetFocusEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnSetFocus(SetFocusEventArgs args)
        {
            SetFocus?.Invoke(this, args);
        }


        #endregion

        #region Keyboard
        /// <summary>
        /// Raises the <see cref="KeyEvent"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="KeyEventEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnKeyEvent(KeyEventEventArgs args)
        {
            KeyEvent?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="BeforeKeyEvent"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="BeforeKeyEventEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnPreKeyEvent(BeforeKeyEventEventArgs args)
        {
            BeforeKeyEvent?.Invoke(this, args);
        }
        #endregion

        #region Load
        /// <summary>
        /// Raises the <see cref="PageLoadStart"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="PageLoadStartEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnPageLoadStart(PageLoadStartEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("[LIFECYCLE] -> OnPageLoadStart");

            PageLoadStart?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="PageLoadError"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="PageLoadErrorEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnPageLoadError(PageLoadErrorEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("[LIFECYCLE] -> OnPageLoadError");

            PageLoadError?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="PageLoadEnd"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="PageLoadEndEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnPageLoadEnd(PageLoadEndEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("[LIFECYCLE] -> OnPageLoadEnd");

            PageLoadEnd?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="FramePageLoadStart"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="FramePageLoadStartEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnFramePageLoadStart(FramePageLoadStartEventArgs args)
        {
            FramePageLoadStart?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="FramePageLoadError"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="FramePageLoadErrorEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnFramePageLoadError(FramePageLoadErrorEventArgs args)
        {

            FramePageLoadError?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="FramePageLoadEnd"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="FramePageLoadEndEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnFramePageLoadEnd(FramePageLoadEndEventArgs args)
        {

            FramePageLoadEnd?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="PageLoadingStateChange"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="PageLoadingStateChangeEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnPageLoadingStateChange(PageLoadingStateChangeEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine($"[BROWSER] -> OnPageLoadingStateChange");

            PageLoadingStateChange?.Invoke(this, args);
        }



        #endregion

        #region Request

        /// <summary>
        /// Raises the <see cref="RenderProcessCrashed"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="RenderProcessCrashedEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnRenderProcessCrashed(RenderProcessCrashedEventArgs args)
        {
            RenderProcessCrashed?.Invoke(this, args);

        }

        /// <summary>
        /// Raises the <see cref="OpenUrlFromTab"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="OpenUrlFromTabEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnOpenUrlFromTab(OpenUrlFromTabEventArgs args)
        {
            OpenUrlFromTab?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="BeforeBrowse"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="BeforeBrowseEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnBeforeBrowse(BeforeBrowseEventArgs args)
        {
            BeforeBrowse?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="GetAuthCredentials"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="GetAuthCredentialsEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnGetAuthCredentials(GetAuthCredentialsEventArgs args)
        {
            AuthCredentialsRequested?.Invoke(this, args);
        }
        #endregion

        #endregion

        #region Window

        /// <summary>
        /// Raises the <see cref="BrowserCreated"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="BrowserEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnBrowserCreated(BrowserEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("[LIFECYCLE] -> OnBrowserCreated");

            BrowserCreated?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="Loaded"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="BrowserEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnLoaded(BrowserEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("[LIFECYCLE] -> OnLoaded");

            Loaded?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="DocumentAvailable"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="BrowserEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnDocumentAvailable(BrowserEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("[LIFECYCLE] -> OnDocumentAvailable");

            DocumentAvailable?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="Activated"/> event.
        /// </summary>
        protected virtual void OnActivated()
        {
            System.Diagnostics.Debug.WriteLine("[LIFECYCLE] -> OnActivated");

            Activated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="Deactivate"/> event.
        /// </summary>
        protected virtual void OnDeactivated()
        {
            System.Diagnostics.Debug.WriteLine("[LIFECYCLE] -> OnDeactivated");

            Deactivate?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="WindowStateChanged"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="WindowStateChangedEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnWindowStateChanged(WindowStateChangedEventArgs args)
        {
        }

        /// <summary>
        /// Raises the <see cref="Closing"/> event.
        /// </summary>
        /// <param name="args">
        /// The <see cref="ClosingEventArgs"/> that contains the event data.
        /// </param>
        protected virtual void OnClosing(ClosingEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("[LIFECYCLE] -> OnClosing");


            Closing?.Invoke(this, args);
        }

        /// <summary>
        /// Raises the <see cref="Closed"/> event.
        /// </summary>
        protected virtual void OnClosed()
        {
            System.Diagnostics.Debug.WriteLine("[LIFECYCLE] -> OnClosed");

            Closed?.Invoke(this, EventArgs.Empty);
        }






        /// <summary>
        /// Raises the <see cref="ResizeBegin"/> event.
        /// </summary>
        /// <param name="args">A <see cref="EventArgs"/> that contains the event data.</param>
        protected virtual void OnResizeBegin(EventArgs args)
        {
            //System.Diagnostics.Debug.WriteLine("[WINDOW] -> OnResizeBegin");

            ResizeBegin?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="ResizeEnd"/> event.
        /// </summary>
        /// <param name="args">A <see cref="EventArgs"/> that contains the event data.</param>
        protected virtual void OnResizeEnd(EventArgs args)
        {
            //System.Diagnostics.Debug.WriteLine("[WINDOW] -> OnResizeEnd");

            ResizeEnd?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="Resize"/> event.
        /// </summary>
        /// <param name="args">A <see cref="EventArgs"/> that contains the event data.</param>
        protected virtual void OnResize(EventArgs args)
        {
            //System.Diagnostics.Debug.WriteLine("[WINDOW] -> OnResize");

            Resize?.Invoke(this, args);

            OnWindowStateChangedCore();

        }

        /// <summary>
        /// Raises the <see cref="Move"/> event.
        /// </summary>
        /// <param name="args">A <see cref="EventArgs"/> that contains the event data.</param>
        protected virtual void OnMove(EventArgs args)
        {
            //System.Diagnostics.Debug.WriteLine("[WINDOW] -> OnMove");

            Move?.Invoke(this, args);
        }


        /// <summary>
        /// Raises the <see cref="Shown"/> event.
        /// </summary>
        /// <param name="args">A <see cref="EventArgs"/> that contains the event data.</param>
        protected virtual void OnShown(EventArgs args)
        {
            //System.Diagnostics.Debug.WriteLine("[WINDOW] -> OnShown");

            Shown?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Raises the <see cref="VisibleChanged"/> event.
        /// </summary>
        /// <param name="args">A <see cref="EventArgs"/> that contains the event data.</param>
        protected virtual void OnVisibleChanged(EventArgs args)
        {
            //System.Diagnostics.Debug.WriteLine("[WINDOW] -> OnVisibleChanged");

            VisibleChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
        #endregion

        #region IDisplayHandler
        /// <summary>
        /// Handles the tooltip display request and raises the <see cref="Tooltip"/> event.
        /// </summary>
        /// <param name="browser">The <see cref="CefBrowser"/> that raised the event.</param>
        /// <param name="text">The tooltip text.</param>
        /// <returns>True if the tooltip was handled; otherwise false.</returns>
        internal bool OnTooltipShowCore(CefBrowser browser, string text)
        {
            var args = new TooltipEventArgs(browser, text);

            InvokeOnUIThread(OnToolTip, args);

            return args.Handled;
        }

        /// <summary>
        /// Raises the <see cref="MediaAccessChange"/> event.
        /// </summary>
        /// <param name="browser">The <see cref="CefBrowser"/> that raised the event.</param>
        /// <param name="hasVideoAccess">Whether video access is granted.</param>
        /// <param name="hasAudioAccess">Whether audio access is granted.</param>
        internal void OnMediaAccessChangeCore(CefBrowser browser, bool hasVideoAccess, bool hasAudioAccess)
        {
            var args = new MediaAccessChangeEventArgs(browser, hasVideoAccess, hasAudioAccess);

            InvokeOnUIThread(OnMediaAccessChange, args);
        }

        /// <summary>
        /// Raises the <see cref="PageLoadingProgressChange"/> event.
        /// </summary>
        /// <param name="browser">The <see cref="CefBrowser"/> that raised the event.</param>
        /// <param name="progress">The loading progress as a fraction (0.0 to 1.0).</param>
        internal void OnPageLoadingProgressChangeCore(CefBrowser browser, double progress)
        {
            var args = new PageLoadingProgressChangeEventArgs(browser, (decimal)(progress * 100));

            InvokeOnUIThread(OnPageLoadingProgressChange, args);
        }

        /// <summary>
        /// Raises the <see cref="FullscreenModeChange"/> event.
        /// </summary>
        /// <param name="browser">The <see cref="CefBrowser"/> that raised the event.</param>
        /// <param name="fullscreen">Whether the browser entered fullscreen mode.</param>
        internal void OnFullscreenModeChangeCore(CefBrowser browser, bool fullscreen)
        {
            var args = new FullscreenModeChangeEventArgs(browser, fullscreen);

            InvokeOnUIThread(OnFullscreenModeChange, args);
        }

        /// <summary>
        /// Raises the <see cref="FaviconUrlChange"/> event.
        /// </summary>
        /// <param name="browser">The <see cref="CefBrowser"/> that raised the event.</param>
        /// <param name="iconUrls">The new favicon URLs.</param>
        internal void OnFaviconUrlChangeCore(CefBrowser browser, string[] iconUrls)
        {
            var args = new FaviconUrlChangeEventArgs(browser, iconUrls);

            InvokeOnUIThread(OnFaviconUrlChange, args);
        }

        /// <summary>
        /// Handles a console message and raises the <see cref="ConsoleMessage"/> event.
        /// </summary>
        /// <param name="browser">The <see cref="CefBrowser"/> that raised the event.</param>
        /// <param name="level">The log severity level.</param>
        /// <param name="message">The console message text.</param>
        /// <param name="source">The source of the message.</param>
        /// <param name="line">The line number of the message.</param>
        /// <returns>True if the message was handled; otherwise false.</returns>
        internal bool OnConsoleMessageCore(CefBrowser browser, CefLogSeverity level, string message, string source, int line)
        {
            var args = new ConsoleMessageEventArgs(browser, level, message, source, line);

            InvokeOnUIThread(OnConsoleMessage, args);

            return args.Handled;
        }

        /// <summary>
        /// Raises the <see cref="StatusMessageChange"/> event.
        /// </summary>
        /// <param name="browser">The <see cref="CefBrowser"/> that raised the event.</param>
        /// <param name="value">The new status message.</param>
        internal void OnStatusMessageCore(CefBrowser browser, string value)
        {
            var args = new StatusMessageChangeEventArgs(browser, value);

            InvokeOnUIThread(OnStatusMessageChange, args);
        }

        /// <summary>
        /// Raises the <see cref="PageTitleChange"/> event.
        /// </summary>
        /// <param name="browser">The <see cref="CefBrowser"/> that raised the event.</param>
        /// <param name="title">The new page title.</param>
        internal void OnPageTitleChangeCore(CefBrowser browser, string title)
        {
            var args = new PageTitleChangeEventArgs(browser, title);

            InvokeOnUIThread(OnPageTitleChange, args);
        }

        /// <summary>
        /// Handles a cursor change and raises the <see cref="CursorChange"/> event.
        /// </summary>
        /// <param name="browser">The <see cref="CefBrowser"/> that raised the event.</param>
        /// <param name="cursorHandle">The native cursor handle.</param>
        /// <param name="type">The standard cursor type.</param>
        /// <param name="customCursorInfo">Custom cursor information.</param>
        /// <returns>True if the cursor change was handled; otherwise false.</returns>
        internal bool OnCursorChangeCore(CefBrowser browser, IntPtr cursorHandle, CefCursorType type, CefCursorInfo customCursorInfo)
        {
            var args = new CursorChangeEventArgs(browser, cursorHandle, type, customCursorInfo);
            OnCursorChange(args);
            return args.Handled;
        }

        /// <summary>
        /// Raises the <see cref="PageAddressChange"/> event for the main frame and the
        /// <see cref="FramePageAddressChange"/> event for any frame.
        /// </summary>
        /// <param name="browser">The <see cref="CefBrowser"/> that raised the event.</param>
        /// <param name="frame">The frame whose address changed.</param>
        /// <param name="url">The new address.</param>
        internal void OnPageAddressChangeCore(CefBrowser browser, CefFrame frame, string url)
        {
            if (frame.IsMain)
            {
                var args1 = new PageAddressChangeEventArgs(browser, frame, url);

                InvokeOnUIThread(OnPageAddressChange, args1);
            }

            var args2 = new FramePageAddressChangeEventArgs(browser, frame, url);
            InvokeOnUIThread(OnFramePageAddressChange, args2);
        }
        #endregion

        #region IDownloadHandler
        /// <summary>
        /// Handles a download permission request and raises the <see cref="DownloadPermissionRequested"/> event.
        /// </summary>
        /// <param name="browser">The <see cref="CefBrowser"/> that raised the event.</param>
        /// <param name="url">The URL to download.</param>
        /// <param name="requestMethod">The HTTP request method.</param>
        /// <returns>True if the download is allowed; otherwise false.</returns>
        internal bool CanDownloadCore(CefBrowser browser, string url, string requestMethod)
        {
            var args = new CanDownloadEventArgs(browser, url, requestMethod);

            InvokeOnUIThread(OnCanDownload, args);

            return args.AllowDownload;
        }

        /// <summary>
        /// Raises the <see cref="BeforeDownload"/> event.
        /// </summary>
        /// <param name="browser">The <see cref="CefBrowser"/> that raised the event.</param>
        /// <param name="item">The download item.</param>
        /// <param name="suggestedName">The suggested file name.</param>
        /// <param name="callback">The callback used to continue or cancel the download.</param>
        internal void OnBeforeDownloadCore(CefBrowser browser, CefDownloadItem item, string suggestedName, CefBeforeDownloadCallback callback)
        {
            var args = new BeforeDownloadEventArgs(browser, item, suggestedName, callback);

            InvokeOnUIThread(OnBeforeDownload, args);
        }

        /// <summary>
        /// Raises the <see cref="DownloadUpdated"/> event.
        /// </summary>
        /// <param name="browser">The <see cref="CefBrowser"/> that raised the event.</param>
        /// <param name="downloadItem">The download item.</param>
        /// <param name="callback">The callback used to continue or cancel the download.</param>
        internal void OnDownloadUpdatedCore(CefBrowser browser, CefDownloadItem downloadItem, CefDownloadItemCallback callback)
        {
            var args = new DownloadUpdatedEventArgs(browser, downloadItem, callback);

            InvokeOnUIThread(OnDownloadUpdated, args);
        }
        #endregion

        #region IDragHandler
        /// <summary>
        /// Handles a drag-enter request and raises the <see cref="DragEnter"/> event.
        /// </summary>
        /// <param name="browser">The <see cref="CefBrowser"/> that raised the event.</param>
        /// <param name="dragData">The data being dragged.</param>
        /// <param name="mask">The supported drag operations.</param>
        /// <returns>True if the drag enter is allowed; otherwise false.</returns>
        internal bool OnDragEnterCore(CefBrowser browser, CefDragData dragData, CefDragOperationsMask mask)
        {
            if (!AllowDrop) return true;

            var args = new DragEnterEventArgs(browser, dragData, mask);

            InvokeOnUIThread(OnDragEnter, args);

            return args.AllowDragEnter;
        }

        /// <summary>
        /// Handles a change to the browser's draggable regions.
        /// </summary>
        /// <param name="browser">The <see cref="CefBrowser"/> that raised the event.</param>
        /// <param name="frame">The frame whose draggable regions changed.</param>
        /// <param name="regions">The new draggable regions.</param>
        internal void OnDraggableRegionsChangedCore(CefBrowser browser, CefFrame frame, CefDraggableRegion[] regions)
        {
        }
        #endregion

        #region IFocusHandler
        /// <summary>
        /// Raises the <see cref="TakeFocus"/> event.
        /// </summary>
        /// <param name="browser">The <see cref="CefBrowser"/> that raised the event.</param>
        /// <param name="next">Whether focus is moving to the next control.</param>
        internal void OnTakeFocusCore(CefBrowser browser, bool next)
        {
            var args = new BrowserEventArgs(browser);

            OnTakeFocus(args);

        }

        /// <summary>
        /// Raises the <see cref="GotFocus"/> event.
        /// </summary>
        /// <param name="browser">The <see cref="CefBrowser"/> that raised the event.</param>
        internal void OnGotFocusCore(CefBrowser browser)
        {
            var args = new BrowserEventArgs(browser);


            OnGotFocus(args);
        }

        /// <summary>
        /// Handles a focus request and raises the <see cref="SetFocus"/> event.
        /// </summary>
        /// <param name="browser">The <see cref="CefBrowser"/> that raised the event.</param>
        /// <param name="source">The source of the focus request.</param>
        /// <returns>True if the focus request was handled; otherwise false.</returns>
        internal bool OnSetFocusCore(CefBrowser browser, CefFocusSource source)
        {
            var args = new SetFocusEventArgs(browser, source);

            OnSetFocus(args);

            return args.Handled;
        }
        #endregion

        #region IKeyboardHandler
        /// <summary>
        /// Handles a keyboard event and raises the <see cref="KeyEvent"/> event.
        /// </summary>
        /// <param name="browser">The <see cref="CefBrowser"/> that raised the event.</param>
        /// <param name="keyEvent">The key event data.</param>
        /// <param name="osEvent">The native OS event handle.</param>
        /// <returns>True if the key event was handled; otherwise false.</returns>
        internal bool OnKeyEventCore(CefBrowser browser, CefKeyEvent keyEvent, nint osEvent)
        {
            var args = new KeyEventEventArgs(browser, keyEvent);

            OnKeyEvent(args);

            return args.Handled;
        }

        /// <summary>
        /// Handles a keyboard event before it is sent to the browser and raises the <see cref="BeforeKeyEvent"/> event.
        /// </summary>
        /// <param name="browser">The <see cref="CefBrowser"/> that raised the event.</param>
        /// <param name="keyEvent">The key event data.</param>
        /// <param name="os_event">The native OS event handle.</param>
        /// <param name="isKeyboardShortcut">Receives whether the key event is a keyboard shortcut.</param>
        /// <returns>True if the key event was handled; otherwise false.</returns>
        internal bool OnPreKeyEventCore(CefBrowser browser, CefKeyEvent keyEvent, nint os_event, out bool isKeyboardShortcut)
        {
            var args = new BeforeKeyEventEventArgs(browser, keyEvent);

            OnPreKeyEvent(args);

            isKeyboardShortcut = args.IsKeyboardShortcut;

            return args.Handled;
        }
        #endregion

        #region ILifeSpanHandler
        /// <summary>
        /// 浏览器创建完成后的回调,切回 UI 线程执行 <see cref="BrowserCreatedCore"/>。
        /// </summary>
        /// <param name="browser">触发事件的 <see cref="CefBrowser"/>。</param>
        internal void OnAfterCreatedCore(CefBrowser browser)
        {
            InvokeOnUIThread(() => BrowserCreatedCore(browser));
        }

        /// <summary>
        /// 请求关闭浏览器,触发关闭事件并返回是否取消关闭。
        /// </summary>
        /// <param name="browser">触发事件的 <see cref="CefBrowser"/>。</param>
        /// <returns>若取消关闭返回 true,否则 false。</returns>
        internal bool DoCloseCore(CefBrowser browser)
        {
            return OnBrowserClosingCore(browser);
        }

        /// <summary>
        /// 浏览器关闭前的回调,调用 <see cref="OnBrowserClosedCore"/> 通知已关闭。
        /// </summary>
        /// <param name="browser">触发事件的 <see cref="CefBrowser"/>。</param>
        internal void OnBeforeCloseCore(CefBrowser browser)
        {
            OnBrowserClosedCore(browser);
        }

        /// <summary>
        /// 处理弹出窗口请求:未启用内嵌浏览器时用系统默认浏览器打开目标地址,
        /// 否则交由 <see cref="BeforePopup"/> 处理。
        /// </summary>
        /// <param name="browser">触发事件的 <see cref="CefBrowser"/>。</param>
        /// <param name="frame">触发事件的帧。</param>
        /// <param name="targetUrl">目标地址。</param>
        /// <param name="targetFrameName">目标帧名称。</param>
        /// <param name="targetDisposition">窗口打开方式。</param>
        /// <param name="userGesture">是否由用户手势触发。</param>
        /// <param name="popupFeatures">弹出窗口特性。</param>
        /// <param name="windowInfo">窗口信息。</param>
        /// <param name="client">接收新的 <see cref="CefClient"/>。</param>
        /// <param name="settings">接收新的浏览器设置。</param>
        /// <param name="extraInfo">接收附加信息。</param>
        /// <param name="noJavascriptAccess">接收是否禁用 JavaScript 访问。</param>
        /// <returns>若已处理返回 true,否则 false。</returns>
        internal bool OnBeforePopupCore(CefBrowser browser, CefFrame frame, string targetUrl, string targetFrameName, CefWindowOpenDisposition targetDisposition, bool userGesture, CefPopupFeatures popupFeatures, CefWindowInfo windowInfo, ref CefClient client, CefBrowserSettings settings, ref CefDictionaryValue extraInfo, ref bool noJavascriptAccess)
        {
            var useEmbeddedBrowser = Robot.App.Program.UseEmbeddedBrowser;

            if (!useEmbeddedBrowser)
            {
                var ps = new System.Diagnostics.ProcessStartInfo(targetUrl)
                {
                    UseShellExecute = true,
                    Verb = "open"
                };
                System.Diagnostics.Process.Start(ps);

                return true;
            }

            return BeforePopup(browser, frame, targetUrl, targetFrameName, targetDisposition, userGesture, popupFeatures, windowInfo, ref client, settings, ref extraInfo, ref noJavascriptAccess);
        }

        /// <summary>
        /// 触发关闭事件,切回 UI 线程执行 <see cref="OnClosingCore"/>。
        /// </summary>
        /// <param name="browser">触发事件的 <see cref="CefBrowser"/>。</param>
        /// <returns>若取消关闭返回 true,否则 false。</returns>
        private bool OnBrowserClosingCore(CefBrowser browser)
        {
            var args = new ClosingEventArgs();

            InvokeOnUIThread(OnClosingCore, args);

            return args.Cancel;
        }

        /// <summary>
        /// 触发已关闭事件,切回 UI 线程执行 <see cref="OnClosedCore"/>。
        /// </summary>
        /// <param name="browser">触发事件的 <see cref="CefBrowser"/>。</param>
        private void OnBrowserClosedCore(CefBrowser browser)
        {
            InvokeOnUIThread(OnClosedCore);
        }
        #endregion

        #region ILoadHandler
        /// <summary>
        /// 页面加载完成回调,调用 <see cref="OnPageLoadEndCore"/> 通知加载结束。
        /// </summary>
        /// <param name="browser">触发事件的 <see cref="CefBrowser"/>。</param>
        /// <param name="frame">触发事件的帧。</param>
        /// <param name="httpStatusCode">HTTP 状态码。</param>
        internal void OnLoadEndCore(CefBrowser browser, CefFrame frame, int httpStatusCode)
        {
            OnPageLoadEndCore(browser, frame, httpStatusCode);
        }

        /// <summary>
        /// 页面加载失败回调,调用 <see cref="OnPageLoadErrorCore"/> 通知加载错误。
        /// </summary>
        /// <param name="browser">触发事件的 <see cref="CefBrowser"/>。</param>
        /// <param name="frame">触发事件的帧。</param>
        /// <param name="errorCode">错误码。</param>
        /// <param name="errorText">错误描述文本。</param>
        /// <param name="failedUrl">加载失败的地址。</param>
        internal void OnLoadErrorCore(CefBrowser browser, CefFrame frame, CefErrorCode errorCode, string errorText, string failedUrl)
        {
            OnPageLoadErrorCore(browser, frame, errorCode, errorText, failedUrl);
        }

        /// <summary>
        /// 加载状态变化回调,调用 <see cref="OnPageLoadingStateChangeCore"/> 通知状态变化。
        /// </summary>
        /// <param name="browser">触发事件的 <see cref="CefBrowser"/>。</param>
        /// <param name="isLoading">是否正在加载。</param>
        /// <param name="canGoBack">是否可以后退。</param>
        /// <param name="canGoForward">是否可以前进。</param>
        internal void OnLoadingStateChangeCore(CefBrowser browser, bool isLoading, bool canGoBack, bool canGoForward)
        {
            OnPageLoadingStateChangeCore(browser, isLoading, canGoBack, canGoForward);
        }

        /// <summary>
        /// 页面开始加载回调,调用 <see cref="OnPageLoadStartCore"/> 通知加载开始。
        /// </summary>
        /// <param name="browser">触发事件的 <see cref="CefBrowser"/>。</param>
        /// <param name="frame">触发事件的帧。</param>
        /// <param name="transitionType">页面跳转类型。</param>
        internal void OnLoadStartCore(CefBrowser browser, CefFrame frame, CefTransitionType transitionType)
        {
            OnPageLoadStartCore(browser, frame, transitionType);
        }

        /// <summary>
        /// 页面开始加载:主帧触发 <see cref="OnPageLoadStart"/> 事件,任意帧触发 <see cref="OnFramePageLoadStart"/> 事件。
        /// </summary>
        /// <param name="browser">触发事件的 <see cref="CefBrowser"/>。</param>
        /// <param name="frame">触发事件的帧。</param>
        /// <param name="transitionType">页面跳转类型。</param>
        private void OnPageLoadStartCore(CefBrowser browser, CefFrame frame, CefTransitionType transitionType)
        {
            if (frame.IsMain)
            {
                var args1 = new PageLoadStartEventArgs(browser, frame, transitionType);

                InvokeOnUIThread(OnPageLoadStart, args1);
            }

            var args2 = new FramePageLoadStartEventArgs(browser, frame, transitionType);



            InvokeOnUIThread(OnFramePageLoadStart, args2);
        }

        /// <summary>
        /// 记录上一次页面加载错误(错误码与失败地址),用于判断是否为重复错误。
        /// </summary>
        (CefErrorCode, string)? _lastPageLoadError;
        /// <summary>
        /// 连续相同页面加载错误的重试计数,超过阈值后抛出异常。
        /// </summary>
        int _pageLoadErrorRetryCount = 0;

        /// <summary>
        /// 页面加载失败:主帧触发 <see cref="OnPageLoadError"/> 事件并加载错误页,
        /// 连续相同错误超过阈值时抛出异常;任意帧触发 <see cref="OnFramePageLoadError"/> 事件。
        /// </summary>
        /// <param name="browser">触发事件的 <see cref="CefBrowser"/>。</param>
        /// <param name="frame">触发事件的帧。</param>
        /// <param name="errorCode">错误码。</param>
        /// <param name="errorText">错误描述文本。</param>
        /// <param name="failedUrl">加载失败的地址。</param>
        private void OnPageLoadErrorCore(CefBrowser browser, CefFrame frame, CefErrorCode errorCode, string errorText, string failedUrl)
        {
            if (frame.IsMain && errorCode == CefErrorCode.Aborted) return;

            if (frame.IsMain)
            {
                var args1 = new PageLoadErrorEventArgs(browser, frame, errorCode, errorText, failedUrl);
                InvokeOnUIThread(OnPageLoadError, args1);

                frame.LoadUrl($"host://pages/error/{errorCode}?text={errorText}&url={failedUrl}");


                if (_lastPageLoadError == (errorCode, failedUrl))
                {
                    _pageLoadErrorRetryCount++;
                }

                if (_pageLoadErrorRetryCount > 20)
                {
                    _pageLoadErrorRetryCount = 0;
                    _lastPageLoadError = null;

                    throw new Exception($"Page load error: {errorCode} {errorText} {failedUrl}");

                }

                _lastPageLoadError = (errorCode, failedUrl);

                HideSplash();
            }

            var args2 = new FramePageLoadErrorEventArgs(browser, frame, errorCode, errorText, failedUrl);
            InvokeOnUIThread(OnFramePageLoadError, args2);
        }

        /// <summary>
        /// 页面加载完成:主帧触发 <see cref="OnPageLoadEnd"/> 事件并隐藏启动画面;任意帧触发 <see cref="OnFramePageLoadEnd"/> 事件。
        /// </summary>
        /// <param name="browser">触发事件的 <see cref="CefBrowser"/>。</param>
        /// <param name="frame">触发事件的帧。</param>
        /// <param name="httpStatusCode">HTTP 状态码。</param>
        private void OnPageLoadEndCore(CefBrowser browser, CefFrame frame, int httpStatusCode)
        {
            if (frame.IsMain)
            {
                var args1 = new PageLoadEndEventArgs(browser, frame, httpStatusCode);
                InvokeOnUIThread(OnPageLoadEnd, args1);

                HideSplash();
            }

            var args2 = new FramePageLoadEndEventArgs(browser, frame, httpStatusCode);
            InvokeOnUIThread(OnFramePageLoadEnd, args2);


        }

        /// <summary>
        /// 加载状态变化:触发 <see cref="OnPageLoadingStateChange"/> 事件。
        /// </summary>
        /// <param name="browser">触发事件的 <see cref="CefBrowser"/>。</param>
        /// <param name="isLoading">是否正在加载。</param>
        /// <param name="canGoBack">是否可以后退。</param>
        /// <param name="canGoForward">是否可以前进。</param>
        private void OnPageLoadingStateChangeCore(CefBrowser browser, bool isLoading, bool canGoBack, bool canGoForward)
        {
            var args = new PageLoadingStateChangeEventArgs(browser, isLoading, canGoBack, canGoForward);
            InvokeOnUIThread(OnPageLoadingStateChange, args);
        }
        #endregion

        #region IRenderHandler
        /// <summary>
        /// 创建并返回无障碍处理器。
        /// </summary>
        /// <returns>新的 <see cref="RobotFormAccessibilityHandler"/>。</returns>
        internal CefAccessibilityHandler? GetAccessibilityHandlerCore()
        {
            return new RobotFormAccessibilityHandler();
        }

        /// <summary>
        /// 加速绘制回调,当前未实现。
        /// </summary>
        internal void OnAcceleratedPaintCore()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 获取触摸手柄尺寸,当前返回空尺寸。
        /// </summary>
        /// <param name="orientation">对齐方向。</param>
        /// <param name="size">输出的尺寸。</param>
        internal void GetTouchHandleSizeCore(CefHorizontalAlignment orientation, out CefSize size)
        {
            size = new CefSize(0, 0);
        }

        /// <summary>
        /// 触摸手柄状态变化回调,当前为空实现。
        /// </summary>
        /// <param name="state">触摸手柄状态。</param>
        internal void OnTouchHandleStateChangedCore(CefTouchHandleState state)
        {
        }

        /// <summary>
        /// 获取根屏幕矩形,当前返回 false(未提供)。
        /// </summary>
        /// <param name="rect">输出的屏幕矩形。</param>
        /// <returns>是否提供了屏幕矩形。</returns>
        internal bool GetRootScreenRectCore(ref CefRectangle rect)
        {
            return false;
        }

        /// <summary>
        /// 获取屏幕信息,调用 <see cref="GetScreenInfo"/> 填充设备缩放因子。
        /// </summary>
        /// <param name="screenInfo">待填充的屏幕信息。</param>
        /// <returns>是否成功填充。</returns>
        internal bool GetScreenInfoCore(CefScreenInfo screenInfo)
        {
            return GetScreenInfo(screenInfo);
        }

        /// <summary>
        /// 将视图坐标转换为屏幕坐标,调用 <see cref="GetScreenPoint"/>。
        /// </summary>
        /// <param name="viewX">视图 X 坐标。</param>
        /// <param name="viewY">视图 Y 坐标。</param>
        /// <param name="screenX">输出的屏幕 X 坐标。</param>
        /// <param name="screenY">输出的屏幕 Y 坐标。</param>
        /// <returns>是否成功转换。</returns>
        internal bool GetScreenPointCore(int viewX, int viewY, ref int screenX, ref int screenY)
        {
            return GetScreenPoint(viewX, viewY, ref screenX, ref screenY);
        }

        /// <summary>
        /// 获取视图矩形,调用 <see cref="GetViewRect"/>。
        /// </summary>
        /// <param name="rect">输出的视图矩形。</param>
        internal void GetViewRectCore(out CefRectangle rect)
        {
            GetViewRect(out rect);
        }

        /// <summary>
        /// 弹出窗口显示状态变化,调用 <see cref="OnPopupShow"/>。
        /// </summary>
        /// <param name="show">是否显示。</param>
        internal void OnPopupShowCore(bool show)
        {
            OnPopupShow(show);
        }

        /// <summary>
        /// 弹出窗口尺寸变化,调用 <see cref="OnPopupSize"/>。
        /// </summary>
        /// <param name="rect">弹出窗口矩形。</param>
        internal void OnPopupSizeCore(CefRectangle rect)
        {
            OnPopupSize(rect);
        }

        /// <summary>
        /// IME 组合范围变化,调用 <see cref="OnImeCompositionRangeChanged"/>。
        /// </summary>
        /// <param name="selectedRange">选中的字符范围。</param>
        /// <param name="characterBounds">各字符的边界矩形。</param>
        internal void OnImeCompositionRangeChangedCore(CefRange selectedRange, CefRectangle[] characterBounds)
        {
            OnImeCompositionRangeChanged(selectedRange, characterBounds);
        }

        /// <summary>
        /// 离屏绘制回调,调用 <see cref="OnOffscreenPaint"/>。
        /// </summary>
        /// <param name="type">绘制元素类型。</param>
        /// <param name="dirtyRects">脏区域矩形数组。</param>
        /// <param name="buffer">像素缓冲区指针。</param>
        /// <param name="width">缓冲区宽度。</param>
        /// <param name="height">缓冲区高度。</param>
        internal void OnPaintCore(CefPaintElementType type, CefRectangle[] dirtyRects, nint buffer, int width, int height)
        {
            OnOffscreenPaint(type, dirtyRects, buffer, width, height);
        }

        /// <summary>
        /// 滚动偏移变化回调,当前为空实现。
        /// </summary>
        /// <param name="x">水平偏移。</param>
        /// <param name="y">垂直偏移。</param>
        internal void OnScrollOffsetChangedCore(double x, double y)
        {
        }

        /// <summary>
        /// 文本选择变化回调,当前为空实现。
        /// </summary>
        /// <param name="selectedText">选中的文本。</param>
        /// <param name="selectedRange">选中的字符范围。</param>
        internal void OnTextSelectionChangedCore(string selectedText, CefRange selectedRange)
        {
        }

        /// <summary>
        /// 请求虚拟键盘,切回 UI 线程执行 <see cref="OnVirtualKeyboardRequested"/>。
        /// </summary>
        /// <param name="inputMode">文本输入模式。</param>
        internal void OnVirtualKeyboardRequestedCore(CefTextInputMode inputMode)
        {
            InvokeOnUIThread(() => { OnVirtualKeyboardRequested(inputMode); });
        }

        /// <summary>
        /// 开始拖拽:切回 UI 线程执行 <see cref="StartDragging"/>。
        /// </summary>
        /// <param name="dragData">拖拽数据。</param>
        /// <param name="allowedOps">允许的拖拽操作。</param>
        /// <param name="x">起始 X 坐标。</param>
        /// <param name="y">起始 Y 坐标。</param>
        /// <returns>若已处理返回 true,否则 false。</returns>
        internal bool StartDraggingCore(CefDragData dragData, CefDragOperationsMask allowedOps, int x, int y)
        {
            if (HostWindow == null) return false;

            InvokeOnUIThread(() => StartDragging(dragData, allowedOps, x, y));

            return true;
        }

        /// <summary>
        /// 更新拖拽光标,当前为空实现。
        /// </summary>
        /// <param name="operation">拖拽操作。</param>
        internal void UpdateDragCursorCore(CefDragOperationsMask operation)
        {
        }

        /// <summary>
        /// 当前 DPI 缩放因子。
        /// </summary>
        internal float CurrentScaleFactor => GetCurrentScaleFactor();

        /// <summary>
        /// 获取当前窗口的 DPI 缩放因子,窗口句柄无效时返回 1.0。
        /// </summary>
        /// <returns>当前缩放因子。</returns>
        private float GetCurrentScaleFactor()
        {
            if (WindowHandle == (nint)0) return 1.0f;

            return SystemDpiManager.GetScaleFactorForWindow(WindowHandle);
        }

        /// <summary>
        /// 填充屏幕信息的设备缩放因子。
        /// </summary>
        /// <param name="screenInfo">待填充的屏幕信息。</param>
        /// <returns>始终返回 true。</returns>
        private bool GetScreenInfo(CefScreenInfo screenInfo)
        {
            screenInfo.DeviceScaleFactor = GetCurrentScaleFactor();

            return true;
        }

        /// <summary>
        /// 将视图坐标按缩放因子换算后转换为屏幕坐标。
        /// </summary>
        /// <param name="viewX">视图 X 坐标。</param>
        /// <param name="viewY">视图 Y 坐标。</param>
        /// <param name="screenX">输出的屏幕 X 坐标。</param>
        /// <param name="screenY">输出的屏幕 Y 坐标。</param>
        /// <returns>窗口句柄有效返回 true,否则 false。</returns>
        private bool GetScreenPoint(int viewX, int viewY, ref int screenX, ref int screenY)
        {
            if (WindowHandle == (nint)0) return false;


            var pt = new POINT((int)(Math.Ceiling(viewX * CurrentScaleFactor)), (int)(Math.Ceiling(viewY * CurrentScaleFactor)));

            ClientToScreen(WindowHandle, ref pt);

            screenX = pt.X;
            screenY = pt.Y;

            return true;
        }


        /// <summary>
        /// 计算视图矩形:窗口句柄无效时返回空矩形,否则按缩放因子换算客户端尺寸,
        /// 窗口最小化或客户区为空时改用窗口正常位置尺寸,并记录离屏视图矩形。
        /// </summary>
        /// <param name="rect">输出的视图矩形。</param>
        private void GetViewRect(out CefRectangle rect)
        {
            if (WindowHandle == (nint)0)
            {
                rect = new CefRectangle(0, 0, 0, 0);
                return;
            }

            rect = new CefRectangle();

            GetClientRect(WindowHandle, out var clientRect);

            rect.X = rect.Y = 0;

            if (IsIconic(WindowHandle) || clientRect.Width == 0 || clientRect.Height == 0)
            {
                var placement = new WINDOWPLACEMENT();

                GetWindowPlacement(WindowHandle, ref placement);

                clientRect = placement.rcNormalPosition;

                rect.Width = (int)(Math.Ceiling(clientRect.Width / CurrentScaleFactor));
                rect.Height = (int)(Math.Ceiling(clientRect.Height / CurrentScaleFactor));
            }
            else
            {
                rect.Width = (int)(Math.Ceiling(clientRect.Width / CurrentScaleFactor));
                rect.Height = (int)(Math.Ceiling(clientRect.Height / CurrentScaleFactor));
            }

            _offscreenViewRect = new CefRectangle
            {
                X = 0,
                Y = 0,
                Width = clientRect.Width,
                Height = clientRect.Height
            };
        }

        /// <summary>
        /// 记录弹出窗口显示状态,隐藏时清空弹出窗口矩形。
        /// </summary>
        /// <param name="show">是否显示。</param>
        private void OnPopupShow(bool show)
        {
            _offscreenIsPopupShown = show;

            if (!show)
            {
                _offscreenPopupRect = null;
            }
        }

        /// <summary>
        /// 记录弹出窗口矩形,按缩放因子换算为屏幕坐标。
        /// </summary>
        /// <param name="rect">弹出窗口矩形。</param>
        private void OnPopupSize(CefRectangle rect)
        {
            _offscreenPopupRect = new CefRectangle
            {
                X = (int)(Math.Ceiling(rect.X * CurrentScaleFactor)),
                Y = (int)(Math.Ceiling(rect.Y * CurrentScaleFactor)),
                Width = (int)(Math.Ceiling(rect.Width * CurrentScaleFactor)),
                Height = (int)(Math.Ceiling(rect.Height * CurrentScaleFactor))
            };
        }

        /// <summary>
        /// 离屏弹出窗口矩形(屏幕坐标),未显示时为 null。
        /// </summary>
        CefRectangle? _offscreenPopupRect;
        /// <summary>
        /// 离屏视图矩形(视图坐标)。
        /// </summary>
        CefRectangle? _offscreenViewRect;
        /// <summary>
        /// 离屏弹出窗口是否显示。
        /// </summary>
        bool _offscreenIsPopupShown;

        /// <summary>
        /// 离屏绘制处理,当前为空实现。
        /// </summary>
        /// <param name="type">绘制元素类型。</param>
        /// <param name="dirtyRects">脏区域矩形数组。</param>
        /// <param name="buffer">像素缓冲区指针。</param>
        /// <param name="width">缓冲区宽度。</param>
        /// <param name="height">缓冲区高度。</param>
        private void OnOffscreenPaint(CefPaintElementType type, CefRectangle[] dirtyRects, nint buffer, int width, int height)
        {
        }


        /// <summary>
        /// 根据虚拟键盘输入模式设置焦点是否位于可编辑元素。
        /// </summary>
        /// <param name="inputMode">文本输入模式。</param>
        private void OnVirtualKeyboardRequested(CefTextInputMode inputMode)
        {
            if (inputMode == CefTextInputMode.None)
            {
                SetFocusOnEditableElement(false);
            }
            else
            {
                SetFocusOnEditableElement(true);
            }
        }

        /// <summary>
        /// IME 组合范围变化:切回 UI 线程通知 <see cref="ImeHandler"/> 更新组合范围。
        /// </summary>
        /// <param name="selectedRange">选中的字符范围。</param>
        /// <param name="characterBounds">各字符的边界矩形。</param>
        private void OnImeCompositionRangeChanged(CefRange selectedRange, CefRectangle[] characterBounds)
        {
            InvokeOnUIThread(() => ImeHandler?.ChangeCompositionRange(selectedRange, characterBounds));
        }

        /// <summary>
        /// 执行系统拖拽:将拖拽数据写入 <see cref="DataObject"/> 并发起拖放,
        /// 结束后通知浏览器宿主拖拽源已结束。
        /// </summary>
        /// <param name="dragData">拖拽数据。</param>
        /// <param name="allowedOps">允许的拖拽操作。</param>
        /// <param name="x">起始 X 坐标。</param>
        /// <param name="y">起始 Y 坐标。</param>
        private void StartDragging(CefDragData dragData, CefDragOperationsMask allowedOps, int x, int y)
        {
            if (HostWindow == null) return;

            var dataObj = new DataObject();


            if (!string.IsNullOrEmpty(dragData.FragmentText))
                dataObj.SetText(dragData.FragmentText, TextDataFormat.Text);
            else if (!string.IsNullOrEmpty(dragData.FragmentHtml))
                dataObj.SetText(dragData.FragmentHtml, TextDataFormat.Html);


            var result = HostWindow.DoDragDrop(dataObj, GetDragDropEffects(allowedOps));

            var ops = GetCefDragOperationsMask(result);

            BrowserHost?.DragSourceEndedAt(x, y, ops);
            BrowserHost?.DragSourceSystemDragEnded();
        }



        /// <summary>
        /// 浏览器无障碍处理器,当前为空实现。
        /// </summary>
        internal class RobotFormAccessibilityHandler : CefAccessibilityHandler
        {
            /// <summary>
            /// 无障碍位置变化回调,当前为空实现。
            /// </summary>
            /// <param name="value">无障碍位置数据。</param>
            protected override void OnAccessibilityLocationChange(CefValue value)
            {
            }

            /// <summary>
            /// 无障碍树变化回调,当前为空实现。
            /// </summary>
            /// <param name="value">无障碍树数据。</param>
            protected override void OnAccessibilityTreeChange(CefValue value)
            {
            }

        }

        #endregion

        #region IRenderHandler.Implements
        /// <summary>
        /// 离屏模式下的输入法处理器,负责处理 IME 组合输入。
        /// </summary>
        internal OffscreenImeHandler? ImeHandler { get; set; }

        /// <summary>
        /// 注册离屏模式下的宿主窗口事件:将键盘与鼠标事件转发为浏览器输入事件。
        /// </summary>
        internal void RegisterOffscreenModeEvents()
        {
            if (HostWindow == null) throw new NullReferenceException();

            HostWindow.KeyDown += (_, args) => OffscreenKeyDown(args);
            HostWindow.KeyUp += (_, args) => OffscreenKeyUp(args);
            HostWindow.KeyPress += (_, args) => OffscreenKeyPress(args);

            HostWindow.MouseMove += (_, args) => OffscreenMouseMove(args);
            HostWindow.MouseLeave += (_, args) => OffscreenMouseLeave();

            HostWindow.MouseDown += (_, args) => OffscreenMouseDown(args);
            HostWindow.MouseUp += (_, args) => OffscreenMouseUp(args);

            HostWindow.MouseClick += (_, args) => OffscreenMouseClick(args);

            HostWindow.MouseWheel += (_, args) => OffscreenMouseWheel(args);

        }

        /// <summary>
        /// 取消当前 IME 组合输入:通知处理器取消,并向浏览器提交空文本、结束组合。
        /// </summary>
        /// <param name="host">浏览器宿主。</param>
        private void CancelImeComposition(CefBrowserHost host)
        {
            ImeHandler?.OnImeCancelComposition();

            host.ImeCommitText(string.Empty, new CefRange(int.MaxValue, int.MaxValue), 0);

            host.ImeSetComposition(string.Empty, 0, new CefCompositionUnderline(), new CefRange(int.MaxValue, int.MaxValue), new CefRange(0, 0));


            host.ImeFinishComposingText(false);

            //SendMessage(WindowHandle, WindowMessage.WM_IME_KEYDOWN, 0);
        }

        /// <summary>
        /// 离屏鼠标滚轮:将宿主窗口坐标换算为视图坐标后,向浏览器发送滚轮事件。
        /// </summary>
        /// <param name="e">鼠标事件。</param>
        private void OffscreenMouseWheel(MouseEventArgs e)
        {
            var pt = e.Location;

            GetPointInCurrentView(ref pt);

            BrowserHost?.SendMouseWheelEvent(new CefMouseEvent(pt.X, pt.Y, GetMouseModifiers(e.Button)), 0, e.Delta);

        }


        /// <summary>
        /// 离屏鼠标按下:换算坐标后,向浏览器发送鼠标按下事件。
        /// </summary>
        /// <param name="e">鼠标事件。</param>
        private void OffscreenMouseDown(MouseEventArgs e)
        {
            var pt = e.Location;


            GetPointInCurrentView(ref pt);

            CefMouseButtonType? buttonType = null;

            switch (e.Button)
            {
                case MouseButtons.Right:
                    buttonType = CefMouseButtonType.Right;
                    break;
                case MouseButtons.Middle:
                    buttonType = CefMouseButtonType.Middle;
                    break;
                case MouseButtons.Left:
                    buttonType = CefMouseButtonType.Left;
                    break;
            }

            if (buttonType.HasValue)
            {
                BrowserHost?.SendMouseClickEvent(new CefMouseEvent(pt.X, pt.Y, GetMouseModifiers(e.Button)), buttonType.Value, false, e.Clicks);
            }
        }

        /// <summary>
        /// 离屏鼠标抬起:换算坐标后,向浏览器发送鼠标抬起事件。
        /// </summary>
        /// <param name="e">鼠标事件。</param>
        private void OffscreenMouseUp(MouseEventArgs e)
        {
            var pt = e.Location;


            GetPointInCurrentView(ref pt);

            CefMouseButtonType? buttonType = null;

            switch (e.Button)
            {
                case MouseButtons.Right:
                    buttonType = CefMouseButtonType.Right;
                    break;
                case MouseButtons.Middle:
                    buttonType = CefMouseButtonType.Middle;
                    break;
                case MouseButtons.Left:
                    buttonType = CefMouseButtonType.Left;
                    break;
            }

            if (buttonType.HasValue)
            {
                BrowserHost?.SendMouseClickEvent(new CefMouseEvent(pt.X, pt.Y, GetMouseModifiers(e.Button)), buttonType.Value, true, e.Clicks);
            }
        }

        /// <summary>
        /// 离屏鼠标点击:侧键 XButton1 后退、XButton2 前进。
        /// </summary>
        /// <param name="e">鼠标事件。</param>
        private void OffscreenMouseClick(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.XButton1)
            {
                if (Browser?.CanGoBack == true)
                    Browser?.GoBack();
            }

            if (e.Button == MouseButtons.XButton2)
            {
                if (Browser?.CanGoForward == true)
                    Browser?.GoForward();
            }

        }

        /// <summary>
        /// 离屏鼠标离开:向浏览器发送鼠标移出(离开)事件。
        /// </summary>
        private void OffscreenMouseLeave()
        {
            BrowserHost?.SendMouseMoveEvent(new CefMouseEvent(0, 0, CefEventFlags.None), true);
        }

        /// <summary>
        /// 离屏鼠标移动:换算坐标后,向浏览器发送鼠标移动事件。
        /// </summary>
        /// <param name="e">鼠标事件。</param>
        private void OffscreenMouseMove(MouseEventArgs e)
        {
            var pt = e.Location;

            GetPointInCurrentView(ref pt);

            BrowserHost?.SendMouseMoveEvent(new CefMouseEvent(pt.X, pt.Y, GetMouseModifiers(e.Button)), false);
        }

        /// <summary>
        /// 离屏按键按下:向浏览器发送按键按下事件。
        /// </summary>
        /// <param name="e">键盘事件。</param>
        private void OffscreenKeyDown(KeyEventArgs e)
        {
            BrowserHost?.SendKeyEvent(new CefKeyEvent
            {
                EventType = CefKeyEventType.KeyDown,
                WindowsKeyCode = (int)e.KeyCode,
                NativeKeyCode = (int)e.KeyValue,
                Modifiers = GetKeyboardModifiers(e.Modifiers),
                FocusOnEditableField = _isOnEditableField,
            });
        }

        /// <summary>
        /// 离屏按键抬起:向浏览器发送按键抬起事件。
        /// </summary>
        /// <param name="e">键盘事件。</param>
        private void OffscreenKeyUp(KeyEventArgs e)
        {
            BrowserHost?.SendKeyEvent(new CefKeyEvent
            {
                EventType = CefKeyEventType.KeyUp,
                WindowsKeyCode = (int)e.KeyCode,
                NativeKeyCode = (int)e.KeyValue,
                Modifiers = GetKeyboardModifiers(e.Modifiers),
                FocusOnEditableField = !_isOnEditableField,
            });
        }



        /// <summary>
        /// 离屏按键输入:标记事件已处理,并向浏览器发送字符输入事件。
        /// </summary>
        /// <param name="e">按键输入事件。</param>
        private void OffscreenKeyPress(KeyPressEventArgs e)
        {
            e.Handled = true;

            BrowserHost?.SendKeyEvent(new CefKeyEvent
            {
                EventType = CefKeyEventType.Char,
                WindowsKeyCode = e.KeyChar,
                Character = e.KeyChar,
                UnmodifiedCharacter = e.KeyChar,
                FocusOnEditableField = _isOnEditableField,
            });
        }


        /// <summary>
        /// 将宿主窗口坐标按当前 DPI 缩放因子换算为浏览器视图坐标。
        /// </summary>
        /// <param name="point">待换算的坐标(原地修改)。</param>
        private void GetPointInCurrentView(ref Point point)
        {
            var scaleFactor = SystemDpiManager.GetScaleFactorForWindow(WindowHandle);

            point.X = (int)(point.X / scaleFactor);
            point.Y = (int)(point.Y / scaleFactor);
        }

        /// <summary>
        /// 将 WinForms 鼠标按键转换为 Cef 鼠标修饰键标志。
        /// </summary>
        /// <param name="mouseButtons">WinForms 鼠标按键。</param>
        /// <returns>对应的 Cef 鼠标修饰键标志。</returns>
        private static CefEventFlags GetMouseModifiers(MouseButtons mouseButtons)
        {
            var modifiers = new CefEventFlags();

            if (mouseButtons == MouseButtons.Left)
                modifiers |= CefEventFlags.LeftMouseButton;

            if (mouseButtons == MouseButtons.Middle)
                modifiers |= CefEventFlags.MiddleMouseButton;

            if (mouseButtons == MouseButtons.Right)
                modifiers |= CefEventFlags.RightMouseButton;

            return modifiers;
        }


        /// <summary>
        /// 将 WinForms 键盘修饰键转换为 Cef 键盘修饰键标志。
        /// </summary>
        /// <param name="modifiers">WinForms 键盘修饰键。</param>
        /// <returns>对应的 Cef 键盘修饰键标志。</returns>
        private static CefEventFlags GetKeyboardModifiers(Keys modifiers)
        {
            var result = new CefEventFlags();

            if (modifiers == Keys.Alt)
                result |= CefEventFlags.AltDown;

            if (modifiers == Keys.Control)
                result |= CefEventFlags.ControlDown;

            if (modifiers == Keys.Shift)
                result |= CefEventFlags.ShiftDown;

            return result;
        }

        /// <summary>
        /// 标记当前焦点是否位于可编辑字段,用于向浏览器事件传递焦点状态。
        /// </summary>
        private bool _isOnEditableField = false;

        /// <summary>
        /// 设置焦点是否位于可编辑元素:据此启用或禁用宿主窗口的 IME 模式。
        /// </summary>
        /// <param name="onEditableElement">是否位于可编辑元素。</param>
        internal void SetFocusOnEditableElement(bool onEditableElement)
        {

            if (HostWindow == null) return;

            _isOnEditableField = onEditableElement;

            if (onEditableElement == true)
            {
                HostWindow.ImeMode = ImeMode.Inherit;
            }
            else
            {
                HostWindow.ImeMode = ImeMode.Disable;
            }
        }

        /// <summary>
        /// 将 Cef 拖放操作掩码转换为 WinForms 拖放效果。
        /// </summary>
        /// <param name="mask">Cef 拖放操作掩码。</param>
        /// <returns>对应的 WinForms 拖放效果。</returns>
        private static DragDropEffects GetDragDropEffects(CefDragOperationsMask mask)
        {
            if (mask.HasFlag(CefDragOperationsMask.Every))
            {
                return DragDropEffects.Scroll | DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link;
            }
            if (mask.HasFlag(CefDragOperationsMask.Copy))
            {
                return DragDropEffects.Copy;
            }
            if (mask.HasFlag(CefDragOperationsMask.Move))
            {
                return DragDropEffects.Move;
            }
            if (mask.HasFlag(CefDragOperationsMask.Link))
            {
                return DragDropEffects.Link;
            }
            return DragDropEffects.None;

        }

        /// <summary>
        /// 将 WinForms 拖放效果转换为 Cef 拖放操作掩码。
        /// </summary>
        /// <param name="dragDropEffects">WinForms 拖放效果。</param>
        /// <returns>对应的 Cef 拖放操作掩码。</returns>
        private static CefDragOperationsMask GetCefDragOperationsMask(DragDropEffects dragDropEffects)
        {
            var operations = CefDragOperationsMask.None;

            if (dragDropEffects.HasFlag(DragDropEffects.All))
            {
                operations |= CefDragOperationsMask.Every;
            }
            if (dragDropEffects.HasFlag(DragDropEffects.Copy))
            {
                operations |= CefDragOperationsMask.Copy;
            }
            if (dragDropEffects.HasFlag(DragDropEffects.Move))
            {
                operations |= CefDragOperationsMask.Move;
            }
            if (dragDropEffects.HasFlag(DragDropEffects.Link))
            {
                operations |= CefDragOperationsMask.Link;
            }

            return operations;
        }


        /// <summary>
        /// 处理浏览器 IME 相关窗口消息:分发给 <see cref="ImeHandler"/> 并返回是否已处理。
        /// </summary>
        /// <param name="m">待处理的窗口消息(原地修改)。</param>
        /// <returns>若消息已处理返回 true,否则 false。</returns>
        private bool BrowserImeMessageHandler(ref Message m)
        {
            var msg = (WindowMessage)m.Msg;

            switch (msg)
            {
                case WindowMessage.WM_IME_SETCONTEXT:
                    {
                        ImeHandler?.OnIMESetContext(ref m);
                    }
                    return true;
                case WindowMessage.WM_IME_STARTCOMPOSITION:
                    {
                        ImeHandler?.OnImeStartComposition();
                    }
                    return true;
                case WindowMessage.WM_IME_COMPOSITION:
                    {
                        ImeHandler?.OnImeComposition(msg, m.WParam, m.LParam);
                    }
                    return true;
                case WindowMessage.WM_IME_ENDCOMPOSITION:
                    {
                        ImeHandler?.OnImeCancelComposition();
                    }
                    return false;
            }

            return false;
        }

        #endregion

        #region IRequestHandler
        /// <summary>
        /// 处理认证凭据请求,触发 <see cref="OnGetAuthCredentials"/> 事件。
        /// </summary>
        /// <param name="browser">触发事件的 <see cref="CefBrowser"/>。</param>
        /// <param name="originUrl">发起请求的原始地址。</param>
        /// <param name="isProxy">是否为代理认证。</param>
        /// <param name="host">主机名。</param>
        /// <param name="port">端口。</param>
        /// <param name="realm">认证域。</param>
        /// <param name="scheme">认证方案。</param>
        /// <param name="callback">用于返回凭据的回调。</param>
        /// <returns>若已处理返回 true,否则 false。</returns>
        internal bool GetAuthCredentialsCore(CefBrowser browser, string originUrl, bool isProxy, string host, int port, string realm, string scheme, CefAuthCallback callback)
        {
            var args = new GetAuthCredentialsEventArgs(browser, originUrl, isProxy, host, port, realm, scheme, callback);

            InvokeOnUIThread(OnGetAuthCredentials, args);


            return args.Handled;
        }


        /// <summary>
        /// 处理从标签页打开地址的请求,触发 <see cref="OnOpenUrlFromTab"/> 事件;
        /// 若未取消则在主帧加载目标地址。
        /// </summary>
        /// <param name="browser">触发事件的 <see cref="CefBrowser"/>。</param>
        /// <param name="frame">触发事件的帧。</param>
        /// <param name="targetUrl">目标地址。</param>
        /// <param name="targetDisposition">窗口打开方式。</param>
        /// <param name="userGesture">是否由用户手势触发。</param>
        /// <returns>若已处理返回 true,否则 false。</returns>
        internal bool OnOpenUrlFromTabCore(CefBrowser browser, CefFrame frame, string targetUrl, CefWindowOpenDisposition targetDisposition, bool userGesture)
        {
            var args = new OpenUrlFromTabEventArgs(browser, frame, targetUrl, targetDisposition, userGesture);

            InvokeOnUIThread(OnOpenUrlFromTab, args);

            var retval = args.Cancel;

            if (!retval)
            {
                Browser?.GetMainFrame().LoadUrl(targetUrl);
            }

            return retval;
        }


        /// <summary>
        /// 渲染进程终止回调:非正常终止时触发 <see cref="OnRenderProcessCrashed"/> 事件,
        /// 若请求重启则重新加载页面。
        /// </summary>
        /// <param name="browser">触发事件的 <see cref="CefBrowser"/>。</param>
        /// <param name="status">终止状态。</param>
        internal void OnRenderProcessTerminatedCore(CefBrowser browser, CefTerminationStatus status)
        {
            if (status == CefTerminationStatus.Termination) return;

            var args = new RenderProcessCrashedEventArgs(browser, status);

            InvokeOnUIThread(() => OnRenderProcessCrashed(args));

            if (args.RestartProcess)
            {
                browser.Reload();
            }
        }

        /// <summary>
        /// 主帧文档可用回调,触发 <see cref="OnDocumentAvailable"/> 事件。
        /// </summary>
        /// <param name="browser">触发事件的 <see cref="CefBrowser"/>。</param>
        internal void OnDocumentAvailableInMainFrameCore(CefBrowser browser)
        {
            var args = new BrowserEventArgs(browser);

            InvokeOnUIThread(OnDocumentAvailable, args);
        }

        /// <summary>
        /// 渲染视图就绪回调:创建浏览器消息拦截器,并触发 <see cref="OnLoaded"/> 事件。
        /// </summary>
        /// <param name="browser">触发事件的 <see cref="CefBrowser"/>。</param>
        internal void OnRenderViewReadyCore(CefBrowser browser)
        {
            CreateBrowserMessageInterceptor();

            var args = new BrowserEventArgs(browser);

            InvokeOnUIThread(OnLoaded, args);
        }

        /// <summary>
        /// 浏览前回调,触发 <see cref="OnBeforeBrowse"/> 事件。
        /// </summary>
        /// <param name="browser">触发事件的 <see cref="CefBrowser"/>。</param>
        /// <param name="frame">触发事件的帧。</param>
        /// <param name="request">请求对象。</param>
        /// <param name="userGesture">是否由用户手势触发。</param>
        /// <param name="isRedirect">是否为重定向。</param>
        /// <returns>若已处理返回 true,否则 false。</returns>
        internal bool OnBeforeBrowseCore(CefBrowser browser, CefFrame frame, CefRequest request, bool userGesture, bool isRedirect)
        {
            var args = new BeforeBrowseEventArgs(browser, frame, request, userGesture, isRedirect);

            InvokeOnUIThread(OnBeforeBrowse, args);

            return args.Handled;
        }

        #endregion

        #region Members
        /// <summary>
        /// 记录进入全屏前的窗口边框样式,用于退出全屏时恢复。
        /// </summary>
        private FormBorderStyle? _lastFormBorderStyle;
        /// <summary>
        /// 记录进入全屏前的窗口状态,用于退出全屏时恢复。
        /// </summary>
        private FormWindowState? _lastFormWindowState;
        /// <summary>
        /// 应用标题,用于拼接窗口标题。
        /// </summary>
        private string? _appTitle;

        /// <summary>
        /// 系统菜单"关于"项的命令 ID。
        /// </summary>
        const int SYSMENU_ABOUT_ID = 0x9001;
        /// <summary>
        /// 系统菜单"全屏"项的命令 ID。
        /// </summary>
        const int SYSMENU_FULL_SCREEN_ID = 0x9002;

        //private CefBrowserSettings? _browserSettings;

        /// <summary>
        /// 内嵌的 WebView 实例。
        /// </summary>
        internal WebViewLifeSpan WebView { get; }

        /// <summary>
        /// 可拖拽区域,用于判定鼠标位置是否处于可拖拽区。
        /// </summary>
        public Region? DraggableRegion { get; set; }

        /// <summary>
        /// 启动画面控件,未启用时为 null。
        /// </summary>
        private SplashScreen? _splashScreen = null;

        /// <summary>
        /// 浏览器渲染控件消息拦截器,用于拦截浏览器窗口消息。
        /// </summary>
        private BrowserRenderWidgetHostMessageInterceptor? _browserRenderWidgetMessageInterceptor = null;

        /// <summary>
        /// 同步的 JavaScript 浏览器请求处理器,按消息名索引。
        /// </summary>
        private Dictionary<string, Func<JavaScriptValue, JavaScriptValue>> JavaScriptBrowserRequestHandlers { get; } = new();

        /// <summary>
        /// 异步的 JavaScript 浏览器请求处理器,按消息名索引。
        /// </summary>
        private Dictionary<string, Action<JavaScriptValue, JavaScriptPromise>> JavaScriptBrowserRequestAsyncHandlers { get; } = new();

        /// <summary>
        /// JavaScript 浏览器消息处理器,按消息名索引。
        /// </summary>
        private Dictionary<string, Action<JavaScriptValue>> JavaScriptBrowserMessageHandlers { get; } = new();

        /// <summary>
        /// 宿主窗口(无边框窗口),承载 WebView。
        /// </summary>
        internal BorderlessWindow? HostWindow { get; private set; }

        /// <summary>
        /// 控件是否启用。
        /// </summary>
        private bool _enabled = true;

        /// <summary>
        /// 创建宿主窗口:构建无边框窗口并配置其外观、尺寸、事件,最后创建窗口。
        /// </summary>
        internal void CreateHostWindowCore()
        {
            var target = new BorderlessWindow();

            HostWindow = target;

            target.OnWndProc = WndProcCore;
            target.OnDefWndProc = DefWndProcCore;

            target.IsHitTestEnabled = Sizable;

            target.ShowInTaskbar = ShowInTaskbar;

            target.Icon = Icon;

            AppTitle = DefaultAppTitle;

            target.Text = BuildTitleString();

            target.MinimumSize = InitialMinimumSize;

            target.MaximumSize = InitialMaximumSize;

            target.Size = InitialSize;

            target.MinimizeBox = Minimizable;

            target.MaximizeBox = Maximizable;

            if (target.Icon == null && !Sizable)
            {
                target.ControlBox = false;
            }

            target.Load += (sender, args) =>
            {
                var target = (Form)sender!;
                HostWindowCreatedCore(target);

                if (StartCentered)
                {
                    target.StartPosition = target.Owner == null ? FormStartPosition.CenterScreen : FormStartPosition.CenterParent;
                }
                else if (InitialLocation != null)
                {
                    target.StartPosition = FormStartPosition.Manual;

                    target.Location = InitialLocation.Value;
                }
            };

            target.Shown += (_, args) =>
            {
                OnActivatedCore();

                OnShown(args);
            };

            RegisterHostWindowEvents();

            CreateWindow(target);
        }

        /// <summary>
        /// 构建窗口标题:启用页面标题时按模板拼接页面标题与应用标题,否则仅返回应用标题。
        /// </summary>
        /// <returns>窗口标题字符串。</returns>
        private string BuildTitleString()
        {
            if (UsePageTitle)
            {
                if (string.IsNullOrEmpty(PageTitle))
                {
                    return AppTitle;
                }
                else
                {
                    return string.Format(TitlePattern, PageTitle, AppTitle);
                }
            }
            else
            {
                return AppTitle;
            }
        }


        /// <summary>
        /// 宿主窗口创建完成:记录窗口句柄、修改系统菜单、创建并显示启动画面,最后创建浏览器。
        /// </summary>
        /// <param name="target">已创建的宿主窗口。</param>
        internal void HostWindowCreatedCore(Form target)
        {

            WindowHandle = target.Handle;

            OwnerHandle = target.Owner?.Handle ?? IntPtr.Zero;

            System.Diagnostics.Debug.WriteLine("[LIFECYCLE] -> WindowCreated");

            ModifySystemMenu();

            _splashScreen = new SplashScreen(target, PaintSplashScreen);

            target.Controls.Add(_splashScreen);

            _splashScreen.Visible = EnableSplashScreen;

            ShowSplash();

            target.Activate();

            CreateBrowserCore();
        }

        /// <summary>
        /// 修改系统菜单:按需插入"全屏"项,并追加"关于"项。
        /// </summary>
        private void ModifySystemMenu()
        {


            var hSysMenu = GetSystemMenu(WindowHandle, false);

            if (AllowFullScreen)
            {

                InsertMenu(hSysMenu, (uint)5, MenuFlags.MF_BYPOSITION, (nint)SYSMENU_FULL_SCREEN_ID, "&FullScreen");
                //InsertMenu(hSysMenu, (uint)SysCommand.SC_CLOSE, MenuFlags.MF_BYCOMMAND | MenuFlags.MF_SEPARATOR, IntPtr.Zero, string.Empty);
            }

            if (!DisableAboutMenu)
            {
                AppendMenu(hSysMenu, MenuFlags.MF_SEPARATOR, IntPtr.Zero, string.Empty);
                AppendMenu(hSysMenu, MenuFlags.MF_STRING, (IntPtr)SYSMENU_ABOUT_ID, "&About ...");
            }
        }

        /// <summary>
        /// 配置浏览器设置:执行环境自定义配置后创建浏览器。
        /// </summary>
        /// <param name="settings">待配置与使用的浏览器设置。</param>
        internal void ConfigureBrowserSettingsCore(CefBrowserSettings settings)
        {

            Robot.App.Program.ConfigureBrowserSettings?.Invoke(settings);

            CreateBrowser(settings);
        }

        /// <summary>
        /// 创建浏览器:获取默认设置、设置背景色、配置设置,并加载初始地址。
        /// </summary>
        internal void CreateBrowserCore()
        {
            var settings = Robot.App.Program.GetDefaultBrowserSettings();

            settings.BackgroundColor = new CefColor(0xFF, 0xFF, 0xFF, 0xFF);

            ConfigureBrowserSettingsCore(settings);

            WebView.Url = Url;

            WebView.Create(settings);
        }

        /// <summary>
        /// 浏览器创建完成:调整 WebView 尺寸、启用状态,并注册宿主窗口的尺寸/移动/DPI/关闭等事件。
        /// </summary>
        /// <param name="browser">已创建的 <see cref="CefBrowser"/>。</param>
        internal void BrowserCreatedCore(CefBrowser browser)
        {
            ResizeWebView();

            OnEnabledChangedCore();

            var target = HostWindow!;


            target.Resize += (_, _) =>
            {
                ResizeWebView();
            };

            target.VisibleChanged += (_, _) =>
            {
                ResizeWebView();
            };

            target.ResizeBegin += (_, _) =>
            {
                WebView.NotifyMoveOrResizeStarted();
            };

            target.ResizeEnd += (_, _) =>
            {
                WebView.WasResized();
            };

            target.Move += (sender, _) =>
            {
                var target = (Form?)sender;
                if (target != null && target.WindowState == FormWindowState.Normal)
                {
                    WebView.NotifyMoveOrResizeStarted();
                }
            };

            target.WindowDpiAdapter.WindowDpiChanged += (_, _) =>
            {
                WebView.NotifyScreenInfoChanged();
                WebView.NotifyMoveOrResizeStarted();
            };

            target.FormClosing += (_, args) =>
            {
                if (args.CloseReason == CloseReason.WindowsShutDown || WebView.CanClose)
                {
                    return;
                }

                args.Cancel = true;

                BrowserHost?.CloseBrowser(false);
            };

            target.Activated += (_, _) =>
            {
                OnActivatedCore();
            };

            target.Deactivate += (_, _) =>
            {
                OnDeactivateCore();

            };

            System.Diagnostics.Debug.WriteLine("[LIFECYCLE] -> BrowserCreated");

            OnBrowserCreated(new BrowserEventArgs(browser));
        }

        /// <summary>
        /// 显示启动画面(若启用)。
        /// </summary>
        internal void ShowSplash()
        {
            if (EnableSplashScreen && _splashScreen != null)
            {
                InvokeOnUIThread(() =>
                {
                    _splashScreen.Visible = true;

                    _splashScreen.BringToFront();
                });
            }
        }

        /// <summary>
        /// 隐藏启动画面(若启用),窗口激活时恢复浏览器焦点并调整 WebView 尺寸。
        /// </summary>
        internal void HideSplash()
        {
            if (EnableSplashScreen && _splashScreen != null)
            {
                InvokeOnUIThread(() =>
                {

                    _splashScreen.SendToBack();
                    _splashScreen.Visible = false;


                    if (HostWindow != null && HostWindow.IsWindowActivated)
                    {
                        BrowserHost?.SetFocus(true);

                        ResizeWebView();

                    }
                });
            }
        }
        /// <summary>
        /// 绘制启动画面:填充背景色、居中绘制 Logo 与初始化文本。
        /// </summary>
        /// <param name="e">绘制事件。</param>
        protected virtual void PaintSplashScreen(PaintEventArgs e)
        {

            const string Initializing_Text = "Powered by Robot";

            var bounds = e.ClipRectangle;

            var g = e.Graphics;

            g.Clear(ColorTranslator.FromHtml("#004F99"));

            var img = Resources.Robot;

            var scale = bounds.Width / img.Width > 3 ? 1.0f : ((float)bounds.Width / 3) / (float)img.Width;

            if (scale > 1) scale = 1;

            var imgWidth = img.Width * scale;
            var imgHeight = img.Height * scale;

            g.DrawImage(img, new RectangleF((bounds.Width - imgWidth) / 2, (bounds.Height - imgHeight) / 2, imgWidth, imgHeight));



            var font = new Font("Segoue UI", 12 * (HostWindow?.WindowScaleFactor ?? 1.0f));

            var fontSize = g.MeasureString(Initializing_Text, font);

            g.DrawString(Initializing_Text, font, Brushes.White, new PointF((bounds.Width - fontSize.Width - 20), (bounds.Height - fontSize.Height - 20)));

        }

        /// <summary>
        /// 浏览器上下文创建:触发上下文创建事件,并在存在 IME 处理器时取消当前组合输入。
        /// </summary>
        /// <param name="browser">触发事件的 <see cref="CefBrowser"/>。</param>
        /// <param name="frame">触发事件的帧。</param>
        internal void ContextCreatedCore(CefBrowser browser, CefFrame frame)
        {

            ContextCreated(browser, frame);

            if (ImeHandler != null)
            {
                CancelImeComposition(browser.GetHost());
            }
        }


        /// <summary>
        /// 窗口激活:设置浏览器焦点,触发激活事件并通知 WebView。
        /// </summary>
        internal void OnActivatedCore()
        {
            SetBrowserFocus();


            OnActivated();

            WebView.InvokeOnActivated();


        }

        /// <summary>
        /// 设置浏览器焦点:无启动画面或启动画面隐藏时聚焦浏览器,否则将启动画面置于最前。
        /// </summary>
        private void SetBrowserFocus()
        {
            if (_splashScreen == null)
            {
                BrowserHost?.SetFocus(true);
            }
            else
            {
                if (_splashScreen.Visible == false)
                {
                    BrowserHost?.SetFocus(true);
                }
                else
                {
                    InvokeOnUIThread(() => _splashScreen.BringToFront());
                }
            }
        }

        /// <summary>
        /// 窗口状态变化:触发窗口状态变化事件并通知 WebView。
        /// </summary>
        internal void OnWindowStateChangedCore()
        {
            //OnWindowStateChanged(new WindowStateChangedEventArgs(WindowState));

            WebView.InvokeOnWindowStateChanged(WindowState.ToString());

        }

        /// <summary>
        /// 窗口失活:触发失活事件、取消浏览器焦点并通知 WebView。
        /// </summary>
        internal void OnDeactivateCore()
        {
            OnDeactivated();
            BrowserHost?.SetFocus(false);

            WebView.InvokeOnDeactivate();

        }
        /// <summary>
        /// 启用状态变化:按启用状态启用或禁用浏览器与宿主窗口。
        /// </summary>
        internal void OnEnabledChangedCore()
        {
            if (BrowserHandle != (nint)0 && WindowHandle != (nint)0)
            {
                EnableWindow(BrowserHandle, Enabled);
                EnableWindow(WindowHandle, Enabled);
            }
        }

        /// <summary>
        /// 关闭事件:触发关闭事件。
        /// </summary>
        /// <param name="args">关闭事件参数。</param>
        internal void OnClosingCore(ClosingEventArgs args)
        {
            OnClosing(args);
        }

        /// <summary>
        /// 已关闭事件:触发已关闭事件。
        /// </summary>
        internal void OnClosedCore()
        {
            OnClosed();
        }

        /// <summary>
        /// 创建浏览器渲染控件消息拦截器:成功后释放旧拦截器并替换为新实例。
        /// </summary>
        private async void CreateBrowserMessageInterceptor()
        {
            var retval = await BrowserRenderWidgetHostMessageInterceptor.Setup(_browserRenderWidgetMessageInterceptor, this, BrowserWndProcCore);

            if (retval != null)
            {
                if (_browserRenderWidgetMessageInterceptor != null)
                {
                    _browserRenderWidgetMessageInterceptor.ReleaseBrowserHandle();
                }

                _browserRenderWidgetMessageInterceptor = retval;
            }
        }

        /// <summary>
        /// 设置全屏状态:进入全屏时记录并保存当前边框与状态,退出时恢复。
        /// </summary>
        /// <param name="fullscreen">是否进入全屏。</param>
        /// <param name="state">退出全屏时恢复的窗口状态,为 null 时使用记录的状态。</param>
        private void SetFullscreenState(bool fullscreen, FormWindowState? state = null)
        {
            if (HostWindow == null || (!AllowFullScreen && fullscreen)) return;

            if (fullscreen)
            {
                _lastFormBorderStyle = HostWindow.FormBorderStyle;
                _lastFormWindowState = HostWindow.WindowState == FormWindowState.Minimized ? FormWindowState.Normal : HostWindow.WindowState;

                HostWindow.WindowState = FormWindowState.Normal;

                HostWindow.FormBorderStyle = FormBorderStyle.None;

                HostWindow.WindowState = FormWindowState.Maximized;

                IsFullscreen = true;
            }
            else
            {
                HostWindow.FormBorderStyle = _lastFormBorderStyle == null ? HostWindow.FormBorderStyle : _lastFormBorderStyle.Value;

                if (state == null)
                {
                    var formState = _lastFormWindowState == null ? FormWindowState.Normal : _lastFormWindowState.Value;

                    if (formState != HostWindow.WindowState) HostWindow.WindowState = formState;
                }
                else
                {
                    HostWindow.WindowState = state.Value;
                }

                _lastFormBorderStyle = null;
                _lastFormWindowState = null;

                IsFullscreen = false;
            }
        }

        /// <summary>
        /// 注册宿主窗口事件:尺寸变化、移动、可见性变化时通知 WebView 并触发对应事件。
        /// </summary>
        private void RegisterHostWindowEvents()
        {
            var hostWindow = HostWindow!;


            hostWindow.ResizeBegin += (_, args) => OnResizeBegin(args);
            hostWindow.Resize += (_, args) =>
            {
                OnResize(args);

                var isMaximized = (HostWindow?.WindowState == FormWindowState.Maximized);

                RECT rect;

                if (isMaximized)
                {
                    GetClientRect(WindowHandle, out rect);

                }
                else
                {
                    GetWindowRect(WindowHandle, out rect);
                }

                WebView.InvokeOnWindowResized(rect);

            };
            hostWindow.ResizeEnd += (_, args) => OnResizeEnd(args);

            hostWindow.Move += (_, args) =>
            {
                OnMove(args);

                GetClientRect(WindowHandle, out var rect);

                WebView.InvokeOnWindowMoved(rect.Left, rect.Top);
            };

            hostWindow.VisibleChanged += (_, args) => OnVisibleChanged(args);

        }

        /// <summary>
        /// 处理异常,当前为空实现。
        /// </summary>
        /// <param name="exception">待处理的异常。</param>
        internal void HandleException(Exception exception)
        {
        }



        /// <summary>
        /// 处理同步的 JavaScript 浏览器请求:按消息名查找并调用对应处理器。
        /// </summary>
        /// <param name="message">消息名。</param>
        /// <param name="value">请求参数。</param>
        /// <returns>处理器返回值,无对应处理器时返回 null。</returns>
        internal JavaScriptValue? OnBrowserRequest(string message, JavaScriptValue value)
        {
            if (JavaScriptBrowserRequestHandlers.TryGetValue(message, out var handler))
            {
                return handler.Invoke(value);
            }

            return null;
        }

        /// <summary>
        /// 处理异步的 JavaScript 浏览器请求:按消息名查找并调用对应处理器。
        /// </summary>
        /// <param name="message">消息名。</param>
        /// <param name="value">请求参数。</param>
        /// <param name="promise">用于返回结果的 Promise。</param>
        internal void OnBrowserRequestAsync(string message, JavaScriptValue value, JavaScriptPromise promise)
        {
            if (JavaScriptBrowserRequestAsyncHandlers.TryGetValue(message, out var handler))
            {
                handler.Invoke(value, promise);
            }
        }

        /// <summary>
        /// 处理 JavaScript 浏览器消息:按消息名查找并调用对应处理器。
        /// </summary>
        /// <param name="message">消息名。</param>
        /// <param name="value">消息参数。</param>
        internal void OnBrowserMessage(string message, JavaScriptValue value)
        {
            if (JavaScriptBrowserMessageHandlers.TryGetValue(message, out var handler))
            {
                handler.Invoke(value);
            }
        }

        #region host window message handlers
        /// <summary>
        /// 宿主窗口消息处理入口:浏览器未初始化时直接返回,否则依次处理上下文菜单、系统颜色模式、
        /// 系统菜单命令,最后交由 <see cref="WndProc"/> 处理。
        /// </summary>
        /// <param name="m">待处理的窗口消息(原地修改)。</param>
        /// <returns>若消息已处理返回 true,否则 false。</returns>
        internal bool WndProcCore(ref Message m)
        {
            if (!WebView.IsBrowserInitialized) return false;

            var msg = (WindowMessage)m.Msg;

            HandleContextMenuMessages(msg);

            var retval = HandleSystemMenuCommand(m);

            if (retval) return true;

            if (retval) return true;

            //if (msg == WindowMessage.WM_WINDOWPOSCHANGED)
            //{
            //    var windowpos = m.LParam.ToStructure<WINDOWPOS>();

            //    if ((windowpos.flags & SetWindowPosFlags.SWP_NOSIZE) != SetWindowPosFlags.SWP_NOSIZE)
            //    {

            //        System.Diagnostics.Debug.WriteLine("[WIN32API] -> WM_WINDOWPOSCHANGED");
            //        ResizeWebView();
            //    }
            //}

            return WndProc(ref m);
        }

        /// <summary>
        /// 处理系统菜单命令:全屏切换、还原退出全屏,以及非客户区右键弹出系统菜单。
        /// </summary>
        /// <param name="m">窗口消息。</param>
        /// <returns>若消息已处理返回 true,否则 false。</returns>
        private bool HandleSystemMenuCommand(Message m)
        {
            var msg = (WindowMessage)m.Msg;

            if (msg == WindowMessage.WM_SYSCOMMAND)
            {
                var cmd = (int)m.WParam;

                if (cmd == (int)SysCommand.SC_RESTORE && IsFullscreen)
                {
                    SetFullscreenState(false);

                    return true;
                }

                if (cmd == SYSMENU_FULL_SCREEN_ID)
                {
                    SetFullscreenState(!IsFullscreen);
                }
            }

            if (msg == WindowMessage.WM_NCRBUTTONUP)
            {
                var point = new Point(Macros.GET_X_LPARAM(m.LParam), Macros.GET_Y_LPARAM(m.LParam));
                ShowSystemMenu(ref point);
            }

            return false;
        }

        /// <summary>
        /// 在指定位置弹出系统菜单,并将选中的命令 ID 作为系统命令消息发送。
        /// </summary>
        /// <param name="pt">弹出位置(屏幕坐标)。</param>
        private void ShowSystemMenu(ref Point pt)
        {
            var hMenu = GetSystemMenu(WindowHandle, false);
            var hCmd = TrackPopupMenuEx(hMenu, TrackPopupMenuFlags.TPM_RETURNCMD | TrackPopupMenuFlags.TPM_TOPALIGN | TrackPopupMenuFlags.TPM_LEFTALIGN, pt.X, pt.Y, WindowHandle);

            PostMessage(WindowHandle, (uint)WindowMessage.WM_SYSCOMMAND, (IntPtr)hCmd, IntPtr.Zero);
        }

        /// <summary>
        /// 默认窗口消息处理入口,交由 <see cref="DefWndProc"/> 处理。
        /// </summary>
        /// <param name="m">待处理的窗口消息(原地修改)。</param>
        /// <returns>若消息已处理返回 true,否则 false。</returns>
        internal bool DefWndProcCore(ref Message m)
        {
            var msg = (WindowMessage)m.Msg;

            return DefWndProc(ref m);
        }

        /// <summary>
        /// 处理上下文菜单相关消息:非客户区左键或右键按下时关闭上下文菜单。
        /// </summary>
        /// <param name="msg">窗口消息。</param>
        private void HandleContextMenuMessages(WindowMessage msg)
        {
            if (msg == WindowMessage.WM_NCLBUTTONDOWN || msg == WindowMessage.WM_NCRBUTTONDOWN)
            {
                WebView.CloseContextMenu();
            }
        }
        #endregion

        #region browser window message handlers
        /// <summary>
        /// 浏览器窗口消息处理入口:先尝试 <see cref="BrowserMessageHandler"/>,未处理则交由 <see cref="BrowserWndProc"/>。
        /// </summary>
        /// <param name="m">待处理的窗口消息(原地修改)。</param>
        /// <returns>若消息已处理返回 true,否则 false。</returns>
        internal bool BrowserWndProcCore(ref Message m)
        {
            var retval = BrowserMessageHandler(ref m);

            if (retval) return true;

            return BrowserWndProc(ref m);
        }

        /// <summary>
        /// 触摸消息 ID(WM_TOUCH)。
        /// </summary>
        private const int WM_TOUCH = 0x0240;

        /// <summary>
        /// 处理浏览器窗口消息:分发鼠标移动、光标设置、左键/右键按下与抬起、左键双击等消息。
        /// </summary>
        /// <param name="m">待处理的窗口消息(原地修改)。</param>
        /// <returns>若消息已处理返回 true,否则 false。</returns>
        private bool BrowserMessageHandler(ref Message m)
        {
            var msg = (WindowMessage)m.Msg;

            switch (msg)
            {
                case WindowMessage.WM_MOUSEMOVE when UseBrowserHitTest:
                    return BrowserWmMouseMove(ref m);
                case WindowMessage.WM_SETCURSOR when UseBrowserHitTest:
                    return BrowserWmSetCursor(ref m);
                case WindowMessage.WM_LBUTTONDOWN:
                    return BrowserLButtonDown(ref m);
                case WindowMessage.WM_RBUTTONDOWN:
                    return BrowserRButtonHandler(true, ref m);
                case WindowMessage.WM_RBUTTONUP:
                    return BrowserRButtonHandler(false, ref m);
                case WindowMessage.WM_LBUTTONDBLCLK:
                    return BrowserLButtonDoubleClick(ref m);

            }


            //if(m.Msg == WM_TOUCH)
            //{
            //    return DecodeTouch(ref m);
            //}

            return false;
        }

        //private bool DecodeTouch(ref Message m)
        //{
        //    var inputCount = (uint)Macros.LOWORD(m.WParam);

        //    var inputs = new TOUCHINPUT[inputCount];

        //    var cbSize= Marshal.SizeOf<TOUCHINPUT>();

        //    if(!GetTouchInputInfo(m.LParam, inputCount, inputs, cbSize))
        //    {
        //        return false;
        //    }


        //    if(inputCount == 1)
        //    {
        //        var ti = inputs[inputCount];

        //        var isDown = ((ti.dwFlags & TOUCHEVENTF.TOUCHEVENTF_DOWN) == TOUCHEVENTF.TOUCHEVENTF_DOWN);

        //        if (isDown)
        //        {
        //            var point = new POINT(ti.x / 100, ti.y / 100);

        //            ScreenToClient(WindowHandle, ref point);

        //            var isInDraggableArea = DraggableRegion?.IsVisible(point) ?? false;

        //            if (isInDraggableArea)
        //            {
        //                ReleaseCapture();

        //                PostMessage(WindowHandle, (uint)WindowMessage.WM_NCLBUTTONDOWN, (IntPtr)HitTestValues.HTCAPTION, Macros.MAKELPARAM((ushort)point.X, (ushort)point.Y));

        //                return true;
        //            }

        //        }
        //    }


        //    CloseTouchInputHandle(m.LParam);



        //    return false;
        //}

        /// <summary>
        /// 处理浏览器窗口左键双击:在可拖拽区域且窗口可最大化/可缩放时,发送非客户区双击消息以切换最大化。
        /// </summary>
        /// <param name="m">窗口消息。</param>
        /// <returns>若消息已处理返回 true,否则 false。</returns>
        private bool BrowserLButtonDoubleClick(ref Message m)
        {
            var point = new Point(Macros.GET_X_LPARAM(m.LParam), Macros.GET_Y_LPARAM(m.LParam));

            var isInDraggableArea = DraggableRegion?.IsVisible(point) ?? false;

            if (isInDraggableArea && Maximizable && Sizable)
            {
                if (IsFullscreen)
                {
                    return false;
                }

                PostMessage(WindowHandle, (uint)WindowMessage.WM_NCLBUTTONDBLCLK, (IntPtr)HitTestValues.HTCAPTION, IntPtr.Zero);

                return true;
            }

            return false;

        }

        /// <summary>
        /// 处理浏览器窗口右键按下/抬起:在可拖拽区域且允许系统菜单时,发送非客户区右键消息以弹出系统菜单。
        /// </summary>
        /// <param name="isDown">是否为按下。</param>
        /// <param name="m">窗口消息。</param>
        /// <returns>若消息已处理返回 true,否则 false。</returns>
        private bool BrowserRButtonHandler(bool isDown, ref Message m)
        {
            if (!AllowSystemMenu)
            {
                return false;
            }



            var point = new POINT(Macros.GET_X_LPARAM(m.LParam), Macros.GET_Y_LPARAM(m.LParam));

            var isInDraggableArea = DraggableRegion?.IsVisible(point) ?? false;

            if (isInDraggableArea)
            {
                ClientToScreen(WindowHandle, ref point);

                if (isDown)
                {
                    PostMessage(WindowHandle, (uint)WindowMessage.WM_NCRBUTTONDOWN, (IntPtr)HitTestValues.HTSYSMENU, Macros.MAKELPARAM((ushort)point.X, (ushort)point.Y));
                }
                else
                {
                    PostMessage(WindowHandle, (uint)WindowMessage.WM_NCRBUTTONUP, (IntPtr)HitTestValues.HTSYSMENU, Macros.MAKELPARAM((ushort)point.X, (ushort)point.Y));
                }


                return true;
            }

            return false;
        }

        /// <summary>
        /// 处理浏览器窗口左键按下:在可缩放区域或可拖拽区域时,发送非客户区左键按下消息以启动窗口拖动/缩放。
        /// </summary>
        /// <param name="m">窗口消息。</param>
        /// <returns>若消息已处理返回 true,否则 false。</returns>
        private bool BrowserLButtonDown(ref Message m)
        {
            var point = new POINT(Macros.GET_X_LPARAM(m.LParam), Macros.GET_Y_LPARAM(m.LParam));

            var isInDraggableArea = DraggableRegion?.IsVisible(point) ?? false;

            var mode = HostWindow!.HitTest(point);

            if (mode == HitTestValues.HTNOWHERE) return false;

            ClientToScreen(WindowHandle, ref point);

            var lparam = Macros.MAKELPARAM((uint)point.X, (uint)point.Y);

            if (Sizable && UseBrowserHitTest && mode != HitTestValues.HTCLIENT && HostWindow!.WindowState == FormWindowState.Normal)
            {
                ReleaseCapture();

                PostMessage(WindowHandle, (uint)WindowMessage.WM_NCLBUTTONDOWN, (IntPtr)mode, lparam);

                return true;
            }
            else if (isInDraggableArea)
            {
                ReleaseCapture();

                PostMessage(WindowHandle, (uint)WindowMessage.WM_NCLBUTTONDOWN, (IntPtr)HitTestValues.HTCAPTION, lparam);

                return true;
            }

            return false;
        }

        /// <summary>
        /// 处理浏览器窗口光标设置:在窗口边框/角落命中时设置对应的调整光标。
        /// </summary>
        /// <param name="m">窗口消息(命中非客户区时设置结果并返回 true)。</param>
        /// <returns>若消息已处理返回 true,否则 false。</returns>
        private bool BrowserWmSetCursor(ref Message m)
        {
            #region SETCURSOR
            /// <summary>
            /// 根据命中区域加载并设置对应的调整光标。
            /// </summary>
            /// <param name="mode">命中区域。</param>
            void SetCursor(HitTestValues mode)
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
                    var oldCursor = User32.SetCursor(handle);

                    oldCursor?.Close();
                }
            }
            #endregion

            if (HostWindow?.WindowState != FormWindowState.Normal) return false;

            if (!Sizable) return false;


            var pos = GetMessagePos();
            var point = new POINT(Macros.LOWORD(pos), Macros.HIWORD(pos));
            ScreenToClient(WindowHandle, ref point);

            var retval = HostWindow!.HitTest(point);

            if (retval != HitTestValues.HTCLIENT)
            {
                SetCursor(retval);

                m.Result = (IntPtr)1;

                return true;
            }

            return false;
        }

        /// <summary>
        /// 处理浏览器窗口鼠标移动:在可缩放且启用浏览器命中测试时,判断是否命中非客户区。
        /// </summary>
        /// <param name="m">窗口消息。</param>
        /// <returns>若命中非客户区(非 HTNOWHERE 且非 HTCLIENT)返回 true,否则 false。</returns>
        private bool BrowserWmMouseMove(ref Message m)
        {
            if (HostWindow?.WindowState != FormWindowState.Normal) return false;

            if (!Sizable) return false;

            if (!UseBrowserHitTest) return false;


            var lparam = m.LParam;

            var point = new Point(Macros.GET_X_LPARAM(lparam), Macros.GET_Y_LPARAM(lparam));

            var retval = HostWindow!.HitTest(point);

            return retval != HitTestValues.HTNOWHERE && retval != HitTestValues.HTCLIENT;
        }
        #endregion
        #endregion

        #region Overrides
        /// <summary>
        /// 获取表单绑定的窗口句柄。
        /// </summary>
        internal protected IntPtr WindowHandle { get; private set; }

        /// <summary>
        /// 获取浏览器窗口句柄。
        /// </summary>
        internal protected IntPtr BrowserHandle { get; private set; }

        /// <summary>
        /// 获取表单所有者的窗口句柄。
        /// </summary>
        internal protected IntPtr? OwnerHandle { get; private set; }

        /// <summary>
        /// 获取 <see cref="CefBrowser"/> 实例。
        /// </summary>
        internal protected CefBrowser? Browser => WebView.Browser;

        /// <summary>
        /// 获取 <see cref="CefBrowserHost"/> 实例。
        /// </summary>
        internal protected CefBrowserHost? BrowserHost => WebView.BrowserHost;

        /// <summary>
        /// 是否禁用系统菜单中的"关于"项,默认为 false。
        /// </summary>
        protected virtual bool DisableAboutMenu => false;

        /// <summary>
        /// 获取或设置要加载的页面地址。
        /// </summary>
        public string Url
        {
            get => WebView.Url;
            set => WebView.Url = value;
        }

        /// <summary>
        /// 调整 WebView 尺寸以适配当前窗口尺寸,并按窗口状态通知 WebView 可见性变化。
        /// </summary>
        internal protected void ResizeWebView()
        {
            if (HostWindow == null || WindowHandle == (nint)0) return;


            if (HostWindow.WindowState != FormWindowState.Minimized)
            {
                GetClientRect(WindowHandle, out var rect);
                WebView.ResizeWebView(rect.Width, rect.Height);
            }

            if (WindowState == RobotFormWindowState.Minimized || Visible == false)
            {
                WebView.WasHidden(true);
            }
            else
            {
                WebView.WasHidden(false);
            }
        }

        /// <summary>
        /// 处理窗口消息。
        /// </summary>
        /// <param name="m">待处理的窗口 <see cref="Message"/>。</param>
        /// <returns>
        /// 若消息已处理返回 true,否则 false。
        /// </returns>
        protected virtual bool WndProc(ref Message m)
        {
            return false;
        }

        /// <summary>
        /// 处理由 DefWindowMessage 调用的消息。
        /// </summary>
        /// <param name="m">
        /// 待处理的窗口 <see cref="Message"/>。
        /// </param>
        /// <returns>
        /// 若消息已处理返回 true,否则 false。
        /// </returns>
        protected virtual bool DefWndProc(ref Message m)
        {
            return false;
        }

        /// <summary>
        /// 处理浏览器窗口消息。
        /// </summary>
        /// <param name="m">
        /// 待处理的窗口 <see cref="Message"/>。
        /// </param>
        /// <returns>若消息已处理返回 true,否则 false。</returns>
        protected virtual bool BrowserWndProc(ref Message m)
        {
            return false;
        }

        /// <summary>
        /// 在浏览器实例创建前调用,允许自定义浏览器设置。
        /// </summary>
        /// <param name="settings">
        /// 可用于配置浏览器的 <see cref="CefBrowserSettings"/> 实例。
        /// </param>
        protected virtual void CreateBrowser(CefBrowserSettings settings)
        {
        }

        /// <summary>
        /// 在宿主窗口实例创建时调用。
        /// </summary>
        /// <param name="form">
        /// 宿主窗口实例。
        /// </param>
        protected virtual void CreateWindow(Form form)
        {
        }


        /// <summary>
        /// 在浏览器实例的上下文创建时调用。
        /// </summary>
        /// <param name="browser">
        /// 浏览器实例。
        /// </param>
        /// <param name="frame">
        /// 拥有该上下文的帧。
        /// </param>
        protected virtual void ContextCreated(CefBrowser browser, CefFrame frame)
        {
        }

        /// <summary>
        /// 在 UI 线程上、创建新的弹出浏览器之前调用。
        /// <see cref="CefBrowser"/> 与 <see cref="CefFrame"/> 表示弹出请求的来源;
        /// <paramref name="targetUrl"/> 与 <paramref name="targetFrameName"/> 指示弹出浏览器应导航到的位置,
        /// 若请求未指定则可能为空;<paramref name="targetDisposition"/> 指示用户意图打开的位置(如当前标签、新标签等);
        /// <paramref name="userGesture"/> 在弹出由显式用户手势(如点击链接)打开时为 true,自动打开(如 DomContentLoaded 事件)时为 false;
        /// <paramref name="popupFeatures"/> 结构包含关于所请求弹出窗口的附加信息。
        /// 允许创建弹出浏览器时,可选择性修改 <paramref name="windowInfo"/>、<paramref name="client"/>、<paramref name="settings"/> 与
        /// <paramref name="noJavascriptAccess"/> 并返回 false;取消创建则返回 true。
        /// <paramref name="client"/> 与 <paramref name="settings"/> 默认为源浏览器的值;
        /// 若 <paramref name="noJavascriptAccess"/> 设为 false,新浏览器将不可脚本化,且可能不与源浏览器处于同一渲染进程。
        /// 若父浏览器被 CefBrowserView 包装,对 <paramref name="windowInfo"/> 的任何修改都会被忽略。
        /// 若父浏览器在弹出浏览器创建完成前被销毁(以弹出浏览器的 OnAfterCreated 调用为标志),弹出浏览器创建将被取消。
        /// <paramref name="extraInfo"/> 参数提供机会指定将传递给渲染进程 CefRenderProcessHandler::OnBrowserCreated() 的、
        /// 针对所创建弹出浏览器的附加信息。
        /// </summary>
        /// <param name="browser">
        /// 发起该弹出的浏览器实例。
        /// </param>
        /// <param name="frame">
        /// 发起该弹出的 HTML 帧。
        /// </param>
        /// <param name="targetUrl">
        /// 弹出内容的地址。
        /// </param>
        /// <param name="targetFrameName">
        /// 目标帧的名称。若目标不是命名帧,该值为空。
        /// </param>
        /// <param name="targetDisposition">
        /// 指示用户意图基于标准 Chromium 行为导航浏览器的位置(如当前标签、新标签等)。
        /// </param>
        /// <param name="userGesture">
        /// 若浏览器通过显式用户手势(如点击链接)导航则为 true,自动导航(如 DomContentLoaded 事件)则为 false。
        /// </param>
        /// <param name="popupFeatures">
        /// 包含关于所请求弹出窗口的附加信息的结构。
        /// </param>
        /// <param name="windowInfo">
        /// 窗口信息。
        /// </param>
        /// <param name="client">
        /// 设为新浏览器窗口的客户端。若留空,地址将在当前浏览器窗口中打开。
        /// </param>
        /// <param name="settings">
        /// 浏览器设置,默认为源浏览器的设置。
        /// </param>
        /// <param name="extraInfo">
        /// 设为将传递给新弹出的附加信息。
        /// </param>
        /// <param name="noJavascriptAccess">
        /// 指示新浏览器窗口是否应可脚本化且与源浏览器处于同一进程。
        /// </param>
        /// <returns>
        /// 返回 true 取消导航,返回 false 允许导航继续。
        /// </returns>
        protected virtual bool BeforePopup(CefBrowser browser, CefFrame frame, string targetUrl, string targetFrameName, CefWindowOpenDisposition targetDisposition, bool userGesture, CefPopupFeatures popupFeatures, CefWindowInfo windowInfo, ref CefClient client, CefBrowserSettings settings, ref CefDictionaryValue extraInfo, ref bool noJavascriptAccess)
        {
            return false;
        }

        /// <summary>
        /// 为指定消息注册同步请求处理器。
        /// </summary>
        /// <param name="message">
        /// 消息名。
        /// </param>
        /// <param name="handler">
        /// 请求处理器。应从 <see cref="JavaScriptValue"/> 立即返回一个值。
        /// </param>
        protected void RegisterJavaScriptRequestHandler(string message, Func<JavaScriptValue, JavaScriptValue> handler)
        {
            JavaScriptBrowserRequestHandlers[message] = handler;
        }

        /// <summary>
        /// 为指定消息注册异步请求处理器。
        /// </summary>
        /// <param name="message">
        /// 消息名。
        /// </param>
        /// <param name="handler">
        /// 请求处理器。可使用 <see cref="JavaScriptPromise"/> 稍后解析或拒绝该请求。
        /// </param>
        protected void RegisterJavaScriptRequestHandler(string message, Action<JavaScriptValue, JavaScriptPromise> handler)
        {
            JavaScriptBrowserRequestAsyncHandlers[message] = handler;
        }

        /// <summary>
        /// 为指定消息注册消息处理器。
        /// </summary>
        /// <param name="message">
        /// 消息名。
        /// </param>
        /// <param name="handler">
        /// 消息处理器。<see cref="JavaScriptValue"/> 为前端环境传入的数据。
        /// </param>
        protected void RegisterJavaScriptMessagHandler(string message, Action<JavaScriptValue> handler)
        {
            JavaScriptBrowserMessageHandlers[message] = handler;
        }

        /// <summary>
        /// 移除指定消息的所有消息处理器。
        /// </summary>
        /// <param name="message">
        /// 消息名。
        /// </param>
        protected void RegisterJavaScriptMessagHandler(string message)
        {
            JavaScriptBrowserMessageHandlers.Remove(message);
        }


        #endregion
    }
}
