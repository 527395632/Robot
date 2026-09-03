// Robot 桌面软件 — 开发者工具客户端
// 承载开发者工具窗口的 CEF 客户端

using Xilium.CefGlue;

namespace Robot.Browser.DevTools
{

    /// <summary>
    /// 开发者工具客户端:承载开发者工具窗口的 CEF 客户端。
    /// </summary>
    internal class DevToolsClient : CefClient
    {
        /// <summary>
        /// 开发者工具窗口。
        /// </summary>
        public DevToolsWindow DevToolsWindow { get; }

        /// <summary>
        /// 初始化 <see cref="DevToolsClient"/> 实例。
        /// </summary>
        /// <param name="devToolsWindow">开发者工具窗口。</param>
        public DevToolsClient(DevToolsWindow devToolsWindow)
        {
            DevToolsWindow = devToolsWindow;
        }

        /// <summary>
        /// 获取生命周期处理器。
        /// </summary>
        /// <returns>生命周期处理器。</returns>
        protected override CefLifeSpanHandler? GetLifeSpanHandler()
        {
            return new DevToolsLifeSpanHandler(DevToolsWindow);
        }
    }
}
