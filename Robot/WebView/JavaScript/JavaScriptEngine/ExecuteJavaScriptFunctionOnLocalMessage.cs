// Robot 桌面软件 — JavaScript 函数本地执行消息
// 用于在本地线程执行 JavaScript 函数、并回填执行结果的消息载体

using System;

namespace Robot.JavaScript
{

    /// <summary>
    /// JavaScript 函数本地执行消息:携带函数标识与执行数据。
    /// </summary>
    internal record ExecuteJavaScriptFunctionOnLocalMessage
    {
        /// <summary>
        /// 函数 ID。
        /// </summary>
        public Guid FunctionId { get; set; }

        /// <summary>
        /// 执行数据。
        /// </summary>
        public string? Data { get; set; }
    }
}
