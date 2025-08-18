using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace CustomBigFixedHeaderConsoleApp;

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
                var myRequestInfo = new MyBigFixedHeaderRequestInfo()
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
             .SetTcpDataHandlingAdapter(() => new MyCustomBigFixedHeaderDataHandlingAdapter())
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

            if (e.RequestInfo is MyBigFixedHeaderRequestInfo myRequest)
            {
                client.Logger.Info($"已从{client.Id}接收到：DataType={myRequest.DataType},OrderType={myRequest.OrderType},消息={Encoding.UTF8.GetString(myRequest.Body)}");
            }
            return Task.CompletedTask;
        };

        await service.SetupAsync(new TouchSocketConfig()//载入配置
             .SetListenIPHosts("tcp://127.0.0.1:7789", 7790)//同时监听两个地址
             .SetTcpDataHandlingAdapter(() => new MyCustomBigFixedHeaderDataHandlingAdapter())
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

internal class MyCustomBigFixedHeaderDataHandlingAdapter : CustomBigFixedHeaderDataHandlingAdapter<MyBigFixedHeaderRequestInfo>
{
    public override int HeaderLength => 3;

    protected override MyBigFixedHeaderRequestInfo GetInstance()
    {
        return new MyBigFixedHeaderRequestInfo();
    }
}

internal class MyBigFixedHeaderRequestInfo : IBigFixedHeaderRequestInfo
{
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

    private long m_bodyLength;

    private readonly List<byte> m_bytes = new List<byte>();

    #region 接口成员
    long IBigFixedHeaderRequestInfo.BodyLength => this.m_bodyLength;

    void IBigFixedHeaderRequestInfo.OnAppendBody(ReadOnlySpan<byte> buffer)
    {
        //每次追加数据
        this.m_bytes.AddRange(buffer.ToArray());
    }

    bool IBigFixedHeaderRequestInfo.OnFinished()
    {
        if (this.m_bytes.Count == this.m_bodyLength)
        {
            this.Body = this.m_bytes.ToArray();
            return true;
        }
        return false;
    }

    bool IBigFixedHeaderRequestInfo.OnParsingHeader(ReadOnlySpan<byte> header)
    {
        //在该示例中，第一个字节表示后续的所有数据长度，但是header设置的是3，所以后续还应当接收length-2个长度。
        this.m_bodyLength = header[0] - 2;
        this.DataType = header[1];
        this.OrderType = header[2];
        return true;
    }
    #endregion

}
