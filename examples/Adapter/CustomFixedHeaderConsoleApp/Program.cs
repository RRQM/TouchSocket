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

namespace CustomFixedHeaderConsoleApp;

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
                var myRequestInfo = new MyFixedHeaderRequestInfo()
                {
                    Body = Encoding.UTF8.GetBytes("hello"),
                    DataType = (byte)i,
                    OrderType = (byte)i
                };

                //构建发送数据
                using (var byteBlock = new ByteBlock(1024))
                {
                    WriterExtension.WriteValue(ref byteBlock,(byte)(byte)(myRequestInfo.Body.Length + 2));//先写长度，因为该长度还包含数据类型和指令类型，所以+2
                    WriterExtension.WriteValue(ref byteBlock,(byte)myRequestInfo.DataType);//然后数据类型
                    WriterExtension.WriteValue(ref byteBlock,(byte)myRequestInfo.OrderType);//然后指令类型
                    byteBlock.Write(myRequestInfo.Body);//再写数据

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
             .SetTcpDataHandlingAdapter(() => new MyFixedHeaderCustomDataHandlingAdapter())
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

            if (e.RequestInfo is MyFixedHeaderRequestInfo myRequest)
            {
                client.Logger.Info($"已从{client.Id}接收到：DataType={myRequest.DataType},OrderType={myRequest.OrderType},消息={Encoding.UTF8.GetString(myRequest.Body)}");
            }
            return Task.CompletedTask;
        };

        await service.SetupAsync(new TouchSocketConfig()//载入配置
             .SetListenIPHosts("tcp://127.0.0.1:7789", 7790)//同时监听两个地址
             .SetTcpDataHandlingAdapter(() => new MyFixedHeaderCustomDataHandlingAdapter())
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

public class MyFixedHeaderCustomDataHandlingAdapter : CustomFixedHeaderDataHandlingAdapter<MyFixedHeaderRequestInfo>
{
    /// <summary>
    /// 接口实现，指示固定包头长度
    /// </summary>
    public override int HeaderLength => 3;

    /// <summary>
    /// 获取新实例
    /// </summary>
    /// <returns></returns>
    protected override MyFixedHeaderRequestInfo GetInstance()
    {
        return new MyFixedHeaderRequestInfo();
    }
}

public class MyFixedHeaderRequestInfo : IFixedHeaderRequestInfo
{
    /// <summary>
    /// 接口实现，标识数据长度
    /// </summary>
    public int BodyLength { get; private set; }

    /// <summary>
    /// 自定义属性，标识数据类型
    /// </summary>
    public byte DataType { get; set; }

    /// <summary>
    /// 自定义属性，标识指令类型
    /// </summary>
    public byte OrderType { get; set; }

    /// <summary>
    /// 自定义属性，标识实际数据
    /// </summary>
    public byte[] Body { get; set; }

    public bool OnParsingBody(ReadOnlySpan<byte> body)
    {
        if (body.Length == this.BodyLength)
        {
            this.Body = body.ToArray();
            return true;
        }
        return false;
    }

    public bool OnParsingHeader(ReadOnlySpan<byte> header)
    {
        //在该示例中，第一个字节表示后续的所有数据长度，但是header设置的是3，所以后续还应当接收length-2个长度。
        this.BodyLength = header[0] - 2;
        this.DataType = header[1];
        this.OrderType = header[2];
        return true;
    }
}