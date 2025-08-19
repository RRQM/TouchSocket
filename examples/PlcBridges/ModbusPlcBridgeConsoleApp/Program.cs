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

using TouchSocket.Core;
using TouchSocket.Modbus;
using TouchSocket.PlcBridges;
using TouchSocket.SerialPorts;
using TouchSocket.Sockets;

namespace ModbusPlcBridgeConsoleApp;

#region 代码测试 {1,3,5,7,9,10-20}
internal class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            //此功能是Pro版本的功能，如果您有Pro版本的授权，请在此处进行授权。
            Enterprise.ForTest();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        //测试要求：
        //请在测试之前，确保您已经安装了Modbus设备模拟器。
        //并且已经启动了Modbus设备模拟器。其中需要：
        // 1.一个Tcp协议的Modbus设备，地址从0开始，寄存器数量为20个。同时还有地址从50开始，寄存器数量为20个（可以一次性设置70个寄存器）。
        // 2.一个Udp协议的Modbus设备，地址从10开始，寄存器数量为20个（可以一次性设置30个寄存器）。
        // 3.一个串口协议的Modbus设备，地址从20开始，寄存器数量为20个（可以一次性设置40个寄存器）。


        var plcBridge = new PlcBridgeService();
        await plcBridge.SetupAsync(new TouchSocketConfig());

        //现在假设以下情况：
        // 1.一个Tcp协议Modbus设备，地址从0开始，寄存器数量为20个。同时还有地址从50开始，寄存器数量为20个。
        // 2.一个Udp协议Modbus设备，地址从10开始，寄存器数量为20个。
        // 3.一个串口协议Modbus设备，地址从20开始，寄存器数量为20个。

        //我们接下来使用PlcBridgeService来桥接这些设备。

        // 目前计划：
        // Tcp设备映射到桥接地址[0-40)中。
        // Udp设备映射到桥接地址[40-60)中。
        // 串口设备映射到桥接地址[60-80)中。

        // 1) 首先，对于Tcp协议的Modbus设备，我们需要先初始化连接器。

        var modbusTcpMaster = new ModbusTcpMaster();
        await modbusTcpMaster.ConnectAsync("127.0.0.1:502");

        // 2) 创建一个ModbusHoldingRegistersDrive来桥接Tcp设备的[0-20)的地址。
        var plcDrive1 = new MyModbusHoldingRegistersDrive(modbusTcpMaster, new ModbusDriveOption()
        {
            Start = 0,
            Count = 20,
            //Group = "Group",
            Name = "TcpDevice1",
            SlaveId = 1, // Modbus从站ID
            ModbusStart = 0,// Modbus寄存器设备起始地址
        });
        await plcBridge.AddDriveAsync(plcDrive1);

        // 3) 创建一个ModbusHoldingRegistersDrive来桥接Tcp设备的[50-70)的地址。
        // 注意！
        // 我们这里对于同一个设备的不同地址段，可以创建多个驱动。
        // 如果设备不支持并发读取，我们可以通过配置Group来实现串行操作。
        var plcDrive2 = new MyModbusHoldingRegistersDrive(modbusTcpMaster, new ModbusDriveOption()
        {
            Start = 20,
            Count = 20,
            //Group = "Group",
            Name = "TcpDevice2",
            SlaveId = 1, // Modbus从站ID
            ModbusStart = 50,// Modbus寄存器设备起始地址
        });
        await plcBridge.AddDriveAsync(plcDrive2);

        // 4) 对于Udp协议的Modbus设备，我们同样需要先初始化连接器。
        var modbusUdpMaster = new ModbusUdpMaster();
        await modbusUdpMaster.SetupAsync(new TouchSocketConfig()
             .UseUdpReceive()
             .SetRemoteIPHost("127.0.0.1:503"));
        await modbusUdpMaster.StartAsync();

        // 5) 创建一个ModbusHoldingRegistersDrive来桥接Udp设备的[10-30)的地址。
        var plcDrive3 = new MyModbusHoldingRegistersDrive(modbusUdpMaster, new ModbusDriveOption()
        {
            Start = 40,
            Count = 20,
            //Group = "Group",
            Name = "UdpDevice1",
            SlaveId = 1, // Modbus从站ID
            ModbusStart = 10,// Modbus寄存器设备起始地址
        });
        await plcBridge.AddDriveAsync(plcDrive3);

        // 6) 对于串口协议的Modbus设备，我们同样需要先初始化连接器。
        var modbusRtuMaster = new ModbusRtuMaster();
        await modbusRtuMaster.SetupAsync(new TouchSocketConfig()
             .SetSerialPortOption(new SerialPortOption()
             {
                 BaudRate = 9600,
                 DataBits = 8,
                 Parity = System.IO.Ports.Parity.Even,
                 PortName = "COM2",
                 StopBits = System.IO.Ports.StopBits.One
             }));
        await modbusRtuMaster.ConnectAsync();

        // 7) 创建一个ModbusHoldingRegistersDrive来桥接串口设备的[20-40)的地址。
        var plcDrive4 = new MyModbusHoldingRegistersDrive(modbusRtuMaster, new ModbusDriveOption()
        {
            Start = 60,
            Count = 20,
            //Group = "Group",
            Name = "SerialDevice1",
            SlaveId = 1, // Modbus从站ID
            ModbusStart = 20,// Modbus寄存器设备起始地址
        });
        await plcBridge.AddDriveAsync(plcDrive4);

        // 8) 启动PLC桥接服务
        await plcBridge.StartAsync();

        var modbusResponse=await modbusTcpMaster.ReadHoldingRegistersAsync(0,70);

        var plcOperator = plcBridge.CreateOperator<short>();
        var result = await plcOperator.ReadAsync(0, 80);

        // 9) 现在我们可以进行读写操作了。一般来说可以使用操作器，直接读写。
        // 但我们这里直接使用PlcObject来进行读写。

        MyPlcObject myPlcObject = new MyPlcObject(plcBridge);

        // 写入long类型的寄存器数据
        var setInt64Result = await myPlcObject.SetInt64DataAsync(1000);
        Console.WriteLine($"写入Int64结束，结果：{setInt64Result}");

        // 读取long类型的寄存器数据
        var readInt64Result = await myPlcObject.GetInt64DataAsync();
        Console.WriteLine($"读取Int64结束，结果：{readInt64Result}");

        var data = Enumerable.Range(1, 80).Select(i => (short)i).ToArray();

        //写入所有的80个寄存器数据
        var setAllInt16Result = await myPlcObject.SetAllInt16DataAsync(data);

        Console.WriteLine($"写入所有Int16结束，结果：{setAllInt16Result}");
        // 读取所有的80个寄存器数据
        var readAllInt16Result = await myPlcObject.GetAllInt16DataAsync();

        Console.WriteLine($"读取Int64结束，结果：{readAllInt16Result.ToResult()}，Values={readAllInt16Result.Value.ToArray().ToJsonString()}");

        //读取所有的80个寄存器数据，注意：80个寄存器会被合并成20个long类型的值。
        var readAllInt64Result = await myPlcObject.GetAllInt64DataAsync();
        Console.WriteLine($"读取所有Int64结束，结果：{readAllInt64Result.ToResult()}，Values={readAllInt64Result.Value.ToArray().ToJsonString()}");

        // 10) 最后，记得释放资源
        await plcBridge.StopAsync();

        await modbusTcpMaster.CloseAsync();
        await modbusUdpMaster.StopAsync();
        await modbusRtuMaster.CloseAsync();
        modbusTcpMaster.Dispose();
        modbusUdpMaster.Dispose();
        modbusRtuMaster.Dispose();

        plcBridge.Dispose();
        Console.ReadKey();
    }
}

partial class MyPlcObject : PlcObject
{
    public MyPlcObject(IPlcBridgeService bridgeService) : base(bridgeService)
    {
    }

    /// <summary>
    /// 以<see cref="short"/>类型读取所有的80个寄存器。
    /// </summary>
    [PlcField<short>(Start = 0, Quantity = 80)]
    private ReadOnlyMemory<short> m_allInt16Data;

    /// <summary>
    /// 以<see cref="long"/>类型读取所有的80个寄存器。注意：80个寄存器会被合并成20个<see cref="long"/>类型的值。
    /// </summary>
    [PlcField<short>(Start = 0, Quantity = 20)]
    private ReadOnlyMemory<long> m_allInt64Data;

    /// <summary>
    /// 此处测试一个<see cref="long"/>类型的读取。地址从59开始，长度为4个寄存器。读取正好跨越Udp和串口设备的地址范围。
    /// </summary>
    [PlcField<short>(Start = 59)]
    private long m_int64Data;
}

class MyModbusHoldingRegistersDrive : ModbusHoldingRegistersDrive
{
    public MyModbusHoldingRegistersDrive(IModbusMaster master, ModbusDriveOption option) : base(master, option)
    {
    }

    protected override async Task<Result> ExecuteReadAsync(ExecuteReadableValue<short> readableValue, CancellationToken token)
    {
        // 在这里可以添加一些自定义的逻辑，例如日志记录、异常处理等。

        var result = await base.ExecuteReadAsync(readableValue, token);

        Console.WriteLine($"设备类型={this.Master.GetType().Name}，读取地址={readableValue.Start + this.ModbusStart}，长度= {readableValue.Count}的数据，结果：{result.ToJsonString()}");
        return result;
    }

    protected override async Task<Result> ExecuteWriteAsync(WritableValue<short> writableValue, CancellationToken token)
    {
        var result = await base.ExecuteWriteAsync(writableValue, token);

        Console.WriteLine($"设备类型={this.Master.GetType().Name}，写入地址={writableValue.Start + this.ModbusStart}，长度={writableValue.Count}的数据，结果：{result.ToJsonString()}");
        return result;
    }
}
#endregion