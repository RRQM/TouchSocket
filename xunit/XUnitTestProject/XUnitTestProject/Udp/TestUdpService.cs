//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace XUnitTestProject.Udp
{
    public class TestUdpService : UnitBase
    {
        [Fact]
        public void ShouldShowProperties()
        {
            var udpSession = new UdpSession();
            Assert.Equal(ServerState.None, udpSession.ServerState);

            var config = new TouchSocketConfig();//UDP配置
            config.SetRemoteIPHost(new IPHost("127.0.0.1:10086"))
                .SetBindIPHost(new IPHost("127.0.0.1:10087"))
                .SetServerName("RRQMUdpServer");

            udpSession.Setup(config);//加载配置
            udpSession.Start();//启动

            Assert.Equal(ServerState.Running, udpSession.ServerState);
            Assert.Equal("RRQMUdpServer", udpSession.ServerName);
            Assert.Equal("tcp://127.0.0.1:10086/", udpSession.RemoteIPHost.ToString());

            udpSession.Stop();
            Assert.Equal(ServerState.Stopped, udpSession.ServerState);

            udpSession.Start();
            Assert.Equal(ServerState.Running, udpSession.ServerState);
            Assert.Equal("RRQMUdpServer", udpSession.ServerName);
            Assert.Equal("tcp://127.0.0.1:10086/", udpSession.RemoteIPHost.ToString());

            udpSession.Dispose();
            Assert.Equal(ServerState.Disposed, udpSession.ServerState);

            Assert.ThrowsAny<Exception>(() =>
            {
                udpSession.Start();
            });
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        public void NormalShouldCanSendAndReceive(int count)
        {
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

            for (var i = 0; i < count; i++)
            {
                udpSender.Send(BitConverter.GetBytes(i));
            }

            Thread.Sleep(5000);
            Assert.Equal(count, revCount);

            revCount = 0;

            for (var i = 0; i < count; i++)
            {
                udpSender.SendAsync(BitConverter.GetBytes(i));
            }
            Thread.Sleep(5000);
            Assert.Equal(count, revCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        public void UdpPackageAdapterShouldCanSendAndReceive(int count)
        {
            var udpSender = new UdpSession();
            var revCount = 0;
            udpSender.Received += (c,e) =>
            {
                Interlocked.Increment(ref revCount);
                return Task.CompletedTask;
            };

            var port1 = new Random().Next(10000, 60000);
            Thread.Sleep(1000);
            var port2 = new Random().Next(10000, 60000);

            udpSender.Setup(new TouchSocketConfig()//加载配置
                .SetUdpDataHandlingAdapter(() => new UdpPackageAdapter())
                .SetRemoteIPHost(new IPHost($"127.0.0.1:{port1}"))
                .SetBindIPHost(new IPHost($"127.0.0.1:{port2}")))
                .Start();

            var udpReceiver = new UdpSession();
            udpReceiver.Received += (c,e) =>
            {
                lock (udpReceiver)
                {
                    udpReceiver.Send(e.EndPoint, e.ByteBlock);//将接收到的数据发送至默认终端
                }

                return Task.CompletedTask;
            };

            var config = new TouchSocketConfig();
            config.SetBindIPHost(new IPHost($"127.0.0.1:{port1}"))
                .SetUdpDataHandlingAdapter(() => new UdpPackageAdapter());

            udpReceiver.Setup(config);//加载配置
            udpReceiver.Start();//启动

            for (var i = 0; i < count; i++)
            {
                udpSender.Send(BitConverter.GetBytes(i));
            }

            Thread.Sleep(5000);
            Assert.Equal(count, revCount);

            revCount = 0;

            for (var i = 0; i < count; i++)
            {
                udpSender.SendAsync(BitConverter.GetBytes(i));
            }
            Thread.Sleep(5000);
            Assert.Equal(count, revCount);
        }

        [Fact]
        public void BigUdpPackageAdapterShouldCanSendAndReceive()
        {
            var udpSender = new UdpSession();
            var revCount = 0;
            udpSender.Received += (c,e) =>
            {
                Interlocked.Increment(ref revCount);
                return Task.CompletedTask;
            };

            var port1 = new Random().Next(10000, 60000);
            Thread.Sleep(1000);
            var port2 = new Random().Next(10000, 60000);

            udpSender.Setup(new TouchSocketConfig()//加载配置
                .SetUdpDataHandlingAdapter(() => new UdpPackageAdapter())
                .SetRemoteIPHost(new IPHost($"127.0.0.1:{port1}"))
                .SetBindIPHost(new IPHost($"127.0.0.1:{port2}")))
                .Start();

            var udpReceiver = new UdpSession();
            udpReceiver.Received += (c,e) =>
            {
                lock (udpReceiver)
                {
                    udpReceiver.Send(e.EndPoint, e.ByteBlock);//将接收到的数据发送至默认终端
                }

                return Task.CompletedTask;
            };

            var config = new TouchSocketConfig();
            config.SetBindIPHost(new IPHost($"127.0.0.1:{port1}"))
                 .SetThreadCount(10)
                .SetUdpDataHandlingAdapter(() => new UdpPackageAdapter());

            udpReceiver.Setup(config);//加载配置
            udpReceiver.Start();//启动

            for (var i = 0; i < 10; i++)
            {
                udpSender.Send(new byte[1024 * 512]);
            }

            Thread.Sleep(5000);
            Assert.Equal(10, revCount);

            revCount = 0;

            for (var i = 0; i < 10; i++)
            {
                udpSender.Send(new byte[1024 * 512]);
            }
            Thread.Sleep(5000);
            Assert.Equal(10, revCount);
        }
    }
}