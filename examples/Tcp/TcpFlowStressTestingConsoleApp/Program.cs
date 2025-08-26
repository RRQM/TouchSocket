// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

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
        private static async Task Main(string[] args)
        {
            var service = await GetTcpService();

            var client = await GetTcpClient();

            Console.WriteLine("请输入BufferLength");
            var bytes = new byte[int.Parse(Console.ReadLine())];
            //var bytes = new byte[1024*512];
            Random.Shared.NextBytes(bytes);
            while (true)
            {
                await client.SendAsync(bytes);
            }
        }

        private static async Task<TcpService> GetTcpService()
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

        private static async Task<TcpClient> GetTcpClient()
        {
            var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync("127.0.0.1:7789");
            return tcpClient;
        }

    }
}