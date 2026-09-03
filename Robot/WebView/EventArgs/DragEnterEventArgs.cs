// Robot 桌面软件 — 拖拽进入事件参数
// 对应 CEF OnDragEnter 回调,携带拖拽数据与操作掩码,可设置 AllowDragEnter

using System;
using Xilium.CefGlue;

namespace Robot
{

    /// <summary>
    /// 拖拽进入事件参数(对应 CEF OnDragEnter 回调)。
    /// 携带拖拽数据与操作掩码,可设置 <see cref="AllowDragEnter"/> 控制是否允许拖入。
    /// </summary>
    public class DragEnterEventArgs : EventArgs
    {
        /// <summary>
        /// 初始化 <see cref="DragEnterEventArgs"/> 实例。
        /// </summary>
        public DragEnterEventArgs(CefBrowser browser, CefDragData dragData, CefDragOperationsMask mask)
        {
            Browser = browser;
            DragData = dragData;
            Mask = mask;
        }

        /// <summary>
        /// 触发事件的浏览器。
        /// </summary>
        public CefBrowser Browser { get; }
        /// <summary>
        /// 拖拽数据。
        /// </summary>
        public CefDragData DragData { get; }
        /// <summary>
        /// 拖拽操作掩码。
        /// </summary>
        public CefDragOperationsMask Mask { get; }
        /// <summary>
        /// 是否允许拖入。
        /// </summary>
        public bool AllowDragEnter { get; set; } = false;
    }
}
