// Robot 桌面软件 — 进程间消息桥
// 通过命名管道在浏览器进程与渲染进程间转发桥接消息,并分发到已注册的处理器

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Robot;
using Xilium.CefGlue;

namespace Robot.Browser
{

    /// <summary>
    /// 进程间消息桥:通过命名管道在浏览器进程与渲染进程间转发桥接消息,并分发到已注册的处理器。
    /// </summary>
    internal sealed class MessageBridge : IDisposable
    {
        /// <summary>
        /// 已注册的消息桥处理器集合。
        /// </summary>
        List<MessageBridgeHandler> MessageBridgeHandlers { get; } = new();

        /// <summary>
        /// 关联的浏览器。
        /// </summary>
        public CefBrowser Browser { get; }

        /// <summary>
        /// 是否渲染进程侧。
        /// </summary>
        public bool IsRenderer { get; }

        /// <summary>
        /// 进程消息分发器。
        /// </summary>
        public ProcessMessageDispatcher MessageDispatcher { get; }

        /// <summary>
        /// 命名管道服务端(仅主进程侧创建)。
        /// </summary>
        internal MessageBridgePipeServer? Pipe { get; }

        /// <summary>
        /// 初始化 <see cref="MessageBridge"/> 实例。
        /// </summary>
        /// <param name="browser">关联的浏览器。</param>
        /// <param name="isRenderer">是否渲染进程侧。</param>
        /// <param name="messageDispatcher">进程消息分发器。</param>
        public MessageBridge(CefBrowser browser, bool isRenderer, ProcessMessageDispatcher messageDispatcher)
        {
            Browser = browser;
            IsRenderer = isRenderer;
            MessageDispatcher = messageDispatcher;
            MessageDispatcher.RegisterMessageHandler("Robot.MessageBridgeMessage", OnMessageBridgeCommunicateCore);

            if (!isRenderer)
            {
                Pipe = new MessageBridgePipeServer(this, GetPipeName(browser.Identifier));
            }
        }

        /// <summary>
        /// 生成命名管道名称。
        /// </summary>
        /// <param name="browserId">浏览器标识。</param>
        /// <returns>命名管道名称。</returns>
        internal static string GetPipeName(int browserId)
        {
            int processId;

            if (!Robot.App.Program.IsRenderer)
            {
                processId = System.Diagnostics.Process.GetCurrentProcess().Id;
            }
            else
            {
                processId = Robot.App.Program.BrowserProcessId;
            }

            return $"Robot-MessageBridgeProxy-{processId}-{browserId}";
        }

        /// <summary>
        /// 注册消息桥处理器(通过类型实例化)。
        /// </summary>
        /// <typeparam name="T">处理器类型。</typeparam>
        public void RegisterMessageBridgeHandler<T>() where T : MessageBridgeHandler, new()
        {
            var type = typeof(T);

            var handler = Activator.CreateInstance(type, this) as MessageBridgeHandler;

            if (handler == null) throw new TypeInitializationException(type.FullName, null);

            MessageBridgeHandlers.Add(handler);
        }

        /// <summary>
        /// 注册消息桥处理器(传入实例)。
        /// </summary>
        /// <param name="handler">处理器实例。</param>
        public void RegisterMessageBridgeHandler(MessageBridgeHandler handler)
        {
            MessageBridgeHandlers.Add(handler);
        }

        /// <summary>
        /// 处理桥接消息通信:解析 JSON 消息并分发到对应处理器。
        /// </summary>
        /// <param name="args">进程消息接收事件参数。</param>
        private void OnMessageBridgeCommunicateCore(ProcessMessageReceivedEventArgs args)
        {
            var msgs = args.Message.Arguments!;

            var buff = msgs.GetBinary(0).ToArray();

            var json = Encoding.Unicode.GetString(buff);

            var message = BridgeMessage.FromJson(json);

            if (message != null)
            {
                var handlers = MessageBridgeHandlers.SelectMany(x => x.BridgeMessageHandlers).ToDictionary(k => k.Key, v => v.Value);

                if (handlers.TryGetValue(message.Name, out var handler))
                {
                    handler.Invoke(args.Browser, args.Frame, args.ProcessId, message);
                }
            }
        }

        /// <summary>
        /// 向本地(浏览器进程)发送消息。
        /// </summary>
        /// <param name="frame">目标帧。</param>
        /// <param name="message">桥接消息。</param>
        internal static void SendMessageToLocal(CefFrame frame, BridgeMessage message)
        {
            SendMessage(CefProcessId.Browser, frame, message);
        }

        /// <summary>
        /// 向远端(渲染进程)发送消息。
        /// </summary>
        /// <param name="frame">目标帧。</param>
        /// <param name="message">桥接消息。</param>
        internal static void SendMessageToRemote(CefFrame frame, BridgeMessage message)
        {
            SendMessage(CefProcessId.Renderer, frame, message);
        }

        /// <summary>
        /// 向指定进程发送消息。
        /// </summary>
        /// <param name="processId">目标进程。</param>
        /// <param name="frame">目标帧。</param>
        /// <param name="message">桥接消息。</param>
        internal static void SendMessage(CefProcessId processId, CefFrame frame, BridgeMessage message)
        {
            var msg = CefProcessMessage.Create("Robot.MessageBridgeMessage");
            var json = message.ToJson();
            var buff = Encoding.Unicode.GetBytes(json);
            msg.Arguments!.SetBinary(0, CefBinaryValue.Create(buff));

            frame.SendProcessMessage(processId, msg);
        }

        /// <summary>
        /// 异步执行请求(通过命名管道客户端)。
        /// </summary>
        /// <param name="request">桥接请求。</param>
        /// <returns>桥接响应。</returns>
        internal static async Task<MessageBridgeResponse> ExecuteRequestAsync(MessageBridgeRequest request)
        {
            var client = new MessageBridgePipeClient(GetPipeName(request.BrowserId));

            return await client.RequestAsync(request);
        }

        /// <summary>
        /// 同步执行请求。
        /// </summary>
        /// <param name="request">桥接请求。</param>
        /// <returns>桥接响应。</returns>
        internal static MessageBridgeResponse ExecuteRequest(MessageBridgeRequest request)
        {
            return ExecuteRequestAsync(request).GetAwaiter().GetResult();
        }

        /// <summary>
        /// 处理收到的桥接请求,分发到对应处理器。
        /// </summary>
        /// <param name="request">桥接请求。</param>
        /// <returns>桥接响应,无匹配处理器时为 null。</returns>
        internal MessageBridgeResponse? OnMessageBridgeRequestReviced(MessageBridgeRequest request)
        {
            var MessageRequestHandlers = MessageBridgeHandlers.SelectMany(x => x.BridgeRequestHandlers).ToDictionary(k => k.Key, v => v.Value);

            if (MessageRequestHandlers.TryGetValue(request.Name, out var handler))
            {
                return handler.Invoke(request);
            }

            return null;
        }

        /// <summary>
        /// 释放资源。
        /// </summary>
        public void Dispose()
        {
            Pipe?.Dispose();
        }

        /// <summary>
        /// 远端上下文创建后回调,转发给所有处理器。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="context">V8 上下文。</param>
        internal void OnContextCreated(CefBrowser browser, CefFrame frame, CefV8Context context)
        {
            MessageBridgeHandlers.ForEach(handler => handler.OnRemoteContextCreated(browser, frame, context));
        }

        /// <summary>
        /// 远端上下文释放后回调,转发给所有处理器。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="context">V8 上下文。</param>
        internal void OnContextReleased(CefBrowser browser, CefFrame frame, CefV8Context context)
        {
            MessageBridgeHandlers.ForEach(handler => handler.OnRemoteContextReleased(browser, frame, context));
        }

        /// <summary>
        /// 浏览前回调,转发给所有处理器。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="request">请求。</param>
        /// <param name="userGesture">是否用户手势触发。</param>
        /// <param name="isRedirect">是否为重定向。</param>
        internal void OnBeforeBrowse(CefBrowser browser, CefFrame frame, CefRequest request, bool userGesture, bool isRedirect)
        {
            MessageBridgeHandlers.ForEach(handler => handler.OnBeforeBrowse(browser, frame, request, userGesture, isRedirect));
        }

        /// <summary>
        /// 关闭前回调,转发给所有处理器。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        internal void OnBeforeClose(CefBrowser browser)
        {
            MessageBridgeHandlers.ForEach(handler => handler.OnBeforeClose(browser));
        }

        /// <summary>
        /// 渲染进程终止回调,转发给所有处理器。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        internal void OnRenderProcessTerminated(CefBrowser browser)
        {
            MessageBridgeHandlers.ForEach(handler => handler.OnRenderProcessTerminated(browser));
        }
    }
}
