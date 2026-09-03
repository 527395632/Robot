// Robot 桌面软件 — JavaScript Promise
// 表示跨进程的 JavaScript Promise,支持在远端解析(Resolve)或拒绝(Reject)

using Robot.Browser;
using System;
using Xilium.CefGlue;

namespace Robot.JavaScript
{

    /// <summary>
    /// JavaScript Promise:表示跨进程的 JavaScript Promise,支持在远端解析(Resolve)或拒绝(Reject)。
    /// </summary>
    public class JavaScriptPromise
    {
        /// <summary>
        /// 是否已被处理(解析或拒绝)。
        /// </summary>
        private bool _isHandled = false;

        /// <summary>
        /// 是否渲染进程侧。
        /// </summary>
        internal bool IsRenderer { get; }

        /// <summary>
        /// Promise 关联的帧。
        /// </summary>
        internal CefFrame Frame { get; }

        /// <summary>
        /// Promise 唯一标识。
        /// </summary>
        internal Guid Uuid { get; }

        /// <summary>
        /// 初始化 <see cref="JavaScriptPromise"/> 实例。
        /// </summary>
        /// <param name="frame">Promise 关联的帧。</param>
        /// <param name="uuid">Promise 唯一标识。</param>
        /// <param name="isRenderer">是否渲染进程侧,默认为主进程。</param>
        internal JavaScriptPromise(CefFrame frame, Guid uuid, bool isRenderer = false)
        {
            Frame = frame;
            Uuid = uuid;
            IsRenderer = isRenderer;
        }

        /// <summary>
        /// 以给定值解析该 Promise。
        /// </summary>
        /// <param name="retvals">用于解析的值,可以是任意 JavaScript 值(含 undefined)。</param>
        /// <exception cref="InvalidOperationException">该 Promise 已被处理过(只能调用一次)时抛出。</exception>
        public void Resolve(params JavaScriptValue[] retvals)
        {
            if (_isHandled) throw new InvalidOperationException("This method can be only called once.");

            _isHandled = true;

            var arguments = new JavaScriptArray();

            foreach (var retval in retvals)
            {
                arguments.Add(retval);
            }

            var message = new BridgeMessage(JavaScriptEngineBridge.EXECUTE_JAVASCRIPT_PROMISE_MESSAGE);

            message.SetData(new ExecuteJavaScriptFunctionMessage { FunctionId = Uuid, Success = true, FrameId = Frame.Identifier, Data = arguments.ToJson() });

            MessageBridge.SendMessageToRemote(Frame, message);
        }

        /// <summary>
        /// 以给定原因拒绝该 Promise。
        /// </summary>
        /// <param name="reason">可选的失败原因说明。</param>
        /// <exception cref="InvalidOperationException">该 Promise 已被处理过(只能调用一次)时抛出。</exception>
        public void Reject(string? reason = null)
        {
            if (_isHandled) throw new InvalidOperationException("This method can be only called once.");

            _isHandled = true;

            var message = new BridgeMessage(JavaScriptEngineBridge.EXECUTE_JAVASCRIPT_PROMISE_MESSAGE);

            message.SetData(new ExecuteJavaScriptFunctionMessage { FunctionId = Uuid, Success = false, FrameId = Frame.Identifier, ExceptionText = reason });

            MessageBridge.SendMessageToRemote(Frame, message);
        }
    }
}
