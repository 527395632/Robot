using System;

namespace Robot.Forms
{

    /// <summary>
    /// 窗口激活状态变化事件参数。
    /// </summary>
    public class WindowActivatedEventArgs : EventArgs
    {
        /// <summary>
        /// 初始化窗口激活状态变化事件参数。
        /// </summary>
        /// <param name="state">窗口是否处于激活状态。</param>
        internal WindowActivatedEventArgs(bool state)
        {
            IsActivated = state;
        }

        /// <summary>
        /// 窗口是否处于激活状态。
        /// </summary>
        public bool IsActivated { get; }
    }
}
