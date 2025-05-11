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

namespace TouchSocket.Sockets;

/// <summary>
/// 定义一个静态扩展类，用于处理等待客户端操作的扩展方法
/// </summary>
public static class WaitingClientExtension
{
    #region CreateWaitingClient

    /// <summary>
    /// 创建可等待的客户端。
    /// </summary>
    /// <typeparam name="TClient">客户端类型，必须同时实现<see cref="IReceiverClient{TResult}"/>和ISender接口。</typeparam>
    /// <typeparam name="TResult">接收结果类型，必须继承自IReceiverResult。</typeparam>
    /// <param name="client">要转换为可等待客户端的实例。</param>
    /// <param name="waitingOptions">等待选项，用于控制等待行为。</param>
    /// <returns>返回一个新的WaitingClient实例，该实例提供对原客户端的封装，并支持等待操作。</returns>
    public static IWaitingClient<TClient, TResult> CreateWaitingClient<TClient, TResult>(this TClient client, WaitingOptions waitingOptions) where TClient : IReceiverClient<TResult>, ISender, IRequestInfoSender
        where TResult : IReceiverResult
    {
        return new InternalWaitingClient<TClient, TResult>(client, waitingOptions);
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
    /// 创建一个等待客户端，用于接收UDP数据。
    /// </summary>
    /// <param name="client">IUdpSession 实例。</param>
    /// <returns>返回一个 IWaitingClient 实例，可用于接收 UDP 消息。</returns>
    public static IWaitingClient<IUdpSession, IUdpReceiverResult> CreateWaitingClient(this IUdpSession client)
    {
        // 使用默认的等待选项创建等待客户端
        return client.CreateWaitingClient<IUdpSession, IUdpReceiverResult>(new WaitingOptions());
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
    /// 为指定的 TCP 客户端创建一个等待客户端实例。
    /// </summary>
    /// <param name="client">要为其创建等待客户端的 ITcpClient 实例。</param>
    /// <returns>返回创建的<see cref="IWaitingClient{TClient, TResult}"/>实例。</returns>
    public static IWaitingClient<ITcpClient, IReceiverResult> CreateWaitingClient(this ITcpClient client)
    {
        // 使用默认的等待选项实例化等待客户端
        return client.CreateWaitingClient<ITcpClient, IReceiverResult>(new WaitingOptions());
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

    /// <summary>
    /// 为指定的 ITcpSessionClient 实例创建一个等待客户端。
    /// </summary>
    /// <param name="client">要创建等待客户端的 ITcpSessionClient 实例。</param>
    /// <returns>返回一个IWaitingClient实例。</returns>
    public static IWaitingClient<ITcpSessionClient, IReceiverResult> CreateWaitingClient(this ITcpSessionClient client)
    {
        // 使用默认的等待选项创建等待客户端
        return client.CreateWaitingClient<ITcpSessionClient, IReceiverResult>(new WaitingOptions());
    }

    #endregion CreateWaitingClient

    #region SendThenResponseAsync

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
    public static async Task<ResponsedData> SendThenResponseAsync<TClient, TResult>(this IWaitingClient<TClient, TResult> client, string msg, CancellationToken token)
        where TClient : IReceiverClient<TResult>, ISender, IRequestInfoSender
        where TResult : IReceiverResult
    {
        using (var byteBlock = new ByteBlock(1024))
        {
            byteBlock.WriteNormalString(msg, Encoding.UTF8);
            return await client.SendThenResponseAsync(byteBlock.Memory, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
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
    public static async Task<ResponsedData> SendThenResponseAsync<TClient, TResult>(this IWaitingClient<TClient, TResult> client, string msg, int millisecondsTimeout = 5000)
        where TClient : IReceiverClient<TResult>, ISender, IRequestInfoSender
        where TResult : IReceiverResult
    {

        using (var byteBlock = new ByteBlock(1024))
        {
            byteBlock.WriteNormalString(msg, Encoding.UTF8);
            return await client.SendThenResponseAsync(byteBlock.Memory, millisecondsTimeout).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }

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
        where TClient : IReceiverClient<TResult>, ISender, IRequestInfoSender
        where TResult : IReceiverResult
    {
        // 使用CancellationTokenSource来控制等待时间
        using (var tokenSource = new CancellationTokenSource(millisecondsTimeout))
        {
            // 尝试发送数据并等待响应
            try
            {
                return await client.SendThenResponseAsync(memory, tokenSource.Token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            // 捕获操作取消异常，并将其转换为超时异常
            catch (OperationCanceledException)
            {
                throw new TimeoutException();
            }
        }
    }

    /// <summary>
    /// 异步发送请求并等待响应的扩展方法。
    /// </summary>
    /// <typeparam name="TClient">客户端类型，必须同时实现<see cref="IReceiverClient{TResult}"/>、<see cref="ISender"/>和<see cref="IRequestInfoSender"/>接口。</typeparam>
    /// <typeparam name="TResult">接收结果类型，必须实现IReceiverResult接口。</typeparam>
    /// <param name="client">等待客户端，实现了<see cref="IWaitingClient{TClient, TResult}"/>接口。</param>
    /// <param name="requestInfo">请求信息，用于发送。</param>
    /// <param name="millisecondsTimeout">操作的超时时间（以毫秒为单位），默认为5000毫秒。</param>
    /// <returns>返回一个异步任务，结果为响应数据。</returns>
    /// <exception cref="TimeoutException">当操作超时时抛出。</exception>
    public static async Task<ResponsedData> SendThenResponseAsync<TClient, TResult>(this IWaitingClient<TClient, TResult> client, IRequestInfo requestInfo, int millisecondsTimeout = 5000)
        where TClient : IReceiverClient<TResult>, ISender, IRequestInfoSender
        where TResult : IReceiverResult
    {
        // 使用CancellationTokenSource来控制等待时间
        using (var tokenSource = new CancellationTokenSource(millisecondsTimeout))
        {
            // 尝试发送数据并等待响应
            try
            {
                return await client.SendThenResponseAsync(requestInfo, tokenSource.Token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            // 捕获操作取消异常，并将其转换为超时异常
            catch (OperationCanceledException)
            {
                throw new TimeoutException();
            }
        }
    }
    #endregion SendThenResponseAsync

    #region SendThenReturnAsync

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
    public static Task<byte[]> SendThenReturnAsync<TClient, TResult>(this IWaitingClient<TClient, TResult> client, string msg, int millisecondsTimeout = 5000)
        where TClient : IReceiverClient<TResult>, ISender, IRequestInfoSender
        where TResult : IReceiverResult
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
    public static async Task<byte[]> SendThenReturnAsync<TClient, TResult>(this IWaitingClient<TClient, TResult> client, ReadOnlyMemory<byte> memory, int millisecondsTimeout = 5000)
        where TClient : IReceiverClient<TResult>, ISender, IRequestInfoSender
        where TResult : IReceiverResult
    {
        // 创建一个CancellationTokenSource对象，并设置超时时间
        using (var tokenSource = new CancellationTokenSource(millisecondsTimeout))
        {
            try
            {
                // 使用client对象的SendThenReturnAsync方法发送数据并返回响应
                return await client.SendThenReturnAsync(memory, tokenSource.Token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            catch (OperationCanceledException)
            {
                // 如果操作被取消（即达到超时时间），则抛出TimeoutException异常
                throw new TimeoutException();
            }
        }
    }

    /// <summary>
    /// 异步发送请求并返回响应数据
    /// </summary>
    /// <typeparam name="TClient">客户端类型</typeparam>
    /// <typeparam name="TResult">接收结果类型，必须实现IReceiverResult接口</typeparam>
    /// <param name="client">等待客户端实例，提供发送和接收功能</param>
    /// <param name="requestInfo">请求信息，包含要发送的数据</param>
    /// <param name="millisecondsTimeout">操作超时时间（以毫秒为单位），默认为5000毫秒</param>
    /// <returns>返回一个任务，该任务结果是一个字节数组，包含响应数据</returns>
    /// <exception cref="TimeoutException">当操作超时时，抛出此异常</exception>
    public static async Task<byte[]> SendThenReturnAsync<TClient, TResult>(this IWaitingClient<TClient, TResult> client, IRequestInfo requestInfo, int millisecondsTimeout = 5000)
       where TClient : IReceiverClient<TResult>, ISender, IRequestInfoSender
       where TResult : IReceiverResult
    {
        // 创建一个CancellationTokenSource对象，并设置超时时间
        using (var tokenSource = new CancellationTokenSource(millisecondsTimeout))
        {
            try
            {
                // 使用client对象的SendThenReturnAsync方法发送数据并返回响应
                return (await client.SendThenResponseAsync(requestInfo, tokenSource.Token).ConfigureAwait(EasyTask.ContinueOnCapturedContext)).ByteBlock?.ToArray();
            }
            catch (OperationCanceledException)
            {
                // 如果操作被取消（即达到超时时间），则抛出TimeoutException异常
                throw new TimeoutException();
            }
        }
    }

    /// <summary>
    /// 在给定的客户端上异步发送数据并返回响应数据。
    /// </summary>
    /// <typeparam name="TClient">发送和请求信息的客户端类型。</typeparam>
    /// <typeparam name="TResult">接收结果的类型。</typeparam>
    /// <param name="client">一个等待客户端对象。</param>
    /// <param name="memory">要发送的数据。</param>
    /// <param name="token">用于取消操作的取消令牌。</param>
    /// <returns>一个异步任务，该任务的结果是返回的响应数据。</returns>
    public static async Task<byte[]> SendThenReturnAsync<TClient, TResult>(this IWaitingClient<TClient, TResult> client, ReadOnlyMemory<byte> memory, CancellationToken token)
        where TClient : IReceiverClient<TResult>, ISender, IRequestInfoSender
        where TResult : IReceiverResult
    {
        // 使用client对象的SendThenResponseAsync方法发送数据并返回响应
        return (await client.SendThenResponseAsync(memory, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext)).ByteBlock?.ToArray();
    }

    #endregion SendThenReturnAsync

    #region SendThenResponse

    /// <summary>
    /// 向服务器发送数据并等待接收响应。
    /// </summary>
    /// <typeparam name="TClient">客户端类型，必须同时实现<see cref="IReceiverClient{TResult}"/>和ISender接口。</typeparam>
    /// <typeparam name="TResult">接收结果类型，必须实现IReceiverResult接口。</typeparam>
    /// <param name="client">实现等待客户端接口<see cref="IWaitingClient{TClient, TResult}"/>的实例。</param>
    /// <param name="memory">要发送的数据，以只读内存形式提供。</param>
    /// <param name="token">用于取消操作的取消令牌。</param>
    /// <returns>包含操作结果的ResponsedData对象。</returns>
    [AsyncToSyncWarning]
    public static ResponsedData SendThenResponse<TClient, TResult>(this IWaitingClient<TClient, TResult> client, ReadOnlyMemory<byte> memory, CancellationToken token)
       where TClient : IReceiverClient<TResult>, ISender, IRequestInfoSender
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
    [AsyncToSyncWarning]
    public static ResponsedData SendThenResponse<TClient, TResult>(this IWaitingClient<TClient, TResult> client, string msg, CancellationToken token)
        where TClient : IReceiverClient<TResult>, ISender, IRequestInfoSender
        where TResult : IReceiverResult
    {
        // 将字符串消息转换为字节数组
        return client.SendThenResponseAsync(msg, token).GetFalseAwaitResult();
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
    [AsyncToSyncWarning]
    public static ResponsedData SendThenResponse<TClient, TResult>(this IWaitingClient<TClient, TResult> client, string msg, int millisecondsTimeout = 5000)
        where TClient : IReceiverClient<TResult>, ISender, IRequestInfoSender
        where TResult : IReceiverResult
    {
        // 将字符串消息转换为字节数组，以便发送
        return client.SendThenResponseAsync(msg, millisecondsTimeout).GetFalseAwaitResult();
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
    [AsyncToSyncWarning]
    public static ResponsedData SendThenResponse<TClient, TResult>(this IWaitingClient<TClient, TResult> client, ReadOnlyMemory<byte> memory, int millisecondsTimeout = 5000)
        where TClient : IReceiverClient<TResult>, ISender, IRequestInfoSender
        where TResult : IReceiverResult
    {
        // 使用CancellationTokenSource来控制等待时间
        using (var tokenSource = new CancellationTokenSource(millisecondsTimeout))
        {
            try
            {
                // 实际发送数据并等待响应
                return client.SendThenResponseAsync(memory, tokenSource.Token).GetFalseAwaitResult();
            }
            catch (OperationCanceledException)
            {
                // 如果操作因超时而被取消，则转换为TimeoutException异常抛出
                throw new TimeoutException();
            }
        }
    }

    #endregion SendThenResponse

    #region SendThenReturn

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
    [AsyncToSyncWarning]
    public static byte[] SendThenReturn<TClient, TResult>(this IWaitingClient<TClient, TResult> client, string msg, int millisecondsTimeout = 5000)
        where TClient : IReceiverClient<TResult>, ISender, IRequestInfoSender
        where TResult : IReceiverResult
    {
        // 使用UTF-8编码将字符串消息转换为字节流，并调用重载的SendThenReturn方法
        return SendThenReturnAsync(client, Encoding.UTF8.GetBytes(msg), millisecondsTimeout).GetFalseAwaitResult();
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
    [AsyncToSyncWarning]
    public static byte[] SendThenReturn<TClient, TResult>(this IWaitingClient<TClient, TResult> client, ReadOnlyMemory<byte> memory, int millisecondsTimeout = 5000)
        where TClient : IReceiverClient<TResult>, ISender, IRequestInfoSender
        where TResult : IReceiverResult
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
    /// 向接收方发送数据后返回发送结果。
    /// 此方法适用于需要同步发送数据并立即获取结果的场景。
    /// </summary>
    /// <param name="client">提供发送和接收功能的客户端对象。</param>
    /// <param name="memory">要发送的数据，以<see cref="ReadOnlyMemory{T}"/>形式表示。</param>
    /// <param name="token">用于取消操作的取消令牌。</param>
    /// <typeparam name="TClient">客户端类型，必须实现<see cref="IReceiverClient{TResult}"/>、<see cref="ISender"/>和<see cref="IRequestInfoSender"/>接口。</typeparam>
    /// <typeparam name="TResult">接收结果类型，必须实现<see cref="IReceiverResult"/>接口。</typeparam>
    /// <returns>返回发送后的结果数据，类型为<see cref="byte"/>数组。</returns>
    [AsyncToSyncWarning]
    public static byte[] SendThenReturn<TClient, TResult>(this IWaitingClient<TClient, TResult> client, ReadOnlyMemory<byte> memory, CancellationToken token)
        where TClient : IReceiverClient<TResult>, ISender, IRequestInfoSender
        where TResult : IReceiverResult
    {
        // 调用异步版本的SendThenReturn方法，并直接获取其结果
        return client.SendThenReturnAsync(memory, token).GetFalseAwaitResult();
    }

    #endregion SendThenReturn
}