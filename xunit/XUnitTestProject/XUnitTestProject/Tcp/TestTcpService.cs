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

using TouchSocket.Core;
using TouchSocket.Sockets;

namespace XUnitTestProject.Tcp
{
    public class TestTcpService : UnitBase
    {
        [Fact]
        public void AddOrRemoveShouleBeOk()
        {
            var service = new TcpService();
            service.Setup(new TouchSocketConfig());
            service.Start();//启动

            service.AddListen(new TcpListenOption()//在Service运行时，可以调用，直接添加监听
            {
                IpHost = 7791,
                Name = "server3",//名称用于区分监听
                ServiceSslOption = null,//可以针对当前监听，单独启用ssl加密
                Adapter = () => new FixedHeaderPackageAdapter(),//可以单独对当前地址监听，配置适配器
                                                                //还有其他可配置项，都是单独对当前地址有效。
            });

            foreach (var item in service.Monitors)
            {
                service.RemoveListen(item);//在Service运行时，可以调用，直接移除现有监听
            }
            service.Dispose();
        }

        [Fact]
        public async Task CheckClearPluginShouleBeOk()
        {
            string msg = null;
            var port = new Random().Next(10000, 60000);
            var service = new TcpService();
            service.Setup(new TouchSocketConfig()
                .SetListenIPHosts(port)
                .ConfigurePlugins(a =>
                {
                    a.UseCheckClear()
                    .SetCheckClearType(CheckClearType.All)
                    .SetTick(TimeSpan.FromSeconds(5));

                    a.Add(nameof(ITcpDisconnectedPlugin.OnTcpDisconnected), (DisconnectEventArgs e) =>
                    {
                        msg = e.Message;
                    });
                }));

            service.Start();//启动

            var client = new TcpClient();
            client.Connect($"127.0.0.1:{port}");

            Assert.True(client.Online);

            await Task.Delay(6000);

            Assert.False(client.Online);
            Assert.Contains("无数据", msg);
        }

        [Fact]
        public void ShouldShowProperties()
        {
            var service = new TcpService();
            Assert.Equal(ServerState.None, service.ServerState);

            //注入配置

            var config = new TouchSocketConfig();
            config.SetListenIPHosts(new IPHost[] { new IPHost($"127.0.0.1:8848"), new IPHost($"127.0.0.1:8849") })
                .SetThreadCount(1)
                .SetServerName("RRQMServer")
                .SetMaxCount(1000);

            //载入配置
            service.Setup(config);

            service.Start();
            Assert.NotNull(service);
            Assert.Equal(2, service.Monitors.Count());
            Assert.Equal("RRQMServer", service.ServerName);
            Assert.Equal(ServerState.Running, service.ServerState);
            Assert.Equal(1000, service.MaxCount);

            service.Stop();
            Assert.NotNull(service);
            Assert.Equal(ServerState.Stopped, service.ServerState);
            Assert.True(service.Monitors.Count() == 0);

            service.Start();
            Assert.NotNull(service);
            Assert.Equal(2, service.Monitors.Count());
            Assert.Equal("RRQMServer", service.ServerName);
            Assert.Equal(ServerState.Running, service.ServerState);
            Assert.Equal(1000, service.MaxCount);

            service.Dispose();
            Assert.True(service.Monitors.Count() == 0);
            Assert.Equal(ServerState.Disposed, service.ServerState);

            Assert.ThrowsAny<Exception>(() =>
            {
                service.Start();
            });
        }
    }
}