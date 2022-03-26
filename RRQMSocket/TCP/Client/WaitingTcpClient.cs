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
using RRQMCore.Run;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 发送并等待结果返回的TCP客户端
    /// </summary>
    public class WaitingTcpClient : TcpClientBase, IWaitSender
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public WaitingTcpClient()
        {
            this.waitData = new WaitData<byte[]>();
        }

        WaitData<byte[]> waitData;

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
        public byte[] SendThenReturn(byte[] buffer, int offset, int length, int timeout = 1000 * 5, CancellationToken token = default)
        {
            lock (this)
            {
                waitData.Reset();
                this.Send(buffer, offset, length);
                this.waitData.SetCancellationToken(token);
                switch (this.waitData.Wait(timeout))
                {
                    case WaitDataStatus.SetRunning:
                        return waitData.WaitResult;
                    case WaitDataStatus.Overtime:
                        throw new TimeoutException();
                    case WaitDataStatus.Canceled:
                        {
                            return default;
                        }
                    case WaitDataStatus.Default:
                    case WaitDataStatus.Disposed:
                    default:
                        throw new RRQMException(RRQMCore.ResType.UnknownError.GetResString());
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
        public byte[] SendThenReturn(byte[] buffer, int timeout = 1000 * 5, CancellationToken token = default)
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
        public byte[] SendThenReturn(ByteBlock byteBlock, int timeout = 1000 * 5, CancellationToken token = default)
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
        public Task<byte[]> SendThenReturnAsync(byte[] buffer, int offset, int length, int timeout = 1000 * 5, CancellationToken token = default)
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
        public Task<byte[]> SendThenReturnAsync(byte[] buffer, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                return this.SendThenReturn(buffer, timeout, token);
            });
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
        public Task<byte[]> SendThenReturnAsync(ByteBlock byteBlock, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                return this.SendThenReturn(byteBlock, timeout, token);
            });
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        protected override void HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            this.waitData.Set(byteBlock.ToArray());
            base.HandleReceivedData(byteBlock, requestInfo);
        }
    }
}
