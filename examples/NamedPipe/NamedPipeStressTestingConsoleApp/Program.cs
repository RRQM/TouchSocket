using System.Text;
using TouchSocket.Core;
using TouchSocket.NamedPipe;
using TouchSocket.Sockets;

namespace NamedPipeStressTestingConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("请输入管道名称");
            var name = Console.ReadLine();

            var service = CreateService(name);
            var client = CreateClient(name);



            byte[] buffer = new byte[1024 * 1024];
            while (true)
            {
                client.Send(buffer);
            }
        }

        private static NamedPipeClient CreateClient(string name)
        {
            var client = new NamedPipeClient();
            //载入配置
            client.Setup(new TouchSocketConfig()
                .SetPipeServer(".")//一般本机管道时，可以不用此配置
                .SetPipeName(name)//管道名称
                .ConfigurePlugins(a =>
                {

                })
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();//添加一个日志注入
                }));
            client.Connect();
            client.Logger.Info("客户端成功连接");
            return client;
        }

        private static NamedPipeService CreateService(string name)
        {
            var service = new NamedPipeService();
            service.Connecting = (client, e) => { return EasyTask.CompletedTask; };//有客户端正在连接
            service.Connected = (client, e) => { return EasyTask.CompletedTask; };//有客户端成功连接
            service.Disconnected = (client, e) => { return EasyTask.CompletedTask; };//有客户端断开连接\

            long count = 0;
            DateTime dateTime = DateTime.Now;
            service.Received = (client, e) =>
            {
                if (DateTime.Now - dateTime > TimeSpan.FromSeconds(1))
                {
                    Console.WriteLine((count / (1048576.0)).ToString("0.00"));
                    count = 0;
                    dateTime = DateTime.Now;
                }
                count += e.ByteBlock.Len;
                return EasyTask.CompletedTask;
            };
            service.Setup(new TouchSocketConfig()//载入配置
                .SetPipeName(name)//设置命名管道名称
                .ConfigureContainer(a =>//容器的配置顺序应该在最前面
                {
                    a.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
                })
                .ConfigurePlugins(a =>
                {
                    //a.Add();//此处可以添加插件
                }));
            service.Start();//启动
            service.Logger.Info("服务器已启动");
            return service;
        }
    }
}