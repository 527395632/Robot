// Robot 桌面软件 — 消息桥命名管道服务端
// 监听命名管道连接,接收桥接请求并返回响应

using Microsoft.Extensions.Logging;
using System;
using System.IO.Pipes;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Robot.Browser
{

    /// <summary>
    /// 消息桥命名管道服务端:监听命名管道连接,接收桥接请求并返回响应。
    /// </summary>
    internal class MessageBridgePipeServer : IDisposable
    {
        /// <summary>
        /// 取消令牌源(用于停止监听循环)。
        /// </summary>
        private CancellationTokenSource? _cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// 关联的消息桥。
        /// </summary>
        public MessageBridge Bridge { get; }

        /// <summary>
        /// 初始化 <see cref="MessageBridgePipeServer"/> 实例并启动监听循环。
        /// </summary>
        /// <param name="bridge">关联的消息桥。</param>
        /// <param name="pipeName">命名管道名称。</param>
        public MessageBridgePipeServer(MessageBridge bridge, string pipeName)
        {
            Bridge = bridge;

            Task.Run(async () =>
            {
                const int MaxErrorsAllowed = 5;

                var errorCount = 0;

                try
                {
                    while (!_cancellationTokenSource.IsCancellationRequested)
                    {
                        try
                        {
                            using var server = new NamedPipeServerStream(pipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

                            await server.WaitForConnectionAsync(_cancellationTokenSource.Token);
                            AcceptClient(server);
                        }
                        catch (OperationCanceledException) { }
                        catch (Exception ex)
                        {
                            errorCount++;

                            if (errorCount > MaxErrorsAllowed)
                            {
                                break;
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                    var cancellationTokenSource = _cancellationTokenSource;
                    _cancellationTokenSource = null;
                    cancellationTokenSource.Dispose();
                }
            });
        }

        /// <summary>
        /// 接受客户端连接:读取请求、分发处理并写回响应。
        /// </summary>
        /// <param name="server">命名管道服务端流。</param>
        private void AcceptClient(NamedPipeServerStream server)
        {
            MessageBridgeResponse? response;

            using var stream = new MessageBridgePipeStream(server);

            try
            {
                var message = stream.ReadMessage();

                var request = JsonSerializer.Deserialize<MessageBridgeRequest>(message);

                if (request == null) throw new NullReferenceException($"{nameof(request)}");

                response = Bridge.OnMessageBridgeRequestReviced(request);
            }
            catch (Exception ex)
            {
                response = new MessageBridgeResponse
                {
                    IsSuccess = false,
                    Exception = ex.Message
                };
            }

            if (response == null)
            {
                response = new MessageBridgeResponse
                {
                    IsSuccess = false,
                    Exception = "Can't found handler for this request."
                };
            }

            try
            {
                stream.WriteMessage(response.ToJson());

                server.Flush();

                server.WaitForPipeDrain();
            }
            catch (Exception ex)
            {
            }
            finally
            {
                server.Disconnect();
                server.Dispose();
            }
        }

        /// <summary>
        /// 释放资源(停止监听循环)。
        /// </summary>
        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}
