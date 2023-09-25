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
            Console.WriteLine("Hello, World!");
        }

        private static TcpDmtpClient GetTcpDmtpClient()
        {
            var client = new TouchSocketConfig()
                   .SetRemoteIPHost("127.0.0.1:7789")
                   .SetVerifyToken("File")
                   .ConfigureContainer(a =>
                   {
                       a.AddConsoleLogger();
                   })
                   .ConfigurePlugins(a =>
                   {

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

                   })
                   .SetVerifyToken("File");//连接验证口令。

            service.Setup(config)
                .Start();
            service.Logger.Info("服务器成功启动");
            return service;
        }

    }

}