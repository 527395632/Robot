// Robot 桌面软件 — 框架加载开始事件参数
// 对应 CEF OnFrameLoadStart 回调,携带框架与过渡类型

using System;
using Xilium.CefGlue;

namespace Robot
{

    /// <summary>
    /// 框架加载开始事件参数(对应 CEF OnFrameLoadStart 回调),携带框架与过渡类型。
    /// </summary>
    public class FramePageLoadStartEventArgs : EventArgs
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
        /// 过渡类型。
        /// </summary>
        public CefTransitionType TransitionType { get; }

        /// <summary>
        /// 初始化 <see cref="FramePageLoadStartEventArgs"/> 实例。
        /// </summary>
        public FramePageLoadStartEventArgs(CefBrowser browser, CefFrame frame, CefTransitionType transitionType)
        {
            Browser = browser;
            Frame = frame;
            TransitionType = transitionType;
        }
    }
}
