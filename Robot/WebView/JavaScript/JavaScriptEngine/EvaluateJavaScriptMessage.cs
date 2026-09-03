// Robot 桌面软件 — JavaScript 求值消息
// 用于跨进程发起 JavaScript 求值请求的消息载体

namespace Robot.JavaScript
{

    /// <summary>
    /// JavaScript 求值消息:携带任务标识、代码、来源地址与行号。
    /// </summary>
    internal record EvaluateJavaScriptMessage
    {
        /// <summary>
        /// 任务 ID。
        /// </summary>
        public required int TaskId { get; init; }

        /// <summary>
        /// JavaScript 代码。
        /// </summary>
        public required string Code { get; init; } = string.Empty;

        /// <summary>
        /// 来源地址。
        /// </summary>
        public required string Url { get; init; } = string.Empty;

        /// <summary>
        /// 行号。
        /// </summary>
        public required int Line { get; init; }

    }
}
