// Robot 桌面软件 — 设置焦点事件参数
// 对应 CEF OnSetFocus 回调,携带焦点来源,可设置 Handled 拦截

using System;
using Xilium.CefGlue;

namespace Robot
{

    /// <summary>
    /// 设置焦点事件参数(对应 CEF OnSetFocus 回调)。
    /// 携带焦点来源,可设置 <see cref="Handled"/> 拦截。
    /// </summary>
    public class SetFocusEventArgs
    {
        /// <summary>
        /// 初始化 <see cref="SetFocusEventArgs"/> 实例。
        /// </summary>
        public SetFocusEventArgs(CefBrowser browser, CefFocusSource source)
        {
            Browser = browser;
            Source = source;
        }

        /// <summary>
        /// 触发事件的浏览器。
        /// </summary>
        public CefBrowser Browser { get; }
        /// <summary>
        /// 焦点来源。
        /// </summary>
        public CefFocusSource Source { get; }
        /// <summary>
        /// 是否已处理(设为 true 可拦截默认焦点设置)。
        /// </summary>
        public bool Handled { get; set; }
    }
}
