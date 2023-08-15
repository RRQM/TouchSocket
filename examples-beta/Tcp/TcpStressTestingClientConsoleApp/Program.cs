using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TcpStressTestingClientConsoleApp
{
    internal class Program
    {
        public const int Count = 100000;
        public const int DataLength = 2000;
        static async Task Main(string[] args)
        {
            await Console.Out.WriteLineAsync($"即将测试1000个客户端，每个客户端广播{Count}条数据");
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(RunGroup(100));
            }
            await Task.WhenAll(tasks);
        }

        static Task RunGroup(int count)
        {
            return Task.Factory.StartNew(() =>
            {
                var bytes = new byte[DataLength];
                List<TcpClient> clients = new List<TcpClient>();
                for (int i = 0; i < count; i++)
                {
                    var client = GetTcpClient();
                    clients.Add(client);
                }

                try
                {
                    for (int i = 0; i < Count; i++)
                    {
                        foreach (var item in clients)
                        {
                            item.Send(bytes);
                        }
                    }
                }
                finally
                {
                    ConsoleLogger.Default.Error("退出");
                }
            }, TaskCreationOptions.LongRunning);

        }

        static TcpClient GetTcpClient()
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.Received = (client, byteBlock, requestInfo) =>
            {
                //客户端接收
            };

            //载入配置
            tcpClient.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789")
                //.UseDelaySender()
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();//添加一个日志注入
                })
                .ConfigurePlugins(a =>
                {
                    //a.UseAutoBufferLength()
                    //.SetMin(1024);
                })
                );

            tcpClient.Connect();//调用连接，当连接不成功时，会抛出异常。
            //tcpClient.Logger.Info("客户端成功连接");
            return tcpClient;
        }

    }
}