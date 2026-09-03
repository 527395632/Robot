// Robot 桌面软件 — 嵌入式浏览器下载处理器
// 处理嵌入式浏览器的下载请求

using Xilium.CefGlue;

namespace Robot.Browser.EmbeddedBrowser
{

    /// <summary>
    /// 嵌入式浏览器下载处理器:处理嵌入式浏览器的下载请求。
    /// </summary>
    internal class EmbeddedBrowserDownloadHandler : CefDownloadHandler
    {
        /// <summary>
        /// 嵌入式浏览器客户端。
        /// </summary>
        public EmbeddedlBrowserClient BrowserClient { get; }

        /// <summary>
        /// 初始化 <see cref="EmbeddedBrowserDownloadHandler"/> 实例。
        /// </summary>
        /// <param name="browserClient">嵌入式浏览器客户端。</param>
        public EmbeddedBrowserDownloadHandler(EmbeddedlBrowserClient browserClient)
        {
            BrowserClient = browserClient;
        }

        /// <summary>
        /// 下载前回调:继续下载。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="downloadItem">下载项。</param>
        /// <param name="suggestedName">建议文件名。</param>
        /// <param name="callback">下载前回调。</param>
        protected override void OnBeforeDownload(CefBrowser browser, CefDownloadItem downloadItem, string suggestedName, CefBeforeDownloadCallback callback)
        {
            callback.Continue(suggestedName, true);
        }
    }
}
