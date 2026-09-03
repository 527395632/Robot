// Robot 桌面软件 — JavaScript Promise 扩展
// 为 CefV8Context 提供创建 Promise 上下文(含 resolve/reject)的扩展方法

using System;
using Xilium.CefGlue;

namespace Robot.JavaScript
{

    /// <summary>
    /// JavaScript Promise 扩展:为 CefV8Context 提供创建 Promise 上下文(含 resolve/reject)的扩展方法。
    /// </summary>
    public static class JavaScriptPromiseExtensions
    {
        /// <summary>
        /// 在指定 V8 上下文中创建 Promise 上下文并返回其 promise 值。
        /// </summary>
        /// <param name="context">创建 Promise 的 V8 上下文。</param>
        /// <param name="uuid">Promise 唯一标识。</param>
        /// <returns>创建得到的 promise 值。</returns>
        /// <exception cref="ArgumentException">无法创建 Promise 对象时抛出。</exception>
        public static CefV8Value CreateJavaScriptPromiseContext(this CefV8Context context, Guid uuid)
        {
            var promiseCreationCode = """"
    (()=>{
        const result = {};
        const promise = new Promise((resolve,reject)=>{
            result.resolve = resolve;
            result.reject = reject;
        });
        result.promise = promise;
        return result;
    })();
    """";

            if (!context.TryEval(promiseCreationCode, context.GetFrame().Url, 0, out var returnValue, out _) || returnValue == null)
            {
                throw new ArgumentException("Cannot create JavaScript promise object.");
            }

            var promise = returnValue?.GetValue("promise");

            if(returnValue == null ||promise == null)
            {
                throw new ArgumentException("Cannot create JavaScript promise object.");
            }

            var promiseFunction = new JavaScriptPromiseContext(uuid, context, returnValue);

            JavaScriptEngineBridge.JavaScriptPromiseContexts.Add(promiseFunction);

            return promise;
        }
    }
}
