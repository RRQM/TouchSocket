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
using System.Collections.Concurrent;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;

namespace TouchSocket.Sockets;

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

    private const int BatchSize = 100;
    private const int MaxMemoryLength = 1024;
    private readonly Task m_sendTask;
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
    private readonly AsyncResetEvent m_asyncResetEventForTask = new AsyncResetEvent(false, true);
    private readonly AsyncResetEvent m_asyncResetEventForSend = new AsyncResetEvent(true, false);
    private readonly ConcurrentQueue<SendSegment> m_sendingBytes = new ConcurrentQueue<SendSegment>();
    private readonly SemaphoreSlim m_semaphoreSlimForMax = new SemaphoreSlim(BatchSize);
    private ExceptionDispatchInfo m_exceptionDispatchInfo;
    private short m_version;
    private bool m_noDelay;
    private readonly SocketOperationResult m_result = new SocketOperationResult();
    private readonly CancellationTokenSource m_cancellationTokenSource = new();
    private readonly CancellationToken m_cancellationToken;
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
        this.m_cancellationToken = this.m_cancellationTokenSource.Token;
        this.m_sendTask = Task.Run(this.TaskSend);
        this.m_sendTask.FireAndForget();
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
        base.Dispose(disposing);
        if (disposing)
        {
            this.m_socketReceiver.SafeDispose();
            this.m_socketSender.SafeDispose();
            this.m_semaphoreForSend.SafeDispose();
            this.m_asyncResetEventForTask.SafeDispose();
            this.m_asyncResetEventForSend.SafeDispose();
            this.m_semaphoreSlimForMax.SafeDispose();
            this.m_cancellationTokenSource.Cancel();
            this.m_cancellationTokenSource.SafeDispose();
        }
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

    public bool NoDelay => this.m_noDelay;

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

    public async Task<Result> ShutdownAsync(SocketShutdown how)
    {
        //主要等待发送队列任务完成
        await this.m_semaphoreForSend.WaitAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        try
        {
            await this.m_asyncResetEventForSend.WaitOneAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            this.m_socket.Shutdown(how);

            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
        finally
        {
            this.m_semaphoreForSend.Release();
        }
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

    public async Task<SocketOperationResult> ReadAsync(Memory<byte> memory)
    {
        if (this.m_useSsl)
        {
#if NET6_0_OR_GREATER

            var r = await this.m_sslStream.ReadAsync(memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

#else
            var bytes = memory.GetArray();
            var r = await Task<int>.Factory.FromAsync(this.m_sslStream.BeginRead, this.m_sslStream.EndRead, bytes.Array, 0, bytes.Count, default).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
#endif
            this.m_result.BytesTransferred = r;
            this.m_receiveCounter.Increment(r);
            return this.m_result;
        }
        else
        {
            var result = await this.m_socketReceiver.ReceiveAsync(this.m_socket, memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            this.m_receiveCounter.Increment(result.BytesTransferred);
            return result;
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
        this.m_noDelay = socket.NoDelay;
    }

    /// <summary>
    /// 重置环境。
    /// </summary>
    public void Reset()
    {
        this.m_version++;
        this.m_exceptionDispatchInfo = null;
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
        await this.m_semaphoreForSend.WaitAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        try
        {
            // 如果内存长度小于最大长度，且不是无延迟模式
            if (memory.Length < MaxMemoryLength && !this.NoDelay)
            {
                var dispatchInfo = this.m_exceptionDispatchInfo;
                dispatchInfo?.Throw();

                await this.m_semaphoreSlimForMax.WaitAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);

                var sendSegment = ArrayPool<byte>.Shared.Rent(memory.Length);
                //if (!this.m_stores.TryPop(out var sendSegment))
                //{
                //    sendSegment = new SendSegment()
                //    {
                //        Date = new byte[MaxMemoryLength]
                //    };
                //}

                var segment = memory.GetArray();

                Array.Copy(segment.Array, segment.Offset, sendSegment, 0, segment.Count);


                this.m_sendingBytes.Enqueue(new SendSegment(sendSegment, memory.Length, this.m_version));
                //var byteBlock = new ValueByteBlock(memory.Length);
                //byteBlock.Write(memory.Span);
                //this.m_ints.Enqueue(byteBlock);

                this.m_asyncResetEventForSend.Reset();

                this.m_asyncResetEventForTask.Set();
                return;
            }

            await this.m_asyncResetEventForSend.WaitOneAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            await this.PrivateSendAsync(memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        finally
        {
            this.m_semaphoreForSend.Release();
        }
    }

    private async Task TaskSend()
    {
        var valuesToProcess = new SendSegment[BatchSize];

        while (!this.m_cancellationToken.IsCancellationRequested)
        {
            // 重置计数器和数组内容
            var count = 0;

            // 尝试填充数组
            while (count < BatchSize && this.m_sendingBytes.TryDequeue(out var value))
            {
                if (value.version == this.m_version)
                {
                    valuesToProcess[count++] = value;
                }
                this.m_semaphoreSlimForMax.Release();
            }

            if (this.m_cancellationToken.IsCancellationRequested)
            {
                return;
            }

            // 如果有元素需要处理，并且没有异常
            if (count > 0)
            {
                //Debug.WriteLine(count.ToString());
                if (this.m_exceptionDispatchInfo == null)
                {
                    using (var byteBlock = new ValueByteBlock(count * MaxMemoryLength))
                    {
                        for (var i = 0; i < count; i++)
                        {
                            var value = valuesToProcess[i];

                            byteBlock.Write(new ReadOnlySpan<byte>(value.Data, 0, value.Length));

                            ArrayPool<byte>.Shared.Return(value.Data);
                            //value.Dispose();
                        }

                        try
                        {
                            await this.PrivateSendAsync(byteBlock.Memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        }
                        catch (Exception ex)
                        {
                            this.m_exceptionDispatchInfo = ExceptionDispatchInfo.Capture(ex);
                            this.m_sendingBytes.Clear();

                        }
                    }
                }

                Array.Clear(valuesToProcess, 0, count);
            }
            else
            {
                //Debug.WriteLine("Pause");
                // 队列为空，设置事件并等待
                this.m_asyncResetEventForSend.Set();
                await this.m_asyncResetEventForTask.WaitOneAsync(this.m_cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
        }
    }

    private async Task PrivateSendAsync(ReadOnlyMemory<byte> memory)
    {
        var length = memory.Length;
#if NET6_0_OR_GREATER
        if (this.UseSsl)
        {
            await this.SslStream.WriteAsync(memory, this.m_cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
#else
        if (this.m_useSsl)
        {
            var segment = memory.GetArray();
            await this.SslStream.WriteAsync(segment.Array, segment.Offset, segment.Count, this.m_cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
#endif
        else
        {
            var offset = 0;
            while (length > 0 && !this.m_cancellationToken.IsCancellationRequested)
            {
                var result = await this.m_socketSender.SendAsync(this.m_socket, memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                if (result.SocketError != null)
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

    #endregion Send

    private void OnReceivePeriod(long value)
    {
        this.m_receiveBufferSize = Math.Max(TouchSocketCoreUtility.HitBufferLength(value), this.MinBufferSize);
    }

    private void OnSendPeriod(long value)
    {
        this.m_sendBufferSize = Math.Max(TouchSocketCoreUtility.HitBufferLength(value), this.MinBufferSize);
    }

    private struct SendSegment
    {
        public byte[] Data;
        public int Length;
        public short version;

        public SendSegment(byte[] bytes, int length, short version)
        {
            this.Data = bytes;
            this.Length = length;
            this.version = version;
        }
    }
}