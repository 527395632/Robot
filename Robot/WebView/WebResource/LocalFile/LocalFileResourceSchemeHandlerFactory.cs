// Robot 桌面软件 — 本地文件资源方案处理器工厂
// 创建本地文件资源处理器, 按方案与域名拦截请求

using Xilium.CefGlue;

namespace Robot.WebResource
{

    /// <summary>
    /// 本地文件资源方案处理器工厂:创建本地文件资源处理器, 按方案与域名拦截请求。
    /// </summary>
    internal class LocalFileResourceSchemeHandlerFactory : ResourceSchemeHandlerFactory
    {
        /// <summary>
        /// 本地文件资源选项。
        /// </summary>
        public LocalFileResourceOptions Options { get; }

        /// <summary>
        /// 初始化 <see cref="LocalFileResourceSchemeHandlerFactory"/> 实例。
        /// </summary>
        /// <param name="options">本地文件资源选项。</param>
        public LocalFileResourceSchemeHandlerFactory(LocalFileResourceOptions options)
            : base(options.Scheme, options.DomainName)
        {
            Options = options;
        }

        /// <summary>
        /// 创建本地文件资源处理器。
        /// </summary>
        /// <param name="browser">发起请求的浏览器实例。</param>
        /// <param name="frame">发起请求的帧实例。</param>
        /// <param name="request">资源请求对象。</param>
        /// <returns>本地文件资源处理器实例。</returns>
        protected override CefResourceHandler GetResourceHandler(CefBrowser browser, CefFrame frame, CefRequest request)
        {
            return new LocalFileResourceHandler(browser, frame, request, Options);
        }
    }
}
