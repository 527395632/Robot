// Robot 桌面软件 — 窗口绑定对象注册
// 提供依赖注入扩展方法,注册窗口绑定对象

using System;
using Microsoft.Extensions.DependencyInjection;

namespace Robot.JavaScript
{

    /// <summary>
    /// 窗口绑定对象注册:提供依赖注入扩展方法,注册窗口绑定对象。
    /// </summary>
    public static class JavaScriptWindowBindingObjectRegister
    {
        /// <summary>
        /// 以注册委托添加窗口绑定对象。
        /// </summary>
        /// <typeparam name="T">窗口绑定对象类型。</typeparam>
        /// <param name="services">服务集合。</param>
        /// <param name="registerDelegate">注册委托。</param>
        /// <returns>服务集合(支持链式调用)。</returns>
        public static IServiceCollection AddWindowBindingObject<T>(this IServiceCollection services, Func<IServiceProvider, T> registerDelegate) where T : JavaScriptWindowBindingObject
        {
            services.AddScoped<JavaScriptWindowBindingObject>(registerDelegate);

            return services;
        }

        /// <summary>
        /// 添加窗口绑定对象类型。
        /// </summary>
        /// <typeparam name="T">窗口绑定对象类型。</typeparam>
        /// <param name="services">服务集合。</param>
        /// <returns>服务集合(支持链式调用)。</returns>
        public static IServiceCollection AddWindowBindingObject<T>(this IServiceCollection services) where T : JavaScriptWindowBindingObject
        {
            JavaScriptWindowBindingObjectBridge.WindowBindingObjectTypes.Add(typeof(T));
            return services;
        }
    }
}
