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
using System.Net.Security;
using System.Net.Sockets;
using TouchSocket.Resources;

namespace TouchSocket.Sockets;

internal sealed class TcpTransport : BaseTransport
{
    private readonly bool m_bufferOnDemand;
    private readonly object m_closeTaskLock = new object();
    private readonly Socket m_socket;
    private readonly TcpCore m_tcpCore;
    private PipeReader m_reader;
    private Task m_shutdownTask;
    private PipeWriter m_writer;

    public TcpTransport(TcpCore tcpCore, TransportOption option) : base(option)
    {
        this.m_tcpCore = tcpCore ?? throw new ArgumentNullException(nameof(tcpCore));
        this.m_socket = tcpCore.Socket;
        this.m_bufferOnDemand = option.BufferOnDemand;
        this.Start();
    }

    /// <summary>
    /// 获取用于读取数据的管道读取器
    /// </summary>
    public override PipeReader Reader => this.m_reader ?? base.Reader;

    /// <summary>
    /// 获取是否使用SSL
    /// </summary>
    public bool UseSsl { get; private set; }

    /// <summary>
    /// 获取用于写入数据的管道写入器
    /// </summary>
    public override PipeWriter Writer => this.m_writer ?? base.Writer;

    /// <summary>
    /// 服务端SSL认证
    /// </summary>
    /// <param name="sslOption">SSL配置选项</param>
    public async Task AuthenticateAsync(ServiceSslOption sslOption)
    {
        if (this.UseSsl)
        {
            return;
        }
        var sourceStream = new TransportStream(this);
        SslStream sslStream;
        if (sslOption.CertificateValidationCallback != null)
        {
            sslStream = new SslStream(sourceStream, false, sslOption.CertificateValidationCallback);
        }
        else
        {
            sslStream = new SslStream(sourceStream, false);
        }

        await sslStream.AuthenticateAsServerAsync(sslOption.Certificate, sslOption.ClientCertificateRequired
            , sslOption.SslProtocols
            , sslOption.CheckCertificateRevocation).ConfigureDefaultAwait();

        this.m_reader = PipeReader.Create(sslStream);
        this.m_writer = PipeWriter.Create(sslStream);
        this.UseSsl = true;
    }

    /// <summary>
    /// 客户端SSL认证
    /// </summary>
    /// <param name="sslOption">SSL配置选项</param>
    public async Task AuthenticateAsync(ClientSslOption sslOption)
    {
        if (this.UseSsl)
        {
            return;
        }
        var sourceStream = new TransportStream(this);
        SslStream sslStream;
        if (sslOption.CertificateValidationCallback != null)
        {
            sslStream = new SslStream(sourceStream, false, sslOption.CertificateValidationCallback);
        }
        else
        {
            sslStream = new SslStream(sourceStream, false);
        }

        await sslStream.AuthenticateAsClientAsync(sslOption.TargetHost, sslOption.ClientCertificates, sslOption.SslProtocols, sslOption.CheckCertificateRevocation).ConfigureDefaultAwait();

        this.m_reader = PipeReader.Create(sslStream);
        this.m_writer = PipeWriter.Create(sslStream);
        this.UseSsl = true;
    }

    /// <summary>
    /// 同步关闭连接
    /// </summary>
    /// <returns>操作结果</returns>
    public Result Close()
    {
        try
        {
            var socket = this.m_socket;
            if (socket == null)
            {
                return Result.Success;
            }

            try
            {
                if (socket.Connected)
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
            }
            catch (Exception)
            {
                // Socket已经断开或未连接,忽略此异常
            }
            socket.Close(0);
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    /// <inheritdoc/>
    public override async Task<Result> CloseAsync(string msg, CancellationToken cancellationToken = default)
    {
        try
        {
            if (this.TryBeginClose(msg))
            {
                // 先发出关闭信号，解除所有基于 Pipe 的等待，再关闭底层 Socket。
                // 这样 SSL 包装层在 CompleteAsync 时不会继续等待底层管道读写自然结束。
                this.SignalClose();
                this.Close();
            }

            Task shutdownTask;
            lock (this.m_closeTaskLock)
            {
                this.m_shutdownTask ??= this.ShutdownAsync();
                shutdownTask = this.m_shutdownTask;
            }

            if (cancellationToken.CanBeCanceled)
            {
                await shutdownTask.WithCancellation(cancellationToken).ConfigureDefaultAwait();
            }
            else
            {
                await shutdownTask.ConfigureDefaultAwait();
            }

            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    /// <summary>
    /// 运行接收循环，优化了ValueTask的快速路径处理
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    protected override async Task RunReceive(CancellationToken cancellationToken)
    {
        var pipeWriter = this.m_pipeReceive.Writer;
        Exception error = null;

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (this.m_bufferOnDemand)
                {
                    var waitResult = await this.m_tcpCore.WaitForDataAsync().ConfigureDefaultAwait();

                    if (waitResult.SocketError != SocketError.Success)
                    {
                        this.m_closedEventArgs ??= new ClosedEventArgs(false, TouchSocketResource.RemoteDisconnects);
                        error = new SocketException((int)waitResult.SocketError);
                        break;
                    }
                }

                var memory = pipeWriter.GetMemory(this.ReceiveBufferSize);
                var receiveTask = this.m_tcpCore.ReceiveAsync(memory);

                TcpOperationResult result;

                // 快速路径：如果操作同步完成，避免异步机制的开销
                if (receiveTask.IsCompleted)
                {
                    result = receiveTask.Result;
                }
                else
                {
                    // 慢速路径：异步等待
                    result = await receiveTask.ConfigureDefaultAwait();
                }

                if (result.SocketError == SocketError.Success)
                {
                    if (result.BytesTransferred == 0)
                    {
                        // 远程连接已断开（收到 FIN）
                        this.m_closedEventArgs ??= new ClosedEventArgs(false, TouchSocketResource.RemoteDisconnects);
                        break;
                    }

                    // 更新接收计数器
                    this.m_receiveCounter.Increment(result.BytesTransferred);

                    // 推进缓冲区指针
                    pipeWriter.Advance(result.BytesTransferred);

                    // 刷新数据到管道
                    var flushTask = pipeWriter.FlushAsync(cancellationToken);

                    FlushResult flushResult;

                    // 快速路径：如果刷新同步完成
                    if (flushTask.IsCompleted)
                    {
                        flushResult = flushTask.Result;
                    }
                    else
                    {
                        // 慢速路径：异步等待刷新
                        flushResult = await flushTask.ConfigureDefaultAwait();
                    }

                    if (flushResult.IsCompleted || flushResult.IsCanceled)
                    {
                        break;
                    }
                }
                else
                {
                    // 处理接收失败的情况
                    this.m_closedEventArgs ??= new ClosedEventArgs(false, TouchSocketResource.RemoteDisconnects);
                    error = new SocketException((int)result.SocketError);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            this.m_closedEventArgs ??= new ClosedEventArgs(false, ex.Message);
            error = ex;
        }
        finally
        {
            // 无论以何种方式退出（break、return、异常），始终完成管道写入器。
            // 这确保 ReceiveLoopAsync 中的 ReadAsync(CancellationToken.None) 不会永久阻塞：
            // 当 FlushAsync 因 CancelPendingFlush() 返回 IsCanceled=true 而 break 退出时，
            // 若不在此处 Complete，而 CancelPendingRead() 标志已被消费，则 ReceiveLoopAsync 将死锁。
            await CompleteReceivePipeAsync(pipeWriter, error).ConfigureDefaultAwait();

            // 取消内置接收和发送任务
            base.m_tokenSource.SafeCancel();
        }
    }

    /// <summary>
    /// 运行发送循环，优化了ValueTask的快速路径处理
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    protected override async Task RunSend(CancellationToken cancellationToken)
    {
        var pipeReader = this.m_pipeSend.Reader;

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                //不订阅取消令牌，安全退出。
                var readTask = pipeReader.ReadAsync(CancellationToken.None);

                ReadResult readResult;

                // 快速路径：如果读取同步完成
                if (readTask.IsCompleted)
                {
                    readResult = readTask.Result;
                }
                else
                {
                    // 慢速路径：异步等待
                    readResult = await readTask.ConfigureDefaultAwait();
                }

                if (readResult.IsCanceled)
                {
                    break;
                }

                if (readResult.IsCompleted && readResult.Buffer.IsEmpty)
                {
                    break;
                }

                try
                {
                    // 发送数据
                    await this.m_tcpCore.SendAsync(readResult.Buffer).ConfigureDefaultAwait();

                    // 推进读取位置
                    pipeReader.AdvanceTo(readResult.Buffer.End);

                    // 更新发送计数器
                    this.m_sentCounter.Increment(readResult.Buffer.Length);

                    if (readResult.IsCompleted)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    // 处理发送失败的情况
                    pipeReader.Complete(ex);
                    break;
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            pipeReader.Complete(ex);
        }
        finally
        {
            pipeReader.Complete();
            // 发送循环退出时取消接收管道的挂起 Flush，
            // 确保 RunReceive 在背压场景（消费方慢导致 FlushAsync 阻塞）下也能立即解除阻塞。
            this.m_pipeReceive.Writer.CancelPendingFlush();
        }
    }

    /// <summary>
    /// 完成接收管道的辅助方法，优化异常处理路径
    /// </summary>
    /// <param name="pipeWriter">管道写入器</param>
    /// <param name="exception">可选的异常</param>
    private static async ValueTask CompleteReceivePipeAsync(PipeWriter pipeWriter, Exception exception)
    {
        if (exception == null)
        {
            await pipeWriter.CompleteAsync().ConfigureDefaultAwait();
        }
        else
        {
            await pipeWriter.CompleteAsync(exception).ConfigureDefaultAwait();
        }
    }

    private async Task ShutdownAsync()
    {
        await this.WaitForTransportClosedAsync().ConfigureDefaultAwait();

        if (this.m_writer != null)
        {
            await this.m_writer.CompleteAsync().ConfigureDefaultAwait();
        }

        if (this.m_reader != null)
        {
            await this.m_reader.CompleteAsync().ConfigureDefaultAwait();
        }
    }
}