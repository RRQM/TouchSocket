using TouchSocket.Dmtp;
using TouchSocket.Sockets;

namespace WorkerService_NET6
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    //���Tcp��������Ĭ���ǵ���
                    services.AddTcpService(config =>
                    {
                        config.SetListenIPHosts(7789);
                    });

                    services.AddTransientTcpClient(config =>
                    {
                        config.SetRemoteIPHost("127.0.0.1:7789");
                    });

                    services.AddTcpService<ITcpDmtpService, TcpDmtpService>(config =>
                    {
                        config.SetListenIPHosts(8848);
                    });

                    ////���˲̬Tcp��������
                    ////˲̬����������host����ʱ�������Լ�start����Ҫ�ֶ�����
                    //services.AddScopedSetupConfigObject<ITcpService, TcpService>(config =>
                    //{
                    //    config.SetListenIPHosts(7789);
                    //});
                });

            var host = builder.Build();

            host.Run();
        }
    }
}