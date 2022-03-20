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
using RRQMCore;
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
    ///  <description><see cref="SendThenReturn(byte[], int, int, int, CancellationToken)"/>函数在执行时，为Lock同步。
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

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="e"></param>
        protected override void OnReceived(ProtocolSubscriberEventArgs e)
        {
            e.AddOperation(RRQMCore.Operation.Handled);
            byte[] data = e.ByteBlock.ToArray(2);
            this.waitData.Set(data);
        }

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

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="RRQMNotConnectedException">客户端没有连接</exception>
        /// <exception cref="RRQMOverlengthException">发送数据超长</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        /// <returns>返回的数据</returns>
        public byte[] SendThenReturn(byte[] buffer, int offset, int length, int timeout = 5000, CancellationToken token = default)
        {
            lock (this)
            {
                this.waitData.Reset();
                this.client.Send(this.protocol, buffer, offset, length);
                this.waitData.SetCancellationToken(token);

                switch (this.waitData.Wait(timeout))
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

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="RRQMNotConnectedException">客户端没有连接</exception>
        /// <exception cref="RRQMOverlengthException">发送数据超长</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        /// <returns>返回的数据</returns>
        public byte[] SendThenReturn(byte[] buffer, int timeout = 5000, CancellationToken token = default)
        {
            return this.SendThenReturn(buffer, 0, buffer.Length, timeout, token);
        }

        /// <summary>
        /// 发送流中的有效数据
        /// </summary>
        /// <param name="byteBlock">数据块载体</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="RRQMNotConnectedException">客户端没有连接</exception>
        /// <exception cref="RRQMOverlengthException">发送数据超长</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        /// <returns>返回的数据</returns>
        public byte[] SendThenReturn(ByteBlock byteBlock, int timeout = 5000, CancellationToken token = default)
        {
            return this.SendThenReturn(byteBlock.Buffer, 0, byteBlock.Len, timeout, token);
        }

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="RRQMNotConnectedException">客户端没有连接</exception>
        /// <exception cref="RRQMOverlengthException">发送数据超长</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        /// <returns>返回的数据</returns>
        public Task<byte[]> SendThenReturnAsync(byte[] buffer, int offset, int length, int timeout, CancellationToken token = default)
        {
            return Task.Run(() =>
             {
                 return this.SendThenReturn(buffer, offset, length, timeout, token);
             });
        }

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="RRQMNotConnectedException">客户端没有连接</exception>
        /// <exception cref="RRQMOverlengthException">发送数据超长</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        /// <returns>返回的数据</returns>
        public Task<byte[]> SendThenReturnAsync(byte[] buffer, int timeout = 5000, CancellationToken token = default)
        {
            return this.SendThenReturnAsync(buffer, 0, buffer.Length, timeout, token);
        }

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="byteBlock">数据块载体</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="RRQMNotConnectedException">客户端没有连接</exception>
        /// <exception cref="RRQMOverlengthException">发送数据超长</exception>
        /// <exception cref="RRQMException">其他异常</exception>
        /// <returns>返回的数据</returns>
        public Task<byte[]> SendThenReturnAsync(ByteBlock byteBlock, int timeout = 5000, CancellationToken token = default)
        {
            return this.SendThenReturnAsync(byteBlock.Buffer, 0, byteBlock.Len, timeout, token);
        }
    }
}