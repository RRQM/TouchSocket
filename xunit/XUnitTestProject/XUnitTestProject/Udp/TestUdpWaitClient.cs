using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace XUnitTestProject.Udp
{
    public class TestUdpWaitClient : UnitBase
    {
        [Fact]
        public void UdpWaitClientShouldBeOk()
        {
            var waitTime = 500;
            var udpSender = new UdpSession();
            var revCount = 0;
            udpSender.Received += (endpoint, e) =>
            {
                Interlocked.Increment(ref revCount);
                return Task.CompletedTask;
            };

            var port1 = new Random().Next(10000, 60000);
            Thread.Sleep(1000);
            var port2 = new Random().Next(10000, 60000);

            udpSender.Setup(new TouchSocketConfig()//加载配置
                .SetUdpDataHandlingAdapter(() => new NormalUdpDataHandlingAdapter())
                .SetRemoteIPHost(new IPHost($"127.0.0.1:{port1}"))
                .SetBindIPHost(new IPHost($"127.0.0.1:{port2}")))
                .Start();

            var udpReceiver = new UdpSession();
            udpReceiver.Received += (c, e) =>
            {
                lock (udpReceiver)
                {
                    udpReceiver.Send(e.EndPoint, e.ByteBlock);//将接收到的数据发送至默认终端
                }

                return Task.CompletedTask;
            };

            var config = new TouchSocketConfig();
            config.SetBindIPHost(new IPHost($"127.0.0.1:{port1}"))
                .SetUdpDataHandlingAdapter(() => new NormalUdpDataHandlingAdapter());

            udpReceiver.Setup(config);//加载配置
            udpReceiver.Start();//启动


            using (var waitingClient = udpSender.CreateWaitingClient(new WaitingOptions()))
            {
                for (int i = 0; i < 10; i++)
                {
                    var bytes = waitingClient.SendThenReturn("RRQM");
                    Assert.Equal("RRQM", Encoding.UTF8.GetString(bytes));
                    Thread.Sleep(waitTime);
                }
            }
        }

        [Fact]
        public async Task UdpWaitClientAsyncShouldBeOk()
        {
            var waitTime = 500;
            var udpSender = new UdpSession();
            var revCount = 0;
            udpSender.Received += (endpoint, e) =>
            {
                Interlocked.Increment(ref revCount);
                return Task.CompletedTask;
            };

            var port1 = new Random().Next(10000, 60000);
            Thread.Sleep(1000);
            var port2 = new Random().Next(10000, 60000);

            udpSender.Setup(new TouchSocketConfig()//加载配置
                .SetUdpDataHandlingAdapter(() => new NormalUdpDataHandlingAdapter())
                .SetRemoteIPHost(new IPHost($"127.0.0.1:{port1}"))
                .SetBindIPHost(new IPHost($"127.0.0.1:{port2}")))
                .Start();

            var udpReceiver = new UdpSession();
            udpReceiver.Received += async (c, e) =>
            {
                await udpReceiver.SendAsync(e.EndPoint, e.ByteBlock);//将接收到的数据发送至默认终端
            };

            var config = new TouchSocketConfig();
            config.SetBindIPHost(new IPHost($"127.0.0.1:{port1}"))
                .SetUdpDataHandlingAdapter(() => new NormalUdpDataHandlingAdapter());

            udpReceiver.Setup(config);//加载配置
            udpReceiver.Start();//启动


            using (var waitingClient = udpSender.CreateWaitingClient(new WaitingOptions()))
            {
                for (int i = 0; i < 10; i++)
                {
                    var bytes = await waitingClient.SendThenReturnAsync("RRQM");
                    Assert.Equal("RRQM", Encoding.UTF8.GetString(bytes));
                    await Task.Delay(waitTime);
                }
            }
        }
    }
}
