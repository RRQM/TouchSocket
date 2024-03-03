using TouchSocket.Core;
using TouchSocket.Sockets;

namespace ReuseAddressServerConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var service = new TcpService();
            service.Setup(new TouchSocketConfig()//载入配置
                .UseReuseAddress()
                .SetListenIPHosts("tcp://127.0.0.1:7789", 7790)//同时监听两个地址
                .ConfigureContainer(a =>//容器的配置顺序应该在最前面
                {
                    a.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
                })
                .ConfigurePlugins(a =>
                {
                    a.Add<MyClassPlugin>();
                    //a.Add();//此处可以添加插件
                }));

            service.Start();//启动

            service.Logger.Info("服务器已启动");

            Console.ReadKey();
        }
    }

    internal class MyClassPlugin : PluginBase, ITcpConnectedPlugin<ISocketClient>, ITcpDisconnectedPlugin<ISocketClient>, ITcpReceivedPlugin<ISocketClient>
    {
        private readonly ILog m_logger;

        public MyClassPlugin(ILog logger)
        {
            this.m_logger = logger;
        }

        public async Task OnTcpConnected(ISocketClient client, ConnectedEventArgs e)
        {
            this.m_logger.Info($"已连接，信息：{client.GetInfo()}");
            await e.InvokeNext();
        }

        public async Task OnTcpDisconnected(ISocketClient client, DisconnectEventArgs e)
        {
            this.m_logger.Info($"已断开，信息：{client.GetInfo()}");
            await e.InvokeNext();
        }

        public async Task OnTcpReceived(ISocketClient client, ReceivedDataEventArgs e)
        {
            this.m_logger.Info($"收到数据，信息：{e.ByteBlock.ToString()}");
            await e.InvokeNext();
        }
    }
}