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
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;

namespace TouchSocket.Sockets;

internal abstract class BaseTransport : ITransport
{
    private readonly int m_maxBufferSize;
    private readonly int m_minBufferSize;
    protected readonly Pipe m_pipeReceive;
    protected readonly Pipe m_pipeSend;
    private readonly CancellationTokenSource m_tokenSource = new CancellationTokenSource();
    private int m_receiveBufferSize = 1024 * 10;
    protected ValueCounter m_receiveCounter;
    private readonly SemaphoreSlim m_semaphoreSlimForWriter = new SemaphoreSlim(1, 1);
    private int m_sendBufferSize = 1024 * 10;
    protected ValueCounter m_sentCounter;

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

    protected void Start()
    {
        _ = EasyTask.SafeRun(this.RunReceive, this.m_tokenSource.Token);
        _ = EasyTask.SafeRun(this.RunSend, this.m_tokenSource.Token);
    }

    public ClosedEventArgs ClosedEventArgs { get; protected set; }

    public CancellationToken ClosedToken => this.m_tokenSource.Token;

    /// <summary>
    /// 获取用于读取数据的管道读取器
    /// </summary>
    public PipeReader Input => this.m_pipeReceive.Reader;

    /// <summary>
    /// 获取用于写入数据的管道写入器
    /// </summary>
    public PipeWriter Output => this.m_pipeSend.Writer;

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

    public SemaphoreSlim SemaphoreSlimForWriter => this.m_semaphoreSlimForWriter;

    public virtual async Task<Result> CloseAsync(string msg, CancellationToken token = default)
    {
        try
        {
            if (this.m_tokenSource.IsCancellationRequested)
            {
                return Result.Success;
            }
            this.ClosedEventArgs ??= new ClosedEventArgs(true, msg);
            // 停止所有后台任务
            this.m_tokenSource.SafeCancel();

            // 完成发送管道
            await this.m_pipeSend.Writer.CompleteAsync().SafeWaitAsync(token);

            // 等待发送管道读取器完成
            await this.m_pipeSend.Reader.CompleteAsync().SafeWaitAsync(token);

            // 完成接收管道
            await this.m_pipeReceive.Writer.CompleteAsync().SafeWaitAsync(token);

            // 等待接收管道读取器完成
            await this.m_pipeReceive.Reader.CompleteAsync().SafeWaitAsync(token);
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex.Message);
        }
    }


    private void OnReceivePeriod(long value)
    {
        this.m_receiveBufferSize = Math.Max(TouchSocketCoreUtility.HitBufferLength(value), this.m_minBufferSize);
    }

    private void OnSendPeriod(long value)
    {
        this.m_sendBufferSize = Math.Max(TouchSocketCoreUtility.HitBufferLength(value), this.m_minBufferSize);
    }

    protected abstract Task RunReceive(CancellationToken token);

    protected abstract Task RunSend(CancellationToken token);
}
