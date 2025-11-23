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

internal sealed class HttpServerDataHandlingAdapter : SingleStreamDataHandlingAdapter
{
    public HttpServerDataHandlingAdapter(Func<ServerHttpRequest, Task> func)
    {
        this.m_func = func;
    }
    private ServerHttpRequest m_currentRequest;
    private ServerHttpRequest m_requestRoot;
    private long m_surLen;
    private Task m_task;

    // chunked 解析相关状态
    private bool m_isChunked;
    private ChunkedState m_chunkedState;

    /// <summary>
    /// Chunked 传输状态枚举
    /// </summary>
    private enum ChunkedState
    {
        /// <summary>
        /// 等待块大小
        /// </summary>
        WaitingChunkSize,
        /// <summary>
        /// 等待块数据
        /// </summary>
        WaitingChunkData,
        /// <summary>
        /// 等待块结束标记
        /// </summary>
        WaitingChunkEnd,
        /// <summary>
        /// 等待尾部标记
        /// </summary>
        WaitingTrailer,
        /// <summary>
        /// 完成
        /// </summary>
        Completed
    }

    /// <inheritdoc/>
    public override void OnLoaded(object owner)
    {
        if (owner is not HttpSessionClient httpSessionClient)
        {
            throw new Exception($"此适配器必须适用于{nameof(IHttpService)}");
        }

        this.m_requestRoot = new ServerHttpRequest(httpSessionClient);
        base.OnLoaded(owner);
    }

    /// <inheritdoc/>
    protected override async Task PreviewReceivedAsync<TReader>(TReader reader)
    {
        while (reader.BytesRemaining > 0)
        {
            if (this.DisposedValue)
            {
                return;
            }

            if (this.m_currentRequest == null)
            {
                if (this.m_task != null)
                {
                    await this.m_task.ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    this.m_task = null;
                }

                this.m_requestRoot.Reset();
                this.m_currentRequest = this.m_requestRoot;
                if (this.m_currentRequest.ParsingHeader(ref reader))
                {

                    var contentLength = this.m_currentRequest.ContentLength;
                    if (contentLength == 0)
                    {
                        // 没有具体长度时，才检查是否为 chunked 传输
                        this.m_isChunked = this.m_currentRequest.IsChunk;
                    }

                    if (this.m_isChunked)
                    {
                        // 初始化 chunked 状态
                        this.m_chunkedState = ChunkedState.WaitingChunkSize;
                        this.m_task = this.GoReceivedAsync(this.m_currentRequest);
                    }
                    else if (contentLength > reader.BytesRemaining)
                    {
                        this.m_surLen = contentLength;
                        this.m_task = this.GoReceivedAsync(this.m_currentRequest);
                    }
                    else
                    {
                        if (contentLength > 0)
                        {
                            var content = reader.GetMemory((int)contentLength);
                            reader.Advance((int)contentLength);
                            this.m_currentRequest.InternalSetContent(content);
                        }
                        else
                        {
                            this.m_currentRequest.InternalSetContent(ReadOnlyMemory<byte>.Empty);
                        }
                        await this.GoReceivedAsync(this.m_currentRequest);
                        this.m_currentRequest = null;
                    }
                }
                else
                {
                    this.m_currentRequest = null;
                    return;
                }
            }
            else if (this.m_isChunked)
            {
                // 处理 chunked 数据
                var result = await this.ProcessChunkedDataAsync(reader).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                if (result == ChunkedProcessResult.NeedMoreData)
                {
                    return; // 需要更多数据
                }
                else if (result == ChunkedProcessResult.Completed)
                {
                    // chunked 传输完成
                    this.m_currentRequest.CompleteInput();
                    this.ResetChunkedState();
                }
                // Continue 表示继续处理
            }
            else if (this.m_surLen > 0)
            {
                if (reader.BytesRemaining > 0)
                {
                    var len = (int)Math.Min(this.m_surLen, reader.BytesRemaining);

                    await this.m_currentRequest.InternalInputAsync(reader.GetMemory(len)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    this.m_surLen -= len;
                    reader.Advance(len);
                    if (this.m_surLen == 0)
                    {
                        this.m_currentRequest.CompleteInput();
                        this.m_currentRequest = null;
                    }
                }
            }
            else
            {
                this.m_currentRequest = null;
            }
        }
    }

    private Task GoReceivedAsync(ServerHttpRequest request)
    {
        return this.m_func.Invoke(request);
    }

    /// <summary>
    /// Chunked 处理结果枚举
    /// </summary>
    private enum ChunkedProcessResult
    {
        /// <summary>
        /// 需要更多数据
        /// </summary>
        NeedMoreData,
        /// <summary>
        /// 继续处理
        /// </summary>
        Continue,
        /// <summary>
        /// 完成
        /// </summary>
        Completed
    }

    /// <summary>
    /// 处理 chunked 传输数据
    /// </summary>
    /// <typeparam name="TReader">字节读取器类型</typeparam>
    /// <param name="reader">字节读取器</param>
    /// <returns>处理结果</returns>
    private async ValueTask<ChunkedProcessResult> ProcessChunkedDataAsync<TReader>(TReader reader) where TReader : class, IBytesReader
    {
        switch (this.m_chunkedState)
        {
            case ChunkedState.WaitingChunkSize:
                return await this.ProcessChunkSizeAsync(reader).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            case ChunkedState.WaitingChunkData:
                return await this.ProcessChunkDataAsync(reader).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            case ChunkedState.WaitingChunkEnd:
                return this.ProcessChunkEnd(reader);

            case ChunkedState.WaitingTrailer:
                return this.ProcessTrailer(reader);

            case ChunkedState.Completed:
                return ChunkedProcessResult.Completed;

            default:
                throw new InvalidOperationException($"未知的 chunked 状态: {this.m_chunkedState}");
        }
    }

    private long m_expectedChunkSize;
    private readonly Func<ServerHttpRequest, Task> m_func;

    /// <summary>
    /// 处理块大小
    /// </summary>
    private async ValueTask<ChunkedProcessResult> ProcessChunkSizeAsync<TReader>(TReader reader) where TReader : class, IBytesReader
    {
        // 查找 CRLF
        var crlfIndex = FindCRLF(reader);
        if (crlfIndex < 0)
        {
            return ChunkedProcessResult.NeedMoreData;
        }

        // 解析块大小
        var chunkSizeSpan = reader.GetSpan(crlfIndex);
        if (!TryParseChunkSize(chunkSizeSpan, out var chunkSize))
        {
            throw new InvalidOperationException("无效的 chunk 大小格式");
        }

        this.m_expectedChunkSize = chunkSize;

        // 跳过块大小行和 CRLF
        reader.Advance(crlfIndex + 2);

        if (chunkSize == 0)
        {
            // 最后一个块
            this.m_chunkedState = ChunkedState.WaitingTrailer;
        }
        else
        {
            // 有数据的块
            this.m_chunkedState = ChunkedState.WaitingChunkData;
        }

        return ChunkedProcessResult.Continue;
    }

    /// <summary>
    /// 处理块数据
    /// </summary>
    private async ValueTask<ChunkedProcessResult> ProcessChunkDataAsync<TReader>(TReader reader) where TReader : class, IBytesReader
    {
        if (reader.BytesRemaining < this.m_expectedChunkSize)
        {
            return ChunkedProcessResult.NeedMoreData;
        }

        // 读取块数据
        var chunkData = reader.GetMemory((int)this.m_expectedChunkSize);
        await this.m_currentRequest.InternalInputAsync(chunkData).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        reader.Advance((int)this.m_expectedChunkSize);

        this.m_chunkedState = ChunkedState.WaitingChunkEnd;
        return ChunkedProcessResult.Continue;
    }

    /// <summary>
    /// 处理块结束标记
    /// </summary>
    private ChunkedProcessResult ProcessChunkEnd<TReader>(TReader reader) where TReader : IBytesReader
    {
        if (!TrySkipCRLF(reader))
        {
            return ChunkedProcessResult.NeedMoreData;
        }

        // 准备处理下一个块
        this.m_chunkedState = ChunkedState.WaitingChunkSize;
        return ChunkedProcessResult.Continue;
    }

    /// <summary>
    /// 处理尾部标记
    /// </summary>
    private ChunkedProcessResult ProcessTrailer<TReader>(TReader reader) where TReader : IBytesReader
    {
        // 检查是否有足够的数据处理 trailer
        if (reader.BytesRemaining < 2)
        {
            return ChunkedProcessResult.NeedMoreData;
        }

        // 检查是否直接是最终的 CRLF
        var span = reader.GetSpan(2);
        if (span[0] == '\r' && span[1] == '\n')
        {
            reader.Advance(2);
            this.m_chunkedState = ChunkedState.Completed;
            return ChunkedProcessResult.Completed;
        }

        // 处理可能的 trailer headers
        while (reader.BytesRemaining > 0)
        {
            var crlfIndex = FindCRLF(reader);
            if (crlfIndex < 0)
            {
                return ChunkedProcessResult.NeedMoreData;
            }

            if (crlfIndex == 0)
            {
                // 空行，表示 trailer 结束
                reader.Advance(2);
                this.m_chunkedState = ChunkedState.Completed;
                return ChunkedProcessResult.Completed;
            }

            // 跳过 trailer header 行
            reader.Advance(crlfIndex + 2);
        }

        return ChunkedProcessResult.NeedMoreData;
    }

    /// <summary>
    /// 在读取器中查找 CRLF 的位置
    /// </summary>
    /// <typeparam name="TReader">字节读取器类型</typeparam>
    /// <param name="reader">字节读取器</param>
    /// <returns>CRLF 的位置，如果未找到则返回 -1</returns>
    private static int FindCRLF<TReader>(TReader reader) where TReader : IBytesReader
    {
        const int maxSearchLength = 256; // 限制搜索长度，防止恶意数据
        var searchLength = (int)Math.Min(reader.BytesRemaining, maxSearchLength);

        if (searchLength < 2)
        {
            return -1;
        }

        var sequence = reader.Sequence.Slice(0, searchLength);

        // 使用 ReadOnlySequence 的 IndexOf 方法查找 CRLF
        var crlfIndex = sequence.IndexOf(TouchSocketHttpUtility.CRLF);
        return crlfIndex >= 0 ? (int)crlfIndex : -1;
    }

    /// <summary>
    /// 尝试解析十六进制块大小
    /// </summary>
    /// <param name="hexSpan">十六进制字节跨度</param>
    /// <param name="chunkSize">解析出的块大小</param>
    /// <returns>true 如果解析成功</returns>
    private static bool TryParseChunkSize(ReadOnlySpan<byte> hexSpan, out long chunkSize)
    {
        chunkSize = 0;

        if (hexSpan.Length == 0)
        {
            return false;
        }

        for (var i = 0; i < hexSpan.Length; i++)
        {
            var b = hexSpan[i];

            // 检查是否遇到了块扩展分隔符或空格
            if (b == ';' || b == ' ' || b == '\t')
            {
                break; // 忽略块扩展和空格
            }

            int digit;
            if (b >= '0' && b <= '9')
            {
                digit = b - '0';
            }
            else if (b >= 'A' && b <= 'F')
            {
                digit = b - 'A' + 10;
            }
            else if (b >= 'a' && b <= 'f')
            {
                digit = b - 'a' + 10;
            }
            else
            {
                return false; // 无效字符
            }

            // 检查溢出
            if (chunkSize > (long.MaxValue - digit) / 16)
            {
                return false;
            }

            chunkSize = chunkSize * 16 + digit;
        }

        return true;
    }

    /// <summary>
    /// 尝试跳过 CRLF
    /// </summary>
    /// <typeparam name="TReader">字节读取器类型</typeparam>
    /// <param name="reader">字节读取器</param>
    /// <returns>true 如果成功跳过</returns>
    private static bool TrySkipCRLF<TReader>(TReader reader) where TReader : IBytesReader
    {
        if (reader.BytesRemaining >= 2)
        {
            var span = reader.GetSpan(2);
            if (span[0] == '\r' && span[1] == '\n')
            {
                reader.Advance(2);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 重置 chunked 状态
    /// </summary>
    private void ResetChunkedState()
    {
        this.m_isChunked = false;
        this.m_chunkedState = ChunkedState.WaitingChunkSize;
        this.m_expectedChunkSize = 0;
        this.m_currentRequest = null;
    }
}