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
        private static void Main()
        {
            CreateService();
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }

        private static void CreateService()
        {
            var service = new TcpService();

            service.SetupAsync(new TouchSocketConfig()//��������
                .SetListenIPHosts(7789)
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();
                })
                .ConfigurePlugins(a =>
                {
                    a.Add<MyPlugin1>();//�˴�������Ӳ��
                }));
            service.StartAsync();//����

            service.Logger.Info("������������");
        }

        private class MyPlugin1 : PluginBase, ITcpReceivedPlugin
        {
            private readonly ILog m_logger;

            public MyPlugin1(ILog logger)
            {
                this.m_logger = logger;
            }

            public async Task OnTcpReceived(ITcpSession client, ReceivedDataEventArgs e)
            {
                this.m_logger.Info($"�յ����ݣ�{e.ByteBlock.ToString()}");

                if (client is ITcpSessionClient sessionClient)
                {
                    await sessionClient.SendAsync(e.ByteBlock.Memory);
                }
            }
        }
    }
}