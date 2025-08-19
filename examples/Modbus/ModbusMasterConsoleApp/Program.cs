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
using TouchSocket.SerialPorts;
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
        //写入单个寄存器
        await master.WriteSingleRegisterAsync(1, 0, 1);//默认short ABCD端序
        await master.WriteSingleRegisterAsync(1, 1, 1000);//默认short ABCD端序

   
        var valueByteBlock = new ValueByteBlock(1024);
        try
        {
            WriterExtension.WriteValue(ref valueByteBlock, (ushort)2, EndianType.Big);//ABCD端序
            WriterExtension.WriteValue(ref valueByteBlock, (ushort)2000, EndianType.Little);//DCBA端序
            WriterExtension.WriteValue(ref valueByteBlock, (int)int.MaxValue, EndianType.BigSwap);//BADC端序
            WriterExtension.WriteValue(ref valueByteBlock, (long)long.MaxValue, EndianType.LittleSwap);//CDAB端序

            //写入字符串，会先用4字节表示字符串长度，然后按utf8编码写入字符串
            WriterExtension.WriteString(ref valueByteBlock, (string)"Hello1");

            //如果想要直接写入字符串，可以使用WriteNormalString方法
            //valueByteBlock.WriteNormalString("Hello1", System.Text.Encoding.UTF8);

            //注意：写入字符串时，应当保证写入后的字节总数为双数。如果是单数，则会报错。

            //写入到寄存器
            await master.WriteMultipleRegistersAsync(1, 2, valueByteBlock.ToArray());
        }
        finally
        {
            valueByteBlock.Dispose();
        }

        //读取寄存器
        var response = await master.ReadHoldingRegistersAsync(1, 0, 30);

        //创建一个读取器
        var span= response.Data.Span;

        Console.WriteLine(span.ReadValue<ushort>(EndianType.Big));
        Console.WriteLine(span.ReadValue<ushort>(EndianType.Big));
        Console.WriteLine(span.ReadValue<ushort>(EndianType.Big));
        Console.WriteLine(span.ReadValue<ushort>(EndianType.Little));
        Console.WriteLine(span.ReadValue<int>(EndianType.BigSwap));
        Console.WriteLine(span.ReadValue<long>(EndianType.LittleSwap));
        Console.WriteLine(span.ReadString());
    }

    /// <summary>
    /// 读写线圈，在测试时，请选择对应的Modbus Slave类型，且调到线圈操作，至少5个长度
    /// </summary>
    /// <param name="master"></param>
    public static async Task ReadWriteCoilsShouldBeOk(IModbusMaster master)
    {
        //写单个线圈
        await master.WriteSingleCoilAsync(1, 0, true);
        await master.WriteSingleCoilAsync(1, 1, false);

        //写多个线圈
        await master.WriteMultipleCoilsAsync(1, 2, new bool[] { true, false, true });

        //读取线圈
        var values = await master.ReadCoilsAsync(1, 0, 5);
        foreach (var value in values.Span)
        {
            Console.WriteLine(value);
        }
    }

    /// <summary>
    /// Tcp协议的主站
    /// </summary>
    /// <returns></returns>
    public static async Task<IModbusMaster> GetModbusTcpMasterAsync()
    {
        var client = new ModbusTcpMaster();
        await client.SetupAsync(new TouchSocketConfig()
             .ConfigurePlugins(a =>
             {
                 a.UseModbusTcpMasterReconnectionPlugin()
                        .UsePolling(TimeSpan.FromSeconds(1));
             }));
        await client.ConnectAsync("127.0.0.1:502");
        return client;
    }

    /// <summary>
    /// Udp协议的主站
    /// </summary>
    /// <returns></returns>
    public static async Task<IModbusMaster> GetModbusUdpMaster()
    {
        var client = new ModbusUdpMaster();
        await client.SetupAsync(new TouchSocketConfig()
             .UseUdpReceive()
             .SetRemoteIPHost("127.0.0.1:502"));
        await client.StartAsync();
        return client;
    }

    /// <summary>
    /// 串口协议的主站
    /// </summary>
    /// <returns></returns>
    public static async Task<IModbusMaster> GetModbusRtuMaster()
    {
        var client = new ModbusRtuMaster();
        await client.SetupAsync(new TouchSocketConfig()
             .SetSerialPortOption(new SerialPortOption()
             {
                 BaudRate = 9600,
                 DataBits = 8,
                 Parity = System.IO.Ports.Parity.Even,
                 PortName = "COM2",
                 StopBits = System.IO.Ports.StopBits.One
             }));
        await client.ConnectAsync();
        return client;
    }

    /// <summary>
    /// 基于Tcp协议，但使用Rtu的主站
    /// </summary>
    /// <returns></returns>
    public static async Task<IModbusMaster> GetModbusRtuOverTcpMaster()
    {
        var client = new ModbusRtuOverTcpMaster();
        await client.ConnectAsync("127.0.0.1:502");
        return client;
    }

    /// <summary>
    /// 基于Udp协议，但使用Rtu的主站
    /// </summary>
    /// <returns></returns>
    public static async Task<IModbusMaster> GetModbusRtuOverUdpMaster()
    {
        var client = new ModbusRtuOverUdpMaster();
        await client.SetupAsync(new TouchSocketConfig()
             .UseUdpReceive()
             .SetRemoteIPHost("127.0.0.1:502"));
        await client.StartAsync();
        return client;
    }
}

internal class MyClass
{
    public int P1 { get; set; }
    public int P2 { get; set; }
}

public static class MasterReconnectionPluginExtension
{
    public static ReconnectionPlugin<IModbusTcpMaster> UseModbusTcpMasterReconnectionPlugin(this IPluginManager pluginManager)
    {
        ModbusTcpMasterReconnectionPlugin modbusTcpMasterReconnectionPlugin = new ModbusTcpMasterReconnectionPlugin();
        pluginManager.Add(modbusTcpMasterReconnectionPlugin);
        return modbusTcpMasterReconnectionPlugin;
    }
}

internal sealed class ModbusTcpMasterReconnectionPlugin : ReconnectionPlugin<IModbusTcpMaster>, ITcpClosedPlugin
{
    public override Func<IModbusTcpMaster, int, Task<bool?>> ActionForCheck { get; set; }

    public ModbusTcpMasterReconnectionPlugin()
    {
        this.ActionForCheck = (c, i) => Task.FromResult<bool?>(c.Online);
    }

    public async Task OnTcpClosed(ITcpSession client, ClosedEventArgs e)
    {
        await e.InvokeNext().ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        if (client is not IModbusTcpMaster tClient)
        {
            return;
        }

        if (e.Manual)
        {
            return;
        }

        _ = Task.Run(async () =>
        {
            while (true)
            {
                if (this.DisposedValue)
                {
                    return;
                }
                if (await this.ActionForConnect.Invoke(tClient).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
                {
                    return;
                }
            }
        });
    }
}