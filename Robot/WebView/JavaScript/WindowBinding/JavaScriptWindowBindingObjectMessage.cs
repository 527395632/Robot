// Robot 桌面软件 — 窗口绑定对象消息
// 承载跨进程执行窗口绑定函数所需的对象名、函数名与实参

using System;

namespace Robot.JavaScript
{

    /// <summary>
    /// 窗口绑定对象消息:承载跨进程执行窗口绑定函数所需的对象名、函数名与实参。
    /// </summary>
    internal record JavaScriptWindowBindingObjectMessage
    {
        /// <summary>
        /// 窗口绑定对象名称。
        /// </summary>
        public required string ObjectName { get; set; }

        /// <summary>
        /// 函数唯一标识。
        /// </summary>
        public required Guid Uuid { get; set; }

        /// <summary>
        /// 函数名称。
        /// </summary>
        public required string FunctionName { get; set; }

        /// <summary>
        /// 函数实参 JSON。
        /// </summary>
        public required string Arguments { get; set; }
    };
}
