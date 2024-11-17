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

namespace TouchSocket.SerialPorts
{
    /// <summary>
    /// Serial核心
    /// </summary>
    internal class SerialCore : SerialPort, IValueTaskSource<SerialOperationResult>
    {
        private static readonly Action<object> s_continuationCompleted = _ => { };

        private volatile Action<object> m_continuation;
        private SerialData m_eventType;

        /// <summary>
        /// Serial核心
        /// </summary>
        public SerialCore(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits) :
            base(portName, baudRate, parity, dataBits, stopBits)
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

            this.DataReceived += this.SerialCore_DataReceived;
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~SerialCore()
        {
            this.Dispose(false);
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

        public SerialOperationResult GetResult(short token)
        {
            this.m_continuation = null;

            var length = Math.Min(this.BytesToRead, this.m_byteBlock.Capacity);
            var bytes = this.m_byteBlock.Memory.GetArray();
            var r = this.Read(bytes.Array, 0, length);

            return new SerialOperationResult(r, this.m_eventType);
        }

        public ValueTaskSourceStatus GetStatus(short token)
        {
            return !ReferenceEquals(this.m_continuation, s_continuationCompleted) ? ValueTaskSourceStatus.Pending : this.IsOpen ? ValueTaskSourceStatus.Succeeded : ValueTaskSourceStatus.Faulted;
        }

        public void OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
        {
            this.m_userToken = state;
            var prevContinuation = Interlocked.CompareExchange(ref this.m_continuation, continuation, null);
            if (ReferenceEquals(prevContinuation, s_continuationCompleted))
            {
                this.m_userToken = null;

                void Run(object o)
                {
                    continuation.Invoke(o);
                }

                ThreadPool.UnsafeQueueUserWorkItem(Run, state);
            }
        }

        public ValueTask<SerialOperationResult> ReceiveAsync(ByteBlock byteBlock)
        {
            this.SetBuffer(byteBlock);

            return new ValueTask<SerialOperationResult>(this, 0);
        }

        /// <summary>
        /// 发送数据。
        /// <para>
        /// 内部会根据是否启用Ssl，进行直接发送，还是Ssl发送。
        /// </para>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public virtual void Send(byte[] buffer, int offset, int length)
        {
            this.m_semaphoreForSend.Wait();
            try
            {
                this.Write(buffer, offset, length);
                this.m_sendCounter.Increment(length);
            }
            finally
            {
                this.m_semaphoreForSend.Release();
            }
        }

        
        public virtual async Task SendAsync(ReadOnlyMemory<byte> memory)
        {
            await this.m_semaphoreForSend.WaitAsync().ConfigureAwait(false);
            try
            {
                var segment = memory.GetArray();
                this.Write(segment.Array, segment.Offset, segment.Count);

                this.m_sendCounter.Increment(memory.Length);
            }
            finally
            {
                this.m_semaphoreForSend.Release();
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
            var c = this.m_continuation;

            if (c != null || (c = Interlocked.CompareExchange(ref this.m_continuation, s_continuationCompleted, null)) != null)
            {
                var continuationState = this.m_userToken;
                this.m_userToken = null;

                this.m_continuation = s_continuationCompleted; // in case someone's polling IsCompleted
                this.m_eventType = e.EventType;
                void Run(object o)
                {
                    c.Invoke(o);
                }
                ThreadPool.UnsafeQueueUserWorkItem(Run, continuationState);
            }
        }

        private void SetBuffer(ByteBlock byteBlock)
        {
            this.m_byteBlock = byteBlock;
        }

        #region 字段

        private readonly SemaphoreSlim m_semaphoreForSend = new SemaphoreSlim(1, 1);
        private ByteBlock m_byteBlock;
        private int m_receiveBufferSize = 1024 * 10;
        private ValueCounter m_receiveCounter;
        private int m_sendBufferSize = 1024 * 10;
        private ValueCounter m_sendCounter;
        private object m_userToken;

        #endregion 字段
    }
}