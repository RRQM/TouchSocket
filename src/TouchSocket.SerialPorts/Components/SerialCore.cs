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
using System.Threading.Tasks.Sources;

namespace TouchSocket.SerialPorts;

/// <summary>
/// Serial核心
/// </summary>
internal sealed class SerialCore : SafetyDisposableObject, IValueTaskSource<SerialOperationResult>
{
    private readonly CancellationTokenSource m_cancellationTokenSource = new();
    private readonly CircularBuffer<byte> m_circularBuffer = new CircularBuffer<byte>(1024 * 4);
    private readonly Lock m_lock = new Lock();
    private readonly SerialPort m_serialPort;
    private readonly bool m_streamAsync;
    private ManualResetValueTaskSourceCore<SerialOperationResult> m_core = new ManualResetValueTaskSourceCore<SerialOperationResult>();
    private Memory<byte> m_memory;
    private volatile bool m_disposed = false;
    private volatile bool m_hasWaitingReceiver = false; // 标记是否有等待中的接收者

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

    SerialOperationResult IValueTaskSource<SerialOperationResult>.GetResult(short cancellationToken)
    {
        return this.m_core.GetResult(cancellationToken);
    }

    ValueTaskSourceStatus IValueTaskSource<SerialOperationResult>.GetStatus(short cancellationToken)
    {
        return this.m_core.GetStatus(cancellationToken);
    }

    void IValueTaskSource<SerialOperationResult>.OnCompleted(Action<object> continuation, object state, short cancellationToken, ValueTaskSourceOnCompletedFlags flags)
    {
        this.m_core.OnCompleted(continuation, state, cancellationToken, flags);
    }

    public async ValueTask<SerialOperationResult> ReceiveAsync(Memory<byte> memory, CancellationToken cancellationToken)
    {
        this.ThrowIfDisposed();

        if (this.m_streamAsync)
        {
            this.m_cancellationTokenSource.Token.ThrowIfCancellationRequested();
            // 如果是异步流，则使用异步读取方式
            var stream = this.m_serialPort.BaseStream;
            var r = await stream.ReadAsync(memory, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return new SerialOperationResult(r, SerialData.Chars);
        }
        else
        {
            return await this.ValueReceiveAsync(memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }

    public async Task SendAsync(ReadOnlyMemory<byte> memory, CancellationToken cancellationToken = default)
    {
        this.ThrowIfDisposed();
        this.m_cancellationTokenSource.Token.ThrowIfCancellationRequested();

        if (this.m_streamAsync)
        {
            var stream = this.m_serialPort.BaseStream;
            await stream.WriteAsync(memory, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        else
        {
            var bytes = memory.GetArray();
            this.m_serialPort.Write(bytes.Array, bytes.Offset, bytes.Count);
        }
    }

    protected override void SafetyDispose(bool disposing)
    {
        if (disposing)
        {
            this.m_disposed = true;

            try
            {
                // 首先取消令牌，阻止新的操作
                this.m_cancellationTokenSource.SafeCancel();

                // 在持有锁的情况下处理任务源，确保线程安全
                lock (this.m_lock)
                {
                    if (this.m_hasWaitingReceiver)
                    {
                        this.m_core.SetException(new ObjectDisposedException(nameof(SerialCore)));
                        this.m_hasWaitingReceiver = false;
                    }
                }

                // 移除事件处理器（在串口关闭之前）
                this.m_serialPort.DataReceived -= this.SerialCore_DataReceived;

                // 关闭串口
                if (this.m_serialPort.IsOpen)
                {
                    this.m_serialPort.Close();
                }

                // 释放资源
                this.m_serialPort.SafeDispose();
                this.m_cancellationTokenSource.SafeDispose();
            }
            catch
            {
                // 释放过程中的异常应该被忽略
            }
        }
    }

    private void SerialCore_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        if (this.m_disposed || this.m_cancellationTokenSource.Token.IsCancellationRequested)
        {
            return;
        }

        try
        {
            lock (this.m_lock)
            {
                if (this.m_disposed) // 双重检查
                {
                    return;
                }

                if (!this.m_hasWaitingReceiver)
                {
                    // 如果没有等待接收，则放入环形缓冲区
                    var spinWait = new SpinWait();
                    Memory<byte> buffer;

                    do
                    {
                        if (this.m_disposed || this.m_cancellationTokenSource.Token.IsCancellationRequested)
                        {
                            return;
                        }
                        buffer = this.m_circularBuffer.GetWriteMemory();
                        if (!buffer.IsEmpty)
                        {
                            break;
                        }
                        spinWait.SpinOnce();
                    }
                    while (true);

                    var r = this.m_serialPort.Read(buffer);
                    this.m_circularBuffer.AdvanceWrite(r);
                }
                else
                {
                    // 有等待的接收者，直接读取到其缓冲区
                    if (!this.m_memory.IsEmpty)
                    {
                        var buffer = this.m_memory.GetArray();
                        var r = this.m_serialPort.Read(buffer);
                        var serialOperationResult = new SerialOperationResult(r, e.EventType);

                        // 清除等待状态
                        this.m_hasWaitingReceiver = false;
                        this.m_memory = Memory<byte>.Empty;

                        // 设置结果
                        this.m_core.SetResult(serialOperationResult);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            lock (this.m_lock)
            {
                if (this.m_hasWaitingReceiver)
                {
                    this.m_hasWaitingReceiver = false;
                    this.m_memory = Memory<byte>.Empty;
                    this.m_core.SetException(ex);
                }
            }
        }
    }

    private ValueTask<SerialOperationResult> ValueReceiveAsync(Memory<byte> memory)
    {
        lock (this.m_lock)
        {
            this.ThrowIfDisposed();

            // 首先检查缓冲区是否有数据
            if (!this.m_circularBuffer.IsEmpty)
            {
                var r = this.m_circularBuffer.Read(memory.Span);
                return new ValueTask<SerialOperationResult>(new SerialOperationResult(r, SerialData.Chars));
            }

            // 确保没有其他等待中的接收者
            if (this.m_hasWaitingReceiver)
            {
                throw new InvalidOperationException("已有一个接收操作正在等待中");
            }

            // 重置任务源并设置等待状态
            this.m_core.Reset();
            this.m_memory = memory;
            this.m_hasWaitingReceiver = true;

            return new ValueTask<SerialOperationResult>(this, this.m_core.Version);
        }
    }
}