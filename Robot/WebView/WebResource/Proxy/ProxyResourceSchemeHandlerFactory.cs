// Robot 桌面软件 — 代理资源方案处理器工厂
// 创建代理资源处理器, 按方案与域名拦截请求并转发到代理地址

using Xilium.CefGlue;

namespace Robot.WebResource
{

    /// <summary>
    /// 代理资源方案处理器工厂:创建代理资源处理器, 按方案与域名拦截请求并转发到代理地址。
    /// </summary>
    internal class ProxyResourceSchemeHandlerFactory : ResourceSchemeHandlerFactory
    {
        /// <summary>
        /// 代理地址。
        /// </summary>
        public string Proxy { get; }

        /// <summary>
        /// 初始化 <see cref="ProxyResourceSchemeHandlerFactory"/> 实例。
        /// </summary>
        /// <param name="scheme">自定义方案名。</param>
        /// <param name="domainName">域名。</param>
        /// <param name="proxy">代理地址。</param>
        public ProxyResourceSchemeHandlerFactory(string scheme, string domainName, string proxy)
            : base(scheme, domainName)
        {
            Proxy = proxy;
        }

        /// <summary>
        /// 创建代理资源处理器。
        /// </summary>
        /// <param name="browser">发起请求的浏览器实例。</param>
        /// <param name="frame">发起请求的帧实例。</param>
        /// <param name="request">资源请求对象。</param>
        /// <returns>代理资源处理器实例。</returns>
        protected override CefResourceHandler? GetResourceHandler(CefBrowser browser, CefFrame frame, CefRequest request)
        {
            return new ProxyResourceHandler(browser, frame, request, Proxy);
        }
    }
}
