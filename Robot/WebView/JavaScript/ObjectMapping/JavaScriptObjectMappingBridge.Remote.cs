// Robot 桌面软件 — 对象映射桥(远端部分)
// 处理远端侧的对象映射:上下文创建时映射对象,接收创建消息时投递任务到渲染线程

using Robot.Browser;
using Xilium.CefGlue;

namespace Robot.JavaScript
{

    /// <summary>
    /// 对象映射桥(远端部分):处理远端侧的对象映射,上下文创建时映射对象,接收创建消息时投递任务到渲染线程。
    /// </summary>
    internal partial class JavaScriptObjectMappingBridge
    {
        /// <summary>
        /// 远端上下文创建回调:映射对象。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="context">V8 上下文。</param>
        public override void OnRemoteContextCreated(CefBrowser browser, CefFrame frame, CefV8Context context)
        {
            MapObjects(frame);
        }

        /// <summary>
        /// 远端上下文释放回调(远端侧无处理)。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="context">V8 上下文。</param>
        public override void OnRemoteContextReleased(CefBrowser browser, CefFrame frame, CefV8Context context)
        {
        }

        /// <summary>
        /// 在远端处理对象映射创建消息:将对象创建任务投递到渲染线程。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="id">进程标识。</param>
        /// <param name="message">桥消息。</param>
        private void HandleCreateMappedJavaScriptObjectMessageOnRemote(CefBrowser browser, CefFrame frame, CefProcessId id, BridgeMessage message)
        {
            var data = message.DeserializeData<string>()!;

            //if(id == CefProcessId.Renderer)
            //{


            //}

            CefRuntime.PostTask(CefThreadId.Renderer, new JavaScriptObjectMapCreationTaskOnRemote(this, data)
            {
                Frame = frame,
            });
        }
    }
}
