// Robot 桌面软件 — 页面地址变化事件参数
// 对应 CEF OnAddressChange 回调,携带框架与地址

using System;
using Xilium.CefGlue;

namespace Robot
{

    /// <summary>
    /// 页面地址变化事件参数(对应 CEF OnAddressChange 回调),携带框架与地址。
    /// </summary>
    public class PageAddressChangeEventArgs : EventArgs
    {
        /// <summary>
        /// 触发事件的浏览器。
        /// </summary>
        public CefBrowser Browser { get; }
        /// <summary>
        /// 目标框架。
        /// </summary>
        public CefFrame Frame { get; }
        /// <summary>
        /// 地址。
        /// </summary>
        public string Address { get; }

        /// <summary>
        /// 初始化 <see cref="PageAddressChangeEventArgs"/> 实例。
        /// </summary>
        public PageAddressChangeEventArgs(CefBrowser browser, CefFrame frame, string address)
        {
            Browser = browser;
            Frame = frame;
            Address = address;
        }
    }
}
