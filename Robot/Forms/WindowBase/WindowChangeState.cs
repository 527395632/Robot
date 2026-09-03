namespace Robot.Forms
{

    /// <summary>
    /// 窗口变化状态: 描述窗口尺寸/形态变化后的目标状态。
    /// </summary>
    public enum WindowChangeState
    {
        /// <summary>
        /// 还原状态: 窗口从最大化/最小化恢复到普通大小。
        /// </summary>
        Restore,

        /// <summary>
        /// 最大化状态: 窗口被放大至充满工作区。
        /// </summary>
        Maximize,

        /// <summary>
        /// 最小化状态: 窗口被缩小到任务栏。
        /// </summary>
        Minimize
    }
}
