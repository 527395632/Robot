// Robot 桌面软件 — 嵌入式文件资源注册
// 将嵌入式文件资源方案处理器工厂注册到依赖注入容器

using Microsoft.Extensions.DependencyInjection;

namespace Robot.WebResource
{

    /// <summary>
    /// 嵌入式文件资源注册:将嵌入式文件资源方案处理器工厂注册到依赖注入容器。
    /// </summary>
    public static class EmbeddedFileResourceRegister
    {
        /// <summary>
        /// 注册嵌入式文件资源方案处理器工厂。
        /// </summary>
        /// <param name="services">服务集合。</param>
        /// <param name="options">嵌入式文件资源选项。</param>
        /// <returns>传入的服务集合, 便于链式调用。</returns>
        public static IServiceCollection AddEmbeddedFileResource(this IServiceCollection services, EmbeddedFileResourceOptions options)
        {
            services.AddScoped<ResourceSchemeHandlerFactory, EmbeddedFileResourceSchemeHandlerFactory>(provider =>
            {
                return new EmbeddedFileResourceSchemeHandlerFactory(options);
            });

            return services;
        }
    }
}
