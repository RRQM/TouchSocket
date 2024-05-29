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
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// Tcp核心
    /// </summary>
    internal sealed class TcpCore : DisposableObject
    {
        /// <summary>
        /// 最小缓存尺寸
        /// </summary>
        public int MinBufferSize { get; set; } = 1024 * 10;

        /// <summary>
        /// 最大缓存尺寸
        /// </summary>
        public int MaxBufferSize { get; set; } = 1024 * 512;

        #region 字段

        private int m_receiveBufferSize = 1024 * 10;
        private ValueCounter m_receiveCounter;
        private int m_sendBufferSize = 1024 * 10;
        private ValueCounter m_sentCounter;
        private Socket m_socket;
        private SslStream m_sslStream;
        private bool m_useSsl;
        private readonly SemaphoreSlim m_semaphoreForSend = new SemaphoreSlim(1, 1);
        private readonly SocketReceiver m_socketReceiver = new SocketReceiver();
        private readonly SocketSender m_socketSender = new SocketSender();
        #endregion 字段

        /// <summary>
        /// Tcp核心
        /// </summary>
        public TcpCore()
        {
            this.m_receiveCounter = new ValueCounter
            {
                Period = TimeSpan.FromSeconds(1),
                OnPeriod = this.OnReceivePeriod
            };

            this.m_sentCounter = new ValueCounter
            {
                Period = TimeSpan.FromSeconds(1),
                OnPeriod = this.OnSendPeriod
            };
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~TcpCore()
        {
            this.Dispose(disposing: false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.m_socketReceiver.SafeDispose();
                this.m_socketSender.SafeDispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// 接收缓存池,运行时的值会根据流速自动调整
        /// </summary>
        public int ReceiveBufferSize => Math.Min(Math.Max(this.m_receiveBufferSize, this.MinBufferSize), this.MaxBufferSize);

        /// <summary>
        /// 接收计数器
        /// </summary>
        public ValueCounter ReceiveCounter => this.m_receiveCounter;

        /// <summary>
        /// 发送缓存池,运行时的值会根据流速自动调整
        /// </summary>
        public int SendBufferSize => Math.Min(Math.Max(this.m_sendBufferSize, this.MinBufferSize), this.MaxBufferSize);

        /// <summary>
        /// 发送计数器
        /// </summary>
        public ValueCounter SendCounter => this.m_sentCounter;

        /// <summary>
        /// Socket
        /// </summary>
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
        public void Authenticate(ServiceSslOption sslOption)
        {
            if (this.m_useSsl)
            {
                return;
            }

            var sslStream = (sslOption.CertificateValidationCallback != null) ? new SslStream(new NetworkStream(this.m_socket, false), false, sslOption.CertificateValidationCallback) : new SslStream(new NetworkStream(this.m_socket, false), false);
            sslStream.AuthenticateAsServer(sslOption.Certificate);

            this.m_sslStream = sslStream;
            this.m_useSsl = true;
        }

        /// <summary>
        /// 以Ssl客户端模式授权
        /// </summary>
        /// <param name="sslOption"></param>
        public void Authenticate(ClientSslOption sslOption)
        {
            if (this.m_useSsl)
            {
                return;
            }

            var sslStream = (sslOption.CertificateValidationCallback != null) ? new SslStream(new NetworkStream(this.m_socket, false), false, sslOption.CertificateValidationCallback) : new SslStream(new NetworkStream(this.m_socket, false), false);
            if (sslOption.ClientCertificates == null)
            {
                sslStream.AuthenticateAsClient(sslOption.TargetHost);
            }
            else
            {
                sslStream.AuthenticateAsClient(sslOption.TargetHost, sslOption.ClientCertificates, sslOption.SslProtocols, sslOption.CheckCertificateRevocation);
            }
            this.m_sslStream = sslStream;
            this.m_useSsl = true;
        }

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
            await sslStream.AuthenticateAsServerAsync(sslOption.Certificate).ConfigureAwait(false);

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
                await sslStream.AuthenticateAsClientAsync(sslOption.TargetHost).ConfigureFalseAwait();
            }
            else
            {
                await sslStream.AuthenticateAsClientAsync(sslOption.TargetHost, sslOption.ClientCertificates, sslOption.SslProtocols, sslOption.CheckCertificateRevocation).ConfigureFalseAwait();
            }
            this.m_sslStream = sslStream;
            this.m_useSsl = true;
        }

        public async ValueTask<SocketOperationResult> ReadAsync(Memory<byte> memory)
        {
            SocketOperationResult result;
            if (this.m_useSsl)
            {
#if NET6_0_OR_GREATER

                var r = await this.m_sslStream.ReadAsync(memory).ConfigureAwait(false);

#else
                var bytes = memory.GetArray();
                var r = await Task<int>.Factory.FromAsync(this.m_sslStream.BeginRead, this.m_sslStream.EndRead, bytes.Array, 0, bytes.Count, default).ConfigureFalseAwait();
#endif
                result = new SocketOperationResult(r);
            }
            else
            {
                result = await this.m_socketReceiver.ReceiveAsync(this.m_socket, memory).ConfigureAwait(false);
            }

            this.m_receiveCounter.Increment(result.BytesTransferred);
            return result;
        }

        /// <summary>
        /// 重置环境，并设置新的<see cref="Socket"/>。
        /// </summary>
        /// <param name="socket"></param>
        public void Reset(Socket socket)
        {
            if (socket is null)
            {
                throw new ArgumentNullException(nameof(socket));
            }

            if (!socket.Connected)
            {
                throw new Exception(TouchSocketResource.SocketHaveToConnected);
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
            this.m_socketSender.Reset();
            this.m_receiveCounter.Reset();
            this.m_sentCounter.Reset();
            this.m_sslStream?.Dispose();
            this.m_sslStream = null;
            this.m_socket = null;
            this.m_receiveBufferSize = this.MinBufferSize;
            this.m_sendBufferSize = this.MinBufferSize;
        }

        #region Send

        /// <summary>
        /// 异步发送数据。
        /// <para>
        /// 内部会根据是否启用Ssl，进行直接发送，还是Ssl发送。
        /// </para>
        /// </summary>
        /// <param name="memory"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task SendAsync(ReadOnlyMemory<byte> memory)
        {
            await this.m_semaphoreForSend.WaitAsync().ConfigureFalseAwait();

            var length = memory.Length;
            try
            {
#if NET6_0_OR_GREATER
                if (this.UseSsl)
                {
                    await this.SslStream.WriteAsync(memory, CancellationToken.None).ConfigureAwait(false);
                }
#else

                if (this.m_useSsl)
                {
                    if (MemoryMarshal.TryGetArray(memory, out var segment))
                    {
                        await this.SslStream.WriteAsync(segment.Array, segment.Offset, segment.Count, CancellationToken.None).ConfigureFalseAwait();
                    }
                    else
                    {
                        var array = memory.ToArray();
                        await this.SslStream.WriteAsync(array, 0, array.Length, CancellationToken.None).ConfigureFalseAwait();
                    }
                }
#endif
                else
                {
                    var offset = 0;
                    while (length > 0)
                    {
                        var result = await this.m_socketSender.SendAsync(this.m_socket, memory).ConfigureAwait(false);
                        if (result.HasError)
                        {
                            throw result.SocketError;
                        }
                        if (result.BytesTransferred == 0 && length > 0)
                        {
                            throw new Exception(TouchSocketResource.IncompleteDataTransmission);
                        }
                        offset += result.BytesTransferred;
                        length -= result.BytesTransferred;
                    }

                    //this.m_socketSender.Reset();
                }
                this.m_sentCounter.Increment(length);
            }
            finally
            {
                this.m_semaphoreForSend.Release();
            }
        }
        #endregion


        private void OnReceivePeriod(long value)
        {
            this.m_receiveBufferSize = Math.Max(TouchSocketCoreUtility.HitBufferLength(value), this.MinBufferSize);
        }

        private void OnSendPeriod(long value)
        {
            this.m_sendBufferSize = Math.Max(TouchSocketCoreUtility.HitBufferLength(value), this.MinBufferSize);
        }
    }
}