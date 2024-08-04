//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace NATServiceConsoleApp
{
    internal class MyNATService : NATService
    {
        protected override Task<byte[]> OnNATReceived(NATSessionClient socketClient, ReceivedDataEventArgs e)
        {
            //服务器收到的数据
            return base.OnNATReceived(socketClient, e);
        }

        protected override Task OnTargetClientClosed(NATSessionClient socketClient, ITcpClient tcpClient, ClosedEventArgs e)
        {
            socketClient.Logger.Info($"{socketClient.IP}:{socketClient.Port}的转发客户端{tcpClient.IP}:{tcpClient.Port}已经断开连接。");
            return base.OnTargetClientClosed(socketClient, tcpClient, e);
        }

        protected override Task<byte[]> OnTargetClientReceived(NATSessionClient socketClient, ITcpClient tcpClient, ReceivedDataEventArgs e)
        {
            //连接的客户端收到的数据
            return base.OnTargetClientReceived(socketClient, tcpClient, e);
        }

        protected override async Task OnTcpConnected(NATSessionClient socketClient, ConnectedEventArgs e)
        {
            await base.OnTcpConnected(socketClient, e);
            try
            {
                //此处模拟的是只要连接到NAT服务器，就转发。
                //实际上，这个方法可以随时调用。
                await socketClient.AddTargetClientAsync(new TouchSocketConfig()
                     .SetRemoteIPHost("127.0.0.1:7789")
                     .ConfigurePlugins(a =>
                     {
                     }));
            }
            catch (Exception ex)
            {
                socketClient.Logger.Exception(ex);
            }
        }
    }

    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var service = new MyNATService();
            var config = new TouchSocketConfig();
            config.SetListenIPHosts(new IPHost[] { new IPHost(7788) });

            await service.SetupAsync(config);
            await service.StartAsync();

            Console.WriteLine("转发服务器已启动。已将7788端口转发到127.0.0.1:7789地址");
        }
    }
}