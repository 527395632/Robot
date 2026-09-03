// Robot 桌面软件 — 资源方案处理器选项
// 定义资源方案处理器的默认文件名列表

namespace Robot.WebResource
{

    /// <summary>
    /// 资源方案处理器选项:定义资源方案处理器的默认文件名列表。
    /// </summary>
    public sealed class ResourceSchemeHandlerOptions
    {
        /// <summary>
        /// 默认文件名列表, 用于请求目录时回退到默认页面。
        /// </summary>
        public string[] DefaultFileName { get; set; } = new string[] { "index.html", "index.htm", "default.html" };
    }
}
