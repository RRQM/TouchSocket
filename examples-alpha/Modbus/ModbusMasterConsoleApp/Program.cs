using TouchSocket.Core;
using TouchSocket.Modbus;
using TouchSocket.SerialPorts;
using TouchSocket.Sockets;

namespace ModbusClientConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var client = GetModbusTcpMaster();

            ReadWriteHoldingRegisters(client);
            Console.ReadKey();
        }

        /// <summary>
        /// 要测试，请打开Modbus Slave软件，设置HoldingRegisters。至少30个长度。
        /// </summary>
        public static void ReadWriteHoldingRegisters(IModbusMaster client)
        {
            //写入单个寄存器
            client.WriteSingleRegister(1, 0, 1);//默认short ABCD端序
            client.WriteSingleRegister(1, 1, 1000);//默认short ABCD端序

            using (var valueByteBlock = new ValueByteBlock(1024))
            {
                valueByteBlock.Write((ushort)2, EndianType.Big);//ABCD端序
                valueByteBlock.Write((ushort)2000, EndianType.Little);//DCBA端序
                valueByteBlock.Write(int.MaxValue, EndianType.BigSwap);//BADC端序
                valueByteBlock.Write(long.MaxValue, EndianType.LittleSwap);//CDAB端序

                //写入字符串，会先用4字节表示字符串长度，然后按utf8编码写入字符串
                valueByteBlock.Write("Hello");

                //写入到寄存器
                client.WriteMultipleRegisters(1, 2, valueByteBlock.ToArray());
            }

            //读取寄存器
            var response = client.ReadHoldingRegisters(1, 0, 30);

            //创建一个读取器
            var reader = response.CreateReader();

            Console.WriteLine(reader.ReadInt16(EndianType.Big));
            Console.WriteLine(reader.ReadInt16(EndianType.Big));
            Console.WriteLine(reader.ReadInt16(EndianType.Big));
            Console.WriteLine(reader.ReadInt16(EndianType.Little));
            Console.WriteLine(reader.ReadInt32(EndianType.BigSwap));
            Console.WriteLine(reader.ReadInt64(EndianType.LittleSwap));
            Console.WriteLine(reader.ReadString());
        }

        /// <summary>
        /// 读写线圈，在测试时，请选择对应的Modbus Slave类型，且调到线圈操作，至少5个长度
        /// </summary>
        /// <param name="client"></param>
        public static void ReadWriteCoilsShouldBeOk(IModbusMaster client)
        {
            //写单个线圈
            client.WriteSingleCoil(1, 0, true);
            client.WriteSingleCoil(1, 1, false);

            //写多个线圈
            client.WriteMultipleCoils(1, 2, new bool[] { true, false, true });

            //读取线圈
            var values = client.ReadCoils(1, 0, 5);
            foreach (var value in values)
            {
                Console.WriteLine(value);
            }
        }

        /// <summary>
        /// Tcp协议的主站
        /// </summary>
        /// <returns></returns>
        public static IModbusTcpMaster GetModbusTcpMaster()
        {
            var client = new ModbusTcpMaster();

            client.ConnectAsync("127.0.0.1:502");
            return client;
        }

        /// <summary>
        /// Udp协议的主站
        /// </summary>
        /// <returns></returns>
        public static IModbusMaster GetModbusUdpMaster()
        {
            var client = new ModbusUdpMaster();
            client.SetupAsync(new TouchSocketConfig()
                .UseUdpReceive()
                .SetRemoteIPHost("127.0.0.1:502"));
            client.StartAsync();
            return client;
        }

        /// <summary>
        /// 串口协议的主站
        /// </summary>
        /// <returns></returns>
        public static IModbusMaster GetModbusRtuMaster()
        {
            var client = new ModbusRtuMaster();
            client.SetupAsync(new TouchSocketConfig()
                .SetSerialPortOption(new SerialPortOption()
                {
                    BaudRate = 9600,
                    DataBits = 8,
                    Parity = System.IO.Ports.Parity.Even,
                    PortName = "COM2",
                    StopBits = System.IO.Ports.StopBits.One
                }));
            client.ConnectAsync();
            return client;
        }

        /// <summary>
        /// 基于Tcp协议，但使用Rtu的主站
        /// </summary>
        /// <returns></returns>
        public static IModbusMaster GetModbusRtuOverTcpMaster()
        {
            var client = new ModbusRtuOverTcpMaster();
            client.ConnectAsync("127.0.0.1:502");
            return client;
        }

        /// <summary>
        /// 基于Udp协议，但使用Rtu的主站
        /// </summary>
        /// <returns></returns>
        public static IModbusMaster GetModbusRtuOverUdpMaster()
        {
            var client = new ModbusRtuOverUdpMaster();
            client.SetupAsync(new TouchSocketConfig()
                .UseUdpReceive()
                .SetRemoteIPHost("127.0.0.1:502"));
            client.StartAsync();
            return client;
        }
    }

    internal class MyClass
    {
        public int P1 { get; set; }
        public int P2 { get; set; }
    }
}