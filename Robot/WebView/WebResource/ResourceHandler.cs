// Robot 桌面软件 — 资源处理器基类
// 封装 CEF 资源处理流程: 解析请求、异步获取响应、支持 CORS 与分片读取

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Xilium.CefGlue;

namespace Robot.WebResource
{

    /// <summary>
    /// 资源处理器基类:封装 CEF 资源处理流程, 解析请求、异步获取响应、支持 CORS 与分片读取。
    /// </summary>
    public abstract class ResourceHandler
        : CefResourceHandler
    {
        /// <summary>
        /// CORS 响应头: 允许的头。
        /// </summary>
        private const string ACCESS_CONTROL_ALLOW_HEADERS = "Access-Control-Allow-Headers";

        /// <summary>
        /// CORS 响应头: 允许的方法。
        /// </summary>
        private const string ACCESS_CONTROL_ALLOW_METHODS = "Access-Control-Allow-Methods";

        /// <summary>
        /// CORS 响应头: 允许的源。
        /// </summary>
        private const string ACCESS_CONTROL_ALLOW_ORIGIN = "Access-Control-Allow-Origin";

        /// <summary>
        /// CORS 响应头: 预检缓存时长。
        /// </summary>
        private const string ACCESS_CONTROL_MAX_AGE = "Access-Control-Max-Age";

        /// <summary>
        /// 响应头: 帧选项。
        /// </summary>
        private const string X_FRAME_OPTIONS = "X-Frame-Options";

        /// <summary>
        /// 响应头: 服务器标识。
        /// </summary>
        private const string X_POWERED_BY = "X-Powered-By";

        /// <summary>
        /// 是否启用 CORS 策略, 默认为 false, 可由子类重写。
        /// </summary>
        protected virtual bool EnableCORSPolicy
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// 用于保持处理器实例存活的 GC 句柄。
        /// </summary>
        private GCHandle _gcHandle;

        /// <summary>
        /// 当前读取流偏移量。
        /// </summary>
        private int _readStreamOffset;

        /// <summary>
        /// 分片请求起始位置; 未指定时为 null。
        /// </summary>
        private int? _buffStartPostition = null;

        /// <summary>
        /// 分片请求结束位置; 未指定时为 null。
        /// </summary>
        private int? _buffEndPostition = null;

        /// <summary>
        /// 是否为分片(部分)内容请求。
        /// </summary>
        private bool _isPartContent = false;

        /// <summary>
        /// 取消令牌源, 用于取消异步获取响应任务。
        /// </summary>
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// 资源响应; 异步获取完成后赋值。
        /// </summary>
        private ResourceResponse? _resourceResponse;

        /// <summary>
        /// 根据资源请求获取资源响应, 由子类实现。
        /// </summary>
        /// <param name="request">资源请求对象。</param>
        /// <returns>资源响应。</returns>
        abstract protected ResourceResponse GetResourceResponse(ResourceRequest request);

        /// <summary>
        /// 资源方案处理器选项。
        /// </summary>
        protected ResourceSchemeHandlerOptions SchemeOptions { get; }

        /// <summary>
        /// 根据文件扩展名获取 MIME 类型。
        /// </summary>
        /// <param name="fileName">文件名或扩展名。</param>
        /// <returns>MIME 类型; 无扩展名时返回 application/octet-stream。</returns>
        protected static string GetMimeType(string fileName)
        {
            var ext = Path.GetExtension(fileName)?.Trim('.') ?? string.Empty;

            if (string.IsNullOrEmpty(ext))
            {
                return "application/octet-stream";
            }

            return CefRuntime.GetMimeType(ext);
        }

        /// <summary>
        /// 初始化 <see cref="ResourceHandler"/> 实例。
        /// </summary>
        public ResourceHandler()
        {
            var options = new ResourceSchemeHandlerOptions();

            SchemeOptions = options;

            _gcHandle = GCHandle.Alloc(this);
        }

        /// <summary>
        /// 跳过指定字节数(直接标记为已跳过)。
        /// </summary>
        /// <param name="bytesToSkip">需跳过的字节数。</param>
        /// <param name="bytesSkipped">实际跳过的字节数。</param>
        /// <param name="callback">跳过完成回调。</param>
        /// <returns>始终返回 true。</returns>
        protected override bool Skip(long bytesToSkip, out long bytesSkipped, CefResourceSkipCallback callback)
        {
            bytesSkipped = bytesToSkip;
            return true;
        }

        /// <summary>
        /// 填充响应头与响应长度。
        /// </summary>
        /// <param name="response">响应对象。</param>
        /// <param name="responseLength">响应长度。</param>
        /// <param name="redirectUrl">重定向地址(此处为空)。</param>
        protected override void GetResponseHeaders(CefResponse response, out long responseLength, out string redirectUrl)
        {
            var statusCode = _resourceResponse?.HttpStatus ?? StatusCodes.Status400BadRequest;

            if (_resourceResponse != null)
            {
                response.SetHeaderMap(_resourceResponse.Headers);
            }

            response.Status = (int)statusCode;

            redirectUrl = string.Empty;

            if (statusCode == StatusCodes.Status200OK && _resourceResponse != null)
            {
                responseLength = _resourceResponse.Length;

                response.MimeType = _resourceResponse.ContentType ?? string.Empty;

                // 分片请求时设置分片相关响应头
                if (_isPartContent)
                {
                    response.SetHeaderByName("Accept-Ranges", "bytes", true);

                    var startPos = 0;
                    var endPos = _resourceResponse.Length - 1;

                    if (_buffStartPostition.HasValue && _buffEndPostition.HasValue)
                    {
                        startPos = _buffStartPostition.Value;
                        endPos = _buffStartPostition.Value;
                    }
                    else if (!_buffEndPostition.HasValue && _buffStartPostition.HasValue)
                    {
                        startPos = _buffStartPostition.Value;
                    }

                    response.SetHeaderByName("Content-Range", $"bytes {startPos}-{endPos}/{_resourceResponse.Length}", true);
                    response.SetHeaderByName("Content-Length", $"{endPos - startPos + 1}", true);

                    response.Status = 206;
                }

                response.SetHeaderByName("Content-Type", response.MimeType, true);

                response.SetHeaderByName(X_POWERED_BY, $"Robot/{Assembly.GetExecutingAssembly().GetName().Version}", true);
            }
            else
            {
                responseLength = 0;
            }
        }

        /// <summary>
        /// 打开资源请求: 解析请求头与请求体, 异步获取响应。
        /// </summary>
        /// <param name="request">CEF 请求对象。</param>
        /// <param name="handleRequest">是否由本处理器处理请求。</param>
        /// <param name="callback">打开完成回调。</param>
        /// <returns>始终返回 true。</returns>
        protected override bool Open(CefRequest request, out bool handleRequest, CefCallback callback)
        {
            var uri = new Uri(request.Url);
            var headers = request.GetHeaderMap();

            // 解析 range 请求头, 记录分片起止位置
            if (!string.IsNullOrEmpty(headers.Get("range")))
            {
                var rangeString = headers?.Get("range") ?? string.Empty;
                var group = System.Text.RegularExpressions.Regex.Match(rangeString, @"(?<start>\d+)-(?<end>\d*)")?.Groups;
                if (group != null)
                {
                    if (!string.IsNullOrEmpty(group["start"].Value) && int.TryParse(group["start"].Value, out var startPos))
                    {
                        _buffStartPostition = startPos;
                    }

                    if (!string.IsNullOrEmpty(group["end"].Value) && int.TryParse(group["end"].Value, out var endPos))
                    {
                        _buffEndPostition = endPos;
                    }
                }
                _isPartContent = true;
            }

            _readStreamOffset = 0;

            if (_buffStartPostition.HasValue)
            {
                _readStreamOffset = _buffStartPostition.Value;
            }

            // 解析请求体: 区分字节数据与上传文件
            var postData = new List<byte>();
            var uploadFiles = new List<string>();

            if (request.PostData != null)
            {
                var items = request.PostData.GetElements();

                if (items != null && items.Length > 0)
                {
                    foreach (var item in items)
                    {
                        var buffer = item.GetBytes();

                        switch (item.ElementType)
                        {
                            case CefPostDataElementType.Bytes:
                                postData.AddRange(buffer);
                                break;
                            case CefPostDataElementType.File:
                                uploadFiles.Add(item.GetFile());
                                break;
                        }
                    }
                }
            }

            var method = request.Method;

            var resourceRequest = new ResourceRequest(uri, method, headers, postData.ToArray(), uploadFiles.ToArray(), request);

            handleRequest = false;

            // 异步获取资源响应, 完成后回调
            Task.Run(() =>
            {
                try
                {
                    _resourceResponse = GetResourceResponse(resourceRequest);

                    // 启用 CORS 策略时补充跨域响应头
                    if (EnableCORSPolicy)
                    {
                        _resourceResponse.Headers.Set(ACCESS_CONTROL_ALLOW_HEADERS, "*");
                        _resourceResponse.Headers.Set(ACCESS_CONTROL_ALLOW_METHODS, "*");
                        _resourceResponse.Headers.Set(X_FRAME_OPTIONS, "ALLOWALL");

                        _resourceResponse.Headers.Set(ACCESS_CONTROL_MAX_AGE, "3600");

                        if (!string.IsNullOrEmpty(request.GetHeaderByName("origin")))
                        {
                            _resourceResponse.Headers.Set(ACCESS_CONTROL_ALLOW_ORIGIN, request.GetHeaderByName("origin"));
                        }
                        else
                        {
                            _resourceResponse.Headers.Set(ACCESS_CONTROL_ALLOW_ORIGIN, "*");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);

                    callback.Cancel();
                }
            }, _cancellationTokenSource.Token).ContinueWith(t =>
            {
                callback.Continue();
            });

            return true;
        }

        /// <summary>
        /// 取消资源请求。
        /// </summary>
        protected override void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// 从资源响应中读取数据写入响应流。
        /// </summary>
        /// <param name="response">响应流。</param>
        /// <param name="bytesToRead">需读取的字节数。</param>
        /// <param name="bytesRead">实际读取的字节数。</param>
        /// <param name="callback">读取完成回调。</param>
        /// <returns>读取成功返回 true; 无数据可读时返回 false。</returns>
        protected override bool Read(Stream response, int bytesToRead, out int bytesRead, CefResourceReadCallback callback)
        {
            if (_resourceResponse?.ContentBody == null)
            {
                bytesRead = 0;
                return false;
            }

            var total = _resourceResponse.Length;

            var bytesToCopy = (int)(total - _readStreamOffset);

            if (total == 0 || bytesToCopy <= 0)
            {
                bytesRead = 0;
                return false;
            }

            bytesToCopy = Math.Min(bytesToCopy, bytesToRead);

            _resourceResponse.ContentBody.Position = _readStreamOffset;

            var buff = new byte[bytesToCopy];
            var read = _resourceResponse.ContentBody.Read(buff, 0, bytesToCopy);

            if (read <= 0)
            {
                bytesRead = 0;
                return false;
            }

            response.Write(buff, 0, read);

            _readStreamOffset += read;
            bytesRead = read;

            // 读取完成后释放响应与 GC 句柄
            if (_readStreamOffset == _resourceResponse.Length)
            {
                _resourceResponse.Dispose();
                _gcHandle.Free();
            }

            return true;
        }
    }
}
