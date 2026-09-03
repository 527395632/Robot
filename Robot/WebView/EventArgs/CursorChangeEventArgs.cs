// Robot 桌面软件 — 光标变化事件参数
// 对应 CEF OnCursorChange 回调,可获取/设置 Win32 光标,支持自定义光标

using System;
using System.IO;
using System.Windows.Forms;
using Vanara.PInvoke;
using Xilium.CefGlue;
using static Vanara.PInvoke.User32;

namespace Robot
{

    /// <summary>
    /// 光标变化事件参数(对应 CEF OnCursorChange 回调)。
    /// 可获取/设置 Win32 光标,支持自定义光标。
    /// </summary>
    public class CursorChangeEventArgs : EventArgs
    {
        /// <summary>
        /// 光标句柄。
        /// </summary>
        private nint cursorHandle;
        /// <summary>
        /// 自定义光标信息。
        /// </summary>
        private CefCursorInfo customCursorInfo;

        /// <summary>
        /// 初始化 <see cref="CursorChangeEventArgs"/> 实例。
        /// </summary>
        public CursorChangeEventArgs(CefBrowser browser, nint cursorHandle, CefCursorType type, CefCursorInfo customCursorInfo)
        {
            Browser = browser;
            this.cursorHandle = cursorHandle;
            CursorType = type;
            this.customCursorInfo = customCursorInfo;
        }

        /// <summary>
        /// 触发事件的浏览器。
        /// </summary>
        public CefBrowser Browser { get; }
        /// <summary>
        /// 光标类型。
        /// </summary>
        public CefCursorType CursorType { get; }

        /// <summary>
        /// 获取 Win32 光标:标准光标按句柄创建,自定义光标按缓冲区创建,否则返回默认光标。
        /// </summary>
        public Cursor GetCursor()
        {
            if (cursorHandle != IntPtr.Zero && CursorType != CefCursorType.None && CursorType != CefCursorType.Custom)
            {
                return new Cursor(cursorHandle);
            }
            else if (IsCustomCursor)
            {
                using var buff = new MemoryStream(customCursorInfo.GetBuffer());
                var cursor = new Cursor(buff);
                return cursor;
            }
            return Cursors.Default;
        }

        /// <summary>
        /// 设置 Win32 光标。
        /// </summary>
        public void SetCursor(Cursor cursor)
        {
            User32.SetCursor(new User32.SafeHCURSOR(cursor.Handle));
        }

        /// <summary>
        /// 是否为自定义光标。
        /// </summary>
        public bool IsCustomCursor => CursorType == CefCursorType.Custom;

        /// <summary>
        /// 是否已处理。
        /// </summary>
        public bool Handled { get; set; }
    }
}
