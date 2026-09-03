// Robot 桌面软件 — JavaScript 值类型
// 枚举 JavaScript 值的所有类型

using System.ComponentModel;

namespace Robot.JavaScript
{

    /// <summary>
    /// JavaScript 值类型:枚举 JavaScript 值的所有类型。
    /// </summary>
    public enum JavaScriptValueType
    {
        /// <summary>
        /// 未定义。
        /// </summary>
        [Description("未定义")]
        Undefined = -1,

        /// <summary>
        /// 空值。
        /// </summary>
        [Description("空值")]
        Null,

        /// <summary>
        /// 布尔值。
        /// </summary>
        [Description("布尔值")]
        Bool,

        /// <summary>
        /// 数值。
        /// </summary>
        [Description("数值")]
        Number,

        /// <summary>
        /// 字符串。
        /// </summary>
        [Description("字符串")]
        String,

        /// <summary>
        /// 对象。
        /// </summary>
        [Description("对象")]
        Object,

        /// <summary>
        /// 函数。
        /// </summary>
        [Description("函数")]
        Function,

        /// <summary>
        /// 数组。
        /// </summary>
        [Description("数组")]
        Array,

        /// <summary>
        /// 日期。
        /// </summary>
        [Description("日期")]
        Date,

        /// <summary>
        /// 属性。
        /// </summary>
        [Description("属性")]
        Property,

        /// <summary>
        /// JSON。
        /// </summary>
        [Description("JSON")]
        Json
    }
}
