using System.Diagnostics;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TcpStressTestingConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //BytePool.SetDefault(new BytePool(1024*1024,100));
            Console.WriteLine($"当前内存池容量：{BytePool.Default.Capacity / (1048576.0):0.00}Mb");
            var service = GetTcpService();

            //for (int i = 0; i < 10; i++)
            //{
            //    ProcessStartInfo info = new ProcessStartInfo()
            //    {
            //        UseShellExecute = false,
            //        FileName = Path.GetFullPath("TcpStressTestingClientConsoleApp.exe")
            //    };
            //    Process.Start(info);
            //}

            Console.ReadKey();
        }



        static TcpService GetTcpService()
        {
            TcpService service = new TcpService();
            service.Received = (client, byteBlock, requestInfo) =>
            {
                ////var bytes = byteBlock.ToArray();
                //foreach (var id in client.GetOtherIds())
                //{
                //    if (service.TryGetSocketClient(id, out var socketClient))
                //    {
                //        socketClient.Send(byteBlock);
                //    }
                //}

                client.Send(byteBlock);
            };

            service.Setup(new TouchSocketConfig()//载入配置
                                                 //.UseDelaySender()
                .SetListenIPHosts("tcp://127.0.0.1:7789", 7790)//同时监听两个地址
                .SetBufferLength(1024 * 2)
                .ConfigureContainer(a =>//容器的配置顺序应该在最前面
                {
                    a.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
                })
                .ConfigurePlugins(a =>
                {
                    //a.UseAutoBufferLength()
                    //.SetMin(1024);
                    //a.Add();//此处可以添加插件
                }))
                .Start();//启动
            service.Logger.Info("服务器已启动");
            return service;
        }
    }
}