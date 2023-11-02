using CustomDmtpActorConsoleApp.SimpleDmtpRpc;
using System.Reflection;
using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.Rpc;
using TouchSocket.Resources;
using TouchSocket.Sockets;

namespace CustomDmtpActorConsoleApp
{
    /// <summary>
    /// 开发自定义DmtpActor。
    /// </summary>
    internal class Program
    {
        static void Main(string[] args)
        {
            var service = GetTcpDmtpService();
            var client = GetTcpDmtpClient();

            while (true)
            {
                string methodName = Console.ReadLine();
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

        private static TcpDmtpClient GetTcpDmtpClient()
        {
            var client = new TouchSocketConfig()
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
                   .BuildWithTcpDmtpClient();

            client.Logger.Info("连接成功");
            return client;
        }

        private static TcpDmtpService GetTcpDmtpService()
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

            service.Setup(config);
            service.Start();
            service.Logger.Info("服务器成功启动");
            return service;
        }

    }

    class MyServer
    {
        public void SayHello()
        {
            Console.WriteLine("Hello");
        }
    }

}