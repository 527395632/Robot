// Robot 桌面软件 — 代理资源处理器
// 将资源请求转发到代理地址, 透传请求头与请求体, 并回传响应

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Xilium.CefGlue;

namespace Robot.WebResource
{

    /// <summary>
    /// 代理资源处理器:将资源请求转发到代理地址, 透传请求头与请求体, 并回传响应。
    /// </summary>
    internal class ProxyResourceHandler : ResourceHandler
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
        /// 代理地址。
        /// </summary>
        public string Proxy { get; }

        /// <summary>
        /// 用于转发请求的 HTTP 客户端(启用 Cookie)。
        /// </summary>
        HttpClient httpClient = new HttpClient(new HttpClientHandler { UseCookies = true });

        /// <summary>
        /// 是否启用 CORS 策略。
        /// </summary>
        protected override bool EnableCORSPolicy { get; }

        /// <summary>
        /// 初始化 <see cref="ProxyResourceHandler"/> 实例。
        /// </summary>
        /// <param name="browser">发起请求的浏览器实例。</param>
        /// <param name="frame">发起请求的帧实例。</param>
        /// <param name="request">资源请求对象。</param>
        /// <param name="proxy">代理地址。</param>
        /// <param name="enableCorsPolicy">是否启用 CORS 策略, 默认为 true。</param>
        public ProxyResourceHandler(CefBrowser browser, CefFrame frame, CefRequest request, string proxy, bool enableCorsPolicy = true)
        {
            Browser = browser;
            Frame = frame;
            Request = request;
            Proxy = proxy;

            EnableCORSPolicy = enableCorsPolicy;
        }

        /// <summary>
        /// 将请求转发到代理地址并返回响应。
        /// </summary>
        /// <param name="request">资源请求对象。</param>
        /// <returns>代理返回的资源响应。</returns>
        protected override ResourceResponse GetResourceResponse(ResourceRequest request)
        {
            httpClient.BaseAddress = new Uri(Proxy);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var message = new HttpRequestMessage(new HttpMethod(Request.Method), new Uri(Request.Url).PathAndQuery);

            // 透传请求头
            if (request.Headers != null)
            {
                for (var i = 0; i < request.Headers.Count; i++)
                {
                    var headerKey = request.Headers.GetKey(i);
                    var headerValue = request.Headers.Get(i);

                    if (headerKey != null && !message.Headers.TryAddWithoutValidation(headerKey, headerValue))
                    {
                    }
                }
            }

            // 根据请求体类型构造请求内容: JSON / 表单 / 文件上传
            if (request.JsonData != null && request.IsJson)
            {
                var data = request.JsonData;

                // JSON 字符串被引号包裹时, 去掉引号并反转义
                if (data.StartsWith("\"") && data.EndsWith("\""))
                {
                    data = data.Substring(1, data.Length - 2);

                    data = Regex.Unescape(data);
                }

                message.Content = new StringContent(data, request.ContentEncoding, request.ContentType);
            }
            else if (request.FormData != null && request.FormData.AllKeys != null && request.FormData.AllKeys.Length > 0)
            {
                var formData = request.FormData!.AllKeys!.Where(x => x != null).ToDictionary(x => x!, x => request.FormData![x!]);
                var formContent = new FormUrlEncodedContent(formData);
                message.Content = formContent;
            }
            else if (request.UploadFiles != null && request.UploadFiles.Length > 0)
            {
                var multipartContent = new MultipartFormDataContent();
                foreach (var file in request.UploadFiles)
                {
                    var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
                    var fileName = Path.GetFileName(file);
                    var fileContent = new StreamContent(fileStream);
                    multipartContent.Add(fileContent, "file", fileName);
                }
                message.Content = multipartContent;
            }

            var result = httpClient.SendAsync(message).GetAwaiter().GetResult()!;

            var response = new ResourceResponse()
            {
                ContentType = result.Content.Headers.ContentType?.MediaType,
                HttpStatus = (int)result.StatusCode,
            };

            // 透传响应头
            foreach (var header in result.Headers.ToList())
            {
                foreach (var v in header.Value)
                {
                    response.Headers.Add(header.Key, v);
                }
            }

            response.ContentBody = result.Content.ReadAsStreamAsync().GetAwaiter().GetResult();

            return response;
        }
    }
}
