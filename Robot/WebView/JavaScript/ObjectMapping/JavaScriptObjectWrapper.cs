// Robot 桌面软件 — JavaScript 对象包装器
// 抽象基类,将 C# 对象字段/属性/函数映射到宿主 JavaScript 对象

using System;

namespace Robot.JavaScript
{

    /// <summary>
    /// JavaScript 对象包装器:抽象基类,将 C# 对象字段/属性/函数映射到宿主 JavaScript 对象。
    /// </summary>
    public abstract class JavaScriptObjectWrapper
    {
        /// <summary>
        /// 宿主 JavaScript 对象。
        /// </summary>
        internal JavaScriptObject HostObject { get; } = new();

        /// <summary>
        /// 添加一个字段。
        /// </summary>
        /// <param name="name">字段名称。</param>
        /// <param name="value">字段值。</param>
        protected void AddField(string name, JavaScriptValue value)
        {
            HostObject.Add(name, value);
        }

        /// <summary>
        /// 定义一个带访问器/设置器的属性。
        /// </summary>
        /// <param name="name">属性名称。</param>
        /// <param name="getter">属性读取委托。</param>
        /// <param name="setter">属性写入委托;为 null 时只读。</param>
        protected void DefineProperty(string name, Func<JavaScriptValue> getter, Action<JavaScriptValue>? setter = null)
        {
            HostObject.DefineProperty(name, getter, setter);
        }

        /// <summary>
        /// 添加一个同步函数。
        /// </summary>
        /// <param name="name">函数名称。</param>
        /// <param name="functionInvoker">同步函数委托。</param>
        protected void AddSynchronousFunction(string name, Func<JavaScriptArray, JavaScriptValue?> functionInvoker)
        {
            HostObject.Add(name, functionInvoker);
        }

        /// <summary>
        /// 添加一个异步函数。
        /// </summary>
        /// <param name="name">函数名称。</param>
        /// <param name="functionInvoker">异步函数委托。</param>
        protected void AddAsynchronousFunction(string name, Action<JavaScriptArray,JavaScriptPromise> functionInvoker)
        {
            HostObject.Add(name, functionInvoker);
        }
    }
}
