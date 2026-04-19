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

using System.Buffers;
using System.IO.Pipelines;

namespace TouchSocket.Http;

internal sealed class ServerHttpRequest : HttpRequest
{
    private ReadOnlyMemory<byte> m_contentMemory;
    private PipeReader m_pipeReader;
    private long m_bodyBytesRemaining;
    private bool m_isChunkedBody;
    private int m_contentReadPosition = 0;
    private long m_chunkDataRemaining = 0;
    private bool m_needSkipCRLF = false;

    public ServerHttpRequest(HttpSessionClient httpSessionClient) : base(httpSessionClient)
    {
    }

    protected internal override void Reset()
    {
        this.m_contentMemory = default;
        this.m_pipeReader = null;
        this.m_bodyBytesRemaining = 0;
        this.m_isChunkedBody = false;
        this.m_contentReadPosition = 0;
        this.m_chunkDataRemaining = 0;
        this.m_needSkipCRLF = false;
        base.Reset();
    }

    /// <inheritdoc/>
    public override async ValueTask<ReadOnlyMemory<byte>> GetContentAsync(CancellationToken cancellationToken = default)
    {
        if (this.ContentStatus == ContentCompletionStatus.Unknown)
        {
            if (!this.IsChunk && this.ContentLength == 0)
            {
                this.m_contentMemory = ReadOnlyMemory<byte>.Empty;
                this.ContentStatus = ContentCompletionStatus.ContentCompleted;
                return this.m_contentMemory;
            }

            if (!this.IsChunk && this.ContentLength > MaxCacheSize)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_MoreThan(nameof(this.ContentLength), this.ContentLength, MaxCacheSize);
            }

            try
            {
                using var memoryStream = new MemoryStream((int)this.ContentLength);
                using var bufferOwner = MemoryPool<byte>.Shared.Rent(8192);
                var rentedBuffer = bufferOwner.Memory;

                while (true)
                {
                    var read = await this.ReadAsync(rentedBuffer, cancellationToken).ConfigureDefaultAwait();
                    if (read == 0)
                    {
                        break;
                    }
                    memoryStream.Write(rentedBuffer.Slice(0, read).Span);
                }
                this.ContentStatus = ContentCompletionStatus.ContentCompleted;
                this.m_contentMemory = memoryStream.ToArray();
                return this.m_contentMemory;
            }
            catch
            {
                this.ContentStatus = ContentCompletionStatus.Incomplete;
                return default;
            }
        }
        else
        {
            return this.ContentStatus == ContentCompletionStatus.ContentCompleted ? this.m_contentMemory : default;
        }
    }

    /// <inheritdoc/>
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
            return EasyValueTask.FromResult(0);
        }

        if (this.m_pipeReader == null)
        {
            return EasyValueTask.FromResult(0);
        }

        return this.m_isChunkedBody
            ? this.ReadChunkedBlockAsync(buffer, cancellationToken)
            : this.ReadFixedLengthBlockAsync(buffer, cancellationToken);
    }

    internal void InternalSetForPipeReading(PipeReader pipeReader, long contentLength, bool isChunked)
    {
        this.m_pipeReader = pipeReader;
        this.m_bodyBytesRemaining = contentLength;
        this.m_isChunkedBody = isChunked;
    }

    /// <summary>
    /// 获取请求体是否已被完整消费。
    /// </summary>
    internal bool InternalIsBodyConsumed =>
        this.ContentStatus == ContentCompletionStatus.ContentCompleted ||
        this.ContentStatus == ContentCompletionStatus.ReadCompleted ||
        this.m_pipeReader == null ||
        (!this.m_isChunkedBody && this.m_bodyBytesRemaining == 0);

    /// <summary>
    /// 获取剩余未读取的固定长度请求体字节数。
    /// </summary>
    internal long InternalBodyBytesRemaining => this.m_bodyBytesRemaining;

    /// <summary>
    /// 获取请求体是否为 chunked 传输编码。
    /// </summary>
    internal bool InternalIsChunkedBody => this.m_isChunkedBody;

    internal async Task InternalDrainBodyAsync(CancellationToken cancellationToken)
    {
        if (this.ContentStatus == ContentCompletionStatus.ContentCompleted ||
            this.ContentStatus == ContentCompletionStatus.ReadCompleted ||
            this.m_pipeReader == null)
        {
            return;
        }
        if (this.m_isChunkedBody)
        {
            await this.DrainChunkedAsync(cancellationToken).ConfigureDefaultAwait();
        }
        else
        {
            await this.DrainFixedLengthAsync(cancellationToken).ConfigureDefaultAwait();
        }
    }

    private async ValueTask<int> ReadFixedLengthBlockAsync(Memory<byte> buffer, CancellationToken cancellationToken)
    {
        if (this.m_bodyBytesRemaining <= 0)
        {
            this.ContentStatus = ContentCompletionStatus.ReadCompleted;
            return 0;
        }

        while (true)
        {
            var readResult = await this.m_pipeReader.ReadAsync(cancellationToken).ConfigureDefaultAwait();
            var pipeBuffer = readResult.Buffer;

            if (pipeBuffer.Length == 0 && readResult.IsCompleted)
            {
                this.ContentStatus = ContentCompletionStatus.Incomplete;
                return 0;
            }

            var toRead = (int)Math.Min(Math.Min(this.m_bodyBytesRemaining, pipeBuffer.Length), buffer.Length);
            if (toRead == 0)
            {
                this.m_pipeReader.AdvanceTo(pipeBuffer.Start, pipeBuffer.End);
                continue;
            }

            pipeBuffer.Slice(0, toRead).CopyTo(buffer.Span);
            this.m_bodyBytesRemaining -= toRead;
            this.m_pipeReader.AdvanceTo(pipeBuffer.GetPosition(toRead));

            if (this.m_bodyBytesRemaining == 0)
            {
                this.ContentStatus = ContentCompletionStatus.ReadCompleted;
            }

            return toRead;
        }
    }

    private async ValueTask<int> ReadChunkedBlockAsync(Memory<byte> buffer, CancellationToken cancellationToken)
    {
        while (true)
        {
            if (this.m_chunkDataRemaining > 0)
            {
                var readResult = await this.m_pipeReader.ReadAsync(cancellationToken).ConfigureDefaultAwait();
                var pipeBuffer = readResult.Buffer;

                if (pipeBuffer.IsEmpty && readResult.IsCompleted)
                {
                    this.ContentStatus = ContentCompletionStatus.Incomplete;
                    return 0;
                }

                if (pipeBuffer.IsEmpty)
                {
                    this.m_pipeReader.AdvanceTo(pipeBuffer.Start, pipeBuffer.End);
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
                        this.m_pipeReader.AdvanceTo(pipeBuffer.GetPosition(toRead + 2));
                    }
                    else
                    {
                        this.m_pipeReader.AdvanceTo(pipeBuffer.GetPosition(toRead));
                        this.m_needSkipCRLF = true;
                    }
                }
                else
                {
                    this.m_pipeReader.AdvanceTo(pipeBuffer.GetPosition(toRead));
                }

                return toRead;
            }

            if (this.m_needSkipCRLF)
            {
                var crlfResult = await this.m_pipeReader.ReadAsync(cancellationToken).ConfigureDefaultAwait();
                var crlfBuffer = crlfResult.Buffer;

                if (crlfBuffer.Length < 2)
                {
                    if (crlfResult.IsCompleted)
                    {
                        this.ContentStatus = ContentCompletionStatus.Incomplete;
                        return 0;
                    }
                    this.m_pipeReader.AdvanceTo(crlfBuffer.Start, crlfBuffer.End);
                    continue;
                }

                this.m_pipeReader.AdvanceTo(crlfBuffer.GetPosition(2));
                this.m_needSkipCRLF = false;
                continue;
            }

            var headerResult = await this.m_pipeReader.ReadAsync(cancellationToken).ConfigureDefaultAwait();
            var headerBuffer = headerResult.Buffer;

            if (headerBuffer.Length == 0 && headerResult.IsCompleted)
            {
                this.ContentStatus = ContentCompletionStatus.Incomplete;
                return 0;
            }

            var crlfIndex = headerBuffer.IndexOf(TouchSocketHttpUtility.CRLF);
            if (crlfIndex < 0)
            {
                if (headerResult.IsCompleted)
                {
                    this.ContentStatus = ContentCompletionStatus.Incomplete;
                    return 0;
                }
                this.m_pipeReader.AdvanceTo(headerBuffer.Start, headerBuffer.End);
                continue;
            }

            var chunkSizeSlice = headerBuffer.Slice(0, crlfIndex);
            using var tempBuffer = new ContiguousMemoryBuffer(chunkSizeSlice);
            if (!TryParseChunkSize(tempBuffer.Memory.Span, out var chunkSize))
            {
                ThrowHelper.ThrowInvalidOperationException("无效的 chunk 大小格式");
            }

            var afterSizePos = headerBuffer.GetPosition(crlfIndex + 2);
            var remaining = headerBuffer.Slice(afterSizePos);

            if (chunkSize == 0)
            {
                var consumed = remaining.Length >= 2 ? headerBuffer.GetPosition(2, afterSizePos) : afterSizePos;
                this.m_pipeReader.AdvanceTo(consumed);
                this.ContentStatus = ContentCompletionStatus.ReadCompleted;
                return 0;
            }

            this.m_pipeReader.AdvanceTo(headerBuffer.GetPosition(crlfIndex + 2));
            this.m_chunkDataRemaining = chunkSize;
        }
    }

    private async Task DrainFixedLengthAsync(CancellationToken cancellationToken)
    {
        while (this.m_bodyBytesRemaining > 0)
        {
            var readResult = await this.m_pipeReader.ReadAsync(cancellationToken).ConfigureDefaultAwait();
            var buffer = readResult.Buffer;
            if (buffer.Length == 0 && readResult.IsCompleted)
            {
                this.m_bodyBytesRemaining = 0;
                return;
            }
            var toSkip = Math.Min(this.m_bodyBytesRemaining, buffer.Length);
            this.m_bodyBytesRemaining -= toSkip;
            this.m_pipeReader.AdvanceTo(buffer.GetPosition(toSkip));
        }
    }

    private async Task DrainChunkedAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var readResult = await this.m_pipeReader.ReadAsync(cancellationToken).ConfigureDefaultAwait();
            var buffer = readResult.Buffer;

            if (buffer.Length == 0 && readResult.IsCompleted)
            {
                return;
            }

            var crlfIndex = buffer.IndexOf(TouchSocketHttpUtility.CRLF);
            if (crlfIndex < 0)
            {
                if (readResult.IsCompleted)
                {
                    return;
                }
                this.m_pipeReader.AdvanceTo(buffer.Start, buffer.End);
                continue;
            }

            var chunkSizeSlice = buffer.Slice(0, crlfIndex);
            using var tempBuffer = new ContiguousMemoryBuffer(chunkSizeSlice);
            if (!TryParseChunkSize(tempBuffer.Memory.Span, out var chunkSize))
            {
                return;
            }

            var afterSizePos = buffer.GetPosition(crlfIndex + 2);
            var remaining = buffer.Slice(afterSizePos);

            if (chunkSize == 0)
            {
                var consumed = remaining.Length >= 2 ? buffer.GetPosition(2, afterSizePos) : afterSizePos;
                this.m_pipeReader.AdvanceTo(consumed);
                return;
            }

            if (remaining.Length < chunkSize + 2)
            {
                if (readResult.IsCompleted)
                {
                    return;
                }
                this.m_pipeReader.AdvanceTo(buffer.Start, buffer.End);
                continue;
            }

            this.m_pipeReader.AdvanceTo(buffer.GetPosition(chunkSize + 2, afterSizePos));
        }
    }

    private static bool TryParseChunkSize(ReadOnlySpan<byte> hexBytes, out long chunkSize)
    {
        chunkSize = 0;
        if (hexBytes.Length == 0)
        {
            return false;
        }
        for (var i = 0; i < hexBytes.Length; i++)
        {
            var b = hexBytes[i];
            if (b == ';' || b == ' ' || b == '\t')
            {
                break;
            }
            int digit;
            if (b >= '0' && b <= '9') { digit = b - '0'; }
            else if (b >= 'A' && b <= 'F') { digit = b - 'A' + 10; }
            else if (b >= 'a' && b <= 'f') { digit = b - 'a' + 10; }
            else { return false; }
            if (chunkSize > (long.MaxValue - digit) / 16)
            {
                return false;
            }
            chunkSize = chunkSize * 16 + digit;
        }
        return true;
    }
}