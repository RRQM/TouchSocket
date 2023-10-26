using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.NamedPipe
{
    /// <summary>
    /// NamedPipeServiceExtension
    /// </summary>
    public static class NamedPipeServiceExtension
    {
        /// <inheritdoc cref="IService.Start"/>
        public static TService Start<TService>(this TService service, string pipeName) where TService : INamedPipeService
        {
            TouchSocketConfig config;
            if (service.Config == null)
            {
                config = new TouchSocketConfig();
                config.SetPipeName(pipeName);
                service.Setup(config);
            }
            else
            {
                config = service.Config;
                config.SetPipeName(pipeName);
            }
            service.Start();
            return service;
        }
    }
}