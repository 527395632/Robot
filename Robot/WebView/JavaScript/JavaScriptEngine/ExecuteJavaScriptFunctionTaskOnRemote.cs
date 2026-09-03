// Robot 桌面软件 — JavaScript 函数执行任务(远程)
// 在远程进程执行已注册的 JavaScript 函数,并将执行结果消息发回本地

using Robot.Browser;
using System;
using Xilium.CefGlue;

namespace Robot.JavaScript
{

    /// <summary>
    /// JavaScript 函数执行任务(远程):在 V8 上下文执行已注册函数,并将执行结果消息发回本地。
    /// </summary>
    internal class ExecuteJavaScriptFunctionTaskOnRemote : CefTask
    {
        /// <summary>
        /// JavaScript 引擎桥。
        /// </summary>
        public JavaScriptEngineBridge Bridge { get; }

        /// <summary>
        /// 目标帧。
        /// </summary>
        public required CefFrame Frame { get; init; }

        /// <summary>
        /// 函数执行消息数据。
        /// </summary>
        public required ExecuteJavaScriptFunctionMessage TaskData { get; init; }

        /// <summary>
        /// 初始化 <see cref="ExecuteJavaScriptFunctionTaskOnRemote"/> 实例。
        /// </summary>
        /// <param name="bridge">JavaScript 引擎桥。</param>
        public ExecuteJavaScriptFunctionTaskOnRemote(JavaScriptEngineBridge bridge)
        {
            Bridge = bridge;
        }

        /// <summary>
        /// 执行任务:在 V8 上下文执行已注册函数,并将执行结果消息发回本地。
        /// </summary>
        protected override void Execute()
        {
            var func = JavaScriptValue.GetJavaScriptValue(TaskData.FunctionId);


            // 仅处理远程函数调用器类型的函数值
            if (func == null || func.ValueType != JavaScriptValueType.Function || func.GetType() != typeof(JavaScriptFunctionInvokerOnRemote)) return;


            var caller = (JavaScriptFunctionInvokerOnRemote)func;

            var message = new BridgeMessage(JavaScriptEngineBridge.EXECUTE_JAVASCRIPT_FUNCTION_MESSAGE);


            try
            {
                var args = JavaScriptValue.FromJson(TaskData.Data!);


                CefV8Value[]? arguments;

                var context = Frame.V8Context ?? CefV8Context.GetCurrentContext();

                context.Enter();


                if (args.ValueType != JavaScriptValueType.Array || args == null)
                {
                    arguments = new CefV8Value[] { };
                }
                else
                {
                    var array = args.ToArray();
                    array.AssociateToFrame(Frame);


                    arguments = array.ToCefV8Arguments();

                }

                context.Exit();


                var retval = caller.FunctionBody.ExecuteFunctionWithContext(context, caller.FunctionBody, arguments);



                if (retval != null)
                {
                    message.SetData(new ExecuteJavaScriptFunctionMessage
                    {
                        TaskId = TaskData.TaskId,
                        FunctionId = TaskData.FunctionId,
                        FrameId = Frame.Identifier,
                        Success = true,
                        Data = retval!.ToJavaScriptValue().ToJson()

                    });
                }
                else
                {
                    message.SetData(new ExecuteJavaScriptFunctionMessage
                    {
                        TaskId = TaskData.TaskId,
                        FunctionId = TaskData.FunctionId,
                        FrameId = Frame.Identifier,
                        Success = false,
                        ExceptionText = "Cannot execute function."
                    });
                }

                Bridge.SendMessageToLocal(Frame, message);

            }
            catch (Exception ex)
            {
                message.SetData(new ExecuteJavaScriptFunctionMessage
                {
                    TaskId = TaskData.TaskId,
                    FunctionId = TaskData.FunctionId,
                    FrameId = Frame.Identifier,
                    Success = false,
                    ExceptionText = ex.Message
                });

                Bridge.SendMessageToLocal(Frame, message);

            }

        }
    }
}
