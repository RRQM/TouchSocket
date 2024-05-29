using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Sockets;

namespace DmtpChannelConsoleApp
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var service = await GetTcpDmtpService();
            var client = await GetTcpDmtpClient();

            var consoleAction = new ConsoleAction();

            consoleAction.Add("1", "测试完成写入", async () => { await RunComplete(client); });
            consoleAction.Add("2", "测试Hold", async () => { await RunHoldOn(client); });

            consoleAction.ShowAll();

            await consoleAction.RunCommandLineAsync();
        }

        private static async Task RunHoldOn(IDmtpActorObject client)
        {
            //HoldOn的使用，主要是解决同一个通道中，多个数据流传输的情况。

            //1.创建通道，同时支持通道路由和元数据传递
            using (var channel = client.CreateChannel())
            {
                //设置限速
                //channel.MaxSpeed = 1024 * 1024;

                ConsoleLogger.Default.Info($"通道创建成功，即将写入");
                var bytes = new byte[1024];

                for (var i = 0; i < 100; i++)//循环100次
                {
                    for (var j = 0; j < 10; j++)
                    {
                        //2.持续写入数据
                        await channel.WriteAsync(bytes);
                    }
                    //3.在某个阶段完成数据传输时，可以调用HoldOn
                    await channel.HoldOnAsync("等一下下");
                }
                //4.在写入完成后调用终止指令。例如：Complete、Cancel、HoldOn、Dispose等
                await channel.CompleteAsync("我完成了");

                ConsoleLogger.Default.Info("通道写入结束");
            }
        }

        private static async Task RunComplete(IDmtpActorObject client)
        {
            var count = 1024 * 1;//测试1Gb数据

            //1.创建通道，同时支持通道路由和元数据传递
            using (var channel = client.CreateChannel())
            {
                //设置限速
                //channel.MaxSpeed = 1024 * 1024;

                ConsoleLogger.Default.Info($"通道创建成功，即将写入{count}Mb数据");
                var bytes = new byte[1024 * 1024];
                for (var i = 0; i < count; i++)
                {
                    //2.持续写入数据
                    await channel.WriteAsync(bytes);
                }

                //3.在写入完成后调用终止指令。例如：Complete、Cancel、HoldOn、Dispose等
                await channel.CompleteAsync("我完成了");
                ConsoleLogger.Default.Info("通道写入结束");
            }
        }

        private static async Task<TcpDmtpClient> GetTcpDmtpClient()
        {
            var client = await new TouchSocketConfig()
                   .SetRemoteIPHost("127.0.0.1:7789")
                   .SetDmtpOption(new DmtpOption()
                   {
                       VerifyToken = "Channel"
                   })
                   .SetSendTimeout(0)
                   .ConfigureContainer(a =>
                   {
                       a.AddConsoleLogger();
                   })
                   .ConfigurePlugins(a =>
                   {
                       a.Add<MyPlugin>();
                   })
                   .BuildClientAsync<TcpDmtpClient>();

            client.Logger.Info("连接成功");
            return client;
        }

        private static async Task<TcpDmtpService> GetTcpDmtpService()
        {
            var service = new TcpDmtpService();

            var config = new TouchSocketConfig()//配置
                   .SetListenIPHosts(7789)
                   .SetSendTimeout(0)
                   .ConfigureContainer(a =>
                   {
                       a.AddConsoleLogger();
                   })
                   .ConfigurePlugins(a =>
                   {
                       a.Add<MyPlugin>();
                   })
                   .SetDmtpOption(new DmtpOption()
                   {
                       VerifyToken = "Channel"//连接验证口令。
                   });

            await service.SetupAsync(config);
            await service.StartAsync();
            service.Logger.Info("服务器成功启动");
            return service;
        }
    }

    internal class MyPlugin : PluginBase, IDmtpCreatedChannelPlugin
    {
        private readonly ILog m_logger;

        public MyPlugin(ILog logger)
        {
            this.m_logger = logger;
        }

        public async Task OnDmtpCreatedChannel(IDmtpActorObject client, CreateChannelEventArgs e)
        {
            if (client.TrySubscribeChannel(e.ChannelId, out var channel))
            {
                //接收端也可以限速
                //channel.MaxSpeed = 1024 * 1024;

                //设定读取超时时间
                //channel.Timeout = TimeSpan.FromSeconds(30);
                using (channel)
                {
                    this.m_logger.Info("通道开始接收");

                    //此判断主要是探测是否有Hold操作
                    while (channel.CanMoveNext)
                    {
                        long count = 0;
                        foreach (var byteBlock in channel)
                        {
                            //这里处理数据
                            count += byteBlock.Length;
                            this.m_logger.Info($"通道已接收：{count}字节");
                        }

                        this.m_logger.Info($"通道接收结束，状态={channel.Status}，短语={channel.LastOperationMes}，共接收{count / (1048576.0):0.00}Mb字节");
                    }
                }
            }

            await e.InvokeNext();
        }
    }
}