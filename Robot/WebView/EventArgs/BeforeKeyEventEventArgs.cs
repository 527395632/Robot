// Robot 桌面软件 — 按键事件前参数
// 对应 CEF OnPreKeyEvent 回调,携带按键事件,可标记是否已处理/是否为快捷键

using System;
using Xilium.CefGlue;

namespace Robot
{

    /// <summary>
    /// 按键事件前参数(对应 CEF OnPreKeyEvent 回调)。
    /// 携带按键事件,可标记是否已处理、是否为键盘快捷键。
    /// </summary>
    public class BeforeKeyEventEventArgs : EventArgs
    {
        /// <summary>
        /// 初始化 <see cref="BeforeKeyEventEventArgs"/> 实例。
        /// </summary>
        public BeforeKeyEventEventArgs(CefBrowser browser, CefKeyEvent keyEvent)
        {
            Browser = browser;
            KeyEvent = keyEvent;
        }

        /// <summary>
        /// 是否为键盘快捷键。
        /// </summary>
        public bool IsKeyboardShortcut { get; set; }
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
