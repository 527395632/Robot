// Robot 桌面软件 — 右键菜单项
// 描述单个右键菜单项的文本、图标、命令 ID、子菜单等信息

using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Xilium.CefGlue;

namespace Robot.Browser.ContextMenu
{

    /// <summary>
    /// 右键菜单项:描述单个右键菜单项的文本、图标、命令 ID、子菜单等信息。
    /// </summary>
    public class ContextMenuItem
    {
        /// <summary>
        /// 菜单项文本。
        /// </summary>
        public string? Text { get; set; }

        /// <summary>
        /// 命令 ID。
        /// </summary>
        public int CommandId { get; set; }

        /// <summary>
        /// 菜单项图标。
        /// </summary>
        public Image? Icon { get; set; }

        /// <summary>
        /// 是否启用。
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 是否为分隔符。
        /// </summary>
        public bool IsSeperator { get; set; }

        /// <summary>
        /// 是否勾选(null 表示不适用)。
        /// </summary>
        public bool? IsChecked { get; set; }

        /// <summary>
        /// 菜单项类型。
        /// </summary>
        public CefMenuItemType MenuItemType { get; set; } = CefMenuItemType.Command;

        /// <summary>
        /// 子菜单集合。
        /// </summary>
        public List<ContextMenuItem>? SubMenus { get; set; }

        /// <summary>
        /// 快捷键(null 表示无)。
        /// </summary>
        public Keys? ShortKeys { get; set; }
    }
}
