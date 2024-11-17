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

namespace XUnitTestProject.Modbus
{
    public class TestModbusRtuMaster
    {
        public IModbusRtuMaster GetMaster()
        {
            var client = new ModbusRtuMaster();
            client.Setup(new TouchSocketConfig()
                .SetSerialPortOption(new SerialPortOption()
                {
                    BaudRate = 9600,
                    DataBits = 8,
                    Parity = System.IO.Ports.Parity.Even,
                    PortName = "COM2",
                    StopBits = System.IO.Ports.StopBits.One
                }));
            client.Connect();
            return client;
        }

        [Fact]
        public void ReadDiscreteInputs()
        {
            var client = this.GetMaster();
            var response = client.ReadDiscreteInputs(1,0, 10);

            foreach (var coil in response)
            {
            }
        }

        [Fact]
        public void ReadInputRegisters()
        {
            var client = this.GetMaster();
            var response = client.ReadInputRegisters(1,0, 10);

            Assert.True(response.ErrorCode == ModbusErrorCode.Success);
        }

        /// <summary>
        /// 要进行单元测试，请打开Modbus Slave软件，设置Coils。至少5个长度。
        /// </summary>
        [Fact]
        public void ReadWriteCoilsShouldBeOk()
        {
            var client = GetMaster();

            client.WriteSingleCoil(1,0, true);
            

            client.WriteSingleCoil(1,1, false);
            

            client.WriteMultipleCoils(1,2, new bool[] { true, false, true });
           

            var values = client.ReadCoils(1,0, 5);
            Assert.NotNull(values);
            Assert.Equal(5, values.Length);
            Assert.True(values[0]);
            Assert.False(values[1]);
            Assert.True(values[2]);
            Assert.False(values[3]);
            Assert.True(values[4]);
        }

        /// <summary>
        /// 要进行单元测试，请打开Modbus Slave软件，设置HoldingRegisters。至少10个长度。
        /// </summary>
        [Fact]
        public void ReadWriteHoldingRegisters()
        {
            var client = this.GetMaster();
            client.WriteSingleRegister(1,0, 1);
            client.WriteSingleRegister(1,1, 1000);

            using (var valueByteBlock = new ValueByteBlock(1024))
            {
                valueByteBlock.Write((ushort)2, EndianType.Little);
                valueByteBlock.Write((ushort)2000, EndianType.Little);
                valueByteBlock.Write(int.MaxValue, EndianType.BigSwap);
                valueByteBlock.Write(long.MaxValue, EndianType.LittleSwap);
                client.WriteMultipleRegisters(1,2, valueByteBlock.ToArray());
            }

            var response = client.ReadHoldingRegisters(1,0, 1 + 1 + 1 + 1 + 2 + 4);
            var reader = response.CreateReader();
            Assert.Equal(1, reader.ReadInt16(EndianType.Big));
            Assert.Equal(1000, reader.ReadInt16(EndianType.Big));
            Assert.Equal(2, reader.ReadInt16(EndianType.Little));
            Assert.Equal(2000, reader.ReadInt16(EndianType.Little));
            Assert.Equal(int.MaxValue, reader.ReadInt32(EndianType.BigSwap));
            Assert.Equal(long.MaxValue, reader.ReadInt64(EndianType.LittleSwap));
        }

        [Fact]
        public void ReadWriteHoldingRegistersFC23()
        {
            var client = this.GetMaster();

            using (var valueByteBlock = new ValueByteBlock(1024))
            {
                valueByteBlock.Write((short)1, EndianType.Big);
                valueByteBlock.Write((short)1000, EndianType.Big);
                valueByteBlock.Write((ushort)2, EndianType.Little);//DCBA端序
                valueByteBlock.Write((ushort)2000, EndianType.Little);//DCBA端序
                valueByteBlock.Write(int.MaxValue, EndianType.BigSwap);//BADC端序
                valueByteBlock.Write(long.MaxValue, EndianType.LittleSwap);//CDAB端序

                //写入多个寄存器
                var response = client.ReadWriteMultipleRegisters(1,0, 1 + 1 + 1 + 1 + 2 + 4, 0, valueByteBlock.ToArray());

                //读值
                var reader = response.CreateReader();
                Assert.Equal(1, reader.ReadInt16(EndianType.Big));
                Assert.Equal(1000, reader.ReadInt16(EndianType.Big));
                Assert.Equal(2, reader.ReadInt16(EndianType.Little));
                Assert.Equal(2000, reader.ReadInt16(EndianType.Little));
                Assert.Equal(int.MaxValue, reader.ReadInt32(EndianType.BigSwap));
                Assert.Equal(long.MaxValue, reader.ReadInt64(EndianType.LittleSwap));
            }
        }
    }
}