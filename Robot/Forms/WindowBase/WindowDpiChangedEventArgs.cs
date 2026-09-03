using System;

namespace Robot.Forms
{

    /// <summary>
    /// 窗口 DPI 变化事件参数: 携带变化前后的设备 DPI。
    /// </summary>
    public class WindowDpiChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 变化前的设备 DPI。
        /// </summary>
        public int OldDPI { get; }

        /// <summary>
        /// 变化后的设备 DPI。
        /// </summary>
        public int NewDPI { get; }

        /// <summary>
        /// 初始化窗口 DPI 变化事件参数。
        /// </summary>
        /// <param name="oldDpi">变化前的设备 DPI。</param>
        /// <param name="newDpi">变化后的设备 DPI。</param>
        internal WindowDpiChangedEventArgs(int oldDpi, int newDpi)
        {
            OldDPI = oldDpi;
            NewDPI = newDpi;
        }
    }
}
