// Robot 桌面软件 — 右键菜单渲染器
// 自定义右键菜单的渲染,支持深色/浅色主题与图标亮度调整

using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Robot.Browser.ContextMenu
{

    /// <summary>
    /// 右键菜单渲染器:自定义右键菜单的渲染,支持深色/浅色主题与图标亮度调整。
    /// </summary>
    internal class ContextMenuStripRenderer : ToolStripProfessionalRenderer
    {
        /// <summary>
        /// 是否深色模式。
        /// </summary>
        public bool IsDarkMode { get; }

        /// <summary>
        /// 菜单项内边距。
        /// </summary>
        const int PADDING = 6;

        /// <summary>
        /// 初始化 <see cref="ContextMenuStripRenderer"/> 实例。
        /// </summary>
        /// <param name="isDarkMode">是否深色模式。</param>
        public ContextMenuStripRenderer(bool isDarkMode) :
            base(isDarkMode ? new ContextMenuStripColorTableDark() : new ContextMenuStripColorTableLight())
        {
            IsDarkMode = isDarkMode;
        }

        /// <summary>
        /// 初始化菜单项。
        /// </summary>
        /// <param name="item">菜单项。</param>
        protected override void InitializeItem(ToolStripItem item)
        {
            base.InitializeItem(item);
        }

        /// <summary>
        /// 初始化工具栏:扩展宽度。
        /// </summary>
        /// <param name="toolStrip">工具栏。</param>
        protected override void Initialize(ToolStrip toolStrip)
        {
            toolStrip.Width += 20;

            base.Initialize(toolStrip);
        }

        /// <summary>
        /// 渲染工具栏背景。
        /// </summary>
        /// <param name="e">渲染事件参数。</param>
        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            base.OnRenderToolStripBackground(e);
        }

        /// <summary>
        /// 渲染菜单项背景。
        /// </summary>
        /// <param name="e">菜单项渲染事件参数。</param>
        protected override void OnRenderItemBackground(ToolStripItemRenderEventArgs e)
        {
            base.OnRenderItemBackground(e);
        }

        /// <summary>
        /// 渲染菜单项背景。
        /// </summary>
        /// <param name="e">菜单项渲染事件参数。</param>
        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            base.OnRenderMenuItemBackground(e);
        }

        /// <summary>
        /// 渲染菜单项图标:深色模式下对启用项图标进行亮度调整。
        /// </summary>
        /// <param name="e">菜单项图标渲染事件参数。</param>
        protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
        {
            if (e.Image == null || !IsDarkMode)
            {
                base.OnRenderItemImage(e);

                return;
            }

            if (!e.Item.Enabled)
            {
                base.OnRenderItemImage(e);

                return;
            }

            var brightness = 1.5f; // 亮度不变
            var contrast = 1.0f; // 对比度不变
            var gamma = 1.0f; // 伽马不变

            var adjustedBrightness = brightness - 1.0f;

            float[][] ptsArray = {
                new float[] {contrast, 0, 0, 0, 0},
                new float[] {0, contrast, 0, 0, 0},
                new float[] {0, 0, contrast, 0, 0},
                new float[] {0, 0, 0, 1.0f, 0},
                new float[] {adjustedBrightness, adjustedBrightness,
            adjustedBrightness, 0, 1}};

            var imageAttributes = new ImageAttributes();
            imageAttributes.ClearColorMatrix();
            imageAttributes.SetColorMatrix(new ColorMatrix(ptsArray),
            ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            imageAttributes.SetGamma(gamma, ColorAdjustType.Bitmap);

            e.Graphics.DrawImage(e.Image, e.ImageRectangle, 0, 0, e.Image.Width, e.Image.Height, GraphicsUnit.Pixel, imageAttributes);

            //base.OnRenderItemImage(e);
        }

        /// <summary>
        /// 渲染菜单项文本:根据主题与启用状态设置文本颜色。
        /// </summary>
        /// <param name="e">菜单项文本渲染事件参数。</param>
        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            if (IsDarkMode)
            {
                if (e.Item.Enabled)
                {
                    e.TextColor = Color.FromArgb(0xff, 0xff, 0xff);
                }
                else
                {
                    e.TextColor = Color.FromArgb(0xfa, 0xfa, 0xfa);
                }
            }
            else
            {
                if (e.Item.Enabled)
                {
                    e.TextColor = Color.FromArgb(0x1a, 0x1a, 0x1a);
                }
                else
                {
                    e.TextColor = Color.FromArgb(0xc3, 0xc3, 0xc3);
                }
            }

            //e.Item.Padding = new Padding(PADDING, PADDING / 2, PADDING, PADDING / 2);

            //e.Item.TextAlign = ContentAlignment.MiddleLeft;

            //e.TextRectangle = new Rectangle(e.TextRectangle.X, e.TextRectangle.Y, e.TextRectangle.Width + e.Item.Padding.Horizontal, e.TextRectangle.Height + e.Item.Padding.Vertical);

            //e.Item.Size = new Size(e.Item.Size.Width + e.Item.Padding.Horizontal, e.Item.Size.Height + e.Item.Padding.Vertical);

            base.OnRenderItemText(e);
        }
    }
}
