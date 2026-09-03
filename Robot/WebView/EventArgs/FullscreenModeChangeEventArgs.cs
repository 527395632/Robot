// Robot 桌面软件 — 全屏模式变化事件参数
// 对应 CEF OnFullscreenModeChange 回调,携带是否全屏,可设置 Cancel 取消

using System;
using Xilium.CefGlue;

namespace Robot
{

    /// <summary>
    /// 全屏模式变化事件参数(对应 CEF OnFullscreenModeChange 回调)。
    /// 携带是否全屏,可设置 <see cref="Cancel"/> 取消。
    /// </summary>
    public class FullscreenModeChangeEventArgs : EventArgs
    {
        /// <summary>
        /// 触发事件的浏览器。
        /// </summary>
        public CefBrowser Browser { get; }
        /// <summary>
        /// 是否全屏。
        /// </summary>
        public bool Fullscreen { get; }

        /// <summary>
        /// 初始化 <see cref="FullscreenModeChangeEventArgs"/> 实例。
        /// </summary>
        public FullscreenModeChangeEventArgs(CefBrowser browser, bool fullscreen)
        {
            Browser = browser;
            Fullscreen = fullscreen;
        }

        /// <summary>
        /// 是否取消。
        /// </summary>
        public bool Cancel { get; set; } = false;
    }
}
