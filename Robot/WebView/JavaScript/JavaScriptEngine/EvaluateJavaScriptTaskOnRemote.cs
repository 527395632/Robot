// Robot 桌面软件 — JavaScript 求值任务(远程)
// 在远程进程执行 JavaScript 求值,并将结果消息发回本地

using Robot.Browser;
using Xilium.CefGlue;

namespace Robot.JavaScript
{

    /// <summary>
    /// JavaScript 求值任务(远程):在 V8 上下文执行代码,并将求值结果消息发回本地。
    /// </summary>
    internal class EvaluateJavaScriptTaskOnRemote : CefTask
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
        /// 求值消息数据。
        /// </summary>
        public required EvaluateJavaScriptMessage TaskData { get; init; }

        /// <summary>
        /// 初始化 <see cref="EvaluateJavaScriptTaskOnRemote"/> 实例。
        /// </summary>
        /// <param name="handler">JavaScript 引擎桥。</param>
        public EvaluateJavaScriptTaskOnRemote(JavaScriptEngineBridge handler)
        {
            Bridge = handler;
        }

        /// <summary>
        /// 执行任务:在 V8 上下文执行代码,并将求值结果消息发回本地。
        /// </summary>
        protected override void Execute()
        {
            var v8 = Frame.V8Context;

            var message = new BridgeMessage(JavaScriptEngineBridge.EVALUATE_JAVASCRIPT_COMPLETE_MESSAGE);

            var isExecutedSuccess = v8.TryEval(TaskData.Code, TaskData.Url, TaskData.Line, out var retval, out var v8Exception);

            if (isExecutedSuccess)
            {
                if (v8.Enter())
                {
                    message.SetData(new EvaluateJavaScriptCompleteMessage
                    {
                        TaskId = TaskData.TaskId,
                        Success = true,
                        Data = retval!.ToJavaScriptValue().ToJson()
                    });

                    v8.Exit();
                }
                else
                {
                    message.SetData(new EvaluateJavaScriptCompleteMessage
                    {
                        TaskId = TaskData.TaskId,
                        Success = false,
                        Message = "Cannot enter v8 context."
                    });
                }


            }
            else
            {
                message.SetData(new EvaluateJavaScriptCompleteMessage
                {
                    TaskId = TaskData.TaskId,
                    Success = false,
                    Message = v8Exception!.Message,
                    Exception = new JavaScriptException
                    {
                        StartColumn = v8Exception.StartColumn,
                        StartPosition = v8Exception.StartPosition,
                        EndColumn = v8Exception.EndColumn,
                        EndPosition = v8Exception.EndPosition,
                        LineNumber = v8Exception.LineNumber,
                        ScriptResourceName = v8Exception.ScriptResourceName,
                        SourceLine = v8Exception.SourceLine,
                    }
                });
            }


            Bridge.SendMessageToLocal(Frame, message);

            //retval?.Dispose();
            //v8Exception?.Dispose();
            v8?.Dispose();
        }
    }
}
