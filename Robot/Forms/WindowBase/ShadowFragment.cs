namespace Robot.Forms
{

    /// <summary>
    /// 阴影元素状态: 记录阴影元素窗口的尺寸与激活状态。
    /// </summary>
    internal record ShadowElementState
    {
        /// <summary>
        /// 阴影元素宽度。
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// 阴影元素高度。
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// 是否处于激活状态。
        /// </summary>
        public bool IsActive { get; set; }
    }
}