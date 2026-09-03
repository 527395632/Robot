// Robot 桌面软件 — JavaScript Promise 上下文
// 在 CEF 侧承载 Promise 的 resolve/reject 函数,供远端解析或拒绝时调用

using System;
using System.Diagnostics.CodeAnalysis;
using Xilium.CefGlue;

namespace Robot.JavaScript
{

    /// <summary>
    /// JavaScript Promise 上下文:在 CEF 侧承载 Promise 的 resolve/reject 函数,供远端解析或拒绝时调用。
    /// </summary>
    internal class JavaScriptPromiseContext : IDisposable
    {
        /// <summary>
        /// Promise 唯一标识。
        /// </summary>
        public required Guid Uuid { get; init; }

        /// <summary>
        /// Promise 关联的 V8 上下文。
        /// </summary>
        public CefV8Context Context { get; }

        /// <summary>
        /// Promise 的 resolve 函数。
        /// </summary>
        public CefV8Value Resolve { get; }

        /// <summary>
        /// Promise 的 reject 函数。
        /// </summary>
        public CefV8Value Reject { get; }

        /// <summary>
        /// 初始化 <see cref="JavaScriptPromiseContext"/> 实例。
        /// </summary>
        /// <param name="uuid">Promise 唯一标识。</param>
        /// <param name="context">Promise 关联的 V8 上下文。</param>
        /// <param name="promiseFunction">Promise 函数(用于取出 resolve/reject)。</param>
        [SetsRequiredMembers]
        public JavaScriptPromiseContext(Guid uuid, CefV8Context context, CefV8Value promiseFunction)
        {
            Uuid = uuid;
            Context = context;
            Resolve = promiseFunction.GetValue("resolve");
            Reject = promiseFunction.GetValue("reject");
        }

        /// <summary>
        /// 释放 resolve/reject 函数持有的 V8 资源。
        /// </summary>
        public void Dispose()
        {
            Resolve?.Dispose();
            Reject?.Dispose();
        }
    }
}
