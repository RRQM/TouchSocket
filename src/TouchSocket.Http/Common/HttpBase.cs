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
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Http;

/// <summary>
/// Http基础头部
/// </summary>
public abstract class HttpBase : IRequestInfo
{
    /// <summary>
    /// 定义缓存的最大大小，这里设置为100MB。
    /// 这个值是根据预期的内存使用量和性能需求确定的。
    /// 过大的缓存可能会导致内存使用率过高，影响系统的其他部分。
    /// 过小的缓存则可能无法有效减少对外部资源的访问，降低程序的运行效率。
    /// </summary>
    public const int MaxCacheSize = 1024 * 1024 * 100;

    /// <summary>
    /// 服务器版本
    /// </summary>
    public static readonly string ServerVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

    private readonly InternalHttpHeader m_headers = new InternalHttpHeader();

    private readonly HttpBlockSegment m_httpBlockSegment = new HttpBlockSegment();

    /// <summary>
    /// 可接受MIME类型
    /// </summary>
    public string Accept
    {
        get => this.m_headers.Get(HttpHeaders.Accept);
        set => this.m_headers.Add(HttpHeaders.Accept, value);
    }

    /// <summary>
    /// 允许编码
    /// </summary>
    public string AcceptEncoding
    {
        get => this.m_headers.Get(HttpHeaders.AcceptEncoding);
        set => this.m_headers.Add(HttpHeaders.AcceptEncoding, value);
    }

    /// <summary>
    /// 获取或设置HTTP内容。
    /// </summary>
    public virtual HttpContent Content { get; set; }

    /// <summary>
    /// 内容填充完成
    /// </summary>
    public bool? ContentCompleted { get; protected set; } = null;

    /// <summary>
    /// 内容长度
    /// </summary>
    public long ContentLength
    {
        get
        {
            var contentLength = this.m_headers.Get(HttpHeaders.ContentLength);
            return contentLength.IsNullOrEmpty() ? 0 : long.TryParse(contentLength, out var value) ? value : 0;
        }
        set => this.m_headers.Add(HttpHeaders.ContentLength, value.ToString());
    }

    /// <summary>
    /// 内容类型
    /// </summary>
    public string ContentType
    {
        get => this.m_headers.Get(HttpHeaders.ContentType);
        set => this.m_headers.Add(HttpHeaders.ContentType, value);
    }

    /// <summary>
    /// 请求头集合
    /// </summary>
    public IHttpHeader Headers => this.m_headers;

    /// <summary>
    /// 是否在Server端工作
    /// </summary>
    public abstract bool IsServer { get; }

    /// <summary>
    /// 保持连接。
    /// <para>
    /// 一般的，当是http1.1时，如果没有显式的Connection: close，即返回true。当是http1.0时，如果没有显式的Connection: Keep-Alive，即返回false。
    /// </para>
    /// </summary>
    public bool KeepAlive
    {
        get
        {
            var keepAlive = this.Headers.Get(HttpHeaders.Connection);
            return this.ProtocolVersion == "1.0"
                ? !keepAlive.IsNullOrEmpty() && keepAlive.Equals("keep-alive", StringComparison.OrdinalIgnoreCase)
                : keepAlive.IsNullOrEmpty() || keepAlive.Equals("keep-alive", StringComparison.OrdinalIgnoreCase);
        }
        set
        {
            if (this.ProtocolVersion == "1.0")
            {
                if (value)
                {
                    this.m_headers.Add(HttpHeaders.Connection, "Keep-Alive");
                }
                else
                {
                    this.m_headers.Add(HttpHeaders.Connection, "close");
                }
            }
            else
            {
                if (!value)
                {
                    this.m_headers.Add(HttpHeaders.Connection, "close");
                }
            }
        }
    }

    /// <summary>
    /// 协议名称，默认HTTP
    /// </summary>
    public Protocol Protocols { get; protected set; } = Protocol.Http;

    /// <summary>
    /// HTTP协议版本，默认1.1
    /// </summary>
    public string ProtocolVersion { get; set; } = "1.1";

    ///// <summary>
    ///// 请求行
    ///// </summary>
    //public string RequestLine { get; private set; }

    internal Task CompleteInput()
    {
        return this.m_httpBlockSegment.InternalComplete(string.Empty);
    }

    internal Task InternalInputAsync(ReadOnlyMemory<byte> memory)
    {
        return this.m_httpBlockSegment.InternalInputAsync(memory);
    }

    internal bool ParsingHeader<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock
    {
        var index = byteBlock.Span.Slice(byteBlock.Position).IndexOf(StringExtension.Default_RNRN_Utf8Span);
        if (index > 0)
        {
            var headerLength = index - byteBlock.Position + 2;
            this.ReadHeaders(byteBlock.Span.Slice(byteBlock.Position, headerLength));
            byteBlock.Position += headerLength;
            byteBlock.Position += 2;
            return true;
        }
        else
        {
            return false;
        }
    }

    internal virtual void ResetHttp()
    {
        this.m_headers.Clear();
        this.ContentCompleted = null;
        //this.RequestLine = default;

        this.m_httpBlockSegment.InternalReset();
    }

    /// <summary>
    /// 读取请求行。
    /// </summary>
    /// <param name="requestLineSpan">包含请求行的只读字节跨度。</param>
    protected abstract void ReadRequestLine(ReadOnlySpan<byte> requestLineSpan);

    private void ParseHeaderLine(ReadOnlySpan<byte> line)
    {
        var colonIndex = line.IndexOf((byte)':');
        if (colonIndex <= 0)
        {
            return; // 无效格式
        }

        // 分割键值
        var keySpan = line.Slice(0, colonIndex).Trim();
        var valueSpan = line.Slice(colonIndex + 1).Trim();

        if (!keySpan.IsEmpty && !valueSpan.IsEmpty)
        {
            string key = keySpan.ToString(Encoding.UTF8).ToLower();
            string value = valueSpan.ToString(Encoding.UTF8);
            this.m_headers[key] = value;
        }
    }

    private void ReadHeaders(ReadOnlySpan<byte> span)
    {
        //string ss=span.ToString(Encoding.UTF8);
        this.m_headers.Clear();

        // 解析请求行（首个有效行）
        var lineEnd = span.IndexOf("\r\n"u8);
        if (lineEnd == -1) // 没有完整请求行
        {
            throw new ArgumentException("Invalid HTTP header format.");
        }

        // 提取请求行
        var requestLineSpan = span.Slice(0, lineEnd);
        ReadRequestLine(requestLineSpan);
        //this.RequestLine = requestLineSpan.ToString(Encoding.UTF8);

        // 跳过请求行及CRLF（+2）
        var remaining = span.Slice(lineEnd + 2);

        // 解析headers
        while (true)
        {
            // 查找当前行结尾
            var headerEnd = remaining.IndexOf("\r\n"u8);
            if (headerEnd == -1)
            {
                break;
            }

            // 空行表示headers结束
            if (headerEnd == 0)
            {
                remaining = remaining.Slice(2); // 跳过空行的CRLF
                break;
            }

            // 提取单行header
            var lineSpan = remaining.Slice(0, headerEnd);
            this.ParseHeaderLine(lineSpan);

            // 移动到下一行
            remaining = remaining.Slice(headerEnd + 2);
        }

        //this.LoadHeaderProperties();
    }

    #region Content

    /// <summary>
    /// 获取一次性内容。
    /// </summary>
    /// <returns>返回一个包含字节的只读内存的任务。</returns>
    /// <param name="cancellationToken">用于取消异步操作的令牌。</param>
    public abstract ValueTask<ReadOnlyMemory<byte>> GetContentAsync(CancellationToken cancellationToken = default);

    internal abstract void InternalSetContent(in ReadOnlyMemory<byte> content);

    #endregion Content

    #region Read

    /// <summary>
    /// 异步读取HTTP块段的内容。
    /// </summary>
    /// <param name="cancellationToken">用于取消异步操作的令牌。</param>
    /// <returns>返回一个<see cref="IReadOnlyMemoryBlockResult"/>，表示异步读取操作的结果。</returns>
    public virtual ValueTask<IReadOnlyMemoryBlockResult> ReadAsync(CancellationToken cancellationToken)
    {
        // 调用m_httpBlockSegment的InternalValueWaitAsync方法，等待HTTP块段的内部值。
        return this.m_httpBlockSegment.InternalValueWaitAsync(cancellationToken);
    }

    /// <summary>
    /// 异步读取并复制流数据
    /// </summary>
    /// <param name="stream">需要读取并复制的流</param>
    /// <param name="cancellationToken">异步操作的取消令牌</param>
    /// <returns>一个异步任务，表示复制操作的完成</returns>
    public async Task ReadCopyToAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        while (true)
        {
            using (var blockResult = await this.ReadAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!blockResult.Memory.Equals(ReadOnlyMemory<byte>.Empty))
                {
                    var memory = blockResult.Memory;
                    await stream.WriteAsync(memory, cancellationToken);
                }
                if (blockResult.IsCompleted)
                {
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 异步读取并复制流数据
    /// </summary>
    /// <param name="stream">需要读取并复制的流</param>
    /// <param name="flowOperator">用于控制流操作的HttpFlowOperator实例</param>
    /// <returns>一个表示操作结果的Result任务</returns>
    public async Task<Result> ReadCopyToAsync(Stream stream, HttpFlowOperator flowOperator)
    {
        var cancellationToken = flowOperator.Token;
        try
        {
            flowOperator.SetLength(this.ContentLength);

            while (true)
            {
                using (var blockResult = await this.ReadAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!blockResult.Memory.Equals(ReadOnlyMemory<byte>.Empty))
                    {
                        var memory = blockResult.Memory;
                        await stream.WriteAsync(memory, cancellationToken);
                        await flowOperator.AddFlowAsync(memory.Length).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    }
                    if (blockResult.IsCompleted)
                    {
                        break;
                    }
                }
            }
            return flowOperator.SetResult(Result.Success);
        }
        catch (OperationCanceledException)
        {
            return flowOperator.SetResult(Result.Canceled);
        }
        catch (Exception ex)
        {
            return flowOperator.SetResult(Result.FromException(ex));
        }
    }

    #endregion Read
}