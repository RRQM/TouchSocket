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
            service.Setup(new Core.TouchSocketConfig()
                .SetListenIPHosts(iPHosts));
            service.Start();
            return service;
        }

        #endregion ITcpService

        #region Udp

        /// <inheritdoc cref="IService.Start"/>
        public static TService Start<TService>(this TService service, IPHost iPHost) where TService : IUdpSession
        {
            service.Setup(new Core.TouchSocketConfig()
                .SetBindIPHost(iPHost));
            service.Start();
            return service;
        }

        #endregion Udp
    }
}