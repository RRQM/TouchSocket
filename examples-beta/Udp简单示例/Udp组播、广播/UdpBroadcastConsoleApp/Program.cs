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
            udpService.Received = (remote, byteBlock, requestInfo) =>
            {
                Console.WriteLine(byteBlock.ToString());
            };
            udpService.Setup(new TouchSocketConfig()
                .SetBindIPHost(new IPHost(7789))
                .UseBroadcast()
                .SetUdpDataHandlingAdapter(() => new NormalUdpDataHandlingAdapter()))
                .Start();

            //加入组播组
            //udpService.JoinMulticastGroup(IPAddress.Parse("224.5.6.7"));

            var udpClient = new UdpSession();
            udpClient.Setup(new TouchSocketConfig()
                .SetBindIPHost(new IPHost(7788))
                .UseBroadcast()//该配置在广播时是必须的
                .SetUdpDataHandlingAdapter(() => new NormalUdpDataHandlingAdapter()))
                .Start();

            while (true)
            {
                udpClient.Send(new IPEndPoint(IPAddress.Parse("224.5.6.7"), 7789), Encoding.UTF8.GetBytes("我是组播"));
                udpClient.Send(new IPEndPoint(IPAddress.Parse("255.255.255.255"), 7789), Encoding.UTF8.GetBytes("我是广播"));
                Thread.Sleep(1000);
            }
        }
    }
}