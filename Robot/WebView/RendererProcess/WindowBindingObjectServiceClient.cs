// Robot 桌面软件 — 窗口绑定对象服务客户端
// 通过命名管道向浏览器进程请求窗口绑定对象数据

using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Threading.Tasks;

namespace Robot.Browser
{

    /// <summary>
    /// 窗口绑定对象服务客户端:通过命名管道向浏览器进程请求窗口绑定对象数据。
    /// </summary>
    internal class WindowBindingObjectServiceClient
    {
        /// <summary>
        /// 管道名称。
        /// </summary>
        public string PipeName { get; }

        /// <summary>
        /// 初始化 <see cref="WindowBindingObjectServiceClient"/> 实例。
        /// </summary>
        /// <param name="pipeName">管道名称。</param>
        public WindowBindingObjectServiceClient(string pipeName)
        {
            PipeName = pipeName;
        }

        /// <summary>
        /// 同步请求:连接管道、发送请求并读取响应。
        /// </summary>
        /// <param name="request">请求内容。</param>
        /// <returns>响应内容;失败时返回 null。</returns>
        public string? Request(string request)
        {
            var client = new NamedPipeClientStream(PipeName);

            //MessageBox.Show($"CLIENT: {PipeName}");

            try
            {
                // Connect 默认无限超时:管道名不存在时无限等待而非抛异常,renderer 主线程会被永久冻结
                // 限制 2 秒,IPC 故障时快速降级返回 null,不阻塞导航
                // 注意:勿设 ReadTimeout —— Windows 命名管道 CanTimeout=false,设置即抛 InvalidOperationException
                client.Connect(2000);

                client.ReadMode = PipeTransmissionMode.Byte;

                var stream = new MessageBridgePipeStream(client);

                if (!client.CanWrite) return null;

                stream.WriteMessage(request);

                client.Flush();

                client.WaitForPipeDrain();

                var message = stream.ReadMessage();

                return message;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                Debug.WriteLine(ex);
                return null;
            }
            finally
            {
                client.Close();
                client.Dispose();
            }
        }

        /// <summary>
        /// 异步请求:在后台线程执行同步请求。
        /// </summary>
        /// <param name="request">请求内容。</param>
        /// <returns>响应内容;失败时返回 null。</returns>
        public Task<string?> RequestAsync(string request)
        {
            return Task.Run(() =>
            {
                return Request(request);
            });
        }
    }
}
