// Robot 桌面软件 — 消息桥命名管道客户端
// 通过命名管道向服务端发送桥接请求并读取响应

using System;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace Robot.Browser
{

    /// <summary>
    /// 消息桥命名管道客户端:通过命名管道向服务端发送桥接请求并读取响应。
    /// </summary>
    internal class MessageBridgePipeClient
    {
        /// <summary>
        /// 命名管道名称。
        /// </summary>
        public string PipeName { get; }

        /// <summary>
        /// 初始化 <see cref="MessageBridgePipeClient"/> 实例。
        /// </summary>
        /// <param name="pipeName">命名管道名称。</param>
        public MessageBridgePipeClient(string pipeName)
        {
            PipeName = pipeName;
        }

        /// <summary>
        /// 异步发送请求并读取响应。
        /// </summary>
        /// <param name="request">桥接请求。</param>
        /// <returns>桥接响应(失败时携带异常信息)。</returns>
        public Task<MessageBridgeResponse> RequestAsync(MessageBridgeRequest request)
        {
            return Task.Run(() =>
            {
                var client = new NamedPipeClientStream(PipeName);

                try
                {
                    client.Connect();

                    client.ReadMode = PipeTransmissionMode.Byte;

                    var stream = new MessageBridgePipeStream(client);

                    stream.WriteMessage(request.ToJson());

                    client.Flush();

                    client.WaitForPipeDrain();

                    var message = stream.ReadMessage();

                    return MessageBridgeResponse.FromJson(message);
                }
                catch (Exception ex)
                {
                    return new MessageBridgeResponse
                    {
                        IsSuccess = false,
                        Exception = ex.Message
                    };
                }
                finally
                {
                    client.Close();
                    client.Dispose();
                }
            });
        }
    }
}
