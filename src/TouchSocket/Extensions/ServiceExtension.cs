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
        public static TService Start<TService>(this TService service, params IPHost[] iPHosts) where TService : ITcpService
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
            return service;
        }

        #endregion ITcpService

        #region Udp

        /// <inheritdoc cref="IService.Start"/>
        public static TService Start<TService>(this TService service, IPHost iPHost) where TService : IUdpSession
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
            return service;
        }

        #endregion Udp
    }
}