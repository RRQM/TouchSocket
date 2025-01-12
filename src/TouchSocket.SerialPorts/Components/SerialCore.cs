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
using TouchSocket.Core;

namespace TouchSocket.SerialPorts
{
    /// <summary>
    /// Serial核心
    /// </summary>
    internal class SerialCore : ValueTaskSource<SerialOperationResult>
    {
        private readonly SemaphoreSlim m_semaphoreForSend = new SemaphoreSlim(1, 1);
        private readonly SerialPort m_serialPort;
        private ByteBlock m_byteBlock;
        private SerialData m_eventType;
        private int m_receiveBufferSize = 1024 * 10;

        private ValueCounter m_receiveCounter;

        private int m_sendBufferSize = 1024 * 10;

        private ValueCounter m_sendCounter;

        /// <summary>
        /// Serial核心
        /// </summary>
        public SerialCore(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
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
            this.m_serialPort.DataReceived += this.SerialCore_DataReceived;
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

        public ValueTask<SerialOperationResult> ReceiveAsync(ByteBlock byteBlock)
        {
            this.SetBuffer(byteBlock);

            return new ValueTask<SerialOperationResult>(this, 0);
        }

        public virtual async Task SendAsync(ReadOnlyMemory<byte> memory)
        {
            await this.m_semaphoreForSend.WaitAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            try
            {
                var segment = memory.GetArray();
                this.m_serialPort.Write(segment.Array, segment.Offset, segment.Count);

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
                    this.m_serialPort.Close();
                    this.m_serialPort.Dispose();
                }
                catch
                {
                }
            }
            base.Dispose(disposing);
        }

        protected override SerialOperationResult GetResult()
        {
            var length = Math.Min(this.m_serialPort.BytesToRead, this.m_byteBlock.Capacity);
            var bytes = this.m_byteBlock.Memory.GetArray();
            var r = this.m_serialPort.Read(bytes.Array, 0, length);

            return new SerialOperationResult(r, this.m_eventType);
        }

        protected override void Scheduler(Action<object> action, object state)
        {
            void Run(object o)
            {
                action.Invoke(o);
            }
            ThreadPool.UnsafeQueueUserWorkItem(Run, state);
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
            this.m_eventType = e.EventType;
            base.Complete(true);
        }

        private void SetBuffer(ByteBlock byteBlock)
        {
            this.m_byteBlock = byteBlock;
        }
    }
}