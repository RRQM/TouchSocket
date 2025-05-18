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

using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets;

/// <summary>
/// 提供针对服务的一系列扩展方法。
/// </summary>
public static class ServiceExtension
{
    #region IServiceBase

    /// <summary>
    /// 启动服务的扩展方法。
    /// </summary>
    /// <typeparam name="TService">服务的类型，必须继承自IServiceBase。</typeparam>
    /// <param name="service">要启动的服务实例。</param>
    /// <remarks>
    /// 此方法使用了GetFalseAwaitResult方法来避免捕获异常，这在某些异步操作中可能是必要的，
    /// 但在常规情况下，应该避免在生产代码中直接调用非await的异步方法结果。
    /// </remarks>
    [AsyncToSyncWarning]
    public static void Start<TService>(this TService service) where TService : IServiceBase
    {
        service.StartAsync().GetFalseAwaitResult();
    }

    /// <summary>
    /// 停止给定的服务。
    /// </summary>
    /// <typeparam name="TService">要停止的服务类型，必须实现IServiceBase接口。</typeparam>
    /// <param name="service">要执行停止操作的服务实例。</param>
    [AsyncToSyncWarning]
    public static Result Stop<TService>(this TService service) where TService : IServiceBase
    {
        // 直接调用服务的StopAsync方法，并获取其FalseAwaitResult，确保异步操作被立即处理。
        return service.StopAsync().GetFalseAwaitResult();
    }

    #endregion IServiceBase

    #region ITcpService

    /// <inheritdoc cref="IServiceBase.StartAsync"/>
    [AsyncToSyncWarning]
    public static void Start<TService>(this TService service, params IPHost[] iPHosts) where TService : ITcpServiceBase
    {
        StartAsync(service, iPHosts).GetFalseAwaitResult();
    }

    /// <inheritdoc cref="IServiceBase.StartAsync"/>
    public static async Task StartAsync<TService>(this TService service, params IPHost[] iPHosts) where TService : ITcpServiceBase
    {
        TouchSocketConfig config;
        if (service.Config == null)
        {
            config = new TouchSocketConfig();
            config.SetListenIPHosts(iPHosts);
            await service.SetupAsync(config).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        else
        {
            config = service.Config;
            config.SetListenIPHosts(iPHosts);
        }
        await service.StartAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    #endregion ITcpService

    #region Udp

    /// <inheritdoc cref="IServiceBase.StartAsync"/>
    [AsyncToSyncWarning]
    public static void Start<TService>(this TService service, IPHost iPHost) where TService : IUdpSession
    {
        StartAsync(service, iPHost).GetFalseAwaitResult();
    }

    /// <inheritdoc cref="IServiceBase.StartAsync"/>
    public static async Task StartAsync<TService>(this TService service, IPHost iPHost) where TService : IUdpSession
    {
        TouchSocketConfig config;
        if (service.Config == null)
        {
            config = new TouchSocketConfig();
            config.SetBindIPHost(iPHost);
            await service.SetupAsync(config).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        else
        {
            config = service.Config;
            config.SetBindIPHost(iPHost);
        }
        await service.StartAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    #endregion Udp

    #region IConnectableService<TClient>

    /// <summary>
    /// 从连接服务中获取指定ID的客户端。
    /// </summary>
    /// <param name="connectableService">一个可连接的服务实例，提供了访问其客户端集合的方法。</param>
    /// <param name="id">要查找的客户端的唯一标识符。</param>
    /// <typeparam name="TClient">客户端的类型，必须同时实现<see cref="IIdClient"/>和<see cref="IClient"/>接口。</typeparam>
    /// <returns>如果找到指定ID的客户端，则返回该客户端；否则，抛出<see cref="ClientNotFindException"/>异常。</returns>
    /// <exception cref="ClientNotFindException">当无法找到指定ID的客户端时抛出。</exception>
    public static TClient GetClient<TClient>(this IConnectableService<TClient> connectableService, string id)
        where TClient : IIdClient, IClient
    {
        // 尝试从客户端集合中获取指定ID的客户端。
        // 如果获取成功，则返回该客户端；如果获取失败，则抛出异常。
        return connectableService.Clients.TryGetClient(id, out var client) ? client : throw new ClientNotFindException();
    }

    /// <summary>
    /// 尝试从可连接服务中获取与指定ID匹配的客户端。
    /// </summary>
    /// <param name="connectableService">一个实现了<see cref="IConnectableService{TClient}"/>接口的可连接服务对象。</param>
    /// <param name="id">要获取的客户端的唯一标识符。</param>
    /// <param name="client">如果找到匹配的客户端，则设置此参数为该客户端对象；如果未找到，则设置为default(TClient)。</param>
    /// <returns>如果找到匹配的客户端，则返回<see langword="true"/>；否则返回<see langword="false"/>。</returns>
    public static bool TryGetClient<TClient>(this IConnectableService<TClient> connectableService, string id, out TClient client)
                where TClient : IIdClient, IClient
    {
        return connectableService.Clients.TryGetClient(id, out client);
    }

    #endregion IConnectableService<TClient>
}