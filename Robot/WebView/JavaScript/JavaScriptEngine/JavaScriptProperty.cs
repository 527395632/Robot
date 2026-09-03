// Robot 桌面软件 — JavaScript 属性
// 表示带访问器/设置器的 JavaScript 对象属性,支持可写性控制

using System;
using System.Text.Json;

namespace Robot.JavaScript
{

    /// <summary>
    /// JavaScript 属性:表示带访问器/设置器的 JavaScript 对象属性,支持可写性控制。
    /// </summary>
    public class JavaScriptProperty : JavaScriptValue
    {
        /// <summary>
        /// 是否可写。
        /// </summary>
        public bool Writable { get; internal set; }

        /// <summary>
        /// 属性读取委托;为 null 时不可读。
        /// </summary>
        public Func<JavaScriptValue>? Getter { get; set; }

        /// <summary>
        /// 属性写入委托(内部存储)。
        /// </summary>
        Action<JavaScriptValue>? _setter;

        /// <summary>
        /// 属性写入委托;设置非 null 时自动置为可写。
        /// </summary>
        public Action<JavaScriptValue>? Setter
        {
            get => _setter; set
            {
                _setter = value;

                if (_setter != null)
                    Writable = true;
                else
                    Writable = false;
            }
        }

        /// <summary>
        /// 初始化 <see cref="JavaScriptProperty"/> 实例。
        /// </summary>
        internal JavaScriptProperty() : base(JavaScriptValueType.Property) { }

        /// <summary>
        /// 生成该属性的值定义。
        /// </summary>
        /// <returns>承载该属性元数据的值定义。</returns>
        internal override JavaScriptValueDefinition ToDefinition()
        {
            return new JavaScriptValueDefinition
            {
                Name = Name,
                Uuid = Uuid,
                ValueType = ValueType,
                ValueDefinition = new JavaScriptPropertyDefinition { Writable = Writable}
            };
        }

        /// <summary>
        /// 从 JSON 反序列化出 JavaScript 属性。
        /// </summary>
        /// <param name="json">JSON 字符串。</param>
        /// <returns>反序列化得到的属性;JSON 为 null 时返回 null。</returns>
        public static new JavaScriptProperty? FromJson(string json)
        {
            return FromDefinition(JsonSerializer.Deserialize<JavaScriptPropertyDefinition>(json));
        }

        /// <summary>
        /// 从属性定义构建 JavaScript 属性。
        /// </summary>
        /// <param name="definition">属性定义。</param>
        /// <returns>构建得到的属性;入参为 null 时返回 null。</returns>
        internal static JavaScriptProperty? FromDefinition(JavaScriptPropertyDefinition? definition)
        {
            if (definition == null)
            {
                return null;
            }

            var value = new JavaScriptProperty
            {
                Writable = definition.Writable
            };

            return value;
        }
    }
}
