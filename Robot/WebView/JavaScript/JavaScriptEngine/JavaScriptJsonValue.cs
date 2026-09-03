// Robot 桌面软件 — JavaScript JSON 值
// 承载以 JSON 字符串形式表示的 JavaScript 值,支持反序列化为强类型对象

using System.Text.Json;

namespace Robot.JavaScript
{

    /// <summary>
    /// JavaScript JSON 值:承载以 JSON 字符串形式表示的 JavaScript 值,支持反序列化为强类型对象。
    /// </summary>
    public class JavaScriptJsonValue : JavaScriptValue
    {
        /// <summary>
        /// 以 JSON 字符串初始化 <see cref="JavaScriptJsonValue"/> 实例。
        /// </summary>
        /// <param name="source">JSON 字符串。</param>
        public JavaScriptJsonValue(string source)
            : base(JavaScriptValueType.Json, source) { }

        /// <summary>
        /// 以对象初始化 <see cref="JavaScriptJsonValue"/> 实例(内部序列化为 JSON 字符串)。
        /// </summary>
        /// <param name="source">待序列化的对象。</param>
        public JavaScriptJsonValue(object source)
            : base(JavaScriptValueType.Json, JsonSerializer.Serialize(source)) { }

        /// <summary>
        /// 将原始 JSON 值反序列化为指定类型对象。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <returns>反序列化得到的对象;原始值为 null 时返回默认值。</returns>
        /// <exception cref="JsonException">JSON 反序列化失败时抛出。</exception>
        public T? GetObject<T>()
        {
            var value = (string?)RawValue;

            if (value == null) return default(T);

            return JsonSerializer.Deserialize<T>(value);
        }
    }
}
