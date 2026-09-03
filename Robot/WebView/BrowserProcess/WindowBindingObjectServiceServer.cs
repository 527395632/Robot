// Robot 桌面软件 — 窗口绑定对象服务端
// 通过命名管道向渲染进程提供窗口绑定对象类型信息

using Robot.JavaScript;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Robot.Browser
{

    /// <summary>
    /// 窗口绑定对象服务端:通过命名管道向渲染进程提供窗口绑定对象类型信息。
    /// </summary>
    internal class WindowBindingObjectServiceServer : IDisposable
    {
        /// <summary>
        /// 取消令牌源(用于停止监听循环)。
        /// </summary>
        private CancellationTokenSource? _cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// 取消令牌源是否已释放。
        /// </summary>
        private bool _isTokenSourceDisposed = false;

        /// <summary>
        /// 初始化 <see cref="WindowBindingObjectServiceServer"/> 实例并启动监听循环。
        /// </summary>
        /// <param name="pipeName">命名管道名称。</param>
        public WindowBindingObjectServiceServer(string pipeName)
        {
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
                            Debug.WriteLine(ex);

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
                    _isTokenSourceDisposed = true;
                }
            });
        }

        /// <summary>
        /// 接受客户端连接:读取请求命令并写回响应。
        /// </summary>
        /// <param name="server">命名管道服务端流。</param>
        private void AcceptClient(NamedPipeServerStream server)
        {
            using var stream = new MessageBridgePipeStream(server);

            string response = string.Empty;

            try
            {
                var message = stream.ReadMessage();

                switch (message)
                {
                    case "GetWindowBindingObjects":
                        response = GetWindowBindingObjects();
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            try
            {
                stream.WriteMessage(response);

                server.Flush();

                server.WaitForPipeDrain();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                server.Disconnect();
                server.Dispose();
            }
        }

        /// <summary>
        /// 获取窗口绑定对象类型描述集合的 JSON。
        /// </summary>
        /// <returns>描述集合的 JSON 字符串。</returns>
        private string GetWindowBindingObjects()
        {
            var objectTypes = JavaScriptWindowBindingObjectBridge.WindowBindingObjectTypes;

            var objects = new List<JavaScriptWindowBindingObjectDescriper>();

            foreach (var type in objectTypes)
            {
                var fileInfo = new FileInfo(new Uri(type.Assembly.Location).LocalPath);
                var filePath = fileInfo.FullName;
                var typeName = type.FullName;

                if (typeName == null) continue;

                var describer = new JavaScriptWindowBindingObjectDescriper(filePath, typeName);

                objects.Add(describer);
            }

            return JsonSerializer.Serialize(objects);
        }

        /// <summary>
        /// 释放资源。
        /// </summary>
        public void Dispose()
        {
            if (!_isTokenSourceDisposed)
            {
                //_cancellationTokenSource?.Cancel();
            }
        }
    }
}
