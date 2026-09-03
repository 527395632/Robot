// Robot 桌面软件 — JavaScript 异步函数值
// 表示异步 JavaScript 函数的 JavaScriptValue 子类,执行时通过委托回调数组与 Promise

using System;
using Xilium.CefGlue;

namespace Robot.JavaScript
{

    /// <summary>
    /// JavaScript 异步函数值:以委托形式承载异步函数体,执行时回调数组与 Promise。
    /// </summary>
    public class JavaScriptAsynchronousFunction : JavaScriptValue
    {
        /// <summary>
        /// 异步函数委托:接收参数数组与 Promise。
        /// </summary>
        internal Action<JavaScriptArray, JavaScriptPromise> FunctionDelegate { get; }

        /// <summary>
        /// 初始化 <see cref="JavaScriptAsynchronousFunction"/> 实例并关联目标帧。
        /// </summary>
        /// <param name="frame">目标帧。</param>
        /// <param name="action">异步函数委托。</param>
        internal JavaScriptAsynchronousFunction(CefFrame frame, Action<JavaScriptArray, JavaScriptPromise> action)
        : base(JavaScriptValueType.Function)
        {
            Frame = frame;
            FunctionDelegate = action;
        }

        /// <summary>
        /// 初始化 <see cref="JavaScriptAsynchronousFunction"/> 实例。
        /// </summary>
        /// <param name="action">异步函数委托。</param>
        public JavaScriptAsynchronousFunction(Action<JavaScriptArray, JavaScriptPromise> action)
        : base(JavaScriptValueType.Function)
        {
            FunctionDelegate = action;
        }

        /// <summary>
        /// 转换为 JavaScript 值定义(标记为异步、主进程侧)。
        /// </summary>
        /// <returns>对应的 JavaScript 值定义。</returns>
        internal override JavaScriptValueDefinition ToDefinition()
        {
            return new JavaScriptValueDefinition
            {
                Name = Name,
                Uuid = Uuid,
                ValueType = ValueType,
                ValueDefinition = new JavaScriptFunctionInvokerDefinition
                {
                    IsAsynchronous = true,
                    IsRenderer = false
                },
            };
        }
    }
}
