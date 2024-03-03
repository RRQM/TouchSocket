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
                //添加TcpService。
                services.AddTcpService(config =>
                {
                    config.SetListenIPHosts(7789);
                });

                //添加TcpClient
                //注意，Client类的服务，在整个服务Start时，不会执行Connect。所以需要自行连接，或者配置无人值守连接
                services.AddSingletonTcpClient(config =>
                {
                    config.SetRemoteIPHost("127.0.0.1:7789");
                });

                services.AddServiceHostedService<IMyTcpService, MyTcpService>(config =>
                {
                    config.SetListenIPHosts(7790);
                });

                //添加命名管道服务
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