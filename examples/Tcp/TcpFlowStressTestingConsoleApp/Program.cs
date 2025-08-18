using System.Threading.Tasks;
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
        static async Task Main(string[] args)
        {
            var service =await GetTcpService();

            var client =await GetTcpClient();

            Console.WriteLine("请输入BufferLength");
            var bytes = new byte[int.Parse(Console.ReadLine())];
            //var bytes = new byte[1024*512];
            Random.Shared.NextBytes(bytes);
            while (true)
            {
                await client.SendAsync(bytes);
            }
        }

        static async Task<TcpService> GetTcpService()
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
                counter.Increment(e.Memory.Length);
                await Task.CompletedTask;
            };
            await service.StartAsync(7789);
            service.Logger.Info("服务器已启动");
            return service;
        }

        static async Task<TcpClient> GetTcpClient()
        {
            var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync("127.0.0.1:7789");
            return tcpClient;
        }

    }
}