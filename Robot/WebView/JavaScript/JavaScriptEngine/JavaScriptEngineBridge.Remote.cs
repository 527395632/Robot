// Robot 桌面软件 — JavaScript 引擎桥(远程部分)
// 在远程(渲染)进程处理 JavaScript 求值、函数执行、Promise 执行等消息

using Robot.Browser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xilium.CefGlue;

namespace Robot.JavaScript
{

    internal partial class JavaScriptEngineBridge
    {
        /// <summary>
        /// 已存储的 JavaScript Promise 上下文集合。
        /// </summary>
        internal static List<JavaScriptPromiseContext> JavaScriptPromiseContexts { get; } = new();



        /// <summary>
        /// 远程 V8 上下文创建回调:释放目标帧关联的已存储对象。
        /// </summary>
        /// <param name="browser">浏览器实例。</param>
        /// <param name="frame">目标帧。</param>
        /// <param name="context">V8 上下文。</param>
        public override void OnRemoteContextCreated(CefBrowser browser, CefFrame frame, CefV8Context context)
        {
            ReleaseStoredObjectByContext(frame);
        }

        /// <summary>
        /// 远程 V8 上下文释放回调:释放目标帧关联的已存储对象。
        /// </summary>
        /// <param name="browser">浏览器实例。</param>
        /// <param name="frame">目标帧。</param>
        /// <param name="context">V8 上下文。</param>
        public override void OnRemoteContextReleased(CefBrowser browser, CefFrame frame, CefV8Context context)
        {
            ReleaseStoredObjectByContext(frame);
        }

        /// <summary>
        /// 释放目标帧关联的已存储对象与 Promise 上下文。
        /// </summary>
        /// <param name="frame">目标帧。</param>
        private void ReleaseStoredObjectByContext(CefFrame frame)
        {
            try
            {
                JavaScriptValue.Release(frame);

                var storedPromises = JavaScriptPromiseContexts.Where(x => x.Context.IsSame(frame.V8Context)).ToArray();


                for (var i = 0; i < storedPromises.Length; i++)
                {
                    var func = storedPromises[i];
                    JavaScriptPromiseContexts.Remove(func);
                    func?.Dispose();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// 处理远程 JavaScript 求值消息:在渲染线程执行求值任务。
        /// </summary>
        /// <param name="browser">浏览器实例。</param>
        /// <param name="frame">目标帧。</param>
        /// <param name="processId">进程标识。</param>
        /// <param name="message">桥消息。</param>
        private void HandleEvaluateJavaScriptMessageOnRemote(CefBrowser browser, CefFrame frame, CefProcessId processId, BridgeMessage message)
        {
            var data = message.DeserializeData<EvaluateJavaScriptMessage>()!;


            CefRuntime.PostTask(CefThreadId.Renderer, new EvaluateJavaScriptTaskOnRemote(this) { Frame = frame, TaskData = data });
        }

        /// <summary>
        /// 处理远程 JavaScript 函数执行消息:在渲染线程执行函数任务。
        /// </summary>
        /// <param name="browser">浏览器实例。</param>
        /// <param name="frame">目标帧。</param>
        /// <param name="id">进程标识。</param>
        /// <param name="message">桥消息。</param>
        private void HandleExecuteJavaScriptFunctionMessageOnRemote(CefBrowser browser, CefFrame frame, CefProcessId id, BridgeMessage message)
        {

            var data = message.DeserializeData<ExecuteJavaScriptFunctionMessage>()!;

            CefRuntime.PostTask(CefThreadId.Renderer, new ExecuteJavaScriptFunctionTaskOnRemote(this) { Frame = frame, TaskData = data });
        }

        /// <summary>
        /// 处理远程 JavaScript Promise 执行消息:在渲染线程执行 Promise 任务。
        /// </summary>
        /// <param name="browser">浏览器实例。</param>
        /// <param name="frame">目标帧。</param>
        /// <param name="id">进程标识。</param>
        /// <param name="message">桥消息。</param>
        private void HandleExecuteJavaScriptPromiseMessageOnRemote(CefBrowser browser, CefFrame frame, CefProcessId id, BridgeMessage message)
        {
            var data = message.DeserializeData<ExecuteJavaScriptFunctionMessage>()!;

            CefRuntime.PostTask(CefThreadId.Renderer, new ExecuteJavaScriptFunctionPromiseTaskOnRemote(this) { Frame = frame, TaskData = data });
        }
    }
}
