using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.Http
{
    /// <summary>
    /// 响应头枚举
    /// </summary>
    public enum ResponseHeader
    {
        /// <summary>
        /// Cache-Control 标头，指定请求/响应链上所有缓存机制必须服从的缓存指令。
        /// </summary>
        [Description("Cache-Control")]
        CacheControl = 0,

        /// <summary>
        /// Connection 标头，指定特定连接需要的选项。
        /// </summary>
        [Description("Connection")]
        Connection = 1,

        /// <summary>
        /// Date 标头，指定响应产生的日期和时间。
        /// </summary>
        [Description("Date")]
        Date = 2,

        /// <summary>
        /// Keep-Alive 标头，指定用于维护持久连接的参数。
        /// </summary>
        [Description("Keep-Alive")]
        KeepAlive = 3,

        /// <summary>
        /// Pragma 标头，指定可应用于请求/响应链上的任何代理的特定于实现的指令。
        /// </summary>
        [Description("Pragma")]
        Pragma = 4,

        /// <summary>
        /// Trailer 标头，指定指示的标头字段在消息（使用分块传输编码方法进行编码）的尾部显示。
        /// </summary>
        [Description("Trailer")]
        Trailer = 5,

        /// <summary>
        /// Transfer-Encoding 标头，指定对消息正文应用哪种类型的转换（如果有）。
        /// </summary>
        [Description("Transfer-Encoding")]
        TransferEncoding = 6,

        /// <summary>
        /// Upgrade 标头，指定客户端支持的附加通信协议。
        /// </summary>
        [Description("Upgrade")]
        Upgrade = 7,

        /// <summary>
        /// Via 标头，指定网关和代理程序要使用的中间协议。
        /// </summary>
        [Description("Via")]
        Via = 8,

        /// <summary>
        /// Warning 标头，指定关于可能未在消息中反映的消息的状态或转换的附加信息。
        /// </summary>
        [Description("Warning")]
        Warning = 9,

        /// <summary>
        /// Allow 标头，指定支持的 HTTP 方法集。
        /// </summary>
        [Description("Allow")]
        Allow = 10,

        /// <summary>
        /// Content-Length 标头，指定伴随正文数据的长度（以字节为单位）。
        /// </summary>
        [Description("Content-Length")]
        ContentLength = 11,

        /// <summary>
        /// Content-Type 标头，指定伴随正文数据的 MIME 类型。
        /// </summary>
        [Description("Content-Type")]
        ContentType = 12,

        /// <summary>
        /// Content-Encoding 标头，指定已应用于伴随正文数据的编码。
        /// </summary>
        [Description("Content-Encoding")]
        ContentEncoding = 13,

        /// <summary>
        /// Content-Langauge 标头，指定自然语言或伴随正文数据的语言。
        /// </summary>
        [Description("Content-Langauge")]
        ContentLanguage = 14,

        /// <summary>
        /// Content-Location 标头，指定可以从中获取伴随正文的 URI。
        /// </summary>
        [Description("Content-Location")]
        ContentLocation = 15,

        /// <summary>
        /// Content-MD5 标头，指定伴随正文数据的 MD5 摘要，用于提供端到端消息完整性检查。
        /// </summary>
        [Description("Content-MD5")]
        ContentMd5 = 16,

        /// <summary>
        /// Range 标头，指定客户端请求返回的响应的单个或多个子范围来代替整个响应。
        /// </summary>
        [Description("Range")]
        ContentRange = 17,

        /// <summary>
        /// Expires 标头，指定日期和时间，在此之后伴随的正文数据应视为陈旧的。
        /// </summary>
        [Description("Expires")]
        Expires = 18,

        /// <summary>
        /// Last-Modified 标头，指定上次修改伴随的正文数据的日期和时间。
        /// </summary>
        [Description("Last-Modified")]
        LastModified = 19,

        /// <summary>
        /// Accept-Ranges 标头，指定服务器接受的范围。
        /// </summary>
        [Description("Accept-Ranges")]
        AcceptRanges = 20,

        /// <summary>
        /// Age 标头，指定自起始服务器生成响应以来的时间长度（以秒为单位）。
        /// </summary>
        [Description("Age")]
        Age = 21,

        /// <summary>
        /// Etag 标头，指定请求的变量的当前值。
        /// </summary>
        [Description("Etag")]
        ETag = 22,

        /// <summary>
        /// Location 标头，指定为获取请求的资源而将客户端重定向到的 URI。
        /// </summary>
        [Description("Location")]
        Location = 23,

        /// <summary>
        /// Proxy-Authenticate 标头，指定客户端必须对代理验证其自身。
        /// </summary>
        [Description("Proxy-Authenticate")]
        ProxyAuthenticate = 24,

        /// <summary>
        /// Retry-After 标头，指定某个时间（以秒为单位）或日期和时间，在此时间之后客户端可以重试其请求。
        /// </summary>
        [Description("Retry-After")]
        RetryAfter = 25,

        /// <summary>
        /// Server 标头，指定关于起始服务器代理的信息。
        /// </summary>
        [Description("Server")]
        Server = 26,

        /// <summary>
        /// Set-Cookie 标头，指定提供给客户端的 Cookie 数据。
        /// </summary>
        [Description("Set-Cookie")]
        SetCookie = 27,

        /// <summary>
        /// Vary 标头，指定用于确定缓存的响应是否为新响应的请求标头。
        /// </summary>
        [Description("Vary")]
        Vary = 28,

        /// <summary>
        /// WWW-Authenticate 标头，指定客户端必须对服务器验证其自身。
        /// </summary>
        [Description("WWW-Authenticate")]
        WwwAuthenticate = 29,


    }
}
