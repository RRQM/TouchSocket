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

namespace TouchSocket.Http;

internal sealed class ServerHttpRequest : HttpRequest
{
    private readonly AsyncExchange<ReadOnlyMemory<byte>> m_asyncExchange;

    private ReadOnlyMemory<byte> m_contentMemory;

    public ServerHttpRequest(HttpSessionClient httpSessionClient) : base(httpSessionClient)
    {
        this.m_asyncExchange = new AsyncExchange<ReadOnlyMemory<byte>>();
    }

    protected internal override void Reset()
    {
        if (this.m_asyncExchange.IsCompleted)
        {
            this.m_asyncExchange.Reset();
        }
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
                    this.ContentStatus = ContentCompletionStatus.ContentCompleted;
                    this.m_contentMemory = memoryStream.ToArray();
                    return this.m_contentMemory;
                }
            }
            catch
            {
                this.ContentStatus = ContentCompletionStatus.Incomplete;
                return default;
            }
            finally
            {
            }
        }
        else
        {
            return this.ContentStatus == ContentCompletionStatus.ContentCompleted ? this.m_contentMemory : default;
        }
    }

    /// <inheritdoc/>
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


        var readLeaseTask = this.m_asyncExchange.ReadAsync(cancellationToken);

        ReadLease<ReadOnlyMemory<byte>> readLease;
        if (readLeaseTask.IsCompleted)
        {
            readLease = readLeaseTask.Result;
        }
        else
        {
            readLease = await readLeaseTask.ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }

        var memory = readLease.Value;

        if (readLease.IsCompleted)
        {
            this.ContentStatus = ContentCompletionStatus.ReadCompleted;
        }

        return new HttpReadOnlyMemoryBlockResult(readLease.Dispose, memory, readLease.IsCompleted);
    }

    internal void CompleteInput()
    {
        this.m_asyncExchange.Complete();
    }

    internal ValueTask<bool> InternalInputAsync(in ReadOnlyMemory<byte> memory)
    {
        return this.m_asyncExchange.WriteAsync(memory, CancellationToken.None);
    }

    /// <inheritdoc/>
    internal void InternalSetContent(ReadOnlyMemory<byte> content)
    {
        this.m_contentMemory = content;
        this.ContentStatus = ContentCompletionStatus.ContentCompleted;
    }
}