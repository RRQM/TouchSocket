using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace PackageAdapterConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
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

        private static SingleStreamDataHandlingAdapter GetAdapter()
        {
            return new TerminatorPackageAdapter("\r\n");//使用换行终止字符
            //return new PeriodPackageAdapter() { CacheTimeout=TimeSpan.FromMilliseconds(100) };//使用周期适配器。
        }

        private static TcpClient CreateClient()
        {
            var client = new TcpClient();
            //载入配置
            client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost("127.0.0.1:7789")
                .SetTcpDataHandlingAdapter(GetAdapter)//赋值适配，必须使用委托，且返回的适配，必须new。不能返回一个单例
                .ConfigureContainer(a =>
                {
                    a.AddConsoleLogger();//添加一个日志注入
                }));

            client.Connect();//调用连接，当连接不成功时，会抛出异常。
            client.Logger.Info("客户端成功连接");
            return client;
        }

        private static TcpService CreateService()
        {
            var service = new TcpService();
            service.Received = (client, e) =>
            {
                //从客户端收到信息
                var mes = Encoding.UTF8.GetString(e.ByteBlock.Buffer, 0, e.ByteBlock.Len);//注意：数据长度是byteBlock.Len
                client.Logger.Info($"已从{client.Id}接收到信息：{mes}");
                return EasyTask.CompletedTask;
            };

            service.Setup(new TouchSocketConfig()//载入配置
                .SetListenIPHosts("tcp://127.0.0.1:7789", 7790)//同时监听两个地址
                .SetTcpDataHandlingAdapter(GetAdapter)
                .ConfigureContainer(a =>
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