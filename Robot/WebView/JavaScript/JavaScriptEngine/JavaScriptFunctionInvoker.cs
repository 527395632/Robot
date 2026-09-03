// Robot 桌面软件 — JavaScript 函数调用器
// 表示可跨进程调用的 JavaScript 函数,支持同步与异步执行

using Robot.Browser;
using System;
using System.Threading.Tasks;

namespace Robot.JavaScript
{

    /// <summary>
    /// JavaScript 函数调用器:表示可跨进程调用的 JavaScript 函数,支持同步与异步执行。
    /// </summary>
    public sealed class JavaScriptFunctionInvoker : JavaScriptValue
    {


        /// <summary>
        /// 是否异步函数。
        /// </summary>
        public required bool IsAsynchronous { get; init; }

        /// <summary>
        /// 是否渲染进程侧。
        /// </summary>
        public required bool IsRenderer { get; init; }

        /// <summary>
        /// 初始化 <see cref="JavaScriptFunctionInvoker"/> 实例。
        /// </summary>
        internal JavaScriptFunctionInvoker()
        : base(JavaScriptValueType.Function)
        {

        }

        /// <summary>
        /// 异步执行函数(以可变参数形式传入实参)。
        /// </summary>
        /// <param name="arguments">函数实参。</param>
        /// <returns>承载执行结果的任务。</returns>
        public Task<JavaScriptResult> ExecuteAsync(params JavaScriptValue[] arguments)
        {
            var array = new JavaScriptArray();

            if (arguments != null)
            {
                foreach (var item in arguments)
                {
                    array.Add(item);
                }
            }

            array.AssociateToFrame(Frame);


            return ExecuteAsync(array);

        }

        /// <summary>
        /// 异步执行函数(以参数数组形式传入实参)。
        /// </summary>
        /// <param name="arguments">函数实参数组;为 null 时使用空数组。</param>
        /// <returns>承载执行结果的任务。</returns>
        public Task<JavaScriptResult> ExecuteAsync(JavaScriptArray? arguments = null)
        {
            var tcs = new TaskCompletionSource<JavaScriptResult>();
            var taskId = tcs.GetHashCode();

            if (arguments == null)
            {
                arguments = new JavaScriptArray();
            }

            arguments.AssociateToFrame(Frame);

            if (Frame != null && JavaScriptEngineBridge.JavaScriptExecutionResults.TryAdd((taskId, Frame.Identifier), tcs))
            {
                MessageBridge.SendMessageToRemote(Frame, new BridgeMessage(JavaScriptEngineBridge.EXECUTE_JAVASCRIPT_FUNCTION_MESSAGE, new ExecuteJavaScriptFunctionMessage()
                {
                    TaskId = taskId,
                    FunctionId = Uuid,
                    FrameId = Frame.Identifier,
                    Data = arguments.ToJson()
                }));
            }
            else
            {
                tcs.SetException(new ArgumentException());
            }

            return tcs.Task;
        }


    }
}
