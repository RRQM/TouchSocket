//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System.Runtime.CompilerServices;

namespace TouchSocket.Http;
/// <summary>
/// 请求头静态类
/// </summary>
public static class HttpHeaders
{
    /// <summary>
    /// Cache-Control 标头，指定请求/响应链上所有缓存控制机制必须服从的指令。
    /// </summary>
    public const string CacheControl = "Cache-Control";

    /// <summary>
    /// Connection 标头，指定特定连接需要的选项。
    /// </summary>
    public const string Connection = "Connection";

    /// <summary>
    /// Date 标头，指定开始创建请求的日期和时间。
    /// </summary>
    public const string Date = "Date";

    /// <summary>
    /// Keep-Alive 标头，指定用以维护持久性连接的参数。
    /// </summary>
    public const string KeepAlive = "Keep-Alive";

    /// <summary>
    /// Pragma 标头，指定可应用于请求/响应链上的任何代理的特定于实现的指令。
    /// </summary>
    public const string Pragma = "Pragma";

    /// <summary>
    /// Trailer 标头，指定标头字段显示在以 chunked 传输编码方式编码的消息的尾部。
    /// </summary>
    public const string Trailer = "Trailer";

    /// <summary>
    /// Transfer-Encoding 标头，指定对消息正文应用的转换的类型（如果有）。
    /// </summary>
    public const string TransferEncoding = "Transfer-Encoding";

    /// <summary>
    /// Upgrade 标头，指定客户端支持的附加通信协议。
    /// </summary>
    public const string Upgrade = "Upgrade";

    /// <summary>
    /// Via 标头，指定网关和代理程序要使用的中间协议。
    /// </summary>
    public const string Via = "Via";

    /// <summary>
    /// Warning 标头，指定关于可能未在消息中反映的消息的状态或转换的附加信息。
    /// </summary>
    public const string Warning = "Warning";

    /// <summary>
    /// Allow 标头，指定支持的 HTTP 方法集。
    /// </summary>
    public const string Allow = "Allow";

    /// <summary>
    /// Content-Length 标头，指定伴随正文数据的长度（以字节为单位）。
    /// </summary>
    public const string ContentLength = "Content-Length";

    /// <summary>
    /// Content-Type 标头，指定伴随正文数据的 MIME 类型。
    /// </summary>
    public const string ContentType = "Content-Type";

    /// <summary>
    /// Content-Encoding 标头，指定已应用于伴随正文数据的编码。
    /// </summary>
    public const string ContentEncoding = "Content-Encoding";

    /// <summary>
    /// Content-Langauge 标头，指定伴随正文数据的自然语言。
    /// </summary>
    public const string ContentLanguage = "Content-Langauge";

    /// <summary>
    /// Content-Location 标头，指定可从其中获得伴随正文的 URI。
    /// </summary>
    public const string ContentLocation = "Content-Location";

    /// <summary>
    /// Content-MD5 标头，指定伴随正文数据的 MD5 摘要，用于提供端到端消息完整性检查。
    /// </summary>
    public const string ContentMd5 = "Content-MD5";

    /// <summary>
    /// Content-Range 标头，指定在完整正文中应用伴随部分正文数据的位置。
    /// </summary>
    public const string ContentRange = "Content-Range";

    /// <summary>
    /// Expires 标头，指定日期和时间，在此之后伴随的正文数据应视为陈旧的。
    /// </summary>
    public const string Expires = "Expires";

    /// <summary>
    /// Last-Modified 标头，指定上次修改伴随的正文数据的日期和时间。
    /// </summary>
    public const string LastModified = "Last-Modified";

    /// <summary>
    /// Accept 标头，指定响应可接受的 MIME 类型。
    /// </summary>
    public const string Accept = "Accept";

    /// <summary>
    /// Accept-Charset 标头，指定响应可接受的字符集。
    /// </summary>
    public const string AcceptCharset = "Accept-Charset";

    /// <summary>
    /// Accept-Encoding 标头，指定响应可接受的内容编码。
    /// </summary>
    public const string AcceptEncoding = "Accept-Encoding";

    /// <summary>
    /// Accept-Langauge 标头，指定响应首选的自然语言。
    /// </summary>
    public const string AcceptLanguage = "Accept-Langauge";

    /// <summary>
    /// Authorization 标头，指定客户端为向服务器验证自身身份而出示的凭据。
    /// </summary>
    public const string Authorization = "Authorization";

    /// <summary>
    /// Cookie 标头，指定向服务器提供的 Cookie 数据。
    /// </summary>
    public const string Cookie = "Cookie";

    /// <summary>
    /// Expect 标头，指定客户端要求的特定服务器行为。
    /// </summary>
    public const string Expect = "Expect";

    /// <summary>
    /// From 标头，指定控制请求用户代理的用户的 Internet 电子邮件地址。
    /// </summary>
    public const string From = "From";

    /// <summary>
    /// Host 标头，指定所请求资源的主机名和端口号。
    /// </summary>
    public const string Host = "Host";

    /// <summary>
    /// If-Match 标头，指定仅当客户端的指示资源的缓存副本是最新的时，才执行请求的操作。
    /// </summary>
    public const string IfMatch = "If-Match";

    /// <summary>
    /// If-Modified-Since 标头，指定仅当自指示的数据和时间之后修改了请求的资源时，才执行请求的操作。
    /// </summary>
    public const string IfModifiedSince = "If-Modified-Since";

    /// <summary>
    /// If-None-Match 标头，指定仅当客户端的指示资源的缓存副本都不是最新的时，才执行请求的操作。
    /// </summary>
    public const string IfNoneMatch = "If-None-Match";

    /// <summary>
    /// If-Range 标头，指定如果客户端的缓存副本是最新的，仅发送指定范围的请求资源。
    /// </summary>
    public const string IfRange = "If-Range";

    /// <summary>
    /// If-Unmodified-Since 标头，指定仅当自指示的日期和时间之后修改了请求的资源时，才执行请求的操作。
    /// </summary>
    public const string IfUnmodifiedSince = "If-Unmodified-Since";

    /// <summary>
    /// Max-Forwards 标头，指定一个整数，表示此请求还可转发的次数。
    /// </summary>
    public const string MaxForwards = "Max-Forwards";

    /// <summary>
    /// Proxy-Authorization 标头，指定客户端为向代理验证自身身份而出示的凭据。
    /// </summary>
    public const string ProxyAuthorization = "Proxy-Authorization";

    /// <summary>
    /// Referer 标头，指定从中获得请求 URI 的资源的 URI。
    /// </summary>
    public const string Referer = "Referer";

    /// <summary>
    /// Range 标头，指定代替整个响应返回的客户端请求的响应的子范围。
    /// </summary>
    public const string Range = "Range";

    /// <summary>
    /// TE 标头，指定响应可接受的传输编码方式。
    /// </summary>
    public const string Te = "TE";

    /// <summary>
    /// Translate 标头，与 WebDAV 功能一起使用的 HTTP 规范的 Microsoft 扩展。
    /// </summary>
    public const string Translate = "Translate";

    /// <summary>
    /// User-Agent 标头，指定有关客户端代理的信息。
    /// </summary>
    public const string UserAgent = "User-Agent";

    /// <summary>
    /// Accept-Ranges 标头，指定服务器接受的范围。
    /// </summary>
    public const string AcceptRanges = "Accept-Ranges";

    /// <summary>
    /// Age 标头，指定自起始服务器生成响应以来的时间长度（以秒为单位）。
    /// </summary>
    public const string Age = "Age";

    /// <summary>
    /// Etag 标头，指定请求的变量的当前值。
    /// </summary>
    public const string ETag = "Etag";

    /// <summary>
    /// Location 标头，指定为获取请求的资源而将客户端重定向到的 URI。
    /// </summary>
    public const string Location = "Location";

    /// <summary>
    /// Proxy-Authenticate 标头，指定客户端必须对代理验证其自身。
    /// </summary>
    public const string ProxyAuthenticate = "Proxy-Authenticate";

    /// <summary>
    /// Retry-After 标头，指定某个时间（以秒为单位）或日期和时间，在此时间之后客户端可以重试其请求。
    /// </summary>
    public const string RetryAfter = "Retry-After";

    /// <summary>
    /// Server 标头，指定关于起始服务器代理的信息。
    /// </summary>
    public const string Server = "Server";

    /// <summary>
    /// Set-Cookie 标头，指定提供给客户端的 Cookie 数据。
    /// </summary>
    public const string SetCookie = "Set-Cookie";

    /// <summary>
    /// Vary 标头，指定用于确定缓存的响应是否为新响应的请求标头。
    /// </summary>
    public const string Vary = "Vary";

    /// <summary>
    /// WWW-Authenticate 标头，指定客户端必须对服务器验证其自身。
    /// </summary>
    public const string WwwAuthenticate = "WWW-Authenticate";

    /// <summary>
    /// Origin。
    /// </summary>
    public const string Origin = "Origin";

    /// <summary>
    /// Content-Disposition
    /// </summary>
    public const string ContentDisposition = "Content-Disposition";

    internal static bool IsPredefinedHeader(string key)
    {
        var keyLength = key.Length;
        if (keyLength == 0)
        {
            return false;
        }

        var firstChar = char.ToLowerInvariant(key[0]);

        return firstChar switch
        {
            'a' => IsAGroupHeader(key, keyLength),
            'c' => IsCGroupHeader(key, keyLength),
            'd' => keyLength == 4 && ReferenceEquals(key, HttpHeaders.Date),
            'e' => IsEGroupHeader(key, keyLength),
            'f' => keyLength == 4 && ReferenceEquals(key, HttpHeaders.From),
            'h' => keyLength == 4 && ReferenceEquals(key, HttpHeaders.Host),
            'i' => IsIGroupHeader(key, keyLength),
            'k' => keyLength == 9 && ReferenceEquals(key, HttpHeaders.KeepAlive),
            'l' => keyLength == 8 && ReferenceEquals(key, HttpHeaders.Location),
            'm' => keyLength == 11 && ReferenceEquals(key, HttpHeaders.MaxForwards),
            'o' => keyLength == 6 && ReferenceEquals(key, HttpHeaders.Origin),
            'p' => IsPGroupHeader(key, keyLength),
            'r' => IsRGroupHeader(key, keyLength),
            's' => IsSGroupHeader(key, keyLength),
            't' => IsTGroupHeader(key, keyLength),
            'u' => keyLength == 10 && ReferenceEquals(key, HttpHeaders.UserAgent),
            'v' => IsVGroupHeader(key, keyLength),
            'w' => keyLength == 13 && ReferenceEquals(key, HttpHeaders.WwwAuthenticate),
            _ => false
        };
    }

    #region 各分组Header判断（模块化，易维护）
    [MethodImpl(MethodImplOptions.AggressiveInlining | (MethodImplOptions)512)]
    private static bool IsAGroupHeader(string key, int keyLength)
    {
        // 预定义长度：Age(3)、Allow(5)、Accept(6)、Accept-Ranges(12)、Accept-Charset/Authorization(13)、Accept-Encoding(14)、Accept-Language(15)
        return keyLength switch
        {
            3 => ReferenceEquals(key, HttpHeaders.Age),
            5 => ReferenceEquals(key, HttpHeaders.Allow),
            6 => ReferenceEquals(key, HttpHeaders.Accept),
            12 => ReferenceEquals(key, HttpHeaders.AcceptRanges),
            13 => ReferenceEquals(key, HttpHeaders.AcceptCharset) || ReferenceEquals(key, HttpHeaders.Authorization),
            14 => ReferenceEquals(key, HttpHeaders.AcceptEncoding),
            15 => ReferenceEquals(key, HttpHeaders.AcceptLanguage),
            _ => false // 长度不匹配直接返回false
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | (MethodImplOptions)512)]
    private static bool IsCGroupHeader(string key, int keyLength)
    {
        // 预定义长度：Cookie(6)、Connection/Content-MD5(10)、Content-Type(12)、Cache-Control/Content-Range(13)、Content-Length(14)、Content-Encoding(15)、Content-Language/Content-Location(16)、Content-Disposition(19)
        return keyLength switch
        {
            6 => ReferenceEquals(key, HttpHeaders.Cookie),
            10 => ReferenceEquals(key, HttpHeaders.Connection) || ReferenceEquals(key, HttpHeaders.ContentMd5),
            12 => ReferenceEquals(key, HttpHeaders.ContentType),
            13 => ReferenceEquals(key, HttpHeaders.CacheControl) || ReferenceEquals(key, HttpHeaders.ContentRange),
            14 => ReferenceEquals(key, HttpHeaders.ContentLength),
            15 => ReferenceEquals(key, HttpHeaders.ContentEncoding),
            16 => ReferenceEquals(key, HttpHeaders.ContentLanguage) || ReferenceEquals(key, HttpHeaders.ContentLocation),
            19 => ReferenceEquals(key, HttpHeaders.ContentDisposition),
            _ => false
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | (MethodImplOptions)512)]
    private static bool IsEGroupHeader(string key, int keyLength)
    {
        // 预定义长度：ETag(4)、Expect(6)、Expires(7)
        return keyLength switch
        {
            4 => ReferenceEquals(key, HttpHeaders.ETag),
            6 => ReferenceEquals(key, HttpHeaders.Expect),
            7 => ReferenceEquals(key, HttpHeaders.Expires),
            _ => false
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | (MethodImplOptions)512)]
    private static bool IsIGroupHeader(string key, int keyLength)
    {
        // 预定义长度：If-Match/If-Range(7)、If-None-Match(11)、If-Modified-Since(15)、If-Unmodified-Since(18)
        return keyLength switch
        {
            7 => ReferenceEquals(key, HttpHeaders.IfMatch) || ReferenceEquals(key, HttpHeaders.IfRange),
            11 => ReferenceEquals(key, HttpHeaders.IfNoneMatch),
            15 => ReferenceEquals(key, HttpHeaders.IfModifiedSince),
            18 => ReferenceEquals(key, HttpHeaders.IfUnmodifiedSince),
            _ => false
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | (MethodImplOptions)512)]
    private static bool IsPGroupHeader(string key, int keyLength)
    {
        // 预定义长度：Pragma(6)、Proxy-Authorization/Proxy-Authenticate(16)
        return keyLength switch
        {
            6 => ReferenceEquals(key, HttpHeaders.Pragma),
            16 => ReferenceEquals(key, HttpHeaders.ProxyAuthorization) || ReferenceEquals(key, HttpHeaders.ProxyAuthenticate),
            _ => false
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | (MethodImplOptions)512)]
    private static bool IsRGroupHeader(string key, int keyLength)
    {
        // 预定义长度：Range(5)、Referer(6)、Retry-After(10)
        return keyLength switch
        {
            5 => ReferenceEquals(key, HttpHeaders.Range),
            6 => ReferenceEquals(key, HttpHeaders.Referer),
            10 => ReferenceEquals(key, HttpHeaders.RetryAfter),
            _ => false
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | (MethodImplOptions)512)]
    private static bool IsSGroupHeader(string key, int keyLength)
    {
        // 预定义长度：Server(6)、Set-Cookie(8)
        return keyLength switch
        {
            6 => ReferenceEquals(key, HttpHeaders.Server),
            8 => ReferenceEquals(key, HttpHeaders.SetCookie),
            _ => false
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | (MethodImplOptions)512)]
    private static bool IsTGroupHeader(string key, int keyLength)
    {
        // 预定义长度：TE(2)、Trailer(7)、Translate(8)、Transfer-Encoding(16)
        return keyLength switch
        {
            2 => ReferenceEquals(key, HttpHeaders.Te),
            7 => ReferenceEquals(key, HttpHeaders.Trailer),
            8 => ReferenceEquals(key, HttpHeaders.Translate),
            16 => ReferenceEquals(key, HttpHeaders.TransferEncoding),
            _ => false
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | (MethodImplOptions)512)]
    private static bool IsVGroupHeader(string key, int keyLength)
    {
        // 预定义长度：Via(3)、Vary(4)、Warning(7)
        return keyLength switch
        {
            3 => ReferenceEquals(key, HttpHeaders.Via),
            4 => ReferenceEquals(key, HttpHeaders.Vary),
            7 => ReferenceEquals(key, HttpHeaders.Warning),
            _ => false
        };
    }
    #endregion
}