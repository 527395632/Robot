// Robot 桌面软件 — 按键事件参数
// 对应 CEF OnKeyEvent 回调,携带按键事件,可设置 Handled 拦截

using System;
using Xilium.CefGlue;

namespace Robot
{

    /// <summary>
    /// 按键事件参数(对应 CEF OnKeyEvent 回调)。
    /// 携带按键事件,可设置 <see cref="Handled"/> 拦截。
    /// </summary>
    public class KeyEventEventArgs : EventArgs
    {
        /// <summary>
        /// 初始化 <see cref="KeyEventEventArgs"/> 实例。
        /// </summary>
        public KeyEventEventArgs(CefBrowser browser, CefKeyEvent keyEvent)
        {
            Browser = browser;
            KeyEvent = keyEvent;
        }

        /// <summary>
        /// 是否已处理(设为 true 可拦截该按键)。
        /// </summary>
        public bool Handled { get; set; }
        /// <summary>
        /// 触发事件的浏览器。
        /// </summary>
        public CefBrowser Browser { get; }
        /// <summary>
        /// 按键事件。
        /// </summary>
        public CefKeyEvent KeyEvent { get; }
    }
}
