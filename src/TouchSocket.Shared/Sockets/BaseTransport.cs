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

using System.IO.Pipelines;
using TouchSocket.Resources;

namespace TouchSocket.Sockets;

internal abstract class BaseTransport : SafetyDisposableObject, ITransport
{
    protected readonly Pipe m_pipeReceive;
    protected readonly Pipe m_pipeSend;
    protected ClosedEventArgs m_closedEventArgs;
    protected ValueCounter m_receiveCounter;
    protected ValueCounter m_sentCounter;
    private static readonly ClosedEventArgs s_defaultClosedEventArgs = new ClosedEventArgs(false, TouchSocketResource.RemoteDisconnects);
    private readonly int m_maxBufferSize;
    private readonly int m_minBufferSize;
    private readonly SemaphoreSlim m_readLocker = new SemaphoreSlim(1, 1);
    protected readonly CancellationTokenSource m_tokenSource = new CancellationTokenSource();
    private readonly SemaphoreSlim m_writeLocker = new SemaphoreSlim(1, 1);
    private int m_receiveBufferSize = 1024 * 10;
    private int m_sendBufferSize = 1024 * 10;

    public BaseTransport(TransportOption option)
    {
        var maxBufferSize = option.MaxBufferSize;
        var minBufferSize = option.MinBufferSize;
        var receivePipeOptions = option.ReceivePipeOptions;
        var sendPipeOptions = option.SendPipeOptions;

        this.m_pipeReceive = receivePipeOptions == null ? new Pipe() : new Pipe(receivePipeOptions);
        this.m_pipeSend = sendPipeOptions == null ? new Pipe() : new Pipe(sendPipeOptions);
        this.m_receiveCounter = new ValueCounter(TimeSpan.FromSeconds(1), this.OnReceivePeriod);

        this.m_sentCounter = new ValueCounter(TimeSpan.FromSeconds(1), this.OnSendPeriod);
        this.m_maxBufferSize = maxBufferSize;
        this.m_minBufferSize = minBufferSize;
    }

    public ClosedEventArgs ClosedEventArgs => this.m_closedEventArgs ?? s_defaultClosedEventArgs;

    public CancellationToken ClosedToken => this.m_tokenSource.Token;

    /// <summary>
    /// 获取用于读取数据的管道读取器
    /// </summary>
    public virtual PipeReader Reader => this.m_pipeReceive.Reader;

    public SemaphoreSlim ReadLocker => this.m_readLocker;

    /// <summary>
    /// 接收缓存池，运行时的值会根据流速自动调整
    /// </summary>
    public int ReceiveBufferSize => Math.Min(Math.Max(this.m_receiveBufferSize, this.m_minBufferSize), this.m_maxBufferSize);

    /// <summary>
    /// 接收计数器
    /// </summary>
    public ValueCounter ReceiveCounter => this.m_receiveCounter;

    /// <summary>
    /// 发送缓存池，运行时的值会根据流速自动调整
    /// </summary>
    public int SendBufferSize => Math.Min(Math.Max(this.m_sendBufferSize, this.m_minBufferSize), this.m_maxBufferSize);

    /// <summary>
    /// 发送计数器
    /// </summary>
    public ValueCounter SendCounter => this.m_sentCounter;

    public SemaphoreSlim WriteLocker => this.m_writeLocker;

    /// <summary>
    /// 获取用于写入数据的管道写入器
    /// </summary>
    public virtual PipeWriter Writer => this.m_pipeSend.Writer;

    public virtual async Task<Result> CloseAsync(string msg, CancellationToken cancellationToken = default)
    {
        try
        {
            if (this.m_tokenSource.IsCancellationRequested)
            {
                return Result.Success;
            }
            this.m_closedEventArgs ??= new ClosedEventArgs(true, msg);
            // 停止所有后台任务
            this.m_tokenSource.SafeCancel();

            // 完成发送管道
            await this.m_pipeSend.Writer.CompleteAsync().SafeWaitAsync(cancellationToken);

            // 等待发送管道读取器完成
            await this.m_pipeSend.Reader.CompleteAsync().SafeWaitAsync(cancellationToken);

            // 完成接收管道
            await this.m_pipeReceive.Writer.CompleteAsync().SafeWaitAsync(cancellationToken);

            // 等待接收管道读取器完成
            await this.m_pipeReceive.Reader.CompleteAsync().SafeWaitAsync(cancellationToken);
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex.Message);
        }
    }

    protected abstract Task RunReceive(CancellationToken cancellationToken);

    protected abstract Task RunSend(CancellationToken cancellationToken);

    protected override void SafetyDispose(bool disposing)
    {
        this.m_tokenSource.SafeCancel();
        this.m_tokenSource.SafeDispose();
    }

    protected void Start()
    {
        _ = EasyTask.SafeRun(this.RunReceive, this.m_tokenSource.Token);
        _ = EasyTask.SafeRun(this.RunSend, this.m_tokenSource.Token);
    }

    private void OnReceivePeriod(long value)
    {
        this.m_receiveBufferSize = Math.Max(TouchSocketCoreUtility.HitBufferLength(value), this.m_minBufferSize);
    }

    private void OnSendPeriod(long value)
    {
        this.m_sendBufferSize = Math.Max(TouchSocketCoreUtility.HitBufferLength(value), this.m_minBufferSize);
    }
}