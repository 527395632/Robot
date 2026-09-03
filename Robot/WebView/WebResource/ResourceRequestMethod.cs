// Robot 桌面软件 — 资源请求方法
// 枚举 HTTP 请求方法

using System.ComponentModel;

namespace Robot.WebResource
{

    /// <summary>
    /// 资源请求方法:枚举 HTTP 请求方法。
    /// </summary>
    public enum ResourceRequestMethod
    {
        /// <summary>
        /// 未指定请求方法。
        /// </summary>
        [Description("未指定请求方法")]
        All,

        /// <summary>
        /// GET 请求方法。
        /// </summary>
        [Description("GET 请求方法")]
        GET,

        /// <summary>
        /// POST 请求方法。
        /// </summary>
        [Description("POST 请求方法")]
        POST,

        /// <summary>
        /// PUT 请求方法。
        /// </summary>
        [Description("PUT 请求方法")]
        PUT,

        /// <summary>
        /// DELETE 请求方法。
        /// </summary>
        [Description("DELETE 请求方法")]
        DELETE,

        /// <summary>
        /// HEAD 请求方法。
        /// </summary>
        [Description("HEAD 请求方法")]
        HEAD,

        /// <summary>
        /// OPTIONS 请求方法。
        /// </summary>
        [Description("OPTIONS 请求方法")]
        OPTIONS,

        /// <summary>
        /// PATCH 请求方法。
        /// </summary>
        [Description("PATCH 请求方法")]
        PATCH
    }
}
