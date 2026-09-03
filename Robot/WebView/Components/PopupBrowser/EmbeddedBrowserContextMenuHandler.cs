// Robot 桌面软件 — 嵌入式浏览器右键菜单处理器
// 过滤 CEF 默认右键菜单,仅保留编辑类与用户自定义菜单项

using System.Collections.Generic;
using Robot.Browser.ContextMenu;
using Xilium.CefGlue;

namespace Robot.Browser.EmbeddedBrowser
{

    /// <summary>
    /// 嵌入式浏览器右键菜单处理器:过滤 CEF 默认右键菜单,仅保留编辑类与用户自定义菜单项。
    /// </summary>
    internal class EmbeddedBrowserContextMenuHandler : CefContextMenuHandler
    {
        /// <summary>
        /// 嵌入式浏览器客户端。
        /// </summary>
        public EmbeddedlBrowserClient BrowserClient { get; }

        /// <summary>
        /// 初始化 <see cref="EmbeddedBrowserContextMenuHandler"/> 实例。
        /// </summary>
        /// <param name="browserClient">嵌入式浏览器客户端。</param>
        public EmbeddedBrowserContextMenuHandler(EmbeddedlBrowserClient browserClient)
        {
            BrowserClient = browserClient;
        }

        /// <summary>
        /// 右键菜单显示前回调:移除非编辑类且非用户自定义的菜单项。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="state">右键菜单参数。</param>
        /// <param name="model">菜单模型。</param>
        protected override void OnBeforeContextMenu(CefBrowser browser, CefFrame frame, CefContextMenuParams state, CefMenuModel model)
        {
            List<int> removeCmds = new();

            for (var i = 0; i < (int)model.Count; i++)
            {
                var nCmd = model.GetCommandIdAt((nuint)i);

                if (!ContextMenuHelper.IsEditingItem(nCmd) && !ContextMenuHelper.IsUserDefinedItem(nCmd))
                {
                    removeCmds.Add(nCmd);
                }
            }

            foreach (var cmdId in removeCmds)
            {
                model.Remove(cmdId);
            }
        }

        /// <summary>
        /// 右键菜单命令回调。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="state">右键菜单参数。</param>
        /// <param name="commandId">命令 ID。</param>
        /// <param name="eventFlags">事件标志。</param>
        /// <returns>是否已处理。</returns>
        protected override bool OnContextMenuCommand(CefBrowser browser, CefFrame frame, CefContextMenuParams state, int commandId, CefEventFlags eventFlags)
        {
            return base.OnContextMenuCommand(browser, frame, state, commandId, eventFlags);
        }

        /// <summary>
        /// 右键菜单关闭回调。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        protected override void OnContextMenuDismissed(CefBrowser browser, CefFrame frame)
        {
            base.OnContextMenuDismissed(browser, frame);
        }

        /// <summary>
        /// 运行右键菜单回调。
        /// </summary>
        /// <param name="browser">浏览器。</param>
        /// <param name="frame">帧。</param>
        /// <param name="parameters">右键菜单参数。</param>
        /// <param name="model">菜单模型。</param>
        /// <param name="callback">运行菜单回调。</param>
        /// <returns>是否已处理。</returns>
        protected override bool RunContextMenu(CefBrowser browser, CefFrame frame, CefContextMenuParams parameters, CefMenuModel model, CefRunContextMenuCallback callback)
        {
            return base.RunContextMenu(browser, frame, parameters, model, callback);
        }
    }
}
