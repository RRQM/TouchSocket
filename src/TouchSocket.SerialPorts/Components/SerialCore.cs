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

using System.Buffers;
using System.IO.Ports;

namespace TouchSocket.SerialPorts;

/// <summary>
/// Serial核心
/// </summary>
internal sealed class SerialCore : SafetyDisposableObject
{
    private readonly CancellationTokenSource m_cancellationTokenSource = new();
    private readonly SegmentedPipe m_eventPipe = new SegmentedPipe();
    private readonly Lock m_lock = new Lock();
    private readonly SerialPort m_serialPort;
    private readonly bool m_streamAsync;
    private readonly SemaphoreSlim m_readSemaphore = new SemaphoreSlim(0, 1);

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
                this.m_eventPipe.SafeDispose();
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

            var bytesRead = await stream.ReadAsync(memory, linkedCts.Token).ConfigureDefaultAwait();
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
        //pr:https://github.com/RRQM/TouchSocket/pull/122
        //结合pr，使用SegmentedPipe修复。
        this.ThrowIfDisposed();

        lock (this.m_lock)
        {
            if (this.m_eventPipe.Count > 0)
            {
                return this.DequeueFromEventPipe(memory);
            }
        }

        try
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken,
                this.m_cancellationTokenSource.Token);

            await this.m_readSemaphore.WaitAsync(linkedCts.Token).ConfigureDefaultAwait();

            lock (this.m_lock)
            {
                this.ThrowIfDisposed();

                if (this.m_eventPipe.Count > 0)
                {
                    return this.DequeueFromEventPipe(memory);
                }

                return new SerialOperationResult(0, SerialData.Chars);
            }
        }
        catch (OperationCanceledException) when (this.DisposedValue || this.m_cancellationTokenSource.Token.IsCancellationRequested)
        {
            ThrowHelper.ThrowObjectDisposedException(nameof(SerialCore));
            return default;
        }
    }

    /// <summary>
    /// 从事件管道中读取数据，调用方须持有 <see cref="m_lock"/>。
    /// </summary>
    private SerialOperationResult DequeueFromEventPipe(Memory<byte> memory)
    {
        var result = this.m_eventPipe.Reader.Read();
        var buffer = result.Buffer;
        var toRead = (int)Math.Min(memory.Length, buffer.Length);
        buffer.Slice(0, toRead).CopyTo(memory.Span);
        this.m_eventPipe.Reader.AdvanceTo(buffer.GetPosition(toRead));
        return new SerialOperationResult(toRead, SerialData.Chars);
    }

    private void SerialCore_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        if (this.DisposedValue || this.m_cancellationTokenSource.Token.IsCancellationRequested)
        {
            return;
        }

        try
        {
            lock (this.m_lock)
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

                var mem = this.m_eventPipe.Writer.GetMemory(bytesToRead);
                var read = this.m_serialPort.Read(mem.Slice(0, bytesToRead));
                if (read <= 0)
                {
                    return;
                }

                this.m_eventPipe.Writer.Advance(read);
            }

            // 在锁外释放信号量，避免唤醒的接收方立即争锁；
            // 用 try/catch 代替 CurrentCount 判断，消除竞态。
            try
            {
                this.m_readSemaphore.Release();
            }
            catch (SemaphoreFullException)
            {
                // 接收方尚未消费上次信号，队列数据不丢失，下次 ReceiveFromEventAsync 快速路径可读取。
            }
        }
        catch (Exception)
        {
        }
    }
}