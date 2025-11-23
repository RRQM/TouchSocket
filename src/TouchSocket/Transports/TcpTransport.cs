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
using System.Runtime.CompilerServices;
using TouchSocket.Resources;

namespace TouchSocket.Sockets;

internal sealed class TcpTransport : BaseTransport
{
    private readonly TcpCore m_tcpCore;
    private readonly Socket m_socket;
    private readonly bool m_bufferOnDemand;
    private PipeReader m_reader;
    private PipeWriter m_writer;

    /// <summary>
    /// 获取用于读取数据的管道读取器
    /// </summary>
    public override PipeReader Reader => this.m_reader ?? base.Reader;

    /// <summary>
    /// 获取用于写入数据的管道写入器
    /// </summary>
    public override PipeWriter Writer => this.m_writer ?? base.Writer;

    /// <summary>
    /// 获取是否使用SSL
    /// </summary>
    public bool UseSsl { get; private set; }

    public TcpTransport(TcpCore tcpCore, TransportOption option) : base(option)
    {
        this.m_tcpCore = tcpCore ?? throw new ArgumentNullException(nameof(tcpCore));
        this.m_socket = tcpCore.Socket;
        this.m_bufferOnDemand = option.BufferOnDemand;
        this.Start();
    }

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
            , sslOption.CheckCertificateRevocation).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

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

        await sslStream.AuthenticateAsClientAsync(sslOption.TargetHost, sslOption.ClientCertificates, sslOption.SslProtocols, sslOption.CheckCertificateRevocation).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        this.m_reader = PipeReader.Create(sslStream);
        this.m_writer = PipeWriter.Create(sslStream);
        this.UseSsl = true;
    }

    /// <inheritdoc/>
    public override async Task<Result> CloseAsync(string msg, CancellationToken cancellationToken = default)
    {
        try
        {
            if (this.m_writer != null)
            {
                // 完成发送管道
                await this.m_writer.CompleteAsync().SafeWaitAsync(cancellationToken);
            }

            if (this.m_reader != null)
            {
                // 等待发送管道读取器完成
                await this.m_reader.CompleteAsync().SafeWaitAsync(cancellationToken);
            }
            await base.CloseAsync(msg, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            this.Close();
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex.Message);
        }
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
            catch (SocketException)
            {
                // Socket已经断开或未连接,忽略此异常
            }
            catch (ObjectDisposedException)
            {
                // Socket已经被释放,忽略此异常
            }

            socket.Close();
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

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TcpOperationResult result;
                if (this.m_bufferOnDemand)
                {
                    result = await this.m_tcpCore.WaitForDataAsync();

                    if (result.SocketError != SocketError.Success)
                    {
                        // 处理接收失败的情况
                        this.m_closedEventArgs ??= new ClosedEventArgs(false, TouchSocketResource.RemoteDisconnects);
                        var socketException = new SocketException((int)result.SocketError);
                        await CompleteReceivePipeAsync(pipeWriter, socketException).ConfigureAwait(false);
                        break;
                    }
                }
                var memory = pipeWriter.GetMemory(this.ReceiveBufferSize);
                var receiveTask = this.m_tcpCore.ReceiveAsync(memory);



                // 快速路径：如果操作同步完成，避免异步机制的开销
                if (receiveTask.IsCompleted)
                {
                    result = receiveTask.Result;
                }
                else
                {
                    // 慢速路径：异步等待
                    result = await receiveTask.ConfigureAwait(false);
                }

                if (result.SocketError == SocketError.Success)
                {
                    if (result.BytesTransferred == 0)
                    {
                        // 远程连接已断开
                        this.m_closedEventArgs ??= new ClosedEventArgs(false, TouchSocketResource.RemoteDisconnects);
                        await CompleteReceivePipeAsync(pipeWriter, null).ConfigureAwait(false);
                        return;
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
                        flushResult = await flushTask.ConfigureAwait(false);
                    }

                    if (flushResult.IsCompleted)
                    {
                        break;
                    }
                }
                else
                {
                    // 处理接收失败的情况
                    this.m_closedEventArgs ??= new ClosedEventArgs(false, TouchSocketResource.RemoteDisconnects);
                    var socketException = new SocketException((int)result.SocketError);
                    await CompleteReceivePipeAsync(pipeWriter, socketException).ConfigureAwait(false);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            this.m_closedEventArgs ??= new ClosedEventArgs(false, ex.Message);
            await CompleteReceivePipeAsync(pipeWriter, ex).ConfigureAwait(false);
        }
        finally
        {
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
                    readResult = await readTask.ConfigureAwait(false);
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
                    await this.m_tcpCore.SendAsync(readResult.Buffer).ConfigureAwait(false);

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
        }
    }

    /// <summary>
    /// 完成接收管道的辅助方法，优化异常处理路径
    /// </summary>
    /// <param name="pipeWriter">管道写入器</param>
    /// <param name="exception">可选的异常</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static async ValueTask CompleteReceivePipeAsync(PipeWriter pipeWriter, Exception exception)
    {
        if (exception == null)
        {
            await pipeWriter.CompleteAsync().SafeWaitAsync().ConfigureAwait(false);
        }
        else
        {
            await pipeWriter.CompleteAsync(exception).SafeWaitAsync().ConfigureAwait(false);
        }
    }
}