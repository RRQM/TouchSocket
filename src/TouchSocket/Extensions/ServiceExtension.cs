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
/// 提供针对服务的一系列扩展方法。
/// </summary>
public static class ServiceExtension
{
    #region ITcpService
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
    public static async Task StartAsync<TService>(this TService service, IPHost iPHost, CancellationToken cancellationToken = default) where TService : IUdpSession
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
        await service.StartAsync(cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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