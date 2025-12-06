//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System.Buffers;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
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

    private readonly StringPool m_stringPool = new StringPool(Encoding.UTF8,32);

    private bool m_isChunk;
    private long m_contentLength;

    /// <summary>
    /// 可接受MIME类型
    /// </summary>
    public TextValues Accept
    {
        get => this.m_headers.Get(HttpHeaders.Accept);
        set => this.m_headers.Add(HttpHeaders.Accept, value);
    }

    /// <summary>
    /// 允许编码
    /// </summary>
    public TextValues AcceptEncoding
    {
        get => this.m_headers.Get(HttpHeaders.AcceptEncoding);
        set => this.m_headers.Add(HttpHeaders.AcceptEncoding, value);
    }

    /// <summary>
    /// 获取或设置HTTP内容。
    /// </summary>
    public virtual HttpContent Content { get; set; }

    /// <summary>
    /// 内容填充完成状态
    /// </summary>
    public ContentCompletionStatus ContentStatus { get; protected set; } = ContentCompletionStatus.Unknown;

    /// <summary>
    /// 是否分块
    /// </summary>
    public bool IsChunk
    {
        get => this.m_isChunk;
        set
        {
            this.m_isChunk = value;
            if (value)
            {
                this.Headers.Add(HttpHeaders.TransferEncoding, "chunked");
            }
            else
            {
                this.Headers.Remove(HttpHeaders.TransferEncoding);
            }
        }
    }

    /// <summary>
    /// 内容长度
    /// </summary>
    public long ContentLength
    {
        get => this.m_contentLength;
        set
        {
            this.m_contentLength = value;
            this.m_headers.Add(HttpHeaders.ContentLength, value.ToString());
        }
    }

    /// <summary>
    /// 内容类型
    /// </summary>
    public TextValues ContentType
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
    /// 协议名称，默认HTTP
    /// </summary>
    public Protocol Protocols { get; protected set; } = Protocol.Http;

    /// <summary>
    /// HTTP协议版本，默认1.1
    /// </summary>
    public string ProtocolVersion { get; set; } = "1.1";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool ParsingHeader<TReader>(ref TReader reader) where TReader : IBytesReader
    {
        var sequence = reader.Sequence;
        var index = sequence.IndexOf(TouchSocketHttpUtility.CRLFCRLF);

        if (index < 0)
        {
            return false;
        }

        var headerLength = (int)index;

        // 提前检查数据完整性，避免后续无效操作
        var totalRequired = headerLength + 4; // +4 为 \r\n\r\n 的长度
        if (reader.BytesRemaining < totalRequired)
        {
            return false;
        }

        // 一次性获取头部数据，减少多次调用GetSpan的开销
        if (headerLength < 1024)
        {
            Span<byte> headerSpan = stackalloc byte[headerLength];

            sequence.Slice(0, headerLength).CopyTo(headerSpan);

            // 调用优化后的头部解析方法
            this.ReadHeadersOptimized(headerSpan);
        }
        else
        {
            var headerSpan = reader.GetSpan(headerLength);
            // 调用优化后的头部解析方法
            this.ReadHeadersOptimized(headerSpan);
        }

        // 跳过头部数据和 \r\n\r\n 分隔符
        reader.Advance(totalRequired);
        return true;
    }

    protected internal virtual void Reset()
    {
        this.m_headers.Clear();
        this.ContentStatus = ContentCompletionStatus.Unknown;
        this.m_isChunk = false;
        this.m_contentLength = 0;
    }

    /// <summary>
    /// 读取请求行。
    /// </summary>
    /// <param name="requestLineSpan">包含请求行的只读字节跨度。</param>
    protected abstract void ReadRequestLine(ReadOnlySpan<byte> requestLineSpan);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ParseHeaderLineOptimized(ReadOnlySpan<byte> line)
    {
        var colonIndex = line.IndexOf(TouchSocketHttpUtility.COLON);
        if (colonIndex <= 0)
        {
            return;
        }

        var keySpan = line.Slice(0, colonIndex);
        var valueSpan = TouchSocketHttpUtility.TrimWhitespace(line.Slice(colonIndex + 1));

        if (keySpan.IsEmpty || valueSpan.IsEmpty)
        {
            return;
        }

        if (keySpan.Length == 14 && EqualsIgnoreCaseAscii(keySpan, "Content-Length"u8))
        {
            if (TryParseLong(valueSpan, out var length))
            {
                this.m_contentLength = length;
            }
            var value = this.m_stringPool.Get(valueSpan);
            this.m_headers.AddInternal(HttpHeaders.ContentLength, value);
            return;
        }

        if (keySpan.Length == 17 && EqualsIgnoreCaseAscii(keySpan, "Transfer-Encoding"u8))
        {
            this.m_isChunk = EqualsIgnoreCaseAscii(valueSpan, "chunked"u8);
            var value = this.m_stringPool.Get(valueSpan);
            this.m_headers.AddInternal(HttpHeaders.TransferEncoding, value);
            return;
        }
        
        var key = this.m_stringPool.Get(keySpan);

        var commaCount = 0;
        for (var i = 0; i < valueSpan.Length; i++)
        {
            if (valueSpan[i] == (byte)',')
            {
                commaCount++;
            }
        }

        if (commaCount == 0)
        {
            var value = this.m_stringPool.Get(valueSpan);
            this.m_headers.AddInternal(key, value);
            return;
        }

        var values = new string[commaCount + 1];
        var valueIndex = 0;
        var start = 0;
        
        while (start < valueSpan.Length)
        {
            var nextComma = valueSpan.Slice(start).IndexOf((byte)',');
            ReadOnlySpan<byte> segment;
            
            if (nextComma >= 0)
            {
                segment = valueSpan.Slice(start, nextComma);
                start += nextComma + 1;
            }
            else
            {
                segment = valueSpan.Slice(start);
                start = valueSpan.Length;
            }

            segment = TouchSocketHttpUtility.TrimWhitespace(segment);
            if (!segment.IsEmpty)
            {
                values[valueIndex++] = this.m_stringPool.Get(segment);
            }
        }

        if (valueIndex == 0)
        {
            return;
        }
        
        if (valueIndex == 1)
        {
            this.m_headers.AddInternal(key, values[0]);
        }
        else if (valueIndex < values.Length)
        {
            var trimmedValues = new string[valueIndex];
            Array.Copy(values, trimmedValues, valueIndex);
            this.m_headers.AddInternal(key, trimmedValues);
        }
        else
        {
            this.m_headers.AddInternal(key, values);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool EqualsIgnoreCaseAscii(ReadOnlySpan<byte> span1, ReadOnlySpan<byte> span2)
    {
        if (span1.Length != span2.Length)
        {
            return false;
        }

        for (var i = 0; i < span1.Length; i++)
        {
            var c1 = span1[i];
            var c2 = span2[i];

            if (c1 == c2)
            {
                continue;
            }

            if ((uint)(c1 - 'A') <= 'Z' - 'A')
            {
                c1 = (byte)(c1 | 0x20);
            }

            if ((uint)(c2 - 'A') <= 'Z' - 'A')
            {
                c2 = (byte)(c2 | 0x20);
            }

            if (c1 != c2)
            {
                return false;
            }
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryParseLong(ReadOnlySpan<byte> span, out long result)
    {
        result = 0;
        if (span.IsEmpty)
        {
            return false;
        }

        var sign = 1;
        var index = 0;
        
        if (span[0] == (byte)'-' || span[0] == (byte)'+')
        {
            sign = span[0] == (byte)'-' ? -1 : 1;
            index++;
            if (index >= span.Length)
            {
                return false;
            }
        }

        for (var i = index; i < span.Length; i++)
        {
            var c = span[i];
            if (c < (byte)'0' || c > (byte)'9')
            {
                return false;
            }

            var digit = c - '0';
            result = result * 10 + digit;
        }

        result *= sign;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ReadHeadersOptimized(ReadOnlySpan<byte> span)
    {
        // 解析请求行（首个有效行）
        var lineEnd = span.IndexOf(TouchSocketHttpUtility.CRLF);
        if (lineEnd == -1)
        {
            ThrowHelper.ThrowException("Invalid HTTP header format.");
        }

        // 提取并处理请求行
        var requestLineSpan = span.Slice(0, lineEnd);
        this.ReadRequestLine(requestLineSpan);

        // 跳过请求行及CRLF
        var remaining = span.Slice(lineEnd + 2);

        // 优化的头部解析循环
        while (!remaining.IsEmpty)
        {
            var headerEnd = remaining.IndexOf(TouchSocketHttpUtility.CRLF);

            if (headerEnd == -1)
            {
                // 处理最后一行没有CRLF的情况
                if (!remaining.IsEmpty)
                {
                    this.ParseHeaderLineOptimized(remaining);
                }
                break;
            }

            if (headerEnd == 0)
            {
                // 空行表示headers结束
                break;
            }

            // 提取并解析当前行
            var lineSpan = remaining.Slice(0, headerEnd);
            this.ParseHeaderLineOptimized(lineSpan);

            // 移动到下一行
            remaining = remaining.Slice(headerEnd + 2);
        }
    }

    #region Content

    /// <summary>
    /// 获取一次性内容。
    /// </summary>
    /// <returns>返回一个包含字节的只读内存的任务。</returns>
    /// <param name="cancellationToken">用于取消异步操作的令牌。</param>
    public abstract ValueTask<ReadOnlyMemory<byte>> GetContentAsync(CancellationToken cancellationToken = default);

    #endregion Content

    #region Read

    /// <summary>
    /// 异步读取HTTP块段的内容。
    /// </summary>
    /// <param name="cancellationToken">用于取消异步操作的令牌。</param>
    /// <returns>返回一个<see cref="IReadOnlyMemoryBlockResult"/>，表示异步读取操作的结果。</returns>
    public abstract ValueTask<HttpReadOnlyMemoryBlockResult> ReadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步读取并复制流数据
    /// </summary>
    /// <param name="stream">需要读取并复制的流</param>
    /// <param name="cancellationToken">异步操作的取消令牌</param>
    /// <returns>一个异步任务，表示复制操作的完成</returns>
    public async Task<Result> ReadCopyToAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var flowOperator = new HttpFlowOperator()
        {
            Token = cancellationToken,
            MaxSpeed = int.MaxValue
        };
        return await this.ReadCopyToAsync(stream, flowOperator).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return flowOperator.SetResult(Result.Canceled);
                    }

                    var memory = blockResult.Memory;
                    Debug.WriteLine($"读取块大小：{memory.Length}，时间：{DateTime.Now:HH:mm:ss ffff}");
                    if (!memory.IsEmpty)
                    {

                        await stream.WriteAsync(memory, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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