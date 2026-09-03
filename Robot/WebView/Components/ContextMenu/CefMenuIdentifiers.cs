// Robot 桌面软件 — CEF 菜单标识符
// 定义 CEF 右键菜单项的标识符常量

using System.ComponentModel;

namespace Robot.Browser.ContextMenu
{

    /// <summary>
    /// CEF 菜单标识符:定义 CEF 右键菜单项的标识符常量。
    /// </summary>
    internal enum CefMenuIdentifiers
    {
        /// <summary>
        /// 未找到。
        /// </summary>
        [Description("未找到")]
        NOT_FOUND = -1,

        // 导航。
        /// <summary>
        /// 后退。
        /// </summary>
        [Description("后退")]
        MENU_ID_BACK = 100,

        /// <summary>
        /// 前进。
        /// </summary>
        [Description("前进")]
        MENU_ID_FORWARD = 101,

        /// <summary>
        /// 重新加载。
        /// </summary>
        [Description("重新加载")]
        MENU_ID_RELOAD = 102,

        /// <summary>
        /// 强制重新加载(不使用缓存)。
        /// </summary>
        [Description("强制重新加载")]
        MENU_ID_RELOAD_NOCACHE = 103,

        /// <summary>
        /// 停止加载。
        /// </summary>
        [Description("停止加载")]
        MENU_ID_STOPLOAD = 104,

        // 编辑。
        /// <summary>
        /// 撤销。
        /// </summary>
        [Description("撤销")]
        MENU_ID_UNDO = 110,

        /// <summary>
        /// 重做。
        /// </summary>
        [Description("重做")]
        MENU_ID_REDO = 111,

        /// <summary>
        /// 剪切。
        /// </summary>
        [Description("剪切")]
        MENU_ID_CUT = 112,

        /// <summary>
        /// 复制。
        /// </summary>
        [Description("复制")]
        MENU_ID_COPY = 113,

        /// <summary>
        /// 粘贴。
        /// </summary>
        [Description("粘贴")]
        MENU_ID_PASTE = 114,

        /// <summary>
        /// 删除。
        /// </summary>
        [Description("删除")]
        MENU_ID_DELETE = 115,

        /// <summary>
        /// 全选。
        /// </summary>
        [Description("全选")]
        MENU_ID_SELECT_ALL = 116,

        // 其他。
        /// <summary>
        /// 查找。
        /// </summary>
        [Description("查找")]
        MENU_ID_FIND = 130,

        /// <summary>
        /// 打印。
        /// </summary>
        [Description("打印")]
        MENU_ID_PRINT = 131,

        /// <summary>
        /// 查看源代码。
        /// </summary>
        [Description("查看源代码")]
        MENU_ID_VIEW_SOURCE = 132,

        // 所有用户自定义菜单 ID 应位于 MENU_ID_USER_FIRST 与 MENU_ID_USER_LAST 之间,
        // 以避免与 tools/gritsettings/resource_ids 文件中定义的 Chromium 和 CEF ID 范围重叠。
        //MENU_ID_USER_FIRST = 26500,
        //MENU_ID_USER_LAST = 28500,

        /// <summary>
        /// 显示开发者工具。
        /// </summary>
        [Description("显示开发者工具")]
        MENU_ID_SHOW_DEVTOOLS = 28499,

        /// <summary>
        /// 隐藏开发者工具。
        /// </summary>
        [Description("隐藏开发者工具")]
        MENU_ID_HIDE_DEVTOOLS = 28498
    }
}
