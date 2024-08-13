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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Modbus;
using TouchSocket.Sockets;

namespace XUnitTestProject.Modbus
{
    public class TestModbusRtuOverTcpMaster
    {
        public IModbusMaster GetMaster()
        {
            var client = new ModbusRtuOverTcpMaster();
            client.Connect("127.0.0.1:7810");
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

            //写单个线圈
            client.WriteSingleCoil(1,0, true);
            client.WriteSingleCoil(1,1, false);

            //写多个线圈
            client.WriteMultipleCoils(1,2, new bool[] { true, false, true });

            //读取线圈
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

            //写入单个寄存器
            client.WriteSingleRegister(1,0, 1);//默认ABCD端序
            client.WriteSingleRegister(1,1, 1000);//默认ABCD端序

            using (var valueByteBlock = new ValueByteBlock(1024))
            {
                valueByteBlock.Write((ushort)2, EndianType.Little);//DCBA端序
                valueByteBlock.Write((ushort)2000, EndianType.Little);//DCBA端序
                valueByteBlock.Write(int.MaxValue, EndianType.BigSwap);//BADC端序
                valueByteBlock.Write(long.MaxValue, EndianType.LittleSwap);//CDAB端序

                //写入多个寄存器
                client.WriteMultipleRegisters(1,2, valueByteBlock.ToArray());
            }

            //读取寄存器
            var response = client.ReadHoldingRegisters(1,0, 1 + 1 + 1 + 1 + 2 + 4);

            //读值
            var reader = response.CreateReader();
            Assert.Equal(1, reader.ReadInt16(EndianType.Big));
            Assert.Equal(1000, reader.ReadInt16(EndianType.Big));
            Assert.Equal(2, reader.ReadInt16(EndianType.Little));
            Assert.Equal(2000, reader.ReadInt16(EndianType.Little));
            Assert.Equal(int.MaxValue, reader.ReadInt32(EndianType.BigSwap));
            Assert.Equal(long.MaxValue, reader.ReadInt64(EndianType.LittleSwap));

            //或者一次性读多值
            //short[] values = reader.ToInt16s().ToArray();
        }
    }
}