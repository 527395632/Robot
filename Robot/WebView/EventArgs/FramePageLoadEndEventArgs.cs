// Robot 桌面软件 — 框架加载完成事件参数
// 对应 CEF OnFrameLoadEnd 回调,携带框架与 HTTP 状态码

using System;
using Xilium.CefGlue;

namespace Robot
{

    /// <summary>
    /// 框架加载完成事件参数(对应 CEF OnFrameLoadEnd 回调),携带框架与 HTTP 状态码。
    /// </summary>
    public class FramePageLoadEndEventArgs : EventArgs
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
        /// HTTP 状态码。
        /// </summary>
        public int HttpStatusCode { get; }

        /// <summary>
        /// 初始化 <see cref="FramePageLoadEndEventArgs"/> 实例。
        /// </summary>
        public FramePageLoadEndEventArgs(CefBrowser browser, CefFrame frame, int httpStatusCode)
        {
            Browser = browser;
            Frame = frame;
            HttpStatusCode = httpStatusCode;
        }
    }
}
