// Robot 桌面软件 — WebView 浏览器客户端
// 承载 CEF 各处理器(显示/下载/拖拽/焦点/键盘/加载/渲染/请求/右键菜单),并转发进程消息

using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Drawing;
using Vanara.PInvoke;
using Xilium.CefGlue;

namespace Robot.Browser
{

    /// <summary>
    /// WebView 浏览器客户端:创建并缓存各 CEF 处理器,转发进程消息。
    /// </summary>
    internal class WebViewClient : CefClient
    {
        /// <summary>
        /// 所属 WebView。
        /// </summary>
        public WebViewLifeSpan WebView { get; }

        /// <summary>
        /// 初始化 <see cref="WebViewClient"/> 实例。
        /// </summary>
        /// <param name="webView">所属 WebView。</param>
        public WebViewClient(WebViewLifeSpan webView)
        {
            WebView = webView;
        }

        /// <summary>
        /// 获取右键菜单处理器(懒加载)。
        /// </summary>
        /// <returns>右键菜单处理器。</returns>
        protected override CefContextMenuHandler? GetContextMenuHandler()
        {
            Debug.WriteLine("[DBG-CTX] GetContextMenuHandler 被调用");

            return _contextMenuHandler ??= new WebViewContextMenuHandlerAdapter(WebView);
        }

        /// <summary>
        /// 获取显示处理器(懒加载)。
        /// </summary>
        /// <returns>显示处理器。</returns>
        protected override CefDisplayHandler? GetDisplayHandler()
        {
            return _displayHandler ??= new WebViewDisplayHandlerAdapter(WebView.WebViewHost);
        }

        /// <summary>
        /// 获取下载处理器(懒加载)。
        /// </summary>
        /// <returns>下载处理器。</returns>
        protected override CefDownloadHandler? GetDownloadHandler()
        {
            return _downloadHandler ??= new WebViewDownloadHandlerAdapter(WebView.WebViewHost);
        }

        /// <summary>
        /// 获取拖拽处理器(懒加载)。
        /// </summary>
        /// <returns>拖拽处理器。</returns>
        protected override CefDragHandler? GetDragHandler()
        {
            return _dragHandler ??= new WebViewDragHandlerAdapter(WebView);
        }

        /// <summary>
        /// 获取焦点处理器(懒加载)。
        /// </summary>
        /// <returns>焦点处理器。</returns>
        protected override CefFocusHandler? GetFocusHandler()
        {
            return _focusHandler ??= new WebViewFocusHandlerAdapter(WebView.WebViewHost);
        }

        /// <summary>
        /// 获取键盘处理器(懒加载)。
        /// </summary>
        /// <returns>键盘处理器。</returns>
        protected override CefKeyboardHandler? GetKeyboardHandler()
        {
            return _keyboardHandler ??= new WebViewKeyboardHandlerAdapter(WebView.WebViewHost);
        }

        /// <summary>
        /// 获取生命周期处理器(即 WebView 本身)。
        /// </summary>
        /// <returns>生命周期处理器。</returns>
        protected override CefLifeSpanHandler? GetLifeSpanHandler()
        {
            return WebView;
        }

        /// <summary>
        /// 获取加载处理器(懒加载)。
        /// </summary>
        /// <returns>加载处理器。</returns>
        protected override CefLoadHandler? GetLoadHandler()
        {
            return _loadHandler ??= new WebViewLoadHandlerAdapter(WebView.WebViewHost);
        }

        /// <summary>
        /// 获取渲染处理器(懒加载)。
        /// </summary>
        /// <returns>渲染处理器。</returns>
        protected override CefRenderHandler? GetRenderHandler()
        {
            return _renderHandler ??= new WebViewRenderHandlerAdapter(WebView.WebViewHost);
        }

        /// <summary>
        /// 获取请求处理器(懒加载)。
        /// </summary>
        /// <returns>请求处理器。</returns>
        protected override CefRequestHandler? GetRequestHandler()
        {
            return _requestHandler ??= new WebViewRequestHandlerAdapter(WebView);
        }

        /// <summary>
        /// 接收进程消息:分发给消息分发器。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="sourceProcess">源进程。</param>
        /// <param name="message">进程消息。</param>
        /// <returns>是否已处理。</returns>
        protected override bool OnProcessMessageReceived(CefBrowser browser, CefFrame frame, CefProcessId sourceProcess, CefProcessMessage message)
        {
            WebView.MessageDispatcher.DispatchMessage(browser, frame, sourceProcess, message);

            return false;
        }

        /// <summary>
        /// 右键菜单处理器缓存。
        /// </summary>
        private CefContextMenuHandler? _contextMenuHandler;

        /// <summary>
        /// 显示处理器缓存。
        /// </summary>
        private CefDisplayHandler? _displayHandler;

        /// <summary>
        /// 下载处理器缓存。
        /// </summary>
        private CefDownloadHandler? _downloadHandler;

        /// <summary>
        /// 拖拽处理器缓存。
        /// </summary>
        private CefDragHandler? _dragHandler;

        /// <summary>
        /// 焦点处理器缓存。
        /// </summary>
        private CefFocusHandler? _focusHandler;

        /// <summary>
        /// 键盘处理器缓存。
        /// </summary>
        private CefKeyboardHandler? _keyboardHandler;

        /// <summary>
        /// 加载处理器缓存。
        /// </summary>
        private CefLoadHandler? _loadHandler;

        /// <summary>
        /// 渲染处理器缓存。
        /// </summary>
        private CefRenderHandler? _renderHandler;

        /// <summary>
        /// 请求处理器缓存。
        /// </summary>
        private CefRequestHandler? _requestHandler;
    }

    /// <summary>
    /// 显示处理器适配器:将 CEF 显示事件转发给 <see cref="RobotWindow"/>。
    /// </summary>
    internal class WebViewDisplayHandlerAdapter : CefDisplayHandler
    {
        /// <summary>
        /// 目标宿主。
        /// </summary>
        private readonly RobotWindow _form;

        /// <summary>
        /// 初始化 <see cref="WebViewDisplayHandlerAdapter"/> 实例。
        /// </summary>
        /// <param name="form">目标宿主。</param>
        public WebViewDisplayHandlerAdapter(RobotWindow form)
        {
            _form = form;
        }

        /// <summary>
        /// 地址变化:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="url">新地址。</param>
        protected override void OnAddressChange(CefBrowser browser, CefFrame frame, string url) => _form.OnPageAddressChangeCore(browser, frame, url);

        /// <summary>
        /// 自动缩放:未处理。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="newSize">新尺寸。</param>
        /// <returns>是否已处理。</returns>
        protected override bool OnAutoResize(CefBrowser browser, ref CefSize newSize) => false;

        /// <summary>
        /// 控制台消息:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="level">日志级别。</param>
        /// <param name="message">消息内容。</param>
        /// <param name="source">来源。</param>
        /// <param name="line">行号。</param>
        /// <returns>是否已处理。</returns>
        protected override bool OnConsoleMessage(CefBrowser browser, CefLogSeverity level, string message, string source, int line) => _form.OnConsoleMessageCore(browser, level, message, source, line);

        /// <summary>
        /// 光标变化:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="cursorHandle">光标句柄。</param>
        /// <param name="type">光标类型。</param>
        /// <param name="customCursorInfo">自定义光标信息。</param>
        /// <returns>是否已处理。</returns>
        protected override bool OnCursorChange(CefBrowser browser, nint cursorHandle, CefCursorType type, CefCursorInfo customCursorInfo) => _form.OnCursorChangeCore(browser, cursorHandle, type, customCursorInfo);

        /// <summary>
        /// 图标地址变化:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="iconUrls">图标地址集合。</param>
        protected override void OnFaviconUrlChange(CefBrowser browser, string[] iconUrls) => _form.OnFaviconUrlChangeCore(browser, iconUrls);

        /// <summary>
        /// 全屏模式变化:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="fullscreen">是否全屏。</param>
        protected override void OnFullscreenModeChange(CefBrowser browser, bool fullscreen) => _form.OnFullscreenModeChangeCore(browser, fullscreen);

        /// <summary>
        /// 加载进度变化:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="progress">进度值。</param>
        protected override void OnLoadingProgressChange(CefBrowser browser, double progress) => _form.OnPageLoadingProgressChangeCore(browser, progress);

        /// <summary>
        /// 媒体访问变化:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="hasVideoAccess">是否有视频访问。</param>
        /// <param name="hasAudioAccess">是否有音频访问。</param>
        protected override void OnMediaAccessChange(CefBrowser browser, bool hasVideoAccess, bool hasAudioAccess) => _form.OnMediaAccessChangeCore(browser, hasVideoAccess, hasAudioAccess);

        /// <summary>
        /// 状态消息变化:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="value">状态消息。</param>
        protected override void OnStatusMessage(CefBrowser browser, string value) => _form.OnStatusMessageCore(browser, value);

        /// <summary>
        /// 标题变化:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="title">新标题。</param>
        protected override void OnTitleChange(CefBrowser browser, string title) => _form.OnPageTitleChangeCore(browser, title);

        /// <summary>
        /// 提示文本:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="text">提示文本。</param>
        /// <returns>是否已处理。</returns>
        protected override bool OnTooltip(CefBrowser browser, string text) => _form.OnTooltipShowCore(browser, text);
    }

    /// <summary>
    /// 下载处理器适配器:将 CEF 下载事件转发给 <see cref="RobotWindow"/>。
    /// </summary>
    internal class WebViewDownloadHandlerAdapter : CefDownloadHandler
    {
        /// <summary>
        /// 目标宿主。
        /// </summary>
        private readonly RobotWindow _form;

        /// <summary>
        /// 初始化 <see cref="WebViewDownloadHandlerAdapter"/> 实例。
        /// </summary>
        /// <param name="form">目标宿主。</param>
        public WebViewDownloadHandlerAdapter(RobotWindow form)
        {
            _form = form;
        }

        /// <summary>
        /// 是否允许下载:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="url">下载地址。</param>
        /// <param name="requestMethod">请求方法。</param>
        /// <returns>是否允许下载。</returns>
        protected override bool CanDownload(CefBrowser browser, string url, string requestMethod) => _form.CanDownloadCore(browser, url, requestMethod);

        /// <summary>
        /// 下载前:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="downloadItem">下载项。</param>
        /// <param name="suggestedName">建议文件名。</param>
        /// <param name="callback">下载回调。</param>
        protected override void OnBeforeDownload(CefBrowser browser, CefDownloadItem downloadItem, string suggestedName, CefBeforeDownloadCallback callback) => _form.OnBeforeDownloadCore(browser, downloadItem, suggestedName, callback);

        /// <summary>
        /// 下载更新:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="downloadItem">下载项。</param>
        /// <param name="callback">下载项回调。</param>
        protected override void OnDownloadUpdated(CefBrowser browser, CefDownloadItem downloadItem, CefDownloadItemCallback callback) => _form.OnDownloadUpdatedCore(browser, downloadItem, callback);
    }

    /// <summary>
    /// 拖拽处理器适配器:转发拖拽事件并维护可拖拽区域。
    /// </summary>
    internal class WebViewDragHandlerAdapter : CefDragHandler
    {
        /// <summary>
        /// 所属 WebView。
        /// </summary>
        private readonly WebViewLifeSpan _webView;

        /// <summary>
        /// 目标宿主。
        /// </summary>
        private readonly RobotWindow _form;

        /// <summary>
        /// 初始化 <see cref="WebViewDragHandlerAdapter"/> 实例。
        /// </summary>
        /// <param name="webView">所属 WebView。</param>
        public WebViewDragHandlerAdapter(WebViewLifeSpan webView)
        {
            _webView = webView;
            _form = webView.WebViewHost;
        }

        /// <summary>
        /// 拖拽进入:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="dragData">拖拽数据。</param>
        /// <param name="mask">允许的拖拽操作。</param>
        /// <returns>是否已处理。</returns>
        protected override bool OnDragEnter(CefBrowser browser, CefDragData dragData, CefDragOperationsMask mask)
        {
            return _form.OnDragEnterCore(browser, dragData, mask);
        }

        /// <summary>
        /// 可拖拽区域变化:转发给宿主并按 DPI 缩放后更新宿主可拖拽区域。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="regions">可拖拽区域集合。</param>
        protected override void OnDraggableRegionsChanged(CefBrowser browser, CefFrame frame, CefDraggableRegion[] regions)
        {
            _form.OnDraggableRegionsChangedCore(browser, frame, regions);

            var draggableRegion = new Region(new Rectangle(0, 0, 0, 0));

            if (_webView.WindowHandle != IntPtr.Zero && regions != null && regions.Length > 0)
            {
                var scaleFactor = User32.GetDpiForWindow(_webView.WindowHandle) / 96f;

                foreach (var region in regions)
                {
                    var rect = new Rectangle((int)(region.Bounds.X * scaleFactor), (int)(region.Bounds.Y * scaleFactor), (int)(region.Bounds.Width * scaleFactor), (int)(region.Bounds.Height * scaleFactor));

                    if (region.Draggable)
                    {
                        draggableRegion.Union(rect);
                    }
                    else
                    {
                        draggableRegion.Exclude(rect);
                    }
                }
            }

            _webView.WebViewHost.DraggableRegion?.Dispose();

            _webView.WebViewHost.DraggableRegion = draggableRegion;
        }
    }

    /// <summary>
    /// 焦点处理器适配器:将 CEF 焦点事件转发给 <see cref="RobotWindow"/>。
    /// </summary>
    internal class WebViewFocusHandlerAdapter : CefFocusHandler
    {
        /// <summary>
        /// 目标宿主。
        /// </summary>
        private readonly RobotWindow _form;

        /// <summary>
        /// 初始化 <see cref="WebViewFocusHandlerAdapter"/> 实例。
        /// </summary>
        /// <param name="form">目标宿主。</param>
        public WebViewFocusHandlerAdapter(RobotWindow form)
        {
            _form = form;
        }

        /// <summary>
        /// 获得焦点:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        protected override void OnGotFocus(CefBrowser browser) => _form.OnGotFocusCore(browser);

        /// <summary>
        /// 设置焦点:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="source">焦点来源。</param>
        /// <returns>是否已处理。</returns>
        protected override bool OnSetFocus(CefBrowser browser, CefFocusSource source) => _form.OnSetFocusCore(browser, source);

        /// <summary>
        /// 转移焦点:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="next">是否转移到下一个控件。</param>
        protected override void OnTakeFocus(CefBrowser browser, bool next) => _form.OnTakeFocusCore(browser, next);
    }

    /// <summary>
    /// 键盘处理器适配器:将 CEF 键盘事件转发给 <see cref="RobotWindow"/>。
    /// </summary>
    internal class WebViewKeyboardHandlerAdapter : CefKeyboardHandler
    {
        /// <summary>
        /// 目标宿主。
        /// </summary>
        private readonly RobotWindow _form;

        /// <summary>
        /// 初始化 <see cref="WebViewKeyboardHandlerAdapter"/> 实例。
        /// </summary>
        /// <param name="form">目标宿主。</param>
        public WebViewKeyboardHandlerAdapter(RobotWindow form)
        {
            _form = form;
        }

        /// <summary>
        /// 按键事件:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="keyEvent">按键事件。</param>
        /// <param name="osEvent">操作系统事件。</param>
        /// <returns>是否已处理。</returns>
        protected override bool OnKeyEvent(CefBrowser browser, CefKeyEvent keyEvent, nint osEvent) => _form.OnKeyEventCore(browser, keyEvent, osEvent);

        /// <summary>
        /// 按键前事件:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="keyEvent">按键事件。</param>
        /// <param name="osEvent">操作系统事件。</param>
        /// <param name="isKeyboardShortcut">是否为键盘快捷键。</param>
        /// <returns>是否已处理。</returns>
        protected override bool OnPreKeyEvent(CefBrowser browser, CefKeyEvent keyEvent, nint osEvent, out bool isKeyboardShortcut) => _form.OnPreKeyEventCore(browser, keyEvent, osEvent, out isKeyboardShortcut);
    }

    /// <summary>
    /// 加载处理器适配器:将 CEF 加载事件转发给 <see cref="RobotWindow"/>。
    /// </summary>
    internal class WebViewLoadHandlerAdapter : CefLoadHandler
    {
        /// <summary>
        /// 目标宿主。
        /// </summary>
        private readonly RobotWindow _form;

        /// <summary>
        /// 初始化 <see cref="WebViewLoadHandlerAdapter"/> 实例。
        /// </summary>
        /// <param name="form">目标宿主。</param>
        public WebViewLoadHandlerAdapter(RobotWindow form)
        {
            _form = form;
        }

        /// <summary>
        /// 加载开始:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="transitionType">过渡类型。</param>
        protected override void OnLoadStart(CefBrowser browser, CefFrame frame, CefTransitionType transitionType) => _form.OnLoadStartCore(browser, frame, transitionType);

        /// <summary>
        /// 加载结束:主帧就绪时通知前端,再转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="httpStatusCode">HTTP 状态码。</param>
        protected override void OnLoadEnd(CefBrowser browser, CefFrame frame, int httpStatusCode)
        {
            if (frame.IsMain)
            {
                frame.ExecuteJavaScript("window.host && host?.hostWindow?.internal?.setDocumentReadyState()", string.Empty, 0);
            }

            _form.OnLoadEndCore(browser, frame, httpStatusCode);
        }

        /// <summary>
        /// 加载错误:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="errorCode">错误码。</param>
        /// <param name="errorText">错误文本。</param>
        /// <param name="failedUrl">失败地址。</param>
        protected override void OnLoadError(CefBrowser browser, CefFrame frame, CefErrorCode errorCode, string errorText, string failedUrl) => _form.OnLoadErrorCore(browser, frame, errorCode, errorText, failedUrl);

        /// <summary>
        /// 加载状态变化:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="isLoading">是否正在加载。</param>
        /// <param name="canGoBack">是否可后退。</param>
        /// <param name="canGoForward">是否可前进。</param>
        protected override void OnLoadingStateChange(CefBrowser browser, bool isLoading, bool canGoBack, bool canGoForward) => _form.OnLoadingStateChangeCore(browser, isLoading, canGoBack, canGoForward);
    }

    /// <summary>
    /// 渲染处理器适配器:将 CEF 渲染事件转发给 <see cref="RobotWindow"/>。
    /// </summary>
    internal class WebViewRenderHandlerAdapter : CefRenderHandler
    {
        /// <summary>
        /// 目标宿主。
        /// </summary>
        private readonly RobotWindow _form;

        /// <summary>
        /// 初始化 <see cref="WebViewRenderHandlerAdapter"/> 实例。
        /// </summary>
        /// <param name="form">目标宿主。</param>
        public WebViewRenderHandlerAdapter(RobotWindow form)
        {
            _form = form;
        }

        /// <summary>
        /// 获取无障碍处理器:转发给宿主。
        /// </summary>
        /// <returns>无障碍处理器。</returns>
        protected override CefAccessibilityHandler? GetAccessibilityHandler() => _form.GetAccessibilityHandlerCore();

        /// <summary>
        /// 获取根屏幕矩形:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="rect">输出的屏幕矩形。</param>
        /// <returns>是否成功。</returns>
        protected override bool GetRootScreenRect(CefBrowser browser, ref CefRectangle rect) => _form.GetRootScreenRectCore(ref rect);

        /// <summary>
        /// 获取屏幕信息:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="screenInfo">屏幕信息。</param>
        /// <returns>是否成功。</returns>
        protected override bool GetScreenInfo(CefBrowser browser, CefScreenInfo screenInfo) => _form.GetScreenInfoCore(screenInfo);

        /// <summary>
        /// 获取屏幕坐标:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="viewX">视图 X 坐标。</param>
        /// <param name="viewY">视图 Y 坐标。</param>
        /// <param name="screenX">输出的屏幕 X 坐标。</param>
        /// <param name="screenY">输出的屏幕 Y 坐标。</param>
        /// <returns>是否成功。</returns>
        protected override bool GetScreenPoint(CefBrowser browser, int viewX, int viewY, ref int screenX, ref int screenY) => _form.GetScreenPointCore(viewX, viewY, ref screenX, ref screenY);

        /// <summary>
        /// 获取触摸句柄尺寸:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="orientation">方向。</param>
        /// <param name="size">输出的尺寸。</param>
        protected override void GetTouchHandleSize(CefBrowser browser, CefHorizontalAlignment orientation, out CefSize size) => _form.GetTouchHandleSizeCore(orientation, out size);

        /// <summary>
        /// 获取视图矩形:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="rect">输出的视图矩形。</param>
        protected override void GetViewRect(CefBrowser browser, out CefRectangle rect) => _form.GetViewRectCore(out rect);

        /// <summary>
        /// 加速绘制:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="type">绘制元素类型。</param>
        /// <param name="dirtyRects">脏矩形集合。</param>
        /// <param name="sharedHandle">共享句柄。</param>
        protected override void OnAcceleratedPaint(CefBrowser browser, CefPaintElementType type, CefRectangle[] dirtyRects, nint sharedHandle) => _form.OnAcceleratedPaintCore();

        /// <summary>
        /// 输入法组合范围变化:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="selectedRange">选区范围。</param>
        /// <param name="characterBounds">字符边界集合。</param>
        protected override void OnImeCompositionRangeChanged(CefBrowser browser, CefRange selectedRange, CefRectangle[] characterBounds) => _form.OnImeCompositionRangeChangedCore(selectedRange, characterBounds);

        /// <summary>
        /// 绘制:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="type">绘制元素类型。</param>
        /// <param name="dirtyRects">脏矩形集合。</param>
        /// <param name="buffer">像素缓冲区。</param>
        /// <param name="width">宽度。</param>
        /// <param name="height">高度。</param>
        protected override void OnPaint(CefBrowser browser, CefPaintElementType type, CefRectangle[] dirtyRects, nint buffer, int width, int height) => _form.OnPaintCore(type, dirtyRects, buffer, width, height);

        /// <summary>
        /// 弹窗显示:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="show">是否显示。</param>
        protected override void OnPopupShow(CefBrowser browser, bool show) => _form.OnPopupShowCore(show);

        /// <summary>
        /// 弹窗尺寸:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="rect">弹窗矩形。</param>
        protected override void OnPopupSize(CefBrowser browser, CefRectangle rect) => _form.OnPopupSizeCore(rect);

        /// <summary>
        /// 滚动偏移变化:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="x">X 偏移。</param>
        /// <param name="y">Y 偏移。</param>
        protected override void OnScrollOffsetChanged(CefBrowser browser, double x, double y) => _form.OnScrollOffsetChangedCore(x, y);

        /// <summary>
        /// 文本选区变化:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="selectedText">选中文本。</param>
        /// <param name="selectedRange">选区范围。</param>
        protected override void OnTextSelectionChanged(CefBrowser browser, string selectedText, CefRange selectedRange) => _form.OnTextSelectionChangedCore(selectedText, selectedRange);

        /// <summary>
        /// 触摸句柄状态变化:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="state">句柄状态。</param>
        protected override void OnTouchHandleStateChanged(CefBrowser browser, CefTouchHandleState state) => _form.OnTouchHandleStateChangedCore(state);

        /// <summary>
        /// 请求虚拟键盘:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="inputMode">输入模式。</param>
        protected override void OnVirtualKeyboardRequested(CefBrowser browser, CefTextInputMode inputMode) => _form.OnVirtualKeyboardRequestedCore(inputMode);

        /// <summary>
        /// 开始拖拽:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="dragData">拖拽数据。</param>
        /// <param name="allowedOps">允许的拖拽操作。</param>
        /// <param name="x">X 坐标。</param>
        /// <param name="y">Y 坐标。</param>
        /// <returns>是否成功。</returns>
        protected override bool StartDragging(CefBrowser browser, CefDragData dragData, CefDragOperationsMask allowedOps, int x, int y) => _form.StartDraggingCore(dragData, allowedOps, x, y);

        /// <summary>
        /// 更新拖拽光标:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="operation">拖拽操作。</param>
        protected override void UpdateDragCursor(CefBrowser browser, CefDragOperationsMask operation) => _form.UpdateDragCursorCore(operation);
    }

    /// <summary>
    /// 请求处理器适配器:将 CEF 请求事件转发给 <see cref="RobotWindow"/> 并联动消息桥。
    /// </summary>
    internal class WebViewRequestHandlerAdapter : CefRequestHandler
    {
        /// <summary>
        /// 所属 WebView。
        /// </summary>
        private readonly WebViewLifeSpan _webView;

        /// <summary>
        /// 目标宿主。
        /// </summary>
        private readonly RobotWindow _form;

        /// <summary>
        /// 初始化 <see cref="WebViewRequestHandlerAdapter"/> 实例。
        /// </summary>
        /// <param name="webView">所属 WebView。</param>
        public WebViewRequestHandlerAdapter(WebViewLifeSpan webView)
        {
            _webView = webView;
            _form = webView.WebViewHost;
        }

        /// <summary>
        /// 获取认证凭据:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="originUrl">来源地址。</param>
        /// <param name="isProxy">是否为代理。</param>
        /// <param name="host">主机。</param>
        /// <param name="port">端口。</param>
        /// <param name="realm">域。</param>
        /// <param name="scheme">协议。</param>
        /// <param name="callback">认证回调。</param>
        /// <returns>是否已处理。</returns>
        protected override bool GetAuthCredentials(CefBrowser browser, string originUrl, bool isProxy, string host, int port, string realm, string scheme, CefAuthCallback callback)
            => _form.GetAuthCredentialsCore(browser, originUrl, isProxy, host, port, realm, scheme, callback);

        /// <summary>
        /// 获取资源请求处理器:未处理。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="request">请求。</param>
        /// <param name="isNavigation">是否为导航。</param>
        /// <param name="isDownload">是否为下载。</param>
        /// <param name="requestInitiator">请求发起者。</param>
        /// <param name="disableDefaultHandling">是否禁用默认处理。</param>
        /// <returns>资源请求处理器。</returns>
        protected override CefResourceRequestHandler? GetResourceRequestHandler(CefBrowser browser, CefFrame frame, CefRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
            => null;

        /// <summary>
        /// 浏览前:转发给宿主并联动消息桥。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="request">请求。</param>
        /// <param name="userGesture">是否用户手势触发。</param>
        /// <param name="isRedirect">是否为重定向。</param>
        /// <returns>是否已处理。</returns>
        protected override bool OnBeforeBrowse(CefBrowser browser, CefFrame frame, CefRequest request, bool userGesture, bool isRedirect)
        {
            var retval = _form.OnBeforeBrowseCore(browser, frame, request, userGesture, isRedirect);

            _webView.MessageBridge?.OnBeforeBrowse(browser, frame, request, userGesture, isRedirect);

            return retval;
        }

        /// <summary>
        /// 证书错误:未处理。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="certError">证书错误码。</param>
        /// <param name="requestUrl">请求地址。</param>
        /// <param name="sslInfo">SSL 信息。</param>
        /// <param name="callback">回调。</param>
        /// <returns>是否已处理。</returns>
        protected override bool OnCertificateError(CefBrowser browser, CefErrorCode certError, string requestUrl, CefSslInfo sslInfo, CefCallback callback)
            => false;

        /// <summary>
        /// 主帧文档可用:转发给宿主并触发颜色模式变化。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        protected override void OnDocumentAvailableInMainFrame(CefBrowser browser)
        {
            _form.OnDocumentAvailableInMainFrameCore(browser);

            _webView.ColorModeChange();
        }

        /// <summary>
        /// 从标签页打开地址:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="targetUrl">目标地址。</param>
        /// <param name="targetDisposition">窗口打开方式。</param>
        /// <param name="userGesture">是否用户手势触发。</param>
        /// <returns>是否已处理。</returns>
        protected override bool OnOpenUrlFromTab(CefBrowser browser, CefFrame frame, string targetUrl, CefWindowOpenDisposition targetDisposition, bool userGesture)
            => _form.OnOpenUrlFromTabCore(browser, frame, targetUrl, targetDisposition, userGesture);

        /// <summary>
        /// 渲染进程终止:联动消息桥并转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="status">终止状态。</param>
        protected override void OnRenderProcessTerminated(CefBrowser browser, CefTerminationStatus status)
        {
            _webView.MessageBridge?.OnRenderProcessTerminated(browser);

            _form.OnRenderProcessTerminatedCore(browser, status);
        }

        /// <summary>
        /// 渲染视图就绪:转发给宿主。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        protected override void OnRenderViewReady(CefBrowser browser)
            => _form.OnRenderViewReadyCore(browser);

        /// <summary>
        /// 选择客户端证书:未处理。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="isProxy">是否为代理。</param>
        /// <param name="host">主机。</param>
        /// <param name="port">端口。</param>
        /// <param name="certificates">证书集合。</param>
        /// <param name="callback">选择回调。</param>
        /// <returns>是否已处理。</returns>
        protected override bool OnSelectClientCertificate(CefBrowser browser, bool isProxy, string host, int port, CefX509Certificate[] certificates, CefSelectClientCertificateCallback callback)
            => false;
    }

    /// <summary>
    /// 右键菜单处理器适配器:将 CEF 右键菜单事件转发给 <see cref="WebViewLifeSpan"/>。
    /// </summary>
    internal class WebViewContextMenuHandlerAdapter : CefContextMenuHandler
    {
        /// <summary>
        /// 所属 WebView。
        /// </summary>
        private readonly WebViewLifeSpan _webView;

        /// <summary>
        /// 初始化 <see cref="WebViewContextMenuHandlerAdapter"/> 实例。
        /// </summary>
        /// <param name="webView">所属 WebView。</param>
        public WebViewContextMenuHandlerAdapter(WebViewLifeSpan webView)
        {
            _webView = webView;
        }

        /// <summary>
        /// 右键菜单前:记录日志并委托 WebView 处理。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="state">菜单参数。</param>
        /// <param name="model">菜单模型。</param>
        protected override void OnBeforeContextMenu(CefBrowser browser, CefFrame frame, CefContextMenuParams state, CefMenuModel model)
        {
            Debug.WriteLine($"[DBG-CTX] OnBeforeContextMenu count={model.Count}");

            _webView.OnBeforeContextMenuCore(browser, frame, state, model);
        }

        /// <summary>
        /// 右键菜单命令:委托 WebView 处理。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="state">菜单参数。</param>
        /// <param name="commandId">命令 ID。</param>
        /// <param name="eventFlags">事件标志。</param>
        /// <returns>是否已处理。</returns>
        protected override bool OnContextMenuCommand(CefBrowser browser, CefFrame frame, CefContextMenuParams state, int commandId, CefEventFlags eventFlags)
            => _webView.OnContextMenuCommandCore(browser, frame, state, commandId, eventFlags);

        /// <summary>
        /// 运行右键菜单:记录日志并委托 WebView 处理。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="parameters">菜单参数。</param>
        /// <param name="model">菜单模型。</param>
        /// <param name="callback">运行回调。</param>
        /// <returns>是否已处理。</returns>
        protected override bool RunContextMenu(CefBrowser browser, CefFrame frame, CefContextMenuParams parameters, CefMenuModel model, CefRunContextMenuCallback callback)
        {
            Debug.WriteLine($"[DBG-CTX] RunContextMenu x={parameters.X} y={parameters.Y}");

            return _webView.RunContextMenuCore(browser, frame, parameters, model, callback);
        }

        /// <summary>
        /// 右键菜单关闭:委托 WebView 关闭菜单。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        protected override void OnContextMenuDismissed(CefBrowser browser, CefFrame frame)
            => _webView.CloseContextMenu();

        /// <summary>
        /// 快速菜单命令:未处理。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="commandId">命令 ID。</param>
        /// <param name="eventFlags">事件标志。</param>
        /// <returns>是否已处理。</returns>
        protected override bool OnQuickMenuCommand(CefBrowser browser, CefFrame frame, int commandId, CefEventFlags eventFlags)
            => false;

        /// <summary>
        /// 快速菜单关闭:空实现。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        protected override void OnQuickMenuDismissed(CefBrowser browser, CefFrame frame)
        {
        }

        /// <summary>
        /// 运行快速菜单:未处理。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="location">位置。</param>
        /// <param name="size">尺寸。</param>
        /// <param name="editStateFlags">编辑状态标志。</param>
        /// <param name="callback">运行回调。</param>
        /// <returns>是否已处理。</returns>
        protected override bool RunQuickMenu(CefBrowser browser, CefFrame frame, CefPoint location, CefSize size, CefQuickMenuEditStateFlags editStateFlags, CefRunQuickMenuCallback callback)
            => false;
    }
}
