// Robot 桌面软件 — 窗口绑定对象桥(本地部分)
// 处理本地侧的窗口绑定函数执行请求(同步/异步)

using System;
using System.Linq;
using System.Text.Json;
using Robot.Browser;
using Xilium.CefGlue;

namespace Robot.JavaScript
{

    /// <summary>
    /// 窗口绑定对象桥(本地部分):处理本地侧的窗口绑定函数执行请求(同步/异步)。
    /// </summary>
    internal partial class JavaScriptWindowBindingObjectBridge
    {
        /// <summary>
        /// 浏览前回调(本地侧无处理)。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="request">请求。</param>
        /// <param name="userGesture">是否用户手势触发。</param>
        /// <param name="isRedirect">是否重定向。</param>
        public override void OnBeforeBrowse(CefBrowser browser, CefFrame frame, CefRequest request, bool userGesture, bool isRedirect)
        {
        }

        /// <summary>
        /// 关闭前回调(本地侧无处理)。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        public override void OnBeforeClose(CefBrowser browser)
        {
        }

        /// <summary>
        /// 渲染进程终止回调(本地侧无处理)。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        public override void OnRenderProcessTerminated(CefBrowser browser)
        {
        }

        /// <summary>
        /// 调用方宿主实例。
        /// </summary>
        public RobotWindow InvokerInstance { get; }

        /// <summary>
        /// 在本地处理窗口绑定对象异步函数执行请求。
        /// </summary>
        /// <param name="request">桥请求。</param>
        /// <returns>承载执行结果或异常的响应。</returns>
        private MessageBridgeResponse HandleExecuteWindowBindingObjectAsynchronousFunctionRequestOnLocal(MessageBridgeRequest request)
        {
            var requestData = JsonSerializer.Deserialize<JavaScriptWindowBindingObjectMessage>(request.Payload!)!;

            var objectName = requestData.ObjectName;
            var funcId = requestData.Uuid;
            var funcName = requestData.FunctionName;
            var data = requestData.Arguments;
            var frame = Bridge.Browser.GetFrame(request.FrameId);
            var args = JavaScriptValue.FromJson(data).ToArray();

            var windowBindingObject = WindowBindingObjects.SingleOrDefault(x => x.Name == objectName);

            if (windowBindingObject == null)
            {
                return new MessageBridgeResponse
                {
                    IsSuccess = false,
                    Exception = $"[{nameof(Robot)}]: The `{objectName}` window binding object is not exists."
                };
            }

            args.AssociateToFrame(frame);

            var function = windowBindingObject.WindowBindingFunctions.SingleOrDefault(x => x.FunctionName == funcName);

            if (function == null)
            {
                return new MessageBridgeResponse
                {
                    IsSuccess = false,
                    Exception = $"[{nameof(Robot)}]: The `{funcName}` function is not defined."
                };
            }

            if (function.FunctionType == JavaScriptWindowBindingObjectFunctionType.AsynchronousFunctionOnLocal)
            {
                try
                {
                    function.AsynchronousFunctionOnLocal!.Invoke(InvokerInstance!, args, new JavaScriptPromise(frame, funcId));

                    return new MessageBridgeResponse();
                }
                catch (Exception ex)
                {
                    return new MessageBridgeResponse
                    {
                        IsSuccess = false,
                        Exception = $"[{nameof(Robot)}]: {ex.Message}"
                    };
                }
            }

            return new MessageBridgeResponse
            {
                IsSuccess = false,
                Exception = $"[{nameof(Robot)}]: The handler of `{funcName}` function is not defined."
            };
        }

        /// <summary>
        /// 在本地处理窗口绑定对象同步函数执行请求。
        /// </summary>
        /// <param name="request">桥请求。</param>
        /// <returns>承载执行结果或异常的响应。</returns>
        private MessageBridgeResponse HandleExecuteWindowBindingObjectSynchronousFunctionRequestOnLocal(MessageBridgeRequest request)
        {
            var requestData = JsonSerializer.Deserialize<JavaScriptWindowBindingObjectMessage>(request.Payload!)!;

            var objectName = requestData.ObjectName;
            var funcId = requestData.Uuid;
            var funcName = requestData.FunctionName;
            var data = requestData.Arguments;
            var frame = Bridge.Browser.GetFrame(request.FrameId);
            var args = JavaScriptValue.FromJson(data).ToArray();

            var windowBindingObject = WindowBindingObjects.SingleOrDefault(x => x.Name == objectName);

            if (windowBindingObject == null)
            {
                return new MessageBridgeResponse
                {
                    IsSuccess = false,
                    Exception = $"[{nameof(Robot)}]: The `{objectName}` window binding object is not exists."
                };
            }

            args.AssociateToFrame(frame);

            var function = windowBindingObject.WindowBindingFunctions.SingleOrDefault(x => x.FunctionName == funcName);

            if (function == null)
            {
                return new MessageBridgeResponse
                {
                    IsSuccess = false,
                    Exception = $"[{nameof(Robot)}]: The `{funcName}` function is not defined."
                };
            }

            if (function.FunctionType == JavaScriptWindowBindingObjectFunctionType.SynchronousFunctionOnLocal)
            {
                try
                {
                    var retval = function.SynchronousFunctionOnLocal!.Invoke(InvokerInstance!, args) ?? new JavaScriptValue();

                    return new MessageBridgeResponse()
                    {
                        Data = retval.ToJson()
                    };
                }
                catch (Exception ex)
                {
                    return new MessageBridgeResponse
                    {
                        IsSuccess = false,
                        Exception = $"[{nameof(Robot)}]: {ex.Message}"
                    };
                }
            }

            return new MessageBridgeResponse
            {
                IsSuccess = false,
                Exception = $"[{nameof(Robot)}]: The handler of `{funcName}` function is not defined."
            };
        }
    }
}
