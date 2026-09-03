// Robot 桌面软件 — 资源响应
// 封装一次资源响应的状态码、内容类型、响应头与响应体, 并提供 JSON/文本内容便捷构造

using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Robot.WebResource
{

    /// <summary>
    /// 资源响应:封装一次资源响应的状态码、内容类型、响应头与响应体, 并提供 JSON/文本内容便捷构造。
    /// </summary>
    public sealed class ResourceResponse : IDisposable
    {
        /// <summary>
        /// 响应体内容流。
        /// </summary>
        public Stream? ContentBody { get; set; }

        /// <summary>
        /// HTTP 状态码, 默认为 200。
        /// </summary>
        public int HttpStatus { get; set; } = StatusCodes.Status200OK;

        /// <summary>
        /// 响应体长度; 无内容流时为 0。
        /// </summary>
        public long Length => ContentBody?.Length ?? 0;

        /// <summary>
        /// 内容类型, 默认为 text/plain。
        /// </summary>
        public string? ContentType { get; set; } = "text/plain";

        /// <summary>
        /// 响应头集合。
        /// </summary>
        public NameValueCollection Headers { get; } = new NameValueCollection();

        /// <summary>
        /// 初始化 <see cref="ResourceResponse"/> 实例。
        /// </summary>
        /// <param name="buff">响应体原始字节; 为空时不设置内容流。</param>
        /// <param name="contentType">内容类型; 为空时保持默认 text/plain。</param>
        public ResourceResponse(byte[]? buff = null, string? contentType = null)
        {
            if (!string.IsNullOrEmpty(contentType))
            {
                ContentType = contentType;
            }

            if (buff != null)
            {
                ContentBody = new MemoryStream(buff);
            }

            HttpStatus = StatusCodes.Status200OK;
        }

        /// <summary>
        /// 释放响应体内容流。
        /// </summary>
        public void Dispose()
        {
            ContentBody?.Close();
            ContentBody?.Dispose();
            ContentBody = null;

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 设置字节内容并更新内容类型与状态码。
        /// </summary>
        /// <param name="buff">响应体原始字节。</param>
        /// <param name="contentType">内容类型; 为空时保持当前值。</param>
        internal void Content(byte[] buff, string? contentType = null)
        {
            if (!string.IsNullOrEmpty(contentType))
            {
                ContentType = contentType;
            }

            Headers.Set("Content-Type", ContentType);

            if (ContentBody != null)
            {
                ContentBody.Dispose();
                ContentBody = null;
            }

            ContentBody = new MemoryStream(buff);

            HttpStatus = StatusCodes.Status200OK;
        }

        /// <summary>
        /// 将对象序列化为 JSON 并设置为响应内容。
        /// </summary>
        /// <param name="data">待序列化的对象。</param>
        /// <param name="jsonSerializerOptions">JSON 序列化选项; 为空时使用默认选项。</param>
        internal void JsonContent(object data, JsonSerializerOptions? jsonSerializerOptions = null)
        {
            var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data, jsonSerializerOptions));

            Content(bytes, "application/json");
        }

        /// <summary>
        /// 将指定类型的对象序列化为 JSON 并设置为响应内容。
        /// </summary>
        /// <typeparam name="T">待序列化对象类型。</typeparam>
        /// <param name="data">待序列化的对象。</param>
        /// <param name="jsonSerializerOptions">JSON 序列化选项; 为空时使用默认选项。</param>
        internal void JsonContent<T>(T data, JsonSerializerOptions? jsonSerializerOptions = null)
        {
            var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data, jsonSerializerOptions));

            Content(bytes, "application/json");
        }

        /// <summary>
        /// 将文本设置为响应内容, 使用 UTF-8 编码。
        /// </summary>
        /// <param name="text">响应文本。</param>
        internal void TextContent(string text)
        {
            TextContent(text, Encoding.UTF8);
        }

        /// <summary>
        /// 将文本设置为响应内容, 使用指定编码。
        /// </summary>
        /// <param name="text">响应文本。</param>
        /// <param name="encoding">文本编码。</param>
        internal void TextContent(string text, Encoding encoding)
        {
            Content(text, "text/plain", encoding);
        }

        /// <summary>
        /// 将字符串设置为响应内容, 使用 UTF-8 编码与 text/plain 类型。
        /// </summary>
        /// <param name="content">响应文本。</param>
        internal void Content(string content)
        {
            Content(Encoding.UTF8.GetBytes(content), "text/plain");
        }

        /// <summary>
        /// 将字符串设置为响应内容, 使用 UTF-8 编码与指定内容类型。
        /// </summary>
        /// <param name="content">响应文本。</param>
        /// <param name="contentType">内容类型。</param>
        internal void Content(string content, string contentType)
        {
            Content(Encoding.UTF8.GetBytes(content), contentType);
        }

        /// <summary>
        /// 将字符串设置为响应内容, 使用指定编码与指定内容类型。
        /// </summary>
        /// <param name="content">响应文本。</param>
        /// <param name="contentType">内容类型。</param>
        /// <param name="encoding">文本编码。</param>
        internal void Content(string content, string contentType, Encoding encoding)
        {
            Content(encoding.GetBytes(content), contentType);
        }
    }
}
