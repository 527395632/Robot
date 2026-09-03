// Robot 桌面软件 — 页面加载状态变化事件参数
// 对应 CEF OnLoadingStateChange 回调,携带是否加载中/可否后退/可否前进

using System;
using Xilium.CefGlue;

namespace Robot
{

    /// <summary>
    /// 页面加载状态变化事件参数(对应 CEF OnLoadingStateChange 回调)。
    /// 携带是否加载中/可否后退/可否前进。
    /// </summary>
    public class PageLoadingStateChangeEventArgs : EventArgs
    {
        /// <summary>
        /// 触发事件的浏览器。
        /// </summary>
        public CefBrowser Browser { get; }
        /// <summary>
        /// 是否正在加载。
        /// </summary>
        public bool IsLoading { get; }
        /// <summary>
        /// 是否可后退。
        /// </summary>
        public bool CanGoBack { get; }
        /// <summary>
        /// 是否可前进。
        /// </summary>
        public bool CanGoForward { get; }

        /// <summary>
        /// 初始化 <see cref="PageLoadingStateChangeEventArgs"/> 实例。
        /// </summary>
        public PageLoadingStateChangeEventArgs(CefBrowser browser, bool isLoading, bool canGoBack, bool canGoForward)
        {
            Browser = browser;
            IsLoading = isLoading;
            CanGoBack = canGoBack;
            CanGoForward = canGoForward;
        }
    }
}
