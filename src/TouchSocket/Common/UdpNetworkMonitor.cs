using System;
using System.Net.Sockets;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// Udp监听器
    /// </summary>
    public class UdpNetworkMonitor
    {
        /// <summary>
        /// Udp监听器
        /// </summary>
        /// <param name="iPHost"></param>
        /// <param name="socket"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public UdpNetworkMonitor(IPHost iPHost, Socket socket)
        {
            this.IPHost = iPHost;
            this.Socket = socket ?? throw new ArgumentNullException(nameof(socket));
        }

        /// <summary>
        /// IPHost
        /// </summary>
        public IPHost IPHost { get; }

        /// <summary>
        /// Socket组件
        /// </summary>
        public Socket Socket { get; private set; }
    }
}