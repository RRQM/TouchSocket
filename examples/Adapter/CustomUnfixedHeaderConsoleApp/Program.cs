using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace CustomUnfixedHeaderConsoleApp;

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
                var myRequestInfo = new MyUnfixedHeaderRequestInfo()
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
             .SetTcpDataHandlingAdapter(() => new MyUnfixedHeaderCustomDataHandlingAdapter())
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

            if (e.RequestInfo is MyUnfixedHeaderRequestInfo myRequest)
            {
                client.Logger.Info($"已从{client.Id}接收到：DataType={myRequest.DataType},OrderType={myRequest.OrderType},消息={Encoding.UTF8.GetString(myRequest.Body)}");
            }
            return Task.CompletedTask;
        };

        await service.SetupAsync(new TouchSocketConfig()//载入配置
             .SetListenIPHosts("tcp://127.0.0.1:7789", 7790)//同时监听两个地址
             .SetTcpDataHandlingAdapter(() => new MyUnfixedHeaderCustomDataHandlingAdapter())
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

public class MyUnfixedHeaderCustomDataHandlingAdapter : CustomUnfixedHeaderDataHandlingAdapter<MyUnfixedHeaderRequestInfo>
{
    protected override MyUnfixedHeaderRequestInfo GetInstance()
    {
        return new MyUnfixedHeaderRequestInfo();
    }
}

public class MyUnfixedHeaderRequestInfo : IUnfixedHeaderRequestInfo
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



    public int HeaderLength { get; private set; }

    public bool OnParsingBody(ReadOnlySpan<byte> body)
    {
        if (body.Length == this.BodyLength)
        {
            this.Body = body.ToArray();
            return true;
        }
        return false;
    }


    public bool OnParsingHeader<TReader>(ref TReader reader) where TByteBlock : IByteBlock
    {
        //在使用不固定包头解析时

        //【首先】需要先解析包头
        if (byteBlock.CanReadLength < 3)
        {
            //即直接缓存
            return false;
        }

        //先保存一下初始游标，如果解析时还需要缓存，可能需要回退游标
        var position = byteBlock.Position;

        //【然后】ReadToSpan会递增游标，所以不需要再递增游标
        var header = byteBlock.ReadToSpan(3);

        //如果使用Span自行裁剪的话，就需要手动递增游标
        //var header=byteBlock.Span.Slice(position,3);
        //byteBlock.Position += 3;

        //【然后】解析包头，和BodyLength
        //在该示例中，第一个字节表示后续的所有数据长度，但是header设置的是3，所以后续还应当接收length-2个长度。
        this.BodyLength = header[0] - 2;
        this.DataType = header[1];
        this.OrderType = header[2];

        //【最后】对HeaderLength做有效赋值
        this.HeaderLength = 3;

        return true;
    }
}
