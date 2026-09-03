// Robot 桌面软件 — 窗口绑定对象桥
// 管理窗口绑定对象的注册与跨进程函数执行请求的分发

using System;
using System.Collections.Generic;
using Robot.Browser;
using Xilium.CefGlue;

namespace Robot.JavaScript
{

    /// <summary>
    /// 窗口绑定对象桥:管理窗口绑定对象的注册与跨进程函数执行请求的分发。
    /// </summary>
    internal partial class JavaScriptWindowBindingObjectBridge : MessageBridgeHandler
    {
        /// <summary>
        /// 执行窗口绑定对象同步函数的消息名。
        /// </summary>
        public static readonly string EXECUTE_WINDOW_BINDING_OBJECT_SYNC_FUNCTION_MESSAGE = "Robot.ExecuteWindowBindingObjectSynchronousFunction";

        /// <summary>
        /// 执行窗口绑定对象异步函数的消息名。
        /// </summary>
        public static readonly string EXECUTE_WINDOW_BINDING_OBJECT_ASYNC_FUNCTION_MESSAGE = "Robot.ExecuteWindowBindingObjectAsynchronousFunction";

        /// <summary>
        /// 执行窗口绑定对象消息投递的消息名。
        /// </summary>
        public static readonly string EXECUTE_WINDOW_BINDING_OBJECT_POST_MESSAGE_MESSAGE = "Robot.ExecuteWindowBindingObjectPostMessage";

        /// <summary>
        /// 待实例化的窗口绑定对象类型集合。
        /// </summary>
        public static List<Type> WindowBindingObjectTypes { get; } = new List<Type>();

        /// <summary>
        /// 已实例化的窗口绑定对象集合。
        /// </summary>
        public List<JavaScriptWindowBindingObject> WindowBindingObjects { get; } = new ();

        /// <summary>
        /// 初始化 <see cref="JavaScriptWindowBindingObjectBridge"/> 实例并按进程侧注册请求/消息处理器。
        /// </summary>
        /// <param name="bridge">消息桥。</param>
        /// <param name="target">调用方宿主实例。</param>
        public JavaScriptWindowBindingObjectBridge(MessageBridge bridge, RobotWindow target) : base(bridge)
        {
            InvokerInstance = target;

            if (!bridge.IsRenderer)
            {
                foreach (var type in WindowBindingObjectTypes)
                {
                    if (type == null || !type.IsSubclassOf(typeof(JavaScriptWindowBindingObject))) continue;

                    var instance = Activator.CreateInstance(type) as JavaScriptWindowBindingObject;
                    if (instance != null)
                    {
                        WindowBindingObjects.Add(instance);
                    }
                }

                RegisterRequestHandler(EXECUTE_WINDOW_BINDING_OBJECT_SYNC_FUNCTION_MESSAGE, HandleExecuteWindowBindingObjectSynchronousFunctionRequestOnLocal);

                RegisterRequestHandler(EXECUTE_WINDOW_BINDING_OBJECT_ASYNC_FUNCTION_MESSAGE, HandleExecuteWindowBindingObjectAsynchronousFunctionRequestOnLocal);
            }
            else
            {
                RegisterMessageHandler(EXECUTE_WINDOW_BINDING_OBJECT_POST_MESSAGE_MESSAGE, HandlePostBrowserMessageOnRemote);
            }
        }

        /// <summary>
        /// 向远端投递浏览器消息。
        /// </summary>
        /// <param name="frame">目标帧。</param>
        /// <param name="message">消息名。</param>
        /// <param name="value">消息数据;为 null 时不携带数据。</param>
        public void PostBrowserMessage(CefFrame frame, string message, JavaScriptValue? value)
        {
            MessageBridge.SendMessageToRemote(frame, new BridgeMessage(EXECUTE_WINDOW_BINDING_OBJECT_POST_MESSAGE_MESSAGE, new JavaScriptPostBrowserMessageMessage { Message = message, Data = value?.ToJson() }));
        }
    }
}
