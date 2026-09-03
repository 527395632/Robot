// Robot 桌面软件 — 下载前事件参数
// 对应 CEF OnBeforeDownload 回调,携带下载项与建议文件名,可通过回调确认/取消下载

using System;
using Xilium.CefGlue;

namespace Robot
{

    /// <summary>
    /// 下载前事件参数(对应 CEF OnBeforeDownload 回调)。
    /// 携带下载项与建议文件名,可通过 <see cref="Callback"/> 确认或取消下载。
    /// </summary>
    public class BeforeDownloadEventArgs : EventArgs
    {
        /// <summary>
        /// 初始化 <see cref="BeforeDownloadEventArgs"/> 实例。
        /// </summary>
        public BeforeDownloadEventArgs(CefBrowser browser, CefDownloadItem item, string suggestedName, CefBeforeDownloadCallback callback)
        {
            Browser = browser;
            Item = item;
            SuggestedName = suggestedName;
            Callback = callback;
        }

        /// <summary>
        /// 触发事件的浏览器。
        /// </summary>
        public CefBrowser Browser { get; }
        /// <summary>
        /// 下载项。
        /// </summary>
        public CefDownloadItem Item { get; }
        /// <summary>
        /// 建议的文件名。
        /// </summary>
        public string SuggestedName { get; }
        /// <summary>
        /// 下载回调(用于确认或取消)。
        /// </summary>
        public CefBeforeDownloadCallback Callback { get; }
    }
}
