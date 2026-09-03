// Robot 桌面软件 — 控制台消息事件参数
// 对应 CEF OnConsoleMessage 回调,携带日志级别/消息/来源/行号,可设置 Handled 拦截

using System;
using Xilium.CefGlue;

namespace Robot
{

    /// <summary>
    /// 控制台消息事件参数(对应 CEF OnConsoleMessage 回调)。
    /// 携带日志级别/消息/来源/行号,可设置 <see cref="Handled"/> 拦截。
    /// </summary>
    public class ConsoleMessageEventArgs : EventArgs
    {
        /// <summary>
        /// 初始化 <see cref="ConsoleMessageEventArgs"/> 实例。
        /// </summary>
        public ConsoleMessageEventArgs(CefBrowser browser, CefLogSeverity level, string message, string source, int line)
        {
            Level = level;
            Message = message;
            Source = source;
            Line = line;
        }

        /// <summary>
        /// 日志级别。
        /// </summary>
        public CefLogSeverity Level { get; }
        /// <summary>
        /// 消息内容。
        /// </summary>
        public string Message { get; }
        /// <summary>
        /// 来源。
        /// </summary>
        public string Source { get; }
        /// <summary>
        /// 行号。
        /// </summary>
        public int Line { get; }
        /// <summary>
        /// 是否已处理。
        /// </summary>
        public bool Handled { get; set; } = false;
    }
}
