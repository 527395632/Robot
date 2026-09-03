using Vanara.PInvoke;

namespace Robot.Forms
{

    /// <summary>
    /// 阴影位图状态: 缓存阴影位图对应的矩形区域及其脏标记。
    /// </summary>
    internal class ShadowBitmapState
    {
        /// <summary>
        /// 阴影位图对应的矩形区域。
        /// </summary>
        internal RECT Rectangle { get; private set; }

        /// <summary>
        /// 初始化阴影位图状态, 指定初始矩形区域。
        /// </summary>
        public ShadowBitmapState(RECT rectangle)
        {
            Rectangle = rectangle;
        }

        /// <summary>
        /// 阴影位图宽度。
        /// </summary>
        public int Width => Rectangle.Width;

        /// <summary>
        /// 阴影位图高度。
        /// </summary>
        public int Height => Rectangle.Height;

        /// <summary>
        /// 阴影位图 X 坐标。
        /// </summary>
        public int X => Rectangle.X;

        /// <summary>
        /// 阴影位图 Y 坐标。
        /// </summary>
        public int Y => Rectangle.Y;

        /// <summary>
        /// 更新矩形区域并清除脏标记。
        /// </summary>
        public void UpdateRectangle(RECT rectangle)
        {
            Rectangle = rectangle;
            IsDirty = false;

        }


        /// <summary>
        /// 位图是否需要重新生成。
        /// </summary>
        public bool IsDirty { get; private set; } = true;
    }
}
