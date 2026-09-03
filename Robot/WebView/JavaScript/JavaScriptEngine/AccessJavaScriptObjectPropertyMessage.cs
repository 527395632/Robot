// Robot 桌面软件 — 访问 JavaScript 对象属性消息
// 用于跨进程访问 JavaScript 对象属性的消息载体

using System;

namespace Robot.JavaScript
{

    /// <summary>
    /// 访问 JavaScript 对象属性消息:携带对象与属性标识及数据。
    /// </summary>
    internal record AccessJavaScriptObjectPropertyMessage
    {
        /// <summary>
        /// 对象 UUID。
        /// </summary>
        public required Guid ObjectUuid { get; init; }

        /// <summary>
        /// 属性 UUID。
        /// </summary>
        public required Guid PropertyUuid { get; init; }

        /// <summary>
        /// 属性名称。
        /// </summary>
        public required string PropertyName { get; init; }

        /// <summary>
        /// 属性数据。
        /// </summary>
        public string? Data { get; set; }
    }
}
