// Robot 桌面软件 — 浏览器事件参数基类
// 携带触发事件的 CefBrowser 实例,供各浏览器事件参数复用

using System;
using Xilium.CefGlue;

namespace Robot
{

    /// <summary>
    /// 浏览器事件参数基类,携带触发事件的 <see cref="CefBrowser"/> 实例。
    /// </summary>
    public class BrowserEventArgs : EventArgs
    {
        /// <summary>
        /// 初始化 <see cref="BrowserEventArgs"/> 实例。
        /// </summary>
        public BrowserEventArgs(CefBrowser browser)
        {
            Browser = browser;
        }

        /// <summary>
        /// 触发事件的浏览器。
        /// </summary>
        public CefBrowser Browser { get; }
    }
}
