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
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 发送者扩展类
    /// </summary>
    public static class SenderExtension
    {
        #region ISend

        /// <summary>
        /// 异步发送数据。
        /// </summary>
        /// <typeparam name="TClient">发送器类型参数，必须实现ISender接口。</typeparam>
        /// <param name="client">发送器实例。</param>
        /// <param name="memory">待发送的字节内存块，使用<see cref="ReadOnlyMemory{T}"/>类型以强调数据不会被修改。</param>
        public static void Send<TClient>(this TClient client, ReadOnlyMemory<byte> memory) where TClient : ISender
        {
            // 调用SendAsync方法发送数据，并立即返回，不等待发送完成。这种设计用于提高性能，特别是在高负载情况下。
            client.SendAsync(memory).GetFalseAwaitResult();
        }

        /// <summary>
        /// 以UTF-8的编码同步发送字符串。
        /// </summary>
        /// <typeparam name="TClient">发送者类型，必须实现ISender接口。</typeparam>
        /// <param name="client">发送者实例。</param>
        /// <param name="value">待发送的字符串。</param>
        public static void Send<TClient>(this TClient client, string value) where TClient : ISender
        {
            // 将字符串转换为UTF-8编码的字节序列，然后发送。
            client.Send(Encoding.UTF8.GetBytes(value));
        }

        /// <summary>
        /// 以UTF-8的编码异步发送字符串。
        /// </summary>
        /// <typeparam name="TClient">发送器类型参数，必须实现ISender接口。</typeparam>
        /// <param name="client">发送器实例。</param>
        /// <param name="value">待发送的字符串。</param>
        /// <returns>返回一个Task对象，表示异步操作。</returns>
        public static Task SendAsync<TClient>(this TClient client, string value) where TClient : ISender
        {
            // 将字符串转换为UTF-8编码的字节数组，然后异步发送。
            return client.SendAsync(Encoding.UTF8.GetBytes(value));
        }

        #endregion ISend

        #region IClientSender

        /// <summary>
        /// 异步发送数据。
        /// </summary>
        /// <param name="client">发送数据的客户端对象。</param>
        /// <param name="bytesList">待发送的字节数据列表。</param>
        /// <typeparam name="TClient">客户端对象类型，必须实现<see cref="IClientSender"/>接口。</typeparam>
        public static void Send<TClient>(this TClient client, IList<ArraySegment<byte>> bytesList) where TClient : IClientSender
        {
            // 调用客户端对象的SendAsync方法发送数据，并忽略返回结果。
            client.SendAsync(bytesList).GetFalseAwaitResult();
        }

        #endregion IClientSender

        #region IRequestInfoSender

        /// <summary>
        /// 异步发送请求信息。
        /// </summary>
        /// <param name="client">发起请求的客户端对象。</param>
        /// <param name="requestInfo">要发送的请求信息。</param>
        /// <typeparam name="TClient">客户端对象的类型，必须实现<see cref="IRequestInfoSender"/>接口。</typeparam>
        public static void Send<TClient>(this TClient client, IRequestInfo requestInfo) where TClient : IRequestInfoSender
        {
            // 直接调用客户端对象的SendAsync方法并获取错误的等待结果
            // 这里使用GetFalseAwaitResult()是因为SendAsync方法可能不返回Task对象
            // 这种情况下，GetFalseAwaitResult()可以防止编译器警告，并且不会影响程序的执行
            client.SendAsync(requestInfo).GetFalseAwaitResult();
        }

        #endregion IRequestInfoSender

        #region IIdSender

        /// <summary>
        /// 同步发送请求方法
        /// </summary>
        /// <typeparam name="TClient">泛型参数，表示客户端类型，必须实现IIdRequestInfoSender接口</typeparam>
        /// <param name="client">客户端实例</param>
        /// <param name="id">请求的目标ID</param>
        /// <param name="requestInfo">请求信息对象，包含请求的各种细节</param>
        public static void Send<TClient>(this TClient client, string id, IRequestInfo requestInfo) where TClient : IIdRequestInfoSender
        {
            // 调用异步发送方法而不等待结果，实现同步发送的效果
            client.SendAsync(id, requestInfo).GetFalseAwaitResult();
        }

        /// <summary>
        /// 以UTF-8的编码同步发送字符串。
        /// </summary>
        /// <typeparam name="TClient">泛型参数，表示客户端类型，必须实现IIdSender接口。</typeparam>
        /// <param name="client">客户端实例，用于发送数据。</param>
        /// <param name="id">标识符，用于指定发送的目标。</param>
        /// <param name="value">要发送的字符串内容。</param>
        public static void Send<TClient>(this TClient client, string id, string value) where TClient : IIdSender
        {
            // 使用UTF-8编码将字符串转换为字节数组，并发送。
            client.Send(id, Encoding.UTF8.GetBytes(value));
        }

        /// <summary>
        /// 异步发送数据。
        /// </summary>
        /// <typeparam name="TClient">发送器类型参数，必须实现IIdSender接口。</typeparam>
        /// <param name="client">发送器实例。</param>
        /// <param name="id">发送的数据的唯一标识。</param>
        /// <param name="memory">待发送的字节内存块，使用<see cref="ReadOnlyMemory{T}"/>以强调数据不会被修改。</param>
        public static void Send<TClient>(this TClient client, string id, in ReadOnlyMemory<byte> memory) where TClient : IIdSender
        {
            // 直接调用SendAsync方法并获取结果，这里使用GetFalseAwaitResult是因为发送操作不需要等待完成。
            client.SendAsync(id, memory).GetFalseAwaitResult();
        }

        /// <summary>
        /// 以UTF-8的编码异步发送字符串。
        /// </summary>
        /// <typeparam name="TClient">发送器类型，必须实现IIdSender接口。</typeparam>
        /// <param name="client">发送器实例。</param>
        /// <param name="id">发送的目标标识符。</param>
        /// <param name="value">要发送的字符串内容。</param>
        /// <returns>返回一个Task对象，表示异步操作。</returns>
        public static Task SendAsync<TClient>(this TClient client, string id, string value) where TClient : IIdSender
        {
            // 将字符串转换为UTF-8编码的字节数组，以便发送。
            return client.SendAsync(id, Encoding.UTF8.GetBytes(value));
        }

        #endregion IIdSender

        #region IUdpClientSender

        /// <summary>
        /// 以UTF-8的编码同步发送字符串。
        /// </summary>
        /// <typeparam name="TClient">泛型参数，限定为实现了IUdpClientSender接口的类型。</typeparam>
        /// <param name="client">用于发送数据的客户端实例。</param>
        /// <param name="endPoint">发送数据的目的地，表示为一个端点。</param>
        /// <param name="value">需要发送的字符串数据。</param>
        public static void Send<TClient>(this TClient client, EndPoint endPoint, string value) where TClient : IUdpClientSender
        {
            // 使用UTF-8编码将字符串转换为字节数组，并通过客户端实例发送。
            client.Send(endPoint, Encoding.UTF8.GetBytes(value));
        }

        /// <summary>
        /// 异步发送数据到指定的端点。
        /// </summary>
        /// <typeparam name="TClient">发送客户端的类型，必须实现<see cref="IUdpClientSender"/>接口。</typeparam>
        /// <param name="client">发送客户端实例。</param>
        /// <param name="endPoint">数据发送的目标端点。</param>
        /// <param name="memory">待发送的数据，以只读内存的方式提供。</param>
        public static void Send<TClient>(this TClient client, EndPoint endPoint, ReadOnlyMemory<byte> memory)
            where TClient : IUdpClientSender
        {
            // 调用SendAsync方法并忽略结果，实现同步发送数据的逻辑。
            client.SendAsync(endPoint, memory).GetFalseAwaitResult();
        }

        /// <summary>
        /// 以UTF-8的编码异步发送字符串。
        /// </summary>
        /// <typeparam name="TClient">泛型参数，表示UDP客户端发送器的类型。</typeparam>
        /// <param name="client">UDP客户端实例，用于发送数据。</param>
        /// <param name="endPoint">发送数据的目的地，可以是IP地址和端口号的组合。</param>
        /// <param name="value">需要发送的字符串内容。</param>
        /// <returns>返回一个Task对象，代表异步操作的完成状态。</returns>
        public static Task SendAsync<TClient>(this TClient client, EndPoint endPoint, string value) where TClient : IUdpClientSender
        {
            // 使用UTF-8编码将字符串转换为字节数组，然后调用SendAsync方法发送。
            return client.SendAsync(endPoint, Encoding.UTF8.GetBytes(value));
        }

        #endregion IUdpClientSender

        #region IWaitSender

        /// <summary>
        /// 发送字节流
        /// </summary>
        /// <param name="client">要发送数据的客户端</param>
        /// <param name="msg">要发送的消息</param>
        /// <param name="millisecondsTimeout">等待返回数据的超时时间，默认为5000毫秒</param>
        /// <exception cref="ClientNotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public static byte[] SendThenReturn(this IWaitSender client, string msg, int millisecondsTimeout = 5000)
        {
            // 使用UTF-8编码将字符串消息转换为字节流，并调用重载的SendThenReturn方法
            return SendThenReturn(client, Encoding.UTF8.GetBytes(msg), millisecondsTimeout);
        }

        /// <summary>
        /// 使用指定的超时时间通过IWaitSender发送数据并返回响应。
        /// 此方法通过创建一个带有超时时间的CancellationTokenSource来控制操作的超时。
        /// 如果操作在指定的超时时间内完成，则返回操作的结果，否则抛出一个超时异常。
        /// </summary>
        /// <param name="client">实现IWaitSender接口的实例，用于发送数据并等待响应。</param>
        /// <param name="memory">只读字节内存，表示要发送的数据。</param>
        /// <param name="millisecondsTimeout">操作的超时时间，以毫秒为单位，默认为5000毫秒（5秒）。</param>
        /// <returns>操作成功时返回字节数组，包含响应数据。</returns>
        /// <exception cref="TimeoutException">当操作超时时，会抛出此异常。</exception>
        public static byte[] SendThenReturn(this IWaitSender client, ReadOnlyMemory<byte> memory, int millisecondsTimeout = 5000)
        {
            // 创建一个CancellationTokenSource对象，并设置超时时间
            using (var tokenSource = new CancellationTokenSource(millisecondsTimeout))
            {
                try
                {
                    // 异步发送数据并返回结果，使用取消令牌来控制超时
                    return client.SendThenReturnAsync(memory, tokenSource.Token).GetFalseAwaitResult();
                }
                catch (OperationCanceledException)
                {
                    // 当操作被取消时，抛出一个超时异常
                    ThrowHelper.ThrowTimeoutException();
                    return default;
                }
            }
        }

        /// <summary>
        /// 使用指定的发送者发送数据，然后返回发送的结果。
        /// </summary>
        /// <param name="client">实现<see cref="IWaitSender"/>接口的发送者对象。</param>
        /// <param name="memory">待发送的字节内存块。</param>
        /// <param name="token">用于取消操作的取消令牌。</param>
        /// <returns>发送操作的结果数据。</returns>
        public static byte[] SendThenReturn(this IWaitSender client, ReadOnlyMemory<byte> memory, CancellationToken token)
        {
            // 调用异步版本的SendThenReturn方法，并直接获取其结果
            return client.SendThenReturnAsync(memory, token).GetFalseAwaitResult();
        }

        #endregion IWaitSender

        #region IWaitSenderAsync

        /// <summary>
        /// 异步发送消息并返回结果
        /// </summary>
        /// <param name="client">发送客户端</param>
        /// <param name="msg">待发送的消息</param>
        /// <param name="millisecondsTimeout">超时时间，单位为毫秒，默认为5000毫秒</param>
        /// <exception cref="ClientNotConnectedException">客户端没有连接</exception>
        /// <exception cref="OverlengthException">发送数据超长</exception>
        /// <exception cref="Exception">其他异常</exception>
        /// <returns>返回的数据</returns>
        public static Task<byte[]> SendThenReturnAsync(this IWaitSender client, string msg, int millisecondsTimeout = 5000)
        {
            // 使用UTF-8编码将字符串消息转换为字节数组
            return SendThenReturnAsync(client, Encoding.UTF8.GetBytes(msg), millisecondsTimeout);
        }

        /// <summary>
        /// 异步发送数据并返回响应，如果超时则抛出异常。
        /// </summary>
        /// <param name="client">实现IWaitSender接口的客户端对象，用于发送数据。</param>
        /// <param name="memory">待发送的数据，以<see cref="ReadOnlyMemory{T}"/>的形式指定。</param>
        /// <param name="millisecondsTimeout">操作超时的时间，以毫秒为单位，默认为5000毫秒（5秒）。</param>
        /// <returns>返回发送操作的响应数据，以byte数组形式。</returns>
        /// <exception cref="TimeoutException">当操作超时时，此异常被抛出。</exception>
        public static async Task<byte[]> SendThenReturnAsync(this IWaitSender client, ReadOnlyMemory<byte> memory, int millisecondsTimeout = 5000)
        {
            // 创建一个CancellationTokenSource对象，并设置超时时间
            using (var tokenSource = new CancellationTokenSource(millisecondsTimeout))
            {
                try
                {
                    // 使用client对象的SendThenReturnAsync方法发送数据并返回响应
                    return await client.SendThenReturnAsync(memory, tokenSource.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // 如果操作被取消（即达到超时时间），则抛出TimeoutException异常
                    throw new TimeoutException();
                }
            }
        }

        #endregion IWaitSenderAsync
    }
}