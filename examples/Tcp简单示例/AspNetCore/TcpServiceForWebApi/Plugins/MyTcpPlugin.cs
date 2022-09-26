using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Core.Log;
using TouchSocket.Sockets;
using TouchSocket.Sockets.Plugins;

namespace TcpServiceForWebApi.Plugins
{
    public class MyTcpPlugin:TcpPluginBase<SocketClient>
    {
        private ILogger<MyTcpPlugin> m_logger;

        public MyTcpPlugin(ILogger<MyTcpPlugin> logger)
        {
            this.m_logger = logger;
        }

        protected override void OnConnected(SocketClient client, TouchSocketEventArgs e)
        {
            m_logger.LogInformation("客户端连接");
            base.OnConnected(client, e);
        }
    }
}
