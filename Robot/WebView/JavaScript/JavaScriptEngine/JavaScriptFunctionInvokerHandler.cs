// Robot 桌面软件 — JavaScript 函数调用器处理器
// 在 CEF 侧拦截 JavaScript 函数调用,转发到远端执行并回填返回值

using Robot.Browser;
using System.Text.Json;
using Xilium.CefGlue;

namespace Robot.JavaScript
{

    /// <summary>
    /// JavaScript 函数调用器处理器:在 CEF 侧拦截 JavaScript 函数调用,转发到远端执行并回填返回值。
    /// </summary>
    internal class JavaScriptFunctionInvokerHandler : CefV8Handler
    {
        /// <summary>
        /// 被调用的 JavaScript 函数调用器。
        /// </summary>
        public JavaScriptFunctionInvoker JsValue { get; }

        /// <summary>
        /// 调用发生的 V8 上下文。
        /// </summary>
        public CefV8Context Context { get; }

        /// <summary>
        /// 初始化 <see cref="JavaScriptFunctionInvokerHandler"/> 实例。
        /// </summary>
        /// <param name="jsvalue">待转换的 JavaScript 值(须为函数类型)。</param>
        /// <param name="context">调用发生的 V8 上下文。</param>
        public JavaScriptFunctionInvokerHandler(JavaScriptValue jsvalue, CefV8Context context)
        {
            JsValue = jsvalue.ToFunction();
            Context = context;
        }

        /// <summary>
        /// 执行被调用的 JavaScript 函数:将实参转发到远端,并根据同步/异步回填返回值或异常。
        /// </summary>
        /// <param name="name">被调用的函数名。</param>
        /// <param name="obj">调用发生的对象。</param>
        /// <param name="arguments">函数实参数组。</param>
        /// <param name="returnValue">回填给 V8 的返回值。</param>
        /// <param name="exception">回填给 V8 的异常信息;无异常时为 null。</param>
        /// <returns>始终返回 true,表示已处理该调用。</returns>
        protected override bool Execute(string name, CefV8Value obj, CefV8Value[] arguments, out CefV8Value returnValue, out string exception)
        {
            var browser = Context.GetBrowser();
            var frame = Context.GetFrame();

            var args = new JavaScriptArray();

            foreach (var arg in arguments)
            {
                args.Add(arg.ToJavaScriptValue());
            }

            MessageBridgeResponse response;

            if (JsValue.IsAsynchronous)
            {
                response = MessageBridge.ExecuteRequest(new MessageBridgeRequest
                {
                    Name = JavaScriptEngineBridge.EXECUTE_JAVASCRIPT_PROMISE_MESSAGE,
                    BrowserId = browser.Identifier,
                    FrameId = frame.Identifier,
                    IsRemote = true,
                    Payload = JsonSerializer.Serialize(new ExecuteJavaScriptFunctionOnLocalMessage
                    {
                        FunctionId = JsValue.Uuid,
                        Data = args.ToJson()
                    })
                });
            }
            else
            {
                response = MessageBridge.ExecuteRequest(new MessageBridgeRequest
                {
                    Name = JavaScriptEngineBridge.EXECUTE_JAVASCRIPT_FUNCTION_MESSAGE,
                    BrowserId = browser.Identifier,
                    FrameId = frame.Identifier,
                    IsRemote = true,
                    Payload = JsonSerializer.Serialize(new ExecuteJavaScriptFunctionOnLocalMessage
                    {
                        FunctionId = JsValue.Uuid,
                        Data = args.ToJson()
                    })
                });
            }

            if (response.IsSuccess)
            {
                if (JsValue.IsAsynchronous)
                {
                    returnValue = Context.CreateJavaScriptPromiseContext(JsValue.Uuid);
                }
                else
                {
                    Context.Enter();

                    var retval = JavaScriptValue.FromJson(response.Data!).ToCefV8Value();

                    Context.Exit();

                    if (retval != null)
                    {
                        returnValue = retval;
                    }
                    else
                    {
                        returnValue = CefV8Value.CreateUndefined();
                    }
                }
                exception = null;
            }
            else
            {
                exception = $"[{nameof(Robot)}]: {response.Exception}";
                returnValue = CefV8Value.CreateUndefined();
            }

            return true;
        }
    }
}
