// Robot 桌面软件 — JavaScript 函数执行完成任务(本地)
// 在本地线程完成 JavaScript 函数执行任务,回填结果到任务完成源

using Xilium.CefGlue;

namespace Robot.JavaScript
{

    /// <summary>
    /// JavaScript 函数执行完成任务(本地):从结果集合取出任务完成源并回填执行结果。
    /// </summary>
    internal class ExecuteJavaScriptCompleteTaskOnLocal : CefTask
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
        /// 函数执行完成消息数据。
        /// </summary>
        public required ExecuteJavaScriptFunctionMessage TaskData { get; init; }


        /// <summary>
        /// 初始化 <see cref="ExecuteJavaScriptCompleteTaskOnLocal"/> 实例。
        /// </summary>
        /// <param name="javaScriptBridge">JavaScript 引擎桥。</param>
        public ExecuteJavaScriptCompleteTaskOnLocal(JavaScriptEngineBridge javaScriptBridge)
        {
            Bridge = javaScriptBridge;
        }


        /// <summary>
        /// 执行任务:从结果集合取出任务完成源并回填执行结果。
        /// </summary>
        protected override void Execute()
        {
            var bag = JavaScriptEngineBridge.JavaScriptExecutionResults;

            if (bag.TryRemove((TaskData.TaskId, Frame.Identifier), out var tcs))
            {
                tcs.SetResult(new JavaScriptResult(Frame, TaskData.Success, TaskData.Data ?? string.Empty, TaskData.ExceptionText ?? string.Empty));
            }
        }
    }
}
