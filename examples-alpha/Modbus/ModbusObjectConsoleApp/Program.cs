using TouchSocket.Core;
using TouchSocket.Modbus;
using TouchSocket.Sockets;

namespace ModbusObjectConsoleApp
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

            var Slave = CreateModbusTcpSlave();

            var master = GetModbusTcpMaster();

            var myModbusObject = master.CreateModbusObject<MyModbusObject>();

            myModbusObject.MyProperty1 = true;//直接赋值线圈
            myModbusObject.MyProperty11 = new bool[] { false, true, false, true, false, true, false, true, false };//直接赋值多线圈

            myModbusObject.MyProperty3 = 1;//直接赋值保持寄存器
            myModbusObject.MyProperty33 = new short[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };//直接赋值保持寄存器

            Console.WriteLine(myModbusObject.MyProperty1.ToJsonString());
            Console.WriteLine(myModbusObject.MyProperty11.ToJsonString());
            Console.WriteLine(myModbusObject.MyProperty2.ToJsonString());
            Console.WriteLine(myModbusObject.MyProperty22.ToJsonString());
            Console.WriteLine(myModbusObject.MyProperty3.ToJsonString());
            Console.WriteLine(myModbusObject.MyProperty33.ToJsonString());
            Console.WriteLine(myModbusObject.MyProperty4.ToJsonString());
            Console.WriteLine(myModbusObject.MyProperty44.ToJsonString());
            Console.ReadKey();
        }

        /// <summary>
        /// Tcp从站
        /// </summary>
        /// <returns></returns>
        private static ModbusTcpSlave CreateModbusTcpSlave()
        {
            var service = new ModbusTcpSlave();
            service.SetupAsync(new TouchSocketConfig()
                //监听端口
                .SetListenIPHosts(7808)
                .ConfigurePlugins(a =>
                {
                    a.AddModbusSlavePoint()//添加一个从站站点
                    .SetSlaveId(1)//设置站点号
                    .UseIgnoreSlaveId()//忽略站号验证
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
            return service;
        }

        /// <summary>
        /// Tcp协议的主站
        /// </summary>
        /// <returns></returns>
        public static IModbusTcpMaster GetModbusTcpMaster()
        {
            var client = new ModbusTcpMaster();

            client.ConnectAsync("127.0.0.1:7808");
            return client;
        }
    }

    internal class MyModbusObject : ModbusObject
    {
        #region Coils

        /// <summary>
        /// 声明一个来自线圈的bool属性。
        /// <para>
        /// 配置：站号、数据区、起始地址、超时时间
        /// </para>
        /// </summary>
        [ModbusProperty(SlaveId = 1, Partition = Partition.Coils, StartAddress = 1000, Timeout = 1000)]
        public bool MyProperty1
        {
            get { return this.GetValue<bool>(); }
            set { this.SetValue(value); }
        }

        /// <summary>
        /// 声明一个来自线圈的bool数组属性。
        /// <para>
        /// 配置：站号、数据区、起始地址、超时时间、数量
        /// </para>
        /// </summary>
        [ModbusProperty(SlaveId = 1, Partition = Partition.Coils, StartAddress = 1001, Timeout = 1000, Quantity = 9)]
        public bool[] MyProperty11
        {
            get { return this.GetValueArray<bool>(); }
            set { this.SetValueArray(value); }
        }

        #endregion Coils

        #region DiscreteInputs

        /// <summary>
        /// 声明一个来自离散输入的bool属性。
        /// <para>
        /// 配置：站号、数据区、起始地址、超时时间
        /// </para>
        /// </summary>
        [ModbusProperty(SlaveId = 1, Partition = Partition.DiscreteInputs, StartAddress = 1000, Timeout = 1000)]
        public bool MyProperty2
        {
            get { return this.GetValue<bool>(); }
        }

        /// <summary>
        /// 声明一个来自离散输入的bool数组属性。
        /// <para>
        /// 配置：站号、数据区、起始地址、超时时间、数量
        /// </para>
        /// </summary>
        [ModbusProperty(SlaveId = 1, Partition = Partition.DiscreteInputs, StartAddress = 1001, Timeout = 1000, Quantity = 9)]
        public bool MyProperty22
        {
            get { return this.GetValue<bool>(); }
        }

        #endregion DiscreteInputs

        #region HoldingRegisters

        /// <summary>
        /// 声明一个来自保持寄存器的short属性。
        /// <para>
        /// 配置：站号、数据区、起始地址、超时时间、端序
        /// </para>
        /// </summary>
        [ModbusProperty(SlaveId = 1, Partition = Partition.HoldingRegisters, StartAddress = 1000, Timeout = 1000, EndianType = EndianType.Big)]
        public short MyProperty3
        {
            get { return this.GetValue<short>(); }
            set { this.SetValue(value); }
        }

        /// <summary>
        /// 声明一个来自保持寄存器的short数组属性。
        /// <para>
        /// 配置：站号、数据区、起始地址、超时时间、端序、数组长度
        /// </para>
        /// </summary>
        [ModbusProperty(SlaveId = 1, Partition = Partition.HoldingRegisters, StartAddress = 1001, Timeout = 1000, EndianType = TouchSocket.Core.EndianType.Big, Quantity = 9)]
        public short[] MyProperty33
        {
            get { return this.GetValueArray<short>(); }
            set { this.SetValueArray(value); }
        }

        #endregion HoldingRegisters

        #region InputRegisters

        /// <summary>
        /// 声明一个来自输入寄存器的short属性。
        /// <para>
        /// 配置：站号、数据区、起始地址、超时时间、端序
        /// </para>
        /// </summary>
        [ModbusProperty(SlaveId = 1, Partition = Partition.InputRegisters, StartAddress = 1000, Timeout = 1000, EndianType = EndianType.Big)]
        public short MyProperty4
        {
            get { return this.GetValue<short>(); }
        }

        /// <summary>
        /// 声明一个来自输入寄存器的short数组属性。
        /// <para>
        /// 配置：站号、数据区、起始地址、超时时间、端序、数组长度
        /// </para>
        /// </summary>
        [ModbusProperty(SlaveId = 1, Partition = Partition.InputRegisters, StartAddress = 1000, Timeout = 1000, EndianType = EndianType.Big, Quantity = 10)]
        public short[] MyProperty44
        {
            get { return this.GetValueArray<short>(); }
        }

        #endregion InputRegisters
    }
}