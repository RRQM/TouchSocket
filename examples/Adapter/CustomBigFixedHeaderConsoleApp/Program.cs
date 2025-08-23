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
                var myRequestInfo = new MyDataClass()
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
        #region 接收自定义大数据固定包头适配器
        service.Received = (client, e) =>
        {
            //从客户端收到信息

            if (e.RequestInfo is MyDataClass myRequest)
            {
                client.Logger.Info($"已从{client.Id}接收到：DataType={myRequest.DataType},OrderType={myRequest.OrderType},消息={Encoding.UTF8.GetString(myRequest.Body)}");
            }
            return Task.CompletedTask;
        };
        #endregion

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

#region 创建自定义大数据固定包头适配器 {10,42-46,48-56,58-74}
/// <summary>
/// 第1个字节表示指令类型
/// 第2字节表示数据类型
/// 第3字节表示后续数据的长度。使用ushort(大端)表示，最大长度为65535
/// 后续字节表示载荷数据
/// 最后2字节表示CRC16校验码
/// </summary>
internal class MyCustomBigFixedHeaderDataHandlingAdapter : CustomBigFixedHeaderDataHandlingAdapter<MyDataClass>
{
    public override int HeaderLength => 4;

    protected override MyDataClass GetInstance()
    {
        return new MyDataClass();
    }
}

internal class MyDataClass : IBigFixedHeaderRequestInfo
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

    private long m_length;

    private readonly List<byte> m_bytes = new List<byte>();

    #region 接口成员
    long IBigFixedHeaderRequestInfo.BodyLength => this.m_length;

    void IBigFixedHeaderRequestInfo.OnAppendBody(ReadOnlySpan<byte> buffer)
    {
        //每次追加数据
        this.m_bytes.AddRange(buffer.ToArray());
    }

    bool IBigFixedHeaderRequestInfo.OnFinished()
    {
        if (this.m_bytes.Count == this.m_length)
        {
            this.Body = this.m_bytes.ToArray();
            return true;
        }
        return false;
    }

    bool IBigFixedHeaderRequestInfo.OnParsingHeader(ReadOnlySpan<byte> header)
    {
        //这里header长度已经经过验证，一定是4字节

        //获取指令类型
        this.OrderType = header[0];

        //获取数据类型
        this.DataType = header[1];

        var payloadLength = TouchSocketBitConverter.BigEndian.To<ushort>(header.Slice(2, 2));


        this.m_length = payloadLength + 2;//+2是因为还包含Crc16的长度

        return true;
    }
    #endregion

}
#endregion
