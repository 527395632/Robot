// Robot 桌面软件 — JavaScript 引擎桥
// 跨进程 JavaScript 求值、函数执行、属性访问的桥接处理器

using Robot.Browser;
using System.Threading.Tasks;
using Xilium.CefGlue;

namespace Robot.JavaScript
{

    /// <summary>
    /// JavaScript 引擎桥:跨进程处理 JavaScript 求值、函数执行、属性访问的桥接处理器。
    /// </summary>
    internal partial class JavaScriptEngineBridge : MessageBridgeHandler
    {
        /// <summary>
        /// JavaScript 求值消息标识。
        /// </summary>
        public static readonly string EVALUATE_JAVASCRIPT_MESSAGE = "Robot.EvaluateJavaScript";

        /// <summary>
        /// JavaScript 求值完成消息标识。
        /// </summary>
        public static readonly string EVALUATE_JAVASCRIPT_COMPLETE_MESSAGE = "Robot.EvaluateJavaScriptComplete";

        /// <summary>
        /// JavaScript 函数执行消息标识。
        /// </summary>
        public static readonly string EXECUTE_JAVASCRIPT_FUNCTION_MESSAGE = "Robot.ExecuteJavaScriptFunction";

        /// <summary>
        /// JavaScript Promise 执行消息标识。
        /// </summary>
        public static readonly string EXECUTE_JAVASCRIPT_PROMISE_MESSAGE = "Robot.ExecuteJavaScriptPromise";

        /// <summary>
        /// JavaScript 对象属性读取消息标识。
        /// </summary>
        public static readonly string GET_JAVASCRIPT_OBJECT_PROPERTY_MESSAGE = "Robot.GetJavaScriptObjectProperty";

        /// <summary>
        /// JavaScript 对象属性写入消息标识。
        /// </summary>
        public static readonly string SET_JAVASCRIPT_OBJECT_PROPERTY_MESSAGE = "Robot.SetJavaScriptObjectProperty";

        /// <summary>
        /// 初始化 <see cref="JavaScriptEngineBridge"/> 实例,并按进程侧注册消息与请求处理器。
        /// </summary>
        /// <param name="bridge">消息桥。</param>
        public JavaScriptEngineBridge(MessageBridge bridge) : base(bridge)
        {
            // 本地侧
            if (!Bridge.IsRenderer)
            {
                RegisterMessageHandler(EVALUATE_JAVASCRIPT_COMPLETE_MESSAGE, HandleEvaluateJavaScriptMessageOnLocal);
                RegisterMessageHandler(EXECUTE_JAVASCRIPT_FUNCTION_MESSAGE, HandleExecuteJavaScriptFunctionMessageOnLocal);

                // 在本地处理 JavaScript 函数与 Promise 函数执行请求
                RegisterRequestHandler(EXECUTE_JAVASCRIPT_FUNCTION_MESSAGE, HandleExecuteJavaScriptFunctionRequestOnLocal);

                RegisterRequestHandler(EXECUTE_JAVASCRIPT_PROMISE_MESSAGE, HandleExecuteJavaScriptPromiseRequestOnLocal);

                // 在本地处理 JavaScript 对象属性访问请求
                RegisterRequestHandler(GET_JAVASCRIPT_OBJECT_PROPERTY_MESSAGE, HandleGetJavaScriptObjectPropertyRequestOnLocal);
                RegisterRequestHandler(SET_JAVASCRIPT_OBJECT_PROPERTY_MESSAGE, HandleSetJavaScriptObjectPropertyRequestOnLocal);
            }


            // 远程侧

            if (Bridge.IsRenderer)
            {
                // 在远程处理 JavaScript 求值
                RegisterMessageHandler(EVALUATE_JAVASCRIPT_MESSAGE, HandleEvaluateJavaScriptMessageOnRemote);

                // 在远程处理 JavaScript 函数与 Promise 函数执行请求
                RegisterMessageHandler(EXECUTE_JAVASCRIPT_FUNCTION_MESSAGE, HandleExecuteJavaScriptFunctionMessageOnRemote);

                RegisterMessageHandler(EXECUTE_JAVASCRIPT_PROMISE_MESSAGE, HandleExecuteJavaScriptPromiseMessageOnRemote);
            }

        }


        /// <summary>
        /// 异步发起 JavaScript 求值。
        /// </summary>
        /// <param name="frame">目标帧。</param>
        /// <param name="code">JavaScript 代码。</param>
        /// <param name="url">来源地址。</param>
        /// <param name="line">行号。</param>
        /// <returns>承载求值结果的任务。</returns>
        public Task<JavaScriptResult> EvaluateJavaScriptAsync(CefFrame frame, string code, string url = "about:blank", int line = 0)
        {
            return EvaluateJavaScriptOnLocal(frame, code, url, line);
        }

    }
}
