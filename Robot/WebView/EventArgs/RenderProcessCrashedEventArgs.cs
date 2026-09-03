// Robot 桌面软件 — 渲染进程崩溃事件参数
// 对应 CEF OnRenderProcessTerminated 回调,携带终止状态,可设置 RestartProcess 重启

using System;
using Xilium.CefGlue;

namespace Robot
{

    /// <summary>
    /// 渲染进程崩溃事件参数(对应 CEF OnRenderProcessTerminated 回调)。
    /// 携带终止状态,可设置 <see cref="RestartProcess"/> 重启进程。
    /// </summary>
    public class RenderProcessCrashedEventArgs : EventArgs
    {
        /// <summary>
        /// 触发事件的浏览器。
        /// </summary>
        public CefBrowser Browser { get; }
        /// <summary>
        /// 终止状态。
        /// </summary>
        public CefTerminationStatus Status { get; }

        /// <summary>
        /// 初始化 <see cref="RenderProcessCrashedEventArgs"/> 实例。
        /// </summary>
        public RenderProcessCrashedEventArgs(CefBrowser browser, CefTerminationStatus status)
        {
            Browser = browser;
            Status = status;
        }

        /// <summary>
        /// 是否重启进程。
        /// </summary>
        public bool RestartProcess { get; set; } = false;
    }
}
