// Robot 桌面软件 — 工具提示事件参数
// 对应 CEF OnTooltip 回调,携带提示文本,可设置 Handled 拦截

using System;
using Xilium.CefGlue;

namespace Robot
{

    /// <summary>
    /// 工具提示事件参数(对应 CEF OnTooltip 回调)。
    /// 携带提示文本,可设置 <see cref="Handled"/> 拦截。
    /// </summary>
    public class TooltipEventArgs : EventArgs
    {
        /// <summary>
        /// 触发事件的浏览器。
        /// </summary>
        public CefBrowser Browser { get; }
        /// <summary>
        /// 提示文本。
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// 是否已处理(设为 true 可拦截默认工具提示)。
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// 初始化 <see cref="TooltipEventArgs"/> 实例。
        /// </summary>
        public TooltipEventArgs(CefBrowser browser, string text)
        {
            Browser = browser;
            Text = text;
        }
    }
}
