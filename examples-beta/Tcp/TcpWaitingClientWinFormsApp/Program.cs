using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TcpWaitingClientWinFormsApp
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            CreateService();
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }

        static void CreateService()
        {
            var service = new TcpService();

            service.Setup(new TouchSocketConfig()//载入配置
                .SetListenIPHosts(7789)
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();
                })
                .ConfigurePlugins(a =>
                {
                    a.Add<MyPlugin1>();//此处可以添加插件
                }))
                .Start();//启动

            service.Logger.Info("服务器已启动");
        }

        class MyPlugin1 : PluginBase, ITcpReceivedPlugin
        {
            private readonly ILog m_logger;

            public MyPlugin1(ILog logger)
            {
                this.m_logger = logger;
            }
            public async Task OnTcpReceived(ITcpClientBase client, ReceivedDataEventArgs e)
            {
                this.m_logger.Info($"收到数据：{e.ByteBlock.ToString()}");
                await client.SendAsync(e.ByteBlock.ToString());
            }
        }
    }
}