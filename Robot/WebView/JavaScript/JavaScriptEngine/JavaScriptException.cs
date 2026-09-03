// Robot 桌面软件 — JavaScript 异常
// 承载 V8 抛出的 JavaScript 异常的行列位置与脚本来源信息

namespace Robot.JavaScript
{

    /// <summary>
    /// JavaScript 异常:承载 V8 抛出的异常的行列位置与脚本来源信息。
    /// </summary>
    public sealed class JavaScriptException
    {
        /// <summary>
        /// 起始列。
        /// </summary>
        public required int StartColumn { get; init; }

        /// <summary>
        /// 起始位置。
        /// </summary>
        public required int StartPosition { get; init; }

        /// <summary>
        /// 结束列。
        /// </summary>
        public required int EndColumn { get; init; }

        /// <summary>
        /// 结束位置。
        /// </summary>
        public required int EndPosition { get; init; }

        /// <summary>
        /// 行号。
        /// </summary>
        public required int LineNumber { get; init; }

        /// <summary>
        /// 脚本资源名称。
        /// </summary>
        public required string ScriptResourceName { get; init; }

        /// <summary>
        /// 出错源码行。
        /// </summary>
        public required string SourceLine { get; init; }

    }
}
