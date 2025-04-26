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
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
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

    /// <summary>
    /// 安全性发送关闭报文
    /// </summary>
    /// <typeparam name="TClient">客户端类型，必须是ITcpSession接口的实现</typeparam>
    /// <param name="client">要关闭的客户端对象</param>
    /// <param name="how">关闭的方式，默认值为SocketShutdown.Both，即读写都关闭</param>
    /// <returns>操作是否成功</returns>
    [Obsolete("此操作已被弃用，请使用ShutdownAsync代替", true)]
    public static bool TryShutdown<TClient>(this TClient client, SocketShutdown how = SocketShutdown.Both) where TClient : class, ITcpSession
    {
        // 检查客户端对象是否为null或默认值
        if (client == default)
        {
            return false;
        }
        try
        {
            // 获取客户端的主要Socket对象
            var socket = client.MainSocket;
            // 检查Socket对象是否为空
            if (socket == null)
            {
                return false;
            }
            // 检查Socket连接是否已经建立
            if (!socket.Connected)
            {
                return false;
            }
            // 发送关闭报文
            socket.Shutdown(how);
            return true;
        }
        catch
        {
            return false;
        }
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
        // 尝试关闭客户端，如果客户端为null，则不执行任何操作。
        try
        {
            if (client == null)
            {
                return;
            }
            else
            {
                // 异步调用CloseAsync方法关闭客户端，传递关闭消息，并指定ConfigureAwait为false以避免同步上下文。
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

    #region Close

    /// <summary>
    /// 同步关闭客户端
    /// </summary>
    /// <remarks>
    /// 请注意，该同步方法由<see cref="IClosableClient.CloseAsync(string, System.Threading.CancellationToken)"/>异步转同步而来。所以请谨慎使用。建议直接使用异步。
    /// </remarks>
    /// <typeparam name="TClient">要操作的客户端类型，必须实现<see cref="IClosableClient"/>接口</typeparam>
    /// <param name="client">要关闭的客户端实例</param>
    public static Result Close<TClient>(this TClient client) where TClient : IClosableClient
    {
        // 调用CloseAsync方法并立即返回，不等待异步操作完成。这样做是为了提供一个同步的关闭方式，
        // 但这种做法可能导致调用线程在客户端实际关闭之前就继续执行了，这可能不是期望的行为。
        // 这里选择不使用await是为了明确表示这个操作是故意设计为同步的，尽管这可能不是一个最佳实践。
        return client.CloseAsync(string.Empty).GetFalseAwaitResult();
    }

    /// <summary>
    /// 同步关闭客户端
    /// </summary>
    /// <remarks>
    /// 请注意，该同步方法由<see cref="IClosableClient.CloseAsync(string, System.Threading.CancellationToken)"/>异步转同步而来。所以请谨慎使用。建议直接使用异步。
    /// </remarks>
    /// <typeparam name="TClient">要操作的客户端类型，必须实现<see cref="IClosableClient"/>接口。</typeparam>
    /// <param name="client">要关闭的客户端实例。</param>
    /// <param name="msg">关闭客户端时发送的消息。</param>
    public static Result Close<TClient>(this TClient client, string msg) where TClient : IClosableClient
    {
        // 使用GetFalseAwaitResult方法使异步操作同步执行，这里无需额外注释。
        return client.CloseAsync(msg).GetFalseAwaitResult();
    }

    /// <summary>
    /// 安全性关闭。不会抛出异常。
    /// </summary>
    /// <remarks>
    /// 请注意，该同步方法由<see cref="IClosableClient.CloseAsync(string, System.Threading.CancellationToken)"/>异步转同步而来。所以请谨慎使用。建议直接使用异步。
    /// </remarks>
    /// <typeparam name="TClient">要操作的客户端类型，必须实现<see cref="IClosableClient"/>接口。</typeparam>
    /// <param name="client">要关闭的客户端实例。</param>
    /// <param name="msg">关闭时传递的消息。</param>
    [Obsolete("此操作已被弃用，请直接使用Close代替", true)]
    public static void SafeClose<TClient>(this TClient client, string msg) where TClient : IClosableClient
    {
        // 尝试关闭客户端，如果客户端为null，则不执行任何操作。
        try
        {
            if (client == null)
            {
                return;
            }
            else
            {
                // 使用GetFalseAwaitResult()强制等待异步关闭完成，而不处理异常。
                client.CloseAsync(msg).GetFalseAwaitResult();
            }
        }
        catch
        {
            // 吞掉所有异常，确保安全关闭不会因异常而中断。
        }
    }

    /// <summary>
    /// 安全性关闭。不会抛出异常。
    /// </summary>
    /// <remarks>
    /// 请注意，该同步方法由<see cref="IClosableClient.CloseAsync(string, System.Threading.CancellationToken)"/>异步转同步而来。所以请谨慎使用。建议直接使用异步。
    /// </remarks>
    /// <typeparam name="TClient">要关闭的客户端类型，必须实现<see cref="IClosableClient"/>接口。</typeparam>
    /// <param name="client">要进行安全关闭的客户端实例。</param>
    [Obsolete("此操作已被弃用，请直接使用Close代替", true)]
    public static void SafeClose<TClient>(this TClient client) where TClient : IClosableClient
    {
        // 调用重载的SafeClose方法，传递客户端实例和该方法的名称。
        SafeClose(client, nameof(SafeClose));
    }

    #endregion Close

    #region ConnectAsync

    /// <inheritdoc cref="IConnectableClient.ConnectAsync(int, CancellationToken)"/>
    public static async Task ConnectAsync(this IConnectableClient client, int millisecondsTimeout = 5000)
    {
        await client.ConnectAsync(millisecondsTimeout, CancellationToken.None).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <inheritdoc cref="IConnectableClient.ConnectAsync(int, System.Threading.CancellationToken)"/>
    public static async Task ConnectAsync<TClient>(this TClient client, IPHost ipHost, int millisecondsTimeout = 5000) where TClient : ISetupConfigObject, IConnectableClient
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

    #region Connect

    /// <summary>
    /// 同步执行连接操作。
    /// </summary>
    /// <remarks>
    /// 注意，本同步操作是直接等待的<see cref="IConnectableClient.ConnectAsync(int, CancellationToken)"/>，所以请谨慎使用。
    /// </remarks>
    /// <param name="client">要连接的客户端对象。</param>
    /// <param name="millisecondsTimeout">连接超时时间，以毫秒为单位。默认为5000毫秒。</param>
    /// <param name="cancellationToken">用于取消操作的令牌。</param>
    public static void Connect(this IConnectableClient client, int millisecondsTimeout = 5000, CancellationToken cancellationToken = default)
    {
        // 使用GetFalseAwaitResult()方法直接等待异步操作完成，以实现同步执行。
        // 这种方法适用于不需要处理异步操作结果的场景。
        client.ConnectAsync(millisecondsTimeout, cancellationToken).GetFalseAwaitResult();
    }

    /// <summary>
    /// 同步执行连接操作。
    /// </summary>
    /// <remarks>
    /// 注意，本同步操作是直接等待的<see cref="IConnectableClient.ConnectAsync(int, CancellationToken)"/>，所以请谨慎使用。
    /// </remarks>
    /// <typeparam name="TClient">要连接的客户端类型，必须实现<see cref="ISetupConfigObject"/>和<see cref="IConnectableClient"/>接口。</typeparam>
    /// <param name="client">要进行连接的客户端实例。</param>
    /// <param name="ipHost">连接的目标IP地址和端口信息。</param>
    /// <param name="millisecondsTimeout">连接超时时间，单位为毫秒，默认为5000毫秒。</param>
    public static void Connect<TClient>(this TClient client, IPHost ipHost, int millisecondsTimeout = 5000) where TClient : ISetupConfigObject, IConnectableClient
    {
        ConnectAsync(client, ipHost, millisecondsTimeout).GetFalseAwaitResult();
    }

    /// <summary>
    /// 同步执行连接操作。不会抛出异常。
    /// </summary>
    /// <remarks>
    /// 注意，本同步操作是直接等待的<see cref="IConnectableClient.ConnectAsync(int, CancellationToken)"/>，所以请谨慎使用。
    /// </remarks>
    /// <param name="client">要执行连接操作的客户端对象。</param>
    /// <param name="millisecondsTimeout">连接超时时间，以毫秒为单位。默认值为5000毫秒。</param>
    /// <returns>返回一个<see cref="Result"/>对象，其中包含连接操作的结果代码以及可能的异常消息。</returns>
    public static Result TryConnect(this IConnectableClient client, int millisecondsTimeout = 5000)
    {
        try
        {
            // 使用异步方法进行连接，并直接等待结果。这里使用了GetFalseAwaitResult方法来避免捕获到异常。
            client.ConnectAsync(millisecondsTimeout).GetFalseAwaitResult();
            // 连接成功，返回一个新的Result对象，表示成功。
            return new Result(ResultCode.Success);
        }
        catch (Exception ex)
        {
            // 捕获到异常，返回一个新的Result对象，包含异常信息。
            return new Result(ResultCode.Exception, ex.Message);
        }
    }

    /// <summary>
    /// 同步执行连接操作。不会抛出异常。
    /// </summary>
    /// <remarks>
    /// 注意，本同步操作是直接等待的<see cref="IConnectableClient.ConnectAsync(int, CancellationToken)"/>，所以请谨慎使用。
    /// </remarks>
    /// <typeparam name="TClient">要连接的客户端类型，必须实现<see cref="ISetupConfigObject"/>和<see cref="IConnectableClient"/>.</typeparam>
    /// <param name="client">要执行连接操作的客户端实例。</param>
    /// <param name="millisecondsTimeout">连接超时时间，以毫秒为单位。默认值为5000毫秒（5秒）。</param>
    /// <returns>一个<see cref="Result"/>实例，包含连接操作的结果代码和可能的异常消息。</returns>
    public static Result TryConnect<TClient>(this TClient client, int millisecondsTimeout = 5000) where TClient : ISetupConfigObject, IConnectableClient
    {
        try
        {
            // 使用异步方法进行连接，但以同步方式等待结果。这里使用自定义扩展方法<see cref="GetFalseAwaitResult()"/>来模拟同步执行。
            client.ConnectAsync(millisecondsTimeout).GetFalseAwaitResult();
            // 连接成功，返回成功结果。
            return new Result(ResultCode.Success);
        }
        catch (Exception ex)
        {
            // 捕获到异常，返回包含异常信息的结果。
            return new Result(ResultCode.Exception, ex.Message);
        }
    }

    #endregion Connect
}