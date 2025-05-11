using System.Diagnostics;
using System.Threading.Channels;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TcpStressTestingConsoleApp
{
    internal class Program
    {
        public const int Count = 10;
        public const int DataLength = 2000;

        private static bool m_start = false;

        static async Task Main(string[] args)
        {
            m_channel = Channel.CreateUnbounded<ByteBlock>(new UnboundedChannelOptions()
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
                                            await socketClient.SendAsync(byteBlock.Memory);
                                        }
                                        catch (Exception ex)
                                        {
                                            ConsoleLogger.Default.Error(ex.Message);
                                        }
                                    }
                                }

                                byteBlock.SetHolding(false);
                            }
                        }
                    }
                    catch (Exception ex)
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

        static Channel<ByteBlock> m_channel;

        static async Task<TcpService> GetTcpService()
        {
            var service = new TcpService();
            service.Received = async (client, e) =>
            {
                e.ByteBlock.SetHolding(true);
                await m_channel.Writer.WriteAsync(e.ByteBlock);
                //client.Send(byteBlock);
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

        static async Task RunGroup(int count)
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

        static async Task<TcpClient> GetTcpClient()
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