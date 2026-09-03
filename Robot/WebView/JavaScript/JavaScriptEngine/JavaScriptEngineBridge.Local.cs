// Robot 桌面软件 — JavaScript 引擎桥(本地部分)
// 在本地进程处理 JavaScript 属性访问、函数执行、求值等请求,并回填结果

using Robot.Browser;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xilium.CefGlue;

namespace Robot.JavaScript
{

    internal partial class JavaScriptEngineBridge
    {
        /// <summary>
        /// JavaScript 执行结果集合:以(任务 ID, 帧标识)为键,映射到任务完成源。
        /// </summary>
        internal static ConcurrentDictionary<(int, long), TaskCompletionSource<JavaScriptResult>> JavaScriptExecutionResults { get; } = new();

        /// <summary>
        /// 处理本地 JavaScript 对象属性读取请求。
        /// </summary>
        /// <param name="request">消息桥请求。</param>
        /// <returns>消息桥响应。</returns>
        private MessageBridgeResponse HandleGetJavaScriptObjectPropertyRequestOnLocal(MessageBridgeRequest request)
        {
            var requestData = JsonSerializer.Deserialize<AccessJavaScriptObjectPropertyMessage>(request.Payload!)!;

            var name = requestData.PropertyName;
            var propUuid = requestData.PropertyUuid;
            var objUuid = requestData.ObjectUuid;

            var func = JavaScriptValue.GetJavaScriptValue(propUuid);
            if (func == null || func.ValueType != JavaScriptValueType.Property || func.GetAssociatedFrame() == null || func.GetType() != typeof(JavaScriptProperty))
            {
                return new MessageBridgeResponse
                {
                    IsSuccess = false,
                    Exception = $"Property {name} is not defined."
                };
            }

            var caller = (JavaScriptProperty)func;

            if (caller.Getter != null)
            {
                var retval = caller.Getter.Invoke();
                return new MessageBridgeResponse()
                {
                    Data = retval.ToJson()
                };
            }

            return new MessageBridgeResponse
            {
                IsSuccess = false,
                Exception = $"Property {name} is not readable."
            };

        }

        /// <summary>
        /// 处理本地 JavaScript 对象属性写入请求。
        /// </summary>
        /// <param name="request">消息桥请求。</param>
        /// <returns>消息桥响应。</returns>
        private MessageBridgeResponse HandleSetJavaScriptObjectPropertyRequestOnLocal(MessageBridgeRequest request)
        {
            var requestData = JsonSerializer.Deserialize<AccessJavaScriptObjectPropertyMessage>(request.Payload!)!;

            var name = requestData.PropertyName;
            var propUuid = requestData.PropertyUuid;
            var objUuid = requestData.ObjectUuid;
            var value = JavaScriptValue.FromJson(requestData.Data!);

            var func = JavaScriptValue.GetJavaScriptValue(propUuid);
            if (func == null || func.ValueType != JavaScriptValueType.Property || func.GetAssociatedFrame() == null || func.GetType() != typeof(JavaScriptProperty))
            {
                return new MessageBridgeResponse
                {
                    IsSuccess = false,
                    Exception = $"Property {name} is not defined."
                };
            }

            var caller = (JavaScriptProperty)func;

            if (caller.Setter == null)
            {
                return new MessageBridgeResponse
                {
                    IsSuccess = false,
                    Exception = $"Property {name} is not writable."
                };
            }

            caller.Setter.Invoke(value);

            return new MessageBridgeResponse();
        }

        /// <summary>
        /// 处理本地 JavaScript Promise 函数执行请求。
        /// </summary>
        /// <param name="request">消息桥请求。</param>
        /// <returns>消息桥响应。</returns>
        private MessageBridgeResponse HandleExecuteJavaScriptPromiseRequestOnLocal(MessageBridgeRequest request)
        {
            var requestData = JsonSerializer.Deserialize<ExecuteJavaScriptFunctionOnLocalMessage>(request.Payload!)!;

            var funcId = requestData.FunctionId;
            var args = requestData.Data != null ? JavaScriptValue.FromJson(requestData.Data).ToArray() ?? new JavaScriptArray() : new JavaScriptArray();
            var func = JavaScriptValue.GetJavaScriptValue(funcId);

            if (func == null || func.ValueType != JavaScriptValueType.Function || func.GetType() != typeof(JavaScriptAsynchronousFunction))
            {
                return new MessageBridgeResponse
                {
                    IsSuccess = false,
                    Exception = "Function not found."
                };
            }

            if (func.Frame == null)
            {
                func.AssociateToFrame(Bridge.Browser.GetFrame(request.FrameId));
            }

            if (args.Frame == null)
            {
                args.AssociateToFrame(Bridge.Browser.GetFrame(request.FrameId));
            }

            var caller = (JavaScriptAsynchronousFunction)func;

            MessageBridgeResponse response;

            try
            {
                caller.FunctionDelegate.Invoke(args, new JavaScriptPromise(func.Frame!, funcId));

                response = new MessageBridgeResponse
                {
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                response = new MessageBridgeResponse
                {
                    IsSuccess = false,
                    Exception = ex.Message
                };

            }

            return response;
        }

        /// <summary>
        /// 处理本地 JavaScript 同步函数执行请求。
        /// </summary>
        /// <param name="request">消息桥请求。</param>
        /// <returns>消息桥响应。</returns>
        private MessageBridgeResponse HandleExecuteJavaScriptFunctionRequestOnLocal(MessageBridgeRequest request)
        {
            var requestData = JsonSerializer.Deserialize<ExecuteJavaScriptFunctionOnLocalMessage>(request.Payload!)!;

            var funcId = requestData.FunctionId;
            var args = requestData.Data != null ? JavaScriptValue.FromJson(requestData.Data).ToArray() ?? new JavaScriptArray() : new JavaScriptArray();

            var func = JavaScriptValue.GetJavaScriptValue(funcId);

            if (func == null || func.ValueType != JavaScriptValueType.Function || func.GetType() != typeof(JavaScriptSynchronousFunction))
            {
                return new MessageBridgeResponse
                {
                    IsSuccess = false,
                    Exception = "Function not found."
                };
            }

            if (args.Frame == null)
            {
                args.AssociateToFrame(Bridge.Browser.GetFrame(request.FrameId));
            }

            var caller = (JavaScriptSynchronousFunction)func;

            MessageBridgeResponse response;

            try
            {


                var retval = caller.FunctionDelegate.Invoke(args);


                if (retval == null)
                {
                    retval = new JavaScriptValue(JavaScriptValueType.Null, null);
                }

                response = new MessageBridgeResponse
                {
                    Data = retval.ToJson()
                };
            }
            catch (Exception ex)
            {
                response = new MessageBridgeResponse
                {
                    IsSuccess = false,
                    Exception = ex.Message
                };

            }

            return response;
        }


        /// <summary>
        /// 处理本地 JavaScript 求值完成消息:在 UI 线程回填求值结果。
        /// </summary>
        /// <param name="browser">浏览器实例。</param>
        /// <param name="frame">目标帧。</param>
        /// <param name="id">进程标识。</param>
        /// <param name="message">桥消息。</param>
        private void HandleEvaluateJavaScriptMessageOnLocal(CefBrowser browser, CefFrame frame, CefProcessId id, BridgeMessage message)
        {
            var data = message.DeserializeData<EvaluateJavaScriptCompleteMessage>()!;

            CefRuntime.PostTask(CefThreadId.UI, new EvaluateJavaScriptCompleteTaskOnLocal(this) { Frame = frame, TaskData = data });
        }

        /// <summary>
        /// 处理本地 JavaScript 函数执行完成消息:在 UI 线程回填执行结果。
        /// </summary>
        /// <param name="browser">浏览器实例。</param>
        /// <param name="frame">目标帧。</param>
        /// <param name="id">进程标识。</param>
        /// <param name="message">桥消息。</param>
        private void HandleExecuteJavaScriptFunctionMessageOnLocal(CefBrowser browser, CefFrame frame, CefProcessId id, BridgeMessage message)
        {
            var data = message.DeserializeData<ExecuteJavaScriptFunctionMessage>()!;

            CefRuntime.PostTask(CefThreadId.UI, new ExecuteJavaScriptCompleteTaskOnLocal(this) { Frame = frame, TaskData = data });

        }

        /// <summary>
        /// 在本地发起 JavaScript 求值:登记任务完成源并向远程发送求值消息。
        /// </summary>
        /// <param name="frame">目标帧。</param>
        /// <param name="code">JavaScript 代码。</param>
        /// <param name="url">来源地址。</param>
        /// <param name="line">行号。</param>
        /// <returns>承载求值结果的任务。</returns>
        private Task<JavaScriptResult> EvaluateJavaScriptOnLocal(CefFrame frame, string code, string url = "", int line = 0)
        {
            var tcs = new TaskCompletionSource<JavaScriptResult>();

            var taskId = tcs.GetHashCode();

            if (JavaScriptExecutionResults.TryAdd((taskId, frame.Identifier), tcs))
            {
                MessageBridge.SendMessageToRemote(frame, new BridgeMessage(EVALUATE_JAVASCRIPT_MESSAGE, new EvaluateJavaScriptMessage()
                {
                    TaskId = taskId,
                    Code = code,
                    Url = url,
                    Line = line
                }));
            }
            else
            {
                tcs.SetException(new ArgumentException());
            }

            return tcs.Task;
        }

        /// <summary>
        /// 浏览前回调:取消与目标帧关联的未完成求值任务。
        /// </summary>
        /// <param name="browser">浏览器实例。</param>
        /// <param name="frame">目标帧。</param>
        /// <param name="request">请求对象。</param>
        /// <param name="userGesture">是否由用户手势触发。</param>
        /// <param name="isRedirect">是否为重定向。</param>
        public override void OnBeforeBrowse(CefBrowser browser, CefFrame frame, CefRequest request, bool userGesture, bool isRedirect)
        {
            try
            {
                var tasks = JavaScriptExecutionResults.Where(x => x.Key.Item2 == frame.Identifier).ToList();

                foreach (var task in tasks)
                {
                    JavaScriptExecutionResults.TryRemove(task.Key, out var tcs);
                    tcs?.SetCanceled();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

        }

        /// <summary>
        /// 关闭前回调:取消与浏览器关联的未完成求值任务并释放 JavaScript 值。
        /// </summary>
        /// <param name="browser">浏览器实例。</param>
        public override void OnBeforeClose(CefBrowser browser)
        {
            try
            {
                var tasks = JavaScriptExecutionResults.Where(x => x.Key.Item1 == browser.Identifier).ToList();

                foreach (var task in tasks)
                {
                    JavaScriptExecutionResults.TryRemove(task.Key, out var tcs);
                    tcs?.SetCanceled();
                }

                JavaScriptValue.Release(browser);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

        }

        /// <summary>
        /// 渲染进程终止回调(当前无处理逻辑)。
        /// </summary>
        /// <param name="browser">浏览器实例。</param>
        public override void OnRenderProcessTerminated(CefBrowser browser)
        {

        }
    }
}
