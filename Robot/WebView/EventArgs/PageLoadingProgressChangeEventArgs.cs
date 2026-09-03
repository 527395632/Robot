// Robot 桌面软件 — 页面加载进度变化事件参数
// 对应 CEF OnLoadingProgressChange 回调,携带加载进度

using System;
using Xilium.CefGlue;

namespace Robot
{

    /// <summary>
    /// 页面加载进度变化事件参数(对应 CEF OnLoadingProgressChange 回调),携带加载进度。
    /// </summary>
    public class PageLoadingProgressChangeEventArgs : EventArgs
    {
        /// <summary>
        /// 初始化 <see cref="PageLoadingProgressChangeEventArgs"/> 实例。
        /// </summary>
        public PageLoadingProgressChangeEventArgs(CefBrowser browser, decimal progress)
        {
            Progress = progress;
        }

        /// <summary>
        /// 加载进度(0~1)。
        /// </summary>
        public decimal Progress { get; }
    }
}
