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
using System;
using System.Text;
using System.Threading;
using TouchSocket.Core.Config;
using TouchSocket.Core.Plugins;
using TouchSocket.Sockets;
using TouchSocket.Sockets.Plugins;
using Xunit;

namespace XUnitTest.TCP
{
    public class TestTcpClient:UnitBase
    {
        [Fact]
        public void TLVShouldCanConnectAndReceive()
        {
            int sleep = 1000;
            TcpClient client = new TcpClient();

            TLVDataFrame requestInfo = null;
            client.Received += (c, b, i) =>
            {
                requestInfo = (TLVDataFrame)i;
            };

          
            client.Setup(new TouchSocketConfig()
                  .UsePlugin()
                .ConfigurePlugins(a =>
                {
                    a.Add<TLVPlugin>();
                })
                .SetRemoteIPHost(new IPHost("127.0.0.1:7805")));
            client.Connect();

            client.Send(new TLVDataFrame(10,Encoding.UTF8.GetBytes("rrqm")));
            Thread.Sleep(sleep);
            Assert.NotNull(requestInfo);
            Assert.True(requestInfo.Tag == 10 && (requestInfo.GetValueString() == "rrqm"));

            requestInfo = null;
            for (int i = 0; i < 10; i++)
            {
                Assert.True(client.Ping());
                Assert.Null(requestInfo);
            }


            client.Send(new TLVDataFrame(10, Encoding.UTF8.GetBytes("rrqm")));
            Thread.Sleep(sleep);
            Assert.NotNull(requestInfo);
            Assert.True(requestInfo.Tag == 10 && (requestInfo.GetValueString() == "rrqm"));
        }

        [Fact]
        public void ShouldCanConnectAndReceive()
        {
            int waitTime = 100;
            TcpClient client = new TcpClient();

            bool connected = false;
            int disconnectCount = 0;
            client.Connected += (client, e) =>
            {
                connected = true;
            };
            client.Disconnected += (client, e) =>
            {
                disconnectCount++;
                connected = false;
            };

            int receivedCount = 0;
            client.Received += (tcpClient, arg1, arg2) =>
            {
                receivedCount++;
            };

            client.Setup("127.0.0.1:7789");//载入配置
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
    }
}