// Robot 桌面软件 — 页面标题变化事件参数
// 对应 CEF OnTitleChange 回调,携带新标题

using System;
using Xilium.CefGlue;

namespace Robot
{

    /// <summary>
    /// 页面标题变化事件参数(对应 CEF OnTitleChange 回调),携带新标题。
    /// </summary>
    public class PageTitleChangeEventArgs : EventArgs
    {
        /// <summary>
        /// 触发事件的浏览器。
        /// </summary>
        public CefBrowser Browser { get; }
        /// <summary>
        /// 新标题。
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// 初始化 <see cref="PageTitleChangeEventArgs"/> 实例。
        /// </summary>
        public PageTitleChangeEventArgs(CefBrowser browser, string title)
        {
            Browser = browser;
            Title = title;
        }
    }
}
