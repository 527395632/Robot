// Robot 桌面软件 — JavaScript 执行结果
// 承载跨进程 JavaScript 执行的成功/失败状态、异常文本与返回值

using Xilium.CefGlue;

namespace Robot.JavaScript
{

    /// <summary>
    /// JavaScript 执行结果:承载跨进程 JavaScript 执行的成功/失败状态、异常文本与返回值。
    /// </summary>
    public record JavaScriptResult
    {
        /// <summary>
        /// 结果关联的目标帧。
        /// </summary>
        internal CefFrame TargetFrame { get; }

        /// <summary>
        /// 是否执行成功。
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// 异常文本;无异常时为 null。
        /// </summary>
        public string? ExceptionText { get; }

        /// <summary>
        /// 执行返回值。
        /// </summary>
        public JavaScriptValue ReturnValue { get; }

        /// <summary>
        /// 初始化 <see cref="JavaScriptResult"/> 实例。
        /// </summary>
        /// <param name="frame">结果关联的目标帧。</param>
        /// <param name="isSuccess">是否执行成功。</param>
        /// <param name="data">返回值 JSON;成功且非 null 时反序列化为返回值。</param>
        /// <param name="jsException">异常文本。</param>
        internal JavaScriptResult(CefFrame frame, bool isSuccess, string? data, string? jsException)
        {
            TargetFrame = frame;
            Success = isSuccess;
            ExceptionText = jsException;

            if (isSuccess && data != null)
            {
                ReturnValue = JavaScriptValue.FromJson(data);

                ReturnValue.AssociateToFrame(frame);
            }
            else
            {
                ReturnValue = new JavaScriptValue();
            }
        }
    }
}
