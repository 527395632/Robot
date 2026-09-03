// Robot 桌面软件 — 对象映射桥
// 管理跨进程的 JavaScript 对象注册与映射,按进程侧分发本地/远端消息

using Robot.Browser;
using System;
using System.Collections.Generic;
using System.Linq;
using Xilium.CefGlue;

namespace Robot.JavaScript
{

    /// <summary>
    /// 对象映射桥:管理跨进程的 JavaScript 对象注册与映射,按进程侧分发本地/远端消息。
    /// </summary>
    internal partial class JavaScriptObjectMappingBridge : MessageBridgeHandler
    {
        /// <summary>
        /// 创建已映射 JavaScript 对象的消息名。
        /// </summary>
        public static readonly string CREATE_MAPPED_JAVASCRIPT_OBJECTS_MESSAGE = "Robot.CreateMappedJavascriptObjects";

        /// <summary>
        /// 初始化已映射 JavaScript 对象的消息名。
        /// </summary>
        public static readonly string INITIALIZE_MAPPED_JAVASCRIPT_OBJECTS_MESSAGE = "Robot.InitializeMappedJavaScriptObjects";

        /// <summary>
        /// 已注册的对象集合(按帧与名称索引)。
        /// </summary>
        Dictionary<(CefFrame frame, string name), JavaScriptObject> Objects { get; } = new();

        /// <summary>
        /// 对象是否已初始化。
        /// </summary>
        internal bool IsObjectsInitialized { get; private set; } = false;

        /// <summary>
        /// 初始化 <see cref="JavaScriptObjectMappingBridge"/> 实例并按进程侧注册消息处理器。
        /// </summary>
        /// <param name="bridge">消息桥。</param>
        public JavaScriptObjectMappingBridge(MessageBridge bridge) : base(bridge)
        {
            // 本地消息
            if(!IsRenderer)
            {
                RegisterMessageHandler(INITIALIZE_MAPPED_JAVASCRIPT_OBJECTS_MESSAGE, HandleInitializeJavaScriptObjectMessageOnLocal);
            }

            // 远端消息
            if(IsRenderer)
            {
                RegisterMessageHandler(CREATE_MAPPED_JAVASCRIPT_OBJECTS_MESSAGE, HandleCreateMappedJavaScriptObjectMessageOnRemote);
            }
        }

        /// <summary>
        /// 向本地发送对象映射初始化消息。
        /// </summary>
        /// <param name="frame">目标帧。</param>
        private void MapObjects(CefFrame frame)
        {
            MessageBridge.SendMessageToLocal(frame, new BridgeMessage(INITIALIZE_MAPPED_JAVASCRIPT_OBJECTS_MESSAGE));
        }

        /// <summary>
        /// 开始注册 JavaScript 对象。
        /// </summary>
        /// <param name="frame">目标帧。</param>
        /// <returns>注册句柄。</returns>
        /// <exception cref="InvalidOperationException">该帧已有未结束的注册时抛出。</exception>
        public JavaScriptObjectRegisterHandle BeginRegisterJavaScriptObject(CefFrame frame)
        {
            if (JavaScriptObjectRegisterHandle.Exists(frame))
                throw new InvalidOperationException($"This method can be only called once until {nameof(EndRegisterJavaScriptObject)} be called.");

            return new JavaScriptObjectRegisterHandle(frame);
        }

        /// <summary>
        /// 结束注册 JavaScript 对象。
        /// </summary>
        /// <param name="handle">注册句柄。</param>
        /// <exception cref="InvalidOperationException">该帧没有进行中的注册时抛出。</exception>
        public void EndRegisterJavaScriptObject(JavaScriptObjectRegisterHandle handle)
        {
            if (!JavaScriptObjectRegisterHandle.Exists(handle.Frame))
                throw new InvalidOperationException($"This method can be only called once until {nameof(BeginRegisterJavaScriptObject)} be called.");

            var frame = handle.Frame;

            JavaScriptObjectRegisterHandle.Handles.Remove(handle);

            if (IsObjectsInitialized)
            {
                MapObjects(frame);
            }
        }

        /// <summary>
        /// 注册一个 JavaScript 对象。
        /// </summary>
        /// <param name="handle">注册句柄。</param>
        /// <param name="name">对象名称。</param>
        /// <param name="jsObject">待注册的对象。</param>
        /// <returns>注册成功时返回 true;名称已存在时返回 false。</returns>
        /// <exception cref="InvalidOperationException">未先调用 BeginRegisterJavaScriptObject 时抛出。</exception>
        public bool RegisterJavaScriptObject(JavaScriptObjectRegisterHandle handle, string name, JavaScriptObject jsObject)
        {
            var frame = handle.Frame;

            if (!JavaScriptObjectRegisterHandle.Exists(frame))
            {
                throw new InvalidOperationException($"This method can be only called after {nameof(BeginRegisterJavaScriptObject)} be called.");
            }

            if (Objects.ContainsKey((frame, name)))
            {
                return false;
            }

            if(Objects.Keys.Any(x=>x.frame.Identifier == frame.Identifier && x.name == name))
            {
                return false;
            }

            jsObject.AssociateToFrame(frame);

            Objects.Add((frame, name), jsObject);

            return true;
        }
    }
}
