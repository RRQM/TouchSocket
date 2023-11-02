using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Sockets;

namespace DmtpChannelConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var service = GetTcpDmtpService();
            var client = GetTcpDmtpClient();

            var count = 1024 * 20;//测试20Gb数据

            using (var channel = client.CreateChannel())
            {
                ConsoleLogger.Default.Info($"通道创建成功，即将写入{count}Mb数据");
                var bytes = new byte[1024 * 1024];
                for (var i = 0; i < count; i++)
                {
                    channel.Write(bytes);
                }
                channel.Complete();
                ConsoleLogger.Default.Info("通道写入结束");
            }

            while (true)
            {
                BytePool.Default.Clear();
                GC.Collect();
                Console.ReadKey();
            }

        }

        private static TcpDmtpClient GetTcpDmtpClient()
        {
            var client = new TouchSocketConfig()
                   .SetRemoteIPHost("127.0.0.1:7789")
                   .SetDmtpOption(new DmtpOption()
                   {
                       VerifyToken = "File"
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
                   .BuildWithTcpDmtpClient();

            client.Logger.Info("连接成功");
            return client;
        }

        private static TcpDmtpService GetTcpDmtpService()
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
                       VerifyToken = "File"//连接验证口令。
                   });

            service.Setup(config);
            service.Start();
            service.Logger.Info("服务器成功启动");
            return service;
        }
    }

    internal class MyPlugin : PluginBase, IDmtpCreateChannelPlugin
    {
        private readonly ILog m_logger;

        public MyPlugin(ILog logger)
        {
            this.m_logger = logger;
        }
        public async Task OnCreateChannel(IDmtpActorObject client, CreateChannelEventArgs e)
        {
            if (client.TrySubscribeChannel(e.ChannelId, out var channel))
            {
                using (channel)
                {
                    long count = 0;
                    this.m_logger.Info("通道开始接收");
                    foreach (var byteBlock in channel)
                    {
                        //这里处理数据
                        count += byteBlock.Len;
                    }

                    this.m_logger.Info($"通道接收结束，状态={channel.Status}，共接收{count / (1048576.0):0.00}Mb字节");
                }
            }

            await e.InvokeNext();
        }
    }
}