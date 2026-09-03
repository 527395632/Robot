// Robot 桌面软件 — JavaScript 函数 Promise 任务(远程)
// 在远程进程根据执行结果 resolve 或 reject 已存储的 Promise 上下文

using System;
using System.Diagnostics;
using Xilium.CefGlue;

namespace Robot.JavaScript
{

    /// <summary>
    /// JavaScript 函数 Promise 任务(远程):根据执行结果 resolve 或 reject 已存储的 Promise 上下文。
    /// </summary>
    internal class ExecuteJavaScriptFunctionPromiseTaskOnRemote : CefTask
    {
        /// <summary>
        /// JavaScript 引擎桥。
        /// </summary>
        public JavaScriptEngineBridge Bridge { get; }

        /// <summary>
        /// 目标帧。
        /// </summary>
        public required CefFrame Frame { get; init; }

        /// <summary>
        /// 函数执行消息数据。
        /// </summary>
        public required ExecuteJavaScriptFunctionMessage TaskData { get; init; }

        /// <summary>
        /// 初始化 <see cref="ExecuteJavaScriptFunctionPromiseTaskOnRemote"/> 实例。
        /// </summary>
        /// <param name="bridge">JavaScript 引擎桥。</param>
        public ExecuteJavaScriptFunctionPromiseTaskOnRemote(JavaScriptEngineBridge bridge)
        {
            Bridge = bridge;
        }

        /// <summary>
        /// 执行任务:根据执行结果 resolve 或 reject 已存储的 Promise 上下文。
        /// </summary>
        protected override void Execute()
        {
            // 查找与任务函数 ID 匹配的 Promise 上下文
            var storedObject = JavaScriptEngineBridge.JavaScriptPromiseContexts.Find(x => x.Uuid == TaskData.FunctionId);

            if (storedObject != null)
            {
                try
                {
                    var context = Frame.V8Context ?? CefV8Context.GetCurrentContext();
                    using var global = context.GetGlobal();

                    if (TaskData.Success)
                    {
                        var args = JavaScriptValue.FromJson(TaskData.Data!);

                        CefV8Value[]? arguments;


                        context.Enter();
                        if (args.ValueType != JavaScriptValueType.Array || args == null)
                        {
                            arguments = new CefV8Value[] { };
                        }
                        else
                        {
                            arguments = args.ToArray().ToCefV8Arguments();
                        }
                        context.Exit();


                        storedObject.Resolve.ExecuteFunctionWithContext(context, global, arguments);
                    }
                    else
                    {
                        storedObject.Reject.ExecuteFunctionWithContext(context, global, new CefV8Value[] { CefV8Value.CreateString(TaskData.ExceptionText ?? string.Empty) });
                    }

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                finally
                {
                    JavaScriptEngineBridge.JavaScriptPromiseContexts.Remove(storedObject);
                    storedObject.Dispose();
                }

            }
        }
    }
}
