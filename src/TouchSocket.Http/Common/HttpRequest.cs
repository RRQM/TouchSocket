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
    public string RelativeURL { get; private set; } = "/";

    /// <summary>
    /// Url全地址，包含参数
    /// </summary>
    public string URL { get; private set; } = "/";

    /// <inheritdoc/>
    public override bool IsServer => this.m_isServer;

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

    /// <inheritdoc/>
    internal override void InternalSetContent(in ReadOnlyMemory<byte> content)
    {
        this.m_contentMemory = content;
        this.ContentLength = content.Length;
        this.ContentCompleted = true;
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

    /// <summary>
    /// 设置Url，可带参数
    /// </summary>
    /// <param name="url">要设置的URL地址</param>
    /// <returns>返回当前HttpRequest实例，支持链式调用</returns>
    public HttpRequest SetUrl(string url)
    {
        // 确保URL以斜杠开始，如果不是，则添加斜杠
        this.URL = url.StartsWith("/") ? url : $"/{url}";
        // 解析设置后的URL，以进行进一步的操作
        this.ParseUrl();
        // 返回当前实例，支持链式调用
        return this;
    }

    /// <inheritdoc/>
    internal override void ResetHttp()
    {
        base.ResetHttp();
        this.m_contentMemory = null;
        //this.m_sentHeader = false;
        this.RelativeURL = "/";
        this.URL = "/";
        //this.m_sentLength = 0;
        this.m_query.Clear();
    }

    /// <inheritdoc/>
    protected override void LoadHeaderProperties()
    {
        var first = Regex.Split(this.RequestLine, @"(\s+)").Where(e => e.Trim() != string.Empty).ToArray();
        if (first.Length > 0)
        {
            this.Method = new HttpMethod(first[0].Trim());
        }

        if (first.Length > 1)
        {
            this.SetUrl(Uri.UnescapeDataString(first[1]));
        }
        if (first.Length > 2)
        {
            var ps = first[2].Split('/');
            if (ps.Length == 2)
            {
                this.Protocols = new Protocol(ps[0]);
                this.ProtocolVersion = ps[1];
            }
        }
    }

    private static void GetParameters(string row, in InternalHttpParams pairs)
    {
        if (string.IsNullOrEmpty(row))
        {
            return;
        }

        var kvs = row.Split('&');
        if (kvs == null || kvs.Length == 0)
        {
            return;
        }

        foreach (var item in kvs)
        {
            var kv = item.SplitFirst('=');
            if (kv.Length == 2)
            {
                pairs.AddOrUpdate(kv[0], kv[1]);
            }
        }
    }

    internal void BuildHeader<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
    {
        //var stringBuilder = new StringBuilder();

        //string url = null;
        //if (!string.IsNullOrEmpty(this.RelativeURL))
        //{
        //    if (this.m_query.Count == 0)
        //    {
        //        url = this.RelativeURL;
        //    }
        //    else
        //    {
        //        var urlBuilder = new StringBuilder();
        //        urlBuilder.Append(this.RelativeURL);
        //        urlBuilder.Append('?');
        //        var i = 0;
        //        foreach (var item in this.m_query.Keys)
        //        {
        //            urlBuilder.Append($"{item}={Uri.EscapeDataString(this.m_query[item])}");
        //            if (++i < this.m_query.Count)
        //            {
        //                urlBuilder.Append('&');
        //            }
        //        }
        //        url = urlBuilder.ToString();
        //    }
        //}

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

        //if (string.IsNullOrEmpty(url))
        //{
        //    stringBuilder.Append($"{this.Method} / HTTP/{this.ProtocolVersion}\r\n");
        //}
        //else
        //{
        //    stringBuilder.Append($"{this.Method} {url} HTTP/{this.ProtocolVersion}\r\n");
        //}

        //foreach (var headerKey in this.Headers.Keys)
        //{
        //    stringBuilder.Append($"{headerKey}: ");
        //    stringBuilder.Append(this.Headers[headerKey] + "\r\n");
        //}

        //stringBuilder.Append("\r\n");
        //byteBlock.Write(Encoding.UTF8.GetBytes(stringBuilder.ToString()));
    }

    
    private void ParseUrl()
    {
        if (this.URL.Contains('?'))
        {
            var urls = this.URL.Split('?');
            if (urls.Length > 0)
            {
                this.RelativeURL = urls[0];
            }
            if (urls.Length > 1)
            {
                this.m_query.Clear();
                GetParameters(urls[1], this.m_query);
            }
        }
        else
        {
            this.RelativeURL = this.URL;
        }
    }
}