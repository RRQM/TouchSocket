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
using TouchSocket.Core.Config;
using TouchSocket.Sockets;
using Xunit;

namespace XUnitTest.TCP
{
    public class TestTcpService
    {
        [Fact]
        public void ShouldShowProperties()
        {
            TcpService service = new TcpService();
            Assert.Equal(ServerState.None, service.ServerState);

            //注入配置

            TouchSocketConfig config = new TouchSocketConfig();
            config.SetListenIPHosts(new IPHost[] { new IPHost($"127.0.0.1:8848"), new IPHost($"127.0.0.1:8849") })
                .SetThreadCount(1)
                .SetClearInterval(300)
                .SetServerName("RRQMServer")
                .SetMaxCount(1000)
                .SetBufferLength(1024);

            //载入配置
            service.Setup(config);

            service.Start();
            Assert.NotNull(service);
            Assert.Equal(2, service.Monitors.Length);
            Assert.Equal("RRQMServer", service.ServerName);
            Assert.Equal(ServerState.Running, service.ServerState);
            Assert.Equal(1000, service.MaxCount);
            Assert.Equal(300, service.ClearInterval);

            service.Stop();
            Assert.NotNull(service);
            Assert.Equal(ServerState.Stopped, service.ServerState);
            Assert.Null(service.Monitors);

            service.Start();
            Assert.NotNull(service);
            Assert.Equal(2, service.Monitors.Length);
            Assert.Equal("RRQMServer", service.ServerName);
            Assert.Equal(ServerState.Running, service.ServerState);
            Assert.Equal(1000, service.MaxCount);
            Assert.Equal(300, service.ClearInterval);

            service.Dispose();
            Assert.Null(service.Monitors);
            Assert.Equal(ServerState.Disposed, service.ServerState);

            Assert.ThrowsAny<Exception>(() =>
            {
                service.Start();
            });
        }
    }
}