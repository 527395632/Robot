// Robot 桌面软件 — 框架地址变化事件参数
// 对应 CEF OnFrameAddressChange 回调,携带框架与地址

using System;
using Xilium.CefGlue;

namespace Robot
{

    /// <summary>
    /// 框架地址变化事件参数(对应 CEF OnFrameAddressChange 回调),携带框架与地址。
    /// </summary>
    public class FramePageAddressChangeEventArgs : EventArgs
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
        /// 初始化 <see cref="FramePageAddressChangeEventArgs"/> 实例。
        /// </summary>
        public FramePageAddressChangeEventArgs(CefBrowser browser, CefFrame frame, string address)
        {
            Browser = browser;
            Frame = frame;
            Address = address;
        }
    }
}
