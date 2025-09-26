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
    private readonly TcpCore m_tcpCore;
    private readonly Socket m_socket;
    private PipeReader m_reader;
    private PipeWriter m_writer;

    public override PipeReader Reader => this.m_reader ?? base.Reader;
    public override PipeWriter Writer => this.m_writer ?? base.Writer;

    public bool UseSsl { get; private set; }
    public TcpTransport(TcpCore tcpCore, TransportOption option) : base(option)
    {
        this.m_tcpCore = tcpCore ?? throw new ArgumentNullException(nameof(tcpCore));
        this.m_socket = tcpCore.Socket;
        this.Start();
    }

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

    public Result Close()
    {
        try
        {
            var socket = this.m_socket;
            if (!socket.Connected)
            {
                return Result.Success;
            }
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }


    protected override async Task RunReceive(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var memory = this.m_pipeReceive.Writer.GetMemory(this.ReceiveBufferSize);
                var result = await this.m_tcpCore.ReceiveAsync(memory, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                if (result.SocketError == SocketError.Success)
                {
                    if (result.BytesTransferred == 0)
                    {
                        this.m_closedEventArgs ??= new ClosedEventArgs(false, TouchSocketResource.RemoteDisconnects);
                        await this.m_pipeReceive.Writer.CompleteAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        return;
                    }

                    this.m_receiveCounter.Increment(result.BytesTransferred);

                    this.m_pipeReceive.Writer.Advance(result.BytesTransferred);
                    var flushResult = await this.m_pipeReceive.Writer.FlushAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    if (flushResult.IsCompleted)
                    {
                        break;
                    }
                }
                else
                {
                    // 处理接收失败的情况
                    this.m_closedEventArgs ??= new ClosedEventArgs(false, TouchSocketResource.RemoteDisconnects);
                    await this.m_pipeReceive.Writer.CompleteAsync(new SocketException((int)result.SocketError)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            this.m_closedEventArgs ??= new ClosedEventArgs(false, ex.Message);
            await this.m_pipeReceive.Writer.CompleteAsync(ex).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        finally
        {
            // 取消内置接收和发送任务。
            base.m_tokenSource.SafeCancel();
        }
    }

    protected override async Task RunSend(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var readResult = await this.m_pipeSend.Reader.ReadAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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
                    await this.m_tcpCore.SendAsync(readResult.Buffer, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    this.m_pipeSend.Reader.AdvanceTo(readResult.Buffer.End);

                    this.m_sentCounter.Increment(readResult.Buffer.Length);
                }
                catch (Exception ex)
                {
                    // 处理发送失败的情况
                    this.m_pipeSend.Reader.Complete(ex);
                    break;
                }

                if (readResult.IsCompleted)
                {
                    break;
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            this.m_pipeSend.Reader.Complete(ex);
        }
        finally
        {
            this.m_pipeSend.Reader.Complete();
        }
    }
}