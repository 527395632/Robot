// Robot 桌面软件 — 本地文件资源处理器
// 从物理目录读取本地文件, 按请求路径映射为文件路径并返回响应

using System.IO;
using Xilium.CefGlue;

namespace Robot.WebResource
{

    /// <summary>
    /// 本地文件资源处理器:从物理目录读取本地文件, 按请求路径映射为文件路径并返回响应。
    /// </summary>
    internal class LocalFileResourceHandler : ResourceHandler
    {
        /// <summary>
        /// 发起请求的浏览器实例。
        /// </summary>
        public CefBrowser Browser { get; }

        /// <summary>
        /// 发起请求的帧实例。
        /// </summary>
        public CefFrame Frame { get; }

        /// <summary>
        /// 资源请求对象。
        /// </summary>
        public CefRequest Request { get; }

        /// <summary>
        /// 本地文件资源选项。
        /// </summary>
        public LocalFileResourceOptions Options { get; }

        /// <summary>
        /// 是否启用 CORS 策略(本地文件资源固定启用)。
        /// </summary>
        protected override bool EnableCORSPolicy => true;

        /// <summary>
        /// 初始化 <see cref="LocalFileResourceHandler"/> 实例。
        /// </summary>
        /// <param name="browser">发起请求的浏览器实例。</param>
        /// <param name="frame">发起请求的帧实例。</param>
        /// <param name="request">资源请求对象。</param>
        /// <param name="options">本地文件资源选项。</param>
        public LocalFileResourceHandler(CefBrowser browser, CefFrame frame, CefRequest request, LocalFileResourceOptions options)
        {
            Browser = browser;
            Frame = frame;
            Request = request;
            Options = options;
        }

        /// <summary>
        /// 根据请求从物理目录读取本地文件并返回响应。
        /// </summary>
        /// <param name="request">资源请求对象。</param>
        /// <returns>资源响应;文件不存在时返回 404 状态。</returns>
        protected override ResourceResponse GetResourceResponse(ResourceRequest request)
        {
            var requestUrl = request.RequestUrl;

            var response = new ResourceResponse();

            // 仅处理 GET 请求, 其余方法直接返回 404
            if (request.Method != ResourceRequestMethod.GET)
            {
                response.HttpStatus = StatusCodes.Status404NotFound;

                return response;
            }

            var filePath = request.RelativePath;

            var physicalFilePath = Path.Combine(Options.PhysicalFilePath, filePath);

            // 请求未带文件名时, 依次尝试默认文件名
            if (!request.HasFileName)
            {
                foreach (var defaultFileName in SchemeOptions.DefaultFileName)
                {
                    physicalFilePath = Path.Combine(physicalFilePath, defaultFileName);

                    if (File.Exists(physicalFilePath))
                    {
                        break;
                    }
                }
            }

            // 文件不存在且配置了回退处理时, 使用回退路径
            if (!File.Exists(physicalFilePath) && Options.OnFallback != null)
            {
                var fallbackFile = Options.OnFallback.Invoke(requestUrl);

                physicalFilePath = Path.GetFullPath(Path.Combine(Options.PhysicalFilePath, fallbackFile));
            }

            if (File.Exists(physicalFilePath))
            {
                response.ContentBody = File.Open(physicalFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                response.ContentType = CefRuntime.GetMimeType(Path.GetExtension(physicalFilePath).Trim('.')) ?? "text/plain";
            }
            else
            {
                response.HttpStatus = StatusCodes.Status404NotFound;
            }

            return response;
        }
    }
}
