// Robot 桌面软件 — 主入口
// 直接启动: 直接初始化 CEF + 创建单个无边框窗口, 无 builder/无 AppStartup
// CEF 全局引导(原 BrowserHost + ChromiumEnvironment)已内联为本类静态成员

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Robot.App.Forms;
using Robot.Browser;
using Robot.JavaScript;
using Robot.WebResource;
using Xilium.CefGlue;

namespace Robot.App
{

    internal static class Program
    {
        #region CEF 全局状态(原 BrowserHost + ChromiumEnvironment 内联)

        /// <summary>
        /// 公共数据目录名。
        /// </summary>
        private const string COMMON_DATA_DIR_NAME = @"Robot";

        /// <summary>
        /// 是否渲染进程。
        /// </summary>
        internal static bool IsRenderer { get; private set; }

        /// <summary>
        /// 浏览器进程 ID。
        /// </summary>
        internal static int BrowserProcessId { get; private set; }

        /// <summary>
        /// 应用名称。
        /// </summary>
        internal static string AppName { get; private set; } = string.Empty;

        /// <summary>
        /// 当前区域文化。
        /// </summary>
        internal static CultureInfo CurrentCulture { get; set; } = Application.CurrentCulture;

        /// <summary>
        /// 是否启用 DevTools。
        /// </summary>
        internal static bool EnableDevTools { get; set; } = true;

        /// <summary>
        /// 是否使用内嵌浏览器。
        /// </summary>
        internal static bool UseEmbeddedBrowser { get; set; } = true;

        /// <summary>
        /// libcef.dll 所在目录。
        /// </summary>
        internal static string LibCefPath { get; private set; } = string.Empty;

        /// <summary>
        /// CEF 资源文件目录。
        /// </summary>
        internal static string ResourceFilePath { get; private set; } = string.Empty;

        /// <summary>
        /// 语言包(locales)目录。
        /// </summary>
        internal static string LocaleFilePath { get; private set; } = string.Empty;

        /// <summary>
        /// 用户数据目录(可空)。
        /// </summary>
        internal static string? UserDataPath { get; set; } = null;

        /// <summary>
        /// 是否使用内存用户数据。
        /// </summary>
        internal static bool UseInMemoryUserData { get; set; } = false;

        /// <summary>
        /// 命令行配置委托(可空)。
        /// </summary>
        internal static Action<CefCommandLine>? ConfigureCommandLine { get; set; }

        /// <summary>
        /// CEF 设置配置委托(可空)。
        /// </summary>
        internal static Action<CefSettings>? ConfigureSettings { get; set; }

        /// <summary>
        /// 自定义 scheme 配置委托(可空)。
        /// </summary>
        internal static Action<CefSchemeRegistrar>? ConfigureCustomSchemes { get; set; }

        /// <summary>
        /// 浏览器设置配置委托(可空)。
        /// </summary>
        internal static Action<CefBrowserSettings>? ConfigureBrowserSettings { get; set; }

        /// <summary>
        /// 公共数据目录路径。
        /// </summary>
        internal static string CommonDataDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), COMMON_DATA_DIR_NAME);

        /// <summary>
        /// 默认应用数据目录路径。
        /// </summary>
        internal static string DefaultAppDataDirectory => Path.Combine(CommonDataDirectory, AppName);

        /// <summary>
        /// 应用数据目录路径。
        /// </summary>
        internal static string AppDataDirectory { get; private set; } = string.Empty;

        #endregion

        #region CEF 引导(原 BrowserHost ctor + Run + Load 内联)

        /// <summary>
        /// 初始化 CEF 全局上下文:确定应用名称、数据目录、libcef 路径与浏览器进程 ID。
        /// </summary>
        /// <param name="isRenderer">是否渲染进程。</param>
        internal static void InitializeCefContext(bool isRenderer)
        {
            IsRenderer = isRenderer;
            AppName = Application.ProductName ?? "Robot App";

            var libCefDir = DetectLibCefFilesPath();
            var resourceDir = DetectLibCefResourceFilesPath(libCefDir);

            if (string.IsNullOrEmpty(libCefDir) || string.IsNullOrEmpty(resourceDir))
            {
                // 无法找到libcef.dll文件路径或cef运行时文件结构不正确。
                throw new FileNotFoundException("The libcef.dll file path could not be found or the cef runtime file structure is incorrect.");
            }

            LibCefPath = libCefDir;
            ResourceFilePath = resourceDir;
            LocaleFilePath = Path.Combine(resourceDir, LOCALES_DIR);

            AppDataDirectory = DefaultAppDataDirectory;

            if (isRenderer)
            {
                var args = Environment.GetCommandLineArgs();

                var processIdArg = args.FirstOrDefault(x => x.StartsWith("--host-process-id")) ?? string.Empty;

                if (!int.TryParse(Regex.Replace(processIdArg, "--host-process-id=", string.Empty), out var browserProcessId))
                    throw new ApplicationException("RobotApp 子进程缺少有效的 --host-process-id 参数。");

                BrowserProcessId = browserProcessId;

                if (BrowserProcessId == 0)
                    throw new ApplicationException("RobotApp 子进程的 --host-process-id 参数无效。");
            }
            else
            {
                BrowserProcessId = Process.GetCurrentProcess().Id;
            }
        }

        /// <summary>
        /// 获取默认 CEF 设置。
        /// </summary>
        /// <returns>CEF 设置。</returns>
        private static CefSettings GetDefaultCefSettings()
        {
            return new CefSettings
            {
                LogSeverity = CefLogSeverity.Error,
                ResourcesDirPath = ResourceFilePath,
                LocalesDirPath = LocaleFilePath,
                Locale = CurrentCulture.ToString(),
                JavaScriptFlags = "--expose-gc",
                RootCachePath = AppDataDirectory,
                CachePath = UseInMemoryUserData ? string.Empty : Path.Combine(AppDataDirectory, "Cache"),
                LogFile = Path.Combine(AppDataDirectory, $"{nameof(Robot).ToLower()}-cef.log"),
                MultiThreadedMessageLoop = true,
                ExternalMessagePump = false,
                NoSandbox = true,
                PersistSessionCookies = true,
                PersistUserPreferences = true,
            };
        }

        /// <summary>
        /// 获取默认浏览器设置(白色背景、60 帧无窗口刷新率、UTF-8 编码)。
        /// </summary>
        /// <returns>浏览器设置。</returns>
        internal static CefBrowserSettings GetDefaultBrowserSettings()
        {
            return new CefBrowserSettings
            {
                BackgroundColor = new CefColor(0xff, 0xff, 0xff, 0xff),
                WindowlessFrameRate = 60,
                DefaultEncoding = "UTF-8",
            };
        }

        /// <summary>
        /// 加载 CEF 运行时并执行进程:创建用户数据目录、加载 libcef、执行进程。
        /// </summary>
        /// <param name="args">CEF 主参数。</param>
        /// <param name="app">CEF 应用。</param>
        /// <param name="exitCode">输出的退出码。</param>
        /// <returns>是否成功。</returns>
        private static bool LoadCef(CefMainArgs args, CefApp app, out int exitCode)
        {
            if (UserDataPath != null && !Directory.Exists(UserDataPath))
            {
                var userDataDir = UserDataPath;

                try
                {
                    Directory.CreateDirectory(userDataDir);

                    AppDataDirectory = userDataDir;
                }
                catch
                {
                    AppDataDirectory = DefaultAppDataDirectory;
                }
            }

            try
            {
                Application.CurrentCulture = CurrentCulture;
                CultureInfo.DefaultThreadCurrentCulture = CurrentCulture;
                CultureInfo.DefaultThreadCurrentUICulture = CurrentCulture;

                CefRuntime.Load(LibCefPath);

                exitCode = CefRuntime.ExecuteProcess(args, app, IntPtr.Zero);


                if (exitCode != -1)
                {
                    Environment.Exit(exitCode);

                    Debug.WriteLine($"ExecuteProcess() expected to return -1 but returned {exitCode}");

                    return false;
                }

                foreach (var arg in Environment.GetCommandLineArgs())
                {
                    if (arg.StartsWith("--type="))
                    {

                        exitCode = -2;
                        Environment.Exit(exitCode);

                        Debug.WriteLine($"ExecuteProcess() expected to return -1 but returned {exitCode}");

                        return false;
                    }
                }


                Debug.WriteLine($"ExecuteProcess() returns {exitCode}");
            }
            catch (CefVersionMismatchException ex)
            {
                var title = "Failed to load";
                var msg = "libcef.dll was not found, or the architecture of libcef.dll is incorrect.";

                Debug.WriteLine(ex);

                MessageBox.Show($"{msg}", title, MessageBoxButtons.OK, MessageBoxIcon.Error);

                exitCode = -2;

                return false;

            }
            catch (DllNotFoundException ex)
            {
                var title = "Failed to load";
                var msg = "libcef.dll was not found, or the architecture of libcef.dll is incorrect.";

                Debug.WriteLine(ex);

                MessageBox.Show(msg, title, MessageBoxButtons.OK, MessageBoxIcon.Error);

                exitCode = -2;

                return false;

            }
            return true;
        }

        /// <summary>
        /// 运行浏览器应用:加载 CEF、初始化运行时、注册 scheme 处理器并运行主窗口。
        /// </summary>
        /// <param name="createMainWindow">创建主窗口的委托。</param>
        /// <param name="schemeHandlerFactories">scheme 处理器工厂集合。</param>
        internal static void RunCef(Func<Form> createMainWindow, IReadOnlyList<ResourceSchemeHandlerFactory> schemeHandlerFactories)
        {
            var args = new CefMainArgs(Environment.GetCommandLineArgs());

            var app = new WebViewApp();

            if (LoadCef(args, app, out _))
            {
                var settings = GetDefaultCefSettings();

                ConfigureSettings?.Invoke(settings);

                CefRuntime.Initialize(args, settings, app, IntPtr.Zero);

                foreach (var factory in schemeHandlerFactories)
                {
                    factory.ResourceSchemeHandlerRegister();

                    CefRuntime.RegisterSchemeHandlerFactory(factory.Scheme, factory.DomainName, factory);
                }

                try
                {
                    Application.Run(createMainWindow());
                }
                finally
                {
                    CefRuntime.Shutdown();
                }
            }
        }

        /// <summary>
        /// 关闭 CEF 运行时。
        /// </summary>
        internal static void Shutdown()
        {
            CefRuntime.Shutdown();
        }

        #endregion

        #region 定位 libcef.dll(原 ChromiumEnvironment 内联)

        private const string RESOURCE_DIR = "Resources";
        private const string LOCALES_DIR = "locales";
        private const string ANY_CPU_RUNTIME_DIR = "runtimes";
        private const string UP_LEVEL_DIR = "..";
        private const string DEFAULT_CEF_DIR = "fx";

        //TODO: 更新为最新版本
        private const string CHROMIUM_VERSION = "109.0.5414";

        /// <summary>
        /// 平台架构标识(x64/x86),用于定位 libcef.dll。
        /// </summary>
        private static string Architecture => IntPtr.Size == 8 ? "x64" : "x86";

        /// <summary>
        /// 公共 CEF 运行时目录(公共应用数据目录下的 Robot 版本目录)。
        /// </summary>
        private static string CommonCefRuntimeDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), @"Robot\", CHROMIUM_VERSION);

        /// <summary>
        /// 应用运行目录(当前程序集所在目录)。
        /// </summary>
        private static string ApplicationRunningDirectory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

        /// <summary>
        /// 检查指定目录下是否存在 libcef.dll。
        /// </summary>
        private static bool EnsureLibCefExists(string? path) => path != null && File.Exists(Path.Combine(path, "libcef.dll"));

        /// <summary>
        /// 检查指定目录是否为有效的 CEF 资源目录(含 .pak 文件与 locales 子目录)。
        /// </summary>
        private static bool EnsureLibCefResourceDirExists(string? path) => path != null && Directory.Exists(path) && Directory.GetFiles(path, "*.pak", SearchOption.TopDirectoryOnly).Length > 0 && Directory.Exists(Path.Combine(path, "locales")) && Directory.GetFiles(Path.Combine(path, "locales"), "*.pak", SearchOption.TopDirectoryOnly).Length > 0;

        /// <summary>
        /// 检测 libcef.dll 所在目录:优先取命令行 --libcef-dir-path 参数,否则按候选路径依次探测。
        /// 未找到时返回空字符串。
        /// </summary>
        private static string DetectLibCefFilesPath()
        {
            var arch = Architecture;

            var args = Environment.GetCommandLineArgs();

            var libCefPathArg = args?.FirstOrDefault(x => x.StartsWith("--libcef-dir-path"))?.Split('=');

            if (libCefPathArg != null && libCefPathArg.Length == 2 && EnsureLibCefExists(libCefPathArg[1]))
            {
                return libCefPathArg[1];
            }

            var searchPaths = new string[]
            {
                    ApplicationRunningDirectory,
                    Path.Combine(ApplicationRunningDirectory, arch),
                    Path.Combine(ApplicationRunningDirectory, DEFAULT_CEF_DIR, arch),

                    Path.Combine(ApplicationRunningDirectory, ANY_CPU_RUNTIME_DIR, $"win-{arch}", "native"),
                    Path.Combine(CommonCefRuntimeDirectory, arch),

            };

            foreach (var path in searchPaths)
            {
                if (EnsureLibCefExists(path))
                {
                    return path;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 检测 CEF 资源文件目录:在 libcef.dll 目录及其上级/资源子目录中依次探测。
        /// 未找到时返回空字符串。
        /// </summary>
        private static string DetectLibCefResourceFilesPath(string? libCefDir)
        {
            if (libCefDir == null || string.IsNullOrEmpty(libCefDir))
                return string.Empty;

            var searchPaths = new string[]
            {
                    libCefDir,
                    Path.GetFullPath(Path.Combine(libCefDir, UP_LEVEL_DIR)),
                    Path.GetFullPath(Path.Combine(libCefDir, UP_LEVEL_DIR, RESOURCE_DIR)),
                    Path.Combine(libCefDir, RESOURCE_DIR)
            };

            foreach (var path in searchPaths)
            {
                if (EnsureLibCefResourceDirExists(path))
                {
                    return path;
                }
            }

            return string.Empty;
        }

        #endregion

        #region 主入口

        /// <summary>
        /// 应用主入口。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
#if NETCOREAPP3_1_OR_GREATER
            ApplicationConfiguration.Initialize();
#else
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
#endif
            // 主进程与 CEF 子进程(renderer/gpu 等)共用本入口;子进程命令行带 --type=
            // 硬编码为主进程会让 renderer 用自身 PID 构造扩展管道名(BrowserApp.GetExtensionPipeName),
            // 连不存在的管道 → Connect 无限等待 → renderer 主线程冻结(导航卡死)
            var isRenderer = args.Any(x => x.StartsWith("--type=", StringComparison.OrdinalIgnoreCase));

            InitializeCefContext(isRenderer);

            // 窗口绑定对象注册:browser 侧桥接实例化 + renderer 侧经 describers 自动注册 JS 扩展
            JavaScriptWindowBindingObjectBridge.WindowBindingObjectTypes.Add(typeof(RobotFormWindowBindingObject));

            // CEF/Chromium 配置
            // disable-gpu: 企业远程桌面环境无 GPU,不禁用会 FATAL 崩溃
            ConfigureCommandLine = cl =>
            {
                cl.AppendSwitch("disable-gpu");
                cl.AppendSwitch("remote-debugging-port", "9222");
            };

            // wwwroot 嵌入资源 → http://embedded.app.local
            var wwwroot = new EmbeddedFileResourceSchemeHandlerFactory(new EmbeddedFileResourceOptions
            {
                Scheme = "http",
                DomainName = "embedded.app.local",
                ResourceAssembly = typeof(Program).Assembly,
                EmbeddedResourceDirectoryName = "wwwroot",
            });

            // 创建单个无边框窗口
            RunCef(() => new RobotWindow().GetHostWindow(), new[] { wwwroot });
        }

        #endregion
    }
}
