using System;

namespace Robot.Forms
{

    /// <summary>
    /// 窗口状态变化事件参数: 携带变化后的窗口状态。
    /// </summary>
    public class WindowStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 初始化窗口状态变化事件参数。
        /// </summary>
        /// <param name="state">变化后的窗口状态。</param>
        internal WindowStateChangedEventArgs(WindowChangeState state)
        {
            State = state;
        }

        /// <summary>
        /// 变化后的窗口状态。
        /// </summary>
        public WindowChangeState State { get; }
    }
}
