// Robot 桌面软件 — 嵌入式浏览器显示处理器
// 处理嵌入式浏览器的标题、地址与全屏模式变化

using Robot.Properties;
using System.Windows.Forms;
using Xilium.CefGlue;

namespace Robot.Browser.EmbeddedBrowser
{

    /// <summary>
    /// 嵌入式浏览器显示处理器:处理嵌入式浏览器的标题、地址与全屏模式变化。
    /// </summary>
    internal class EmbeddedBrowserDisplayHandler : CefDisplayHandler
    {
        /// <summary>
        /// 嵌入式浏览器客户端。
        /// </summary>
        public EmbeddedlBrowserClient BrowserClient { get; }

        /// <summary>
        /// 初始化 <see cref="EmbeddedBrowserDisplayHandler"/> 实例。
        /// </summary>
        /// <param name="browserClient">嵌入式浏览器客户端。</param>
        public EmbeddedBrowserDisplayHandler(EmbeddedlBrowserClient browserClient)
        {
            BrowserClient = browserClient;
        }

        /// <summary>
        /// 标题变化回调:更新浏览器窗口标题。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="title">新标题。</param>
        protected override void OnTitleChange(CefBrowser browser, string title)
        {
            BrowserClient.BrowserWindow.Text = $"{title} - Robot Browser";
        }

        /// <summary>
        /// 地址变化回调:显示加载中标题。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="url">新地址。</param>
        protected override void OnAddressChange(CefBrowser browser, CefFrame frame, string url)
        {
            BrowserClient.BrowserWindow.Text = $"Loading... - Robot Browser";
        }

        /// <summary>
        /// 全屏模式变化回调:切换浏览器窗口边框与窗口状态。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="fullscreen">是否全屏。</param>
        protected override void OnFullscreenModeChange(CefBrowser browser, bool fullscreen)
        {
            var browserWindow = BrowserClient.BrowserWindow;

            if (browserWindow == null) return;

            if (fullscreen)
            {
                browserWindow.FormBorderStyle = FormBorderStyle.None;
                browserWindow.WindowState = FormWindowState.Maximized;
            }
            else
            {
                browserWindow.FormBorderStyle = FormBorderStyle.Sizable;
                browserWindow.WindowState = FormWindowState.Normal;
            }
        }
    }
}
