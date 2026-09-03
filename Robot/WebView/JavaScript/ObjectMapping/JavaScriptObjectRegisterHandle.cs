// Robot 桌面软件 — JavaScript 对象注册句柄
// 表示一次对象注册会话,按帧跟踪进行中的注册

using System.Collections.Generic;
using System.Linq;
using Xilium.CefGlue;

namespace Robot.JavaScript
{

    /// <summary>
    /// JavaScript 对象注册句柄:表示一次对象注册会话,按帧跟踪进行中的注册。
    /// </summary>
    public class JavaScriptObjectRegisterHandle
    {
        /// <summary>
        /// 所有进行中的注册句柄。
        /// </summary>
        internal static List<JavaScriptObjectRegisterHandle> Handles { get; } = new();

        /// <summary>
        /// 句柄标识。
        /// </summary>
        internal long Id { get; init; }

        /// <summary>
        /// 句柄关联的帧。
        /// </summary>
        internal CefFrame Frame { get; }

        /// <summary>
        /// 判断指定帧是否已有进行中的注册。
        /// </summary>
        /// <param name="frame">目标帧。</param>
        /// <returns>存在时返回 true。</returns>
        internal static bool Exists(CefFrame frame)
        {
            return Handles.Any(x => x.Frame.Identifier == frame.Identifier);
        }

        /// <summary>
        /// 初始化 <see cref="JavaScriptObjectRegisterHandle"/> 实例并加入句柄集合。
        /// </summary>
        /// <param name="frame">句柄关联的帧。</param>
        internal JavaScriptObjectRegisterHandle(CefFrame frame)
        {
            Frame = frame;
            Id = frame.Identifier;
            Handles.Add(this);
        }
    }
}
