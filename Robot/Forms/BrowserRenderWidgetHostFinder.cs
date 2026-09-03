// Robot 桌面软件 — 浏览器渲染控件宿主查找器
// 在浏览器句柄的子窗口中查找 Chromium 渲染控件宿主窗口

using System;
using System.Runtime.InteropServices;
using System.Text;
using Vanara.PInvoke;
using static Vanara.PInvoke.User32;

namespace Robot.Forms
{

    /// <summary>
    /// 浏览器渲染控件宿主查找器:在浏览器句柄的子窗口中查找 Chromium 渲染控件宿主窗口。
    /// </summary>
    internal static class BrowserRenderWidgetHostFinder
    {
        /// <summary>
        /// 枚举窗口时收集到的类信息。
        /// </summary>
        private class ClassDetails
        {
            /// <summary>
            /// 找到的宿主窗口句柄。
            /// </summary>
            public HWND DescendantFound { get; set; }
        }

        /// <summary>
        /// 枚举子窗口回调:匹配 Chromium 渲染控件宿主类名。
        /// </summary>
        /// <param name="hWnd">当前枚举的窗口句柄。</param>
        /// <param name="lParam">参数(指向 ClassDetails 的 GCHandle)。</param>
        /// <returns>是否继续枚举。</returns>
        private static bool EnumWindow(HWND hWnd, IntPtr lParam)
        {
            const string CHROMIUM_WIDGET_HOST_CLASS_NAME = "Chrome_RenderWidgetHostHWND";

            var buffer = new StringBuilder(256);

            GetClassName(hWnd, buffer, buffer.Capacity);

            if (buffer.ToString() == CHROMIUM_WIDGET_HOST_CLASS_NAME)
            {
                var gcHandle = GCHandle.FromIntPtr(lParam);

                var classDetails = (ClassDetails)gcHandle.Target!;

                classDetails.DescendantFound = hWnd;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 尝试查找 Chromium 渲染控件宿主窗口句柄。
        /// </summary>
        /// <param name="browserHandle">浏览器句柄。</param>
        /// <param name="chromeWidgetHostHandle">输出的宿主窗口句柄。</param>
        /// <returns>是否找到。</returns>
        internal static bool TryFindHandle(IntPtr browserHandle, out HWND chromeWidgetHostHandle)
        {
            var classDetails = new ClassDetails();
            var gcHandle = GCHandle.Alloc(classDetails);

            var childProc = new EnumWindowsProc(EnumWindow);

            EnumChildWindows(browserHandle, childProc, GCHandle.ToIntPtr(gcHandle));

            chromeWidgetHostHandle = classDetails.DescendantFound;

            gcHandle.Free();

            return classDetails.DescendantFound != HWND.NULL;
        }
    }
}
