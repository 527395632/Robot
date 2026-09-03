// Robot 桌面软件 — Win32 窗口句柄包装
// 将原生窗口句柄包装为 IWin32Window 实现

using System;
using System.Windows.Forms;

namespace Robot
{

    /// <summary>
    /// Win32 窗口句柄包装:将原生窗口句柄包装为 <see cref="IWin32Window"/> 实现。
    /// </summary>
    internal class Win32WindowWrap : IWin32Window
    {
        /// <summary>
        /// 原生窗口句柄。
        /// </summary>
        public IntPtr Handle { get; }

        /// <summary>
        /// 初始化 <see cref="Win32WindowWrap"/> 实例。
        /// </summary>
        /// <param name="handle">原生窗口句柄。</param>
        public Win32WindowWrap(IntPtr handle)
        {
            Handle = handle;
        }
    }
}
