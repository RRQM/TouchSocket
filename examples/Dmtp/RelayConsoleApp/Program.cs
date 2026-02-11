using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Relay;
using TouchSocket.Sockets;

namespace RelayConsoleApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Enterprise.ForTest();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine("Dmtp内网穿透示例");
            Console.WriteLine("1. 启动中继服务器");
            Console.WriteLine("2. 启动内网客户端");
            Console.WriteLine("3. 启动测试服务器");
            Console.WriteLine("4. 测试连接");
            Console.WriteLine("请选择操作：");

            var choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    await StartRelayServerAsync();
                    break;
                case "2":
                    await StartRelayClientAsync();
                    break;
                case "3":
                    await StartTestServiceAsync();
                    break;
                case "4":
                    await TestConnectionAsync();
                    break;
                default:
                    await RunFullDemoAsync();
                    break;
            }

            Console.ReadLine();
        }

        #region Relay启动中继服务器
        /// <summary>
        /// 启动中继服务器
        /// </summary>
        static async Task<TcpDmtpService> StartRelayServerAsync(int port = 7789)
        {
            var service = new TcpDmtpService();
            var config = new TouchSocketConfig();
            config.SetListenIPHosts(port)
                  .SetDmtpOption(option =>
                  {
                      option.VerifyToken = "Dmtp";
                  })
                  .ConfigureContainer(a =>
                  {
                      a.AddConsoleLogger();
                      a.AddDmtpRelayService();
                  })
                  .ConfigurePlugins(a =>
                  {
                      a.UseDmtpRelay();

                      a.AddDmtpRelayRegisteringPlugin(async (actor, e) =>
                      {
                          // 这里可以添加验证逻辑，例如根据actor.Id判断是否允许注册
                          e.IsPermitOperation = true;

                          //此处可以配置tcp打开的一些配置
                          //也可以配置ssl或者日志等
                          var socketConfig = e.Config;
                          await e.InvokeNext();
                      });
                  });

            await service.SetupAsync(config);
            await service.StartAsync();

            service.Logger.Info($"中继服务器已启动，监听端口 {port}");
            return service;
        }
        #endregion

        #region Relay启动内网客户端
        /// <summary>
        /// 启动内网客户端并注册端口映射
        /// </summary>
        static async Task<TcpDmtpClient> StartRelayClientAsync(string serverHost = "127.0.0.1:7789", int localPort = 8848, int remotePort = 8889)
        {
            var client = new TcpDmtpClient();
            var config = new TouchSocketConfig();
            config.SetRemoteIPHost(serverHost)
                  .SetDmtpOption(option =>
                  {
                      option.VerifyToken = "Dmtp";
                  })
                  .ConfigureContainer(a =>
                  {
                      a.AddConsoleLogger();
                  })
                  .ConfigurePlugins(a =>
                  {
                      a.UseDmtpRelay();
                  });

            await client.SetupAsync(config);
            await client.ConnectAsync();

            client.Logger.Info($"内网客户端已连接到服务器，ID: {client.Id}");

            var relayConfig = new RelayClientOption
            {
                LocalIPHost = $"127.0.0.1:{localPort}",
                RemoteIPHost = $"0.0.0.0:{remotePort}"
            };

            var relayActor = client.GetDmtpRelayActor();

            try
            {
                var relayClient = await relayActor.CreateRelayClientAsync(relayConfig, CancellationToken.None);
                client.Logger.Info($"端口映射成功：本地 {localPort} -> 服务器 {remotePort}");
                client.Logger.Info($"当前连接数：{relayClient.ConnectionCount}");
            }
            catch (Exception ex)
            {
                client.Logger.Error($"添加中继客户端失败：{ex.Message}");
            }

            return client;
        }
        #endregion

        #region Relay启动测试服务器
        /// <summary>
        /// 启动一个简单的TCP测试服务器（模拟内网服务）
        /// </summary>
        static async Task<TcpService> StartTestServiceAsync(int port = 8848)
        {
            var service = new TcpService();
            await service.SetupAsync(new TouchSocketConfig()
                .SetListenIPHosts(port)
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();
                })
                .ConfigurePlugins(a =>
                {
                    a.Add<MyTcpPlugin>();
                }));

            await service.StartAsync();

            service.Logger.Info($"测试服务器已启动，监听端口 {port}");
            return service;
        }

        class MyTcpPlugin : PluginBase, ITcpReceivedPlugin
        {
            public async Task OnTcpReceived(ITcpSession client, ReceivedDataEventArgs e)
            {
                var message = e.Memory.Span.ToString(System.Text.Encoding.UTF8);
                client.Logger.Info($"收到消息：{message}");

                // 回显消息

                if (client is IClientSender clientSender)
                {
                    await clientSender.SendAsync($"Echo: {message}");
                }

                await e.InvokeNext();
            }
        }
        #endregion

        #region Relay测试连接
        /// <summary>
        /// 测试通过中继服务器连接到内网服务
        /// </summary>
        static async Task TestConnectionAsync(string relayHost = "127.0.0.1:8889")
        {
            var client = new TcpClient();
            await client.SetupAsync(new TouchSocketConfig()
                .SetRemoteIPHost(relayHost)
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();
                }));

            await client.ConnectAsync();

            client.Logger.Info($"已通过中继服务器连接");

            // 发送测试消息
            await client.SendAsync("Hello from relay client!");

            await Task.Delay(1000);
        }
        #endregion

        #region Relay完整示例
        /// <summary>
        /// 运行完整示例
        /// </summary>
        static async Task RunFullDemoAsync()
        {
            Console.WriteLine("=== 启动完整Dmtp内网穿透示例 ===");

            // 1. 启动中继服务器
            var relayServer = await StartRelayServerAsync(7789);

            await Task.Delay(1000);

            // 2. 启动内网测试服务器
            var testService = await StartTestServiceAsync(8848);

            await Task.Delay(1000);

            // 3. 启动内网客户端并注册端口映射
            var relayClient = await StartRelayClientAsync("127.0.0.1:7789", 8848, 8889);

            await Task.Delay(1000);

            // 4. 测试连接
            Console.WriteLine("\n=== 开始测试连接 ===");
            await TestConnectionAsync("127.0.0.1:8889");

            Console.WriteLine("\n=== 示例运行完成 ===");
            Console.WriteLine("按任意键退出...");
        }
        #endregion
    }
}
