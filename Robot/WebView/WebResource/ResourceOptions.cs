// Robot 桌面软件 — 资源选项基类
// 定义资源方案的公共选项: 方案名、域名与资源未找到时的回退委托

namespace Robot.WebResource
{

    /// <summary>
    /// 资源文件未找到时的回退委托。
    /// </summary>
    /// <param name="requestUrl">请求地址。</param>
    /// <returns>返回一个可处理该请求地址的已存在路径。</returns>
    public delegate string ResourceFileFallbackDelegate(string requestUrl);

    /// <summary>
    /// 资源选项基类:定义资源方案的公共选项, 包括方案名、域名与资源未找到时的回退委托。
    /// </summary>
    public abstract class ResourceOptions
    {
        /// <summary>
        /// 自定义方案名, 默认为 http。
        /// </summary>
        public string Scheme { get; init; } = "http";

        /// <summary>
        /// 域名。
        /// </summary>
        public required string DomainName { get; init; }

        /// <summary>
        /// 资源文件未找到时的回退委托; 为空时不进行回退。
        /// </summary>
        public ResourceFileFallbackDelegate? OnFallback { get; init; }
    }
}
