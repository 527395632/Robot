// Robot 桌面软件 — 窗口绑定对象函数
// 承载一个窗口绑定函数的类型、名称与本地/远端同步/异步委托

using System;
using Robot.Browser;

namespace Robot.JavaScript
{

    /// <summary>
    /// 窗口绑定对象函数:承载一个窗口绑定函数的类型、名称与本地/远端同步/异步委托。
    /// </summary>
    internal sealed class JavaScriptWindowBindingObjectFunction
    {
        /// <summary>
        /// 函数唯一标识。
        /// </summary>
        public Guid Uuid { get; } = Guid.NewGuid();

        /// <summary>
        /// 函数类型。
        /// </summary>
        public JavaScriptWindowBindingObjectFunctionType FunctionType { get; init; }

        /// <summary>
        /// 函数名称。
        /// </summary>
        public required string FunctionName { get; init; }

        /// <summary>
        /// 本地侧同步函数委托。
        /// </summary>
        public Func<RobotWindow, JavaScriptArray, JavaScriptValue?>? SynchronousFunctionOnLocal { get; }

        /// <summary>
        /// 本地侧异步函数委托。
        /// </summary>
        public Action<RobotWindow, JavaScriptArray, JavaScriptPromise>? AsynchronousFunctionOnLocal { get; }

        /// <summary>
        /// 远端侧同步函数委托。
        /// </summary>
        public Func<JavaScriptArray, JavaScriptValue?>? SynchronousFunctionOnRemote { get; }

        /// <summary>
        /// 远端侧异步函数委托。
        /// </summary>
        public Action<JavaScriptArray, JavaScriptPromise>? AsynchronousFunctionOnRemote { get; }

        /// <summary>
        /// 以本地侧同步函数委托初始化 <see cref="JavaScriptWindowBindingObjectFunction"/> 实例。
        /// </summary>
        /// <param name="functionDelegate">本地侧同步函数委托。</param>
        public JavaScriptWindowBindingObjectFunction(Func<RobotWindow, JavaScriptArray, JavaScriptValue?> functionDelegate)
        {
            FunctionType = JavaScriptWindowBindingObjectFunctionType.SynchronousFunctionOnLocal;
            SynchronousFunctionOnLocal = functionDelegate;
        }

        /// <summary>
        /// 以本地侧异步函数委托初始化 <see cref="JavaScriptWindowBindingObjectFunction"/> 实例。
        /// </summary>
        /// <param name="functionDelegate">本地侧异步函数委托。</param>
        public JavaScriptWindowBindingObjectFunction(Action<RobotWindow, JavaScriptArray, JavaScriptPromise> functionDelegate)
        {
            FunctionType = JavaScriptWindowBindingObjectFunctionType.AsynchronousFunctionOnLocal;
            AsynchronousFunctionOnLocal = functionDelegate;
        }

        /// <summary>
        /// 以远端侧同步函数委托初始化 <see cref="JavaScriptWindowBindingObjectFunction"/> 实例。
        /// </summary>
        /// <param name="functionDelegate">远端侧同步函数委托。</param>
        public JavaScriptWindowBindingObjectFunction(Func<JavaScriptArray, JavaScriptValue?> functionDelegate)
        {
            FunctionType = JavaScriptWindowBindingObjectFunctionType.SynchronousFunctionOnRemote;
            SynchronousFunctionOnRemote = functionDelegate;
        }

        /// <summary>
        /// 以远端侧异步函数委托初始化 <see cref="JavaScriptWindowBindingObjectFunction"/> 实例。
        /// </summary>
        /// <param name="functionDelegate">远端侧异步函数委托。</param>
        public JavaScriptWindowBindingObjectFunction(Action<JavaScriptArray, JavaScriptPromise> functionDelegate)
        {
            FunctionType = JavaScriptWindowBindingObjectFunctionType.AsynchronousFunctionOnRemote;
            AsynchronousFunctionOnRemote = functionDelegate;
        }
    }
}
