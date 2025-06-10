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
internal class SerialCore : DisposableObject, IValueTaskSource<SerialOperationResult>
{
    private readonly SemaphoreSlim m_receiveLock = new(0, 1);
    private readonly SemaphoreSlim m_semaphoreForSend = new SemaphoreSlim(1, 1);
    private readonly SerialPort m_serialPort;
    private ByteBlock m_byteBlock;
    private CancellationToken m_cancellationToken;
    private readonly CancellationTokenSource m_cancellationTokenSource = new();
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
        this.m_receiveCounter = new ValueCounter
        {
            Period = TimeSpan.FromSeconds(1),
            OnPeriod = this.OnReceivePeriod
        };

        this.m_sendCounter = new ValueCounter
        {
            Period = TimeSpan.FromSeconds(1),
            OnPeriod = this.OnSendPeriod
        };

        this.m_serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);

        if (streamAsync)
        {
            _ = Task.Run(this.BeginReceive);
        }
        else
        {
            this.m_serialPort.DataReceived += this.SerialCore_DataReceived;
        }

        this.m_cancellationToken = this.m_cancellationTokenSource.Token;
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
        this.m_core.OnCompleted(continuation, state, token, flags);
    }

    public ValueTask<SerialOperationResult> ReceiveAsync(ByteBlock byteBlock)
    {
        this.m_core.Reset();
        this.SetBuffer(byteBlock);

        if (this.m_receiveLock.CurrentCount == 0)
        {
            this.m_receiveLock.Release();
        }

        return new ValueTask<SerialOperationResult>(this, this.m_core.Version);
    }

    public virtual async Task SendAsync(ReadOnlyMemory<byte> memory)
    {
        await this.m_semaphoreForSend.WaitAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        try
        {
            var segment = memory.GetArray();
            await this.m_serialPort.BaseStream.WriteAsync(segment.Array, segment.Offset, segment.Count).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            this.m_sendCounter.Increment(memory.Length);
        }
        finally
        {
            this.m_semaphoreForSend.Release();
        }
    }

    protected override void Dispose(bool disposing)
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
                this.m_cancellationTokenSource.Cancel();
                this.m_cancellationTokenSource.Dispose();
                this.m_serialPort.DataReceived -= this.SerialCore_DataReceived;
                this.m_serialPort.Close();
                this.m_serialPort.Dispose();
                this.m_core.SetException(new ObjectDisposedException(nameof(SerialCore)));


            }
            catch
            {
            }
        }
        base.Dispose(disposing);
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
        if (this.m_cancellationToken.IsCancellationRequested)
        {
            return;
        }

        try
        {
            var bytesToRead = this.m_serialPort.BytesToRead;
            while (bytesToRead > 0)
            {
                this.m_receiveLock.Wait(this.m_cancellationToken);

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



    private async Task BeginReceive()
    {

        if (this.m_cancellationToken.IsCancellationRequested)
        {
            return;
        }

        while (!m_cancellationToken.IsCancellationRequested)
        {
            try
            {
                await this.m_receiveLock.WaitAsync(this.m_cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

                var bytes = this.m_byteBlock.TotalMemory.GetArray();



                var r = await this.m_serialPort.BaseStream.ReadAsync(bytes.Array, 0, this.m_byteBlock.Capacity, m_cancellationToken).ConfigureAwait(false);

                if (m_serialPort.BytesToRead > 0)
                    r += await this.m_serialPort.BaseStream.ReadAsync(bytes.Array, r, this.m_byteBlock.Capacity - r, m_cancellationToken).ConfigureFalseAwait();

                this.m_receiveCounter.Increment(r);
                var serialOperationResult = new SerialOperationResult(r, SerialData.Chars);
                this.m_core.SetResult(serialOperationResult);
            }
            catch (Exception ex)
            {
                // 设置任务源的异常
                this.m_core.SetException(ex);
            }
        }

    }

    private void SetBuffer(ByteBlock byteBlock)
    {
        this.m_byteBlock = byteBlock;
    }
}