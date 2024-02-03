using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.NamedPipe;
using TouchSocket.Sockets;

namespace NamedPipeDmtpConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Enterprise.ForTest();
            }
            catch (Exception ex)
            {
                ConsoleLogger.Default.Info(ex.Message);
            }
            var service = GetService();
            var client = GetClient();

            Console.ReadKey();
        }

        static NamedPipeDmtpClient GetClient()
        {
            var client = new NamedPipeDmtpClient();
            client.Setup(new TouchSocketConfig()
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();
                })
                .SetPipeName("TouchSocketPipe")//设置管道名称
                .SetDmtpOption(new DmtpOption()
                {
                    VerifyToken = "Dmtp"
                }));
            client.Connect();

            client.Logger.Info("连接成功");
            return client;
        }

        static NamedPipeDmtpService GetService()
        {
            var service = new NamedPipeDmtpService();
            var config = new TouchSocketConfig()//配置
                   .SetPipeName("TouchSocketPipe")//设置管道名称
                   .ConfigureContainer(a =>
                   {
                       a.AddConsoleLogger();
                   })
                   .SetDmtpOption(new DmtpOption()
                   {
                       VerifyToken = "Dmtp"//设定连接口令，作用类似账号密码
                   });

            service.Setup(config);

            service.Start();

            service.Logger.Info($"{service.GetType().Name}已启动");

            return service;
        }
    }
}
