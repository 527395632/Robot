// Robot 桌面软件 — 浏览器消息投递消息
// 承载跨进程投递到浏览器的消息名与数据

namespace Robot.JavaScript
{

    /// <summary>
    /// 浏览器消息投递消息:承载跨进程投递到浏览器的消息名与数据。
    /// </summary>
    internal record JavaScriptPostBrowserMessageMessage
    {
        /// <summary>
        /// 消息名。
        /// </summary>
        public required string Message { get; init; }

        /// <summary>
        /// 消息数据;无数据时为 null。
        /// </summary>
        public string? Data { get; init; }
    }
}
