// Robot 桌面软件 — JavaScript 函数调用器扩展
// 为 JavaScriptValue 提供转换为函数调用器并异步执行的扩展方法

using System;
using System.Threading.Tasks;

namespace Robot.JavaScript
{

    /// <summary>
    /// JavaScript 函数调用器扩展:为 JavaScriptValue 提供转换为函数调用器并异步执行的扩展方法。
    /// </summary>
    public static class JavaScriptFunctionInvokerExtensions
    {
        /// <summary>
        /// 将 JavaScript 值转换为函数调用器。
        /// </summary>
        /// <param name="jsValue">待转换的 JavaScript 值。</param>
        /// <returns>转换得到的函数调用器。</returns>
        /// <exception cref="InvalidOperationException">该值不是函数类型时抛出。</exception>
        public static JavaScriptFunctionInvoker ToFunction(this JavaScriptValue jsValue)
        {
            if (jsValue != null && jsValue.ValueType == JavaScriptValueType.Function && jsValue is JavaScriptFunctionInvoker)
            {
                return (JavaScriptFunctionInvoker)jsValue;
            }
            else
            {
                throw new InvalidOperationException($"This is not a {nameof(JavaScriptFunctionInvoker)}.");
            }
        }

        /// <summary>
        /// 异步执行 JavaScript 值对应的函数(以可变参数形式传入实参)。
        /// </summary>
        /// <param name="jsValue">待执行的 JavaScript 值。</param>
        /// <param name="arguments">函数实参。</param>
        /// <returns>承载执行结果的任务。</returns>
        /// <exception cref="InvalidOperationException">该值不是函数类型时抛出。</exception>
        public static Task<JavaScriptResult> ExecuteAsync(this JavaScriptValue jsValue, params JavaScriptValue[] arguments)
        {
            if (jsValue.ValueType != JavaScriptValueType.Function && !(jsValue is JavaScriptFunctionInvoker)) throw new InvalidOperationException($"{nameof(ExecuteAsync)} is only allowed for JavaScriptFunction type.");

            return ((JavaScriptFunctionInvoker)jsValue).ExecuteAsync(arguments);
        }

        /// <summary>
        /// 异步执行 JavaScript 值对应的函数(以参数数组形式传入实参)。
        /// </summary>
        /// <param name="jsValue">待执行的 JavaScript 值。</param>
        /// <param name="arguments">函数实参数组;为 null 时使用空数组。</param>
        /// <returns>承载执行结果的任务。</returns>
        /// <exception cref="InvalidOperationException">该值不是函数类型时抛出。</exception>
        public static Task<JavaScriptResult> ExecuteAsync(this JavaScriptValue jsValue, JavaScriptArray? arguments = null)
        {
            if (jsValue.ValueType != JavaScriptValueType.Function && !(jsValue is JavaScriptFunctionInvoker)) throw new InvalidOperationException($"{nameof(ExecuteAsync)} is only allowed for JavaScriptFunction type.");

            return ((JavaScriptFunctionInvoker)jsValue).ExecuteAsync(arguments);
        }
    }
}
