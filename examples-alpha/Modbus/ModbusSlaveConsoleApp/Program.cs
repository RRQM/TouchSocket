using TouchSocket.Core;
using TouchSocket.Modbus;
using TouchSocket.SerialPorts;
using TouchSocket.Sockets;

namespace ModbusSlaveConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                Enterprise.ForTest();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            var service = CreateModbusTcpSlave();

            Console.ReadKey();
        }

        private static ModbusTcpSlave CreateModbusTcpSlave()
        {
            var service = new ModbusTcpSlave();
            service.SetupAsync(new TouchSocketConfig()
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
            service.StartAsync();
            Console.WriteLine("服务已启动");

            //var modbusSlavePoint = service.GetSlavePointBySlaveId(slaveId: 1);
            //var localMaster = modbusSlavePoint.ModbusDataLocater.CreateDataLocaterMaster();
            //var coils = localMaster.ReadCoils(0, 1);
            return service;
        }

        private static ModbusRtuOverTcpSlave CreateModbusRtuOverTcpSlave()
        {
            var service = new ModbusRtuOverTcpSlave();
            service.SetupAsync(new TouchSocketConfig()
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
            service.StartAsync();
            Console.WriteLine("服务已启动");
            return service;
        }

        private static ModbusUdpSlave CreateModbusUdpSlave()
        {
            var service = new ModbusUdpSlave();
            service.SetupAsync(new TouchSocketConfig()
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
            service.StartAsync();
            Console.WriteLine("服务已启动");
            return service;
        }

        private static ModbusRtuOverUdpSlave CreateModbusRtuOverUdpSlave()
        {
            var service = new ModbusRtuOverUdpSlave();
            service.SetupAsync(new TouchSocketConfig()
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
            service.StartAsync();
            Console.WriteLine("服务已启动");
            return service;
        }

        private static ModbusRtuSlave CreateModbusRtuSlave()
        {
            var service = new ModbusRtuSlave();
            service.SetupAsync(new TouchSocketConfig()
                //设置串口
                .SetSerialPortOption(new SerialPortOption()
                {
                    BaudRate = 9600,
                    DataBits = 8,
                    Parity = System.IO.Ports.Parity.Even,
                    PortName = "COM1",
                    StopBits = System.IO.Ports.StopBits.One
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

            service.ConnectAsync();
            Console.WriteLine("已连接COM端口");
            return service;
        }
    }

    internal class MyModbusSlavePlugin : PluginBase, IModbusSlaveExecutingPlugin, IModbusSlaveExecutedPlugin
    {
        public async Task OnModbusSlaveExecuted(IModbusSlavePoint sender, ModbusSlaveExecutedEventArgs e)
        {
            await Console.Out.WriteLineAsync("slavea操作数据完成");
            await e.InvokeNext();
        }

        public async Task OnModbusSlaveExecuting(IModbusSlavePoint sender, ModbusSlaveExecutingEventArgs e)
        {
            //当想要拒绝操作时，可以将IsPermitOperation = false，并且e.ErrorCode可以携带返回错误码。
            //e.IsPermitOperation = false;
            //e.ErrorCode = ModbusErrorCode.ExecuteError;
            await Console.Out.WriteLineAsync("slavea操作数据");
            await e.InvokeNext();
        }
    }
}