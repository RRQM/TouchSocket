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
using System.Threading.Channels;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TcpStressTestingConsoleApp
{
    internal class Program
    {
        public const int Count = 10;
        public const int DataLength = 2000;

        private static bool m_start = false;

        private static async Task Main(string[] args)
        {
            m_channel = Channel.CreateUnbounded<byte[]>(new UnboundedChannelOptions()
            {
                SingleReader = false,
                SingleWriter = false,
            });

            var service = await GetTcpService();

            for (var i = 0; i < 5; i++)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        while (await m_channel.Reader.WaitToReadAsync())
                        {
                            if (m_channel.Reader.TryRead(out var byteBlock))
                            {
                                foreach (var id in service.GetIds())
                                {
                                    if (service.TryGetClient(id, out var socketClient))
                                    {
                                        try
                                        {
                                            await socketClient.SendAsync(byteBlock);
                                        }
                                        catch (Exception ex)
                                        {
                                            ConsoleLogger.Default.Error(ex.Message);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }

                    ConsoleLogger.Default.Info("群发线程退出");
                });
            }


            Console.WriteLine($"即将测试1000个客户端，每个客户端广播{Count}条数据");
            var tasks = new List<Task>();
            for (var i = 0; i < 10; i++)
            {
                tasks.Add(RunGroup(100));
            }

            await Task.Delay(1000);//等一下，确定全都连接

            var sw = Stopwatch.StartNew();
            m_start = true;
            await Task.WhenAll(tasks);
            sw.Stop();
            Console.WriteLine($"测试结束，耗时：{sw.Elapsed}");

            while (true)
            {
                GC.Collect();
                Console.ReadKey();
            }

        }

        private static Channel<byte[]> m_channel;

        private static async Task<TcpService> GetTcpService()
        {
            var service = new TcpService();
            service.Received = async (client, e) =>
            {
                await m_channel.Writer.WriteAsync(e.Memory.ToArray());
            };

            await service.SetupAsync(new TouchSocketConfig()//载入配置
                 .SetListenIPHosts("tcp://127.0.0.1:7789", 7790)//同时监听两个地址
                 .ConfigureContainer(a =>//容器的配置顺序应该在最前面
                 {
                     a.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
                 })
                 .ConfigurePlugins(a =>
                 {
                 }));
            await service.StartAsync();//启动
            service.Logger.Info("服务器已启动");
            return service;
        }

        private static async Task RunGroup(int count)
        {
            await await Task.Factory.StartNew(async () =>
            {
                try
                {
                    var bytes = new byte[DataLength];
                    var clients = new List<TcpClient>();
                    for (var i = 0; i < count; i++)
                    {
                        var client = await GetTcpClient();
                        clients.Add(client);
                    }

                    while (!m_start)
                    {
                        await Task.Delay(100);
                    }
                    for (var i = 0; i < Count; i++)
                    {
                        foreach (var item in clients)
                        {
                            try
                            {
                                await item.SendAsync(bytes);
                                await Task.Delay(10);
                            }
                            catch (Exception ex)
                            {
                                ConsoleLogger.Default.Error(ex.Message);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ConsoleLogger.Default.Error(ex.Message);
                }
                finally
                {
                    ConsoleLogger.Default.Info("退出");
                }
            }, TaskCreationOptions.LongRunning);

        }

        private static async Task<TcpClient> GetTcpClient()
        {
            var tcpClient = new TcpClient();
            //载入配置
            await tcpClient.SetupAsync(new TouchSocketConfig()
                 .SetRemoteIPHost("127.0.0.1:7789")
                 .ConfigureContainer(a =>
                 {
                     a.AddConsoleLogger();//添加一个日志注入
                 })
                 .ConfigurePlugins(a =>
                 {
                 })
                 );

            await tcpClient.ConnectAsync();//调用连接，当连接不成功时，会抛出异常。
            //tcpClient.Logger.Info("客户端成功连接");
            return tcpClient;
        }

    }
}