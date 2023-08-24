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
using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace XUnitTestProject.Tcp
{
    public class TestTcpClient : UnitBase
    {
        [Fact]
        public void TLVShouldCanConnectAndReceive()
        {
            var sleep = 1000;
            var client = new TcpClient();

            TLVDataFrame requestInfo = null;
            client.Received += (c, b, i) =>
            {
                requestInfo = (TLVDataFrame)i;
            };

            client.Setup(new TouchSocketConfig()
                .ConfigurePlugins(a =>
                {
                    a.Add<TLVPlugin>();
                })
                .SetRemoteIPHost(new IPHost("127.0.0.1:7805")));
            client.Connect();

            client.Send(new TLVDataFrame(10, Encoding.UTF8.GetBytes("rrqm")));
            Thread.Sleep(sleep);
            Assert.NotNull(requestInfo);
            Assert.True(requestInfo.Tag == 10 && requestInfo.GetValueString() == "rrqm");

            requestInfo = null;
            for (var i = 0; i < 10; i++)
            {
                Assert.True(client.PingWithTLV());
                Assert.Null(requestInfo);
            }

            client.Send(new TLVDataFrame(10, Encoding.UTF8.GetBytes("rrqm")));
            Thread.Sleep(sleep);
            Assert.NotNull(requestInfo);
            Assert.True(requestInfo.Tag == 10 && requestInfo.GetValueString() == "rrqm");
        }

        [Theory]
        [InlineData(ReceiveType.Iocp)]
        [InlineData(ReceiveType.Bio)]
        public void ShouldCanConnectAndReceive(ReceiveType receiveType)
        {
            var waitTime = 100;
            var client = new TcpClient();

            var connected = false;
            var disconnectCount = 0;
            client.Connected += (client, e) =>
            {
                connected = true;
            };
            client.Disconnected += (client, e) =>
            {
                disconnectCount++;
                connected = false;
            };

            var receivedCount = 0;
            client.Received += (tcpClient, arg1, arg2) =>
            {
                receivedCount++;
            };

            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789")
                .SetReceiveType(receiveType));//载入配置
            client.Connect();//连接
            Thread.Sleep(waitTime);

            Assert.True(client.Online);
            Assert.Equal("127.0.0.1", client.IP);
            Assert.Equal(7789, client.Port);
            Assert.True(connected);

            client.Send(BitConverter.GetBytes(1));
            Thread.Sleep(waitTime);
            Assert.Equal(1, receivedCount);

            client.Send(BitConverter.GetBytes(2));
            Thread.Sleep(waitTime);
            Assert.Equal(2, receivedCount);

            client.Close();
            Thread.Sleep(waitTime);
            Assert.True(!client.Online);
            Assert.True(!connected);
            Assert.Equal(1, disconnectCount);

            client.Connect();
            Thread.Sleep(waitTime);
            Assert.True(client.Online);
            Assert.Equal("127.0.0.1", client.IP);
            Assert.Equal(7789, client.Port);
            Assert.True(connected);

            client.Send(BitConverter.GetBytes(3));
            Thread.Sleep(waitTime);
            Assert.Equal(3, receivedCount);

            client.Send(BitConverter.GetBytes(4));
            Thread.Sleep(waitTime);
            Assert.Equal(4, receivedCount);

            client.Dispose();
            Thread.Sleep(waitTime);
            Assert.True(!client.Online);
            Assert.True(!connected);
            Assert.Equal(2, disconnectCount);

            Assert.ThrowsAny<Exception>(() =>
            {
                client.Connect();
            });

            Thread.Sleep(1000);
            Assert.Equal(2, disconnectCount);
        }

        [Fact]
        public void ConnectShouldBeOk()
        {
            var waitTime = 100;
            var client = new TcpClient();

            var connectingCount = 0L;
            var connectedCount = 0L;
            var disconnectedCount = 0L;
            var disconnectingCount = 0L;

            client.Connecting = (client, e) =>
            {
                Interlocked.Increment(ref connectingCount);
            };

            client.Connected = (client, e) =>
            {
                Interlocked.Increment(ref connectedCount);
            };

            client.Disconnecting = (client, e) =>
            {
                Interlocked.Increment(ref disconnectingCount);
            };

            client.Disconnected = (client, e) =>
            {
                Interlocked.Increment(ref disconnectedCount);
            };

            var receivedCount = 0;
            client.Received = (tcpClient, byteBlock, requestInfo) =>
            {
                receivedCount++;
            };

            client.Setup("127.0.0.1:7789");//载入配置

            for (var i = 0; i < 1000; i++)
            {
                client.Connect();
                Assert.True(client.Online);
                //Thread.Sleep(1);

                Assert.Equal(i + 1, Interlocked.Read(ref connectingCount));
                Assert.Equal(i + 1, Interlocked.Read(ref connectedCount));

                client.Close();
                //Thread.Sleep(1);

                Assert.False(client.Online);
                Assert.Equal(i + 1, Interlocked.Read(ref disconnectingCount));
                Assert.Equal(i + 1, Interlocked.Read(ref disconnectedCount));
            }

            client.Connect();
            Thread.Sleep(waitTime);

            connectingCount = 0;
            connectedCount = 0;
            disconnectingCount = 0;
            disconnectedCount = 0;

            client.Send(BitConverter.GetBytes(1));
            Thread.Sleep(waitTime);
            Assert.Equal(1, receivedCount);

            client.Send(BitConverter.GetBytes(2));
            Thread.Sleep(waitTime);
            Assert.Equal(2, receivedCount);

            client.Dispose();
            Assert.False(client.Online);
            Assert.Equal(1, Interlocked.Read(ref disconnectingCount));
            Assert.Equal(1, Interlocked.Read(ref disconnectedCount));

            Assert.ThrowsAny<Exception>(() =>
            {
                client.Connect();
            });

            connectingCount = 0;
            connectedCount = 0;
            disconnectingCount = 0;
            disconnectedCount = 0;

            Assert.Equal(0, Interlocked.Read(ref connectingCount));
            Assert.Equal(0, Interlocked.Read(ref connectedCount));
            Assert.Equal(0, Interlocked.Read(ref disconnectingCount));
            Assert.Equal(0, Interlocked.Read(ref disconnectedCount));
        }

        [Fact]
        public async Task ReconnectShouldBeOk()
        {
            var port = new Random().Next(10000, 60000);
            var service = new TcpService();
            service.Start(port);

            var waitTime = 1000;
            var client = new TcpClient();

            var disconnectedCount = 0;
            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost($"127.0.0.1:{port}")
                .ConfigurePlugins(a =>
                {
                    a.Add(nameof(ITcpDisconnectedPlugin.OnTcpDisconnected), () =>
                    {
                        disconnectedCount++;
                    });
                    a.UseReconnection(TimeSpan.FromSeconds(1));
                }));//载入配置

            await Task.Delay(waitTime);

            Assert.False(client.Online);

            client.Connect();
            Assert.True(client.Online);

            await Task.Delay(waitTime);

            service.Clear();
            await Task.Delay(waitTime);
            Assert.Equal(1, disconnectedCount);

            await Task.Delay(waitTime);
            Assert.True(client.Online);

            client.Close();
            Assert.Equal(2, disconnectedCount);
            await Task.Delay(waitTime);
            Assert.False(client.Online);
        }

        [Fact]
        public async Task ReconnectShouldBeOk_2()
        {
            var port = new Random().Next(10000, 60000);
            var service = new TcpService();
            service.Start(port);

            var waitTime = 2000;
            var client = new TcpClient();

            var disconnectedCount = 0;
            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost($"127.0.0.1:{port}")
                .ConfigurePlugins(a =>
                {
                    a.Add(nameof(ITcpDisconnectedPlugin.OnTcpDisconnected), () =>
                    {
                        disconnectedCount++;
                    });
                    a.UseReconnection()
                    .SetTick(TimeSpan.FromMilliseconds(500))
                    .UsePolling();
                }));//载入配置

            await Task.Delay(waitTime);

            Assert.True(client.Online);

            await Task.Delay(waitTime);

            service.Clear();
            await Task.Delay(waitTime);
            Assert.Equal(1, disconnectedCount);

            await Task.Delay(waitTime);
            Assert.True(client.Online);

            client.Close();
            Assert.Equal(2, disconnectedCount);
            await Task.Delay(waitTime);
            Assert.True(client.Online);
        }
    }
}