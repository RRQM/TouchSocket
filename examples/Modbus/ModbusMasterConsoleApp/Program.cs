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

using TouchSocket.Core;
using TouchSocket.Modbus;
using TouchSocket.Sockets;

namespace ModbusClientConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var master = await GetModbusTcpMasterAsync();

        await ReadWriteHoldingRegisters(master);
        Console.ReadKey();
    }

    /// <summary>
    /// 要测试，请打开Modbus Slave软件，设置HoldingRegisters。至少30个长度。
    /// </summary>
    public static async Task ReadWriteHoldingRegisters(IModbusMaster master)
    {
        #region ModbusMaster写单个寄存器
        try
        {
            //1:写入的站号
            //0:起始地址
            //1:值
            var modbusResponse = await master.WriteSingleRegisterAsync(1, 0, 1);//默认short ABCD端序
            if (modbusResponse.IsSuccess)
            {
                Console.WriteLine("操作成功");
            }
            else
            {
                Console.WriteLine($"操作失败，异常码：{modbusResponse.ErrorCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        #endregion

        await master.WriteSingleRegisterAsync(1, 1, 1000);//默认short ABCD端序

        #region ModbusMaster写入多个寄存器
        //先构建一个内存块，按照modbus单次最大长度计算，1024字节足够
        var valueByteBlock = new ValueByteBlock(1024);
        try
        {
            WriterExtension.WriteValue(ref valueByteBlock, (ushort)2, EndianType.Big);//ABCD端序
            WriterExtension.WriteValue(ref valueByteBlock, (ushort)2000, EndianType.Little);//DCBA端序
            WriterExtension.WriteValue(ref valueByteBlock, int.MaxValue, EndianType.BigSwap);//BADC端序
            WriterExtension.WriteValue(ref valueByteBlock, long.MaxValue, EndianType.LittleSwap);//CDAB端序

            //写入字符串，会先用4字节表示字符串长度，然后按utf8编码写入字符串
            WriterExtension.WriteString(ref valueByteBlock, "Hello1");

            //如果想要直接写入字符串，可以使用WriteNormalString方法
            //WriterExtension.WriteNormalString(ref valueByteBlock,"Hello1", System.Text.Encoding.UTF8);

            //注意：写入字符串时，应当保证写入后的字节总数为双数。如果是单数，则会报错。

            //写入到寄存器
            await master.WriteMultipleRegistersAsync(1, 2, valueByteBlock.Memory);
        }
        finally
        {
            valueByteBlock.Dispose();
        }
        #endregion

        #region ModbusMaster读取保持寄存器
        //1:读取的站号
        //0:起始地址
        //30:读取长度
        var response = await master.ReadHoldingRegistersAsync(1, 0, 30);

        //获取原始数据
        var memory = response.Data;

        //或者从Span直接读取值
        var span = response.Data.Span;
        Console.WriteLine(span.ReadValue<ushort>(EndianType.Big));
        Console.WriteLine(span.ReadValue<ushort>(EndianType.Big));
        Console.WriteLine(span.ReadValue<ushort>(EndianType.Big));
        Console.WriteLine(span.ReadValue<ushort>(EndianType.Little));
        Console.WriteLine(span.ReadValue<int>(EndianType.BigSwap));
        Console.WriteLine(span.ReadValue<long>(EndianType.LittleSwap));
        Console.WriteLine(span.ReadString());
        #endregion
    }

    public static async Task ReadWriteHoldingRegisters2(IModbusMaster master)
    {
        #region ModbusMaster读取多个寄存器到指定类型
        //读取寄存器
        var response = await master.ReadHoldingRegistersAsync(1, 0, 10);//站点1，从0开始读取10个寄存器

        //获取原始数据
        var memory = response.Data;

        //将数据全部读为无符号32为，且使用大端序，即ABCD
        var values = TouchSocketBitConverter.ConvertValues<byte, uint>(memory.Span, EndianType.Big);
        #endregion

    }

    public static async Task ReadCoilsByInterfaces(IModbusMaster master)
    {
        #region ModbusMaster通过原生接口操作
        //通过原生接口调用
        var modbusRequest = new ModbusRequest(FunctionCode.ReadCoils);
        modbusRequest.SlaveId = (1);//设置站号。如果是Tcp可以不设置
        modbusRequest.StartingAddress = (0);//设置起始
        modbusRequest.Quantity = (1);//设置数量
        modbusRequest.SetValue(false);//如果是写入类操作，可以直接设定值

        //设置超时
        var cts = new CancellationTokenSource(1000);
        try
        {
            //发送请求，并获取响应
            var response = await master.SendModbusRequestAsync(modbusRequest, cts.Token);

            if (response.IsSuccess)
            {
                //操作成功
                Console.WriteLine("操作成功");

                //获取原始数据
                var memory = response.Data;

                //把数据按位bit转换为bool数组
                var bools = TouchSocketBitConverter.ConvertValues<byte, bool>(memory.Span);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
        #endregion
    }

    /// <summary>
    /// 读写线圈，在测试时，请选择对应的Modbus Slave类型，且调到线圈操作，至少5个长度
    /// </summary>
    /// <param name="master"></param>
    public static async Task ReadWriteCoilsShouldBeOk(IModbusMaster master)
    {
        #region ModbusMaster写单个线圈
        //1:写入的站号
        //0:起始地址
        //true:线圈状态
        await master.WriteSingleCoilAsync(1, 0, true);
        #endregion

        await master.WriteSingleCoilAsync(1, 1, false);

        #region ModbusMaster写入多个线圈
        //1:写入的站号
        //2:起始地址
        //new bool[] { true, false, true }:线圈状态
        await master.WriteMultipleCoilsAsync(1, 2, new bool[] { true, false, true });
        #endregion


        #region ModbusMaster读取线圈
        //1:读取的站号
        //0:起始地址
        //5:读取长度
        var values = await master.ReadCoilsAsync(1, 0, 5);
        foreach (var value in values.Span)
        {
            Console.WriteLine(value);
        }
        #endregion

    }

    public static async Task ReadDiscreteInputsShouldBeOk(IModbusMaster master)
    {
        #region ModbusMaster离散输入
        //1:读取的站号
        //0:起始地址
        //5:读取长度
        var values = await master.ReadDiscreteInputsAsync(1, 0, 5);
        foreach (var value in values.Span)
        {
            Console.WriteLine(value);
        }
        #endregion

    }

    public static async Task ReadInputRegistersShouldBeOk(IModbusMaster master)
    {
        #region ModbusMaster读取输入寄存器
        //1:读取的站号
        //0:起始地址
        //5:读取长度
        var response = await master.ReadInputRegistersAsync(1, 0, 5);

        //获取原始数据
        var memory = response.Data;

        //或者从Span直接读取值
        var span = response.Data.Span;
        var value = span.ReadValue<ushort>(EndianType.Big);
        #endregion
    }

    /// <summary>
    /// Tcp协议的主站
    /// </summary>
    /// <returns></returns>
    public static async Task<IModbusMaster> GetModbusTcpMasterAsync()
    {
        #region 创建ModbusTcpMaster
        var client = new ModbusTcpMaster();

        var config = new TouchSocketConfig();
        config.SetRemoteIPHost("127.0.0.1:502");
        config.ConfigurePlugins(a =>
        {
            a.UseReconnection<ModbusTcpMaster>(options =>
            {
                options.PollingInterval = TimeSpan.FromSeconds(1);
            });
        });

        await client.SetupAsync(config);
        await client.ConnectAsync();
        #endregion

        return client;
    }

    /// <summary>
    /// Udp协议的主站
    /// </summary>
    /// <returns></returns>
    public static async Task<IModbusMaster> GetModbusUdpMaster()
    {
        #region 创建ModbusUdpMaster
        var client = new ModbusUdpMaster();
        await client.SetupAsync(new TouchSocketConfig()
             .UseUdpReceive()
             .SetRemoteIPHost("127.0.0.1:502"));
        await client.StartAsync();
        #endregion

        return client;
    }

    /// <summary>
    /// 串口协议的主站
    /// </summary>
    /// <returns></returns>
    public static async Task<IModbusMaster> GetModbusRtuMaster()
    {
        #region 创建ModbusRtuMaster
        var client = new ModbusRtuMaster();
        await client.SetupAsync(new TouchSocketConfig()
             .SetSerialPortOption(options =>
             {
                 options.BaudRate = 9600;
                 options.DataBits = 8;
                 options.Parity = System.IO.Ports.Parity.Even;
                 options.PortName = "COM2";
                 options.StopBits = System.IO.Ports.StopBits.One;
             }));
        await client.ConnectAsync();
        #endregion

        return client;
    }

    /// <summary>
    /// 基于Tcp协议，但使用Rtu的主站
    /// </summary>
    /// <returns></returns>
    public static async Task<IModbusMaster> GetModbusRtuOverTcpMaster()
    {
        #region 创建ModbusRtuOverTcpMaster
        var client = new ModbusRtuOverTcpMaster();
        await client.ConnectAsync("127.0.0.1:502");
        #endregion

        return client;
    }

    /// <summary>
    /// 基于Udp协议，但使用Rtu的主站
    /// </summary>
    /// <returns></returns>
    public static async Task<IModbusMaster> GetModbusRtuOverUdpMaster()
    {
        #region 创建ModbusRtuOverUdpMaster
        var client = new ModbusRtuOverUdpMaster();
        await client.SetupAsync(new TouchSocketConfig()
             .UseUdpReceive()
             .SetRemoteIPHost("127.0.0.1:502"));
        await client.StartAsync();
        #endregion

        return client;
    }
}

internal class MyModbusMaster : DependencyObject, IModbusMaster
{
    #region ModbusMaster原生接口实现
    public Task<IModbusResponse> SendModbusRequestAsync(ModbusRequest request, CancellationToken token)
    {
        throw new NotImplementedException();
    }
    #endregion
}

internal class MyClass
{
    public int P1 { get; set; }
    public int P2 { get; set; }
}