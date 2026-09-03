// Robot 桌面软件 — 下载更新事件参数
// 对应 CEF OnDownloadUpdated 回调,携带下载项与回调

using System;
using Xilium.CefGlue;

namespace Robot
{

    /// <summary>
    /// 下载更新事件参数(对应 CEF OnDownloadUpdated 回调),携带下载项与回调。
    /// </summary>
    public class DownloadUpdatedEventArgs : EventArgs
    {
        /// <summary>
        /// 初始化 <see cref="DownloadUpdatedEventArgs"/> 实例。
        /// </summary>
        public DownloadUpdatedEventArgs(CefBrowser browser, CefDownloadItem item, CefDownloadItemCallback callback)
        {
            Browser = browser;
            Item = item;
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
        /// 下载项回调。
        /// </summary>
        public CefDownloadItemCallback Callback { get; }
    }
}
