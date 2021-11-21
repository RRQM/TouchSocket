using RRQMCore.ByteManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 协议订阅等待
    /// </summary>
    public class WaitSenderSubscriber : SubscriberBase, IWaitSender
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="protocol"></param>
        public WaitSenderSubscriber(short protocol) : base(protocol)
        {
            this.waitData = new RRQMCore.Run.WaitData<byte[]>();
        }

        RRQMCore.Run.WaitData<byte[]> waitData;

        private int timeout = 60 * 1000;

        /// <summary>
        /// 超时设置
        /// </summary>
        public int Timeout
        {
            get { return timeout; }
            set { timeout = value; }
        }


        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="e"></param>
        protected override void OnReceived(ProtocolSubscriberEventArgs e)
        {
            e.Handled = true;
            byte[] data = e.ByteBlock.ToArray(2);
            this.waitData.Set(data);
        }

#pragma warning disable CS1591 
        public byte[] SendThenReturn(byte[] buffer, int offset, int length, CancellationToken token = default)
        {
            lock (this)
            {
                try
                {
                    this.client.Send(this.protocol, buffer, offset, length);
                    this.waitData.SetCancellationToken(token);

                    switch (this.waitData.Wait(this.timeout))
                    {
                        case RRQMCore.Run.WaitDataStatus.SetRunning:
                            {
                                return this.waitData.WaitResult;
                            }
                        case RRQMCore.Run.WaitDataStatus.Overtime:
                            throw new RRQMTimeoutException("操作已超时。");
                        case RRQMCore.Run.WaitDataStatus.Canceled:
                        case RRQMCore.Run.WaitDataStatus.Disposed:
                        case RRQMCore.Run.WaitDataStatus.Waiting:
                        default:
                            return default;
                    }
                }
                finally
                {
                    this.waitData.Reset();
                }
            }
        }

        public byte[] SendThenReturn(byte[] buffer, CancellationToken token = default)
        {
           return this.SendThenReturn(buffer,0,buffer.Length,token);
        }

        public byte[] SendThenReturn(ByteBlock byteBlock, CancellationToken token = default)
        {
            return this.SendThenReturn(byteBlock.Buffer, 0, byteBlock.Len, token);
        }

        public Task<byte[]> SendThenReturnAsync(byte[] buffer, int offset, int length, CancellationToken token = default)
        {
           return Task.Run(()=> 
            {
               return this.SendThenReturn(buffer,offset,length,token);
            });
        }

        public Task<byte[]> SendThenReturnAsync(byte[] buffer, CancellationToken token = default)
        {
           return this.SendThenReturnAsync(buffer,0,buffer.Length,token);
        }

        public Task<byte[]> SendThenReturnAsync(ByteBlock byteBlock, CancellationToken token = default)
        {
            return this.SendThenReturnAsync(byteBlock.Buffer, 0, byteBlock.Len, token);
        }
#pragma warning restore CS1591
    }
}
