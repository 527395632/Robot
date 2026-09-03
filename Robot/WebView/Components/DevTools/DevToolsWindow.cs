// Robot 桌面软件 — 开发者工具窗口
// 承载开发者工具的独立窗口,尺寸随浏览器窗口联动

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Vanara.PInvoke;

namespace Robot.Browser.DevTools
{

    /// <summary>
    /// 开发者工具窗口:承载开发者工具的独立窗口,尺寸随浏览器窗口联动。
    /// </summary>
    internal class DevToolsWindow : Form
    {
        /// <summary>
        /// 关联的 WebView。
        /// </summary>
        public WebViewLifeSpan WebView { get; }

        /// <summary>
        /// 浏览器窗口句柄。
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IntPtr? BrowserWindowHandle { get; internal set; }

        /// <summary>
        /// 初始化 <see cref="DevToolsWindow"/> 实例:按屏幕工作区计算初始尺寸并订阅尺寸变化。
        /// </summary>
        /// <param name="webview">关联的 WebView。</param>
        public DevToolsWindow(WebViewLifeSpan webview)
        {
            AutoScaleMode = AutoScaleMode.Dpi;

            WebView = webview;

            User32.GetWindowRect(webview.WindowHandle, out var rect);

            var screen = Screen.FromRectangle(rect);

            var width = 1440;
            var height = 900;

            if (width > screen.WorkingArea.Width) width = screen.WorkingArea.Width - 20;

            if (height > screen.WorkingArea.Height) height = screen.WorkingArea.Height - 20;

            Size = new Size(width, height);

            Text = "DevTools - Robot Developer's Toolbox";

            SizeChanged += (_, _) =>
            {
                if (BrowserWindowHandle != null)
                {
                    User32.SetWindowPos(BrowserWindowHandle.Value, HWND.NULL, 0, 0, ClientSize.Width, ClientSize.Height, User32.SetWindowPosFlags.SWP_NOMOVE | User32.SetWindowPosFlags.SWP_NOZORDER | User32.SetWindowPosFlags.SWP_NOACTIVATE);
                }
            };
        }

        /// <summary>
        /// 窗口显示后回调。
        /// </summary>
        /// <param name="e">事件参数。</param>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            //System.Diagnostics.Debug.WriteLine($"{nameof(OnShown)} -> Thread {Thread.CurrentThread.ManagedThreadId}");
        }
    }
}
