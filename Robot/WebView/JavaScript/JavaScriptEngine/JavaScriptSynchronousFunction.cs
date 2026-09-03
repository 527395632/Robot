// Robot 桌面软件 — JavaScript 同步函数
// 表示在主进程侧执行的同步 JavaScript 函数,承载其委托

using System;
using Xilium.CefGlue;

namespace Robot.JavaScript
{

    /// <summary>
    /// JavaScript 同步函数:表示在主进程侧执行的同步 JavaScript 函数,承载其委托。
    /// </summary>
    public class JavaScriptSynchronousFunction : JavaScriptValue
    {
        /// <summary>
        /// 同步函数委托。
        /// </summary>
        internal Func<JavaScriptArray, JavaScriptValue?> FunctionDelegate { get; }

        /// <summary>
        /// 以帧与委托初始化 <see cref="JavaScriptSynchronousFunction"/> 实例。
        /// </summary>
        /// <param name="frame">函数关联的帧。</param>
        /// <param name="func">同步函数委托。</param>
        internal JavaScriptSynchronousFunction(CefFrame frame, Func<JavaScriptArray, JavaScriptValue?> func)
        : base(JavaScriptValueType.Function)
        {
            Frame = frame;
            FunctionDelegate = func;
        }

        /// <summary>
        /// 以委托初始化 <see cref="JavaScriptSynchronousFunction"/> 实例。
        /// </summary>
        /// <param name="func">同步函数委托。</param>
        public JavaScriptSynchronousFunction(Func<JavaScriptArray, JavaScriptValue?> func)
        : base(JavaScriptValueType.Function)
        {
            FunctionDelegate = func;
        }

        /// <summary>
        /// 生成该同步函数的值定义(标记为同步、主进程侧)。
        /// </summary>
        /// <returns>承载该同步函数元数据的值定义。</returns>
        internal override JavaScriptValueDefinition ToDefinition()
        {
            return new JavaScriptValueDefinition
            {
                Name = Name,
                Uuid = Uuid,
                ValueType = ValueType,
                ValueDefinition = new JavaScriptFunctionInvokerDefinition
                {
                    IsAsynchronous = false,
                    IsRenderer = false
                },
            };
        }
    }
}
