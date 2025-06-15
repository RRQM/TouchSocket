// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

namespace TouchSocket.Http;
/// <summary>
/// 包含RFC 2616中定义的HTTP 1.1状态码值
/// </summary>
public static class HttpStatusCode
{
    /// <summary>
    /// 表示客户端可以继续其请求，对应HTTP状态码100
    /// </summary>
    public const int Continue = 100;

    /// <summary>
    /// 表示协议版本或协议正在变更，对应HTTP状态码101
    /// </summary>
    public const int SwitchingProtocols = 101;

    /// <summary>
    /// 表示服务器已接受完整请求但尚未完成处理，对应HTTP状态码102
    /// </summary>
    public const int Processing = 102;

    /// <summary>
    /// 向客户端提示服务器可能会发送包含此信息性响应中头部字段的最终响应，对应HTTP状态码103
    /// </summary>
    public const int EarlyHints = 103;

    /// <summary>
    /// 请求成功，请求的信息包含在响应中，这是最常见的状态码，对应HTTP状态码200
    /// </summary>
    public const int OK = 200;

    /// <summary>
    /// 请求导致在发送响应前创建了新资源，对应HTTP状态码201
    /// </summary>
    public const int Created = 201;

    /// <summary>
    /// 请求已接受并将进行后续处理，对应HTTP状态码202
    /// </summary>
    public const int Accepted = 202;

    /// <summary>
    /// 返回的元信息来自缓存副本而非原始服务器，因此可能不正确，对应HTTP状态码203
    /// </summary>
    public const int NonAuthoritativeInformation = 203;

    /// <summary>
    /// 请求已成功处理，响应故意为空，对应HTTP状态码204
    /// </summary>
    public const int NoContent = 204;

    /// <summary>
    /// 客户端应重置（而非重新加载）当前资源，对应HTTP状态码205
    /// </summary>
    public const int ResetContent = 205;

    /// <summary>
    /// 响应是根据包含字节范围的GET请求返回的部分响应，对应HTTP状态码206
    /// </summary>
    public const int PartialContent = 206;

    /// <summary>
    /// 在WebDAV操作中，单个响应包含多个状态码，响应体包含描述状态码的XML，对应HTTP状态码207
    /// </summary>
    public const int MultiStatus = 207;

    /// <summary>
    /// WebDAV绑定的成员已在之前的多状态响应部分中枚举，不再重复包含，对应HTTP状态码208
    /// </summary>
    public const int AlreadyReported = 208;

    /// <summary>
    /// 服务器已完成资源请求，响应是对当前实例应用一个或多个实例操作结果的表示，对应HTTP状态码226
    /// </summary>
    public const int IMUsed = 226;

    /// <summary>
    /// 请求的信息有多个表示形式（与MultipleChoices同义），默认行为是视为重定向并跟随Location头部，对应HTTP状态码300
    /// </summary>
    public const int Ambiguous = 300;

    /// <summary>
    /// 请求的信息有多个表示形式（与Ambiguous同义），默认行为是视为重定向并跟随Location头部，对应HTTP状态码300
    /// </summary>
    public const int MultipleChoices = 300;

    /// <summary>
    /// 请求的信息已永久移动到Location头部指定的URI（与MovedPermanently同义），默认跟随重定向，POST请求重定向时改用GET方法，对应HTTP状态码301
    /// </summary>
    public const int Moved = 301;

    /// <summary>
    /// 请求的信息已永久移动到Location头部指定的URI（与Moved同义），默认跟随重定向，POST请求重定向时改用GET方法，对应HTTP状态码301
    /// </summary>
    public const int MovedPermanently = 301;

    /// <summary>
    /// 请求的信息位于Location头部指定的URI（与Redirect同义），默认跟随重定向，POST请求重定向时改用GET方法，对应HTTP状态码302
    /// </summary>
    public const int Found = 302;

    /// <summary>
    /// 请求的信息位于Location头部指定的URI（与Found同义），默认跟随重定向，POST请求重定向时改用GET方法，对应HTTP状态码302
    /// </summary>
    public const int Redirect = 302;

    /// <summary>
    /// 作为POST请求的结果，自动将客户端重定向到Location头部指定的URI（与SeeOther同义），重定向请求使用GET方法，对应HTTP状态码303
    /// </summary>
    public const int RedirectMethod = 303;

    /// <summary>
    /// 作为POST请求的结果，自动将客户端重定向到Location头部指定的URI（与RedirectMethod同义），重定向请求使用GET方法，对应HTTP状态码303
    /// </summary>
    public const int SeeOther = 303;

    /// <summary>
    /// 客户端的缓存副本是最新的，不传输资源内容，对应HTTP状态码304
    /// </summary>
    public const int NotModified = 304;

    /// <summary>
    /// 请求应使用Location头部指定的代理服务器，对应HTTP状态码305
    /// </summary>
    public const int UseProxy = 305;

    /// <summary>
    /// 未使用，是HTTP/1.1规范的一个未完全指定的扩展提案，对应HTTP状态码306
    /// </summary>
    public const int Unused = 306;

    /// <summary>
    /// 请求的信息位于Location头部指定的URI（与TemporaryRedirect同义），默认跟随重定向，POST请求重定向时保持使用POST方法，对应HTTP状态码307
    /// </summary>
    public const int RedirectKeepVerb = 307;

    /// <summary>
    /// 请求的信息位于Location头部指定的URI（与RedirectKeepVerb同义），默认跟随重定向，POST请求重定向时保持使用POST方法，对应HTTP状态码307
    /// </summary>
    public const int TemporaryRedirect = 307;

    /// <summary>
    /// 请求的信息永久位于Location头部指定的URI，默认跟随重定向，POST请求重定向时保持使用POST方法，对应HTTP状态码308
    /// </summary>
    public const int PermanentRedirect = 308;

    /// <summary>
    /// 服务器无法理解请求，当没有其他适用错误或确切错误未知时返回，对应HTTP状态码400
    /// </summary>
    public const int BadRequest = 400;

    /// <summary>
    /// 请求的资源需要身份验证，WWW-Authenticate头部包含身份验证细节，对应HTTP状态码401
    /// </summary>
    public const int Unauthorized = 401;

    /// <summary>
    /// 为未来使用保留，对应HTTP状态码402
    /// </summary>
    public const int PaymentRequired = 402;

    /// <summary>
    /// 服务器拒绝处理请求，对应HTTP状态码403
    /// </summary>
    public const int Forbidden = 403;

    /// <summary>
    /// 请求的资源在服务器上不存在，对应HTTP状态码404
    /// </summary>
    public const int NotFound = 404;

    /// <summary>
    /// 请求方法（POST或GET）不被请求的资源允许，对应HTTP状态码405
    /// </summary>
    public const int MethodNotAllowed = 405;

    /// <summary>
    /// 客户端通过Accept头部表示不接受任何可用的资源表示形式，对应HTTP状态码406
    /// </summary>
    public const int NotAcceptable = 406;

    /// <summary>
    /// 请求的代理需要身份验证，Proxy-Authenticate头部包含身份验证细节，对应HTTP状态码407
    /// </summary>
    public const int ProxyAuthenticationRequired = 407;

    /// <summary>
    /// 客户端未在服务器预期的时间内发送请求，对应HTTP状态码408
    /// </summary>
    public const int RequestTimeout = 408;

    /// <summary>
    /// 由于服务器上的冲突，请求无法执行，对应HTTP状态码409
    /// </summary>
    public const int Conflict = 409;

    /// <summary>
    /// 请求的资源不再可用，对应HTTP状态码410
    /// </summary>
    public const int Gone = 410;

    /// <summary>
    /// 缺少必需的Content-Length头部，对应HTTP状态码411
    /// </summary>
    public const int LengthRequired = 411;

    /// <summary>
    /// 请求设置的条件（如If-Match、If-None-Match等头部）失败，无法执行请求，对应HTTP状态码412
    /// </summary>
    public const int PreconditionFailed = 412;

    /// <summary>
    /// 请求实体过大，服务器无法处理，对应HTTP状态码413
    /// </summary>
    public const int RequestEntityTooLarge = 413;

    /// <summary>
    /// URI过长，对应HTTP状态码414
    /// </summary>
    public const int RequestUriTooLong = 414;

    /// <summary>
    /// 请求的媒体类型不受支持，对应HTTP状态码415
    /// </summary>
    public const int UnsupportedMediaType = 415;

    /// <summary>
    /// 请求的资源范围无法满足（如范围开始早于资源起点或结束晚于资源终点），对应HTTP状态码416
    /// </summary>
    public const int RequestedRangeNotSatisfiable = 416;

    /// <summary>
    /// Expect头部中给出的期望无法被服务器满足，对应HTTP状态码417
    /// </summary>
    public const int ExpectationFailed = 417;

    /// <summary>
    /// 请求被定向到无法生成响应的服务器，对应HTTP状态码421
    /// </summary>
    public const int MisdirectedRequest = 421;

    /// <summary>
    /// 请求格式正确但因语义错误无法处理（与UnprocessableContent同义），对应HTTP状态码422
    /// </summary>
    public const int UnprocessableEntity = 422;

    /// <summary>
    /// 请求格式正确但因语义错误无法处理（与UnprocessableEntity同义），对应HTTP状态码422
    /// </summary>
    public const int UnprocessableContent = 422;

    /// <summary>
    /// 源或目标资源被锁定，对应HTTP状态码423
    /// </summary>
    public const int Locked = 423;

    /// <summary>
    /// 由于依赖的操作失败，无法对资源执行请求的方法，对应HTTP状态码424
    /// </summary>
    public const int FailedDependency = 424;

    /// <summary>
    /// 客户端应切换到其他协议（如TLS/1.0），对应HTTP状态码426
    /// </summary>
    public const int UpgradeRequired = 426;

    /// <summary>
    /// 服务器要求请求为条件请求，对应HTTP状态码428
    /// </summary>
    public const int PreconditionRequired = 428;

    /// <summary>
    /// 用户在给定时间内发送了过多请求，对应HTTP状态码429
    /// </summary>
    public const int TooManyRequests = 429;

    /// <summary>
    /// 服务器因请求头部字段过大（单个或整体）而不愿处理请求，对应HTTP状态码431
    /// </summary>
    public const int RequestHeaderFieldsTooLarge = 431;

    /// <summary>
    /// 服务器因法律要求拒绝访问资源，对应HTTP状态码451
    /// </summary>
    public const int UnavailableForLegalReasons = 451;

    /// <summary>
    /// 服务器发生通用错误，对应HTTP状态码500
    /// </summary>
    public const int InternalServerError = 500;

    /// <summary>
    /// 服务器不支持请求的功能，对应HTTP状态码501
    /// </summary>
    public const int NotImplemented = 501;

    /// <summary>
    /// 中间代理服务器从另一个代理或原始服务器收到错误响应，对应HTTP状态码502
    /// </summary>
    public const int BadGateway = 502;

    /// <summary>
    /// 服务器暂时不可用（通常因高负载或维护），对应HTTP状态码503
    /// </summary>
    public const int ServiceUnavailable = 503;

    /// <summary>
    /// 中间代理服务器在等待另一个代理或原始服务器的响应时超时，对应HTTP状态码504
    /// </summary>
    public const int GatewayTimeout = 504;

    /// <summary>
    /// 服务器不支持请求的HTTP版本，对应HTTP状态码505
    /// </summary>
    public const int HttpVersionNotSupported = 505;

    /// <summary>
    /// 选择的变体资源配置为参与透明内容协商，因此不是协商过程的合适端点，对应HTTP状态码506
    /// </summary>
    public const int VariantAlsoNegotiates = 506;

    /// <summary>
    /// 服务器无法存储完成请求所需的表示，对应HTTP状态码507
    /// </summary>
    public const int InsufficientStorage = 507;

    /// <summary>
    /// 服务器在处理"Depth: infinity"的WebDAV请求时遇到无限循环，终止操作（用于向后兼容不识别208状态码的客户端），对应HTTP状态码508
    /// </summary>
    public const int LoopDetected = 508;

    /// <summary>
    /// 服务器需要请求进行进一步扩展以完成处理，对应HTTP状态码510
    /// </summary>
    public const int NotExtended = 510;

    /// <summary>
    /// 客户端需要进行网络身份验证以获取网络访问（用于控制网络访问的拦截代理），对应HTTP状态码511
    /// </summary>
    public const int NetworkAuthenticationRequired = 511;
}