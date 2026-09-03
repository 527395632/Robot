// Robot 桌面软件 — 远端 JavaScript 函数调用器
// 表示运行在渲染进程侧的 JavaScript 函数,承载其 V8 函数体

using Xilium.CefGlue;

namespace Robot.JavaScript
{

    /// <summary>
    /// 远端 JavaScript 函数调用器:表示运行在渲染进程侧的 JavaScript 函数,承载其 V8 函数体。
    /// </summary>
    internal class JavaScriptFunctionInvokerOnRemote : JavaScriptValue
    {
        //public CefV8Context Context { get; }

        /// <summary>
        /// 函数体对应的 V8 值。
        /// </summary>
        public CefV8Value FunctionBody { get; }

        /// <summary>
        /// 初始化 <see cref="JavaScriptFunctionInvokerOnRemote"/> 实例。
        /// </summary>
        /// <param name="func">函数体对应的 V8 值。</param>
        internal JavaScriptFunctionInvokerOnRemote(/*CefV8Context context, */CefV8Value func)
        : base(JavaScriptValueType.Function)
        {
            //Context = context;
            FunctionBody = func;
        }

        /// <summary>
        /// 生成该函数调用器的值定义(标记为同步、渲染进程侧)。
        /// </summary>
        /// <returns>承载该函数调用器元数据的值定义。</returns>
        internal override JavaScriptValueDefinition ToDefinition()
        {
            return new JavaScriptValueDefinition
            {
                Name = Name,
                Uuid = Uuid,
                ValueType = ValueType,
                ValueDefinition = new JavaScriptFunctionInvokerDefinition { IsAsynchronous = false, IsRenderer = true }
            };
        }

        /// <summary>
        /// 释放该函数调用器持有的 V8 函数体。
        /// </summary>
        /// <param name="isDisposing">是否由显式 Dispose 触发。</param>
        protected override void Dispose(bool isDisposing)
        {
            FunctionBody.Dispose();
        }
    }
}
