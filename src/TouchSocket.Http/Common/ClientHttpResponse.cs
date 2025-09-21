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

using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Http;

internal sealed class ClientHttpResponse : HttpResponse
{
    private readonly HttpClientBase m_httpClientBase;
    private long m_bytesRead = 0;
    private ByteBlock m_contentByteBlock;
    private ReadOnlyMemory<byte> m_contentMemory;
    private bool m_isContentReadingStarted = false;
   
    internal ClientHttpResponse(HttpClientBase httpClientBase) : base(httpClientBase)
    {
        this.m_httpClientBase = httpClientBase;
    }

    /// <inheritdoc/>
    public override async ValueTask<ReadOnlyMemory<byte>> GetContentAsync(CancellationToken cancellationToken = default)
    {
        if (this.ContentStatus == ContentCompletionStatus.Unknown)
        {
            var contentLength = this.ContentLength;
            if (!this.IsChunk && contentLength == 0)
            {
                this.m_contentMemory = ReadOnlyMemory<byte>.Empty;
                return this.m_contentMemory;
            }

            if (!this.IsChunk && contentLength > MaxCacheSize)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_MoreThan(nameof(contentLength), contentLength, MaxCacheSize);
            }

            try
            {
                var byteBlock = new ByteBlock((int)contentLength);

                while (true)
                {
                    using (var blockResult = await this.ReadAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
                    {
                        byteBlock.Write(blockResult.Memory.Span);
                        if (blockResult.IsCompleted)
                        {
                            break;
                        }
                    }

                    if (byteBlock.Length > MaxCacheSize)
                    {
                        ThrowHelper.ThrowArgumentOutOfRangeException_MoreThan(nameof(contentLength), contentLength, MaxCacheSize);
                    }
                }
                this.InternalSetContent(byteBlock);
                return this.m_contentMemory;
            }
            catch
            {
                this.ContentStatus = ContentCompletionStatus.Incomplete;
                this.m_contentMemory = null;
                return this.m_contentMemory;
            }
        }
        else
        {
            return this.ContentStatus == ContentCompletionStatus.ContentCompleted ? this.m_contentMemory : default;
        }
    }

    public override async ValueTask<HttpReadOnlyMemoryBlockResult> ReadAsync(CancellationToken cancellationToken = default)
    {
        if (this.ContentStatus == ContentCompletionStatus.ContentCompleted)
        {
            // 已经完成读取
            return new HttpReadOnlyMemoryBlockResult(default, this.m_contentMemory, true);
        }
        if (this.ContentStatus == ContentCompletionStatus.ReadCompleted)
        {
            ThrowHelper.ThrowInvalidOperationException("内容已读取完毕。");
        }
        if (this.ContentLength == 0 && !this.IsChunk)
        {
            return HttpReadOnlyMemoryBlockResult.Completed;
        }

        // 获取Transport和Reader
        var transport = this.m_httpClientBase.InternalTransport;
        var reader = transport.Reader;

        // 如果尚未开始读取内容，首先需要处理可能在头部解析过程中缓存的数据
        if (!this.m_isContentReadingStarted)
        {
            this.m_isContentReadingStarted = true;
        }

        // 根据传输类型处理内容
        if (this.IsChunk)
        {
            return await this.ReadChunkedContentAsync(reader, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        else if (this.ContentLength > 0)
        {
            return await this.ReadFixedLengthContentAsync(reader, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }

        return HttpReadOnlyMemoryBlockResult.Completed;
    }

    public async ValueTask<bool> ReadHeader(CancellationToken cancellationToken)
    {
        var reader = this.m_httpClientBase.InternalTransport.Reader;
        // 解析HTTP响应头
        while (true)
        {
            var readResult = await reader.ReadAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            var buffer = readResult.Buffer;
            var bytesReader = new BytesReader(buffer);
            try
            {
                // 解析HTTP响应头
                if (this.ParsingHeader(ref bytesReader))
                {
                    // 头部解析完成，计算消费的数据量
                    var consumedBytes = bytesReader.BytesRead;

                    // 如果没有内容或内容长度为0，直接返回
                    if (this.ContentLength == 0 && !this.IsChunk)
                    {
                        this.InternalSetContent(ReadOnlySequence<byte>.Empty);
                        reader.AdvanceTo(buffer.GetPosition(consumedBytes));
                        return true;
                    }

                    // 检查是否还有足够的数据来读取完整内容
                    var remainingBuffer = buffer.Slice(buffer.GetPosition(consumedBytes));
                    if (!this.IsChunk && remainingBuffer.Length >= this.ContentLength)
                    {
                        // 有足够数据读取完整内容
                        var contentBuffer = remainingBuffer.Slice(0, (int)this.ContentLength);
                        this.InternalSetContent(contentBuffer);

                        // 推进读取位置
                        reader.AdvanceTo(buffer.GetPosition(consumedBytes + this.ContentLength));
                        return true;
                    }

                    // 需要继续读取内容，推进到头部结束位置
                    reader.AdvanceTo(buffer.GetPosition(consumedBytes));
                    break;
                }
                else
                {
                    // 头部未完成，需要更多数据
                    if (readResult.IsCompleted)
                    {
                        throw new InvalidOperationException("连接意外关闭，HTTP响应头不完整");
                    }
                    reader.AdvanceTo(buffer.Start, buffer.End);
                }
            }
            catch
            {
                reader.AdvanceTo(buffer.Start);
                throw;
            }
            finally
            {
                bytesReader.Dispose();
            }
        }

        return false;
    }

    protected internal override void Reset()
    {
        m_contentByteBlock?.Dispose();
        m_contentByteBlock = null;
        this.m_contentMemory = null;
        this.m_bytesRead = 0;
        this.m_isContentReadingStarted = false;
        base.Reset();
    }

    /// <summary>
    /// 解析分块传输的块大小
    /// </summary>
    /// <param name="hexBytes">十六进制字节数据</param>
    /// <param name="chunkSize">解析出的块大小</param>
    /// <returns>是否解析成功</returns>
    private static bool TryParseChunkSize(ReadOnlySpan<byte> hexBytes, out int chunkSize)
    {
        chunkSize = 0;

        if (hexBytes.Length == 0)
        {
            return false;
        }

        for (var i = 0; i < hexBytes.Length; i++)
        {
            var b = hexBytes[i];
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
            else if (b == ';')
            {
                // 块扩展，忽略分号后的内容
                break;
            }
            else
            {
                return false; // 无效字符
            }

            // 检查溢出
            if (chunkSize > (int.MaxValue - digit) / 16)
            {
                return false;
            }

            chunkSize = chunkSize * 16 + digit;
        }

        return true;
    }

    /// <summary>
    /// 读取分块传输的内容
    /// </summary>
    /// <param name="reader">管道读取器</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>HTTP读取结果</returns>
    private async ValueTask<HttpReadOnlyMemoryBlockResult> ReadChunkedContentAsync(PipeReader reader, CancellationToken cancellationToken)
    {
        while (true)
        {
            var readResult = await reader.ReadAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            var buffer = readResult.Buffer;

            try
            {
                var examined = buffer.Start;
                var consumed = buffer.Start;

                // 查找块大小行结束符
                var crlfIndex = buffer.IndexOf(TouchSocketHttpUtility.CRLF);
                if (crlfIndex < 0)
                {
                    // 没有找到CRLF，需要更多数据
                    if (readResult.IsCompleted)
                    {
                        ThrowHelper.ThrowInvalidOperationException("连接意外关闭，分块传输不完整");
                    }
                    reader.AdvanceTo(consumed, buffer.End);
                    continue;
                }

                // 解析块大小
                var chunkSizeSlice = buffer.Slice(0, crlfIndex);

                using var memoryBuffer = new ContiguousMemoryBuffer(chunkSizeSlice);
                var chunkSizeSpan = memoryBuffer.Memory.Span;

                if (!TryParseChunkSize(chunkSizeSpan, out var chunkSize))
                {
                    ThrowHelper.ThrowInvalidOperationException("无效的块大小格式");
                }

                // 跳过块大小和第一个CRLF
                var afterChunkSizePosition = buffer.GetPosition(crlfIndex + 2);
                var remainingBuffer = buffer.Slice(afterChunkSizePosition);

                if (chunkSize == 0)
                {
                    // 最后一个块，跳过可能的尾部CRLF
                    if (remainingBuffer.Length >= 2)
                    {
                        consumed = buffer.GetPosition(2, afterChunkSizePosition);
                    }
                    else
                    {
                        consumed = afterChunkSizePosition;
                    }
                    reader.AdvanceTo(consumed);
                    this.ContentStatus = ContentCompletionStatus.ReadCompleted;
                    return HttpReadOnlyMemoryBlockResult.Completed;
                }

                // 检查是否有足够的数据读取完整块（块数据 + 尾部CRLF）
                if (remainingBuffer.Length < chunkSize + 2) // +2 for trailing CRLF
                {
                    // 数据不足，需要等待更多数据
                    if (readResult.IsCompleted)
                    {
                        ThrowHelper.ThrowInvalidOperationException("连接意外关闭，分块传输不完整");
                    }
                    reader.AdvanceTo(consumed, buffer.End);
                    continue;
                }

                // 读取块数据
                var chunkDataSlice = remainingBuffer.Slice(0, chunkSize);

                var result = new HttpReadOnlyMemoryBlockResult(chunkDataSlice, false);

                // 计算消费位置（块大小行 + 第一个CRLF + 块数据 + 尾部CRLF）
                consumed = buffer.GetPosition(chunkSize + 2, afterChunkSizePosition);
                reader.AdvanceTo(consumed);

                return result;
            }
            catch
            {
                reader.AdvanceTo(buffer.Start);
                throw;
            }
        }
    }

    /// <summary>
    /// 读取固定长度的内容
    /// </summary>
    /// <param name="reader">管道读取器</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>HTTP读取结果</returns>
    private async ValueTask<HttpReadOnlyMemoryBlockResult> ReadFixedLengthContentAsync(PipeReader reader, CancellationToken cancellationToken)
    {
        var totalBytesToRead = this.ContentLength;

        if (this.m_bytesRead >= totalBytesToRead)
        {
            this.ContentStatus = ContentCompletionStatus.ReadCompleted;
            return HttpReadOnlyMemoryBlockResult.Completed;
        }

        var readResult = await reader.ReadAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        var buffer = readResult.Buffer;

        if (buffer.IsEmpty && readResult.IsCompleted)
        {
            ThrowHelper.ThrowInvalidOperationException("连接意外关闭，内容读取不完整");
        }

        var remainingBytes = totalBytesToRead - this.m_bytesRead;
        var bytesToRead =Math.Min( Math.Min(remainingBytes, buffer.Length), TouchSocketHttpUtility.MaxReadSize);

        var contentSlice = buffer.Slice(0, bytesToRead);

        this.m_bytesRead += bytesToRead;
        HttpReadOnlyMemoryBlockResult result;
        // 检查是否已读取完所有内容
        if (this.m_bytesRead >= totalBytesToRead)
        {
            this.ContentStatus = ContentCompletionStatus.ReadCompleted;
            result= new HttpReadOnlyMemoryBlockResult(contentSlice, true);
        }
        else
        {
            result= new HttpReadOnlyMemoryBlockResult(contentSlice, false);
        }
        reader.AdvanceTo(buffer.GetPosition(bytesToRead));

        return result;
    }

    internal void InternalSetContent(ReadOnlySequence<byte> content)
    {
        if (content.IsEmpty)
        {
            this.m_contentMemory = ReadOnlyMemory<byte>.Empty;
            this.ContentLength = 0;
            this.ContentStatus = ContentCompletionStatus.ContentCompleted;
            return;
        }
        m_contentByteBlock = new ByteBlock((int)content.Length);

        foreach (var item in content)
        {
            m_contentByteBlock.Write(item.Span);
        }
        this.m_contentMemory = m_contentByteBlock.Memory;
        this.ContentLength = content.Length;
        this.ContentStatus = ContentCompletionStatus.ContentCompleted;
    }

    internal void InternalSetContent(ByteBlock byteBlock)
    {
        m_contentByteBlock = byteBlock;
        this.m_contentMemory = m_contentByteBlock.Memory;
        this.ContentLength = byteBlock.Length;
        this.ContentStatus = ContentCompletionStatus.ContentCompleted;
    }
}