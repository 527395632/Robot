// Robot 桌面软件 — 窗口绑定对象函数类型
// 枚举窗口绑定函数的执行位置与同步/异步类型

using System.ComponentModel;

namespace Robot.JavaScript
{

    /// <summary>
    /// 窗口绑定对象函数类型:枚举窗口绑定函数的执行位置与同步/异步类型。
    /// </summary>
    public enum JavaScriptWindowBindingObjectFunctionType
    {
        /// <summary>
        /// 本地侧同步函数。
        /// </summary>
        [Description("本地侧同步函数")]
        SynchronousFunctionOnLocal,

        /// <summary>
        /// 远端侧同步函数。
        /// </summary>
        [Description("远端侧同步函数")]
        SynchronousFunctionOnRemote,

        /// <summary>
        /// 本地侧异步函数。
        /// </summary>
        [Description("本地侧异步函数")]
        AsynchronousFunctionOnLocal,

        /// <summary>
        /// 远端侧异步函数。
        /// </summary>
        [Description("远端侧异步函数")]
        AsynchronousFunctionOnRemote
    }
}
