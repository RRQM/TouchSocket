using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace CustomCountSpliterDataHandlingAdapterConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var service = await CreateService();
        var client = await CreateClient();

        ConsoleLogger.Default.Info("按任意键发送10次");
        while (true)
        {
            Console.ReadKey();
            for (var i = 0; i < 10; i++)
            {
                var data = "#part1#part2#part3#part4#part5#part6#par7#";
                //构建发送数据
                using (var byteBlock = new ByteBlock(1024))
                {
                    byteBlock.WriteNormalString(data, Encoding.UTF8);
                    await client.SendAsync(byteBlock.Memory);
                }
            }
        }
    }

    private static async Task<TcpClient> CreateClient()
    {
        var client = new TcpClient();
        //载入配置
        await client.SetupAsync(new TouchSocketConfig()
             .SetRemoteIPHost("127.0.0.1:7789")
             .SetTcpDataHandlingAdapter(() => new MyCustomCountSpliterDataHandlingAdapter())
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
        service.Received = (client, e) =>
        {
            //从客户端收到信息

            if (e.RequestInfo is MyCountSpliterRequestInfo myRequest)
            {
                client.Logger.Info($"已从{client.Id}接收到：消息={myRequest.Data}");
            }
            return Task.CompletedTask;
        };

        await service.SetupAsync(new TouchSocketConfig()//载入配置
             .SetListenIPHosts("tcp://127.0.0.1:7789", 7790)//同时监听两个地址
             .SetTcpDataHandlingAdapter(() => new MyCustomCountSpliterDataHandlingAdapter())
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

internal class MyCountSpliterRequestInfo : IRequestInfo
{
    public string Data { get; private set; }

    public MyCountSpliterRequestInfo(string data)
    {
        this.Data = data;
    }
}

internal class MyCustomCountSpliterDataHandlingAdapter : CustomCountSpliterDataHandlingAdapter<MyCountSpliterRequestInfo>
{
    public MyCustomCountSpliterDataHandlingAdapter() : base(8, Encoding.UTF8.GetBytes("#"))
    {
    }

    protected override MyCountSpliterRequestInfo GetInstance(in ReadOnlySpan<byte> dataSpan)
    {
        return new MyCountSpliterRequestInfo(dataSpan.ToString(Encoding.UTF8));
    }
}
