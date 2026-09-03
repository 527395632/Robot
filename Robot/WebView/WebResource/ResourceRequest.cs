// Robot 桌面软件 — 资源请求
// 封装一次资源请求的 URI、请求头、请求体, 并提供查询串/表单/JSON 解析与反序列化

using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Xilium.CefGlue;

namespace Robot.WebResource
{

    /// <summary>
    /// 资源请求:封装一次资源请求的 URI、请求头、请求体, 并提供查询串/表单/JSON 解析与反序列化。
    /// </summary>
    public sealed class ResourceRequest
    {
        /// <summary>
        /// 请求 URI。
        /// </summary>
        public Uri Uri { get; }

        /// <summary>
        /// 请求地址(不含查询串)。
        /// </summary>
        public string RequestUrl
        {
            get
            {
                var original = Uri.OriginalString;
                if (original.Contains('?'))
                {
                    return original.Substring(0, original.IndexOf("?"));
                }

                return original;
            }
        }

        /// <summary>
        /// 请求头集合。
        /// </summary>
        public NameValueCollection? Headers { get; }

        /// <summary>
        /// 上传文件路径集合。
        /// </summary>
        public string[] UploadFiles { get; }

        /// <summary>
        /// 请求体原始字节。
        /// </summary>
        public byte[]? RawData { get; }

        /// <summary>
        /// 内容类型: 表单(表单 URL 编码)。
        /// </summary>
        private const string CONTENT_TYPE_FORM_URL_ENCODED = "application/x-www-form-urlencoded";

        /// <summary>
        /// 内容类型: JSON。
        /// </summary>
        private const string CONTENT_TYPE_APPLICATION_JSON = "application/json";

        /// <summary>
        /// 内容类型: 多部分表单。
        /// </summary>
        private const string CONTENT_TYPE_MULTIPART_FORM_DATA = "multipart/form-data";

        /// <summary>
        /// 原始请求方法字符串。
        /// </summary>
        private readonly string _method;

        /// <summary>
        /// 原始 CEF 请求对象。
        /// </summary>
        public CefRequest RawRequest { get; }

        /// <summary>
        /// 相对路径(去除前导斜杠)。
        /// </summary>
        public string RelativePath => $"{Uri?.LocalPath ?? string.Empty}".TrimStart('/');

        /// <summary>
        /// 文件名。
        /// </summary>
        public string FileName => Path.GetFileName(RelativePath);

        /// <summary>
        /// 文件扩展名(不含点)。
        /// </summary>
        public string FileExtension => Path.GetExtension(FileName).TrimStart('.');

        /// <summary>
        /// 是否包含文件名。
        /// </summary>
        public bool HasFileName => !string.IsNullOrEmpty(FileName);

        /// <summary>
        /// 查询串键值集合。
        /// </summary>
        public NameValueCollection? QueryString { get; } = null;

        /// <summary>
        /// 表单数据键值集合。
        /// </summary>
        public NameValueCollection? FormData { get; } = null;

        /// <summary>
        /// JSON 请求体字符串。
        /// </summary>
        public string? JsonData { get; } = null;

        /// <summary>
        /// 是否为 JSON 请求。
        /// </summary>
        public bool IsJson
        {
            get
            {
                if (string.IsNullOrEmpty(ContentType))
                {
                    return false;
                }

                return ContentType.Contains(CONTENT_TYPE_APPLICATION_JSON);
            }
        }

        /// <summary>
        /// 请求体编码: 从内容类型中解析 charset, 未指定时使用 UTF-8。
        /// </summary>
        public Encoding ContentEncoding
        {
            get
            {
                var encoding = ContentType;

                if (string.IsNullOrEmpty(encoding) || !encoding.Contains("charset="))
                {
                    encoding = "utf-8";
                }
                else
                {
                    // 匹配 "charset=xxx"
                    var match = Regex.Match(encoding, @"(?<=charset=)(([^;,\r\n]))*");

                    if (match.Success)
                    {
                        encoding = match.Value;
                    }
                }

                return Encoding.GetEncoding(encoding);
            }
        }

        /// <summary>
        /// 请求体字符串内容。
        /// </summary>
        public string StringContent
        {
            get
            {
                if (RawData == null)
                {
                    return string.Empty;
                }

                return ContentEncoding.GetString(RawData);
            }
        }

        /// <summary>
        /// JSON 序列化选项: 属性名大小写不敏感、驼峰命名、忽略循环引用、宽松 JSON 转义。
        /// </summary>
        private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        /// <summary>
        /// 从 JSON 请求体反序列化为指定类型。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <returns>反序列化结果; 非 JSON 请求或解析失败时返回默认值。</returns>
        public T? DeserializeObjectFromJson<T>()
        {
            if (IsJson && RawData != null)
            {
                try
                {
                    return JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(RawData), _jsonSerializerOptions);
                }
                catch
                {
                    return default;
                }
            }

            return default;
        }

        /// <summary>
        /// 请求方法; 无法解析时返回 All。
        /// </summary>
        public ResourceRequestMethod Method
        {
            get
            {
                if (Enum.TryParse(_method, out ResourceRequestMethod value))
                {
                    return value;
                }

                return ResourceRequestMethod.All;
            }
        }

        /// <summary>
        /// 内容类型(取自请求头 Content-Type)。
        /// </summary>
        public string ContentType => Headers?.Get("Content-Type") ?? string.Empty;

        /// <summary>
        /// 初始化 <see cref="ResourceRequest"/> 实例。
        /// </summary>
        /// <param name="uri">请求 URI。</param>
        /// <param name="method">请求方法字符串。</param>
        /// <param name="headers">请求头集合。</param>
        /// <param name="postData">请求体原始字节。</param>
        /// <param name="uploadFiles">上传文件路径集合。</param>
        /// <param name="cefRequest">原始 CEF 请求对象。</param>
        internal ResourceRequest(Uri uri, string method, NameValueCollection? headers, byte[] postData, string[] uploadFiles, CefRequest cefRequest)
        {
            Uri = uri;
            _method = method;
            Headers = headers;
            RawData = postData;
            UploadFiles = uploadFiles;
            RawRequest = cefRequest;
            QueryString = ProcessQueryString(uri.Query);

            // 根据内容类型解析请求体: 表单 URL 编码 / 多部分表单 / 其他
            if (ContentType != null && ContentType.Contains(CONTENT_TYPE_FORM_URL_ENCODED) && RawData != null)
            {
                FormData = ProcessFormUrlEncodedData(RawData);
            }
            else if (ContentType != null && ContentType.Contains(CONTENT_TYPE_MULTIPART_FORM_DATA) && RawData != null)
            {
                FormData = ProcessFormData(RawData);
            }
            else
            {
                FormData = new NameValueCollection();
            }

            if (IsJson)
            {
                try
                {
                    JsonData = JsonSerializer.Serialize(Encoding.UTF8.GetString(RawData));
                }
                catch
                {
                    JsonData = null;
                }
            }
        }

        /// <summary>
        /// 解析多部分表单请求体为键值集合。
        /// </summary>
        /// <param name="bytes">请求体原始字节。</param>
        /// <returns>表单字段键值集合。</returns>
        private NameValueCollection ProcessFormData(byte[] bytes)
        {
            var formData = ContentEncoding.GetString(bytes);

            var fields = new NameValueCollection();

            var boundary = GetBoundary(formData);

            formData = formData.Replace($"{boundary}--", null);

            var boundaryParts = formData.Split(new[] { boundary }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in boundaryParts)
            {
                var lines = part.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                string? fieldName = null;
                string? fieldValue = null;

                foreach (var line in lines)
                {
                    if (line.StartsWith("Content-Disposition: form-data;"))
                    {
                        var dispositionParts = line.Split(new[] { "; " }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var partInfo in dispositionParts)
                        {
                            if (partInfo.StartsWith("name="))
                            {
                                fieldName = partInfo.Substring(5).Trim('"');
                            }
                        }
                    }
                    else if (line.StartsWith("Content-Type:"))
                    {
                        // 如需处理文件内容类型可在此扩展
                    }
                    else
                    {
                        fieldValue = line;
                    }
                }

                if (!string.IsNullOrEmpty(fieldName))
                {
                    fields.Add(fieldName, fieldValue);
                }
            }

            return fields;
        }

        /// <summary>
        /// 获取多部分表单的分隔符。
        /// </summary>
        /// <param name="formData">表单数据字符串。</param>
        /// <returns>分隔符。</returns>
        private string GetBoundary(string formData)
        {
            var endIndex = formData.IndexOf("\r\n", StringComparison.OrdinalIgnoreCase);

            return formData[..endIndex];
        }

        /// <summary>
        /// 解析表单 URL 编码请求体为键值集合。
        /// </summary>
        /// <param name="rawData">请求体原始字节。</param>
        /// <returns>表单字段键值集合。</returns>
        private NameValueCollection ProcessFormUrlEncodedData(byte[] rawData)
        {
            var query = ContentEncoding.GetString(rawData);

            var retval = new NameValueCollection();

            query = query.Trim('?');

            foreach (var pair in query.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var keyvalue = pair.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (keyvalue.Length == 2)
                {
                    retval.Add(keyvalue[0].ToLower(), Uri.UnescapeDataString(keyvalue[1]));
                }
                else if (keyvalue.Length == 1)
                {
                    retval.Add(keyvalue[0].ToLower(), null);
                }
            }

            return retval;
        }

        /// <summary>
        /// 解析查询串为键值集合。
        /// </summary>
        /// <param name="query">查询串。</param>
        /// <returns>查询字段键值集合。</returns>
        private NameValueCollection ProcessQueryString(string query)
        {
            var retval = new NameValueCollection();

            query = query.Trim('?');
            foreach (var pair in query.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var keyvalue = pair.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (keyvalue.Length == 2)
                {
                    retval.Add(keyvalue[0].ToLower(), Uri.UnescapeDataString(keyvalue[1]));
                }
                else if (keyvalue.Length == 1)
                {
                    retval.Add(keyvalue[0].ToLower(), null);
                }
            }

            return retval;
        }
    }
}
