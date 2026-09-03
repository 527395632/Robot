// Robot 桌面软件 — 窗口绑定对象
// 抽象基类,将 C# 原生函数注册为可被 JavaScript 调用的窗口绑定函数

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using Robot.Browser;
using Xilium.CefGlue;

namespace Robot.JavaScript
{

    /// <summary>
    /// 窗口绑定对象:抽象基类,将 C# 原生函数注册为可被 JavaScript 调用的窗口绑定函数。
    /// </summary>
    public abstract class JavaScriptWindowBindingObject : CefV8Handler
    {
        /// <summary>
        /// 防止被 GC 回收的句柄。
        /// </summary>
        GCHandle _gcHandle;

        /// <summary>
        /// 初始化 <see cref="JavaScriptWindowBindingObject"/> 实例。
        /// </summary>
        protected JavaScriptWindowBindingObject()
        {
            _gcHandle = GCHandle.Alloc(this);
        }

        /// <summary>
        /// 对象名称。
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// 注入到页面的 JavaScript 绑定代码。
        /// </summary>
        public abstract string JavaScriptWindowBindingCode { get; }

        /// <summary>
        /// 已注册的窗口绑定函数集合。
        /// </summary>
        internal List<JavaScriptWindowBindingObjectFunction> WindowBindingFunctions { get; } = new();

        #region 本地侧

        // 同步函数
        /// <summary>
        /// 注册一个本地侧同步原生函数(以委托方法名作为函数名)。
        /// </summary>
        /// <param name="functionDelegate">同步函数委托。</param>
        internal protected void RegisterSynchronousNativeFunction(Func<RobotWindow, JavaScriptArray, JavaScriptValue?> functionDelegate)
        {
            RegisterSynchronousNativeFunction(functionDelegate.Method.Name, functionDelegate);
        }

        /// <summary>
        /// 注册一个本地侧同步原生函数。
        /// </summary>
        /// <param name="functionName">函数名称。</param>
        /// <param name="functionDelegate">同步函数委托。</param>
        internal protected void RegisterSynchronousNativeFunction(string functionName, Func<RobotWindow, JavaScriptArray, JavaScriptValue?> functionDelegate)
        {
            WindowBindingFunctions.Add(new JavaScriptWindowBindingObjectFunction(functionDelegate) { FunctionName = functionName });
        }

        // 异步函数
        /// <summary>
        /// 注册一个本地侧异步原生函数(以委托方法名作为函数名)。
        /// </summary>
        /// <param name="functionDelegate">异步函数委托。</param>
        internal protected void RegisterAsynchronousNativeFunction(Action<RobotWindow, JavaScriptArray, JavaScriptPromise> functionDelegate)
        {
            RegisterAsynchronousNativeFunction(functionDelegate.Method.Name, functionDelegate);
        }

        /// <summary>
        /// 注册一个本地侧异步原生函数。
        /// </summary>
        /// <param name="functionName">函数名称。</param>
        /// <param name="functionDelegate">异步函数委托。</param>
        internal protected void RegisterAsynchronousNativeFunction(string functionName, Action<RobotWindow, JavaScriptArray, JavaScriptPromise> functionDelegate)
        {
            WindowBindingFunctions.Add(new JavaScriptWindowBindingObjectFunction(functionDelegate) { FunctionName = functionName });
        }

        #endregion

        #region 远端侧

        // 同步函数
        /// <summary>
        /// 注册一个远端侧同步原生函数(以委托方法名作为函数名)。
        /// </summary>
        /// <param name="functionDelegate">同步函数委托。</param>
        internal protected void RegisterSynchronousNativeFunction(Func<JavaScriptArray, JavaScriptValue?> functionDelegate)
        {
            RegisterSynchronousNativeFunction(functionDelegate.Method.Name, functionDelegate);
        }

        /// <summary>
        /// 注册一个远端侧同步原生函数。
        /// </summary>
        /// <param name="functionName">函数名称。</param>
        /// <param name="functionDelegate">同步函数委托。</param>
        internal protected void RegisterSynchronousNativeFunction(string functionName, Func<JavaScriptArray, JavaScriptValue?> functionDelegate)
        {
            WindowBindingFunctions.Add(new JavaScriptWindowBindingObjectFunction(functionDelegate) { FunctionName = functionName });
        }

        // 异步函数
        /// <summary>
        /// 注册一个远端侧异步原生函数(以委托方法名作为函数名)。
        /// </summary>
        /// <param name="functionDelegate">异步函数委托。</param>
        internal protected void RegisterAsynchronousNativeFunction(Action<JavaScriptArray, JavaScriptPromise> functionDelegate)
        {
            RegisterAsynchronousNativeFunction(functionDelegate.Method.Name, functionDelegate);
        }

        /// <summary>
        /// 注册一个远端侧异步原生函数。
        /// </summary>
        /// <param name="functionName">函数名称。</param>
        /// <param name="functionDelegate">异步函数委托。</param>
        internal protected void RegisterAsynchronousNativeFunction(string functionName, Action<JavaScriptArray, JavaScriptPromise> functionDelegate)
        {
            WindowBindingFunctions.Add(new JavaScriptWindowBindingObjectFunction(functionDelegate) { FunctionName = functionName });
        }

        #endregion

        /// <summary>
        /// 执行被调用的窗口绑定函数:按函数类型转发到本地/远端执行并回填返回值或异常。
        /// </summary>
        /// <param name="name">被调用的函数名。</param>
        /// <param name="obj">调用发生的对象。</param>
        /// <param name="arguments">函数实参数组。</param>
        /// <param name="returnValue">回填给 V8 的返回值。</param>
        /// <param name="exception">回填给 V8 的异常信息;无异常时为 null。</param>
        /// <returns>始终返回 true,表示已处理该调用。</returns>
        protected override bool Execute(string name, CefV8Value obj, CefV8Value[] arguments, out CefV8Value returnValue, out string exception)
        {
            var context = CefV8Context.GetCurrentContext();
            var browser = context.GetBrowser();
            var frame = context.GetFrame();

            var func = WindowBindingFunctions.SingleOrDefault(x => x.FunctionName == name);

            if (func == null)
            {
                exception = $"[{nameof(Robot)}]: Native Function `{name}` is not defined.";
                returnValue = null;
                return true;
            }

            var args = new JavaScriptArray();

            foreach (var arg in arguments)
            {
                args.Add(arg.ToJavaScriptValue());
            }

            exception = null;

            switch (func.FunctionType)
            {
                case JavaScriptWindowBindingObjectFunctionType.SynchronousFunctionOnLocal:
                    {
                        var response = MessageBridge.ExecuteRequest(new MessageBridgeRequest
                        {
                            Name = JavaScriptWindowBindingObjectBridge.EXECUTE_WINDOW_BINDING_OBJECT_SYNC_FUNCTION_MESSAGE,
                            BrowserId = browser.Identifier,
                            FrameId = frame.Identifier,
                            IsRemote = true,
                            Payload = JsonSerializer.Serialize(new JavaScriptWindowBindingObjectMessage
                            {
                                ObjectName = Name,
                                Uuid = func.Uuid,
                                FunctionName = func.FunctionName,
                                Arguments = args.ToJson()
                            })
                        });

                        if (response.IsSuccess && response.Data != null)
                        {
                            var retval = JavaScriptValue.FromJson(response.Data).ToCefV8Value();

                            if (retval != null)
                            {
                                returnValue = retval;
                            }
                            else
                            {
                                returnValue = CefV8Value.CreateUndefined();
                            }
                        }
                        else
                        {
                            returnValue = null;
                            exception = response.Exception ?? string.Empty;
                        }
                    }
                    break;

                case JavaScriptWindowBindingObjectFunctionType.AsynchronousFunctionOnLocal:
                    {
                        var response = MessageBridge.ExecuteRequest(new MessageBridgeRequest
                        {
                            Name = JavaScriptWindowBindingObjectBridge.EXECUTE_WINDOW_BINDING_OBJECT_ASYNC_FUNCTION_MESSAGE,
                            BrowserId = browser.Identifier,
                            FrameId = frame.Identifier,
                            IsRemote = true,
                            Payload = JsonSerializer.Serialize(new JavaScriptWindowBindingObjectMessage
                            {
                                ObjectName = Name,
                                Uuid = func.Uuid,
                                FunctionName = func.FunctionName,
                                Arguments = args.ToJson()
                            })
                        });

                        if (response.IsSuccess)
                        {
                            returnValue = context.CreateJavaScriptPromiseContext(func.Uuid);

                            exception = null;
                        }
                        else
                        {
                            returnValue = null;
                            exception = response.Exception ?? string.Empty;
                        }
                    }
                    break;

                case JavaScriptWindowBindingObjectFunctionType.SynchronousFunctionOnRemote:
                    {
                        if (func.SynchronousFunctionOnRemote == null)
                        {
                            returnValue = null;
                            exception = $"[{nameof(Robot)}]: Synchronous function `{name}` has no function handler.";

                            break;
                        }

                        var retval = func.SynchronousFunctionOnRemote.Invoke(args);

                        returnValue = retval?.ToCefV8Value() ?? CefV8Value.CreateUndefined();
                    }
                    break;
                case JavaScriptWindowBindingObjectFunctionType.AsynchronousFunctionOnRemote:
                    {
                        if (func.AsynchronousFunctionOnRemote == null)
                        {
                            returnValue = null;
                            exception = $"[{nameof(Robot)}]: Asynchronous function `{name}` has no function handler.";

                            break;
                        }

                        func.AsynchronousFunctionOnRemote.Invoke(args, new JavaScriptPromise(frame, func.Uuid, true));

                        returnValue = context.CreateJavaScriptPromiseContext(func.Uuid);
                    }
                    break;
                default:
                    returnValue = null;
                    break;
            }

            return true;
        }

        /// <summary>
        /// 释放 GC 句柄。
        /// </summary>
        /// <param name="disposing">是否由显式 Dispose 触发。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _gcHandle.Free();
            }

            base.Dispose(disposing);
        }
    }
}
