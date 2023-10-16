using System.Diagnostics;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TcpConnectStressTestingConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var service = GetTcpService();

            new SingleTimer(1000, () =>
            {
                ConsoleLogger.Default.Info($"客户端数量：{service.Count}");
            });
            var clients = new List<System.Net.Sockets.TcpClient>();
            while (true)
            {
                Console.WriteLine("按任意键建立1000连接");
                Console.ReadKey();

                var sw = Stopwatch.StartNew();
                for (var i = 0; i < 1000; i++)
                {
                    var client = GetTcpClient();
                    clients.Add(client);//长久引用，以免被GC
                }
                sw.Stop();
                ConsoleLogger.Default.Info($"用时：{sw.Elapsed}");
            }
        }

        static TcpService GetTcpService()
        {
            var service = new TcpService();
            service.Setup(new TouchSocketConfig()
                .SetListenIPHosts(7789));
            service.Start();
            service.Logger.Info("服务器已启动");
            return service;
        }

        static System.Net.Sockets.TcpClient GetTcpClient()
        {
            System.Net.Sockets.TcpClient tcpClient = new System.Net.Sockets.TcpClient();
            tcpClient.Connect("127.0.0.1", 7789);
            return tcpClient;
        }
    }
}