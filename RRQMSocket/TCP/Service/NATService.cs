using RRQMCore.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// NAT服务器
    /// </summary>
    public class NATService : TcpService<NATSocketClient>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="serviceConfig"></param>
        protected override void LoadConfig(ServiceConfig serviceConfig)
        {
            if (serviceConfig.ReceiveType== ReceiveType.NetworkStream)
            {
                throw new RRQMException("转发服务器不允许流模式。");
            }
            base.LoadConfig(serviceConfig);
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected sealed override void OnConnecting(NATSocketClient socketClient, ClientOperationEventArgs e)
        {
            IPHost iPHost = this.ServiceConfig.GetValue<IPHost>(NATServiceConfig.TargetIPHostProperty);
            Socket socket = new Socket(iPHost.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(iPHost.EndPoint);
            socketClient.BeginRunTargetSocket(socket);

            base.OnConnecting(socketClient, e);
        }
    }
}
