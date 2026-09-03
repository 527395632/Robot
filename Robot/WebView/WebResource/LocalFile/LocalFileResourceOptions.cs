// Robot 桌面软件 — 本地文件资源选项
// 配置本地文件资源的物理根目录

namespace Robot.WebResource
{

    /// <summary>
    /// 本地文件资源选项:配置本地文件资源的物理根目录。
    /// </summary>
    public class LocalFileResourceOptions : ResourceOptions
    {
        /// <summary>
        /// 本地文件的物理根目录。
        /// </summary>
        public required string PhysicalFilePath { get; init; }
    }
}
