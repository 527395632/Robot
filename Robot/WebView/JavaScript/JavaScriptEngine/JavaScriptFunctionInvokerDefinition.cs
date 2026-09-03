// Robot 桌面软件 — JavaScript 函数调用器定义
// 承载跨进程函数调用的元数据(是否异步、所在进程侧)

using System.Text.Json;

namespace Robot.JavaScript
{

    /// <summary>
    /// JavaScript 函数调用器定义:承载跨进程函数调用的元数据(是否异步、所在进程侧)。
    /// </summary>
    public class JavaScriptFunctionInvokerDefinition
    {
        /// <summary>
        /// 是否异步函数。
        /// </summary>
        public required bool IsAsynchronous { get; init; }

        /// <summary>
        /// 是否渲染进程侧。
        /// </summary>
        public required bool IsRenderer { get; init; }

        /// <summary>
        /// 从 JSON 反序列化出定义实例。
        /// </summary>
        /// <param name="json">JSON 字符串。</param>
        /// <returns>反序列化得到的定义实例。</returns>
        /// <exception cref="JsonException">反序列化失败时抛出。</exception>
        public static JavaScriptFunctionInvokerDefinition FromJson(string json)
        {
            var retval = JsonSerializer.Deserialize<JavaScriptFunctionInvokerDefinition>(json);
            if (retval == null) throw new JsonException($"Failed to deserialize {nameof(JavaScriptFunctionInvokerDefinition)}");
            return retval;
        }
    }
}
