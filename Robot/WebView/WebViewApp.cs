// Robot 桌面软件 — CEF 应用入口 (CefApp 子类)
// 主进程与渲染子进程共用本类: 配置命令行参数、注册自定义 scheme、装配浏览器/渲染进程处理器

using System.Reflection;
using Xilium.CefGlue;

namespace Robot.Browser
{

    /// <summary>
    /// CEF 应用入口(<see cref="CefApp"/> 子类)。
    /// 主进程与渲染子进程共用本类,负责配置命令行参数、注册自定义 scheme,并装配浏览器/渲染进程处理器。
    /// </summary>
    internal class WebViewApp : CefApp
    {
        /// <summary>
        /// 渲染进程处理器。
        /// </summary>
        private readonly CefRenderProcessHandler _renderProcessHandler;

        /// <summary>
        /// 浏览器进程处理器。
        /// </summary>
        private readonly CefBrowserProcessHandler _browserProcessHandler;

        /// <summary>
        /// 获取扩展代理管道名(基于进程 ID 构造,供渲染进程连接浏览器进程)。
        /// </summary>
        public string GetExtensionPipeName()
        {
            int processId;

            if (!Robot.App.Program.IsRenderer)
            {
                processId = System.Diagnostics.Process.GetCurrentProcess().Id;
            }
            else
            {
                processId = Robot.App.Program.BrowserProcessId;
            }

            return $"Robot-ExtensionProxy-{processId}";
        }

        /// <summary>
        /// 初始化 <see cref="WebViewApp"/> 实例,装配浏览器/渲染进程处理器。
        /// </summary>
        public WebViewApp()
        {
            _browserProcessHandler = new BrowserProcessHandler(this);
            _renderProcessHandler = new RenderProcessHandler(this);
        }

        /// <summary>
        /// 处理命令行参数:追加默认开关(媒体流、自动播放、User-Agent),并调用宿主自定义配置。
        /// </summary>
        protected override void OnBeforeCommandLineProcessing(string processType, CefCommandLine commandLine)
        {
            //TODO:决定好把哪些参数设置为默认参数

            commandLine.AppendSwitch("enable-media-stream");
            commandLine.AppendSwitch("autoplay-policy", "no-user-gesture-required");
            //commandLine.AppendSwitch("renderer-process-limit", "1");
            //commandLine.AppendSwitch("in-process-gpu");
            commandLine.AppendSwitch("user-agent-product", $"Chromium/{CefRuntime.ChromeVersion} Robot/{Assembly.GetExecutingAssembly().GetName().Version}");


            Robot.App.Program.ConfigureCommandLine?.Invoke(commandLine);
        }

        /// <summary>
        /// 获取浏览器进程处理器。
        /// </summary>
        protected override CefBrowserProcessHandler GetBrowserProcessHandler()
        {
            return _browserProcessHandler;
        }

        /// <summary>
        /// 获取渲染进程处理器。
        /// </summary>
        protected override CefRenderProcessHandler GetRenderProcessHandler()
        {
            return _renderProcessHandler;
        }

        /// <summary>
        /// 注册自定义 scheme:调用宿主自定义配置,并注册 host scheme(安全 + 标准)。
        /// </summary>
        protected override void OnRegisterCustomSchemes(CefSchemeRegistrar registrar)
        {
            Robot.App.Program.ConfigureCustomSchemes?.Invoke(registrar);

            registrar.AddCustomScheme("host", CefSchemeOptions.Secure | CefSchemeOptions.Standard);
        }
    }

    /// <summary>
    /// 窗口绑定对象描述器(文件路径 + 类型名)。
    /// </summary>
    internal record JavaScriptWindowBindingObjectDescriper(string FilePath, string TypeName);
}
