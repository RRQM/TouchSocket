using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TcpServiceForWebApi.Plugins
{
    public class MyTcpPlugin : PluginBase, ITcpConnectedPlugin<ISocketClient>
    {
        private ILogger<MyTcpPlugin> m_logger;

        public MyTcpPlugin(ILogger<MyTcpPlugin> logger)
        {
            this.m_logger = logger;
        }

        public async Task OnTcpConnected(ISocketClient client, ConnectedEventArgs e)
        {
            this.m_logger.LogInformation("客户端连接");
            await e.InvokeNext();
        }
    }
}