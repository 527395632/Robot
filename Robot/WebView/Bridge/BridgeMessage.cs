// Robot 桌面软件 — 进程间桥接消息
// 承载跨进程(浏览器进程/渲染进程)传递的消息,支持 JSON 序列化与数据反序列化

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Robot.Browser
{

    /// <summary>
    /// 进程间桥接消息:承载跨进程传递的消息,支持 JSON 序列化与数据反序列化。
    /// </summary>
    internal sealed class BridgeMessage
    {
        /// <summary>
        /// 消息名称。
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// 消息数据(不参与 JSON 序列化)。
        /// </summary>
        [JsonIgnore]
        public object? Data { get; private set; }

        /// <summary>
        /// 消息数据的 JSON 字符串。
        /// </summary>
        public string? Json { get; set; }

        /// <summary>
        /// 初始化空的 <see cref="BridgeMessage"/> 实例。
        /// </summary>
        public BridgeMessage()
        { }

        /// <summary>
        /// 初始化 <see cref="BridgeMessage"/> 实例。
        /// </summary>
        /// <param name="name">消息名称。</param>
        /// <param name="data">消息数据。</param>
        [SetsRequiredMembers]
        public BridgeMessage(string name, object? data = null)
        {
            Name = name;
            Data = data;
            Json = JsonSerializer.Serialize(data);
        }

        /// <summary>
        /// 从 JSON 字符串反序列化消息。
        /// </summary>
        /// <param name="json">JSON 字符串。</param>
        /// <returns>反序列化后的消息,失败为 null。</returns>
        public static BridgeMessage? FromJson(string json)
        {
            var bridgeMessage = JsonSerializer.Deserialize<BridgeMessage>(json);

            return bridgeMessage;
        }

        /// <summary>
        /// 将消息数据反序列化为指定类型。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <returns>反序列化后的数据,无数据时为默认值。</returns>
        public T? DeserializeData<T>()
        {
            if (Json == null || string.IsNullOrEmpty(Json)) return default;

            var obj = JsonSerializer.Deserialize<T>(Json);

            Data = obj;

            return obj;
        }

        /// <summary>
        /// 设置消息数据并同步更新 JSON。
        /// </summary>
        /// <param name="obj">消息数据。</param>
        public void SetData(object obj)
        {
            Data = obj;

            Json = JsonSerializer.Serialize(Data);
        }

        /// <summary>
        /// 将消息序列化为 JSON 字符串。
        /// </summary>
        /// <returns>JSON 字符串。</returns>
        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
