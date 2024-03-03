using System.Net;
using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace UdpBroadcastConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //创建udpService
            var udpService = new UdpSession();
            udpService.Setup(new TouchSocketConfig()
                .SetBindIPHost(new IPHost(7789))
                .UseBroadcast()
                .ConfigurePlugins(a =>
                {
                    a.Add<MyPluginClass1>();
                    a.Add<MyPluginClass2>();
                    a.Add<MyPluginClass3>();
                })
                .SetUdpDataHandlingAdapter(() => new NormalUdpDataHandlingAdapter()));
            udpService.Start();

            //加入组播组
            udpService.JoinMulticastGroup(IPAddress.Parse("224.5.6.7"));

            var udpClient = new UdpSession();
            udpClient.Setup(new TouchSocketConfig()
                //.UseUdpReceive()//作为客户端时，如果需要接收数据，那么需要绑定端口。要么使用SetBindIPHost指定端口，要么调用UseUdpReceive绑定随机端口。
                .SetBindIPHost(new IPHost(7788))
                .UseBroadcast()//该配置在广播时是必须的
                .SetUdpDataHandlingAdapter(() => new NormalUdpDataHandlingAdapter()));
            udpClient.Start();

            while (true)
            {
                udpClient.Send(new IPEndPoint(IPAddress.Parse("224.5.6.7"), 7789), Encoding.UTF8.GetBytes("我是组播"));
                udpClient.Send(new IPEndPoint(IPAddress.Parse("255.255.255.255"), 7789), Encoding.UTF8.GetBytes("我是广播"));

                udpClient.Send(new IPEndPoint(IPAddress.Parse("224.5.6.7"), 7789), Encoding.UTF8.GetBytes("hello"));
                udpClient.Send(new IPEndPoint(IPAddress.Parse("255.255.255.255"), 7789), Encoding.UTF8.GetBytes("hi"));
                Thread.Sleep(1000);
            }
        }

        private class MyPluginClass1 : PluginBase, IUdpReceivedPlugin
        {
            public async Task OnUdpReceived(IUdpSession client, UdpReceivedDataEventArgs e)
            {
                var msg = e.ByteBlock.ToString();
                if (msg == "hello")
                {
                    Console.WriteLine("已处理Hello");
                }
                else
                {
                    //如果判断逻辑发现此处无法处理，即可转到下一个插件
                    await e.InvokeNext();
                }
            }
        }

        private class MyPluginClass2 : PluginBase, IUdpReceivedPlugin
        {
            public async Task OnUdpReceived(IUdpSession client, UdpReceivedDataEventArgs e)
            {
                var msg = e.ByteBlock.ToString();
                if (msg == "hi")
                {
                    Console.WriteLine("已处理Hi");
                }
                else
                {
                    //如果判断逻辑发现此处无法处理，即可转到下一个插件
                    await e.InvokeNext();
                }
            }
        }

        private class MyPluginClass3 : PluginBase, IUdpReceivedPlugin
        {
            public async Task OnUdpReceived(IUdpSession client, UdpReceivedDataEventArgs e)
            {
                var msg = e.ByteBlock.ToString();
                Console.WriteLine(msg);
                await Task.CompletedTask;
            }
        }
    }
}