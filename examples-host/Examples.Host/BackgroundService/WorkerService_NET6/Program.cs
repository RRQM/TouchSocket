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
                    //添加Tcp服务器。默认是单例
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

                    ////添加瞬态Tcp服务器。
                    ////瞬态服务器，在host启动时，不会自己start，需要手动调用
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