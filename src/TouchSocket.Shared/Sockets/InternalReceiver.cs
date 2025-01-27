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
internal sealed class InternalReceiver : ValueTaskSource<IReceiverResult>, IReceiver<IReceiverResult>
{
    #region 字段

    private readonly IReceiverClient<IReceiverResult> m_client;
    private readonly AsyncAutoResetEvent m_resetEventForComplateRead = new AsyncAutoResetEvent(false);
    private ByteBlock m_byteBlock;
    private IRequestInfo m_requestInfo;
    private bool m_cacheMode;
    private int m_maxCacheSize = 1024 * 64;
    private ByteBlock m_cacheByteBlock;
    private readonly InternalReceiverResult m_receiverResult;

    #endregion 字段

    public bool CacheMode { get => this.m_cacheMode; set => this.m_cacheMode = value; }

    public int MaxCacheSize { get => this.m_maxCacheSize; set => this.m_maxCacheSize = value; }

    /// <summary>
    /// Receiver
    /// </summary>
    /// <param name="client"></param>
    public InternalReceiver(IReceiverClient<IReceiverResult> client)
    {
        this.m_client = client;
        this.m_receiverResult = new InternalReceiverResult(this.ComplateRead);
    }

    ///// <inheritdoc/>
    //public Task<IReceiverResult> ReadAsync(CancellationToken token)
    //{
    //    return this.m_receiverResult.IsCompleted ? Task.FromResult<IReceiverResult>(this.m_receiverResult) : base.WaitAsync(token);
    //}

    /// <inheritdoc/>
    public ValueTask<IReceiverResult> ReadAsync(CancellationToken token)
    {
        return this.m_receiverResult.IsCompleted
            ? EasyValueTask.FromResult<IReceiverResult>(this.m_receiverResult)
            : base.ValueWaitAsync(token);
    }

    /// <inheritdoc/>
    public async Task InputReceiveAsync(ByteBlock byteBlock, IRequestInfo requestInfo)
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

        this.Complete(false);
        await this.m_resetEventForComplateRead.WaitOneAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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
            this.m_resetEventForComplateRead.Set();
            this.m_resetEventForComplateRead.SafeDispose();
        }
        this.m_byteBlock = null;
        this.m_requestInfo = null;
        base.Dispose(disposing);
    }

    private void ComplateRead()
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
        this.m_resetEventForComplateRead.Set();
    }

    protected override void Scheduler(Action<object> action, object state)
    {
        void Run(object o)
        {
            action.Invoke(o);
        }
        ThreadPool.UnsafeQueueUserWorkItem(Run, state);
    }

    protected override IReceiverResult GetResult()
    {
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
        return this.m_receiverResult;
    }
}

///// <summary>
///// Receiver
///// </summary>
//internal sealed class InternalReceiver : DisposableObject, IReceiver<IReceiverResult>, IValueTaskSource<IReceiverResult>
//{
//    /// <summary>
//    /// Receiver
//    /// </summary>
//    ~InternalReceiver()
//    {
//        this.Dispose(false);
//    }

//    #region 字段
//    private readonly IReceiverClient<IReceiverResult> m_client;
//    private readonly AsyncAutoResetEvent m_resetEventForComplateRead = new AsyncAutoResetEvent(false);
//    private readonly AsyncAutoResetEvent m_resetEventForRead = new AsyncAutoResetEvent(false);
//    private ByteBlock m_byteBlock;
//    private IRequestInfo m_requestInfo;
//    private bool m_cacheMode;
//    private int m_maxCacheSize = 1024 * 64;
//    private ByteBlock m_cacheByteBlock;
//    private readonly ReceiverResult m_receiverResult;
//    private ManualResetValueTaskSourceCore<ReceiverResult> m_manualResetValueTaskSourceCore = new ManualResetValueTaskSourceCore<ReceiverResult>();
//    #endregion

//    public bool CacheMode { get => this.m_cacheMode; set => this.m_cacheMode = value; }

//    public int MaxCacheSize { get => this.m_maxCacheSize; set => this.m_maxCacheSize = value; }

//    /// <summary>
//    /// Receiver
//    /// </summary>
//    /// <param name="client"></param>
//    public InternalReceiver(IReceiverClient<IReceiverResult> client)
//    {
//        this.m_client = client;
//        this.m_receiverResult = new ReceiverResult(this.ComplateRead);
//    }

//    /// <inheritdoc/>
//    public async Task<IReceiverResult> ReadAsync(CancellationToken token)
//    {
//        return await this.ValueReadAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
//        //this.ThrowIfDisposed();
//        //await this.m_resetEventForRead.WaitOneAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
//        //if (this.m_cacheMode)
//        //{
//        //    this.m_receiverResult.ByteBlock = this.m_cacheByteBlock;
//        //    this.m_receiverResult.RequestInfo = this.m_requestInfo;
//        //}
//        //else
//        //{
//        //    this.m_receiverResult.ByteBlock = this.m_byteBlock;
//        //    this.m_receiverResult.RequestInfo = this.m_requestInfo;
//        //}
//        //return this.m_receiverResult;

//        //throw new NotImplementedException();
//    }

//    /// <inheritdoc/>
//    public ValueTask<IReceiverResult> ValueReadAsync(CancellationToken token)
//    {
//        this.ThrowIfDisposed();
//        this.m_manualResetValueTaskSourceCore.Reset();
//        return new ValueTask<IReceiverResult>(this, this.m_manualResetValueTaskSourceCore.Version);
//    }

//    public void Complete()
//    {
//        this.m_receiverResult.ByteBlock = null;
//        this.m_receiverResult.RequestInfo = null;
//        this.m_manualResetValueTaskSourceCore.SetResult(this.m_receiverResult);
//    }
//    /// <inheritdoc/>
//    public async ValueTask<bool> TryInputReceive(ByteBlock byteBlock, IRequestInfo requestInfo)
//    {
//        if (this.DisposedValue)
//        {
//            return false;
//        }

//        if (this.m_cacheMode)
//        {
//            if (byteBlock == null)
//            {
//                throw new Exception("CacheMode 模式下byteBlock必须有值。");
//            }

//            ByteBlock bytes;
//            if (this.m_cacheByteBlock == null)
//            {
//                bytes = new ByteBlock(byteBlock.Length);
//                bytes.Write(byteBlock);
//            }
//            else if (this.m_cacheByteBlock.CanReadLen > 0)
//            {
//                bytes = new ByteBlock(byteBlock.Length + this.m_cacheByteBlock.CanReadLen);
//                bytes.Write(this.m_cacheByteBlock.Buffer, this.m_cacheByteBlock.Pos, this.m_cacheByteBlock.CanReadLen);
//                bytes.Write(byteBlock);

//                this.m_cacheByteBlock.Dispose();
//            }
//            else
//            {
//                bytes = new ByteBlock(byteBlock.Length);
//                bytes.Write(byteBlock);
//                this.m_cacheByteBlock.Dispose();
//            }

//            bytes.SeekToStart();
//            this.m_cacheByteBlock = bytes;
//            this.m_requestInfo = requestInfo;
//        }
//        else
//        {
//            this.m_byteBlock = byteBlock;
//            this.m_requestInfo = requestInfo;
//        }

//        //this.m_resetEventForRead.Set();

//        if (this.m_cacheMode)
//        {
//            this.m_receiverResult.ByteBlock = this.m_cacheByteBlock;
//            this.m_receiverResult.RequestInfo = this.m_requestInfo;
//        }
//        else
//        {
//            this.m_receiverResult.ByteBlock = this.m_byteBlock;
//            this.m_receiverResult.RequestInfo = this.m_requestInfo;
//        }

//        this.m_manualResetValueTaskSourceCore.SetResult(this.m_receiverResult);

//        if (byteBlock == null && requestInfo == null)
//        {
//            return true;
//        }
//        await this.m_resetEventForComplateRead.WaitOneAsync();
//        return true;
//    }

//    #region IValueTaskSource
//    public IReceiverResult GetResult(short token)
//    {
//        return this.m_manualResetValueTaskSourceCore.GetResult(token);
//        if (this.m_cacheMode)
//        {
//            this.m_receiverResult.ByteBlock = this.m_cacheByteBlock;
//            this.m_receiverResult.RequestInfo = this.m_requestInfo;
//        }
//        else
//        {
//            this.m_receiverResult.ByteBlock = this.m_byteBlock;
//            this.m_receiverResult.RequestInfo = this.m_requestInfo;
//        }
//        return this.m_receiverResult;
//    }

//    public ValueTaskSourceStatus GetStatus(short token)
//    {
//        return this.m_manualResetValueTaskSourceCore.GetStatus(token);
//    }

//    public void OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
//    {
//        this.m_manualResetValueTaskSourceCore.OnCompleted(continuation, state, token, flags);
//    }
//    #endregion

//    /// <inheritdoc/>
//    protected override void Dispose(bool disposing)
//    {
//        if (this.DisposedValue)
//        {
//            return;
//        }

//        if (disposing)
//        {
//            this.m_client.ClearReceiver();
//            this.m_resetEventForComplateRead.Set();
//            this.m_resetEventForComplateRead.SafeDispose();
//            this.m_resetEventForRead.Set();
//            this.m_resetEventForRead.SafeDispose();
//        }
//        this.m_byteBlock = null;
//        this.m_requestInfo = null;
//        base.Dispose(disposing);
//    }

//    private void ComplateRead()
//    {
//        if (this.m_cacheMode && this.m_cacheByteBlock.CanReadLen > this.m_maxCacheSize)
//        {
//            throw new Exception("缓存超过设定值");
//        }
//        this.m_byteBlock = default;
//        this.m_requestInfo = default;
//        this.m_receiverResult.RequestInfo = default;
//        this.m_receiverResult.ByteBlock = default;
//        this.m_resetEventForComplateRead.Set();
//    }
//}

///// <summary>
///// Receiver
///// </summary>
//internal sealed class InternalReceiver : DisposableObject, IReceiver<IReceiverResult>
//{
//    /// <summary>
//    /// Receiver
//    /// </summary>
//    ~InternalReceiver()
//    {
//        this.Dispose(false);
//    }

//    #region 字段
//    private readonly IReceiverClient<IReceiverResult> m_client;
//    private readonly AsyncAutoResetEvent m_resetEventForComplateRead = new AsyncAutoResetEvent(false);
//    private readonly AsyncAutoResetEvent m_resetEventForRead = new AsyncAutoResetEvent(false);
//    private ByteBlock m_byteBlock;
//    private IRequestInfo m_requestInfo;
//    private bool m_cacheMode;
//    private int m_maxCacheSize = 1024 * 64;
//    private ByteBlock m_cacheByteBlock;
//    private readonly ReceiverResult m_receiverResult;
//    #endregion

//    public bool CacheMode { get => this.m_cacheMode; set => this.m_cacheMode = value; }

//    public int MaxCacheSize { get => this.m_maxCacheSize; set => this.m_maxCacheSize = value; }

//    /// <summary>
//    /// Receiver
//    /// </summary>
//    /// <param name="client"></param>
//    public InternalReceiver(IReceiverClient<IReceiverResult> client)
//    {
//        this.m_client = client;
//        this.m_receiverResult = new ReceiverResult(this.ComplateRead);
//    }

//    /// <inheritdoc/>
//    public async Task<IReceiverResult> ReadAsync(CancellationToken token)
//    {
//        this.ThrowIfDisposed();
//        await this.m_resetEventForRead.WaitOneAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
//        if (this.m_cacheMode)
//        {
//            this.m_receiverResult.ByteBlock = this.m_cacheByteBlock;
//            this.m_receiverResult.RequestInfo = this.m_requestInfo;
//        }
//        else
//        {
//            this.m_receiverResult.ByteBlock = this.m_byteBlock;
//            this.m_receiverResult.RequestInfo = this.m_requestInfo;
//        }
//        return this.m_receiverResult;
//    }

//    /// <inheritdoc/>
//    public async ValueTask<IReceiverResult> ValueReadAsync(CancellationToken token)
//    {
//        this.ThrowIfDisposed();
//        await this.m_resetEventForRead.WaitOneAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

//        if (this.m_cacheMode)
//        {
//            this.m_receiverResult.ByteBlock = this.m_cacheByteBlock;
//            this.m_receiverResult.RequestInfo = this.m_requestInfo;
//        }
//        else
//        {
//            this.m_receiverResult.ByteBlock = this.m_byteBlock;
//            this.m_receiverResult.RequestInfo = this.m_requestInfo;
//        }
//        return this.m_receiverResult;
//    }

//    /// <inheritdoc/>
//    public async ValueTask<bool> TryInputReceive(ByteBlock byteBlock, IRequestInfo requestInfo)
//    {
//        if (this.DisposedValue)
//        {
//            return false;
//        }

//        if (this.m_cacheMode)
//        {
//            if (byteBlock == null)
//            {
//                throw new Exception("CacheMode 模式下byteBlock必须有值。");
//            }

//            ByteBlock bytes;
//            if (this.m_cacheByteBlock == null)
//            {
//                bytes = new ByteBlock(byteBlock.Length);
//                bytes.Write(byteBlock);
//            }
//            else if (this.m_cacheByteBlock.CanReadLen > 0)
//            {
//                bytes = new ByteBlock(byteBlock.Length + this.m_cacheByteBlock.CanReadLen);
//                bytes.Write(this.m_cacheByteBlock.Buffer, this.m_cacheByteBlock.Pos, this.m_cacheByteBlock.CanReadLen);
//                bytes.Write(byteBlock);

//                this.m_cacheByteBlock.Dispose();
//            }
//            else
//            {
//                bytes = new ByteBlock(byteBlock.Length);
//                bytes.Write(byteBlock);
//                this.m_cacheByteBlock.Dispose();
//            }

//            bytes.SeekToStart();
//            this.m_cacheByteBlock = bytes;
//            this.m_requestInfo = requestInfo;
//        }
//        else
//        {
//            this.m_byteBlock = byteBlock;
//            this.m_requestInfo = requestInfo;
//        }

//        this.m_resetEventForRead.Set();
//        if (byteBlock == null && requestInfo == null)
//        {
//            return true;
//        }

//        await this.m_resetEventForComplateRead.WaitOneAsync();
//        return true;
//    }

//    /// <inheritdoc/>
//    protected override void Dispose(bool disposing)
//    {
//        if (this.DisposedValue)
//        {
//            return;
//        }

//        if (disposing)
//        {
//            this.m_client.ClearReceiver();
//            this.m_resetEventForComplateRead.Set();
//            this.m_resetEventForComplateRead.SafeDispose();
//            this.m_resetEventForRead.Set();
//            this.m_resetEventForRead.SafeDispose();
//        }
//        this.m_byteBlock = null;
//        this.m_requestInfo = null;
//        base.Dispose(disposing);
//    }

//    private void ComplateRead()
//    {
//        if (this.m_cacheMode && this.m_cacheByteBlock.CanReadLen > this.m_maxCacheSize)
//        {
//            throw new Exception("缓存超过设定值");
//        }
//        this.m_byteBlock = default;
//        this.m_requestInfo = default;
//        this.m_receiverResult.RequestInfo = default;
//        this.m_receiverResult.ByteBlock = default;
//        this.m_resetEventForComplateRead.Set();
//    }
//}