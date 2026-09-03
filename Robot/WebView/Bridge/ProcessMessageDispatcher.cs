// Robot 桌面软件 — 进程消息分发器
// 按消息名称注册处理器,并将收到的进程消息分发到对应处理器

using System;
using System.Collections.Generic;
using Xilium.CefGlue;

namespace Robot.Browser
{

    /// <summary>
    /// 进程消息分发器:按消息名称注册处理器,并将收到的进程消息分发到对应处理器。
    /// </summary>
    internal class ProcessMessageDispatcher
    {
        /// <summary>
        /// 消息处理器集合(按消息名称索引)。
        /// </summary>
        private readonly Dictionary<string, Action<ProcessMessageReceivedEventArgs>> _messageHandlers = new();

        /// <summary>
        /// 分发进程消息到对应处理器。
        /// </summary>
        /// <param name="browser">触发事件的浏览器。</param>
        /// <param name="frame">触发事件的帧。</param>
        /// <param name="sourceProcess">源进程。</param>
        /// <param name="message">进程消息。</param>
        internal void DispatchMessage(CefBrowser browser, CefFrame frame, CefProcessId sourceProcess, CefProcessMessage message)
        {
            if (_messageHandlers.TryGetValue(message.Name, out var existingHandler))
            {
                existingHandler(new ProcessMessageReceivedEventArgs(browser, frame, sourceProcess, message));
            }
        }

        /// <summary>
        /// 向目标进程发送消息。
        /// </summary>
        /// <param name="targetProcess">目标进程。</param>
        /// <param name="frame">目标帧。</param>
        /// <param name="message">进程消息。</param>
        public void SendMessage(CefProcessId targetProcess, CefFrame frame, CefProcessMessage message)
        {
            frame.SendProcessMessage(targetProcess, message);
        }

        /// <summary>
        /// 注册消息处理器(同一名称可叠加多个处理器)。
        /// </summary>
        /// <param name="messageName">消息名称。</param>
        /// <param name="handler">消息处理委托。</param>
        public void RegisterMessageHandler(string messageName, Action<ProcessMessageReceivedEventArgs> handler)
        {
            _messageHandlers.TryGetValue(messageName, out var existingHandler);
            _messageHandlers[messageName] = existingHandler + handler;
        }
    }
}
