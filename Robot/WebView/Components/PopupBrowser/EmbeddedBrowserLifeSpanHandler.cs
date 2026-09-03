// Robot 桌面软件 — 嵌入式浏览器生命周期处理器
// 处理嵌入式浏览器创建、弹窗窗口创建与关闭

using System.Drawing;
using Vanara.PInvoke;
using Xilium.CefGlue;

namespace Robot.Browser.EmbeddedBrowser
{

    /// <summary>
    /// 嵌入式浏览器生命周期处理器:处理嵌入式浏览器创建、弹窗窗口创建与关闭。
    /// </summary>
    internal class EmbeddedBrowserLifeSpanHandler : CefLifeSpanHandler
    {
        /// <summary>
        /// 嵌入式浏览器客户端。
        /// </summary>
        public EmbeddedlBrowserClient BrowserClient { get; }

        /// <summary>
        /// 初始化 <see cref="EmbeddedBrowserLifeSpanHandler"/> 实例。
        /// </summary>
        /// <param name="browserClient">嵌入式浏览器客户端。</param>
        public EmbeddedBrowserLifeSpanHandler(EmbeddedlBrowserClient browserClient)
        {
            BrowserClient = browserClient;
        }

        /// <summary>
        /// 浏览器创建后回调:绑定窗口句柄、浏览器实例并订阅尺寸变化事件。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        protected override void OnAfterCreated(CefBrowser browser)
        {
            BrowserClient.BrowserWindow.BrowserWindowHandle = browser.GetHost().GetWindowHandle();

            BrowserClient.BrowserWindow.Browser = browser;

            var window = BrowserClient.BrowserWindow;

            window.SizeChanged += (_, _) => browser?.GetHost()?.NotifyMoveOrResizeStarted();
            window.ResizeBegin += (_, _) => browser?.GetHost()?.NotifyMoveOrResizeStarted();
            window.ResizeEnd += (_, _) => browser?.GetHost()?.WasResized();
            window.Move += (_, _) => browser?.GetHost()?.NotifyMoveOrResizeStarted();
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
            var bounds = new Rectangle();

            var window = BrowserClient.BrowserWindow;

            User32.GetWindowRect(window.Handle, out var rect);

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

            browserWindow.Location = bounds.Location;
            browserWindow.Size = bounds.Size;

            client = new EmbeddedlBrowserClient(browserWindow);

            browserWindow.Show();

            windowInfo.SetAsChild(browserWindow.Handle, new CefRectangle(0, 0, browserWindow.ClientRectangle.Width, browserWindow.ClientRectangle.Height));

            return false;
        }

        /// <summary>
        /// 关闭前回调。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        protected override void OnBeforeClose(CefBrowser browser)
        {
            base.OnBeforeClose(browser);

            //BrowserClient.BrowserWindow.Close();
        }

        /// <summary>
        /// 关闭浏览器回调。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <returns>是否已处理。</returns>
        protected override bool DoClose(CefBrowser browser)
        {
            return base.DoClose(browser);
        }
    }
}
