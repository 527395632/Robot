// Robot 桌面软件 — 浏览器消息事件参数
// 携带 JS 侧发送的消息名与值

using System;

using Robot.JavaScript;

namespace Robot
{

    /// <summary>
    /// 浏览器消息事件参数,携带 JS 侧发送的消息名与值。
    /// </summary>
    public class BrowserMessageEventArgs : EventArgs
    {
        /// <summary>
        /// 初始化 <see cref="BrowserMessageEventArgs"/> 实例。
        /// </summary>
        public BrowserMessageEventArgs(string message, JavaScriptValue value)
        {
            Message = message;
            Value = value;
        }

        /// <summary>
        /// 消息名。
        /// </summary>
        public string Message { get; }
        /// <summary>
        /// 消息值。
        /// </summary>
        public JavaScriptValue Value { get; }
    }
}
