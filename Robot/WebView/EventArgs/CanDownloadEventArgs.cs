// Robot 桌面软件 — 是否允许下载事件参数
// 携带目标 URL 与请求方法,可设置 AllowDownload 控制是否允许下载

using System;
using Xilium.CefGlue;

namespace Robot
{

    /// <summary>
    /// 是否允许下载事件参数,携带目标 URL 与请求方法。
    /// 可设置 <see cref="AllowDownload"/> 控制是否允许下载。
    /// </summary>
    public class CanDownloadEventArgs : EventArgs
    {
        /// <summary>
        /// 初始化 <see cref="CanDownloadEventArgs"/> 实例。
        /// </summary>
        public CanDownloadEventArgs(CefBrowser browser, string url, string requestMethod)
        {
            Browser = browser;
            Url = url;
            RequestMethod = requestMethod;
        }

        /// <summary>
        /// 触发事件的浏览器。
        /// </summary>
        public CefBrowser Browser { get; }
        /// <summary>
        /// 目标 URL。
        /// </summary>
        public string Url { get; }
        /// <summary>
        /// 请求方法。
        /// </summary>
        public string RequestMethod { get; }

        /// <summary>
        /// 是否允许下载。
        /// </summary>
        public bool AllowDownload { get; set; } = true;
    }
}
