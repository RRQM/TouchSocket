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

using TouchSocket.Resources;

namespace TouchSocket.Sockets;

/// <summary>
/// 客户端扩展类
/// </summary>
public static class ClientExtension
{
    /// <summary>
    /// 获取会话信息
    /// </summary>
    /// <typeparam name="TClient">会话类型，必须实现ITcpSession和IIdClient接口</typeparam>
    /// <param name="client">会话实例</param>
    /// <returns>会话信息字符串，包括IP、端口、ID和协议类型</returns>
    public static string GetInfo<TClient>(this TClient client) where TClient : ITcpSession, IIdClient
    {
        return $"IP&Port={client.IP}:{client.Port},Id={client.Id},Protocol={client.Protocol}";
    }

    /// <summary>
    /// 获取客户端的IP和端口号
    /// </summary>
    /// <typeparam name="TClient">泛型参数，表示客户端会话类型，必须实现ITcpSession接口</typeparam>
    /// <param name="client">具体客户端会话实例</param>
    /// <returns>返回客户端的IP地址和端口号，格式为IP:端口号</returns>
    public static string GetIPPort<TClient>(this TClient client) where TClient : ITcpSession
    {
        return $"{client.IP}:{client.Port}";
    }

    /// <summary>
    /// 获取最后活动时间。即<see cref="IClient.LastReceivedTime"/>与<see cref="IClient.LastSentTime"/>的最近值。
    /// </summary>
    /// <typeparam name="TClient">泛型参数，表示客户端类型，必须实现<see cref="IClient"/>接口。</typeparam>
    /// <param name="client">泛型参数实例，表示具体的客户端对象。</param>
    /// <returns>返回最后活动时间，即<see cref="IClient.LastReceivedTime"/>与<see cref="IClient.LastSentTime"/>中较近的时间。</returns>
    public static DateTimeOffset GetLastActiveTime<TClient>(this TClient client) where TClient : IClient
    {
        // 比较最后一次发送时间和最后一次接收时间，返回较近的时间作为最后活动时间。
        return client.LastSentTime > client.LastReceivedTime ? client.LastSentTime : client.LastReceivedTime;
    }

    /// <summary>
    /// 获取服务器中，除自身以外的所有客户端id
    /// </summary>
    /// <typeparam name="TClient">客户端类型，要求实现ITcpListenableClient和IIdClient接口</typeparam>
    /// <param name="client">当前客户端实例</param>
    /// <returns>返回一个<see cref="IEnumerable{T}"/>，包含除当前客户端外的所有客户端id</returns>
    public static IEnumerable<string> GetOtherIds<TClient>(this TClient client) where TClient : ITcpListenableClient, IIdClient
    {
        // 使用LINQ查询方法，从服务器获取所有客户端id，并筛选出不等于当前客户端id的其他客户端id
        return client.Service.GetIds().Where(id => id != client.Id);
    }

    #region CloseAsync

    /// <summary>
    /// 异步关闭指定的客户端连接。
    /// 该方法通过调用IClosableClient接口的CloseAsync方法来实现关闭操作，传入一个空字符串作为参数。
    /// </summary>
    /// <typeparam name="TClient">客户端类型，必须实现IClosableClient接口。</typeparam>
    /// <param name="client">要关闭的客户端实例。</param>
    /// <returns>一个Task对象，表示异步操作的结果。</returns>
    public static async Task<Result> CloseAsync<TClient>(this TClient client) where TClient : IClosableClient
    {
        if (client is null)
        {
            return Result.FromFail(TouchSocketCoreResource.ArgumentIsNull.Format(nameof(client)));
        }
        return await client.CloseAsync(string.Empty).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 安全性关闭。不会抛出异常。
    /// </summary>
    /// <typeparam name="TClient">要关闭的客户端类型，必须实现IClosableClient接口。</typeparam>
    /// <param name="client">要关闭的客户端实例。</param>
    /// <param name="msg">关闭时传递的消息。</param>
    [Obsolete("此操作已被弃用，请直接使用CloseAsync代替", true)]
    public static async Task SafeCloseAsync<TClient>(this TClient client, string msg) where TClient : IClosableClient
    {
        // 尝试关闭客户端，如果客户端为<see langword="null"/>，则不执行任何操作。
        try
        {
            if (client == null)
            {
                return;
            }
            else
            {
                // 异步调用CloseAsync方法关闭客户端，传递关闭消息，并指定ConfigureAwait为<see langword="false"/>以避免同步上下文。
                await client.CloseAsync(msg).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
        }
        catch
        {
            // 捕获到任何异常都不进行处理，确保SafeCloseAsync方法不会抛出任何异常。
        }
    }

    /// <summary>
    /// 安全性关闭。不会抛出异常。
    /// </summary>
    /// <typeparam name="TClient">一个泛型参数，代表客户端类型，该类型必须实现IClosableClient接口。</typeparam>
    /// <param name="client">要关闭的客户端实例。</param>
    /// <remarks>
    /// 此方法提供了一种安全关闭客户端的方式，确保在关闭过程中不会因为异常而中断。
    /// 它是异步的，允许在不阻塞当前线程的情况下完成关闭操作。
    /// </remarks>
    [Obsolete("此操作已被弃用，请直接使用CloseAsync代替", true)]
    public static Task SafeCloseAsync<TClient>(this TClient client) where TClient : IClosableClient
    {
        // 调用带有自定义名称参数的SafeCloseAsync方法，这里使用方法名作为操作标识。
        return SafeCloseAsync(client, nameof(SafeCloseAsync));
    }

    #endregion CloseAsync

    #region ConnectAsync

    /// <inheritdoc cref="IConnectableClient.ConnectAsync(CancellationToken)"/>
    public static async Task ConnectAsync(this IConnectableClient client, int millisecondsTimeout = 5000)
    {
        using (var cancellationTokenSource = new CancellationTokenSource(millisecondsTimeout))
        {
            try
            {
                await client.ConnectAsync(cancellationTokenSource.Token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
            catch (OperationCanceledException)
            {
                ThrowHelper.ThrowTimeoutException();
            }
        }

    }

    /// <inheritdoc cref="IConnectableClient.ConnectAsync(CancellationToken)"/>
    public static async Task ConnectAsync<TClient>(this TClient client, IPHost ipHost, int millisecondsTimeout = 5000) where TClient : ISetupConfigObject, ITcpConnectableClient
    {
        TouchSocketConfig config;
        if (client.Config == null)
        {
            config = new TouchSocketConfig();
            config.SetRemoteIPHost(ipHost);
            await client.SetupAsync(config).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        else
        {
            config = client.Config;
            config.SetRemoteIPHost(ipHost);
        }
        await client.ConnectAsync(millisecondsTimeout).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 尝试连接。不会抛出异常。
    /// </summary>
    /// <param name="client">要连接的客户端对象，实现了IConnectableClient接口。</param>
    /// <param name="millisecondsTimeout">连接超时时间，单位为毫秒。默认值为5000毫秒。</param>
    /// <returns>返回一个Result对象，其中包含连接操作的结果代码以及可能的异常消息。</returns>
    public static async Task<Result> TryConnectAsync(this IConnectableClient client, int millisecondsTimeout = 5000)
    {
        // 尝试进行连接操作
        try
        {
            await client.ConnectAsync(millisecondsTimeout).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return new Result(ResultCode.Success);
        }
        // 捕获连接过程中可能抛出的任何异常
        catch (Exception ex)
        {
            return new Result(ResultCode.Exception, ex.Message);
        }
    }

    /// <summary>
    /// 尝试连接。不会抛出异常。
    /// </summary>
    /// <typeparam name="TClient">客户端类型，必须实现ISetupConfigObject和IConnectableClient接口。</typeparam>
    /// <param name="client">要连接的客户端对象。</param>
    /// <param name="millisecondsTimeout">连接超时时间，以毫秒为单位。默认为5000毫秒（5秒）。</param>
    /// <returns>返回一个Result对象，其中包含连接操作的结果代码和可能的异常消息。</returns>
    public static async Task<Result> TryConnectAsync<TClient>(this TClient client, int millisecondsTimeout = 5000) where TClient : ISetupConfigObject, IConnectableClient
    {
        try
        {
            // 尝试连接操作，如果超时或发生其他异常，将捕获异常并返回相应的结果。
            await client.ConnectAsync(millisecondsTimeout).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return new Result(ResultCode.Success);
        }
        catch (Exception ex)
        {
            // 当连接过程中抛出异常时，返回包含异常信息的结果对象。
            return new Result(ResultCode.Exception, ex.Message);
        }
    }

    #endregion ConnectAsync

    #region PauseReconnection

    /// <summary>
    /// 指示是否暂停重新连接。
    /// </summary>
    [GeneratorProperty(TargetType = typeof(IDependencyClient))]
    public static readonly DependencyProperty<bool> PauseReconnectionProperty = new DependencyProperty<bool>("PauseReconnection", false);
    #endregion
}