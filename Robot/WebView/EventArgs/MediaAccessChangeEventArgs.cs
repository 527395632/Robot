// Robot 桌面软件 — 媒体访问变化事件参数
// 对应 CEF OnMediaAccessChange 回调,携带视频/音频访问状态

using System;
using Xilium.CefGlue;

namespace Robot
{

    /// <summary>
    /// 媒体访问变化事件参数(对应 CEF OnMediaAccessChange 回调)。
    /// 携带视频/音频访问状态。
    /// </summary>
    public class MediaAccessChangeEventArgs : EventArgs
    {
        /// <summary>
        /// 初始化 <see cref="MediaAccessChangeEventArgs"/> 实例。
        /// </summary>
        public MediaAccessChangeEventArgs(CefBrowser browser, bool hasVideoAccess, bool hasAudioAccess)
        {
            Browser = browser;
            HasVideoAccess = hasVideoAccess;
            HasAudioAccess = hasAudioAccess;
        }

        /// <summary>
        /// 触发事件的浏览器。
        /// </summary>
        public CefBrowser Browser { get; }
        /// <summary>
        /// 是否拥有视频访问权限。
        /// </summary>
        public bool HasVideoAccess { get; }
        /// <summary>
        /// 是否拥有音频访问权限。
        /// </summary>
        public bool HasAudioAccess { get; }
    }
}
