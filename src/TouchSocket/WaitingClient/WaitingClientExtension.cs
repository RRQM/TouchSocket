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
        /// <typeparam name="TClient">客户端类型，必须同时实现<see cref="IReceiverClient{TResult}"/>和ISender接口。</typeparam>
        /// <typeparam name="TResult">接收结果类型，必须继承自IReceiverResult。</typeparam>
        /// <param name="client">要转换为可等待客户端的实例。</param>
        /// <param name="waitingOptions">等待选项，用于控制等待行为。</param>
        /// <returns>返回一个新的WaitingClient实例，该实例提供对原客户端的封装，并支持等待操作。</returns>
        public static IWaitingClient<TClient, TResult> CreateWaitingClient<TClient, TResult>(this TClient client, WaitingOptions waitingOptions) where TClient : IReceiverClient<TResult>, ISender
            where TResult : IReceiverResult
        {
            return new WaitingClient<TClient, TResult>(client, waitingOptions);
        }
        /// <summary>
        /// 为 IUdpSession 类型的客户端创建一个等待客户端。
        /// </summary>
        /// <param name="client">IUdpSession 类型的客户端实例。</param>
        /// <param name="waitingOptions">等待选项，用于配置等待客户端的行为。</param>
        /// <returns>返回一个实现了<see cref="IWaitingClient{TClient, TResult}"/>的等待客户端实例。</returns>
        public static IWaitingClient<IUdpSession, IUdpReceiverResult> CreateWaitingClient(this IUdpSession client, WaitingOptions waitingOptions)
        {
            return client.CreateWaitingClient<IUdpSession, IUdpReceiverResult>(waitingOptions);
        }

        /// <summary>
        /// 为 ITcpClient 类型的客户端创建一个等待客户端。
        /// </summary>
        /// <param name="client">ITcpClient 类型的客户端实例。</param>
        /// <param name="waitingOptions">等待选项，用于配置等待客户端的行为。</param>
        /// <returns>返回一个实现了<see cref="IWaitingClient{TClient, TResult}"/>的等待客户端实例。</returns>
        public static IWaitingClient<ITcpClient, IReceiverResult> CreateWaitingClient(this ITcpClient client, WaitingOptions waitingOptions)
        {
            return client.CreateWaitingClient<ITcpClient, IReceiverResult>(waitingOptions);
        }

        /// <summary>
        /// 为 ITcpSessionClient 类型的客户端创建一个等待客户端。
        /// </summary>
        /// <param name="client">ITcpSessionClient 类型的客户端实例。</param>
        /// <param name="waitingOptions">等待选项，用于配置等待客户端的行为。</param>
        /// <returns>返回一个实现了<see cref="IWaitingClient{TClient, TResult}"/>的等待客户端实例。</returns>
        public static IWaitingClient<ITcpSessionClient, IReceiverResult> CreateWaitingClient(this ITcpSessionClient client, WaitingOptions waitingOptions)
        {
            return client.CreateWaitingClient<ITcpSessionClient, IReceiverResult>(waitingOptions);
        }

        #region 异步发送

        /// <summary>
        /// 发送数据并等待
        /// </summary>
        /// <param name="client">等待客户端接口</param>
        /// <param name="msg">要发送的消息</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="ClientNotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public static Task<ResponsedData> SendThenResponseAsync<TClient, TResult>(this IWaitingClient<TClient, TResult> client, string msg, CancellationToken token)
            where TClient : IReceiverClient<TResult>, ISender
            where TResult : IReceiverResult
        {
            // 将字符串消息转换为字节数组，以便发送
            return client.SendThenResponseAsync(Encoding.UTF8.GetBytes(msg), token);
        }

        /// <summary>
        /// 发送数据并等待
        /// </summary>
        /// <param name="client">等待客户端接口</param>
        /// <param name="msg">要发送的消息</param>
        /// <param name="millisecondsTimeout">超时时间，默认为5000毫秒</param>
        /// <exception cref="ClientNotConnectedException">客户端没有连接时抛出的异常</exception>
        /// <exception cref="OverlengthException">发送数据超长时抛出的异常</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回从客户端接收到的数据</returns>
        public static Task<ResponsedData> SendThenResponseAsync<TClient, TResult>(this IWaitingClient<TClient, TResult> client, string msg, int millisecondsTimeout = 5000)
            where TClient : IReceiverClient<TResult>, ISender
            where TResult : IReceiverResult
        {
            // 将字符串消息转换为字节数组，以便发送
            return client.SendThenResponseAsync(Encoding.UTF8.GetBytes(msg), millisecondsTimeout);
        }

        /// <summary>
        /// 发送数据并等待响应
        /// </summary>
        /// <typeparam name="TClient">客户端类型，需实现<see cref="IReceiverClient{TResult}"/>和ISender接口</typeparam>
        /// <typeparam name="TResult">响应结果类型，需实现IReceiverResult接口</typeparam>
        /// <param name="client">用于发送和接收的客户端实例</param>
        /// <param name="memory">要发送的数据，以只读内存块形式</param>
        /// <param name="millisecondsTimeout">等待响应的超时时间（毫秒），默认为5000毫秒</param>
        /// <returns>返回接收到的响应数据</returns>
        /// <exception cref="TimeoutException">如果在指定的超时时间内没有收到响应，则抛出TimeoutException异常</exception>
        public static async Task<ResponsedData> SendThenResponseAsync<TClient, TResult>(this IWaitingClient<TClient, TResult> client, ReadOnlyMemory<byte> memory, int millisecondsTimeout = 5000)
            where TClient : IReceiverClient<TResult>, ISender
            where TResult : IReceiverResult
        {
            // 使用CancellationTokenSource来控制等待时间
            using (var tokenSource = new CancellationTokenSource(millisecondsTimeout))
            {
                // 尝试发送数据并等待响应
                try
                {
                    return await client.SendThenResponseAsync(memory, tokenSource.Token).ConfigureAwait(false);
                }
                // 捕获操作取消异常，并将其转换为超时异常
                catch (OperationCanceledException)
                {
                    throw new TimeoutException();
                }
            }
        }

        #endregion 异步发送

        #region 同步发送

        /// <summary>
        /// 向服务器发送数据并等待接收响应。
        /// </summary>
        /// <typeparam name="TClient">客户端类型，必须同时实现<see cref="IReceiverClient{TResult}"/>和ISender接口。</typeparam>
        /// <typeparam name="TResult">接收结果类型，必须实现IReceiverResult接口。</typeparam>
        /// <param name="client">实现等待客户端接口<see cref="IWaitingClient{TClient, TResult}"/>的实例。</param>
        /// <param name="memory">要发送的数据，以只读内存形式提供。</param>
        /// <param name="token">用于取消操作的取消令牌。</param>
        /// <returns>包含操作结果的ResponsedData对象。</returns>
        public static ResponsedData SendThenResponse<TClient, TResult>(this IWaitingClient<TClient, TResult> client, ReadOnlyMemory<byte> memory, CancellationToken token)
           where TClient : IReceiverClient<TResult>, ISender
           where TResult : IReceiverResult
        {
            // 调用异步版本的SendThenResponse方法，并直接获取其结果。
            return client.SendThenResponseAsync(memory, token).GetFalseAwaitResult();
        }

        /// <summary>
        /// 发送数据并等待
        /// </summary>
        /// <param name="client">等待客户端接口</param>
        /// <param name="msg">要发送的消息</param>
        /// <param name="token">取消令箭</param>
        /// <exception cref="ClientNotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public static ResponsedData SendThenResponse<TClient, TResult>(this IWaitingClient<TClient, TResult> client, string msg, CancellationToken token)
            where TClient : IReceiverClient<TResult>, ISender
            where TResult : IReceiverResult
        {
            // 将字符串消息转换为字节数组
            return client.SendThenResponse(Encoding.UTF8.GetBytes(msg), token);
        }

        /// <summary>
        /// 发送数据并等待
        /// </summary>
        /// <param name="client">等待客户端接口，用于发送数据并等待响应</param>
        /// <param name="msg">要发送的消息</param>
        /// <param name="millisecondsTimeout">等待响应的超时时间（以毫秒为单位），默认为5000毫秒</param>
        /// <exception cref="ClientNotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public static ResponsedData SendThenResponse<TClient, TResult>(this IWaitingClient<TClient, TResult> client, string msg, int millisecondsTimeout = 5000)
            where TClient : IReceiverClient<TResult>, ISender
            where TResult : IReceiverResult
        {
            // 将字符串消息转换为字节数组，以便发送
            return client.SendThenResponse(Encoding.UTF8.GetBytes(msg), millisecondsTimeout);
        }


        /// <summary>
        /// 发送数据并等待响应。
        /// </summary>
        /// <typeparam name="TClient">客户端类型，必须同时实现<see cref="IReceiverClient{TResult}"/>和ISender接口。</typeparam>
        /// <typeparam name="TResult">接收结果类型，必须实现IReceiverResult接口。</typeparam>
        /// <param name="client">等待客户端实例，用于发送数据并等待响应。</param>
        /// <param name="memory">要发送的数据，以只读内存形式提供。</param>
        /// <param name="millisecondsTimeout">等待响应的超时时间，以毫秒为单位，默认为5000毫秒（5秒）。</param>
        /// <returns>返回从服务端接收到的响应数据。</returns>
        /// <exception cref="TimeoutException">如果在指定的超时时间内没有收到响应，则抛出TimeoutException异常。</exception>
        public static ResponsedData SendThenResponse<TClient, TResult>(this IWaitingClient<TClient, TResult> client, ReadOnlyMemory<byte> memory, int millisecondsTimeout = 5000)
            where TClient : IReceiverClient<TResult>, ISender
            where TResult : IReceiverResult
        {
            // 使用CancellationTokenSource来控制等待时间
            using (var tokenSource = new CancellationTokenSource(millisecondsTimeout))
            {
                try
                {
                    // 实际发送数据并等待响应
                    return client.SendThenResponse(memory, tokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    // 如果操作因超时而被取消，则转换为TimeoutException异常抛出
                    throw new TimeoutException();
                }
            }
        }

        #endregion 同步发送
    }
}