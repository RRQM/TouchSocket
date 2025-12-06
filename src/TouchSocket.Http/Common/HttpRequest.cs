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

using TouchSocket.Sockets;

namespace TouchSocket.Http;

/// <summary>
/// HTTP请求定义
/// </summary>
public class HttpRequest : HttpBase
{
    private readonly bool m_isServer;
    private readonly InternalHttpParams m_query = new InternalHttpParams();
    private string m_relativeURL = "/";
    private string m_url = "/";

    /// <summary>
    /// HttpRequest类的构造函数。
    /// </summary>
    /// <remarks>
    /// 初始化HttpRequest对象的基本属性。
    /// </remarks>
    public HttpRequest()
    {
        this.m_isServer = false;
    }

    internal HttpRequest(HttpSessionClient httpSessionClient)
    {
        this.m_isServer = true;
    }

    /// <inheritdoc/>
    public override bool IsServer => this.m_isServer;

    /// <summary>
    /// 保持连接。
    /// <para>
    /// 一般的，当是http1.1时，如果没有显式的Connection: close，即返回<see langword="true"/>。当是http1.0时，如果没有显式的Connection: Keep-Alive，即返回<see langword="false"/>。
    /// </para>
    /// </summary>
    public bool KeepAlive
    {
        get
        {
            var keepAlive = this.Headers.Get(HttpHeaders.Connection);
            return this.ProtocolVersion == "1.0"
                ? !keepAlive.IsEmpty && keepAlive.Equals("keep-alive", StringComparison.OrdinalIgnoreCase)
                : keepAlive.IsEmpty || keepAlive.Equals("keep-alive", StringComparison.OrdinalIgnoreCase);
        }
        set
        {
            if (this.ProtocolVersion == "1.0")
            {
                if (value)
                {
                    this.Headers.Add(HttpHeaders.Connection, "Keep-Alive");
                }
                else
                {
                    this.Headers.Add(HttpHeaders.Connection, "close");
                }
            }
            else
            {
                if (!value)
                {
                    this.Headers.Add(HttpHeaders.Connection, "close");
                }
            }
        }
    }

    /// <summary>
    /// HTTP请求方式。
    /// </summary>
    public HttpMethod Method { get; set; } = HttpMethod.Get;

    /// <summary>
    /// url参数
    /// </summary>
    public IHttpParams Query => this.m_query;

    /// <summary>
    /// 相对路径（不含参数）
    /// </summary>
    public string RelativeURL => this.m_relativeURL;

    /// <summary>
    /// Url全地址，包含参数
    /// </summary>
    public string URL
    {
        get => this.m_url;
        set
        {
            this.m_url = value;
            this.ParseUrl(this.m_url.AsSpan());
        }
    }

    /// <inheritdoc/>
    public override ValueTask<ReadOnlyMemory<byte>> GetContentAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public override ValueTask<HttpReadOnlyMemoryBlockResult> ReadAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 设置代理Host
    /// </summary>
    /// <param name="host">代理服务器的地址</param>
    /// <returns>返回当前HttpRequest实例，以支持链式调用</returns>
    public HttpRequest SetProxyHost(string host)
    {
        // 将URL属性设置为指定的代理服务器地址
        this.URL = host;
        // 返回当前实例，以支持链式调用
        return this;
    }

    internal void BuildHeader<TWriter>(ref TWriter writer) where TWriter : IBytesWriter
    {
        WriterExtension.WriteNormalString(ref writer, this.Method.ToString(), Encoding.UTF8);//Get
        TouchSocketHttpUtility.AppendSpace(ref writer);//空格
        TouchSocketHttpUtility.AppendUtf8String(ref writer, this.RelativeURL);
        if (this.m_query.Count > 0)
        {
            TouchSocketHttpUtility.AppendQuestionMark(ref writer);
            var i = 0;
            foreach (var item in this.m_query.Keys)
            {
                WriterExtension.WriteNormalString(ref writer, item, Encoding.UTF8);
                TouchSocketHttpUtility.AppendEqual(ref writer);
                var value = this.m_query[item];
                if (!value.IsEmpty)
                {
                    WriterExtension.WriteNormalString(ref writer, Uri.EscapeDataString(value), Encoding.UTF8);
                }

                if (++i < this.m_query.Count)
                {
                    TouchSocketHttpUtility.AppendAnd(ref writer);
                }
            }
        }
        TouchSocketHttpUtility.AppendSpace(ref writer);//空格
        TouchSocketHttpUtility.AppendHTTP(ref writer);//HTTP
        TouchSocketHttpUtility.AppendSlash(ref writer);//斜杠
        WriterExtension.WriteNormalString(ref writer, this.ProtocolVersion, Encoding.UTF8);//1.1
        TouchSocketHttpUtility.AppendRn(ref writer);//换行

        foreach (var headerKey in this.Headers.Keys)
        {
            WriterExtension.WriteNormalString(ref writer, headerKey, Encoding.UTF8);//key
            TouchSocketHttpUtility.AppendColon(ref writer);//冒号
            TouchSocketHttpUtility.AppendSpace(ref writer);//空格
            WriterExtension.WriteNormalString(ref writer, this.Headers[headerKey], Encoding.UTF8);//value
            TouchSocketHttpUtility.AppendRn(ref writer);//换行
        }

        TouchSocketHttpUtility.AppendRn(ref writer);
    }

    /// <inheritdoc/>
    protected internal override void Reset()
    {
        base.Reset();

        this.m_relativeURL = "/";
        this.m_url = "/";

        this.m_query.Clear();
    }

    /// <inheritdoc/>
    protected override void ReadRequestLine(ReadOnlySpan<byte> requestLineSpan)
    {
        var start = 0;

        var methodEnd = TouchSocketHttpUtility.FindNextWhitespace(requestLineSpan, start);
        if (methodEnd == -1)
        {
            throw new Exception("Invalid HTTP request line: " + requestLineSpan.ToString(Encoding.UTF8));
        }

        var methodSpan = requestLineSpan.Slice(start, methodEnd - start);
        this.Method = ParseHttpMethodFast(methodSpan);
        start = TouchSocketHttpUtility.SkipSpaces(requestLineSpan, methodEnd + 1);

        var urlEnd = TouchSocketHttpUtility.FindNextWhitespace(requestLineSpan, start);
        if (urlEnd == -1)
        {
            this.URL = TouchSocketHttpUtility.UnescapeDataString(requestLineSpan.Slice(start));
            return;
        }

        this.URL = TouchSocketHttpUtility.UnescapeDataString(requestLineSpan.Slice(start, urlEnd - start));
        start = TouchSocketHttpUtility.SkipSpaces(requestLineSpan, urlEnd + 1);

        var protocolSpan = requestLineSpan.Slice(start);
        var slashIndex = protocolSpan.IndexOf((byte)'/');
        if (slashIndex > 0 && slashIndex < protocolSpan.Length - 1)
        {
            this.Protocols = new Protocol(protocolSpan.Slice(0, slashIndex).ToString(Encoding.UTF8));
            this.ProtocolVersion = protocolSpan.Slice(slashIndex + 1).ToString(Encoding.UTF8);
        }
    }

    private static HttpMethod ParseHttpMethodFast(ReadOnlySpan<byte> span)
    {
        switch (span.Length)
        {
            case 3:
                if (EqualsIgnoreCaseAscii(span, "GET")) return HttpMethod.Get;
                if (EqualsIgnoreCaseAscii(span, "PUT")) return HttpMethod.Put;
                break;
            case 4:
                if (EqualsIgnoreCaseAscii(span, "POST")) return HttpMethod.Post;
                break;
            case 6:
                if (EqualsIgnoreCaseAscii(span, "DELETE")) return HttpMethod.Delete;
                break;
            case 7:
                if (EqualsIgnoreCaseAscii(span, "CONNECT")) return HttpMethod.Connect;
                break;
        }
        return new HttpMethod(span.ToString(Encoding.UTF8));
    }

    private static bool EqualsIgnoreCaseAscii(ReadOnlySpan<byte> span, string token)
    {
        if (span.Length != token.Length)
        {
            return false;
        }
        for (var i = 0; i < span.Length; i++)
        {
            var b = span[i];
            if (b >= (byte)'a' && b <= (byte)'z')
            {
                b = (byte)(b - 32);
            }
            var c = (byte)token[i];
            if (c >= (byte)'a' && c <= (byte)'z')
            {
                c = (byte)(c - 32);
            }
            if (b != c)
            {
                return false;
            }
        }
        return true;
    }

    private static void GetParameters(ReadOnlySpan<char> querySpan, InternalHttpParams parameters)
    {
        while (!querySpan.IsEmpty)
        {
            // 查找下一个键值对
            var ampIndex = querySpan.IndexOf('&');
            var kvSpan = ampIndex >= 0 ? querySpan.Slice(0, ampIndex) : querySpan;

            // 处理有效的非空对
            if (!kvSpan.IsEmpty)
            {
                TouchSocketHttpUtility.ProcessKeyValuePair(kvSpan, parameters);
            }

            // 如果没有更多配对，则退出循环
            if (ampIndex < 0)
            {
                break;
            }

            // 移动到下一对（跳过“&”）
            querySpan = querySpan.Slice(ampIndex + 1);
        }
    }

    private void ParseUrl(ReadOnlySpan<char> url)
    {
        var queryIndex = url.IndexOf('?');
        if (queryIndex >= 0)
        {
            // 提取相对URL和查询部分
            this.m_relativeURL = url.Slice(0, queryIndex).ToString();
            var querySpan = url.Slice(queryIndex + 1);

            // 清除现有查询参数并解析新参数
            this.m_query.Clear();
            GetParameters(querySpan, this.m_query);
        }
        else
        {
            //清除所有现有参数
            this.m_relativeURL = url.ToString();
            this.m_query.Clear();
        }
    }
}