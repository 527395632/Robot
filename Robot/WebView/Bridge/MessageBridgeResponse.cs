// Robot 桌面软件 — 消息桥响应
// 承载跨进程桥接请求的响应结果,支持 JSON 序列化

using System.Text.Json;

namespace Robot.Browser
{

    /// <summary>
    /// 消息桥响应:承载跨进程桥接请求的响应结果,支持 JSON 序列化。
    /// </summary>
    internal sealed class MessageBridgeResponse
    {
        /// <summary>
        /// 是否成功。
        /// </summary>
        public bool IsSuccess { get; set; } = true;

        /// <summary>
        /// 异常信息(失败时)。
        /// </summary>
        public string? Exception { get; set; }

        /// <summary>
        /// 响应数据。
        /// </summary>
        public string? Data { get; set; }

        /// <summary>
        /// 将响应序列化为 JSON 字符串。
        /// </summary>
        /// <returns>JSON 字符串。</returns>
        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }

        /// <summary>
        /// 从 JSON 字符串反序列化响应。
        /// </summary>
        /// <param name="json">JSON 字符串。</param>
        /// <returns>反序列化后的响应。</returns>
        public static MessageBridgeResponse FromJson(string json)
        {
            return JsonSerializer.Deserialize<MessageBridgeResponse>(json)!;
        }
    }
}
