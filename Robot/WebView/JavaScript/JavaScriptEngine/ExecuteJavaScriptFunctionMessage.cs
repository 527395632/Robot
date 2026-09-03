// Robot 桌面软件 — JavaScript 函数执行消息
// 用于跨进程发起 JavaScript 函数执行请求、并回填执行结果的消息载体

using System;

namespace Robot.JavaScript
{

    /// <summary>
    /// JavaScript 函数执行消息:携带任务标识、函数标识、帧标识及执行结果。
    /// </summary>
    internal record ExecuteJavaScriptFunctionMessage
    {
        /// <summary>
        /// 任务 ID。
        /// </summary>
        public int TaskId { get; init; }

        /// <summary>
        /// 函数 ID。
        /// </summary>
        public Guid FunctionId { get; set; }

        /// <summary>
        /// 帧 ID。
        /// </summary>
        public long FrameId { get; set; }

        /// <summary>
        /// 是否成功。
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 执行数据。
        /// </summary>
        public string? Data { get; set; }

        /// <summary>
        /// 异常文本。
        /// </summary>
        public string? ExceptionText { get; set; }
    }
}
