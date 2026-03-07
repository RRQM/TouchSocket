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

using System.IO.Ports;

namespace TouchSocket.SerialPorts;

/// <summary>
/// Serial核心
/// </summary>
internal sealed class SerialCore : SafetyDisposableObject
{
    private readonly CancellationTokenSource m_cancellationTokenSource = new();
    private readonly CircularBuffer<byte> m_circularBuffer = new CircularBuffer<byte>(1024 * 4);
    private readonly Lock m_eventlock = new Lock();
    private readonly Lock m_bufferlock = new Lock();
    private readonly SerialPort m_serialPort;
    private readonly bool m_streamAsync;
    private readonly SemaphoreSlim m_readSemaphore = new SemaphoreSlim(0, 1);
    private int m_readSemaphoreSignal = 0;
    /// <summary>
    /// Serial核心
    /// </summary>
    public SerialCore(SerialPort serialPort, bool streamAsync)
    {
        this.m_serialPort = serialPort;
        this.m_streamAsync = streamAsync;

        if (!streamAsync)
        {
            this.m_serialPort.DataReceived += this.SerialCore_DataReceived;
        }
    }

    public SerialPort SerialPort => this.m_serialPort;

    public async ValueTask<SerialOperationResult> ReceiveAsync(Memory<byte> memory, CancellationToken cancellationToken)
    {
        this.ThrowIfDisposed();

        if (this.m_streamAsync)
        {
            return await this.ReceiveFromStreamAsync(memory, cancellationToken).ConfigureDefaultAwait();
        }
        else
        {
            return await this.ReceiveFromEventAsync(memory, cancellationToken).ConfigureDefaultAwait();
        }
    }

    public async Task SendAsync(ReadOnlyMemory<byte> memory, CancellationToken cancellationToken = default)
    {
        this.ThrowIfDisposed();

        if (this.DisposedValue || this.m_cancellationTokenSource.Token.IsCancellationRequested)
        {
            ThrowHelper.ThrowObjectDisposedException(nameof(SerialCore));
        }

        try
        {
            if (this.m_streamAsync)
            {
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken,
                    this.m_cancellationTokenSource.Token);

                var stream = this.m_serialPort.BaseStream;
                await stream.WriteAsync(memory, linkedCts.Token).ConfigureDefaultAwait();
            }
            else
            {
                var bytes = memory.GetArray();
                this.m_serialPort.Write(bytes.Array, bytes.Offset, bytes.Count);
            }
        }
        catch (OperationCanceledException) when (this.DisposedValue || this.m_cancellationTokenSource.Token.IsCancellationRequested)
        {
            ThrowHelper.ThrowObjectDisposedException(nameof(SerialCore));
        }
        catch (IOException) when (this.DisposedValue)
        {
            ThrowHelper.ThrowObjectDisposedException(nameof(SerialCore));
        }
    }

    protected override void SafetyDispose(bool disposing)
    {
        if (disposing)
        {
            try
            {
                if (!this.m_streamAsync)
                {
                    this.m_serialPort.DataReceived -= this.SerialCore_DataReceived;
                }

                this.m_cancellationTokenSource.SafeCancel();

                if (this.m_serialPort.IsOpen)
                {
                    this.m_serialPort.Close();
                }

                this.m_serialPort.SafeDispose();
                this.m_cancellationTokenSource.SafeDispose();
                this.m_readSemaphore.SafeDispose();
            }
            catch
            {
            }
        }
    }

    private async ValueTask<SerialOperationResult> ReceiveFromStreamAsync(Memory<byte> memory, CancellationToken cancellationToken)
    {
        if (this.DisposedValue || this.m_cancellationTokenSource.Token.IsCancellationRequested)
        {
            ThrowHelper.ThrowObjectDisposedException(nameof(SerialCore));
        }

        try
        {
            var stream = this.m_serialPort.BaseStream;

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                this.m_cancellationTokenSource.Token);

            if (memory.IsEmpty)
            {
            return new SerialOperationResult(0, SerialData.Chars);
            }
            var bytes = memory.GetArray();
            var bytesRead = await stream.ReadAsync(bytes.Array, bytes.Offset, bytes.Count).ConfigureDefaultAwait();
            //判断是否还有数据，部分串口驱动在ReadAsync时，没有及时更新
            if (m_serialPort.BytesToRead > 0)
                bytesRead += await stream.ReadAsync(bytes.Array, bytes.Offset + bytesRead, bytes.Count - bytesRead).ConfigureDefaultAwait();

            return new SerialOperationResult(bytesRead, SerialData.Chars);
        }
        catch (OperationCanceledException) when (this.DisposedValue || this.m_cancellationTokenSource.Token.IsCancellationRequested)
        {
            ThrowHelper.ThrowObjectDisposedException(nameof(SerialCore));
            return default;
        }
        catch (IOException) when (this.DisposedValue)
        {
            ThrowHelper.ThrowObjectDisposedException(nameof(SerialCore));
            return default;
        }
    }

    private async ValueTask<SerialOperationResult> ReceiveFromEventAsync(Memory<byte> memory, CancellationToken cancellationToken)
    {
        this.ThrowIfDisposed();

        //read后，m_readSemaphore可能已经被释放了，导致跳过等待读到数量0
        //lock (this.m_bufferlock)
        //{
        //    if (!this.m_circularBuffer.IsEmpty)
        //    {
        //        var bytesRead = this.m_circularBuffer.Read(memory.Span);
        //        return new SerialOperationResult(bytesRead, SerialData.Chars);
        //    }
        //}

        try
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                this.m_cancellationTokenSource.Token);

            //循环等待，直到有数据可读或取消
            while (true)
            {
                //缓冲区中有数据，直接读取
                lock (this.m_bufferlock)
                {
                    this.ThrowIfDisposed();

                    if (!this.m_circularBuffer.IsEmpty)
                    {
                        var bytesRead = this.m_circularBuffer.Read(memory.Span);
                        return new SerialOperationResult(bytesRead, SerialData.Chars);
                    }
                }

                await this.m_readSemaphore.WaitAsync(linkedCts.Token).ConfigureDefaultAwait();

                //再次检查缓冲区中是否有数据
                try
                {
                    lock (this.m_bufferlock)
                    {
                        this.ThrowIfDisposed();

                        if (!this.m_circularBuffer.IsEmpty)
                        {
                            var bytesRead = this.m_circularBuffer.Read(memory.Span);
                            return new SerialOperationResult(bytesRead, SerialData.Chars);
                        }
                    }
                }
                finally
                {
                    //无条件重置信号，下次执行时再次检查是否有数据
                    Interlocked.Exchange(ref m_readSemaphoreSignal, 0);
                }
            }

        }
        catch (OperationCanceledException) when (this.DisposedValue || this.m_cancellationTokenSource.Token.IsCancellationRequested)
        {
            ThrowHelper.ThrowObjectDisposedException(nameof(SerialCore));
            return default;
        }
    }

    private void SerialCore_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        if (this.DisposedValue || this.m_cancellationTokenSource.Token.IsCancellationRequested)
        {
            return;
        }

        try
        {
            lock (this.m_eventlock)
            {
                if (this.DisposedValue)
                {
                    return;
                }

                var bytesToRead = this.m_serialPort.BytesToRead;
                if (bytesToRead <= 0)
                {
                    return;
                }

                var spinWait = new SpinWait();
                while (true)
                {
                    if (this.DisposedValue || this.m_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        return;
                    }
                    Memory<byte> buffer = default;
                    //区分事件触发和数据读写锁，防止事件频繁触发时，出现锁饥饿现象导致无法正常读取数据的情况
                    lock (this.m_bufferlock)
                    {
                        buffer = this.m_circularBuffer.GetWriteMemory();
                        if (!buffer.IsEmpty)
                        {
                            var toRead = Math.Min(buffer.Length, bytesToRead);
                            var read = this.m_serialPort.Read(buffer.Slice(0, toRead));
                            this.m_circularBuffer.AdvanceWrite(read);

                            break;
                        }
                        else
                        {
                            if (Interlocked.Exchange(ref m_readSemaphoreSignal, 1) == 0)
                            {
                                this.m_readSemaphore.Release();
                            }
                        }
                    }


                    spinWait.SpinOnce();
                }

                if (Interlocked.Exchange(ref m_readSemaphoreSignal, 1) == 0)
                {
                    this.m_readSemaphore.Release();
                }

                spinWait.SpinOnce();
            }
        }
        catch (Exception)
        {
        }
    }
}