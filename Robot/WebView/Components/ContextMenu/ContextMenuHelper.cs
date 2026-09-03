// Robot 桌面软件 — 右键菜单辅助类
// 判断 CEF 右键菜单项的类型(编辑项、用户自定义项)

namespace Robot.Browser.ContextMenu
{

    /// <summary>
    /// 右键菜单辅助类:判断 CEF 右键菜单项的类型(编辑项、用户自定义项)。
    /// </summary>
    internal class ContextMenuHelper
    {
        /// <summary>
        /// 用户自定义菜单 ID 起始值。
        /// </summary>
        const int MENU_ID_USER_FIRST = 26500;

        /// <summary>
        /// 用户自定义菜单 ID 结束值。
        /// </summary>
        const int MENU_ID_USER_LAST = 28400;

        /// <summary>
        /// 判断指定菜单标识符是否为编辑类菜单项。
        /// </summary>
        /// <param name="contextMenuId">菜单标识符。</param>
        /// <returns>是否为编辑类菜单项。</returns>
        public static bool IsEditingItem(CefMenuIdentifiers contextMenuId)
        {
            var intValue = (int)contextMenuId;
            return IsEditingItem(intValue);
        }

        /// <summary>
        /// 判断指定整数值是否为编辑类菜单项。
        /// </summary>
        /// <param name="intValue">菜单项整数值。</param>
        /// <returns>是否为编辑类菜单项。</returns>
        public static bool IsEditingItem(int intValue)
        {
            return intValue >= (int)CefMenuIdentifiers.MENU_ID_UNDO && intValue <= (int)CefMenuIdentifiers.MENU_ID_SELECT_ALL;
        }

        /// <summary>
        /// 判断指定整数值是否为用户自定义菜单项。
        /// </summary>
        /// <param name="intValue">菜单项整数值。</param>
        /// <returns>是否为用户自定义菜单项。</returns>
        public static bool IsUserDefinedItem(int intValue)
        {
            return intValue > MENU_ID_USER_FIRST && intValue < MENU_ID_USER_LAST;
        }
    }
}
