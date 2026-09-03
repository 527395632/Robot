// Robot 桌面软件 — 本地文件资源注册
// 将本地文件资源方案处理器工厂注册到依赖注入容器

using Microsoft.Extensions.DependencyInjection;

namespace Robot.WebResource
{

    /// <summary>
    /// 本地文件资源注册:将本地文件资源方案处理器工厂注册到依赖注入容器。
    /// </summary>
    public static class LocalFileResourceRegister
    {
        /// <summary>
        /// 注册本地文件资源方案处理器工厂。
        /// </summary>
        /// <param name="services">服务集合。</param>
        /// <param name="options">本地文件资源选项。</param>
        /// <returns>传入的服务集合, 便于链式调用。</returns>
        public static IServiceCollection AddLocalFileResource(this IServiceCollection services, LocalFileResourceOptions options)
        {
            services.AddScoped<ResourceSchemeHandlerFactory, LocalFileResourceSchemeHandlerFactory>(provider =>
            {
                return new LocalFileResourceSchemeHandlerFactory(options);
            });

            return services;
        }
    }
}
