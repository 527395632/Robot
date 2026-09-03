// Robot 桌面软件 — 浏览器进程处理器
// 处理 CEF 浏览器进程生命周期:子进程启动、上下文初始化、资源释放

using Xilium.CefGlue;

namespace Robot.Browser
{

    /// <summary>
    /// 浏览器进程处理器:处理 CEF 浏览器进程生命周期(子进程启动、上下文初始化、资源释放)。
    /// </summary>
    internal class BrowserProcessHandler : CefBrowserProcessHandler
    {
        /// <summary>
        /// 浏览器应用实例。
        /// </summary>
        private WebViewApp app;

        /// <summary>
        /// 窗口绑定对象服务端(上下文初始化后创建)。
        /// </summary>
        private WindowBindingObjectServiceServer? WindowBindingObjectServer { get; set; }

        /// <summary>
        /// 初始化 <see cref="BrowserProcessHandler"/> 实例。
        /// </summary>
        /// <param name="browserApp">浏览器应用实例。</param>
        public BrowserProcessHandler(WebViewApp browserApp)
        {
            app = browserApp;
        }

        /// <summary>
        /// 注册自定义偏好设置。
        /// </summary>
        /// <param name="type">偏好设置类型。</param>
        /// <param name="registrar">偏好设置注册器。</param>
        protected override void OnRegisterCustomPreferences(CefPreferencesType type, CefPreferenceRegistrar registrar)
        {
            base.OnRegisterCustomPreferences(type, registrar);
        }

        /// <summary>
        /// 子进程启动前回调:附加宿主进程 ID 参数。
        /// </summary>
        /// <param name="commandLine">命令行。</param>
        protected override void OnBeforeChildProcessLaunch(CefCommandLine commandLine)
        {
            commandLine.AppendSwitch("host-process-id", System.Diagnostics.Process.GetCurrentProcess().Id.ToString());

            System.Diagnostics.Debug.WriteLine("[SUBPROCESS] commandline arguments:");
            System.Diagnostics.Debug.WriteLine(commandLine.ToString());
        }

        /// <summary>
        /// 上下文初始化后回调:创建窗口绑定对象服务端。
        /// </summary>
        protected override void OnContextInitialized()
        {
            WindowBindingObjectServer = new WindowBindingObjectServiceServer(app.GetExtensionPipeName());
        }

        /// <summary>
        /// 释放资源。
        /// </summary>
        /// <param name="disposing">是否释放托管资源。</param>
        protected override void Dispose(bool disposing)
        {
            WindowBindingObjectServer?.Dispose();
            base.Dispose(disposing);
        }
    }
}
