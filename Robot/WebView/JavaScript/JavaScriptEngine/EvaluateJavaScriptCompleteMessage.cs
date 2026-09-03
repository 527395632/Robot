// Robot 桌面软件 — JavaScript 求值完成消息
// 用于跨进程返回 JavaScript 求值结果的消息载体

namespace Robot.JavaScript
{

    /// <summary>
    /// JavaScript 求值完成消息:携带任务标识、成功状态、数据与异常。
    /// </summary>
    internal record EvaluateJavaScriptCompleteMessage
    {
        /// <summary>
        /// 任务 ID。
        /// </summary>
        public required int TaskId { get; init; }

        /// <summary>
        /// 是否成功。
        /// </summary>
        public required bool Success { get; init; }

        /// <summary>
        /// 求值数据。
        /// </summary>
        public string? Data { get; init; }

        /// <summary>
        /// 消息文本。
        /// </summary>
        public string? Message { get; init; }

        /// <summary>
        /// JavaScript 异常。
        /// </summary>
        public JavaScriptException? Exception { get; init; }

    }
}
