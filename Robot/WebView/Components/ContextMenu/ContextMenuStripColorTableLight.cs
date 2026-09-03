// Robot 桌面软件 — 浅色右键菜单配色表
// 提供浅色主题的右键菜单配色

using System.Drawing;
using System.Windows.Forms;

namespace Robot.Browser.ContextMenu
{

    /// <summary>
    /// 浅色右键菜单配色表:提供浅色主题的右键菜单配色。
    /// </summary>
    internal class ContextMenuStripColorTableLight : ProfessionalColorTable
    {
        /// <summary>
        /// 菜单背景色。
        /// </summary>
        static readonly Color MENU_BACK_COLOR = Color.FromArgb(0xfa, 0xfa, 0xf9);

        /// <summary>
        /// 菜单边框色。
        /// </summary>
        static readonly Color MENU_BORDER_COLOR = Color.FromArgb(0xc7, 0xc7, blue: 0xc7);

        /// <summary>
        /// 菜单高亮色。
        /// </summary>
        static readonly Color MENU_HIGHLIGHT_COLOR = Color.FromArgb(0xed, 0xed, blue: 0xed);

        /// <summary>
        /// 菜单边框颜色。
        /// </summary>
        public override Color MenuBorder => MENU_BORDER_COLOR;

        /// <summary>
        /// 菜单项边框颜色。
        /// </summary>
        public override Color MenuItemBorder => Color.Transparent;

        /// <summary>
        /// 选中菜单项颜色。
        /// </summary>
        public override Color MenuItemSelected => MENU_HIGHLIGHT_COLOR;

        /// <summary>
        /// 选中菜单项渐变起始颜色。
        /// </summary>
        public override Color MenuItemSelectedGradientBegin => MENU_HIGHLIGHT_COLOR;

        /// <summary>
        /// 选中菜单项渐变结束颜色。
        /// </summary>
        public override Color MenuItemSelectedGradientEnd => MENU_HIGHLIGHT_COLOR;

        /// <summary>
        /// 下拉菜单背景颜色。
        /// </summary>
        public override Color ToolStripDropDownBackground => MENU_BACK_COLOR;

        /// <summary>
        /// 图标边距渐变起始颜色。
        /// </summary>
        public override Color ImageMarginGradientBegin => MENU_BACK_COLOR;

        /// <summary>
        /// 图标边距渐变中间颜色。
        /// </summary>
        public override Color ImageMarginGradientMiddle => MENU_BACK_COLOR;

        /// <summary>
        /// 图标边距渐变结束颜色。
        /// </summary>
        public override Color ImageMarginGradientEnd => MENU_BACK_COLOR;
    }
}
