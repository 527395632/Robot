// Robot 桌面软件 — WebView 主类
// 承载 CEF 浏览器的核心类,管理浏览器生命周期、窗口尺寸与 JavaScript 调用

using Robot.Browser.ContextMenu;
using Robot.Browser.DevTools;
using Robot.Browser.EmbeddedBrowser;
using Robot.JavaScript;
using System;
using System.Diagnostics;
using System.Security.Policy;
using System.Threading.Tasks;
using Vanara.PInvoke;
using Xilium.CefGlue;
using static Vanara.PInvoke.User32;

namespace Robot.Browser
{
    /// <summary>
    /// WebView:承载 CEF 浏览器的核心类,管理浏览器生命周期、窗口尺寸与 JavaScript 调用。
    /// </summary>
    internal partial class WebViewLifeSpan : CefLifeSpanHandler
    {

        /// <summary>
        /// 初始化 <see cref="WebViewLifeSpan"/> 实例。
        /// </summary>
        /// <param name="host">WebView 宿主。</param>
        public WebViewLifeSpan(RobotWindow host)
        {
            WebViewHost = host;

            BrowserWindowInfo = CefWindowInfo.Create();

            BrowserClient = new WebViewClient(this);

        }


        #region Base
        /// <summary>
        /// 浏览器客户端。
        /// </summary>
        public WebViewClient BrowserClient { get; private set; }

        /// <summary>
        /// 浏览器窗口信息。
        /// </summary>
        public CefWindowInfo BrowserWindowInfo { get; private set; }

        /// <summary>
        /// 浏览器窗口句柄。
        /// </summary>
        public IntPtr BrowserHandle { get; private set; }

        /// <summary>
        /// 宿主窗口句柄。
        /// </summary>
        public IntPtr WindowHandle { get => WebViewHost.WindowHandle; }

        /// <summary>
        /// WebView 宿主。
        /// </summary>
        public RobotWindow WebViewHost { get; }

        /// <summary>
        /// 是否可关闭。
        /// </summary>
        public bool CanClose { get; private set; } = false;

        /// <summary>
        /// 浏览器是否已初始化。
        /// </summary>
        public bool IsBrowserInitialized { get; private set; } = false;

        /// <summary>
        /// 浏览器实例。
        /// </summary>
        public CefBrowser? Browser { get; private set; }

        /// <summary>
        /// 浏览器宿主。
        /// </summary>
        public CefBrowserHost? BrowserHost => Browser?.GetHost();

        /// <summary>
        /// 是否深色模式,变化时通知前端。
        /// </summary>
        public bool IsDark
        {
            get => _isDark;
            set
            {
                if (value != _isDark)
                {
                    _isDark = value;

                    ColorModeChange();
                }
            }
        }

        /// <summary>
        /// 当前地址,设置时加载。
        /// </summary>
        public string Url
        {
            get => Browser?.GetMainFrame()?.Url ?? _url;
            set
            {
                _url = $"{value}".Trim();




                TaskAction.Run(() =>
                {
                    if (IsBrowserInitialized)
                    {
                        Browser?.GetMainFrame()?.LoadUrl(_url);
                    }
                });
            }
        }

        /// <summary>
        /// 创建浏览器:配置子窗口信息。
        /// </summary>
        /// <param name="settings">浏览器设置。</param>
        public void Create(CefBrowserSettings settings)
        {
            BrowserWindowInfo.StyleEx |= Xilium.CefGlue.Platform.Windows.WindowStyleEx.WS_EX_NOACTIVATE;

            GetClientRect(WindowHandle, out var rect);

            BrowserWindowInfo.SetAsChild(WindowHandle!, new CefRectangle(0, 0, rect.Width, rect.Height));

            CefBrowserHost.CreateBrowser(BrowserWindowInfo, BrowserClient, settings, Url);
        }

        /// <summary>
        /// 通知浏览器移动或缩放开始。
        /// </summary>
        public void NotifyMoveOrResizeStarted()
        {
            BrowserHost?.NotifyMoveOrResizeStarted();
        }

        /// <summary>
        /// 通知浏览器已缩放。
        /// </summary>
        public void WasResized()
        {
            BrowserHost?.WasResized();
        }

        /// <summary>
        /// 通知浏览器屏幕信息变化。
        /// </summary>
        public void NotifyScreenInfoChanged()
        {
            BrowserHost?.NotifyScreenInfoChanged();
        }

        /// <summary>
        /// 通知浏览器显示状态变化。
        /// </summary>
        /// <param name="hidden">是否隐藏。</param>
        public void WasHidden(bool hidden)
        {
            BrowserHost?.WasHidden(hidden);
        }

        /// <summary>
        /// 调整 WebView 尺寸(切换到 UI 线程执行)。
        /// </summary>
        /// <param name="width">宽度。</param>
        /// <param name="height">高度。</param>
        public void ResizeWebView(int width, int height)
        {
            WebViewHost.InvokeOnUIThread(() =>
            {
                ResizeWebViewCore(width, height);
            });
        }

        /// <summary>
        /// 调整 WebView 尺寸核心逻辑:根据窗口状态设置浏览器窗口位置与样式。
        /// </summary>
        /// <param name="width">宽度。</param>
        /// <param name="height">高度。</param>
        public void ResizeWebViewCore(int width, int height)
        {
            if (Browser != null && BrowserHandle == IntPtr.Zero)
            {
                WasResized();

                return;
            }

            if (Browser == null || BrowserHandle == IntPtr.Zero /*|| !IsBrowserInitialized*/) return;

            if (IsIconic(WindowHandle))
            {
                SetWindowPos(BrowserHandle, HWND.NULL, 0, 0, width, height, SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOOWNERZORDER | SetWindowPosFlags.SWP_NOACTIVATE);
            }
            else
            {
                if (WebViewHost.Visible)
                {
                    SetWindowPos(BrowserHandle, HWND.NULL, 0, 0, width, height, SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_SHOWWINDOW);

                    SetWindowLong(BrowserHandle, WindowLongFlags.GWL_STYLE, (IntPtr)(WindowStyles.WS_CHILD | WindowStyles.WS_CLIPCHILDREN | WindowStyles.WS_CLIPSIBLINGS | WindowStyles.WS_TABSTOP | WindowStyles.WS_VISIBLE));
                }
                else
                {
                    SetWindowPos(BrowserHandle, HWND.NULL, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_HIDEWINDOW);

                    SetWindowLong(BrowserHandle, WindowLongFlags.GWL_STYLE, (IntPtr)(WindowStyles.WS_CHILD | WindowStyles.WS_CLIPCHILDREN | WindowStyles.WS_CLIPSIBLINGS | WindowStyles.WS_TABSTOP | WindowStyles.WS_DISABLED));
                }
            }

            WasResized();

        }

        /// <summary>
        /// 释放浏览器资源。
        /// </summary>
        public void Dispose()
        {
            Browser?.Dispose();
        }

        /// <summary>
        /// 显示 DevTools(切换到 UI 线程执行)。
        /// </summary>
        public void ShowDevTools()
        {
            WebViewHost.InvokeOnUIThread(ShowDevToolsCore);
        }

        /// <summary>
        /// 隐藏 DevTools(切换到 UI 线程执行)。
        /// </summary>
        public void HideDevTools()
        {
            WebViewHost.InvokeOnUIThread(HideDevToolsCore);
        }


        /// <summary>
        /// 在主帧执行 JavaScript。
        /// </summary>
        /// <param name="code">JavaScript 代码。</param>
        /// <param name="url">来源地址。</param>
        /// <param name="line">行号。</param>
        public void ExecuteJavaScript(string code, string url = "", int line = 0)
        {
            var frame = Browser?.GetMainFrame();

            if (frame == null) return;

            ExecuteJavaScript(frame, code, url, line);
        }

        /// <summary>
        /// 在指定帧执行 JavaScript。
        /// </summary>
        /// <param name="frame">帧。</param>
        /// <param name="code">JavaScript 代码。</param>
        /// <param name="url">来源地址。</param>
        /// <param name="line">行号。</param>
        public void ExecuteJavaScript(CefFrame frame, string code, string url = "", int line = 0)
        {
            frame.ExecuteJavaScript(code, url, line);
        }

        /// <summary>
        /// 在主帧异步求值 JavaScript。
        /// </summary>
        /// <param name="code">JavaScript 代码。</param>
        /// <param name="url">来源地址。</param>
        /// <param name="line">行号。</param>
        /// <returns>求值结果。</returns>
        public Task<JavaScriptResult> EvaluateJavaScriptAsync(string code, string url = "about:blank", int line = 0)
        {
            if (Browser == null) throw new NullReferenceException("Browser is null.");

            return EvaluateJavaScriptAsync(Browser.GetMainFrame(), code, url, line);
        }

        /// <summary>
        /// 在指定帧异步求值 JavaScript。
        /// </summary>
        /// <param name="frame">帧。</param>
        /// <param name="code">JavaScript 代码。</param>
        /// <param name="url">来源地址。</param>
        /// <param name="line">行号。</param>
        /// <returns>求值结果。</returns>
        public Task<JavaScriptResult> EvaluateJavaScriptAsync(CefFrame frame, string code, string url = "about:blank", int line = 0)
        {
            if (JavaScriptEngine == null) throw new Exception($"{nameof(JavaScriptEngine)} is not ready at this moment.");

            return JavaScriptEngine.EvaluateJavaScriptAsync(frame, code, url, line);
        }

        /// <summary>
        /// 开始注册 JavaScript 对象。
        /// </summary>
        /// <param name="frame">帧。</param>
        /// <returns>注册句柄。</returns>
        public JavaScriptObjectRegisterHandle BeginRegisterJavaScriptObject(CefFrame frame)
        {
            if (JavaScriptObjectMapping == null) throw new Exception($"{nameof(JavaScriptObjectMapping)} is not ready at this moment.");

            return JavaScriptObjectMapping.BeginRegisterJavaScriptObject(frame);
        }

        /// <summary>
        /// 结束注册 JavaScript 对象。
        /// </summary>
        /// <param name="handle">注册句柄。</param>
        public void EndRegisterJavaScriptObject(JavaScriptObjectRegisterHandle handle)
        {
            if (JavaScriptObjectMapping == null) throw new Exception($"{nameof(JavaScriptObjectMapping)} is not ready at this moment.");

            JavaScriptObjectMapping.EndRegisterJavaScriptObject(handle);
        }

        /// <summary>
        /// 注册 JavaScript 对象。
        /// </summary>
        /// <param name="handle">注册句柄。</param>
        /// <param name="name">对象名称。</param>
        /// <param name="jsObject">JavaScript 对象。</param>
        /// <returns>是否成功。</returns>
        public bool RegisterJavaScriptObject(JavaScriptObjectRegisterHandle handle, string name, JavaScriptObject jsObject)
        {
            if (JavaScriptObjectMapping == null) throw new Exception($"{nameof(JavaScriptObjectMapping)} is not ready at this moment.");

            return JavaScriptObjectMapping.RegisterJavaScriptObject(handle, name, jsObject);
        }


        /// <summary>
        /// 注册 JavaScript 对象(宿主对象包装)。
        /// </summary>
        /// <param name="handle">注册句柄。</param>
        /// <param name="name">对象名称。</param>
        /// <param name="jsHostObject">宿主对象包装。</param>
        /// <returns>是否成功。</returns>
        public bool RegisterJavaScriptObject(JavaScriptObjectRegisterHandle handle, string name, JavaScriptObjectWrapper jsHostObject)
        {
            if (JavaScriptObjectMapping == null) throw new Exception($"{nameof(JavaScriptObjectMapping)} is not ready at this moment.");

            return RegisterJavaScriptObject(handle, name, jsHostObject.HostObject);
        } 
        #endregion

        #region IContextMenuHandler
        /// <summary>
        /// 关闭右键菜单(跨线程安全)。
        /// </summary>
        public void CloseContextMenu()
        {
            if (_contextMenu != null)
            {
                if (_contextMenu.InvokeRequired)
                {
                    _contextMenu?.Invoke(new System.Windows.Forms.MethodInvoker(() => _contextMenu.Close(ToolStripDropDownCloseReason.AppClicked)));
                }
                else
                {
                    _contextMenu.Close(ToolStripDropDownCloseReason.AppClicked);
                }
            }
        }

        #region implements

        /// <summary>
        /// 当前右键菜单实例。
        /// </summary>
        private AnimatedContextMenuStrip? _contextMenu;

        /// <summary>
        /// 右键菜单构建前回调:移除非编辑项,插入 DevTools 项。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="state">菜单参数。</param>
        /// <param name="model">菜单模型。</param>
        internal void OnBeforeContextMenuCore(CefBrowser browser, CefFrame frame, CefContextMenuParams state, CefMenuModel model)
        {
            Debug.WriteLine($"[DBG-CTX] OnBeforeContextMenuCore count={model.Count}");

            List<int> removeCmds = new();

            for (var i = 0; i < (int)model.Count; i++)
            {
                var nCmd = model.GetCommandIdAt((nuint)i);


                if (!ContextMenuHelper.IsEditingItem(nCmd) && !ContextMenuHelper.IsUserDefinedItem(nCmd))
                {
                    removeCmds.Add(nCmd);
                }
            }

            foreach (var cmdId in removeCmds)
            {
                model.Remove(cmdId);
            }

            if (Robot.App.Program.EnableDevTools)
            {

                if (model.Count > 0)
                {
                    model.InsertSeparatorAt(0);
                }

                if (BrowserHost?.HasDevTools ?? false)
                {
                    model.InsertItemAt(0, (int)CefMenuIdentifiers.MENU_ID_HIDE_DEVTOOLS, "&Close DevTools");
                }
                else
                {
                    model.InsertItemAt(0, (int)CefMenuIdentifiers.MENU_ID_SHOW_DEVTOOLS, "Show &DevTools");
                }
            }

        }

        /// <summary>
        /// 右键菜单命令回调:处理 DevTools 显示/隐藏。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="state">菜单参数。</param>
        /// <param name="commandId">命令 ID。</param>
        /// <param name="eventFlags">事件标志。</param>
        /// <returns>是否已处理。</returns>
        internal bool OnContextMenuCommandCore(CefBrowser browser, CefFrame frame, CefContextMenuParams state, int commandId, CefEventFlags eventFlags)
        {

            if (commandId == (int)CefMenuIdentifiers.MENU_ID_SHOW_DEVTOOLS)
            {
                ShowDevTools();
                return true;
            }

            if (commandId == (int)CefMenuIdentifiers.MENU_ID_HIDE_DEVTOOLS)
            {
                HideDevTools();
                return true;
            }

            return false;
        }


        /// <summary>
        /// 显示右键菜单回调:转换坐标并显示菜单。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="parameters">菜单参数。</param>
        /// <param name="model">菜单模型。</param>
        /// <param name="callback">右键菜单回调。</param>
        /// <returns>是否已处理。</returns>
        internal bool RunContextMenuCore(CefBrowser browser, CefFrame frame, CefContextMenuParams parameters, CefMenuModel model, CefRunContextMenuCallback callback)
        {
            Debug.WriteLine($"[DBG-CTX] RunContextMenuCore x={parameters.X} y={parameters.Y}");

            var scaleFactor = WebViewHost.CurrentScaleFactor;

            var point = new POINT((int)(parameters.X * scaleFactor), (int)(parameters.Y * scaleFactor));

            User32.ClientToScreen(WindowHandle, ref point);

            var menuItems = CreateContextMenu(parameters, model);

            ShowContextMenu(callback, point, menuItems);

            return true;
        }

        /// <summary>
        /// 根据 CEF 菜单模型创建菜单项列表。
        /// </summary>
        /// <param name="menuParams">菜单参数。</param>
        /// <param name="model">菜单模型。</param>
        /// <returns>菜单项列表。</returns>
        private List<ContextMenuItem> CreateContextMenu(CefContextMenuParams menuParams, CefMenuModel model)
        {
            List<ContextMenuItem> items = new();

            CreateContentMenuItems(items, model);

            var item = items.SingleOrDefault(x => x.CommandId == (int)CefMenuIdentifiers.MENU_ID_REDO);

            if (item != null)
            {
                var idx = items.IndexOf(item);

                var place = idx + 1;

                if (place < items.Count - 1)
                {
                    items.Insert(place, new ContextMenuItem { IsSeperator = true });
                }

            }
            ;
            return items;
        }


        /// <summary>
        /// 递归创建内容菜单项。
        /// </summary>
        /// <param name="items">输出的菜单项列表。</param>
        /// <param name="model">菜单模型。</param>
        private void CreateContentMenuItems(List<ContextMenuItem> items, CefMenuModel model)
        {
            for (var i = 0; i < (int)model.Count; i++)
            {
                var type = model.GetItemTypeAt((UIntPtr)i);

                bool? isChecked = null;

                if (type == CefMenuItemType.Check)
                {
                    isChecked = model.IsCheckedAt((UIntPtr)i);
                }

                var cmdId = model.GetCommandIdAt((UIntPtr)i);

                var text = model.GetLabelAt((UIntPtr)i);

                var isEnabled = model.IsEnabledAt((UIntPtr)i);

                switch (type)
                {
                    case CefMenuItemType.Separator:
                        items.Add(new() { IsSeperator = true });
                        break;
                    case CefMenuItemType.Command:
                    case CefMenuItemType.Check:
                    case CefMenuItemType.Radio:
                        {
                            items.Add(new ContextMenuItem
                            {
                                CommandId = cmdId,
                                Text = text,
                                IsEnabled = isEnabled,
                                IsChecked = isChecked,
                                MenuItemType = type,
                            });
                        }
                        break;
                    case CefMenuItemType.SubMenu:
                        {


                            var subItems = model.GetSubMenuAt((UIntPtr)i);
                            if (subItems != null)
                            {
                                var subMenus = new List<ContextMenuItem>();
                                var menuItem = new ContextMenuItem
                                {
                                    CommandId = cmdId,
                                    Text = text,
                                    IsEnabled = isEnabled,
                                    IsChecked = isChecked,
                                    MenuItemType = type,
                                    SubMenus = new List<ContextMenuItem>()
                                };

                                CreateContentMenuItems(subMenus, subItems);

                                items.Add(menuItem);
                            }
                        }
                        break;
                    case CefMenuItemType.None:
                    default:
                        break;
                }
            }
        }



        /// <summary>
        /// 显示右键菜单并绑定点击/关闭事件。
        /// </summary>
        /// <param name="callback">右键菜单回调。</param>
        /// <param name="point">菜单显示位置。</param>
        /// <param name="menuItems">菜单项列表。</param>
        private void ShowContextMenu(CefRunContextMenuCallback callback, Point point, List<ContextMenuItem> menuItems)
        {
            Debug.WriteLine($"[DBG-CTX] ShowContextMenu point={point.X},{point.Y} items={menuItems.Count}");

            _contextMenu?.Close();
            _contextMenu?.Dispose();
            _contextMenu = null;

            var scaleFactor = WebViewHost.CurrentScaleFactor;

            if (scaleFactor > 1.25f) scaleFactor = 1.25f;

            var contextMenu = new AnimatedContextMenuStrip()
            {
                Renderer = new ContextMenuStripRenderer(IsDark),
            };

            ToolStripDropDownClosedEventHandler? closeHandler = null;
            ToolStripItemClickedEventHandler? clickHandler = null;

            clickHandler = (s, e) =>
            {
                var target = (AnimatedContextMenuStrip)s!;

                var targetItem = e.ClickedItem;

                if (targetItem != null)
                {
                    var config = (ContextMenuItem)targetItem.Tag;

                    callback?.Continue(config.CommandId, CefEventFlags.LeftMouseButton);
                }

                target.ItemClicked -= clickHandler;

                target.Close();
            };

            closeHandler = (s, e) =>
            {
                var target = (AnimatedContextMenuStrip)s!;

                if (callback != null)
                {
                    callback.Cancel();
                }

                target.Closed -= closeHandler;
            };


            contextMenu.ItemClicked += clickHandler;
            contextMenu.Closed += closeHandler;

            contextMenu.Items.Clear();


            BuildContextMenu(contextMenu.Items, menuItems);


            contextMenu.Show(point);

            _contextMenu = contextMenu;

        }

        /// <summary>
        /// 将菜单项列表构建为 ToolStrip 项。
        /// </summary>
        /// <param name="items">输出的 ToolStrip 项集合。</param>
        /// <param name="menuItems">菜单项列表。</param>
        private void BuildContextMenu(ToolStripItemCollection items, List<ContextMenuItem> menuItems)
        {
            foreach (var item in menuItems)
            {
                if (item.IsSeperator)
                {
                    items.Add(new ToolStripSeparator());

                    continue;
                }
                var menuItem = new ToolStripMenuItem
                {
                    Name = $"{item.CommandId}",
                    Text = item.Text,
                    Image = item.Icon,
                    Enabled = item.IsEnabled,
                    Checked = item.IsChecked.GetValueOrDefault(),
                    ShowShortcutKeys = item.ShortKeys.HasValue,
                    ShortcutKeys = item.ShortKeys ?? Keys.None,
                    Tag = item
                };

                if (item.SubMenus != null)
                {
                    BuildContextMenu(menuItem.DropDownItems, item.SubMenus);
                }

                items.Add(menuItem);
            }
        }

        #endregion
        #endregion

        #region ILifeSpanHandler
        /// <summary>
        /// 关闭浏览器回调:委托宿主处理关闭。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <returns>是否已处理。</returns>
        protected override bool DoClose(CefBrowser browser)
        {
            var cancel = WebViewHost.DoCloseCore(browser);

            if (!cancel)
            {
                CanClose = true;

                WebViewHost.Close();
            }

            return true;
        }

        /// <summary>
        /// 浏览器创建后回调:绑定浏览器实例、创建消息桥并注册各消息桥处理器。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        protected override void OnAfterCreated(CefBrowser browser)
        {
            Browser = browser;

            if (BrowserHost == null) throw new NullReferenceException("BrowserHost is null");

            IsBrowserInitialized = true;

            MessageDispatcher.RegisterMessageHandler("Robot.OnContextCreated", args => ContextCreated(args.Browser, args.Frame));

            MessageBridge = new MessageBridge(browser, false, MessageDispatcher);


            BrowserHandle = BrowserHost.GetWindowHandle();

            BrowserHost.NotifyMoveOrResizeStarted();

            BrowserHost.WasResized();

            WebViewHost.OnAfterCreatedCore(browser);

            MessageBridge.RegisterMessageBridgeHandler(JavaScriptEngine = new JavaScriptEngineBridge(MessageBridge));
            MessageBridge.RegisterMessageBridgeHandler(JavaScriptObjectMapping = new JavaScriptObjectMappingBridge(MessageBridge));
            MessageBridge.RegisterMessageBridgeHandler(JavaScriptWindowBindingObject = new JavaScriptWindowBindingObjectBridge(MessageBridge, WebViewHost));
        }

        /// <summary>
        /// 关闭前回调:释放消息桥。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        protected override void OnBeforeClose(CefBrowser browser)
        {
            MessageBridge?.OnBeforeClose(browser);

            WebViewHost.OnBeforeCloseCore(browser);

            MessageBridge?.Dispose();
        }

        /// <summary>
        /// 弹窗前回调:创建新的嵌入式浏览器窗口作为子窗口。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="targetUrl">目标地址。</param>
        /// <param name="targetFrameName">目标帧名称。</param>
        /// <param name="targetDisposition">窗口打开方式。</param>
        /// <param name="userGesture">是否用户手势触发。</param>
        /// <param name="popupFeatures">弹窗特性。</param>
        /// <param name="windowInfo">窗口信息。</param>
        /// <param name="client">输出的客户端。</param>
        /// <param name="settings">浏览器设置。</param>
        /// <param name="extraInfo">附加信息。</param>
        /// <param name="noJavascriptAccess">是否禁用 JavaScript 访问。</param>
        /// <returns>是否已处理。</returns>
        protected override bool OnBeforePopup(CefBrowser browser, CefFrame frame, string targetUrl, string targetFrameName, CefWindowOpenDisposition targetDisposition, bool userGesture, CefPopupFeatures popupFeatures, CefWindowInfo windowInfo, ref CefClient client, CefBrowserSettings settings, ref CefDictionaryValue extraInfo, ref bool noJavascriptAccess)
        {
            var retval = WebViewHost.OnBeforePopupCore(browser, frame, targetUrl, targetFrameName, targetDisposition, userGesture, popupFeatures, windowInfo, ref client, settings, ref extraInfo, ref noJavascriptAccess);

            if (!retval)
            {
                var bounds = new Rectangle();

                User32.WINDOWPLACEMENT placement = new();

                User32.GetWindowPlacement(WindowHandle, ref placement);

                var rect = placement.rcNormalPosition;

                if (popupFeatures.X.HasValue)
                {
                    bounds.X = popupFeatures.X.Value;
                }

                if (popupFeatures.Y.HasValue)
                {
                    bounds.Y = popupFeatures.Y.Value;
                }

                if (popupFeatures.Width.HasValue)
                {
                    bounds.Width = popupFeatures.Width.Value;
                }
                else
                {
                    bounds.Width = rect.Width;
                }

                if (popupFeatures.Height.HasValue)
                {
                    bounds.Height = popupFeatures.Height.Value;
                }
                else
                {
                    bounds.Height = rect.Height;
                }

                var browserWindow = new EmbeddedBrowserWindow();

                client = new EmbeddedlBrowserClient(browserWindow);


                browserWindow.Location = bounds.Location;
                browserWindow.Size = bounds.Size;

                windowInfo.SetAsChild(browserWindow.Handle, new CefRectangle(0, 0, browserWindow.ClientRectangle.Width, browserWindow.ClientRectangle.Height));

                browserWindow.Show();

                return false;
            }
            else
            {
                return retval;
            }
        }

        #endregion

        #region InternalJavaScripts
        /// <summary>
        /// 延迟执行内部脚本:浏览器未初始化时暂存,初始化后补执行。
        /// </summary>
        private Dictionary<string, string> DelayedInternalScripts { get; } = new Dictionary<string, string>();


        /// <summary>
        /// 窗口激活时调用前端 onWindowActivated。
        /// </summary>
        public void InvokeOnActivated()
        {
            var code = $"host?.hostWindow?.internal?.onWindowActivated();";

            if (!IsBrowserInitialized)
            {
                DelayedInternalScripts[nameof(InvokeOnActivated)] = code;
            }

            Browser?.GetMainFrame()?.ExecuteJavaScript(code, string.Empty, 0);

        }

        /// <summary>
        /// 窗口失焦时调用前端 onWindowDeactivate。
        /// </summary>
        public void InvokeOnDeactivate()
        {
            var code = $"host?.hostWindow?.internal?.onWindowDeactivate();";

            if (!IsBrowserInitialized)
            {
                DelayedInternalScripts[nameof(InvokeOnActivated)] = code;
            }

            Browser?.GetMainFrame()?.ExecuteJavaScript(code, string.Empty, 0);
        }

        /// <summary>
        /// 窗口状态变化时调用前端 onWindowStateChanged。
        /// </summary>
        /// <param name="state">窗口状态。</param>
        public void InvokeOnWindowStateChanged(string state)
        {
            var code = $"host?.hostWindow?.internal?.onWindowStateChanged(\"{state}\");";

            if (!IsBrowserInitialized)
            {
                DelayedInternalScripts[nameof(InvokeOnWindowStateChanged)] = code;
            }
            else
            {
                Browser?.GetMainFrame()?.ExecuteJavaScript(code, string.Empty, 0);
            }
        }

        /// <summary>
        /// 窗口尺寸变化时调用前端 onWindowResized。
        /// </summary>
        /// <param name="rect">窗口矩形。</param>
        public void InvokeOnWindowResized(Rectangle rect)
        {
            var code = $"host?.hostWindow?.internal?.onWindowResized({rect.Left},{rect.Top},{rect.Width},{rect.Height});";

            if (!IsBrowserInitialized)
            {
                DelayedInternalScripts[nameof(InvokeOnWindowResized)] = code;
            }
            else
            {
                Browser?.GetMainFrame()?.ExecuteJavaScript(code, string.Empty, 0);
            }
        }

        /// <summary>
        /// 窗口移动时调用前端 onWindowMoved。
        /// </summary>
        /// <param name="x">X 坐标。</param>
        /// <param name="y">Y 坐标。</param>
        public void InvokeOnWindowMoved(int x, int y)
        {
            var code = $"host?.hostWindow?.internal?.onWindowMoved({x},{y});";

            if (!IsBrowserInitialized)
            {
                DelayedInternalScripts[nameof(InvokeOnWindowMoved)] = code;
            }
            else
            {
                Browser?.GetMainFrame()?.ExecuteJavaScript(code, string.Empty, 0);
            }
        }

        /// <summary>
        /// 颜色模式变化时调用前端 onColorSchemeChanged。
        /// </summary>
        /// <param name="isDark">是否深色模式。</param>
        public void InvokeOnColorSchemeChanged(bool isDark)
        {
            var scheme = isDark ? "dark" : "light";
            var code = $"host?.hostWindow?.internal?.onColorSchemeChanged(\"{scheme}\");";

            if (!IsBrowserInitialized)
            {
                DelayedInternalScripts[nameof(InvokeOnColorSchemeChanged)] = code;
            }
            else
            {
                Browser?.GetMainFrame()?.ExecuteJavaScript(code, string.Empty, 0);
            }
        }
        #endregion

        #region Members
        /// <summary>
        /// 默认启动地址。
        /// </summary>
        const string DEFAULT_STARTUP_URL = "host://pages";

        /// <summary>
        /// 当前地址。
        /// </summary>
        private string _url = DEFAULT_STARTUP_URL;

        /// <summary>
        /// 是否深色模式。
        /// </summary>
        private bool _isDark = false;

        /// <summary>
        /// DevTools 窗口。
        /// </summary>
        DevToolsWindow? _devToolsWindow;

        /// <summary>
        /// 消息桥。
        /// </summary>
        public MessageBridge? MessageBridge { get; private set; }

        /// <summary>
        /// JavaScript 引擎桥。
        /// </summary>
        public JavaScriptEngineBridge? JavaScriptEngine { get; private set; }

        /// <summary>
        /// JavaScript 对象映射桥。
        /// </summary>
        public JavaScriptObjectMappingBridge? JavaScriptObjectMapping { get; private set; }

        /// <summary>
        /// JavaScript 窗口绑定对象桥。
        /// </summary>
        public JavaScriptWindowBindingObjectBridge? JavaScriptWindowBindingObject { get; private set; }

        /// <summary>
        /// 进程消息分发器。
        /// </summary>
        public ProcessMessageDispatcher MessageDispatcher { get; } = new ProcessMessageDispatcher();




        /// <summary>
        /// V8 上下文创建回调:委托宿主处理。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        private void ContextCreated(CefBrowser browser, CefFrame frame)
        {
            WebViewHost.ContextCreatedCore(browser, frame);
        }

        /// <summary>
        /// 颜色模式变化:通知前端。
        /// </summary>
        internal void ColorModeChange()
        {
            InvokeOnColorSchemeChanged(IsDark);
        }

        /// <summary>
        /// 加载地址:已初始化则立即加载,否则暂存。
        /// </summary>
        /// <param name="url">地址。</param>
        private void LoadUrl(string url)
        {
            url = url.TrimStart();

            TaskAction.Run(() =>
            {
                if (IsBrowserInitialized)
                {
                    Browser?.GetMainFrame()?.LoadUrl(url);
                }
                else
                {
                    _url = url;
                }
            });

        }

        /// <summary>
        /// 显示 DevTools:创建窗口并附加到浏览器。
        /// </summary>
        private void ShowDevToolsCore()
        {
            if (BrowserHost == null) return;

            if (_devToolsWindow == null || _devToolsWindow.IsDisposed)
            {
                _devToolsWindow = new DevToolsWindow(this);
            }

            User32.GetClientRect(_devToolsWindow.Handle, out var rect);

            var windowInfo = CefWindowInfo.Create();

            windowInfo.SetAsChild(_devToolsWindow.Handle, new CefRectangle(0, 0, rect.Width < 800 ? 800 : rect.Width, rect.Height < 600 ? 600 : rect.Height));

            BrowserHost.ShowDevTools(windowInfo, new DevToolsClient(_devToolsWindow), new CefBrowserSettings
            {
            }, new CefPoint(0, 0));

            if (!_devToolsWindow.Visible)
            {
                _devToolsWindow.Show();
            }
        }

        /// <summary>
        /// 隐藏 DevTools。
        /// </summary>
        private void HideDevToolsCore()
        {
            if (BrowserHost == null) return;

            BrowserHost.CloseDevTools();
        }
        #endregion
    }
}
