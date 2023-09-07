using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace PackageAdapterConsoleApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var service = CreateService();
            var client = CreateClient();

            ConsoleLogger.Default.Info("输入任意内容，回车发送（将会循环发送10次）");
            while (true)
            {
                var str = Console.ReadLine();
                for (var i = 0; i < 10; i++)
                {
                    client.Send(str);
                }
            }
        }

        static TcpClient CreateClient()
        {
            var client = new TcpClient();
            //载入配置
            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789")
                .SetTcpDataHandlingAdapter(() => new TerminatorPackageAdapter("\r\n"))
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();//添加一个日志注入
                }));

            client.Connect();//调用连接，当连接不成功时，会抛出异常。
            client.Logger.Info("客户端成功连接");
            return client;
        }

        static TcpService CreateService()
        {
            var service = new TcpService();
            service.Received = (client, byteBlock, requestInfo) =>
            {
                //从客户端收到信息
                string mes = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len);//注意：数据长度是byteBlock.Len
                client.Logger.Info($"已从{client.Id}接收到信息：{mes}");
            };

            service.Setup(new TouchSocketConfig()//载入配置
                .SetListenIPHosts("tcp://127.0.0.1:7789", 7790)//同时监听两个地址
                .SetTcpDataHandlingAdapter(() => new TerminatorPackageAdapter("\r\n"))
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
                })
                .ConfigurePlugins(a =>
                {
                    //a.Add();//此处可以添加插件
                }))
                .Start();//启动
            service.Logger.Info("服务器已启动");
            return service;
        }
    }
}