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

using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace XUnitTestProject.Tcp
{
    public class TestTcpClient : UnitBase
    {
        [Fact]
        public async Task TLVShouldCanConnectAndReceive()
        {
            var sleep = 1000;
            var client = new TcpClient();

            TLVDataFrame requestInfo = null;
            client.Received = async (c, e) =>
            {
                requestInfo = (TLVDataFrame)e.RequestInfo;
                await Task.CompletedTask;
            };

            client.Setup(new TouchSocketConfig()
                .SetNoDelay(true)
                .ConfigurePlugins(a =>
                {
                    a.Add<TLVPlugin>();
                })
                .SetRemoteIPHost("127.0.0.1:7805"));
            client.Connect();

            client.Send(new TLVDataFrame(10, Encoding.UTF8.GetBytes("rrqm")));
            await Task.Delay(sleep);
            Assert.NotNull(requestInfo);
            Assert.True(requestInfo.Tag == 10 && requestInfo.GetValueString() == "rrqm");

            requestInfo = null;
            for (var i = 0; i < 10; i++)
            {
                Assert.True(client.PingWithTLV());
                Assert.Null(requestInfo);
            }

            client.Send(new TLVDataFrame(10, Encoding.UTF8.GetBytes("rrqm")));
            await Task.Delay(sleep);
            Assert.NotNull(requestInfo);
            Assert.True(requestInfo.Tag == 10 && requestInfo.GetValueString() == "rrqm");
        }

        [Fact]
        public async Task ShouldCanConnectAndReceive()
        {
            var waitTime = 100;
            var client = new TcpClient();

            var connected = false;
            var disconnectCount = 0;
            client.Connected = async (client, e) =>
            {
                connected = true;
                await Task.CompletedTask;
            };
            client.Disconnected = async (client, e) =>
            {
                disconnectCount++;
                connected = false;
                await Task.CompletedTask;
            };

            var receivedCount = 0;
            client.Received = async (tcpClient, e) =>
            {
                receivedCount++;
                await Task.CompletedTask;
            };

            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789"));//载入配置
            await client.ConnectAsync();//连接

            await Task.Delay(waitTime);

            Assert.True(client.Online);
            Assert.Equal("127.0.0.1", client.IP);
            Assert.Equal(7789, client.Port);
            Assert.True(connected);

            client.Send(BitConverter.GetBytes(1));
            await Task.Delay(waitTime);
            Assert.Equal(1, receivedCount);

            client.Send(BitConverter.GetBytes(2));
            await Task.Delay(waitTime);
            Assert.Equal(2, receivedCount);

            client.Close();
            await Task.Delay(waitTime);
            Assert.True(!client.Online);
            Assert.True(!connected);
            Assert.Equal(1, disconnectCount);

            client.Connect();
            await Task.Delay(waitTime);
            Assert.True(client.Online);
            Assert.Equal("127.0.0.1", client.IP);
            Assert.Equal(7789, client.Port);
            Assert.True(connected);

            client.Send(BitConverter.GetBytes(3));
            await Task.Delay(waitTime);
            Assert.Equal(3, receivedCount);

            client.Send(BitConverter.GetBytes(4));
            await Task.Delay(waitTime);
            Assert.Equal(4, receivedCount);

            client.Dispose();
            await Task.Delay(waitTime);
            Assert.True(!client.Online);
            Assert.True(!connected);
            Assert.Equal(2, disconnectCount);

            Assert.ThrowsAny<Exception>(() =>
            {
                client.Connect();
            });

            await Task.Delay(waitTime);
            Assert.Equal(2, disconnectCount);
        }

        [Fact]
        public async Task ShouldCanConnectAndReceiveAsync()
        {
            var waitTime = 100;
            var client = new TcpClient();

            var connected = false;
            var disconnectCount = 0;
            client.Connected = (client, e) =>
            {
                connected = true;
                return Task.CompletedTask;
            };
            client.Disconnected = (client, e) =>
            {
                disconnectCount++;
                connected = false;
                return Task.CompletedTask;
            };

            var receivedCount = 0;
            client.Received = (tcpClient, e) =>
            {
                receivedCount++;
                return Task.CompletedTask;
            };

            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789"));//载入配置
            await client.ConnectAsync();//连接

            await Task.Delay(waitTime);

            Assert.True(client.Online);
            Assert.Equal("127.0.0.1", client.IP);
            Assert.Equal(7789, client.Port);
            Assert.True(connected);

            await client.SendAsync(BitConverter.GetBytes(1));
            await Task.Delay(waitTime);
            Assert.Equal(1, receivedCount);

            await client.SendAsync(BitConverter.GetBytes(2));
            await Task.Delay(waitTime);
            Assert.Equal(2, receivedCount);

            client.Close();
            await Task.Delay(waitTime);
            Assert.True(!client.Online);
            Assert.True(!connected);
            Assert.Equal(1, disconnectCount);

            await client.ConnectAsync();
            await Task.Delay(waitTime);
            Assert.True(client.Online);
            Assert.Equal("127.0.0.1", client.IP);
            Assert.Equal(7789, client.Port);
            Assert.True(connected);

            await client.SendAsync(BitConverter.GetBytes(3));
            await Task.Delay(waitTime);
            Assert.Equal(3, receivedCount);

            await client.SendAsync(BitConverter.GetBytes(4));
            await Task.Delay(waitTime);
            Assert.Equal(4, receivedCount);

            client.Dispose();
            await Task.Delay(waitTime);
            Assert.True(!client.Online);
            Assert.True(!connected);
            Assert.Equal(2, disconnectCount);

            await Assert.ThrowsAnyAsync<Exception>(async () =>
             {
                 await client.ConnectAsync();
             });

            await Task.Delay(waitTime);
            Assert.Equal(2, disconnectCount);
        }

        [Fact]
        public void ConnectShouldBeOk()
        {
            var waitTime = 500;
            var client = new TcpClient();

            var connectingCount = 0L;
            var connectedCount = 0L;
            var disconnectedCount = 0L;
            var disconnectingCount = 0L;

            client.Connecting = (client, e) =>
            {
                Interlocked.Increment(ref connectingCount);
                return Task.CompletedTask;
            };

            client.Connected = (client, e) =>
            {
                Interlocked.Increment(ref connectedCount);
                return Task.CompletedTask;
            };

            client.Disconnecting = (client, e) =>
            {
                Interlocked.Increment(ref disconnectingCount);
                return Task.CompletedTask;
            };

            client.Disconnected = (client, e) =>
            {
                Interlocked.Increment(ref disconnectedCount);
                return Task.CompletedTask;
            };

            var receivedCount = 0;
            client.Received = (tcpClient, e) =>
            {
                receivedCount++;
                return Task.CompletedTask;
            };

            for (var i = 0; i < 10; i++)
            {
                client.Connect("127.0.0.1:7789");
                Assert.True(client.Online);
                Thread.Sleep(waitTime);

                Assert.Equal(i + 1, Interlocked.Read(ref connectingCount));
                Assert.Equal(i + 1, Interlocked.Read(ref connectedCount));

                client.Close();
                Thread.Sleep(waitTime);

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
            Thread.Sleep(waitTime);

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
        public void ReconnectShouldBeOk()
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
                    a.UseReconnection(TimeSpan.FromSeconds(1));
                }));//载入配置


            Thread.Sleep(waitTime);

            Assert.False(client.Online);

            client.Connect();
            Assert.True(client.Online);

            Thread.Sleep(waitTime);

            service.Clear();
            Thread.Sleep(waitTime);
            Assert.Equal(1, disconnectedCount);

            Thread.Sleep(waitTime);
            Assert.True(client.Online);

            client.Close();

            Thread.Sleep(waitTime);
            Assert.Equal(2, disconnectedCount);

            Thread.Sleep(waitTime);
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
            await Task.Delay(waitTime);
            Assert.Equal(2, disconnectedCount);

            await Task.Delay(waitTime);
            Assert.True(client.Online);
        }

        [Fact]
        public void ConnectIpv6ShouldBeOk()
        {
            var client = new TcpClient();

            client.Connect("[2409:8a38:836:6711:c868:de10:445:4]:7812");
            Assert.True(client.Online);
        }
    }
}