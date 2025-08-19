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

namespace CustomAdapterConsoleApp;

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
                var myRequestInfo = new MyRequestInfo()
                {
                    Body = Encoding.UTF8.GetBytes("hello"),
                    DataType = (byte)i,
                    OrderType = (byte)i
                };

                //构建发送数据
                var byteBlock = new ByteBlock(1024);
                try
                {
                    WriterExtension.WriteValue(ref byteBlock, (byte)(byte)(myRequestInfo.Body.Length + 2));//先写长度，因为该长度还包含数据类型和指令类型，所以+2
                    WriterExtension.WriteValue(ref byteBlock, (byte)myRequestInfo.DataType);//然后数据类型
                    WriterExtension.WriteValue(ref byteBlock, (byte)myRequestInfo.OrderType);//然后指令类型
                    byteBlock.Write(myRequestInfo.Body);//再写数据
                    await client.SendAsync(byteBlock.Memory);
                }
                finally
                {
                    byteBlock.Dispose();
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
             .SetTcpDataHandlingAdapter(() => new MyCustomDataHandlingAdapter())
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

            if (e.RequestInfo is MyRequestInfo myRequest)
            {
                client.Logger.Info($"已从{client.Id}接收到：DataType={myRequest.DataType},OrderType={myRequest.OrderType},消息={Encoding.UTF8.GetString(myRequest.Body)}");
            }
            return Task.CompletedTask;
        };

        await service.SetupAsync(new TouchSocketConfig()//载入配置
             .SetListenIPHosts("tcp://127.0.0.1:7789", 7790)//同时监听两个地址
             .SetTcpDataHandlingAdapter(() => new MyCustomDataHandlingAdapter())
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

internal class MyCustomDataHandlingAdapter : CustomDataHandlingAdapter<MyRequestInfo>
{
    
    protected override FilterResult Filter<TByteBlock>(ref TByteBlock byteBlock, bool beCached, ref MyRequestInfo request)
    {
        //以下解析思路为一次性解析，不考虑缓存的临时对象。

        if (byteBlock.BytesRemaining < 3)
        {
            return FilterResult.Cache;//当头部都无法解析时，直接缓存
        }

        //先获取3字节的header
        var header = byteBlock.GetSpan(3);

        //解析header
        //因为第一个字节表示所有长度，而DataType、OrderType已经包含在了header里面。
        //所有只需呀再读取header[0]-2个长度即可。
        var bodyLength = (byte)(header[0] - 2);

        //在判断剩余长度是否足够，不够则缓存
        //这里在加3是因为header是3字节，但是没有Advance，包含header的3个字节
        if (byteBlock.BytesRemaining < bodyLength + 3)
        {
            return FilterResult.Cache;
        }

        //读取body,
        var body = byteBlock.GetSpan(bodyLength + 3).Slice(3, bodyLength);

        byteBlock.Advance(bodyLength + 3);//推进游标

        var myRequestInfo = new MyRequestInfo();

        myRequestInfo.Body = body.ToArray();
        myRequestInfo.DataType = header[1];
        myRequestInfo.OrderType = header[2];
        request = myRequestInfo;//赋值ref

        return FilterResult.Success;//返回成功
    }
}

internal class MyRequestInfo : IRequestInfo
{
    /// <summary>
    /// 自定义属性,Body
    /// </summary>
    public byte[] Body { get; internal set; }

    /// <summary>
    /// 自定义属性,DataType
    /// </summary>
    public byte DataType { get; internal set; }

    /// <summary>
    /// 自定义属性,OrderType
    /// </summary>
    public byte OrderType { get; internal set; }
}