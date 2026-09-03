// Robot 桌面软件 — 消息桥请求
// 承载跨进程桥接请求的元数据,支持 JSON 序列化

using System.Text.Json;

namespace Robot.Browser
{

    /// <summary>
    /// 消息桥请求:承载跨进程桥接请求的元数据,支持 JSON 序列化。
    /// </summary>
    internal sealed class MessageBridgeRequest
    {
        /// <summary>
        /// 请求名称。
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// 是否为远端请求。
        /// </summary>
        public required bool IsRemote { get; set; } = false;

        /// <summary>
        /// 浏览器标识。
        /// </summary>
        public required int BrowserId { get; set; }

        /// <summary>
        /// 帧标识。
        /// </summary>
        public required long FrameId { get; set; }

        /// <summary>
        /// 请求负载。
        /// </summary>
        public string? Payload { get; set; }

        /// <summary>
        /// 将请求序列化为 JSON 字符串。
        /// </summary>
        /// <returns>JSON 字符串。</returns>
        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }

        /// <summary>
        /// 从 JSON 字符串反序列化请求。
        /// </summary>
        /// <param name="json">JSON 字符串。</param>
        /// <returns>反序列化后的请求。</returns>
        public static MessageBridgeRequest FromJson(string json)
        {
            return JsonSerializer.Deserialize<MessageBridgeRequest>(json)!;
        }
    }
}
