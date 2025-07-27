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
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets;

/// <summary>
/// Receiver
/// </summary>
internal sealed class InternalReceiver : BlockSegment<IReceiverResult>, IReceiver<IReceiverResult>
{
    #region 字段

    private readonly IReceiverClient<IReceiverResult> m_client;
    private readonly AsyncAutoResetEvent m_resetEventForComplateRead = new AsyncAutoResetEvent(false);
    private IByteBlockReader m_byteBlock;
    private IRequestInfo m_requestInfo;
    private bool m_cacheMode;
    private int m_maxCacheSize = 1024 * 64;
    private ByteBlock m_cacheByteBlock;
    private InternalReceiverResult m_receiverResult;

    #endregion 字段

    public bool CacheMode { get => this.m_cacheMode; set => this.m_cacheMode = value; }

    public int MaxCacheSize { get => this.m_maxCacheSize; set => this.m_maxCacheSize = value; }

    /// <summary>
    /// Receiver
    /// </summary>
    /// <param name="client"></param>
    public InternalReceiver(IReceiverClient<IReceiverResult> client) : base(true)
    {
        this.m_client = client;
    }

    /// <inheritdoc/>
    public ValueTask<IReceiverResult> ReadAsync(CancellationToken token)
    {
        if (this.m_receiverResult.IsCompleted)
        {
            return EasyValueTask.FromResult<IReceiverResult>(this.m_receiverResult);
        }
        else
        {
            return base.ProtectedReadAsync(token);
        }
    }

    /// <inheritdoc/>
    public async Task InputReceiveAsync(IByteBlockReader byteBlock, IRequestInfo requestInfo)
    {
        if (this.DisposedValue)
        {
            return;
        }

        if (this.m_cacheMode && byteBlock != null)
        {
            ByteBlock bytes;
            if (this.m_cacheByteBlock == null)
            {
                bytes = new ByteBlock(byteBlock.Length);
                bytes.Write(byteBlock.Span);
            }
            else if (this.m_cacheByteBlock.CanReadLength > 0)
            {
                bytes = new ByteBlock(byteBlock.Length + this.m_cacheByteBlock.CanReadLength);
                bytes.Write(this.m_cacheByteBlock.Span.Slice(this.m_cacheByteBlock.Position, this.m_cacheByteBlock.CanReadLength));
                bytes.Write(byteBlock.Span);

                this.m_cacheByteBlock.Dispose();
            }
            else
            {
                bytes = new ByteBlock(byteBlock.Length);
                bytes.Write(byteBlock.Span);
                this.m_cacheByteBlock.Dispose();
            }

            bytes.SeekToStart();
            this.m_cacheByteBlock = bytes;
            this.m_requestInfo = requestInfo;
        }
        else
        {
            this.m_byteBlock = byteBlock;
            this.m_requestInfo = requestInfo;
        }

        if (this.m_cacheMode)
        {
            this.m_receiverResult.ByteBlock = this.m_cacheByteBlock;
            this.m_receiverResult.RequestInfo = this.m_requestInfo;
        }
        else
        {
            this.m_receiverResult.ByteBlock = this.m_byteBlock;
            this.m_receiverResult.RequestInfo = this.m_requestInfo;
        }

        await base.TriggerAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    public async Task Complete(string msg)
    {
        try
        {
            this.m_receiverResult.IsCompleted = true;
            this.m_receiverResult.Message = msg;
            await this.InputReceiveAsync(default, default).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        catch
        {
        }
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (this.DisposedValue)
        {
            return;
        }

        if (disposing)
        {
            this.m_client.ClearReceiver();
            this.m_resetEventForComplateRead.SetAll();
            //this.m_resetEventForComplateRead.SafeDispose();
        }
        this.m_byteBlock = null;
        this.m_requestInfo = null;
        base.Dispose(disposing);
    }

    protected override void CompleteRead()
    {
        var byteBlock = this.m_cacheByteBlock;
        if (this.m_cacheMode)
        {
            if (byteBlock != null)
            {
                if (byteBlock.CanReadLength > this.m_maxCacheSize)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException_MoreThan(nameof(this.m_cacheByteBlock.CanReadLength), this.m_cacheByteBlock.CanReadLength, this.m_maxCacheSize);
                }
            }
        }
        this.m_byteBlock = default;
        this.m_requestInfo = default;
        this.m_receiverResult.RequestInfo = default;
        this.m_receiverResult.ByteBlock = default;
        base.CompleteRead();
    }


    protected override IReceiverResult CreateResult(Action actionForDispose)
    {
        this.m_receiverResult = new InternalReceiverResult(actionForDispose);
        return this.m_receiverResult;
    }
}
