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
        #region 接收自定义固定数量分隔符适配器
        service.Received = (client, e) =>
        {
            //从客户端收到信息

            if (e.RequestInfo is MyCountSpliterRequestInfo myRequest)
            {
                client.Logger.Info($"已从{client.Id}接收到：消息={myRequest.Data}");
            }
            return Task.CompletedTask;
        };
        #endregion
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


#region 创建自定义固定数量分隔符适配器
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

internal class MyCountSpliterRequestInfo : IRequestInfo
{
    public string Data { get; private set; }

    public MyCountSpliterRequestInfo(string data)
    {
        this.Data = data;
    }
}
#endregion