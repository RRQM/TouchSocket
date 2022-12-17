using System;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace NATServiceConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            MyNATService service = new MyNATService();
            var config = new TouchSocketConfig();
            config.SetListenIPHosts(new IPHost[] { new IPHost(7788) });

            service.Setup(config);
            service.Start();

            Console.WriteLine("转发服务器已启动。已将7788端口转发到127.0.0.1:7789地址");
        }
    }

    internal class MyNATService : NATService
    {
        protected override void OnConnected(NATSocketClient socketClient, TouchSocketEventArgs e)
        {
            base.OnConnected(socketClient, e);

            try
            {
                //此处模拟的是只要连接到NAT服务器，就转发。
                //实际上，这个方法可以随时调用。
                socketClient.AddTargetClient(new TouchSocketConfig()
                    .SetRemoteIPHost("127.0.0.1:7789")
                    .ConfigurePlugins(a =>
                    {
                        //在企业版中，使用以下任意方式，可实现转发客户端的断线重连。
                        a.Add<PollingKeepAlivePlugin<TcpClient>>()
                        .SetTick(TimeSpan.FromSeconds(1));//每秒检查
                        //a.UseReconnection();
                    }));
            }
            catch (Exception ex)
            {
                socketClient.Logger.Exception(ex);
            }
        }

        protected override void OnTargetClientDisconnected(NATSocketClient socketClient, ITcpClient tcpClient, ClientDisconnectedEventArgs e)
        {
            socketClient.Logger.Info($"{socketClient.IP}:{socketClient.Port}的转发客户端{tcpClient.IP}:{tcpClient.Port}已经断开连接。");
            base.OnTargetClientDisconnected(socketClient, tcpClient, e);
        }

        protected override byte[] OnNATReceived(NATSocketClient socketClient, ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            //服务器收到的数据
            return base.OnNATReceived(socketClient, byteBlock, requestInfo);
        }

        protected override byte[] OnTargetClientReceived(NATSocketClient socketClient, ITcpClient tcpClient, ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            //连接的客户端收到的数据
            return base.OnTargetClientReceived(socketClient, tcpClient, byteBlock, requestInfo);
        }
    }
}