using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TcpFlowStressTestingConsoleApp
{
    internal class Program
    {
        /// <summary>
        /// Tcp流量吞吐压力测试
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var service = GetTcpService();

            var client = GetTcpClient();

            Console.WriteLine("请输入BufferLength");
            var bytes = new byte[int.Parse(Console.ReadLine())];
            //var bytes = new byte[1024*512];
            Random.Shared.NextBytes(bytes);
            while (true)
            {
                client.Send(bytes);
            }
        }

        static TcpService GetTcpService()
        {
            var counter = new ValueCounter()//计数器
            {
                Period = TimeSpan.FromSeconds(1),
                OnPeriod = v =>
                {
                    ConsoleLogger.Default.Info($"流量={v / 1048576.0:0.000}Mb");
                }
            };
            var service = new TcpService();
            service.Received = async (client, e) =>
            {
                //await Task.Delay(10);
                counter.Increment(e.ByteBlock.Length);
                await Task.CompletedTask;
            };
            service.Start(7789);
            service.Logger.Info("服务器已启动");
            return service;
        }

        static TcpClient GetTcpClient()
        {
            var tcpClient = new TcpClient();
            tcpClient.Connect("127.0.0.1:7789");
            return tcpClient;
        }

    }
}