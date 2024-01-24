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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// WaitingClientExtensions
    /// </summary>
    public static class WaitingClientExtension
    {
        /// <summary>
        /// 创建可等待的客户端。
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="client"></param>
        /// <param name="waitingOptions"></param>
        /// <returns></returns>
        public static IWaitingClient<TClient> CreateWaitingClient<TClient>(this TClient client, WaitingOptions waitingOptions) where TClient : IClient, ISender
        {
            return new WaitingClient<TClient>(client, waitingOptions);
        }

        /// <summary>
        /// 获取筛选条件的可等待的客户端。
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="client"></param>
        /// <param name="waitingOptions"></param>
        /// <param name="func">当条件成立时返回</param>
        /// <returns></returns>
        [Obsolete("本方法已被弃用，请使用CreateWaitingClient替代", true)]
        public static IWaitingClient<TClient> GetWaitingClient<TClient>(this TClient client, WaitingOptions waitingOptions, Func<ResponsedData, bool> func) where TClient : IClient, ISender
        {
            return new WaitingClient<TClient>(client, waitingOptions);
        }

        /// <summary>
        /// 获取可等待的客户端。
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="client"></param>
        /// <param name="waitingOptions"></param>
        /// <returns></returns>
        [Obsolete("本方法已被弃用，请使用CreateWaitingClient替代", true)]
        public static IWaitingClient<TClient> GetWaitingClient<TClient>(this TClient client, WaitingOptions waitingOptions) where TClient : IClient, ISender
        {
            return new WaitingClient<TClient>(client, waitingOptions);
        }

        #region 发送

        /// <summary>
        /// 发送数据并等待
        /// </summary>
        /// <param name="client"></param>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public static ResponsedData SendThenResponse<TClient>(this IWaitingClient<TClient> client, byte[] buffer, CancellationToken token)
            where TClient : IClient, ISender
        {
            return client.SendThenResponse(buffer, 0, buffer.Length, token);
        }

        /// <summary>
        /// 发送数据并等待
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public static ResponsedData SendThenResponse<TClient>(this IWaitingClient<TClient> client, string msg, CancellationToken token)
            where TClient : IClient, ISender
        {
            return client.SendThenResponse(Encoding.UTF8.GetBytes(msg), token);
        }

        /// <summary>
        /// 发送数据并等待
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        /// <param name="timeout"></param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public static ResponsedData SendThenResponse<TClient>(this IWaitingClient<TClient> client, string msg, int timeout = 5000)
            where TClient : IClient, ISender
        {
            return client.SendThenResponse(Encoding.UTF8.GetBytes(msg), timeout);
        }

        /// <summary>
        /// 发送数据并等待
        /// </summary>
        /// <param name="client"></param>
        /// <param name="byteBlock">数据块载体</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public static ResponsedData SendThenResponse<TClient>(this IWaitingClient<TClient> client, ByteBlock byteBlock, CancellationToken token)
            where TClient : IClient, ISender
        {
            return client.SendThenResponse(byteBlock.Buffer, 0, byteBlock.Len, token);
        }

        /// <summary>
        /// 发送数据并等待
        /// </summary>
        /// <param name="client"></param>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="timeout">超时时间</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public static ResponsedData SendThenResponse<TClient>(this IWaitingClient<TClient> client, byte[] buffer, int timeout = 5000)
            where TClient : IClient, ISender
        {
            using (var tokenSource = new CancellationTokenSource(timeout))
            {
                return client.SendThenResponse(buffer, 0, buffer.Length, tokenSource.Token);
            }
        }

        /// <summary>
        /// 发送数据并等待
        /// </summary>
        /// <param name="client"></param>
        /// <param name="byteBlock">数据块载体</param>
        /// <param name="timeout">超时时间</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public static ResponsedData SendThenResponse<TClient>(this IWaitingClient<TClient> client, ByteBlock byteBlock, int timeout = 5000)
            where TClient : IClient, ISender
        {
            using (var tokenSource = new CancellationTokenSource(timeout))
            {
                return client.SendThenResponse(byteBlock.Buffer, 0, byteBlock.Len, tokenSource.Token);
            }
        }

        /// <summary>
        /// 发送数据并等待
        /// </summary>
        /// <param name="client"></param>
        /// <param name="buffer"></param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public static Task<ResponsedData> SendThenResponseAsync<TClient>(this IWaitingClient<TClient> client, byte[] buffer, CancellationToken token)
            where TClient : IClient, ISender
        {
            return client.SendThenResponseAsync(buffer, 0, buffer.Length, token);
        }

        /// <summary>
        /// 发送数据并等待
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public static Task<ResponsedData> SendThenResponseAsync<TClient>(this IWaitingClient<TClient> client, string msg, CancellationToken token)
            where TClient : IClient, ISender
        {
            return client.SendThenResponseAsync(Encoding.UTF8.GetBytes(msg), token);
        }

        /// <summary>
        /// 发送数据并等待
        /// </summary>
        /// <param name="client"></param>
        /// <param name="buffer"></param>
        /// <param name="timeout">超时时间</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public static Task<ResponsedData> SendThenResponseAsync<TClient>(this IWaitingClient<TClient> client, byte[] buffer, int timeout = 5000)
            where TClient : IClient, ISender
        {
            using (var tokenSource = new CancellationTokenSource(timeout))
            {
                return client.SendThenResponseAsync(buffer, 0, buffer.Length, tokenSource.Token);
            }
        }

        /// <summary>
        /// 发送数据并等待
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        /// <param name="timeout">超时时间</param>
        /// <exception cref="NotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public static Task<ResponsedData> SendThenResponseAsync<TClient>(this IWaitingClient<TClient> client, string msg, int timeout = 5000)
            where TClient : IClient, ISender
        {
            using (var tokenSource = new CancellationTokenSource(timeout))
            {
                return client.SendThenResponseAsync(Encoding.UTF8.GetBytes(msg), tokenSource.Token);
            }
        }

        #endregion 发送
    }
}