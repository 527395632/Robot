// Robot 桌面软件 — 本地对象映射创建任务
// 在本地线程执行,将已映射的 JavaScript 对象打包发送到远端

using Robot.Browser;
using System.Collections.Generic;
using Xilium.CefGlue;

namespace Robot.JavaScript
{

    /// <summary>
    /// 本地对象映射创建任务:在本地线程执行,将已映射的 JavaScript 对象打包发送到远端。
    /// </summary>
    internal class JavaScriptObjectMapCreationTaskOnLocal : CefTask
    {
        /// <summary>
        /// 对象映射桥。
        /// </summary>
        public JavaScriptObjectMappingBridge Bridge { get; }

        /// <summary>
        /// 目标帧。
        /// </summary>
        public required CefFrame Frame { get; init; }

        /// <summary>
        /// 待映射的对象集合。
        /// </summary>
        public required Dictionary<string, JavaScriptObject> Objects { get; init; }

        /// <summary>
        /// 初始化 <see cref="JavaScriptObjectMapCreationTaskOnLocal"/> 实例。
        /// </summary>
        /// <param name="bridge">对象映射桥。</param>
        public JavaScriptObjectMapCreationTaskOnLocal(JavaScriptObjectMappingBridge bridge)
        {
            Bridge = bridge;
        }

        /// <summary>
        /// 执行任务:将对象集合打包为消息并发送到远端。
        /// </summary>
        protected override void Execute()
        {
            var message = new BridgeMessage(JavaScriptObjectMappingBridge.CREATE_MAPPED_JAVASCRIPT_OBJECTS_MESSAGE);

            var objects = new JavaScriptObject();

            objects.AssociateToFrame(Frame);

            foreach (var item in Objects)
            {
                objects.Add(item.Key, item.Value);
            }

            message.SetData(objects.ToJson());

            MessageBridge.SendMessageToRemote(Frame, message);
        }
    }
}
