// Robot 桌面软件 — 代理资源注册
// 将代理资源方案处理器工厂注册到依赖注入容器

using Microsoft.Extensions.DependencyInjection;

namespace Robot.WebResource
{

    /// <summary>
    /// 代理资源注册:将代理资源方案处理器工厂注册到依赖注入容器。
    /// </summary>
    public static class ProxyResourceRegister
    {
        /// <summary>
        /// 注册代理资源方案处理器工厂。
        /// </summary>
        /// <param name="services">服务集合。</param>
        /// <param name="scheme">自定义方案名。</param>
        /// <param name="domainName">域名。</param>
        /// <param name="proxy">代理地址。</param>
        /// <returns>传入的服务集合, 便于链式调用。</returns>
        public static IServiceCollection AddProxyResource(this IServiceCollection services, string scheme, string domainName, string proxy)
        {
            services.AddScoped<ResourceSchemeHandlerFactory, ProxyResourceSchemeHandlerFactory>(provider =>
            {
                return new ProxyResourceSchemeHandlerFactory(scheme, domainName, proxy);
            });

            return services;
        }
    }
}
