// Robot 桌面软件 — HTTP 状态码
// 定义常用 HTTP 响应状态码常量

namespace Robot.WebResource
{

    /// <summary>
    /// HTTP 状态码:定义常用 HTTP 响应状态码常量。
    /// </summary>
    public static class StatusCodes
    {
        /// <summary>
        /// HTTP 状态码 100: 继续。
        /// </summary>
        public const int Status100Continue = 100;

        /// <summary>
        /// HTTP 状态码 101: 切换协议。
        /// </summary>
        public const int Status101SwitchingProtocols = 101;

        /// <summary>
        /// HTTP 状态码 102: 处理中。
        /// </summary>
        public const int Status102Processing = 102;

        /// <summary>
        /// HTTP 状态码 200: 成功。
        /// </summary>
        public const int Status200OK = 200;

        /// <summary>
        /// HTTP 状态码 201: 已创建。
        /// </summary>
        public const int Status201Created = 201;

        /// <summary>
        /// HTTP 状态码 202: 已接受。
        /// </summary>
        public const int Status202Accepted = 202;

        /// <summary>
        /// HTTP 状态码 203: 非权威信息。
        /// </summary>
        public const int Status203NonAuthoritative = 203;

        /// <summary>
        /// HTTP 状态码 204: 无内容。
        /// </summary>
        public const int Status204NoContent = 204;

        /// <summary>
        /// HTTP 状态码 205: 重置内容。
        /// </summary>
        public const int Status205ResetContent = 205;

        /// <summary>
        /// HTTP 状态码 206: 部分内容。
        /// </summary>
        public const int Status206PartialContent = 206;

        /// <summary>
        /// HTTP 状态码 207: 多状态。
        /// </summary>
        public const int Status207MultiStatus = 207;

        /// <summary>
        /// HTTP 状态码 208: 已报告。
        /// </summary>
        public const int Status208AlreadyReported = 208;

        /// <summary>
        /// HTTP 状态码 226: IM 已使用。
        /// </summary>
        public const int Status226IMUsed = 226;

        /// <summary>
        /// HTTP 状态码 300: 多种选择。
        /// </summary>
        public const int Status300MultipleChoices = 300;

        /// <summary>
        /// HTTP 状态码 301: 永久重定向。
        /// </summary>
        public const int Status301MovedPermanently = 301;

        /// <summary>
        /// HTTP 状态码 302: 已找到。
        /// </summary>
        public const int Status302Found = 302;

        /// <summary>
        /// HTTP 状态码 303: 参见其他。
        /// </summary>
        public const int Status303SeeOther = 303;

        /// <summary>
        /// HTTP 状态码 304: 未修改。
        /// </summary>
        public const int Status304NotModified = 304;

        /// <summary>
        /// HTTP 状态码 305: 使用代理。
        /// </summary>
        public const int Status305UseProxy = 305;

        /// <summary>
        /// HTTP 状态码 306: 切换代理(RFC 2616, 已移除)。
        /// </summary>
        public const int Status306SwitchProxy = 306;

        /// <summary>
        /// HTTP 状态码 307: 临时重定向。
        /// </summary>
        public const int Status307TemporaryRedirect = 307;

        /// <summary>
        /// HTTP 状态码 308: 永久重定向。
        /// </summary>
        public const int Status308PermanentRedirect = 308;

        /// <summary>
        /// HTTP 状态码 400: 错误请求。
        /// </summary>
        public const int Status400BadRequest = 400;

        /// <summary>
        /// HTTP 状态码 401: 未授权。
        /// </summary>
        public const int Status401Unauthorized = 401;

        /// <summary>
        /// HTTP 状态码 402: 需要付款。
        /// </summary>
        public const int Status402PaymentRequired = 402;

        /// <summary>
        /// HTTP 状态码 403: 禁止访问。
        /// </summary>
        public const int Status403Forbidden = 403;

        /// <summary>
        /// HTTP 状态码 404: 未找到。
        /// </summary>
        public const int Status404NotFound = 404;

        /// <summary>
        /// HTTP 状态码 405: 方法不允许。
        /// </summary>
        public const int Status405MethodNotAllowed = 405;

        /// <summary>
        /// HTTP 状态码 406: 不可接受。
        /// </summary>
        public const int Status406NotAcceptable = 406;

        /// <summary>
        /// HTTP 状态码 407: 需要代理身份验证。
        /// </summary>
        public const int Status407ProxyAuthenticationRequired = 407;

        /// <summary>
        /// HTTP 状态码 408: 请求超时。
        /// </summary>
        public const int Status408RequestTimeout = 408;

        /// <summary>
        /// HTTP 状态码 409: 冲突。
        /// </summary>
        public const int Status409Conflict = 409;

        /// <summary>
        /// HTTP 状态码 410: 已消失。
        /// </summary>
        public const int Status410Gone = 410;

        /// <summary>
        /// HTTP 状态码 411: 需要长度。
        /// </summary>
        public const int Status411LengthRequired = 411;

        /// <summary>
        /// HTTP 状态码 412: 先决条件失败。
        /// </summary>
        public const int Status412PreconditionFailed = 412;

        /// <summary>
        /// HTTP 状态码 413: 请求实体过大(RFC 2616, 已重命名)。
        /// </summary>
        public const int Status413RequestEntityTooLarge = 413;

        /// <summary>
        /// HTTP 状态码 413: 负载过大(RFC 7231)。
        /// </summary>
        public const int Status413PayloadTooLarge = 413;

        /// <summary>
        /// HTTP 状态码 414: 请求 URI 过长(RFC 2616, 已重命名)。
        /// </summary>
        public const int Status414RequestUriTooLong = 414;

        /// <summary>
        /// HTTP 状态码 414: URI 过长(RFC 7231)。
        /// </summary>
        public const int Status414UriTooLong = 414;

        /// <summary>
        /// HTTP 状态码 415: 不支持的媒体类型。
        /// </summary>
        public const int Status415UnsupportedMediaType = 415;

        /// <summary>
        /// HTTP 状态码 416: 请求范围不可满足(RFC 2616, 已重命名)。
        /// </summary>
        public const int Status416RequestedRangeNotSatisfiable = 416;

        /// <summary>
        /// HTTP 状态码 416: 范围不可满足(RFC 7233)。
        /// </summary>
        public const int Status416RangeNotSatisfiable = 416;

        /// <summary>
        /// HTTP 状态码 417: 期望失败。
        /// </summary>
        public const int Status417ExpectationFailed = 417;

        /// <summary>
        /// HTTP 状态码 418: 我是茶壶。
        /// </summary>
        public const int Status418ImATeapot = 418;

        /// <summary>
        /// HTTP 状态码 419: 身份验证超时(未在任何 RFC 中定义)。
        /// </summary>
        public const int Status419AuthenticationTimeout = 419;

        /// <summary>
        /// HTTP 状态码 421: 请求指向错误。
        /// </summary>
        public const int Status421MisdirectedRequest = 421;

        /// <summary>
        /// HTTP 状态码 422: 无法处理的实体。
        /// </summary>
        public const int Status422UnprocessableEntity = 422;

        /// <summary>
        /// HTTP 状态码 423: 已锁定。
        /// </summary>
        public const int Status423Locked = 423;

        /// <summary>
        /// HTTP 状态码 424: 依赖失败。
        /// </summary>
        public const int Status424FailedDependency = 424;

        /// <summary>
        /// HTTP 状态码 426: 需要升级。
        /// </summary>
        public const int Status426UpgradeRequired = 426;

        /// <summary>
        /// HTTP 状态码 428: 需要先决条件。
        /// </summary>
        public const int Status428PreconditionRequired = 428;

        /// <summary>
        /// HTTP 状态码 429: 请求过多。
        /// </summary>
        public const int Status429TooManyRequests = 429;

        /// <summary>
        /// HTTP 状态码 431: 请求头字段过大。
        /// </summary>
        public const int Status431RequestHeaderFieldsTooLarge = 431;

        /// <summary>
        /// HTTP 状态码 451: 因法律原因不可用。
        /// </summary>
        public const int Status451UnavailableForLegalReasons = 451;

        /// <summary>
        /// HTTP 状态码 499: 客户端已关闭请求(非官方状态码, 最初由 Nginx 定义, 常用于客户端断开连接的日志)。
        /// </summary>
        public const int Status499ClientClosedRequest = 499;

        /// <summary>
        /// HTTP 状态码 500: 服务器内部错误。
        /// </summary>
        public const int Status500InternalServerError = 500;

        /// <summary>
        /// HTTP 状态码 501: 未实现。
        /// </summary>
        public const int Status501NotImplemented = 501;

        /// <summary>
        /// HTTP 状态码 502: 错误网关。
        /// </summary>
        public const int Status502BadGateway = 502;

        /// <summary>
        /// HTTP 状态码 503: 服务不可用。
        /// </summary>
        public const int Status503ServiceUnavailable = 503;

        /// <summary>
        /// HTTP 状态码 504: 网关超时。
        /// </summary>
        public const int Status504GatewayTimeout = 504;

        /// <summary>
        /// HTTP 状态码 505: HTTP 版本不支持。
        /// </summary>
        public const int Status505HttpVersionNotsupported = 505;

        /// <summary>
        /// HTTP 状态码 506: 变体协商冲突。
        /// </summary>
        public const int Status506VariantAlsoNegotiates = 506;

        /// <summary>
        /// HTTP 状态码 507: 存储空间不足。
        /// </summary>
        public const int Status507InsufficientStorage = 507;

        /// <summary>
        /// HTTP 状态码 508: 检测到循环。
        /// </summary>
        public const int Status508LoopDetected = 508;

        /// <summary>
        /// HTTP 状态码 510: 未扩展。
        /// </summary>
        public const int Status510NotExtended = 510;

        /// <summary>
        /// HTTP 状态码 511: 需要网络身份验证。
        /// </summary>
        public const int Status511NetworkAuthenticationRequired = 511;
    }
}
