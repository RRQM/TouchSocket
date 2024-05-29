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
        public static IWaitingClient<TClient, TResult> CreateWaitingClient<TClient, TResult>(this TClient client, WaitingOptions waitingOptions) where TClient : IReceiverClient<TResult>, ISender
            where TResult : IReceiverResult
        {
            return new WaitingClient<TClient, TResult>(client, waitingOptions);
        }

        public static IWaitingClient<IUdpSession, IUdpReceiverResult> CreateWaitingClient(this IUdpSession client, WaitingOptions waitingOptions)
        {
            return client.CreateWaitingClient<IUdpSession, IUdpReceiverResult>(waitingOptions);
        }

        public static IWaitingClient<ITcpClient, IReceiverResult> CreateWaitingClient(this ITcpClient client, WaitingOptions waitingOptions)
        {
            return client.CreateWaitingClient<ITcpClient, IReceiverResult>(waitingOptions);
        }

        public static IWaitingClient<ITcpSessionClient, IReceiverResult> CreateWaitingClient(this ITcpSessionClient client, WaitingOptions waitingOptions)
        {
            return client.CreateWaitingClient<ITcpSessionClient, IReceiverResult>(waitingOptions);
        }

        #region 异步发送

        ///// <summary>
        ///// 发送数据并等待
        ///// </summary>
        ///// <param name="client"></param>
        ///// <param name="buffer"></param>
        ///// <param name="token">取消令箭</param>
        ///// <exception cref="ClientNotConnectedException">客户端没有连接</exception>
        ///// <exception cref="OverlengthException">发送数据超长</exception>
        ///// <exception cref="Exception">其他异常</exception>
        ///// <returns>返回的数据</returns>
        //public static Task<ResponsedData> SendThenResponseAsync<TClient, TResult>(this IWaitingClient<TClient, TResult> client, byte[] buffer, CancellationToken token)
        //    where TClient : IReceiverClient<TResult>, ISender
        //    where TResult : IReceiverResult
        //{
        //    return client.SendThenResponseAsync(buffer, 0, buffer.Length, token);
        //}

        /// <summary>
        /// 发送数据并等待
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="ClientNotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public static Task<ResponsedData> SendThenResponseAsync<TClient, TResult>(this IWaitingClient<TClient, TResult> client, string msg, CancellationToken token)
            where TClient : IReceiverClient<TResult>, ISender
            where TResult : IReceiverResult
        {
            return client.SendThenResponseAsync(Encoding.UTF8.GetBytes(msg), token);
        }

        ///// <summary>
        ///// 发送数据并等待
        ///// </summary>
        ///// <param name="client"></param>
        ///// <param name="buffer"></param>
        ///// <param name="millisecondsTimeout">超时时间</param>
        ///// <exception cref="ClientNotConnectedException">客户端没有连接</exception>
        ///// <exception cref="OverlengthException">发送数据超长</exception>
        ///// <exception cref="Exception">其他异常</exception>
        ///// <returns>返回的数据</returns>
        //public static Task<ResponsedData> SendThenResponseAsync<TClient, TResult>(this IWaitingClient<TClient, TResult> client, byte[] buffer, int millisecondsTimeout = 5000)
        //    where TClient : IReceiverClient<TResult>, ISender
        //    where TResult : IReceiverResult
        //{
        //    return client.SendThenResponseAsync(buffer, 0, buffer.Length, millisecondsTimeout);
        //}

        /// <summary>
        /// 发送数据并等待
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        /// <param name="millisecondsTimeout">超时时间</param>
        /// <exception cref="ClientNotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public static Task<ResponsedData> SendThenResponseAsync<TClient, TResult>(this IWaitingClient<TClient, TResult> client, string msg, int millisecondsTimeout = 5000)
            where TClient : IReceiverClient<TResult>, ISender
            where TResult : IReceiverResult
        {
            return client.SendThenResponseAsync(Encoding.UTF8.GetBytes(msg), millisecondsTimeout);
        }

        /// <summary>
        /// 发送数据并等待
        /// </summary>
        /// <typeparam name="TClient">客户端泛型</typeparam>
        /// <param name="client">客户端</param>
        /// <param name="buffer">数据区</param>
        /// <param name="offset">数据偏移</param>
        /// <param name="length">长度</param>
        /// <param name="millisecondsTimeout">超时时间</param>
        /// <returns></returns>
        /// <exception cref="TimeoutException"></exception>
        public static async Task<ResponsedData> SendThenResponseAsync<TClient, TResult>(this IWaitingClient<TClient, TResult> client, Memory<byte> memory, int millisecondsTimeout = 5000)
            where TClient : IReceiverClient<TResult>, ISender
            where TResult : IReceiverResult
        {
            using (var tokenSource = new CancellationTokenSource(millisecondsTimeout))
            {
                try
                {
                    return await client.SendThenResponseAsync(memory, tokenSource.Token).ConfigureFalseAwait();
                }
                catch (OperationCanceledException)
                {
                    throw new TimeoutException();
                }
            }
        }

        #endregion 异步发送

        #region 同步发送

        public static ResponsedData SendThenResponse<TClient, TResult>(this IWaitingClient<TClient, TResult> client, ReadOnlyMemory<byte> memory, CancellationToken token)
           where TClient : IReceiverClient<TResult>, ISender
           where TResult : IReceiverResult
        {
            return client.SendThenResponseAsync(memory,token).GetFalseAwaitResult();
        }

        /// <summary>
        /// 发送数据并等待
        /// </summary>
        /// <param name="client"></param>
        /// <param name="buffer">数据缓存区</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="ClientNotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public static ResponsedData SendThenResponse<TClient, TResult>(this IWaitingClient<TClient, TResult> client, byte[] buffer, CancellationToken token)
            where TClient : IReceiverClient<TResult>, ISender
            where TResult : IReceiverResult
        {
            return client.SendThenResponse(new Memory<byte>(buffer), token);
        }

        /// <summary>
        /// 发送数据并等待
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="ClientNotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public static ResponsedData SendThenResponse<TClient, TResult>(this IWaitingClient<TClient, TResult> client, string msg, CancellationToken token)
            where TClient : IReceiverClient<TResult>, ISender
            where TResult : IReceiverResult
        {
            return client.SendThenResponse(Encoding.UTF8.GetBytes(msg), token);
        }

        /// <summary>
        /// 发送数据并等待
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <exception cref="ClientNotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public static ResponsedData SendThenResponse<TClient, TResult>(this IWaitingClient<TClient, TResult> client, string msg, int millisecondsTimeout = 5000)
            where TClient : IReceiverClient<TResult>, ISender
            where TResult : IReceiverResult
        {
            return client.SendThenResponse(Encoding.UTF8.GetBytes(msg), millisecondsTimeout);
        }

        /// <summary>
        /// 发送数据并等待
        /// </summary>
        /// <param name="client"></param>
        /// <param name="byteBlock">数据块载体</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="ClientNotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public static ResponsedData SendThenResponse<TClient, TResult>(this IWaitingClient<TClient, TResult> client, ByteBlock byteBlock, CancellationToken token)
            where TClient : IReceiverClient<TResult>, ISender
            where TResult : IReceiverResult
        {
            return client.SendThenResponse(byteBlock.Memory, token);
        }

        ///// <summary>
        ///// 发送数据并等待
        ///// </summary>
        ///// <param name="client"></param>
        ///// <param name="buffer">数据缓存区</param>
        ///// <param name="millisecondsTimeout">超时时间</param>
        ///// <exception cref="ClientNotConnectedException">客户端没有连接</exception>
        ///// <exception cref="OverlengthException">发送数据超长</exception>
        ///// <exception cref="Exception">其他异常</exception>
        ///// <returns>返回的数据</returns>
        //public static ResponsedData SendThenResponse<TClient, TResult>(this IWaitingClient<TClient, TResult> client, byte[] buffer, int millisecondsTimeout = 5000)
        //    where TClient : IReceiverClient<TResult>, ISender
        //    where TResult : IReceiverResult
        //{
        //    return SendThenResponse(client, buffer, 0, buffer.Length, millisecondsTimeout);
        //}

        ///// <summary>
        ///// 发送数据并等待
        ///// </summary>
        ///// <param name="client"></param>
        ///// <param name="byteBlock">数据块载体</param>
        ///// <param name="millisecondsTimeout">超时时间</param>
        ///// <exception cref="ClientNotConnectedException">客户端没有连接</exception>
        ///// <exception cref="OverlengthException">发送数据超长</exception>
        ///// <exception cref="Exception">其他异常</exception>
        ///// <returns>返回的数据</returns>
        //public static ResponsedData SendThenResponse<TClient, TResult>(this IWaitingClient<TClient, TResult> client, ByteBlock byteBlock, int millisecondsTimeout = 5000)
        //    where TClient : IReceiverClient<TResult>, ISender
        //    where TResult : IReceiverResult
        //{
        //    return SendThenResponse(client, byteBlock.Buffer, 0, byteBlock.Length, millisecondsTimeout);
        //}

        /// <summary>
        /// 发送数据并等待
        /// </summary>
        /// <typeparam name="TClient">客户端</typeparam>
        /// <param name="client"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <returns></returns>
        /// <exception cref="TimeoutException"></exception>
        public static ResponsedData SendThenResponse<TClient, TResult>(this IWaitingClient<TClient, TResult> client, Memory<byte> memory, int millisecondsTimeout = 5000)
            where TClient : IReceiverClient<TResult>, ISender
            where TResult : IReceiverResult
        {
            using (var tokenSource = new CancellationTokenSource(millisecondsTimeout))
            {
                try
                {
                    return client.SendThenResponse(memory, tokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    throw new TimeoutException();
                }
            }
        }

        #endregion 同步发送
    }
}