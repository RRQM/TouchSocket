using Microsoft.Extensions.Logging;
using TouchSocket.Core;
using TouchSocket.Core.ByteManager;
using TouchSocket.Sockets;
using TouchSocket.Sockets.Plugins;

namespace TcpServiceForWebApi.Plugins
{
    public class MyTcpPlugin : TcpPluginBase<SocketClient>
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

        protected override void OnReceivedData(SocketClient client, ReceivedDataEventArgs e)
        {
            //这里处理数据接收
            //根据适配器类型，e.ByteBlock与e.RequestInfo会呈现不同的值，具体看文档=》适配器部分。
            ByteBlock byteBlock = e.ByteBlock;
            IRequestInfo requestInfo = e.RequestInfo;
        }
    }
}