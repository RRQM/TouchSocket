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
                var myRequestInfo = new MyDataClass()
                {
                    Data = Encoding.UTF8.GetBytes("hello"),
                    DataType = (byte)i,
                    OrderType = (byte)i
                };

                //构建发送数据
                var byteBlock = new ByteBlock(1024);
                try
                {
                    WriterExtension.WriteValue(ref byteBlock, (byte)(myRequestInfo.Data.Length + 2));//先写长度，因为该长度还包含数据类型和指令类型，所以+2
                    WriterExtension.WriteValue(ref byteBlock, myRequestInfo.DataType);//然后数据类型
                    WriterExtension.WriteValue(ref byteBlock, myRequestInfo.OrderType);//然后指令类型
                    byteBlock.Write(myRequestInfo.Data);//再写数据

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
        #region 接收自定义非固定包头适配器
        service.Received = (client, e) =>
        {
            if (e.RequestInfo is MyDataClass myRequest)
            {
                client.Logger.Info($"已从{client.Id}接收到：DataType={myRequest.DataType},OrderType={myRequest.OrderType},消息={Encoding.UTF8.GetString(myRequest.Data)}");
            }
            return Task.CompletedTask;
        };
        #endregion

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

#region 创建自定义非固定包头适配器
/// <summary>
/// 第1个字节表示指令类型
/// 第2字节表示数据类型
/// 第3字节表示后续数据的长度。使用ushort(大端)表示，最大长度为65535
/// 后续字节表示载荷数据
/// 最后2字节表示CRC16校验码
/// </summary>
public class MyUnfixedHeaderCustomDataHandlingAdapter : CustomUnfixedHeaderDataHandlingAdapter<MyDataClass>
{
    protected override MyDataClass GetInstance()
    {
        return new MyDataClass();
    }
}

public class MyDataClass : IUnfixedHeaderRequestInfo
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
    public byte[] Data { get; set; }

    public int HeaderLength { get; private set; }

    public bool OnParsingBody(ReadOnlySpan<byte> body)
    {
        var data = body.Slice(0, body.Length - 2);//最后2个字节是CRC16校验码
        var crc16 = TouchSocketBitConverter.BigEndian.To<ushort>(body.Slice(body.Length - 2, 2));

        //计算CRC16
        var newCrc16 = Crc.Crc16Value(data);
        if (crc16 != newCrc16)
        {
            //CRC校验失败
            throw new Exception("CRC校验失败");
        }

        this.Data = data.ToArray();
        return true;
    }


    public bool OnParsingHeader<TReader>(ref TReader reader) where TReader : IBytesReader
    {
        //在使用不固定包头解析时

        //【首先】需要先解析包头
        if (reader.BytesRemaining < 4)
        {
            //即直接缓存
            return false;
        }

        //【然后】先获取4字节包头
        var header = reader.GetSpan(4);

        reader.Advance(4); //推进游标到包头结束位置

        //【然后】解析包头，和BodyLength
        //获取指令类型
        this.OrderType = header[0];

        //获取数据类型
        this.DataType = header[1];

        var payloadLength = TouchSocketBitConverter.BigEndian.To<ushort>(header.Slice(2, 2));


        //【最后】对HeaderLength做有效赋值
        this.HeaderLength = 4;
        this.BodyLength = payloadLength + 2;

        return true;
    }
}

#endregion
