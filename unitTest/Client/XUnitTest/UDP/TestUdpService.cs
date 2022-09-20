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
using System.Threading;
using TouchSocket.Core.Config;
using TouchSocket.Sockets;
using Xunit;

namespace XUnitTest.UDP
{
    public class TestUdpService
    {
        [Fact]
        public void ShouldShowProperties()
        {
            UdpSession udpSession = new UdpSession();
            Assert.Equal(ServerState.None, udpSession.ServerState);

            var config = new TouchSocketConfig();//UDP配置
            config.SetRemoteIPHost(new IPHost("127.0.0.1:10086"))
                .SetBindIPHost(new IPHost("127.0.0.1:10087"))
                .SetServerName("RRQMUdpServer")
                .SetBufferLength(2048);

            udpSession.Setup(config);//加载配置
            udpSession.Start();//启动

            Assert.Equal(ServerState.Running, udpSession.ServerState);
            Assert.Equal("RRQMUdpServer", udpSession.ServerName);
            Assert.Equal(2048, udpSession.BufferLength);
            Assert.Equal("127.0.0.1:10086", udpSession.RemoteIPHost.ToString());

            udpSession.Stop();
            Assert.Equal(ServerState.Stopped, udpSession.ServerState);

            udpSession.Start();
            Assert.Equal(ServerState.Running, udpSession.ServerState);
            Assert.Equal("RRQMUdpServer", udpSession.ServerName);
            Assert.Equal(2048, udpSession.BufferLength);
            Assert.Equal("127.0.0.1:10086", udpSession.RemoteIPHost.ToString());

            udpSession.Dispose();
            Assert.Equal(ServerState.Disposed, udpSession.ServerState);

            Assert.ThrowsAny<Exception>(() =>
            {
                udpSession.Start();
            });
        }

        [Theory]
        [InlineData(10000)]
        public void ShouldCanSendAndReceive(int count)
        {
            UdpSession udpSession = new UdpSession();
            int revCount = 0;
            udpSession.Received += (endpoint, byteBlock, requestInfo) =>
            {
                Interlocked.Increment(ref revCount);
            };

            udpSession.Setup(new TouchSocketConfig()//加载配置
                .SetUdpDataHandlingAdapter(()=>new UdpPackageAdapter())
                .SetRemoteIPHost(new IPHost("127.0.0.1:7790"))
                .SetBindIPHost(new IPHost("127.0.0.1:7791")))
                .Start();

            for (int i = 0; i < count; i++)
            {
                udpSession.Send(BitConverter.GetBytes(i));
            }

            Thread.Sleep(1000);
            Assert.Equal(count, revCount);

            revCount = 0;

            for (int i = 0; i < count; i++)
            {
                udpSession.SendAsync(BitConverter.GetBytes(i));
            }
            Thread.Sleep(1000);
            Assert.Equal(count, revCount);
        }

        [Fact]
        public void ShouldBigDataCanSendAndReceive()
        {
            UdpSession udpSession = new UdpSession();
            int revCount = 0;
            udpSession.Received += (endpoint, byteBlock, requestInfo) =>
            {
                revCount++;
            };

            udpSession.Setup(new TouchSocketConfig()//加载配置
                .SetRemoteIPHost(new IPHost("127.0.0.1:7790"))
                .SetBindIPHost(new IPHost("127.0.0.1:7791"))
                .SetBufferLength(1024 * 1024)
                .SetUdpDataHandlingAdapter(()=>new UdpPackageAdapter()))
                .Start();

            for (int i = 0; i < 10; i++)
            {
                udpSession.Send(new byte[1024 * 512]);
            }

            Thread.Sleep(1000);
            Assert.Equal(10, revCount);
        }
    }
}