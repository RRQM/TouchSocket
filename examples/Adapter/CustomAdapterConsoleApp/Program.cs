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
                    WriterExtension.WriteValue(ref byteBlock, (byte)(byte)(myRequestInfo.Data.Length + 2));//先写长度，因为该长度还包含数据类型和指令类型，所以+2
                    WriterExtension.WriteValue(ref byteBlock, (byte)myRequestInfo.DataType);//然后数据类型
                    WriterExtension.WriteValue(ref byteBlock, (byte)myRequestInfo.OrderType);//然后指令类型
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

        #region 使用自定义适配器接收数据 {3}
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

#region 用户自定义适配器

/// <summary>
/// 第1个字节表示指令类型
/// 第2字节表示数据类型
/// 第3字节表示后续数据的长度。使用ushort(大端)表示，最大长度为65535
/// 后续字节表示载荷数据
/// 最后2字节表示CRC16校验码
/// </summary>
internal class MyCustomDataHandlingAdapter : CustomDataHandlingAdapter<MyDataClass>
{
    private ushort m_payloadLength;

    protected override FilterResult Filter<TReader>(ref TReader reader, bool beCached, ref MyDataClass request)
    {
        if (beCached)
        {
            //说明上次已经解析了header

            return this.ParseData(ref reader, request);
        }
        else
        {
            //首次解析

            if (reader.BytesRemaining < 4)
            {
                //如果剩余数据小于4个字节，则继续等待
                return FilterResult.Cache;
            }

            //读取前4个字节
            var header = reader.GetSpan(4);

            //推进已读取的4个字节
            reader.Advance(4);

            //获取指令类型
            var orderType = header[0];
            //获取数据类型
            var dataType = header[1];

            //创建数据对象
            request = new MyDataClass()
            {
                OrderType = orderType,
                DataType = dataType
            };

            //获取载荷长度
            this.m_payloadLength = TouchSocketBitConverter.BigEndian.To<ushort>(header.Slice(2, 2));

            return this.ParseData(ref reader, request);
        }
    }

    private FilterResult ParseData<TReader>(ref TReader reader, MyDataClass myDataClass)
        where TReader : IBytesReader
    {
        //判断剩余数据是否足够，+2是因为最后2个字节是CRC16校验码
        if (reader.BytesRemaining < this.m_payloadLength + 2)
        {
            return FilterResult.Cache;
        }

        //读取数据
        var data = reader.GetSpan(this.m_payloadLength);
        reader.Advance(this.m_payloadLength);

        //读取CRC16校验码
        var crcData = reader.GetSpan(2);
        reader.Advance(2);

        //转换CRC16校验码为ushort，相比于byte[]，更节省内存
        var crc16 = TouchSocketBitConverter.BigEndian.To<ushort>(crcData);

        //计算CRC16
        var newCrc16 = Crc.Crc16Value(data);
        if (crc16 != newCrc16)
        {
            //CRC校验失败
            throw new Exception("CRC校验失败");
        }

        //保存数据
        myDataClass.Data = data.ToArray();

        //至此，数据接收完成，可以进行投递处理
        this.m_payloadLength = 0;
        return FilterResult.Success;
    }
}

/// <summary>
/// 定义数据对象
/// </summary>
internal class MyDataClass : IRequestInfo
{
    public byte OrderType { get; set; }
    public byte DataType { get; set; }
    public byte[] Data { get; set; }
}
#endregion