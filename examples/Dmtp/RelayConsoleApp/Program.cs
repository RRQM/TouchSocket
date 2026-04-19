using System.Net;
using System.Text;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Relay;
using TouchSocket.Sockets;

namespace RelayConsoleApp
{
    internal class Program
    {
        // RelayId 持久化路径（生产中可换成数据库或配置文件）
        private const string RelayIdFilePath = "relay-id.txt";

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
            Console.WriteLine("1. TCP穿透完整演示");
            Console.WriteLine("2. UDP穿透完整演示");
            Console.WriteLine("请选择操作（默认1）：");

            var choice = Console.ReadLine();
            switch (choice)
            {
                case "2":
                    await RunFullUdpDemoAsync();
                    break;
                default:
                    await RunFullTcpDemoAsync();
                    break;
            }

            Console.ReadLine();
        }

        #region Relay启动中继服务器
        /// <summary>
        /// 启动中继服务器（同时支持TCP和UDP穿透）。
        /// 服务端通过 <see cref="IDmtpRelayRegisteringPlugin"/> 插件对注册请求进行鉴权，
        /// 通过 <see cref="IDmtpRelayUnregisteringPlugin"/> 插件监听注销事件。
        /// </summary>
        static async Task<TcpDmtpService> StartRelayServerAsync(int port = 7789)
        {
            var service = new TcpDmtpService();
            await service.SetupAsync(new TouchSocketConfig()
                .SetListenIPHosts(port)
                .SetDmtpOption(option =>
                {
                    option.VerifyToken = "Dmtp";
                })
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();
                    // 必须注册中继服务依赖
                    a.AddDmtpRelayService();
                })
                .ConfigurePlugins(a =>
                {
                    // 启用内网穿透功能
                    a.UseDmtpRelay();

                    // 注册事件：客户端请求注册中继端口时触发（服务端鉴权）
                    a.AddDmtpRelayRegisteringPlugin(async (actor, e) =>
                    {
                        // 从元数据中读取鉴权令牌
                        var secret = e.Metadata?["relay-secret"];
                        if (secret != "relay@2025")
                        {
                            e.IsPermitOperation = false;
                            e.Message = "鉴权失败，请提供正确的 relay-secret";
                            await e.InvokeNext();
                            return;
                        }

                        e.IsPermitOperation = true;
                        // 可在此对 e.Config 进行额外配置，例如 SSL、日志等
                        actor.DmtpActor.Logger?.Info($"客户端 [{actor.DmtpActor.Id}] 注册中继端口：{e.RemoteIPHost} -> {e.LocalIPHost}，类型：{e.RelayType}");
                        await e.InvokeNext();
                    });

                    // 注册事件：中继注册被取消时触发（主动注销或客户端断线）
                    a.AddDmtpRelayUnregisteringPlugin(async (actor, e) =>
                    {
                        actor.DmtpActor.Logger?.Info($"中继 [{e.RelayId}] 取消注册，地址：{e.RemoteIPHost}，原因：{e.Message}");
                        await e.InvokeNext();
                    });
                }));

            await service.StartAsync();
            service.Logger.Info($"中继服务器已启动，监听端口 {port}");
            return service;
        }
        #endregion

        #region Relay注册TCP穿透
        /// <summary>
        /// 连接中继服务器并注册TCP穿透。
        /// 注册成功后将 RelayId 持久化，以便进程崩溃重启后能够预注销残留注册。
        /// </summary>
        static async Task<(TcpDmtpClient Client, ITcpRelayRegistration Registration)> RegisterTcpRelayAsync(
            string serverHost = "127.0.0.1:7789",
            string localIPHost = "127.0.0.1:8848",
            string remoteIPHost = "0.0.0.0:8889")
        {
            var client = new TcpDmtpClient();
            await client.SetupAsync(new TouchSocketConfig()
                .SetRemoteIPHost(serverHost)
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
                }));

            await client.ConnectAsync();
            client.Logger.Info($"已连接到中继服务器，客户端 ID: {client.Id}");

            var actor = client.GetDmtpRelayActor();

            // 崩溃恢复：在注册前先尝试预注销上次遗留的注册
            await TryPreUnregisterAsync(actor);

            // 构造携带鉴权信息的元数据
            var metadata = new Metadata
            {
                ["relay-secret"] = "relay@2025"
            };

            // 注册TCP穿透：localIPHost 是内网服务地址，remoteIPHost 是服务器暴露的公网端口
            var registration = await actor.RegisterTcpRelayAsync(localIPHost, remoteIPHost, metadata);

            // 持久化 RelayId，供下次启动时做崩溃恢复预注销
            SaveRelayId(registration.RelayId);

            client.Logger.Info($"TCP穿透注册成功：{localIPHost} <- 服务器:{remoteIPHost}");
            client.Logger.Info($"RelayId: {registration.RelayId}，当前连接数: {registration.ConnectionCount}");

            return (client, registration);
        }
        #endregion

        #region Relay注册UDP穿透
        /// <summary>
        /// 连接中继服务器并注册UDP穿透。
        /// 注册成功后同样将 RelayId 持久化，以便崩溃重启后预注销残留注册。
        /// </summary>
        static async Task<(TcpDmtpClient Client, IUdpRelayRegistration Registration)> RegisterUdpRelayAsync(
            string serverHost = "127.0.0.1:7789",
            string localIPHost = "127.0.0.1:9000",
            string remoteIPHost = "0.0.0.0:9001")
        {
            var client = new TcpDmtpClient();
            await client.SetupAsync(new TouchSocketConfig()
                .SetRemoteIPHost(serverHost)
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
                }));

            await client.ConnectAsync();
            client.Logger.Info($"已连接到中继服务器，客户端 ID: {client.Id}");

            var actor = client.GetDmtpRelayActor();

            // 崩溃恢复：在注册前先尝试预注销上次遗留的注册
            await TryPreUnregisterAsync(actor);

            var metadata = new Metadata
            {
                ["relay-secret"] = "relay@2025"
            };

            // 注册UDP穿透：localIPHost 是内网 UDP 服务地址，remoteIPHost 是服务器暴露的公网UDP端口
            var registration = await actor.RegisterUdpRelayAsync(localIPHost, remoteIPHost, metadata);

            // 持久化 RelayId
            SaveRelayId(registration.RelayId);

            client.Logger.Info($"UDP穿透注册成功：{localIPHost} <- 服务器:{remoteIPHost}");
            client.Logger.Info($"RelayId: {registration.RelayId}");

            return (client, registration);
        }
        #endregion

        #region Relay崩溃恢复预注销
        /// <summary>
        /// 持久化 RelayId 到本地文件。
        /// 生产环境中可替换为数据库、Redis 等持久化方案。
        /// </summary>
        static void SaveRelayId(Guid relayId)
        {
            File.WriteAllText(RelayIdFilePath, relayId.ToString());
        }

        /// <summary>
        /// 从本地文件加载上次持久化的 RelayId。
        /// 若文件不存在或内容无效，返回 <see cref="Guid.Empty"/>。
        /// </summary>
        static Guid LoadRelayId()
        {
            if (!File.Exists(RelayIdFilePath))
            {
                return Guid.Empty;
            }
            var text = File.ReadAllText(RelayIdFilePath).Trim();
            return Guid.TryParse(text, out var id) ? id : Guid.Empty;
        }

        /// <summary>
        /// 崩溃恢复预注销：在发起新注册之前，尝试清除服务端可能残留的旧注册。
        /// 当进程因崩溃而未能正常注销时，服务端仍然持有旧的端口监听；
        /// 重启后重新注册同一端口前必须先调用此方法，否则注册会失败（端口已占用）。
        /// 失败时静默忽略（例如服务端已自行清理），不影响后续注册流程。
        /// </summary>
        static async Task TryPreUnregisterAsync(IDmtpRelayActor actor)
        {
            var lastRelayId = LoadRelayId();
            if (lastRelayId == Guid.Empty)
            {
                return;
            }

            try
            {
                var result = await actor.UnregisterRelayAsync(lastRelayId, "崩溃恢复预注销");
                if (result.IsSuccess)
                {
                    Console.WriteLine($"预注销成功，已清除服务端残留注册 RelayId={lastRelayId}");
                }
                else
                {
                    // 服务端可能已自行清理（连接超时等），静默忽略
                    Console.WriteLine($"预注销返回：{result.Message}（可忽略）");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"预注销异常（可忽略）：{ex.Message}");
            }
            // 无论预注销是否成功，此处不清除持久化的 RelayId；
            // 后续成功新注册后会以新 RelayId 覆盖（幂等）。
        }
        #endregion

        #region Relay启动测试服务器
        /// <summary>
        /// 启动一个简单的TCP回显服务器（模拟内网服务）
        /// </summary>
        static async Task<TcpService> StartTestTcpServiceAsync(int port = 8848)
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
                    a.Add<EchoTcpPlugin>();
                }));

            await service.StartAsync();
            service.Logger.Info($"内网TCP服务已启动，端口 {port}");
            return service;
        }

        class EchoTcpPlugin : PluginBase, ITcpReceivedPlugin
        {
            public async Task OnTcpReceived(ITcpSession client, ReceivedDataEventArgs e)
            {
                var message = Encoding.UTF8.GetString(e.Memory.Span);
                client.Logger.Info($"[TCP服务] 收到：{message}");
                if (client is IClientSender sender)
                {
                    await sender.SendAsync(Encoding.UTF8.GetBytes($"Echo: {message}"));
                }
                await e.InvokeNext();
            }
        }
        #endregion

        #region Relay测试TCP连接
        /// <summary>
        /// 外部客户端通过中继服务器的公网端口访问内网TCP服务
        /// </summary>
        static async Task TestTcpConnectionAsync(string relayHost = "127.0.0.1:8889")
        {
            var client = new TcpClient();
            await client.SetupAsync(new TouchSocketConfig()
                .SetRemoteIPHost(relayHost)
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();
                })
                .ConfigurePlugins(a =>
                {
                    a.Add<DisplayReplyPlugin>();
                }));

            await client.ConnectAsync();
            client.Logger.Info("外部客户端已通过中继连接到内网服务");

            await client.SendAsync(Encoding.UTF8.GetBytes("Hello via TCP Relay!"));
            await Task.Delay(500);
        }

        class DisplayReplyPlugin : PluginBase, ITcpReceivedPlugin
        {
            public async Task OnTcpReceived(ITcpSession client, ReceivedDataEventArgs e)
            {
                var reply = Encoding.UTF8.GetString(e.Memory.Span);
                client.Logger.Info($"[外部客户端] 收到回复：{reply}");
                await e.InvokeNext();
            }
        }
        #endregion

        #region Relay测试UDP连接
        /// <summary>
        /// 外部客户端通过中继服务器的公网UDP端口向内网UDP服务发送数据
        /// </summary>
        static async Task TestUdpConnectionAsync(string relayUdpHost = "127.0.0.1:9001", string innerUdpHost = "127.0.0.1:9000")
        {
            // 启动一个简单的内网 UDP 服务（回显）
            var innerUdp = new UdpSession();
            await innerUdp.SetupAsync(new TouchSocketConfig()
                .SetBindIPHost(new IPHost(innerUdpHost))
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();
                })
                .ConfigurePlugins(a =>
                {
                    a.Add<EchoUdpPlugin>();
                }));
            await innerUdp.StartAsync();
            innerUdp.Logger.Info($"内网UDP服务已启动，监听 {innerUdpHost}");

            // 外部客户端：向中继服务器的公网UDP端口发送数据
            var externalUdp = new UdpSession();
            await externalUdp.SetupAsync(new TouchSocketConfig()
                .SetBindIPHost(new IPHost("127.0.0.1:0"))
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();
                })
                .ConfigurePlugins(a =>
                {
                    a.Add<DisplayUdpReplyPlugin>();
                }));
            await externalUdp.StartAsync();

            var relayEndPoint = new IPHost(relayUdpHost).EndPoint;
            var data = Encoding.UTF8.GetBytes("Hello via UDP Relay!");
            await externalUdp.SendAsync(relayEndPoint, data);
            externalUdp.Logger.Info($"外部UDP客户端已向 {relayUdpHost} 发送数据");

            await Task.Delay(1000);

            await externalUdp.StopAsync();
            await innerUdp.StopAsync();
        }

        class EchoUdpPlugin : PluginBase, IUdpReceivedPlugin
        {
            public async Task OnUdpReceived(IUdpSessionBase client, UdpReceivedDataEventArgs e)
            {
                var message = Encoding.UTF8.GetString(e.Memory.Span);
                client.Logger.Info($"[UDP服务] 收到：{message}，来源：{e.EndPoint}");
                if (client is IUdpClientSender udpSender)
                {
                    await udpSender.SendAsync(e.EndPoint, Encoding.UTF8.GetBytes($"UdpEcho: {message}"));
                }
                await e.InvokeNext();
            }
        }

        class DisplayUdpReplyPlugin : PluginBase, IUdpReceivedPlugin
        {
            public async Task OnUdpReceived(IUdpSessionBase client, UdpReceivedDataEventArgs e)
            {
                var reply = Encoding.UTF8.GetString(e.Memory.Span);
                client.Logger.Info($"[外部UDP客户端] 收到回复：{reply}");
                await e.InvokeNext();
            }
        }
        #endregion

        #region Relay完整TCP示例
        /// <summary>
        /// TCP穿透完整演示：
        /// 中继服务器 7789 → 内网客户端注册 → 内网服务 8848 ← 外部客户端访问 8889
        /// </summary>
        static async Task RunFullTcpDemoAsync()
        {
            Console.WriteLine("=== TCP内网穿透完整演示 ===");

            // 1. 启动中继服务器
            var relayServer = await StartRelayServerAsync(7789);
            await Task.Delay(300);

            // 2. 启动内网TCP服务（模拟被穿透的服务）
            var innerService = await StartTestTcpServiceAsync(8848);
            await Task.Delay(300);

            // 3. 内网客户端连接中继服务器并注册TCP穿透
            //    8889 是暴露在服务器上的公网端口，8848 是内网实际服务端口
            var (relayClient, registration) = await RegisterTcpRelayAsync(
                serverHost: "127.0.0.1:7789",
                localIPHost: "127.0.0.1:8848",
                remoteIPHost: "0.0.0.0:8889");
            await Task.Delay(300);

            // 4. 外部客户端通过中继公网端口访问内网服务
            Console.WriteLine("\n--- 外部客户端发起连接 ---");
            await TestTcpConnectionAsync("127.0.0.1:8889");

            Console.WriteLine($"\n当前活跃连接数：{registration.ConnectionCount}");
            Console.WriteLine("=== TCP演示完成，按任意键退出 ===");
        }
        #endregion

        #region Relay完整UDP示例
        /// <summary>
        /// UDP穿透完整演示：
        /// 中继服务器 7789 → 内网客户端注册 → 内网UDP服务 9000 ← 外部UDP客户端访问 9001
        /// </summary>
        static async Task RunFullUdpDemoAsync()
        {
            Console.WriteLine("=== UDP内网穿透完整演示 ===");

            // 1. 启动中继服务器
            var relayServer = await StartRelayServerAsync(7789);
            await Task.Delay(300);

            // 2. 内网客户端注册UDP穿透
            //    9001 是暴露在服务器上的公网UDP端口，9000 是内网UDP服务端口
            var (relayClient, registration) = await RegisterUdpRelayAsync(
                serverHost: "127.0.0.1:7789",
                localIPHost: "127.0.0.1:9000",
                remoteIPHost: "0.0.0.0:9001");
            await Task.Delay(300);

            // 3. 外部UDP客户端通过中继公网端口向内网UDP服务发送数据
            Console.WriteLine("\n--- 外部UDP客户端发起通信 ---");
            await TestUdpConnectionAsync("127.0.0.1:9001", "127.0.0.1:9000");

            Console.WriteLine("=== UDP演示完成，按任意键退出 ===");
        }
        #endregion
    }
}
