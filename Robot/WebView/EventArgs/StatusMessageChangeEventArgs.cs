// Robot 桌面软件 — 状态栏消息变化事件参数
// 对应 CEF OnStatusMessage 回调,携带状态栏消息

using System;
using Xilium.CefGlue;

namespace Robot
{

    /// <summary>
    /// 状态栏消息变化事件参数(对应 CEF OnStatusMessage 回调),携带状态栏消息。
    /// </summary>
    public class StatusMessageChangeEventArgs : EventArgs
    {
        /// <summary>
        /// 触发事件的浏览器。
        /// </summary>
        public CefBrowser Browser { get; }
        /// <summary>
        /// 状态栏消息。
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// 初始化 <see cref="StatusMessageChangeEventArgs"/> 实例。
        /// </summary>
        public StatusMessageChangeEventArgs(CefBrowser browser, string value)
        {
            Browser = browser;
            Message = value;
        }
    }
}
