// Robot 桌面软件 — 从标签页打开 URL 事件参数
// 对应 CEF OnOpenUrlFromTab 回调,携带目标 URL/打开方式/是否用户手势,可设置 Cancel 取消

using System;
using Xilium.CefGlue;

namespace Robot
{

    /// <summary>
    /// 从标签页打开 URL 事件参数(对应 CEF OnOpenUrlFromTab 回调)。
    /// 携带目标 URL/打开方式/是否用户手势,可设置 <see cref="Cancel"/> 取消。
    /// </summary>
    public class OpenUrlFromTabEventArgs : EventArgs
    {
        /// <summary>
        /// 初始化 <see cref="OpenUrlFromTabEventArgs"/> 实例。
        /// </summary>
        public OpenUrlFromTabEventArgs(CefBrowser browser, CefFrame frame, string targetUrl, CefWindowOpenDisposition targetDisposition, bool userGesture)
        {
            TargetUrl = targetUrl;
            TargetDisposition = targetDisposition;
            UserGesture = userGesture;
        }

        /// <summary>
        /// 目标 URL。
        /// </summary>
        public string TargetUrl { get; }
        /// <summary>
        /// 目标打开方式。
        /// </summary>
        public CefWindowOpenDisposition TargetDisposition { get; }
        /// <summary>
        /// 是否由用户手势触发。
        /// </summary>
        public bool UserGesture { get; }

        /// <summary>
        /// 是否取消。
        /// </summary>
        public bool Cancel { get; set; } = false;
    }
}
