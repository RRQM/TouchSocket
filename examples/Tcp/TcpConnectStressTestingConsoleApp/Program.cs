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

using System.Diagnostics;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TcpConnectStressTestingConsoleApp
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var service = await GetTcpService();

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

        private static async Task<TcpService> GetTcpService()
        {
            var service = new TcpService();
            await service.SetupAsync(new TouchSocketConfig()
                 .ConfigurePlugins(a =>
                 {
                     a.UseTcpSessionCheckClear(options =>
                     {
                         options.Tick = TimeSpan.FromSeconds(60);
                     });
                 })
                 .SetListenIPHosts(7789));
            await service.StartAsync();
            service.Logger.Info("服务器已启动");
            return service;
        }

        private static System.Net.Sockets.TcpClient GetTcpClient()
        {
            var tcpClient = new System.Net.Sockets.TcpClient();
            tcpClient.Connect("127.0.0.1", 7789);
            return tcpClient;
        }
    }
}