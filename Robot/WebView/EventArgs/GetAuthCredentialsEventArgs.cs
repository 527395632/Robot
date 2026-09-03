// Robot 桌面软件 — 获取认证凭据事件参数
// 对应 CEF OnAuthRequired 回调,携带认证信息,可通过回调返回凭据

using System;
using Xilium.CefGlue;

namespace Robot
{

    /// <summary>
    /// 获取认证凭据事件参数(对应 CEF OnAuthRequired 回调)。
    /// 携带认证信息,可通过 <see cref="Callback"/> 返回凭据。
    /// </summary>
    public class GetAuthCredentialsEventArgs : EventArgs
    {
        /// <summary>
        /// 初始化 <see cref="GetAuthCredentialsEventArgs"/> 实例。
        /// </summary>
        public GetAuthCredentialsEventArgs(CefBrowser browser, string originUrl, bool isProxy, string host, int port, string realm, string scheme, CefAuthCallback callback)
        {
            Browser = browser;
            OriginUrl = originUrl;
            IsProxy = isProxy;
            Host = host;
            Port = port;
            Realm = realm;
            Scheme = scheme;
            Callback = callback;
        }

        /// <summary>
        /// 触发事件的浏览器。
        /// </summary>
        public CefBrowser Browser { get; }
        /// <summary>
        /// 来源 URL。
        /// </summary>
        public string OriginUrl { get; }
        /// <summary>
        /// 是否为代理认证。
        /// </summary>
        public bool IsProxy { get; }
        /// <summary>
        /// 主机。
        /// </summary>
        public string Host { get; }
        /// <summary>
        /// 端口。
        /// </summary>
        public int Port { get; }
        /// <summary>
        /// 认证域。
        /// </summary>
        public string Realm { get; }
        /// <summary>
        /// 认证方案。
        /// </summary>
        public string Scheme { get; }
        /// <summary>
        /// 认证回调(用于返回凭据)。
        /// </summary>
        public CefAuthCallback Callback { get; }

        /// <summary>
        /// 是否已处理。
        /// </summary>
        public bool Handled { get; set; }
    }
}
