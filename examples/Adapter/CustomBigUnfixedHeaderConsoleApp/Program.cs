using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace CustomBigUnfixedHeaderConsoleApp;

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
                var myRequestInfo = new MyBigUnfixedHeaderRequestInfo()
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
             .SetTcpDataHandlingAdapter(() => new MyCustomBigUnfixedHeaderDataHandlingAdapter())
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

            if (e.RequestInfo is MyBigUnfixedHeaderRequestInfo myRequest)
            {
                client.Logger.Info($"已从{client.Id}接收到：DataType={myRequest.DataType},OrderType={myRequest.OrderType},消息={Encoding.UTF8.GetString(myRequest.Body)}");
            }
            return Task.CompletedTask;
        };

        await service.SetupAsync(new TouchSocketConfig()//载入配置
             .SetListenIPHosts("tcp://127.0.0.1:7789", 7790)//同时监听两个地址
             .SetTcpDataHandlingAdapter(() => new MyCustomBigUnfixedHeaderDataHandlingAdapter())
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

internal class MyCustomBigUnfixedHeaderDataHandlingAdapter : CustomBigUnfixedHeaderDataHandlingAdapter<MyBigUnfixedHeaderRequestInfo>
{
    protected override MyBigUnfixedHeaderRequestInfo GetInstance()
    {
        return new MyBigUnfixedHeaderRequestInfo();
    }
}

internal class MyBigUnfixedHeaderRequestInfo : IBigUnfixedHeaderRequestInfo
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


    private readonly List<byte> m_bytes = new List<byte>();

    private int m_headerLength;
    private long m_bodyLength;

    #region 接口成员
    int IBigUnfixedHeaderRequestInfo.HeaderLength => this.m_headerLength;

    long IBigUnfixedHeaderRequestInfo.BodyLength => this.m_bodyLength;

    void IBigUnfixedHeaderRequestInfo.OnAppendBody(ReadOnlySpan<byte> buffer)
    {
        //每次追加数据
        this.m_bytes.AddRange(buffer.ToArray());
    }

    bool IBigUnfixedHeaderRequestInfo.OnFinished()
    {
        if (this.m_bytes.Count == this.m_bodyLength)
        {
            this.Body = this.m_bytes.ToArray();
            return true;
        }
        return false;
    }

    bool IBigUnfixedHeaderRequestInfo.OnParsingHeader<TReader>(ref TReader reader)
    {
        if (reader.BytesRemaining < 3)//判断可读数据是否满足一定长度
        {
            return false;
        }

        var pos = reader.BytesRead;//可以先记录游标位置，当解析不能进行时回退游标

        //在该示例中，第一个字节表示后续的所有数据长度，但是header设置的是3，所以后续还应当接收length-2个长度。
        this.m_bodyLength = ReaderExtension.ReadValue<TReader,byte>(ref reader) - 2;
        this.DataType = ReaderExtension.ReadValue<TReader,byte>(ref reader);
        this.OrderType = ReaderExtension.ReadValue<TReader,byte>(ref reader);


        //当执行到这里时，byteBlock.Position已经递增了3个长度。
        //所以无需再其他操作，如果是其他，则需要手动移动byteBlock.Position到指定位置。

        this.m_headerLength = 3;//表示Header消耗了3个字节，实际上可以省略这一行，但是为了性能，最好加上
        return true;
    }
    #endregion
}
