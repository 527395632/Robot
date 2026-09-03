// Robot 桌面软件 — 远端浏览器消息投递任务
// 在远端线程执行,将消息分发到宿主窗口的 internal.dispatchMessage

using System;
using System.Diagnostics;
using Xilium.CefGlue;

namespace Robot.JavaScript
{

    /// <summary>
    /// 远端浏览器消息投递任务:在远端线程执行,将消息分发到宿主窗口的 internal.dispatchMessage。
    /// </summary>
    internal class JavaScriptPostBrowserMessageTaskOnRemote : CefTask
    {
        /// <summary>
        /// 目标帧。
        /// </summary>
        public required CefFrame Frame { get; init; }

        /// <summary>
        /// 待投递的消息数据。
        /// </summary>
        public required JavaScriptPostBrowserMessageMessage TaskData { get; init; }

        /// <summary>
        /// 窗口绑定对象桥。
        /// </summary>
        public JavaScriptWindowBindingObjectBridge Bridge { get; }

        /// <summary>
        /// 初始化 <see cref="JavaScriptPostBrowserMessageTaskOnRemote"/> 实例。
        /// </summary>
        /// <param name="bridge">窗口绑定对象桥。</param>
        public JavaScriptPostBrowserMessageTaskOnRemote(JavaScriptWindowBindingObjectBridge bridge)
        {
            Bridge = bridge;
        }

        /// <summary>
        /// 执行任务:在 V8 上下文中调用宿主窗口的 dispatchMessage 分发消息。
        /// </summary>
        protected override void Execute()
        {
            var context = Frame.V8Context ?? CefV8Context.GetCurrentContext();

            context.Enter();

            try
            {
                var retval = TaskData.Data ==null ? new JavaScriptValue() :  JavaScriptValue.FromJson(TaskData.Data);

                using var global = context.GetGlobal();

                using var host = global.GetValue("host");

                if (host == null) return;

                using var hostWindow = host.GetValue("hostWindow");

                if (hostWindow == null) return;

                using var internalMethods = hostWindow.GetValue("internal");

                if (internalMethods == null) return;

                using var dispatchMessage = internalMethods.GetValue("dispatchMessage");

                if (dispatchMessage == null) return;

                using var args = retval.ToCefV8Value();

                dispatchMessage.ExecuteFunctionWithContext(context, hostWindow, new CefV8Value[] { CefV8Value.CreateString(TaskData.Message), args });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                context.Exit();
            }
        }
    }
}
