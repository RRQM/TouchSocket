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
        #region ITcpService

        /// <inheritdoc cref="IService.Start"/>
        public static void Start<TService>(this TService service, params IPHost[] iPHosts) where TService : ITcpServiceBase
        {
            TouchSocketConfig config;
            if (service.Config == null)
            {
                config = new TouchSocketConfig();
                config.SetListenIPHosts(iPHosts);
                service.Setup(config);
            }
            else
            {
                config = service.Config;
                config.SetListenIPHosts(iPHosts);
            }
            service.Start();
        }

        /// <inheritdoc cref="IService.StartAsync"/>
        public static async Task StartAsync<TService>(this TService service, params IPHost[] iPHosts) where TService : ITcpServiceBase
        {
            TouchSocketConfig config;
            if (service.Config == null)
            {
                config = new TouchSocketConfig();
                config.SetListenIPHosts(iPHosts);
                service.Setup(config);
            }
            else
            {
                config = service.Config;
                config.SetListenIPHosts(iPHosts);
            }
            await service.StartAsync();
        }

        #endregion ITcpService

        #region Udp

        /// <inheritdoc cref="IService.Start"/>
        public static void Start<TService>(this TService service, IPHost iPHost) where TService : IUdpSession
        {
            TouchSocketConfig config;
            if (service.Config == null)
            {
                config = new TouchSocketConfig();
                config.SetBindIPHost(iPHost);
                service.Setup(config);
            }
            else
            {
                config = service.Config;
                config.SetBindIPHost(iPHost);
            }
            service.Start();
        }

        /// <inheritdoc cref="IService.Start"/>
        public static async Task StartAsync<TService>(this TService service, IPHost iPHost) where TService : IUdpSession
        {
            TouchSocketConfig config;
            if (service.Config == null)
            {
                config = new TouchSocketConfig();
                config.SetBindIPHost(iPHost);
                service.Setup(config);
            }
            else
            {
                config = service.Config;
                config.SetBindIPHost(iPHost);
            }
            await service.StartAsync();
        }

        #endregion Udp
    }
}