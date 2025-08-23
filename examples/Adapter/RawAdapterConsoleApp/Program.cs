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

using System;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.NamedPipe;
using TouchSocket.SerialPorts;
using TouchSocket.Sockets;

namespace RawAdapterConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var service = await CreateService();
        var client = await CreateClient();

        ConsoleLogger.Default.Info("输入任意内容，回车发送（将会循环发送10次）");
        while (true)
        {
            var str = Console.ReadLine();
            for (var i = 0; i < 10; i++)
            {
                await client.SendAsync(str);
            }
        }
    }

    private static async Task<TcpClient> CreateClient()
    {
        var client = new TcpClient();
        //载入配置
        await client.SetupAsync(new TouchSocketConfig()
             .SetRemoteIPHost("127.0.0.1:7789")
             .SetTcpDataHandlingAdapter(() => new MyRawDataHandleAdapter())
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
            var mes = e.Memory.Span.ToString(Encoding.UTF8);
            client.Logger.Info($"已从{client.Id}接收到信息：{mes}");
            return Task.CompletedTask;
        };

        await service.SetupAsync(new TouchSocketConfig()//载入配置
             .SetListenIPHosts("tcp://127.0.0.1:7789", 7790)//同时监听两个地址
             .SetTcpDataHandlingAdapter(() => new PeriodPackageAdapter() { CacheTimeout = TimeSpan.FromSeconds(1) })
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

    public static void Config()
    {
        var config = new TouchSocketConfig();

        #region 配置使用Tcp适配器
        config.SetTcpDataHandlingAdapter(() => new TerminatorPackageAdapter("\r\n"));
        #endregion

        #region 配置使用NamedPipe适配器
        config.SetNamedPipeDataHandlingAdapter(() => new TerminatorPackageAdapter("\r\n"));
        #endregion

        #region 配置使用Serial适配器
        config.SetSerialDataHandlingAdapter(() => new TerminatorPackageAdapter("\r\n"));
        #endregion

    }
}

#region 使用原始数据适配器解析
/// <summary>
/// 第1个字节表示指令类型
/// 第2字节表示数据类型
/// 第3字节表示后续数据的长度。使用ushort(大端)表示，最大长度为65535
/// 后续字节表示载荷数据
/// 最后2字节表示CRC16校验码
/// </summary>
internal class MyRawDataHandleAdapter : SingleStreamDataHandlingAdapter
{
    private MyDataClass m_myDataClass;
    private int m_payloadLength;
    protected override async Task PreviewReceivedAsync<TReader>(TReader reader)
    {
        while (reader.BytesRemaining>0)
        {
            if (this.m_myDataClass == null)
            {
                //首次接收该解析对象

                if (reader.BytesRemaining < 4)
                {
                    //如果剩余数据小于4个字节，则继续等待
                    return;
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
                this.m_myDataClass = new MyDataClass()
                {
                    OrderType = orderType,
                    DataType = dataType
                };

                //获取载荷长度
                this.m_payloadLength = TouchSocketBitConverter.BigEndian.To<ushort>(header.Slice(2, 2));

                await this.ParseData(reader);
            }
            else
            {
                //已经读取过头部，继续读取剩余数据
                await this.ParseData(reader);
            }
        }
    }

    private async Task ParseData<TReader>(TReader reader)
        where TReader : class, IBytesReader
    {
        //判断剩余数据是否足够，+2是因为最后2个字节是CRC16校验码
        if (reader.BytesRemaining < this.m_payloadLength + 2)
        {
            return;
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
        this.m_myDataClass.Data = data.ToArray();

        //至此，数据接收完成，可以进行投递处理

        //此处可以选择投递方式，此处选择了使用IRequestInfo投递
        await base.GoReceivedAsync(ReadOnlyMemory<byte>.Empty, this.m_myDataClass);

        //清空数据，准备下次接收
        this.m_myDataClass = null;
        this.m_payloadLength = 0;
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


#region 示例Tcp客户端直接设置适配器 {6}
internal class MyTcpClient : TcpClient
{
    protected override Task OnTcpConnecting(ConnectingEventArgs e)
    {
        //直接设置适配器到当前客户端
        base.SetAdapter(new TerminatorPackageAdapter("\r\n"));
        return base.OnTcpConnecting(e);
    }
}
#endregion
