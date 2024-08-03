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

namespace TouchSocket.Sockets
{
    /// <summary>
    /// ServiceExtension
    /// </summary>
    public static class ServiceExtension
    {
        #region IServiceBase

        public static void Start<TService>(this TService service) where TService : IServiceBase
        {
            service.StartAsync().GetFalseAwaitResult();
        }

        public static void Stop<TService>(this TService service) where TService : IServiceBase
        {
            service.StopAsync().GetFalseAwaitResult();
        }

        #endregion IServiceBase

        #region ITcpService

        /// <inheritdoc cref="IServiceBase.StartAsync"/>
        public static async Task StartAsync<TService>(this TService service, params IPHost[] iPHosts) where TService : ITcpServiceBase
        {
            TouchSocketConfig config;
            if (service.Config == null)
            {
                config = new TouchSocketConfig();
                config.SetListenIPHosts(iPHosts);
                await service.SetupAsync(config).ConfigureAwait(false);
            }
            else
            {
                config = service.Config;
                config.SetListenIPHosts(iPHosts);
            }
            await service.StartAsync().ConfigureAwait(false);
        }

        /// <inheritdoc cref="IServiceBase.StartAsync"/>
        public static void Start<TService>(this TService service, params IPHost[] iPHosts) where TService : ITcpServiceBase
        {
            StartAsync(service, iPHosts).GetFalseAwaitResult();
        }

        #endregion ITcpService

        #region Udp

        /// <inheritdoc cref="IServiceBase.StartAsync"/>
        public static async Task StartAsync<TService>(this TService service, IPHost iPHost) where TService : IUdpSession
        {
            TouchSocketConfig config;
            if (service.Config == null)
            {
                config = new TouchSocketConfig();
                config.SetBindIPHost(iPHost);
                await service.SetupAsync(config).ConfigureAwait(false);
            }
            else
            {
                config = service.Config;
                config.SetBindIPHost(iPHost);
            }
            await service.StartAsync().ConfigureAwait(false);
        }

        /// <inheritdoc cref="IServiceBase.StartAsync"/>
        public static void Start<TService>(this TService service, IPHost iPHost) where TService : IUdpSession
        {
            StartAsync(service, iPHost).GetFalseAwaitResult();
        }

        #endregion Udp

        #region IConnectableService<TClient>

        public static TClient GetClient<TClient>(this IConnectableService<TClient> connectableService, string id)
            where TClient : IIdClient, IClient
        {
            return connectableService.Clients.TryGetClient(id, out var client) ? client : throw new ClientNotFindException();
        }

        public static bool TryGetClient<TClient>(this IConnectableService<TClient> connectableService, string id, out TClient client)
                    where TClient : IIdClient, IClient
        {
            return connectableService.Clients.TryGetClient(id, out client);
        }

        #endregion IConnectableService<TClient>
    }
}