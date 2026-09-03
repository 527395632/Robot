// Robot 桌面软件 — JavaScript 值定义
// 承载 JavaScript 值跨进程传输的元数据(标识、名称、类型、原始值)

using System;

namespace Robot.JavaScript
{

    /// <summary>
    /// JavaScript 值定义:承载 JavaScript 值跨进程传输的元数据(标识、名称、类型、原始值)。
    /// </summary>
    internal class JavaScriptValueDefinition
    {
        /// <summary>
        /// 值唯一标识。
        /// </summary>
        public required Guid Uuid { get; init; }

        /// <summary>
        /// 值名称。
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// 值类型。
        /// </summary>
        public required JavaScriptValueType ValueType { get; init; }

        /// <summary>
        /// 值类型名称。
        /// </summary>
        public string ValueTypeName => Enum.GetName(ValueType.GetType(), ValueType)!;

        /// <summary>
        /// 原始值(按类型承载不同结构)。
        /// </summary>
        public object? ValueDefinition { get; init; }
    }
}
