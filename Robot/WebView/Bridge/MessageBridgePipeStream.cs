// Robot 桌面软件 — 消息桥管道流
// 在管道流上以「长度前缀 + 内容」格式读写消息

using System;
using System.IO;
using System.Text;

namespace Robot.Browser
{

    /// <summary>
    /// 消息桥管道流:在管道流上以「长度前缀 + 内容」格式读写消息。
    /// </summary>
    internal class MessageBridgePipeStream : IDisposable
    {
        /// <summary>
        /// 底层流。
        /// </summary>
        private readonly Stream _stream;

        /// <summary>
        /// 流编码(Unicode)。
        /// </summary>
        private readonly UnicodeEncoding _streamEncoding;

        /// <summary>
        /// 初始化 <see cref="MessageBridgePipeStream"/> 实例。
        /// </summary>
        /// <param name="stream">底层流。</param>
        public MessageBridgePipeStream(Stream stream)
        {
            _stream = stream;
            _streamEncoding = new UnicodeEncoding();
        }

        /// <summary>
        /// 写入消息(先写长度前缀,再写内容)。
        /// </summary>
        /// <param name="message">消息内容。</param>
        public void WriteMessage(string message)
        {
            using var writer = new BinaryWriter(_stream, _streamEncoding, true);
            var messageBytes = _streamEncoding.GetBytes(message);
            var length = Convert.ToInt32(messageBytes.Length);
            writer.Write(length);
            writer.Write(messageBytes);
        }

        /// <summary>
        /// 读取消息(先读长度前缀,再读内容)。
        /// </summary>
        /// <returns>消息内容,流不可读时为空字符串。</returns>
        public string ReadMessage()
        {
            if (!_stream.CanRead) return string.Empty;

            using var reader = new BinaryReader(_stream, _streamEncoding, true);
            var length = reader.ReadInt32();
            var message = reader.ReadBytes(length);
            return _streamEncoding.GetString(message);
        }

        /// <summary>
        /// 释放资源。
        /// </summary>
        public void Dispose()
        {
            _stream?.Dispose();
        }
    }
}
