// Robot 桌面软件 — JavaScript 对象扩展
// 为 JavaScriptValue 提供转换为 JavaScript 对象的扩展方法

using System;

namespace Robot.JavaScript
{

    /// <summary>
    /// JavaScript 对象扩展:为 JavaScriptValue 提供转换为 JavaScript 对象的扩展方法。
    /// </summary>
    public static class JavaScriptObjectExtension
    {
        /// <summary>
        /// 将 JavaScript 值转换为 JavaScript 对象。
        /// </summary>
        /// <param name="jsValue">待转换的 JavaScript 值。</param>
        /// <returns>转换得到的 JavaScript 对象。</returns>
        /// <exception cref="InvalidOperationException">该值不是对象类型时抛出。</exception>
        public static JavaScriptObject ToObject(this JavaScriptValue jsValue)
        {
            if (jsValue != null && jsValue.ValueType == JavaScriptValueType.Object)
            {
                return (JavaScriptObject)jsValue;
            }

            throw new InvalidOperationException($"This is not a {nameof(JavaScriptObject)}.");
        }
    }
}
