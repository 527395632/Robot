// Robot 桌面软件 — 图标 URL 变化事件参数
// 对应 CEF OnFaviconUrlChange 回调,携带图标 URL 列表

using System;
using Xilium.CefGlue;

namespace Robot
{

    /// <summary>
    /// 图标 URL 变化事件参数(对应 CEF OnFaviconUrlChange 回调),携带图标 URL 列表。
    /// </summary>
    public class FaviconUrlChangeEventArgs : EventArgs
    {
        /// <summary>
        /// 初始化 <see cref="FaviconUrlChangeEventArgs"/> 实例。
        /// </summary>
        public FaviconUrlChangeEventArgs(CefBrowser browser, string[] urls)
        {
            Browser = browser;
            Urls = urls;
        }

        /// <summary>
        /// 触发事件的浏览器。
        /// </summary>
        public CefBrowser Browser { get; }
        /// <summary>
        /// 图标 URL 列表。
        /// </summary>
        public string[] Urls { get; }
    }
}
