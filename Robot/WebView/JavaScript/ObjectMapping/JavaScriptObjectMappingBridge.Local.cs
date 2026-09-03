// Robot 桌面软件 — 对象映射桥(本地部分)
// 处理本地侧的对象映射初始化,将对象创建任务投递到 UI 线程

using Robot.Browser;
using System.Linq;
using Xilium.CefGlue;

namespace Robot.JavaScript
{

    /// <summary>
    /// 对象映射桥(本地部分):处理本地侧的对象映射初始化,将对象创建任务投递到 UI 线程。
    /// </summary>
    internal partial class JavaScriptObjectMappingBridge
    {
        /// <summary>
        /// 浏览前回调(本地侧无处理)。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="request">请求。</param>
        /// <param name="userGesture">是否用户手势触发。</param>
        /// <param name="isRedirect">是否重定向。</param>
        public override void OnBeforeBrowse(CefBrowser browser, CefFrame frame, CefRequest request, bool userGesture, bool isRedirect)
        {
        }

        /// <summary>
        /// 关闭前回调(本地侧无处理)。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        public override void OnBeforeClose(CefBrowser browser)
        {
        }

        /// <summary>
        /// 渲染进程终止回调(本地侧无处理)。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        public override void OnRenderProcessTerminated(CefBrowser browser)
        {
        }

        /// <summary>
        /// 在本地处理对象映射初始化消息:将对象创建任务投递到 UI 线程。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="id">进程标识。</param>
        /// <param name="message">桥消息。</param>
        private void HandleInitializeJavaScriptObjectMessageOnLocal(CefBrowser browser, CefFrame frame, CefProcessId id, BridgeMessage message)
        {
            CefRuntime.PostTask(CefThreadId.UI, new JavaScriptObjectMapCreationTaskOnLocal(this)
            {
                Frame = frame,
                Objects = Objects.Where(x => x.Key.frame.Identifier == frame.Identifier).ToDictionary(k => k.Key.name, v => v.Value)
            });

            IsObjectsInitialized = true;
        }
    }
}
