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

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Http;

/// <summary>
/// HTTP请求定义
/// </summary>
public class HttpRequest : HttpBase
{
    private readonly bool m_isServer;
    private readonly InternalHttpParams m_query = new InternalHttpParams();
    private ReadOnlyMemory<byte> m_contentMemory;
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
        // 初始化时，设置m_isServer为false，表示当前请求不是由服务器发起的。
        this.m_isServer = false;
        // 初始化时，设置m_canRead为false，表示当前请求不能读取数据。
    }

    /// <summary>
    /// 初始化 HttpRequest 实例。
    /// </summary>
    /// <param name="httpClientBase">提供底层 HTTP 通信功能的 HttpClientBase 实例。</param>
    [Obsolete("此构造函数已被弃用，请使用无参构造函数代替", true)]
    public HttpRequest(HttpClientBase httpClientBase)
    {
    }

    internal HttpRequest(HttpSessionClient httpSessionClient)
    {
        this.m_isServer = true;
    }

    /// <inheritdoc/>
    public override HttpContent Content
    {
        get => base.Content;
        set
        {
            if (value is ReadonlyMemoryHttpContent readonlyMemoryHttpContent)
            {
                this.ContentLength = readonlyMemoryHttpContent.Memory.Length;
                this.ContentCompleted = true;
                this.m_contentMemory = readonlyMemoryHttpContent.Memory;
            }
            base.Content = value;
        }
    }

    /// <inheritdoc/>
    public override bool IsServer => this.m_isServer;

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
    public string RelativeURL { get => m_relativeURL; }

    /// <summary>
    /// Url全地址，包含参数
    /// </summary>
    public string URL
    {
        get => m_url;
        set
        {
            // 确保URL以斜杠开始，如果不是，则添加斜杠
            this.m_url = value.StartsWith("/") ? value : $"/{value}";
            // 解析设置后的URL，以进行进一步的操作
            this.ParseUrl(this.m_url.AsSpan());
        }
    }

    /// <inheritdoc/>
    public override async ValueTask<ReadOnlyMemory<byte>> GetContentAsync(CancellationToken cancellationToken = default)
    {
        if (!this.ContentCompleted.HasValue)
        {
            if (!this.IsServer)
            {
                //非Server模式下不允许获取
                return default;
            }
            if (this.ContentLength == 0)
            {
                this.m_contentMemory = ReadOnlyMemory<byte>.Empty;
                this.ContentCompleted = true;
                return this.m_contentMemory;
            }

            if (this.ContentLength > MaxCacheSize)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_MoreThan(nameof(this.ContentLength), this.ContentLength, MaxCacheSize);
            }

            try
            {
                using (var memoryStream = new MemoryStream((int)this.ContentLength))
                {
                    while (true)
                    {
                        using (var blockResult = await this.ReadAsync(cancellationToken))
                        {
                            var segment = blockResult.Memory.GetArray();
                            if (blockResult.IsCompleted)
                            {
                                break;
                            }
                            memoryStream.Write(segment.Array, segment.Offset, segment.Count);
                        }
                    }
                    this.ContentCompleted = true;
                    this.m_contentMemory = memoryStream.ToArray();
                    return this.m_contentMemory;
                }
            }
            catch
            {
                this.ContentCompleted = false;
                return default;
            }
            finally
            {
            }
        }
        else
        {
            return this.ContentCompleted == true ? this.m_contentMemory : default;
        }
    }

    /// <inheritdoc/>
    public override async ValueTask<IBlockResult<byte>> ReadAsync(CancellationToken cancellationToken = default)
    {
        if (this.ContentLength == 0)
        {
            return InternalBlockResult.Completed;
        }
        if (this.ContentCompleted.HasValue && this.ContentCompleted.Value)
        {
            return new InternalBlockResult(this.m_contentMemory, true);
        }

        var blockResult = await base.ReadAsync(cancellationToken);
        if (blockResult.IsCompleted)
        {
            this.ContentCompleted = true;
        }

        return blockResult;
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

    internal void BuildHeader<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
    {
        byteBlock.WriteNormalString(this.Method.ToString(), Encoding.UTF8);//Get
        TouchSocketHttpUtility.AppendSpace(ref byteBlock);//空格
        TouchSocketHttpUtility.AppendUtf8String(ref byteBlock, this.RelativeURL);
        if (this.m_query.Count > 0)
        {
            TouchSocketHttpUtility.AppendQuestionMark(ref byteBlock);
            var i = 0;
            foreach (var item in this.m_query.Keys)
            {
                byteBlock.WriteNormalString(item, Encoding.UTF8);
                TouchSocketHttpUtility.AppendEqual(ref byteBlock);
                var value = this.m_query[item];
                if (value.HasValue())
                {
                    byteBlock.WriteNormalString(Uri.EscapeDataString(value), Encoding.UTF8);
                }

                if (++i < this.m_query.Count)
                {
                    TouchSocketHttpUtility.AppendAnd(ref byteBlock);
                }
            }
        }
        TouchSocketHttpUtility.AppendSpace(ref byteBlock);//空格
        TouchSocketHttpUtility.AppendHTTP(ref byteBlock);//HTTP
        TouchSocketHttpUtility.AppendSlash(ref byteBlock);//斜杠
        byteBlock.WriteNormalString(this.ProtocolVersion, Encoding.UTF8);//1.1
        TouchSocketHttpUtility.AppendRn(ref byteBlock);//换行

        foreach (var headerKey in this.Headers.Keys)
        {
            byteBlock.WriteNormalString(headerKey, Encoding.UTF8);//key
            TouchSocketHttpUtility.AppendColon(ref byteBlock);//冒号
            TouchSocketHttpUtility.AppendSpace(ref byteBlock);//空格
            byteBlock.WriteNormalString(this.Headers[headerKey], Encoding.UTF8);//value
            TouchSocketHttpUtility.AppendRn(ref byteBlock);//换行
        }

        TouchSocketHttpUtility.AppendRn(ref byteBlock);
    }

    /// <inheritdoc/>
    internal override void InternalSetContent(in ReadOnlyMemory<byte> content)
    {
        this.m_contentMemory = content;
        this.ContentLength = content.Length;
        this.ContentCompleted = true;
    }

    /// <inheritdoc/>
    internal override void ResetHttp()
    {
        base.ResetHttp();
        this.m_contentMemory = null;
        //this.m_sentHeader = false;
        this.m_relativeURL = "/";
        this.m_url = "/";
        //this.m_sentLength = 0;
        this.m_query.Clear();
    }

    /// <inheritdoc/>
    protected override void ReadRequestLine(ReadOnlySpan<byte> requestLineSpan)
    {
        var start = 0;

        // 解析 HTTP Method (GET/POST)
        var methodEnd = TouchSocketHttpUtility.FindNextWhitespace(requestLineSpan, start);
        if (methodEnd == -1)
        {
            throw new Exception("Invalid HTTP request line: " + requestLineSpan.ToString(Encoding.UTF8));
        }

        this.Method = new HttpMethod(requestLineSpan.Slice(start, methodEnd - start).ToString(Encoding.UTF8));
        start = TouchSocketHttpUtility.SkipSpaces(requestLineSpan, methodEnd + 1);

        // 解析 URL
        int urlEnd = TouchSocketHttpUtility.FindNextWhitespace(requestLineSpan, start);
        if (urlEnd == -1)
        {
            this.URL = TouchSocketHttpUtility.UnescapeDataString(requestLineSpan.Slice(start));
            return; // No protocol version
        }

        this.URL = TouchSocketHttpUtility.UnescapeDataString(requestLineSpan.Slice(start, urlEnd - start));
        start = TouchSocketHttpUtility.SkipSpaces(requestLineSpan, urlEnd + 1);

        // 解析 Protocol (HTTP/1.1)
        ReadOnlySpan<byte> protocolSpan = requestLineSpan.Slice(start);
        int slashIndex = protocolSpan.IndexOf((byte)'/');
        if (slashIndex > 0 && slashIndex < protocolSpan.Length - 1)
        {
            this.Protocols = new Protocol(protocolSpan.Slice(0, slashIndex).ToString(Encoding.UTF8));
            this.ProtocolVersion = protocolSpan.Slice(slashIndex + 1).ToString(Encoding.UTF8);
        }
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
            m_query.Clear();
        }
    }
}