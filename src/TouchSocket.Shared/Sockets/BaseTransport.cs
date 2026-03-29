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
using System.Runtime.CompilerServices;
using TouchSocket.Resources;

namespace TouchSocket.Sockets;

internal abstract class BaseTransport : SafetyDisposableObject, ITransport
{
    protected readonly Pipe m_pipeReceive;
    protected readonly Pipe m_pipeSend;
    protected readonly CancellationTokenSource m_tokenSource = new CancellationTokenSource();
    protected ClosedEventArgs m_closedEventArgs;
    protected ValueCounter m_receiveCounter;
    protected ValueCounter m_sentCounter;
    private static readonly ClosedEventArgs s_defaultClosedEventArgs = new ClosedEventArgs(TouchSocketResource.RemoteDisconnects);
    private readonly object m_closeLock = new object();
    private readonly int m_maxBufferSize;
    private readonly int m_minBufferSize;
    private readonly SemaphoreSlim m_readLocker = new SemaphoreSlim(1, 1);
    private readonly SemaphoreSlim m_writeLocker = new SemaphoreSlim(1, 1);
    private int m_cachedReceiveBufferSize = 1024 * 2;
    private int m_closeSignaled;
    private Task m_receiveTask = Task.CompletedTask;
    private Task m_sendTask = Task.CompletedTask;
    private Task m_transportClosedTask = Task.CompletedTask;

    public BaseTransport(TransportOption option)
    {
        var maxBufferSize = option.MaxBufferSize;
        var minBufferSize = option.MinBufferSize;
        var receivePipeOptions = option.ReceivePipeOptions;
        var sendPipeOptions = option.SendPipeOptions;

        this.m_pipeReceive = receivePipeOptions == null ? new Pipe() : new Pipe(receivePipeOptions);
        this.m_pipeSend = sendPipeOptions == null ? new Pipe() : new Pipe(sendPipeOptions);
        this.m_receiveCounter = new ValueCounter(TimeSpan.FromSeconds(1), this.OnReceivePeriod);

        this.m_sentCounter = new ValueCounter(TimeSpan.FromSeconds(1));
        this.m_maxBufferSize = maxBufferSize;
        this.m_minBufferSize = minBufferSize;

        // 初始化缓存的缓冲区大小
        // 这是一个在内存占用和性能之间很好的平衡点
        this.m_cachedReceiveBufferSize = this.ClampBufferSize(1024 * 2);
    }

    public ClosedEventArgs ClosedEventArgs => this.m_closedEventArgs ?? s_defaultClosedEventArgs;

    public CancellationToken ClosedToken => this.DisposedValue ? new CancellationToken(true) : this.m_tokenSource.Token;

    /// <summary>
    /// 获取用于读取数据的管道读取器
    /// </summary>
    public virtual PipeReader Reader => this.m_pipeReceive.Reader;

    public SemaphoreSlim ReadLocker => this.m_readLocker;

    /// <summary>
    /// 接收缓存池，运行时的值会根据流速自动调整
    /// </summary>
    public int ReceiveBufferSize => this.m_cachedReceiveBufferSize;

    /// <summary>
    /// 接收计数器
    /// </summary>
    public ValueCounter ReceiveCounter => this.m_receiveCounter;

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
            var began = this.TryBeginClose(msg);
            if (began)
            {
                this.SignalClose();
            }

            await this.WaitForTransportClosedAsync(cancellationToken).ConfigureDefaultAwait();
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    protected abstract Task RunReceive(CancellationToken cancellationToken);

    protected abstract Task RunSend(CancellationToken cancellationToken);

    protected override void SafetyDispose(bool disposing)
    {
        if (disposing)
        {
            this.SignalClose();
            this.m_tokenSource.SafeCancel();
            this.m_tokenSource.SafeDispose();
        }
    }

    protected void SignalClose()
    {
        // 取消令牌，停止所有后台循环
        this.m_tokenSource.SafeCancel();

        // 使用 CancelPendingRead/CancelPendingFlush 立即解除所有挂起的管道操作阻塞，
        // 而不是直接 Complete 管道——Complete 有时序竞态风险，可能导致双重完成异常。
        // ReceiveLoopAsync 中的 ReadAsync(CancellationToken.None) 会因此立即返回 IsCanceled=true。
        this.m_pipeReceive.Reader.CancelPendingRead();
        this.m_pipeSend.Reader.CancelPendingRead();

        // 取消挂起的 Flush（背压场景：消费方慢时，RunReceive 可能阻塞在 FlushAsync）
        this.m_pipeReceive.Writer.CancelPendingFlush();
        this.m_pipeSend.Writer.CancelPendingFlush();
    }

    protected void Start()
    {
        this.m_receiveTask = EasyTask.SafeRun(this.RunReceive, this.m_tokenSource.Token);
        this.m_sendTask = EasyTask.SafeRun(this.RunSend, this.m_tokenSource.Token);
        this.m_transportClosedTask = Task.WhenAll(this.m_receiveTask, this.m_sendTask);
    }

    protected bool TryBeginClose(string msg)
    {
        lock (this.m_closeLock)
        {
            this.m_closedEventArgs ??= new ClosedEventArgs(msg);

            if (Interlocked.CompareExchange(ref this.m_closeSignaled, 1, 0) != 0)
            {
                return false;
            }

            return true;
        }
    }

    protected Task WaitForTransportClosedAsync(CancellationToken cancellationToken = default)
    {
        var closedTask = this.m_transportClosedTask;
        if (!cancellationToken.CanBeCanceled)
        {
            return closedTask;
        }

        return closedTask.WithCancellation(cancellationToken);
    }

    /// <summary>
    /// 根据数据流量计算最优缓冲区大小
    /// </summary>
    /// <param name="bytesPerSecond">每秒字节数</param>
    /// <returns>建议的缓冲区大小</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int CalculateOptimalBufferSize(long bytesPerSecond)
    {
        // 定义缓冲区大小常量
        const int KB = 1024;

        return bytesPerSecond switch
        {
            // <10KB/s: 2KB缓冲区，适用于低流量场景(如聊天、控制命令等)
            < 10 * KB => 2 * KB,

            // 10KB/s - 100KB/s: 4KB缓冲区，适用于一般Web请求
            < 100 * KB => 4 * KB,

            // 100KB/s - 1MB/s: 8KB缓冲区，适用于小文件传输
            < KB * KB => 8 * KB,

            // 1MB/s - 10MB/s: 16KB缓冲区，适用于中等流量
            < 10 * KB * KB => 16 * KB,

            // 10MB/s - 50MB/s: 32KB缓冲区，适用于大文件传输
            < 50 * KB * KB => 32 * KB,

            // 50MB/s - 100MB/s: 64KB缓冲区，适用于高速传输
            < 100 * KB * KB => 64 * KB,

            // >=100MB/s: 128KB缓冲区，适用于超高速传输
            _ => 128 * KB
        };
    }

    /// <summary>
    /// 将缓冲区大小限制在最小值和最大值之间
    /// </summary>
    /// <param name="size">原始缓冲区大小</param>
    /// <returns>限制后的缓冲区大小</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int ClampBufferSize(int size)
    {
        return Math.Min(Math.Max(size, this.m_minBufferSize), this.m_maxBufferSize);
    }

    private void OnReceivePeriod(long value)
    {
        var newSize = CalculateOptimalBufferSize(value);
        this.m_cachedReceiveBufferSize = this.ClampBufferSize(newSize);
    }
}