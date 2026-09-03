// Robot 桌面软件 — 嵌入式浏览器窗口
// 承载嵌入式浏览器的独立窗口,尺寸与焦点随浏览器联动

using System.ComponentModel;
using System.Windows.Forms;
using Vanara.PInvoke;
using Xilium.CefGlue;

namespace Robot.Browser.EmbeddedBrowser
{

    /// <summary>
    /// 嵌入式浏览器窗口:承载嵌入式浏览器的独立窗口,尺寸与焦点随浏览器联动。
    /// </summary>
    internal class EmbeddedBrowserWindow : Form
    {
        /// <summary>
        /// 浏览器窗口句柄。
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public nint? BrowserWindowHandle { get; internal set; }

        /// <summary>
        /// 浏览器实例。
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CefBrowser? Browser { get; internal set; }

        /// <summary>
        /// 初始化 <see cref="EmbeddedBrowserWindow"/> 实例:订阅尺寸变化与焦点事件。
        /// </summary>
        public EmbeddedBrowserWindow()
        {
            AutoScaleMode = AutoScaleMode.Dpi;

            Text = $"Loading... - Robot Browser";

            SizeChanged += (_, _) =>
            {
                if (BrowserWindowHandle != null)
                {
                    User32.SetWindowPos(BrowserWindowHandle.Value, HWND.NULL, 0, 0, ClientSize.Width, ClientSize.Height, User32.SetWindowPosFlags.SWP_NOMOVE | User32.SetWindowPosFlags.SWP_NOZORDER | User32.SetWindowPosFlags.SWP_NOACTIVATE);
                }
            };

            Activated += (_, _) =>
            {
                Browser?.GetHost()?.SetFocus(true);
            };

            Deactivate += (_, _) =>
            {
                Browser?.GetHost()?.SetFocus(false);
            };
        }
    }
}
