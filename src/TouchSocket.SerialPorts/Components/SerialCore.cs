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
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using TouchSocket.Core;

namespace TouchSocket.SerialPorts;

/// <summary>
/// Serial核心
/// </summary>
internal class SerialCore : SafetyDisposableObject, IValueTaskSource<SerialOperationResult>
{
    private readonly CancellationTokenSource m_cancellationTokenSource = new();
    private readonly SemaphoreSlim m_receiveLock = new(0, 1);
    private readonly SemaphoreSlim m_semaphoreForSend = new SemaphoreSlim(1, 1);
    private readonly SerialPort m_serialPort;
    private readonly bool m_streamAsync;
    private ByteBlock m_byteBlock;
    private ManualResetValueTaskSourceCore<SerialOperationResult> m_core = new ManualResetValueTaskSourceCore<SerialOperationResult>();

    //private SerialData m_eventType;
    private int m_receiveBufferSize = 1024 * 10;

    private ValueCounter m_receiveCounter;

    private int m_sendBufferSize = 1024 * 10;

    private ValueCounter m_sendCounter;

    /// <summary>
    /// Serial核心
    /// </summary>
    public SerialCore(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits, bool streamAsync)
    {
        this.m_receiveCounter = new ValueCounter(TimeSpan.FromSeconds(1), this.OnReceivePeriod);

        this.m_sendCounter = new ValueCounter(TimeSpan.FromSeconds(1), this.OnSendPeriod);

        this.m_serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);

        this.m_streamAsync = streamAsync;

        if (!streamAsync)
        {
            this.m_serialPort.DataReceived += this.SerialCore_DataReceived;
        }
    }

    /// <summary>
    /// 最大缓存尺寸
    /// </summary>
    public int MaxBufferSize { get; set; } = 1024 * 1024 * 10;

    /// <summary>
    /// 最小缓存尺寸
    /// </summary>
    public int MinBufferSize { get; set; } = 1024 * 10;

    /// <summary>
    /// 接收缓存池,运行时的值会根据流速自动调整
    /// </summary>
    public int ReceiveBufferSize => this.m_receiveBufferSize;

    /// <summary>
    /// 接收计数器
    /// </summary>
    public ValueCounter ReceiveCounter => this.m_receiveCounter;

    /// <summary>
    /// 发送缓存池,运行时的值会根据流速自动调整
    /// </summary>
    public int SendBufferSize => this.m_sendBufferSize;

    /// <summary>
    /// 发送计数器
    /// </summary>
    public ValueCounter SendCounter => this.m_sendCounter;

    public SerialPort SerialPort => this.m_serialPort;

    SerialOperationResult IValueTaskSource<SerialOperationResult>.GetResult(short token)
    {
        return this.m_core.GetResult(token);
    }

    ValueTaskSourceStatus IValueTaskSource<SerialOperationResult>.GetStatus(short token)
    {
        return this.m_core.GetStatus(token);
    }

    void IValueTaskSource<SerialOperationResult>.OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
    {
        try
        {
            this.m_core.OnCompleted(continuation, state, token, flags);
        }
        catch (Exception ex)
        {
            // 如果可能，尝试通知异常
            try
            {
                this.m_core.SetException(ex);
            }
            catch
            {
                // 忽略SetException的异常，避免递归异常
            }
        }
    }

    public async Task<SerialOperationResult> ReceiveAsync(ByteBlock byteBlock, CancellationToken token)
    {
        if (this.m_streamAsync)
        {
            this.m_cancellationTokenSource.Token.ThrowIfCancellationRequested();
            // 如果是异步流，则使用异步读取方式
            var stream = this.m_serialPort.BaseStream;
            var memory = byteBlock.TotalMemory;
            var r = await stream.ReadAsync(memory, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            this.m_receiveCounter.Increment(r);
            return new SerialOperationResult(r, SerialData.Chars);
        }
        else
        {
            return await this.ValueReceiveAsync(byteBlock).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }

    public virtual async Task SendAsync(ReadOnlyMemory<byte> memory, CancellationToken token = default)
    {
        this.m_cancellationTokenSource.Token.ThrowIfCancellationRequested();
        await this.m_semaphoreForSend.WaitAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        try
        {
            var stream = this.m_serialPort.BaseStream;
            await stream.WriteAsync(memory, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            this.m_sendCounter.Increment(memory.Length);
        }
        finally
        {
            this.m_semaphoreForSend.Release();
        }
    }

    protected override void SafetyDispose(bool disposing)
    {
        if (this.DisposedValue)
        {
            return;
        }
        if (disposing)
        {
            try
            {
                // 取消未完成的任务源
                this.m_cancellationTokenSource.SafeCancel();
                this.m_cancellationTokenSource.SafeDispose();
                this.m_serialPort.DataReceived -= this.SerialCore_DataReceived;
                this.m_serialPort.Close();
                this.m_serialPort.SafeDispose();
                this.m_core.SetException(new ObjectDisposedException(nameof(SerialCore)));
            }
            catch
            {
            }
        }
    }

    private void OnReceivePeriod(long value)
    {
        this.m_receiveBufferSize = Math.Max(TouchSocketCoreUtility.HitBufferLength(value), this.MinBufferSize);
    }

    private void OnSendPeriod(long value)
    {
        this.m_sendBufferSize = Math.Max(TouchSocketCoreUtility.HitBufferLength(value), this.MinBufferSize);
    }

    private void SerialCore_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        var token = this.m_cancellationTokenSource.Token;
        if (token.IsCancellationRequested)
        {
            return;
        }

        try
        {
            var bytesToRead = this.m_serialPort.BytesToRead;
            while (bytesToRead > 0)
            {
                this.m_receiveLock.Wait(token);

                var eventType = e.EventType;

                var length = Math.Min(bytesToRead, this.m_byteBlock.Capacity);
                var bytes = this.m_byteBlock.TotalMemory.GetArray();
                var r = this.m_serialPort.Read(bytes.Array, 0, length);
                this.m_receiveCounter.Increment(r);
                var serialOperationResult = new SerialOperationResult(r, eventType);
                this.m_core.SetResult(serialOperationResult);
                bytesToRead -= r;
            }
        }
        catch (Exception ex)
        {
            // 设置任务源的异常
            this.m_core.SetException(ex);
        }
    }

    private void SetBuffer(ByteBlock byteBlock)
    {
        this.m_byteBlock = byteBlock;
    }

    private ValueTask<SerialOperationResult> ValueReceiveAsync(ByteBlock byteBlock)
    {
        this.m_core.Reset();
        this.SetBuffer(byteBlock);

        if (this.m_receiveLock.CurrentCount == 0)
        {
            this.m_receiveLock.Release();
        }

        return new ValueTask<SerialOperationResult>(this, this.m_core.Version);
    }
}