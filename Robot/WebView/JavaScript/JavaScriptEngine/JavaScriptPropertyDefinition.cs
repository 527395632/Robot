// Robot 桌面软件 — JavaScript 属性定义
// 承载 JavaScript 属性跨进程传输的元数据

namespace Robot.JavaScript
{

    /// <summary>
    /// JavaScript 属性定义:承载 JavaScript 属性跨进程传输的元数据。
    /// </summary>
    internal sealed class JavaScriptPropertyDefinition
    {
        /// <summary>
        /// 是否可写。
        /// </summary>
        public required bool Writable { get; set; }
    }
}
