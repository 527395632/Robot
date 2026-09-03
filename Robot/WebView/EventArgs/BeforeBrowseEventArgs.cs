// Robot 桌面软件 — 导航前事件参数
// 对应 CEF OnBeforeBrowse 回调,携带目标框架/请求等信息,可设置 Handled 取消导航

using System;
using Xilium.CefGlue;

namespace Robot
{

    /// <summary>
    /// 导航前事件参数(对应 CEF OnBeforeBrowse 回调)。
    /// 携带目标框架/请求等信息,可设置 <see cref="Handled"/> 取消本次导航。
    /// </summary>
    public class BeforeBrowseEventArgs : EventArgs
    {
        /// <summary>
        /// 初始化 <see cref="BeforeBrowseEventArgs"/> 实例。
        /// </summary>
        public BeforeBrowseEventArgs(CefBrowser browser, CefFrame frame, CefRequest request, bool userGesture, bool isRedirect)
        {
            Browser = browser;
            Frame = frame;
            Request = request;
            UserGesture = userGesture;
            IsRedirect = isRedirect;
        }

        /// <summary>
        /// 触发事件的浏览器。
        /// </summary>
        public CefBrowser Browser { get; }
        /// <summary>
        /// 目标框架。
        /// </summary>
        public CefFrame Frame { get; }
        /// <summary>
        /// 导航请求。
        /// </summary>
        public CefRequest Request { get; }
        /// <summary>
        /// 是否由用户手势触发。
        /// </summary>
        public bool UserGesture { get; }
        /// <summary>
        /// 是否为重定向。
        /// </summary>
        public bool IsRedirect { get; }

        /// <summary>
        /// 是否已处理(设为 true 可取消本次导航)。
        /// </summary>
        public bool Handled { get; set; }
    }
}
