// Robot 桌面软件 — 嵌入式浏览器客户端
// 承载嵌入式浏览器窗口的 CEF 客户端,装配各处理器

using Xilium.CefGlue;

namespace Robot.Browser.EmbeddedBrowser
{

    /// <summary>
    /// 嵌入式浏览器客户端:承载嵌入式浏览器窗口的 CEF 客户端,装配各处理器。
    /// </summary>
    internal class EmbeddedlBrowserClient : CefClient
    {
        /// <summary>
        /// 嵌入式浏览器窗口。
        /// </summary>
        public EmbeddedBrowserWindow BrowserWindow { get; }

        /// <summary>
        /// 初始化 <see cref="EmbeddedlBrowserClient"/> 实例。
        /// </summary>
        /// <param name="browserWindow">嵌入式浏览器窗口。</param>
        public EmbeddedlBrowserClient(EmbeddedBrowserWindow browserWindow)
        {
            BrowserWindow = browserWindow;
        }

        /// <summary>
        /// 获取生命周期处理器。
        /// </summary>
        /// <returns>生命周期处理器。</returns>
        protected override CefLifeSpanHandler? GetLifeSpanHandler()
        {
            return new EmbeddedBrowserLifeSpanHandler(this);
        }

        /// <summary>
        /// 获取显示处理器。
        /// </summary>
        /// <returns>显示处理器。</returns>
        protected override CefDisplayHandler? GetDisplayHandler()
        {
            return new EmbeddedBrowserDisplayHandler(this);
        }

        /// <summary>
        /// 获取下载处理器。
        /// </summary>
        /// <returns>下载处理器。</returns>
        protected override CefDownloadHandler? GetDownloadHandler()
        {
            return new EmbeddedBrowserDownloadHandler(this);
        }

        /// <summary>
        /// 获取右键菜单处理器。
        /// </summary>
        /// <returns>右键菜单处理器。</returns>
        protected override CefContextMenuHandler? GetContextMenuHandler()
        {
            return new EmbeddedBrowserContextMenuHandler(this);
        }
    }
}
