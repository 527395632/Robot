// Robot 桌面软件 — 窗口绑定对象桥(远端部分)
// 处理远端侧的浏览器消息投递,将任务投递到渲染线程

using Robot.Browser;
using Xilium.CefGlue;

namespace Robot.JavaScript
{

    /// <summary>
    /// 窗口绑定对象桥(远端部分):处理远端侧的浏览器消息投递,将任务投递到渲染线程。
    /// </summary>
    internal partial class JavaScriptWindowBindingObjectBridge
    {
        /// <summary>
        /// 在远端处理浏览器消息投递:将投递任务投递到渲染线程。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="id">进程标识。</param>
        /// <param name="message">桥消息。</param>
        private void HandlePostBrowserMessageOnRemote(CefBrowser browser, CefFrame frame, CefProcessId id, BridgeMessage message)
        {
            var data = message.DeserializeData<JavaScriptPostBrowserMessageMessage>()!;

            CefRuntime.PostTask(CefThreadId.Renderer, new JavaScriptPostBrowserMessageTaskOnRemote(this) { Frame = frame, TaskData = data });
        }

        /// <summary>
        /// 远端上下文创建回调(远端侧无处理)。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="context">V8 上下文。</param>
        public override void OnRemoteContextCreated(CefBrowser browser, CefFrame frame, CefV8Context context)
        {
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
    }
}
