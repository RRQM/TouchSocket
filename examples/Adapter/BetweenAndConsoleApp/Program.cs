//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace BetweenAndConsoleApp;

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
                await client.SendAsync("**12##12##");
            }
        }
    }

    private static async Task<TcpClient> CreateClient()
    {
        var client = new TcpClient();
        //载入配置
        await client.SetupAsync(new TouchSocketConfig()
             .SetRemoteIPHost("127.0.0.1:7789")
             .SetTcpDataHandlingAdapter(() => new MyCustomBetweenAndDataHandlingAdapter())
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

            if (e.RequestInfo is MyBetweenAndRequestInfo myRequest)
            {
                client.Logger.Info($"已从{client.Id}接收到：消息={Encoding.UTF8.GetString(myRequest.Body)}");
            }
            return Task.CompletedTask;
        };

        await service.SetupAsync(new TouchSocketConfig()//载入配置
             .SetListenIPHosts("tcp://127.0.0.1:7789", 7790)//同时监听两个地址
             .SetTcpDataHandlingAdapter(() => new MyCustomBetweenAndDataHandlingAdapter())
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

#region 区间字符实体类
internal class MyBetweenAndRequestInfo : IRequestInfo
{
    public MyBetweenAndRequestInfo(byte[] body)
    {
        this.Body = body;
    }

    public byte[] Body { get; private set; }
}
#endregion


internal class MyCustomBetweenAndDataHandlingAdapter : CustomBetweenAndDataHandlingAdapter<MyBetweenAndRequestInfo>
{
    public MyCustomBetweenAndDataHandlingAdapter()
    {
        this.MinSize = 5;//表示，实际数据体不会小于5，例如“**12##12##”数据，解析后会解析成“12##12”

        this.m_startCode = Encoding.UTF8.GetBytes("**");//可以为0长度字节，意味着没有起始标识。
        this.m_endCode = Encoding.UTF8.GetBytes("##");//必须为有效值。
    }

    private readonly ReadOnlyMemory<byte> m_startCode;
    private readonly ReadOnlyMemory<byte> m_endCode;

    public override ReadOnlyMemory<byte> StartCode => this.m_startCode;

    public override ReadOnlyMemory<byte> EndCode => this.m_endCode;

    protected override MyBetweenAndRequestInfo GetInstance(ReadOnlySpan<byte> body)
    {
        return new MyBetweenAndRequestInfo(body.ToArray());
    }
}
