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

using System.Buffers;
using System.IO.Pipelines;
using TouchSocket.Sockets;

namespace TouchSocket.Http;

internal sealed class ClientHttpResponse : HttpResponse
{
    private readonly HttpClientBase m_httpClientBase;
    private long m_bytesRead = 0;
    private ByteBlock m_contentByteBlock;
    private ReadOnlyMemory<byte> m_contentMemory;
    private bool m_isContentReadingStarted = false;
    private int m_contentReadPosition = 0;
    private long m_chunkDataRemaining = 0;
    private bool m_needSkipCRLF = false;

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
                using var bufferOwner = MemoryPool<byte>.Shared.Rent(8192);
                var rentedBuffer = bufferOwner.Memory;

                while (true)
                {
                    var read = await this.ReadAsync(rentedBuffer, cancellationToken).ConfigureDefaultAwait();
                    if (read == 0)
                    {
                        break;
                    }

                    byteBlock.Write(rentedBuffer.Slice(0, read).Span);

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

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (buffer.IsEmpty)
        {
            return EasyValueTask.FromResult(0);
        }

        if (this.ContentStatus == ContentCompletionStatus.ContentCompleted)
        {
            var remaining = this.m_contentMemory.Length - this.m_contentReadPosition;
            if (remaining <= 0)
            {
                return EasyValueTask.FromResult(0);
            }
            var toCopy = Math.Min(buffer.Length, remaining);
            this.m_contentMemory.Slice(this.m_contentReadPosition, toCopy).CopyTo(buffer);
            this.m_contentReadPosition += toCopy;
            return EasyValueTask.FromResult(toCopy);
        }

        if (this.ContentStatus == ContentCompletionStatus.ReadCompleted)
        {
            return EasyValueTask.FromResult(0);
        }

        if (this.ContentLength == 0 && !this.IsChunk)
        {
            this.ContentStatus = ContentCompletionStatus.ReadCompleted;
            return EasyValueTask.FromResult(0);
        }

        var transport = this.m_httpClientBase.InternalTransport;
        var reader = transport.Reader;

        if (!this.m_isContentReadingStarted)
        {
            this.m_isContentReadingStarted = true;
        }

        if (this.IsChunk)
        {
            return this.ReadChunkedContentAsync(reader, buffer, cancellationToken);
        }
        else if (this.ContentLength > 0)
        {
            return this.ReadFixedLengthContentAsync(reader, buffer, cancellationToken);
        }

        this.ContentStatus = ContentCompletionStatus.ReadCompleted;
        return EasyValueTask.FromResult(0);
    }

    public async ValueTask<bool> ReadHeader(CancellationToken cancellationToken)
    {
        var reader = this.m_httpClientBase.InternalTransport.Reader;
        // 解析HTTP响应头
        while (true)
        {
            var readResult = await reader.ReadAsync(cancellationToken).ConfigureDefaultAwait();
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
        this.m_contentByteBlock?.Dispose();
        this.m_contentByteBlock = null;
        this.m_contentMemory = null;
        this.m_bytesRead = 0;
        this.m_isContentReadingStarted = false;
        this.m_contentReadPosition = 0;
        this.m_chunkDataRemaining = 0;
        this.m_needSkipCRLF = false;
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
    private async ValueTask<int> ReadChunkedContentAsync(PipeReader reader, Memory<byte> buffer, CancellationToken cancellationToken)
    {
        while (true)
        {
            if (this.m_chunkDataRemaining > 0)
            {
                var readResult = await reader.ReadAsync(cancellationToken).ConfigureDefaultAwait();
                var pipeBuffer = readResult.Buffer;

                if (pipeBuffer.IsEmpty && readResult.IsCompleted)
                {
                    ThrowHelper.ThrowInvalidOperationException("连接意外关闭，分块传输不完整");
                }

                if (pipeBuffer.IsEmpty)
                {
                    reader.AdvanceTo(pipeBuffer.Start, pipeBuffer.End);
                    continue;
                }

                var toRead = (int)Math.Min(Math.Min(this.m_chunkDataRemaining, pipeBuffer.Length), buffer.Length);
                pipeBuffer.Slice(0, toRead).CopyTo(buffer.Span);
                this.m_chunkDataRemaining -= toRead;

                if (this.m_chunkDataRemaining == 0)
                {
                    var afterData = pipeBuffer.Slice(toRead);
                    if (afterData.Length >= 2)
                    {
                        reader.AdvanceTo(pipeBuffer.GetPosition(toRead + 2));
                    }
                    else
                    {
                        reader.AdvanceTo(pipeBuffer.GetPosition(toRead));
                        this.m_needSkipCRLF = true;
                    }
                }
                else
                {
                    reader.AdvanceTo(pipeBuffer.GetPosition(toRead));
                }

                return toRead;
            }

            if (this.m_needSkipCRLF)
            {
                var crlfResult = await reader.ReadAsync(cancellationToken).ConfigureDefaultAwait();
                var crlfBuffer = crlfResult.Buffer;

                if (crlfBuffer.Length < 2)
                {
                    if (crlfResult.IsCompleted)
                    {
                        ThrowHelper.ThrowInvalidOperationException("连接意外关闭，分块传输不完整");
                    }
                    reader.AdvanceTo(crlfBuffer.Start, crlfBuffer.End);
                    continue;
                }

                reader.AdvanceTo(crlfBuffer.GetPosition(2));
                this.m_needSkipCRLF = false;
                continue;
            }

            var headerResult = await reader.ReadAsync(cancellationToken).ConfigureDefaultAwait();
            var headerBuffer = headerResult.Buffer;

            var crlfIndex = headerBuffer.IndexOf(TouchSocketHttpUtility.CRLF);
            if (crlfIndex < 0)
            {
                if (headerResult.IsCompleted)
                {
                    ThrowHelper.ThrowInvalidOperationException("连接意外关闭，分块传输不完整");
                }
                reader.AdvanceTo(headerBuffer.Start, headerBuffer.End);
                continue;
            }

            var chunkSizeSlice = headerBuffer.Slice(0, crlfIndex);
            using var memoryBuffer = new ContiguousMemoryBuffer(chunkSizeSlice);
            if (!TryParseChunkSize(memoryBuffer.Memory.Span, out var chunkSize))
            {
                ThrowHelper.ThrowInvalidOperationException("无效的块大小格式");
            }

            if (chunkSize == 0)
            {
                var afterHeader = headerBuffer.Slice(crlfIndex + 2);
                if (afterHeader.Length >= 2)
                {
                    reader.AdvanceTo(headerBuffer.GetPosition(crlfIndex + 2 + 2));
                }
                else
                {
                    reader.AdvanceTo(headerBuffer.GetPosition(crlfIndex + 2));
                }
                this.ContentStatus = ContentCompletionStatus.ReadCompleted;
                return 0;
            }

            reader.AdvanceTo(headerBuffer.GetPosition(crlfIndex + 2));
            this.m_chunkDataRemaining = chunkSize;
        }
    }

    /// <summary>
    /// 读取固定长度的内容
    /// </summary>
    /// <param name="reader">管道读取器</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>HTTP读取结果</returns>
    private async ValueTask<int> ReadFixedLengthContentAsync(PipeReader reader, Memory<byte> buffer, CancellationToken cancellationToken)
    {
        var totalBytesToRead = this.ContentLength;

        if (this.m_bytesRead >= totalBytesToRead)
        {
            this.ContentStatus = ContentCompletionStatus.ReadCompleted;
            return 0;
        }

        var readResult = await reader.ReadAsync(cancellationToken).ConfigureDefaultAwait();
        var pipeBuffer = readResult.Buffer;

        if (pipeBuffer.IsEmpty && readResult.IsCompleted)
        {
            ThrowHelper.ThrowInvalidOperationException("连接意外关闭，内容读取不完整");
        }

        var remainingBytes = totalBytesToRead - this.m_bytesRead;
        var bytesToRead = (int)Math.Min(Math.Min(remainingBytes, pipeBuffer.Length), buffer.Length);

        pipeBuffer.Slice(0, bytesToRead).CopyTo(buffer.Span);
        this.m_bytesRead += bytesToRead;
        reader.AdvanceTo(pipeBuffer.GetPosition(bytesToRead));

        if (this.m_bytesRead >= totalBytesToRead)
        {
            this.ContentStatus = ContentCompletionStatus.ReadCompleted;
        }

        return bytesToRead;
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
        this.m_contentByteBlock = new ByteBlock((int)content.Length);

        foreach (var item in content)
        {
            this.m_contentByteBlock.Write(item.Span);
        }
        this.m_contentMemory = this.m_contentByteBlock.Memory;
        this.ContentLength = content.Length;
        this.ContentStatus = ContentCompletionStatus.ContentCompleted;
    }

    internal void InternalSetContent(ByteBlock byteBlock)
    {
        this.m_contentByteBlock = byteBlock;
        this.m_contentMemory = this.m_contentByteBlock.Memory;
        this.ContentLength = byteBlock.Length;
        this.ContentStatus = ContentCompletionStatus.ContentCompleted;
    }
}