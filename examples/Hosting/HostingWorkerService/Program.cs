using TouchSocket.NamedPipe;
using TouchSocket.Sockets;

namespace HostingWorkerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args);

            builder.ConfigureServices(services =>
            {
                //���TcpService��
                services.AddTcpService(config =>
                {
                    config.SetListenIPHosts(7789);
                });

                //���TcpClient
                //ע�⣬Client��ķ�������������Startʱ������ִ��Connect��������Ҫ�������ӣ�������������ֵ������
                services.AddSingletonTcpClient(config =>
                {
                    config.SetRemoteIPHost("127.0.0.1:7789");
                });

                services.AddServiceHostedService<IMyTcpService, MyTcpService>(config =>
                {
                    config.SetListenIPHosts(7790);
                });

                //��������ܵ�����
                services.AddServiceHostedService<INamedPipeService, NamedPipeService>(config =>
                {
                    config.SetPipeName("pipe7789");
                });
            });

            var host = builder.Build();
            host.Run();
        }
    }

    internal class MyTcpService : TcpService, IMyTcpService
    {
    }

    internal interface IMyTcpService : ITcpService
    {
    }
}