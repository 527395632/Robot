// Robot 桌面软件 — 浏览器渲染控件宿主消息拦截器
// 接管 Chromium 渲染控件宿主窗口的消息循环,支持转发与重试挂载

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vanara.PInvoke;

namespace Robot.Forms
{

    /// <summary>
    /// 窗口消息处理委托:接收并修改消息,返回是否已处理。
    /// </summary>
    /// <param name="message">待处理的消息。</param>
    /// <returns>是否已处理该消息。</returns>
    public delegate bool WindowMessageDelegate(ref Message message);

    /// <summary>
    /// 浏览器渲染控件宿主消息拦截器:接管 Chromium 渲染控件宿主窗口的消息循环,支持消息转发与重试挂载。
    /// </summary>
    internal class BrowserRenderWidgetHostMessageInterceptor : NativeWindow
    {
        /// <summary>
        /// 宿主窗口。
        /// </summary>
        private readonly Form _hostWindow;

        /// <summary>
        /// 消息转发委托。
        /// </summary>
        private WindowMessageDelegate? _forwardAction;

        /// <summary>
        /// 浏览器窗口句柄。
        /// </summary>
        public HWND BrowserWindowHandle { get; }

        /// <summary>
        /// 初始化 <see cref="BrowserRenderWidgetHostMessageInterceptor"/> 实例并接管指定宿主窗口句柄。
        /// </summary>
        /// <param name="hostWindow">宿主窗口。</param>
        /// <param name="browserWindowHandle">浏览器窗口句柄。</param>
        /// <param name="forwardAction">消息转发委托。</param>
        internal BrowserRenderWidgetHostMessageInterceptor(Form hostWindow, HWND browserWindowHandle, WindowMessageDelegate forwardAction)
        {
            BrowserWindowHandle = browserWindowHandle;
            AssignHandle(browserWindowHandle.DangerousGetHandle());
            _hostWindow = hostWindow;
            _forwardAction = forwardAction;

            hostWindow.HandleDestroyed += HostWindowHandleDestroyed;
        }

        /// <summary>
        /// 宿主窗口句柄销毁回调:释放浏览器句柄并解绑事件。
        /// </summary>
        /// <param name="sender">事件发送者。</param>
        /// <param name="e">事件参数。</param>
        private void HostWindowHandleDestroyed(object? sender, EventArgs e)
        {
            ReleaseBrowserHandle();

            _hostWindow.HandleDestroyed -= HostWindowHandleDestroyed;
            _forwardAction = null;
        }

        /// <summary>
        /// 释放浏览器句柄。
        /// </summary>
        internal void ReleaseBrowserHandle()
        {
            if (!BrowserWindowHandle.IsNull)
            {
                ReleaseHandle();
            }
        }

        /// <summary>
        /// 窗口过程:先尝试转发消息,未处理时交由基类处理。
        /// </summary>
        /// <param name="m">待处理的消息。</param>
        protected override void WndProc(ref Message m)
        {
            var result = _forwardAction?.Invoke(ref m) ?? false;

            if (!result)
            {
                base.WndProc(ref m);
            }
        }

        /// <summary>
        /// 异步创建并挂载消息拦截器:重试查找 Chromium 渲染控件宿主窗口。
        /// </summary>
        /// <param name="interceptor">已存在的拦截器(句柄变化时重建)。</param>
        /// <param name="host">表单宿主。</param>
        /// <param name="forwardAction">消息转发委托。</param>
        /// <returns>挂载后的拦截器,失败时返回 null。</returns>
        internal static Task<BrowserRenderWidgetHostMessageInterceptor?> Setup(BrowserRenderWidgetHostMessageInterceptor? interceptor, RobotWindow host, WindowMessageDelegate forwardAction)
        {
            return Task.Run(() =>
            {
                try
                {
                    var retryCount = 10;

                    var handle = host.BrowserHandle;

                    if (handle == IntPtr.Zero)
                    {
                        handle = host.WindowHandle;
                    }

                    while (true)
                    {
                        if (BrowserRenderWidgetHostFinder.TryFindHandle(handle, out var chromeWidgetHostHandle))
                        {
                            if (interceptor == null || (interceptor != null && interceptor.BrowserWindowHandle != chromeWidgetHostHandle))
                            {
                                interceptor = new BrowserRenderWidgetHostMessageInterceptor(host.HostWindow!, chromeWidgetHostHandle, forwardAction);

                                System.Diagnostics.Debug.WriteLine("The browser message listener has been attached successfully.");

                                return interceptor;
                            }

                            return null;
                        }
                        else
                        {
                            Thread.Sleep(200);

                            retryCount--;

                            if (retryCount <= 0)
                            {
                                throw new Exception("Browser message listener attach failed.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);

                    return null;
                }
            });
        }
    }
}
