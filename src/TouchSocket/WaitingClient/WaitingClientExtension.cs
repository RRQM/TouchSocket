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
    /// <param name="value">要发送的消息</param>
    /// <param name="cancellationToken">取消令箭</param>
    /// <exception cref="ClientNotConnectedException">客户端没有连接</exception>
    /// <exception cref="OverlengthException">发送数据超长</exception>
    /// <exception cref="Exception">其他异常</exception>
    /// <returns>返回的数据</returns>
    public static async Task<ResponsedData> SendThenResponseAsync<TClient, TResult>(this IWaitingClient<TClient, TResult> client, string value, CancellationToken cancellationToken)
        where TClient : IReceiverClient<TResult>, ISender, IRequestInfoSender
        where TResult : IReceiverResult
    {
        var byteBlock = new ByteBlock(1024);

        try
        {
            WriterExtension.WriteNormalString(ref byteBlock, value, Encoding.UTF8);
            return await client.SendThenResponseAsync(byteBlock.Memory, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        finally
        {
            byteBlock.Dispose();
        }
    }

    /// <summary>
    /// 发送数据并等待
    /// </summary>
    /// <param name="client">等待客户端接口</param>
    /// <param name="value">要发送的消息</param>
    /// <param name="millisecondsTimeout">超时时间，默认为5000毫秒</param>
    /// <exception cref="ClientNotConnectedException">客户端没有连接时抛出的异常</exception>
    /// <exception cref="OverlengthException">发送数据超长时抛出的异常</exception>
    /// <exception cref="Exception">其他异常</exception>
    /// <returns>返回从客户端接收到的数据</returns>
    public static async Task<ResponsedData> SendThenResponseAsync<TClient, TResult>(this IWaitingClient<TClient, TResult> client, string value, int millisecondsTimeout = 5000)
        where TClient : IReceiverClient<TResult>, ISender, IRequestInfoSender
        where TResult : IReceiverResult
    {

        var byteBlock = new ByteBlock(1024);

        try
        {
            WriterExtension.WriteNormalString(ref byteBlock, value, Encoding.UTF8);
            return await client.SendThenResponseAsync(byteBlock.Memory, millisecondsTimeout).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        finally
        {
            byteBlock.Dispose();
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
}