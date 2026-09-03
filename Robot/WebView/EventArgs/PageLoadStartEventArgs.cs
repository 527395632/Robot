// Robot 桌面软件 — 页面加载开始事件参数
// 对应 CEF OnLoadStart 回调,携带框架与过渡类型

using System;
using Xilium.CefGlue;

namespace Robot
{

    /// <summary>
    /// 页面加载开始事件参数(对应 CEF OnLoadStart 回调),携带框架与过渡类型。
    /// </summary>
    public class PageLoadStartEventArgs : EventArgs
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
        /// 初始化 <see cref="PageLoadStartEventArgs"/> 实例。
        /// </summary>
        public PageLoadStartEventArgs(CefBrowser browser, CefFrame frame, CefTransitionType transitionType)
        {
            Browser = browser;
            Frame = frame;
            TransitionType = transitionType;
        }
    }
}
