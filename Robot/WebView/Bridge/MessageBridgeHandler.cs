// Robot 桌面软件 — 消息桥处理器基类
// 提供注册请求/消息处理器与跨进程通信的基类,子类实现具体的 CEF 回调

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Robot;
using Xilium.CefGlue;

namespace Robot.Browser
{

    /// <summary>
    /// 消息桥处理器基类:提供注册请求/消息处理器与跨进程通信的能力,子类实现具体的 CEF 回调。
    /// </summary>
    internal abstract class MessageBridgeHandler
    {
        /// <summary>
        /// 请求处理器集合(按名称索引)。
        /// </summary>
        internal Dictionary<string, Func<MessageBridgeRequest, MessageBridgeResponse>> BridgeRequestHandlers { get; } = new();

        /// <summary>
        /// 消息处理器集合(按名称索引)。
        /// </summary>
        internal Dictionary<string, Action<CefBrowser, CefFrame, CefProcessId, BridgeMessage>> BridgeMessageHandlers { get; } = new();

        /// <summary>
        /// 关联的消息桥。
        /// </summary>
        protected MessageBridge Bridge { get; }

        /// <summary>
        /// 是否渲染进程侧。
        /// </summary>
        protected bool IsRenderer => Bridge.IsRenderer;

        /// <summary>
        /// 初始化 <see cref="MessageBridgeHandler"/> 实例。
        /// </summary>
        /// <param name="bridge">关联的消息桥。</param>
        public MessageBridgeHandler(MessageBridge bridge)
        {
            Bridge = bridge;
        }

        /// <summary>
        /// 注册请求处理器。
        /// </summary>
        /// <param name="name">处理器名称。</param>
        /// <param name="handler">请求处理委托。</param>
        protected void RegisterRequestHandler(string name, Func<MessageBridgeRequest, MessageBridgeResponse> handler)
        {
            BridgeRequestHandlers[name] = handler;
        }

        /// <summary>
        /// 注册消息处理器。
        /// </summary>
        /// <param name="name">处理器名称。</param>
        /// <param name="handler">消息处理委托。</param>
        protected void RegisterMessageHandler(string name, Action<CefBrowser, CefFrame, CefProcessId, BridgeMessage> handler)
        {
            BridgeMessageHandlers[name] = handler;
        }

        /// <summary>
        /// 向本地(浏览器进程)发送消息。
        /// </summary>
        /// <param name="frame">目标帧。</param>
        /// <param name="message">桥接消息。</param>
        public void SendMessageToLocal(CefFrame frame, BridgeMessage message)
        {
            MessageBridge.SendMessageToLocal(frame, message);
        }

        /// <summary>
        /// 向远端(渲染进程)发送消息。
        /// </summary>
        /// <param name="frame">目标帧。</param>
        /// <param name="message">桥接消息。</param>
        public void SendMessageToRemote(CefFrame frame, BridgeMessage message)
        {
            MessageBridge.SendMessageToRemote(frame, message);
        }

        /// <summary>
        /// 同步执行请求。
        /// </summary>
        /// <param name="request">桥接请求。</param>
        /// <returns>桥接响应。</returns>
        public MessageBridgeResponse ExecuteRequest(MessageBridgeRequest request)
        {
            return MessageBridge.ExecuteRequest(request);
        }

        /// <summary>
        /// 异步执行请求。
        /// </summary>
        /// <param name="request">桥接请求。</param>
        /// <returns>桥接响应。</returns>
        public Task<MessageBridgeResponse> ExecuteRequestAsync(MessageBridgeRequest request)
        {
            return MessageBridge.ExecuteRequestAsync(request);
        }

        /// <summary>
        /// 浏览前回调。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="request">请求。</param>
        /// <param name="userGesture">是否用户手势触发。</param>
        /// <param name="isRedirect">是否为重定向。</param>
        public abstract void OnBeforeBrowse(CefBrowser browser, CefFrame frame, CefRequest request, bool userGesture, bool isRedirect);

        /// <summary>
        /// 关闭前回调。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        public abstract void OnBeforeClose(CefBrowser browser);

        /// <summary>
        /// 渲染进程终止回调。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        public abstract void OnRenderProcessTerminated(CefBrowser browser);

        /// <summary>
        /// 远端上下文创建后回调。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="context">V8 上下文。</param>
        public abstract void OnRemoteContextCreated(CefBrowser browser, CefFrame frame, CefV8Context context);

        /// <summary>
        /// 远端上下文释放后回调。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="context">V8 上下文。</param>
        public abstract void OnRemoteContextReleased(CefBrowser browser, CefFrame frame, CefV8Context context);
    }
}
