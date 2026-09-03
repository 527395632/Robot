// Robot 桌面软件 — 页面加载失败事件参数
// 对应 CEF OnLoadError 回调,携带框架/错误码/错误文本/失败 URL

using System;
using Xilium.CefGlue;

namespace Robot
{

    /// <summary>
    /// 页面加载失败事件参数(对应 CEF OnLoadError 回调),携带框架/错误码/错误文本/失败 URL。
    /// </summary>
    public class PageLoadErrorEventArgs : EventArgs
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
        /// 错误码。
        /// </summary>
        public CefErrorCode ErrorCode { get; }
        /// <summary>
        /// 错误文本。
        /// </summary>
        public string ErrorText { get; }
        /// <summary>
        /// 失败的 URL。
        /// </summary>
        public string FailedUrl { get; }

        /// <summary>
        /// 初始化 <see cref="PageLoadErrorEventArgs"/> 实例。
        /// </summary>
        public PageLoadErrorEventArgs(CefBrowser browser, CefFrame frame, CefErrorCode errorCode, string errorText, string failedUrl)
        {
            Browser = browser;
            Frame = frame;
            ErrorCode = errorCode;
            ErrorText = errorText;
            FailedUrl = failedUrl;
        }
    }
}
