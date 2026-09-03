// Robot 桌面软件 — 渲染进程处理器
// 处理渲染进程内浏览器创建、V8 上下文生命周期与窗口绑定对象注册

using Microsoft.Extensions.Logging;
using Robot.JavaScript;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using Xilium.CefGlue;

namespace Robot.Browser
{

    /// <summary>
    /// 渲染进程处理器:处理渲染进程内浏览器创建、V8 上下文生命周期与窗口绑定对象注册。
    /// </summary>
    internal class RenderProcessHandler : CefRenderProcessHandler
    {
        /// <summary>
        /// 浏览器应用。
        /// </summary>
        private WebViewApp _browserApp;

        /// <summary>
        /// 窗口绑定对象服务客户端。
        /// </summary>
        private WindowBindingObjectServiceClient WindowBindingObjectServiceClient { get; }

        /// <summary>
        /// 进程消息分发器。
        /// </summary>
        public ProcessMessageDispatcher MessageDispatcher { get; } = new ProcessMessageDispatcher();

        /// <summary>
        /// 消息桥。
        /// </summary>
        public MessageBridge? MessageBridge { get; private set; }

        /// <summary>
        /// 初始化 <see cref="RenderProcessHandler"/> 实例。
        /// </summary>
        /// <param name="browserApp">浏览器应用。</param>
        public RenderProcessHandler(WebViewApp browserApp)
        {
            _browserApp = browserApp;
            WindowBindingObjectServiceClient = new WindowBindingObjectServiceClient(_browserApp.GetExtensionPipeName());
        }

        /// <summary>
        /// 浏览器创建后回调:创建消息桥并注册各消息桥处理器。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="extraInfo">附加信息。</param>
        protected override void OnBrowserCreated(CefBrowser browser, CefDictionaryValue? extraInfo)
        {
            MessageBridge = new MessageBridge(browser, true, MessageDispatcher);

            MessageBridge.RegisterMessageBridgeHandler(new JavaScriptEngineBridge(MessageBridge));
            MessageBridge.RegisterMessageBridgeHandler(new JavaScriptObjectMappingBridge(MessageBridge));
            MessageBridge.RegisterMessageBridgeHandler(new JavaScriptWindowBindingObjectBridge(MessageBridge, null));
        }

        /// <summary>
        /// 焦点节点变化回调。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="node">节点。</param>
        protected override void OnFocusedNodeChanged(CefBrowser browser, CefFrame frame, CefDomNode node)
        {
            base.OnFocusedNodeChanged(browser, frame, node);
        }

        /// <summary>
        /// 未捕获异常回调。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="context">V8 上下文。</param>
        /// <param name="exception">异常。</param>
        /// <param name="stackTrace">堆栈跟踪。</param>
        protected override void OnUncaughtException(CefBrowser browser, CefFrame frame, CefV8Context context, CefV8Exception exception, CefV8StackTrace stackTrace)
        {
            base.OnUncaughtException(browser, frame, context, exception, stackTrace);
        }

        /// <summary>
        /// 浏览器销毁后回调。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        protected override void OnBrowserDestroyed(CefBrowser browser)
        {
            base.OnBrowserDestroyed(browser);
        }

        /// <summary>
        /// V8 上下文创建后回调:通知浏览器进程、更新上下文状态并标记就绪。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="context">V8 上下文。</param>
        protected override void OnContextCreated(CefBrowser browser, CefFrame frame, CefV8Context context)
        {
            var message = CefProcessMessage.Create("Robot.OnContextCreated");

            frame.SendProcessMessage(CefProcessId.Browser, message);

            MessageBridge?.OnContextCreated(browser, frame, context);

            frame.ExecuteJavaScript("window.host && host?.hostWindow.internal?.setContextReadyState()", string.Empty, 0);

            context.Dispose();
        }

        /// <summary>
        /// V8 上下文释放后回调:通知浏览器进程并释放上下文。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="context">V8 上下文。</param>
        protected override void OnContextReleased(CefBrowser browser, CefFrame frame, CefV8Context context)
        {
            var message = CefProcessMessage.Create("Robot.OnContextReleased");

            frame.SendProcessMessage(CefProcessId.Browser, message);

            MessageBridge?.OnContextReleased(browser, frame, context);

            context.Dispose();
        }

        //List<JavaScriptWindowBindingObject> WindowBindingObjectInstances { get; } = new();

        /// <summary>
        /// WebKit 初始化后回调:注册窗口绑定对象。
        /// </summary>
        protected override void OnWebKitInitialized()
        {
            RegisterWindowBindingObjects();
        }

        /// <summary>
        /// 注册窗口绑定对象:从服务获取描述器,加载程序集并注册扩展。
        /// </summary>
        private void RegisterWindowBindingObjects()
        {
            var response = WindowBindingObjectServiceClient.Request("GetWindowBindingObjects");

            if (string.IsNullOrEmpty(response)) return;

            try
            {
                var describers = JsonSerializer.Deserialize<List<JavaScriptWindowBindingObjectDescriper>>(response!) ?? new List<JavaScriptWindowBindingObjectDescriper>();

                var assemblies = new Dictionary<string, Assembly>();

                foreach (var describer in describers)
                {
                    var path = describer.FilePath.ToLower();
                    var typeName = describer.TypeName;

                    Assembly assembly;

                    if (assemblies.ContainsKey(path))
                    {
                        assembly = assemblies[path];
                    }
                    else
                    {
                        assembly = Assembly.LoadFrom(path);
                        assemblies.Add(path, assembly);
                    }

                    var type = assembly.GetType(typeName);

                    if (type == null || !type.IsSubclassOf(typeof(JavaScriptWindowBindingObject))) continue;

                    try
                    {
                        var instance = Activator.CreateInstance(type) as JavaScriptWindowBindingObject;

                        Debug.WriteLine($"Registering window binding object: {instance?.Name ?? "[FAILED]"}");

                        if (instance != null)
                        {
                            //WindowBindingObjectInstances.Add(instance);
                            CefRuntime.RegisterExtension(instance.Name, instance.JavaScriptWindowBindingCode, instance);
                        }
                    }
                    catch (Exception ex)
                    {
                       Debug.WriteLine(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// 收到进程消息回调:分发消息。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="sourceProcess">源进程。</param>
        /// <param name="message">消息。</param>
        /// <returns>是否已处理。</returns>
        protected override bool OnProcessMessageReceived(CefBrowser browser, CefFrame frame, CefProcessId sourceProcess, CefProcessMessage message)
        {
            MessageDispatcher.DispatchMessage(browser, frame, sourceProcess, message);

            return base.OnProcessMessageReceived(browser, frame, sourceProcess, message);
        }
    }
}
