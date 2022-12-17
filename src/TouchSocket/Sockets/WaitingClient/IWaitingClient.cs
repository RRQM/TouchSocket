using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 等待型客户端。
    /// </summary>
    public interface IWaitingClient<TClient> : IWaitSender where TClient : IClient, IDefaultSender, ISender
    {
        /// <summary>
        /// 等待设置。
        /// </summary>
        public WaitingOptions WaitingOptions { get; set; }

        /// <summary>
        /// 客户端终端
        /// </summary>
        TClient Client { get; }

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        ResponsedData SendThenResponse(byte[] buffer, int offset, int length, int timeout = 1000 * 5, CancellationToken token = default);

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        ResponsedData SendThenResponse(byte[] buffer, int timeout = 1000 * 5, CancellationToken token = default);

        /// <summary>
        /// 发送流中的有效数据
        /// </summary>
        /// <param name="byteBlock">数据块载体</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        ResponsedData SendThenResponse(ByteBlock byteBlock, int timeout = 1000 * 5, CancellationToken token = default);

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        Task<ResponsedData> SendThenResponseAsync(byte[] buffer, int offset, int length, int timeout = 1000 * 5, CancellationToken token = default);

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        Task<ResponsedData> SendThenResponseAsync(byte[] buffer, int timeout = 1000 * 5, CancellationToken token = default);

        /// <summary>
        /// 异步发送
        /// </summary>
        /// <param name="byteBlock">数据块载体</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        Task<ResponsedData> SendThenResponseAsync(ByteBlock byteBlock, int timeout = 1000 * 5, CancellationToken token = default);
    }
}
