// Robot 桌面软件 — 关闭事件参数
// 可设置 Cancel 取消关闭

using System;

namespace Robot
{

    /// <summary>
    /// 关闭事件参数,可设置 <see cref="Cancel"/> 取消关闭。
    /// </summary>
    public class ClosingEventArgs : EventArgs
    {
        /// <summary>
        /// 是否取消关闭。
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// 初始化 <see cref="ClosingEventArgs"/> 实例。
        /// </summary>
        public ClosingEventArgs()
        {
        }
    }
}
