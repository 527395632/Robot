// Robot 桌面软件 — 资源方案处理器工厂基类
// 封装 CEF 方案处理器工厂: 记录方案与域名, 创建资源处理器并支持注册与释放

using System;
using System.Runtime.InteropServices;
using Xilium.CefGlue;

namespace Robot.WebResource
{

    /// <summary>
    /// 资源方案处理器工厂基类:封装 CEF 方案处理器工厂, 记录方案与域名, 创建资源处理器并支持注册与释放。
    /// </summary>
    public abstract class ResourceSchemeHandlerFactory : CefSchemeHandlerFactory, IDisposable
    {
        /// <summary>
        /// 用于保持工厂实例存活的 GC 句柄。
        /// </summary>
        private GCHandle _gcHandler;

        /// <summary>
        /// 自定义方案名。
        /// </summary>
        public string Scheme { get; }

        /// <summary>
        /// 域名。
        /// </summary>
        public string DomainName { get; }

        /// <summary>
        /// 是否为标准方案(http/https/file/ftp/about/data)。
        /// </summary>
        public bool IsStandardScheme
        {
            get
            {
                return (Scheme?.ToLower()) switch
                {
                    "http" or "https" or "file" or "ftp" or "about" or "data" => true,
                    _ => false,
                };
            }
        }

        /// <summary>
        /// 最近一次创建的资源处理器。
        /// </summary>
        private CefResourceHandler? _resourceHandler;

        /// <summary>
        /// 初始化 <see cref="ResourceSchemeHandlerFactory"/> 实例。
        /// </summary>
        /// <param name="scheme">自定义方案名。</param>
        /// <param name="domainName">域名。</param>
        public ResourceSchemeHandlerFactory(string scheme, string domainName)
        {
            _gcHandler = GCHandle.Alloc(this);

            Scheme = scheme;
            DomainName = domainName;
        }

        /// <summary>
        /// 根据浏览器、帧与请求创建资源处理器, 由子类实现。
        /// </summary>
        /// <param name="browser">发起请求的浏览器实例。</param>
        /// <param name="frame">发起请求的帧实例。</param>
        /// <param name="request">资源请求对象。</param>
        /// <returns>资源处理器实例。</returns>
        protected abstract CefResourceHandler? GetResourceHandler(CefBrowser browser, CefFrame frame, CefRequest request);

        /// <summary>
        /// 创建资源处理器: 调用子类实现并记录结果。
        /// </summary>
        /// <param name="browser">发起请求的浏览器实例。</param>
        /// <param name="frame">发起请求的帧实例。</param>
        /// <param name="schemeName">方案名。</param>
        /// <param name="request">资源请求对象。</param>
        /// <returns>资源处理器实例。</returns>
        protected override CefResourceHandler Create(CefBrowser browser, CefFrame frame, string schemeName, CefRequest request)
        {
            _resourceHandler = GetResourceHandler(browser, frame, request);
            return _resourceHandler!;
        }

        /// <summary>
        /// 注册资源方案处理器; 默认空实现, 可由子类重写。
        /// </summary>
        internal protected virtual void ResourceSchemeHandlerRegister()
        {
        }

        /// <summary>
        /// 释放资源: 释放 GC 句柄。
        /// </summary>
        /// <param name="isDisposing">是否由 Dispose 触发。</param>
        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                _gcHandler.Free();
            }

            base.Dispose(isDisposing);
        }

        /// <summary>
        /// 释放资源: 抑制终结器。
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
