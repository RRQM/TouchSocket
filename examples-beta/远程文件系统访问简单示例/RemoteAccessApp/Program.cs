using TouchSocket.Core;
using TouchSocket.Dmtp;
using TouchSocket.Dmtp.RemoteAccess;
using TouchSocket.Sockets;

namespace RemoteAccessApp
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.

            try
            {
                Enterprise.ForTest();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            var service = GetTcpDmtpService();
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }

        static TcpDmtpService GetTcpDmtpService()
        {
            var service = new TouchSocketConfig()//配置
                   .SetListenIPHosts(new IPHost[] { new IPHost(7789) })
                   .ConfigureContainer(a =>
                   {
                       a.AddConsoleLogger();
                   })
                   .ConfigurePlugins(a =>
                   {
                       a.UseDmtpRemoteAccess();//必须添加远程访问插件
                       a.Add<MyRemoteAccessPlugin>();
                   })
                   .SetVerifyToken("Dmtp")//连接验证口令。
                   .BuildWithTcpDmtpService();//此处build相当于new TcpDmtpService，然后Setup，然后Start。
            service.Logger.Info("服务器成功启动");
            return service;
        }

        class MyRemoteAccessPlugin : PluginBase, IDmtpRemoteAccessingPlugin<ITcpDmtpSocketClient>
        {
            Task IDmtpRemoteAccessingPlugin<ITcpDmtpSocketClient>.OnRemoteAccessing(ITcpDmtpSocketClient client, RemoteAccessingEventArgs e)
            {
                Console.WriteLine($"有客户端正在请求远程操作");
                Console.WriteLine($"类型：{e.AccessType}，模式：{e.AccessMode}");
                Console.WriteLine($"请求路径：{e.Path}");
                Console.WriteLine($"目标路径：{e.TargetPath}");

                Console.WriteLine("请输入y/n决定是否允许其操作?");

                var input = Console.ReadLine();
                if (input == "y")
                {
                    e.IsPermitOperation = true;
                }

                return e.InvokeNext();
            }
        }
    }
}