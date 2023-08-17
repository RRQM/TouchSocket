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
        private static void Main()
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

        private static TcpDmtpService GetTcpDmtpService()
        {
            var service = new TouchSocketConfig()//����
                   .SetListenIPHosts(new IPHost[] { new IPHost(7789) })
                   .ConfigureContainer(a =>
                   {
                       a.AddConsoleLogger();
                   })
                   .ConfigurePlugins(a =>
                   {
                       a.UseDmtpRemoteAccess();//�������Զ�̷��ʲ��
                       a.Add<MyRemoteAccessPlugin>();
                   })
                   .SetVerifyToken("Dmtp")//������֤���
                   .BuildWithTcpDmtpService();//�˴�build�൱��new TcpDmtpService��Ȼ��Setup��Ȼ��Start��
            service.Logger.Info("�������ɹ�����");
            return service;
        }

        private class MyRemoteAccessPlugin : PluginBase, IDmtpRemoteAccessingPlugin<ITcpDmtpSocketClient>
        {
            Task IDmtpRemoteAccessingPlugin<ITcpDmtpSocketClient>.OnRemoteAccessing(ITcpDmtpSocketClient client, RemoteAccessingEventArgs e)
            {
                Console.WriteLine($"�пͻ�����������Զ�̲���");
                Console.WriteLine($"���ͣ�{e.AccessType}��ģʽ��{e.AccessMode}");
                Console.WriteLine($"����·����{e.Path}");
                Console.WriteLine($"Ŀ��·����{e.TargetPath}");

                Console.WriteLine("������y/n�����Ƿ����������?");

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