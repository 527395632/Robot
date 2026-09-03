// Robot 桌面软件 — 深色右键菜单配色表
// 提供深色主题的右键菜单配色

using System.Drawing;
using System.Windows.Forms;

namespace Robot.Browser.ContextMenu
{

    /// <summary>
    /// 深色右键菜单配色表:提供深色主题的右键菜单配色。
    /// </summary>
    internal class ContextMenuStripColorTableDark : ProfessionalColorTable
    {
        /// <summary>
        /// 菜单背景色。
        /// </summary>
        static readonly Color MENU_BACK_COLOR = Color.FromArgb(0x2e, 0x2e, 0x2e);

        /// <summary>
        /// 菜单边框色。
        /// </summary>
        static readonly Color MENU_BORDER_COLOR = Color.FromArgb(0x42, 0x42, blue: 0x42);

        /// <summary>
        /// 菜单高亮色。
        /// </summary>
        static readonly Color MENU_HIGHLIGHT_COLOR = Color.FromArgb(0x4d, 0x4d, blue: 0x4d);

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
