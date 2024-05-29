using CustomDmtpActorConsoleApp.SimpleDmtpRpc;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Sockets;

namespace CustomDmtpActorConsoleApp
{
    /// <summary>
    /// 开发自定义DmtpActor。
    /// </summary>
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var service = await GetTcpDmtpService();
            var client = await GetTcpDmtpClient();

            while (true)
            {
                var methodName = Console.ReadLine();
                var actor = client.GetSimpleDmtpRpcActor();

                try
                {
                    actor.Invoke(methodName);
                    Console.WriteLine("调用成功");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private static async Task<TcpDmtpClient> GetTcpDmtpClient()
        {
            var client = await new TouchSocketConfig()
                   .SetRemoteIPHost("127.0.0.1:7789")
                   .SetDmtpOption(new DmtpOption()
                   {
                       VerifyToken = "File"
                   })
                   .ConfigureContainer(a =>
                   {
                       a.AddConsoleLogger();
                   })
                   .ConfigurePlugins(a =>
                   {
                       a.UseSimpleDmtpRpc();

                       a.UseDmtpHeartbeat()//使用Dmtp心跳
                       .SetTick(TimeSpan.FromSeconds(3))
                       .SetMaxFailCount(3);
                   })
                   .BuildClientAsync<TcpDmtpClient>();

            client.Logger.Info("连接成功");
            return client;
        }

        private static async Task<TcpDmtpService> GetTcpDmtpService()
        {
            var service = new TcpDmtpService();

            var config = new TouchSocketConfig()//配置
                   .SetListenIPHosts(new IPHost[] { new IPHost(7789) })
                   .ConfigureContainer(a =>
                   {
                       a.AddConsoleLogger();

                       a.AddDmtpRouteService();//添加路由策略
                   })
                   .ConfigurePlugins(a =>
                   {
                       a.UseSimpleDmtpRpc()
                       .RegisterRpc(new MyServer());
                   })
                   .SetDmtpOption(new DmtpOption()
                   {
                       VerifyToken = "File"//连接验证口令。
                   });

            await service.SetupAsync(config);
            await service.StartAsync();
            service.Logger.Info("服务器成功启动");
            return service;
        }
    }

    internal class MyServer
    {
        public void SayHello()
        {
            Console.WriteLine("Hello");
        }
    }
}