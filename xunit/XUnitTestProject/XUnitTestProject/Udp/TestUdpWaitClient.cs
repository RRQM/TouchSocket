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
                .SetBindIPHost(new IPHost($"127.0.0.1:{port2}")));
            udpSender.Start();

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
                .SetBindIPHost(new IPHost($"127.0.0.1:{port2}")));
            udpSender.Start();

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