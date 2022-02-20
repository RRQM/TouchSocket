//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.ByteManager;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 协议订阅等待
    ///<list type="bullet">
    /// <listheader>
    ///  <term>使用注意事项</term>
    ///  <description><see cref="SendThenReturn(ByteBlock, CancellationToken)"/>函数在执行时，为Lock同步。
    ///  但是也有可能收到上次未返回的数据。</description>
    ///</listheader>
    ///</list>
    /// </summary>
    public class WaitSenderSubscriber : SubscriberBase, ISenderBase, IWaitSender
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="protocol"></param>
        public WaitSenderSubscriber(short protocol) : base(protocol)
        {
            this.waitData = new RRQMCore.Run.WaitData<byte[]>();
        }

        private RRQMCore.Run.WaitData<byte[]> waitData;

        private int timeout = 60 * 1000;

        /// <summary>
        /// 超时设置
        /// </summary>
        public int Timeout
        {
            get { return this.timeout; }
            set { this.timeout = value; }
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

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void Send(byte[] buffer, int offset, int length)
        {
            this.client.Send(this.Protocol, buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        public void Send(byte[] buffer)
        {
            this.Send(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        public void Send(ByteBlock byteBlock)
        {
            this.Send(byteBlock.Buffer, 0, byteBlock.Len);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void SendAsync(byte[] buffer, int offset, int length)
        {
            this.client.SendAsync(this.Protocol, buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        public void SendAsync(byte[] buffer)
        {
            this.SendAsync(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        public void SendAsync(ByteBlock byteBlock)
        {
            this.SendAsync(byteBlock.Buffer, 0, byteBlock.Len);
        }

        public byte[] SendThenReturn(byte[] buffer, int offset, int length, CancellationToken token = default)
        {
            lock (this)
            {
                this.waitData.Reset();
                this.client.Send(this.protocol, buffer, offset, length);
                this.waitData.SetCancellationToken(token);

                switch (this.waitData.Wait(this.timeout))
                {
                    case RRQMCore.Run.WaitDataStatus.SetRunning:
                        {
                            return this.waitData.WaitResult;
                        }
                    case RRQMCore.Run.WaitDataStatus.Overtime:
                        throw new TimeoutException("操作已超时。");
                    case RRQMCore.Run.WaitDataStatus.Canceled:
                    case RRQMCore.Run.WaitDataStatus.Disposed:
                    default:
                        return default;
                }
            }
        }

        public byte[] SendThenReturn(byte[] buffer, CancellationToken token = default)
        {
            return this.SendThenReturn(buffer, 0, buffer.Length, token);
        }

        public byte[] SendThenReturn(ByteBlock byteBlock, CancellationToken token = default)
        {
            return this.SendThenReturn(byteBlock.Buffer, 0, byteBlock.Len, token);
        }

        public Task<byte[]> SendThenReturnAsync(byte[] buffer, int offset, int length, CancellationToken token = default)
        {
            return Task.Run(() =>
             {
                 return this.SendThenReturn(buffer, offset, length, token);
             });
        }

        public Task<byte[]> SendThenReturnAsync(byte[] buffer, CancellationToken token = default)
        {
            return this.SendThenReturnAsync(buffer, 0, buffer.Length, token);
        }

        public Task<byte[]> SendThenReturnAsync(ByteBlock byteBlock, CancellationToken token = default)
        {
            return this.SendThenReturnAsync(byteBlock.Buffer, 0, byteBlock.Len, token);
        }

#pragma warning restore CS1591
    }
}