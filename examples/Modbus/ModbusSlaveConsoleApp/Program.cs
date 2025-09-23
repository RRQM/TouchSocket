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

namespace ModbusSlaveConsoleApp;

internal class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            Enterprise.ForTest();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        var service = await CreateModbusTcpSlaveAsync();

        Console.ReadKey();
    }

    private static async Task<ModbusTcpSlave> CreateModbusTcpSlaveAsync()
    {
        var service = new ModbusTcpSlave();
        await service.SetupAsync(new TouchSocketConfig()
             //监听端口
             .SetListenIPHosts(7808)
             .ConfigurePlugins(a =>
             {
                 a.Add<MyModbusSlavePlugin>();

                 //当添加多个站点时，需要禁用IgnoreSlaveId的设定

                 a.AddModbusSlavePoint()//添加一个从站站点
                 .SetSlaveId(1)//设置站点号
                               //.UseIgnoreSlaveId()//忽略站号验证
                 .SetModbusDataLocater(new ModbusDataLocater(10, 10, 10, 10));//设置数据区

                 a.AddModbusSlavePoint()//再添加一个从站站点
                 .SetSlaveId(2)//设置站点号
                               //.UseIgnoreSlaveId()//忽略站号验证
                 .SetModbusDataLocater(new ModbusDataLocater()//设置数据区
                 {
                     //下列配置表示，起始地址从1000开始，10个长度
                     Coils = new BooleanDataPartition(1000, 10),
                     DiscreteInputs = new BooleanDataPartition(1000, 10),
                     HoldingRegisters = new ShortDataPartition(1000, 10),
                     InputRegisters = new ShortDataPartition(1000, 10)
                 });
             })
             );
        await service.StartAsync();
        Console.WriteLine("服务已启动");

        //var modbusSlavePoint = service.GetSlavePointBySlaveId(slaveId: 1);
        //var localMaster = modbusSlavePoint.ModbusDataLocater.CreateDataLocaterMaster();
        //var coils = localMaster.ReadCoils(0, 1);
        return service;
    }

    private static async Task<ModbusRtuOverTcpSlave> CreateModbusRtuOverTcpSlaveAsync()
    {
        var slave = new ModbusRtuOverTcpSlave();
        await slave.SetupAsync(new TouchSocketConfig()
              //监听端口
              .SetListenIPHosts(7810)
              .ConfigurePlugins(a =>
              {
                  a.Add<MyModbusSlavePlugin>();

                  a.AddModbusSlavePoint()//添加一个从站站点
                  .SetSlaveId(1)//设置站点号
                  .UseIgnoreSlaveId()//忽略站号验证
                  .SetModbusDataLocater(new ModbusDataLocater(10, 10, 10, 10));//设置数据区
              })
              );
        await slave.StartAsync();
        Console.WriteLine("服务已启动");
        return slave;
    }

    private static async Task<ModbusUdpSlave> CreateModbusUdpSlaveAsync()
    {
        var slave = new ModbusUdpSlave();
        await slave.SetupAsync(new TouchSocketConfig()
             //监听端口
             .SetBindIPHost(7809)
             .ConfigurePlugins(a =>
             {
                 a.Add<MyModbusSlavePlugin>();

                 a.AddModbusSlavePoint()//添加一个从站站点
                 .SetSlaveId(1)//设置站点号
                 .UseIgnoreSlaveId()//忽略站号验证
                 .SetModbusDataLocater(new ModbusDataLocater(10, 10, 10, 10));//设置数据区
             })
             );
        await slave.StartAsync();
        Console.WriteLine("服务已启动");
        return slave;
    }

    private static async Task<ModbusRtuOverUdpSlave> CreateModbusRtuOverUdpSlaveAsync()
    {
        var slave = new ModbusRtuOverUdpSlave();
        await slave.SetupAsync(new TouchSocketConfig()
             //监听端口
             .SetBindIPHost(7811)
             .ConfigurePlugins(a =>
             {
                 a.Add<MyModbusSlavePlugin>();

                 a.AddModbusSlavePoint()//添加一个从站站点
                 .SetSlaveId(1)//设置站点号
                 .UseIgnoreSlaveId()//忽略站号验证
                 .SetModbusDataLocater(new ModbusDataLocater(10, 10, 10, 10));//设置数据区
             })
             );
        await slave.StartAsync();
        Console.WriteLine("服务已启动");
        return slave;
    }

    private static async Task<ModbusRtuSlave> CreateModbusRtuSlaveAsync()
    {
        var slave = new ModbusRtuSlave();
        await slave.SetupAsync(new TouchSocketConfig()
             //设置串口
             .SetSerialPortOption(options =>
             {
                 options.BaudRate = 9600;
                 options.DataBits = 8;
                 options.Parity = System.IO.Ports.Parity.Even;
                 options.PortName = "COM1";
                 options.StopBits = System.IO.Ports.StopBits.One;
             })
             .ConfigurePlugins(a =>
             {
                 a.Add<MyModbusSlavePlugin>();

                 a.AddModbusSlavePoint()//添加一个从站站点
                 .SetSlaveId(1)//设置站点号
                               //.UseIgnoreSlaveId()//如果不调用，默认会进行站号验证
                 .SetModbusDataLocater(new ModbusDataLocater(10, 10, 10, 10));//设置数据区
             })
             );

        await slave.ConnectAsync();
        Console.WriteLine("已连接COM端口");
        return slave;
    }
}

internal class MyModbusSlavePlugin : PluginBase, IModbusSlaveExecutingPlugin, IModbusSlaveExecutedPlugin
{
    public async Task OnModbusSlaveExecuted(IModbusSlavePoint sender, ModbusSlaveExecutedEventArgs e)
    {
        await Console.Out.WriteLineAsync("slave操作数据完成");
        await e.InvokeNext();
    }

    public async Task OnModbusSlaveExecuting(IModbusSlavePoint sender, ModbusSlaveExecutingEventArgs e)
    {
        //当想要拒绝操作时，可以将IsPermitOperation = false，并且e.ErrorCode可以携带返回错误码。
        //e.IsPermitOperation = false;
        //e.ErrorCode = ModbusErrorCode.ExecuteError;
        await Console.Out.WriteLineAsync("slave操作数据");
        await e.InvokeNext();
    }
}