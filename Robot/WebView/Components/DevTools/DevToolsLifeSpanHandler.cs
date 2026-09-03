// Robot 桌面软件 — 开发者工具生命周期处理器
// 处理开发者工具浏览器创建后的窗口句柄绑定与尺寸变化通知

using System;
using Xilium.CefGlue;

namespace Robot.Browser.DevTools
{

    /// <summary>
    /// 开发者工具生命周期处理器:处理开发者工具浏览器创建后的窗口句柄绑定与尺寸变化通知。
    /// </summary>
    internal class DevToolsLifeSpanHandler : CefLifeSpanHandler
    {
        /// <summary>
        /// 宿主窗口。
        /// </summary>
        private DevToolsWindow _hostWindow;

        /// <summary>
        /// 初始化 <see cref="DevToolsLifeSpanHandler"/> 实例。
        /// </summary>
        /// <param name="hostWindow">宿主窗口。</param>
        public DevToolsLifeSpanHandler(DevToolsWindow hostWindow)
        {
            _hostWindow = hostWindow;
        }

        /// <summary>
        /// 浏览器创建后回调:绑定窗口句柄并订阅尺寸变化事件。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        protected override void OnAfterCreated(CefBrowser browser)
        {
            base.OnAfterCreated(browser);

            var window = _hostWindow;

            var initAction = new Action(() =>
            {
                window.BrowserWindowHandle = browser.GetHost().GetWindowHandle();
                window.SizeChanged += (_, _) => browser?.GetHost()?.NotifyMoveOrResizeStarted();
                window.ResizeBegin += (_, _) => browser?.GetHost()?.NotifyMoveOrResizeStarted();
                window.ResizeEnd += (_, _) => browser?.GetHost()?.WasResized();
                window.Move += (_, _) => browser?.GetHost()?.NotifyMoveOrResizeStarted();
            });

            if (window.InvokeRequired)
            {
                window.Invoke(initAction);
            }
            else
            {
                initAction.Invoke();
            }
        }
    }
}
