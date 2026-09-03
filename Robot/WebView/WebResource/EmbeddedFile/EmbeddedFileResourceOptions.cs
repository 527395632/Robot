// Robot 桌面软件 — 嵌入式文件资源选项
// 配置嵌入式资源所在程序集、资源目录与默认命名空间

using System.Reflection;

namespace Robot.WebResource
{

    /// <summary>
    /// 嵌入式文件资源选项:配置嵌入式资源所在程序集、资源目录与默认命名空间。
    /// </summary>
    public sealed class EmbeddedFileResourceOptions : ResourceOptions
    {
        /// <summary>
        /// 嵌入式资源目录名称;为空时资源位于程序集根目录。
        /// </summary>
        public string? EmbeddedResourceDirectoryName { get; init; }

        /// <summary>
        /// 默认命名空间;为空时取程序集入口类型命名空间或程序集名称。
        /// </summary>
        public string? DefaultNamespace { get; init; }

        /// <summary>
        /// 资源所在程序集。
        /// </summary>
        public required Assembly ResourceAssembly { get; init; }
    }
}
