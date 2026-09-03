// Robot 桌面软件 — 进程消息接收事件参数
// 承载进程间消息接收事件的上下文(浏览器/帧/源进程/消息)

using Xilium.CefGlue;

namespace Robot.Browser
{

    /// <summary>
    /// 进程消息接收事件参数:承载进程间消息接收事件的上下文。
    /// </summary>
    internal class ProcessMessageReceivedEventArgs
    {
        /// <summary>
        /// 触发事件的浏览器。
        /// </summary>
        public CefBrowser Browser { get; }

        /// <summary>
        /// 触发事件的帧。
        /// </summary>
        public CefFrame Frame { get; }

        /// <summary>
        /// 源进程。
        /// </summary>
        public CefProcessId ProcessId { get; }

        /// <summary>
        /// 进程消息。
        /// </summary>
        public CefProcessMessage Message { get; }

        /// <summary>
        /// 初始化 <see cref="ProcessMessageReceivedEventArgs"/> 实例。
        /// </summary>
        /// <param name="browser">触发事件的浏览器。</param>
        /// <param name="frame">触发事件的帧。</param>
        /// <param name="processId">源进程。</param>
        /// <param name="message">进程消息。</param>
        public ProcessMessageReceivedEventArgs(CefBrowser browser, CefFrame frame, CefProcessId processId, CefProcessMessage message)
        {
            Browser = browser;
            Frame = frame;
            ProcessId = processId;
            Message = message;
        }
    }
}
