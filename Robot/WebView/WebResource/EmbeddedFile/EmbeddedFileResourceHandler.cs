// Robot 桌面软件 — 嵌入式文件资源处理器
// 从程序集(含卫星程序集)中读取嵌入式资源, 按请求路径映射为资源名并返回响应

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Xilium.CefGlue;

namespace Robot.WebResource
{

    /// <summary>
    /// 嵌入式文件资源处理器:从程序集(含卫星程序集)中读取嵌入式资源, 按请求路径映射为资源名并返回响应。
    /// </summary>
    internal class EmbeddedFileResourceHandler : ResourceHandler
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
        /// 嵌入式文件资源选项。
        /// </summary>
        public EmbeddedFileResourceOptions Options { get; }

        /// <summary>
        /// 资源所在程序集。
        /// </summary>
        public Assembly ResourceAssembly => Options.ResourceAssembly;

        /// <summary>
        /// 默认命名空间:优先取选项中的默认命名空间, 否则取程序集入口类型命名空间, 再否则取程序集名称。
        /// </summary>
        public string DefaultNamespace => Options.DefaultNamespace ?? ResourceAssembly.EntryPoint?.DeclaringType?.Namespace ?? ResourceAssembly.GetName().Name!;

        /// <summary>
        /// 是否启用 CORS 策略(嵌入式资源固定启用)。
        /// </summary>
        protected override bool EnableCORSPolicy => true;

        /// <summary>
        /// 初始化 <see cref="EmbeddedFileResourceHandler"/> 实例。
        /// </summary>
        /// <param name="browser">发起请求的浏览器实例。</param>
        /// <param name="frame">发起请求的帧实例。</param>
        /// <param name="request">资源请求对象。</param>
        /// <param name="options">嵌入式文件资源选项。</param>
        public EmbeddedFileResourceHandler(CefBrowser browser, CefFrame frame, CefRequest request, EmbeddedFileResourceOptions options)
        {
            Browser = browser;
            Frame = frame;
            Request = request;
            Options = options;
        }

        /// <summary>
        /// 将相对路径(可选根路径)映射为程序集内的资源名。
        /// </summary>
        /// <param name="relativePath">相对路径。</param>
        /// <param name="rootPath">根路径;为空时仅使用相对路径。</param>
        /// <returns>映射后的资源名。</returns>
        private string GetResourceName(string relativePath, string? rootPath)
        {
            var filePath = relativePath;
            if (!string.IsNullOrEmpty(rootPath))
            {
                filePath = $"{rootPath?.Trim('/', '\\')}/{filePath.Trim('/', '\\')}";
            }

            filePath = filePath.Replace('\\', '/');

            var endTrimIndex = filePath.LastIndexOf('/');

            if (endTrimIndex > -1)
            {
                // 特殊字符处理参考: https://stackoverflow.com/questions/5769705/retrieving-embedded-resources-with-special-characters

                var path = filePath.Substring(0, endTrimIndex);
                path = path.Replace("/", ".");
                if (Regex.IsMatch(path, "\\.(\\d+)"))
                {
                    path = Regex.Replace(path, "\\.(\\d+)", "._$1");
                }

                const string replacePartterns = "`~!@$%^&(),-=";

                foreach (var parttern in replacePartterns)
                {
                    path = path.Replace(parttern, '_');
                }

                filePath = $"{path}{filePath.Substring(endTrimIndex)}".Trim('/');
            }

            var resourceName = $"{DefaultNamespace}.{filePath.Replace('/', '.')}";

            return resourceName;
        }

        /// <summary>
        /// 根据请求从程序集(含卫星程序集)中查找并返回嵌入式资源响应。
        /// </summary>
        /// <param name="request">资源请求对象。</param>
        /// <returns>资源响应;未找到时返回 404 状态。</returns>
        protected override ResourceResponse GetResourceResponse(ResourceRequest request)
        {
            var requestUrl = request.RequestUrl;

            var mainAssembly = ResourceAssembly;

            var response = new ResourceResponse();

            // 仅处理 GET 请求, 其余方法直接返回 404
            if (request.Method != ResourceRequestMethod.GET)
            {
                response.HttpStatus = StatusCodes.Status404NotFound;

                return response;
            }

            var resourceName = GetResourceName(request.RelativePath, Options.EmbeddedResourceDirectoryName);

            Assembly? satelliteAssembly = null;

            try
            {
                var fileInfo = new FileInfo(new Uri(mainAssembly.Location).LocalPath);

                var satelliteFilePath = Path.Combine(fileInfo.DirectoryName ?? string.Empty, $"{Thread.CurrentThread.CurrentCulture}", $"{Path.GetFileNameWithoutExtension(fileInfo.Name)}.resources.dll");

                if (File.Exists(satelliteFilePath))
                {
                    satelliteAssembly = mainAssembly.GetSatelliteAssembly(Thread.CurrentThread.CurrentCulture);
                }
            }
            catch
            {
                // 卫星程序集不存在或加载失败时忽略, 仅使用主程序集
            }

            var embeddedResources = mainAssembly.GetManifestResourceNames().Select(x => new { Target = mainAssembly, Name = x, ResourceName = x, IsSatellite = false });

            if (satelliteAssembly != null)
            {
                // 卫星程序集资源名需按当前文化名重新拼接
                static string ProcessCultureName(string filename) => $"{Path.GetFileNameWithoutExtension(Path.GetFileName(filename))}.{Thread.CurrentThread.CurrentCulture.Name}{Path.GetExtension(filename)}";

                embeddedResources = embeddedResources.Union(satelliteAssembly.GetManifestResourceNames().Select(x => new { Target = satelliteAssembly, Name = ProcessCultureName(x), ResourceName = ProcessCultureName(x), IsSatellite = true }));
            }

            var namespaces = mainAssembly.DefinedTypes.Select(x => x.Namespace).Distinct().ToArray();

            // 将资源名中的原始命名空间替换为默认命名空间, 便于按默认命名空间匹配
            string ChangeResourceName(string rawName)
            {
                var targetName = namespaces.Where(x => x != null && !string.IsNullOrEmpty(x) && rawName.StartsWith(x!)).OrderByDescending(x => x!.Length).FirstOrDefault();

                if (targetName == null)
                {
                    targetName = DefaultNamespace;
                }

                return $"{DefaultNamespace}{rawName.Substring($"{targetName}".Length)}";
            }

            embeddedResources = embeddedResources.Select(x =>
                new
                {
                    x.Target,
                    Name = ChangeResourceName(x.Name),
                    x.ResourceName,
                    x.IsSatellite
                });

            var resource = embeddedResources.SingleOrDefault(x => x.Name.Equals(resourceName, StringComparison.CurrentCultureIgnoreCase));

            // 未命中且请求未带文件名时, 依次尝试默认文件名
            if (resource == null && !request.HasFileName)
            {
                foreach (var defaultFileName in SchemeOptions.DefaultFileName)
                {
                    resourceName = string.Join(".", resourceName, defaultFileName);

                    resource = embeddedResources.SingleOrDefault(x => x.Name.Equals(resourceName, StringComparison.CurrentCultureIgnoreCase));

                    if (resource != null)
                    {
                        break;
                    }
                }
            }

            // 仍未命中且配置了回退处理时, 使用回退路径再次查找
            if (resource == null && Options.OnFallback != null)
            {
                var fallbackFile = Options.OnFallback.Invoke(requestUrl);

                resourceName = GetResourceName(fallbackFile, Options.EmbeddedResourceDirectoryName);

                resource = embeddedResources.SingleOrDefault(x => x.Name.Equals(resourceName, StringComparison.CurrentCultureIgnoreCase));
            }

            if (resource != null)
            {
                var manifestResourceName = resource.ResourceName;

                // 卫星程序集资源名需去掉文化名段后再读取
                if (resource.IsSatellite)
                {
                    manifestResourceName = $"{Path.GetFileNameWithoutExtension(Path.GetFileName(manifestResourceName))}{Path.GetExtension(manifestResourceName)}";
                }

                var contenStream = resource?.Target?.GetManifestResourceStream(manifestResourceName);

                if (contenStream != null)
                {
                    response.ContentBody = contenStream;
                    response.ContentType = CefRuntime.GetMimeType(Path.GetExtension(resourceName).Trim('.')) ?? "text/plain";

                    return response;
                }
            }

            response.HttpStatus = StatusCodes.Status404NotFound;

            return response;
        }
    }
}
