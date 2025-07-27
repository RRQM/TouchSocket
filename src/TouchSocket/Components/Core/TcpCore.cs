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
using System.Buffers;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;

namespace TouchSocket.Sockets;

/// <summary>
/// Tcp核心
/// </summary>
internal sealed class TcpCore : SafetyDisposableObject
{
    /// <summary>
    /// 析构函数
    /// </summary>
    ~TcpCore()
    {
        this.Dispose(disposing: false);
    }

    #region 字段

    private readonly SocketReceiver m_socketReceiver = new SocketReceiver();
    private readonly SocketSender m_socketSender = new SocketSender();
    private Socket m_socket;
    private SslStream m_sslStream;
    private bool m_useSsl;

    #endregion 字段

    public bool SendRunContinuationsAsynchronously { get=>this.m_socketSender.RunContinuationsAsynchronously; set=> this.m_socketSender.RunContinuationsAsynchronously=value; }

    public bool ReceiveRunContinuationsAsynchronously { get=>this.m_socketReceiver.RunContinuationsAsynchronously; set=> this.m_socketReceiver.RunContinuationsAsynchronously=value; }

    public Socket Socket => this.m_socket;

    /// <summary>
    /// 提供一个用于客户端-服务器通信的流，该流使用安全套接字层 (SSL) 安全协议对服务器和（可选）客户端进行身份验证。
    /// </summary>
    public SslStream SslStream => this.m_sslStream;

    /// <summary>
    /// 是否启用了Ssl
    /// </summary>
    public bool UseSsl => this.m_useSsl;

    /// <summary>
    /// 以Ssl服务器模式授权
    /// </summary>
    /// <param name="sslOption"></param>
    /// <returns></returns>
    public async Task AuthenticateAsync(ServiceSslOption sslOption)
    {
        if (this.m_useSsl)
        {
            return;
        }
        var sslStream = (sslOption.CertificateValidationCallback != null) ? new SslStream(new NetworkStream(this.m_socket, false), false, sslOption.CertificateValidationCallback) : new SslStream(new NetworkStream(this.m_socket, false), false);

        await sslStream.AuthenticateAsServerAsync(sslOption.Certificate, sslOption.ClientCertificateRequired
            , sslOption.SslProtocols
            , sslOption.CheckCertificateRevocation).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        //await sslStream.AuthenticateAsServerAsync(sslOption.Certificate).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        this.m_sslStream = sslStream;
        this.m_useSsl = true;
    }

    /// <summary>
    /// 以Ssl客户端模式授权
    /// </summary>
    /// <param name="sslOption"></param>
    /// <returns></returns>
    public async Task AuthenticateAsync(ClientSslOption sslOption)
    {
        if (this.m_useSsl)
        {
            return;
        }

        var sslStream = (sslOption.CertificateValidationCallback != null) ? new SslStream(new NetworkStream(this.m_socket, false), false, sslOption.CertificateValidationCallback) : new SslStream(new NetworkStream(this.m_socket, false), false);
        if (sslOption.ClientCertificates == null)
        {
            await sslStream.AuthenticateAsClientAsync(sslOption.TargetHost).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        else
        {
            await sslStream.AuthenticateAsClientAsync(sslOption.TargetHost, sslOption.ClientCertificates, sslOption.SslProtocols, sslOption.CheckCertificateRevocation).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        this.m_sslStream = sslStream;
        this.m_useSsl = true;
    }

    #region Receive

    public ValueTask<TcpOperationResult> ReceiveAsync(in Memory<byte> memory, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (this.m_useSsl)
        {
            return this.SslReceiveAsync(memory, token);
        }
        return this.m_socketReceiver.ReceiveAsync(this.m_socket, memory);
    }

    private async ValueTask<TcpOperationResult> SslReceiveAsync(Memory<byte> memory, CancellationToken token)
    {
        var r = await this.m_sslStream.ReadAsync(memory, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        return new TcpOperationResult(r, SocketError.Success);
    }

    #endregion Receive

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

    /// <summary>
    /// 重置环境，并设置新的<see cref="Socket"/>。
    /// </summary>
    /// <param name="socket"></param>
    public void Reset(Socket socket)
    {
        ThrowHelper.ThrowArgumentNullExceptionIf(socket, nameof(socket));

        if (!socket.Connected)
        {
            ThrowHelper.ThrowException(TouchSocketResource.SocketHaveToConnected);
        }
        this.Reset();
        this.m_socket = socket;
    }

    /// <summary>
    /// 重置环境。
    /// </summary>
    public void Reset()
    {
        this.m_useSsl = false;
        this.m_sslStream?.Dispose();
        this.m_sslStream = null;
        this.m_socket = null;
    }

    protected override void SafetyDispose(bool disposing)
    {
        if (disposing)
        {
            this.m_socketReceiver.SafeDispose();
            this.m_socketSender.SafeDispose();
        }
    }

    public async Task SendAsync(ReadOnlySequence<byte> buffer, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var length = 0;
        if (this.m_useSsl)
        {
            foreach (var memory in buffer)
            {
                await this.SslStream.WriteAsync(memory, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                length += memory.Length;
            }
        }
        else
        {
            try
            {
                var result = await this.m_socketSender.SendAsync(this.m_socket, buffer).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                if (result.SocketError != SocketError.Success)
                {
                    throw new SocketException((int)result.SocketError);
                }

                foreach (var memory in buffer)
                {
                    length += memory.Length;
                }

                if (result.BytesTransferred != length)
                {
                    ThrowHelper.ThrowException(TouchSocketResource.IncompleteDataTransmission);
                }
            }
            finally
            {
                this.m_socketSender.Reset();
            }
        }
    }

}