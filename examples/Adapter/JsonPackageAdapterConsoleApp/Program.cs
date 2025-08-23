using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace JsonPackageAdapterConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var service = await CreateService();
        var client = await CreateClient();

        ConsoleLogger.Default.Info("输入任意符合Json格式的内容，回车发送（将会循环发送10次）");
        while (true)
        {
            var str = Console.ReadLine();
            for (var i = 0; i < 10; i++)
            {
                await client.SendAsync(str);
            }
        }
    }

    private static async Task<TcpClient> CreateClient()
    {
        var client = new TcpClient();
        //载入配置
        await client.SetupAsync(new TouchSocketConfig()
             .SetRemoteIPHost("127.0.0.1:7789")
             .SetTcpDataHandlingAdapter(() => new JsonPackageAdapter(Encoding.UTF8))//赋值适配，必须使用委托，且返回的适配，必须new。不能返回一个单例
             .ConfigureContainer(a =>
             {
                 a.AddConsoleLogger();//添加一个日志注入
             }));

        await client.ConnectAsync();//调用连接，当连接不成功时，会抛出异常。
        client.Logger.Info("客户端成功连接");
        return client;
    }

    private static async Task<TcpService> CreateService()
    {
        var service = new TcpService();

        #region 内置包Json适配器按JsonPackage解析 {3-11}
        service.Received = async (client, e) =>
        {
            if (e.RequestInfo is JsonPackage jsonPackage)
            {
                var sb = new StringBuilder();
                sb.Append($"已从{client.Id}接收到数据。");
                sb.Append($"数据类型：{jsonPackage.Kind},");
                sb.Append($"数据：{jsonPackage.DataString},");
                sb.Append($"杂质数据：{jsonPackage.ImpurityData.Span.ToString(Encoding.UTF8)}");
                client.Logger.Info(sb.ToString());
            }
            await e.InvokeNext();
        };
        #endregion
        await service.SetupAsync(new TouchSocketConfig()//载入配置
             .SetListenIPHosts("tcp://127.0.0.1:7789", 7790)//同时监听两个地址
             .SetTcpDataHandlingAdapter(() => new JsonPackageAdapter(Encoding.UTF8))
             .ConfigureContainer(a =>
             {
                 a.AddConsoleLogger();//添加一个控制台日志注入（注意：在maui中控制台日志不可用）
             })
             .ConfigurePlugins(a =>
             {
                 //a.Add();//此处可以添加插件
             }));
        await service.StartAsync();//启动
        service.Logger.Info("服务器已启动");
        return service;
    }
}
