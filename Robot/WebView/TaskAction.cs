// Robot 桌面软件 — 任务动作
// 封装 Action 为 CEF 任务,投递到指定线程执行

using System;
using Xilium.CefGlue;

namespace Robot
{

    /// <summary>
    /// 任务动作:封装 <see cref="Action"/> 为 CEF 任务,投递到指定线程执行。
    /// </summary>
    internal sealed class TaskAction : CefTask
    {
        /// <summary>
        /// 待执行的动作。
        /// </summary>
        private Action? _action;

        /// <summary>
        /// 初始化 <see cref="TaskAction"/> 实例。
        /// </summary>
        /// <param name="action">待执行的动作。</param>
        public TaskAction(Action action)
        {
            _action = action;
        }

        /// <summary>
        /// 执行动作并释放引用。
        /// </summary>
        protected override void Execute()
        {
            _action?.Invoke();
            _action = null;
        }

        /// <summary>
        /// 将动作投递到指定线程执行。
        /// </summary>
        /// <param name="action">待执行的动作。</param>
        /// <param name="threadId">目标线程,默认为 UI 线程。</param>
        public static void Run(Action action, CefThreadId threadId = CefThreadId.UI)
        {
            CefRuntime.PostTask(threadId, new TaskAction(action));
        }
    }
}
