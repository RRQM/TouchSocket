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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Modbus;
using TouchSocket.Sockets;

namespace XUnitTestProject.Modbus
{
    public class TestTestModbusObject
    {
        public IModbusTcpMaster GetTcpMaster()
        {
            var client = new ModbusTcpMaster();

            client.Connect("127.0.0.1:7808");
            return client;
        }

        public IModbusMaster GetUdpMaster()
        {
            var client = new ModbusUdpMaster();
            client.Setup(new TouchSocketConfig()
                .UseUdpReceive()
                .SetRemoteIPHost("127.0.0.1:7809"));
            client.Start();
            return client;
        }

        [Fact]
        public void SetGetModbusObjectShouldBeOk()
        {
            var client = GetTcpMaster();
            var modbusObject = client.CreateModbusObject<MyModbusObject>();

            var bools = new bool[] { false, true, false, true, false, true, false, true, false };
            var shorts = new short[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            modbusObject.MyProperty1 = true;//直接赋值线圈
            modbusObject.MyProperty11 = bools;//直接赋值多线圈

            modbusObject.MyProperty3 = short.MaxValue;//直接赋值保持寄存器
            modbusObject.MyProperty33 = shorts; //直接赋值保持寄存器

            Assert.True(modbusObject.MyProperty1==true);
            Assert.True(modbusObject.MyProperty11.SequenceEqual(bools));

            Assert.True(modbusObject.MyProperty3==short.MaxValue);
            Assert.True(modbusObject.MyProperty33.SequenceEqual(shorts));
        }

        [Fact]
        public void SetGetModbusObjectShouldBeOk2()
        {
            var client = GetUdpMaster();
            var modbusObject = client.CreateModbusObject<MyModbusObject2>();

            var bools = new bool[] { false, true, false, true, false, true, false, true, false };
            var shorts = new short[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            modbusObject.MyProperty1 = true;//直接赋值线圈
            modbusObject.MyProperty11 = bools;//直接赋值多线圈

            modbusObject.MyProperty3 = short.MaxValue;//直接赋值保持寄存器
            modbusObject.MyProperty33 = shorts; //直接赋值保持寄存器

            Assert.True(modbusObject.MyProperty1 == true);
            Assert.True(modbusObject.MyProperty11.SequenceEqual(bools));

            Assert.True(modbusObject.MyProperty3 == short.MaxValue);
            Assert.True(modbusObject.MyProperty33.SequenceEqual(shorts));
        }
    }

    class MyModbusObject : ModbusObject
    {
        #region Coils
        /// <summary>
        /// 声明一个来自线圈的bool属性。
        /// <para>
        /// 配置：站号、数据区、起始地址、超时时间
        /// </para>
        /// </summary>
        [ModbusProperty(SlaveId = 1, Partition = Partition.Coils, StartAddress = 0, Timeout = 1000)]
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
        [ModbusProperty(SlaveId = 1, Partition = Partition.Coils, StartAddress = 1, Timeout = 1000, Quantity = 9)]
        public bool[] MyProperty11
        {
            get { return this.GetValueArray<bool>(); }
            set { this.SetValueArray(value); }
        }
        #endregion

        #region DiscreteInputs
        /// <summary>
        /// 声明一个来自离散输入的bool属性。
        /// <para>
        /// 配置：站号、数据区、起始地址、超时时间
        /// </para>
        /// </summary>
        [ModbusProperty(SlaveId = 1, Partition = Partition.DiscreteInputs, StartAddress = 0, Timeout = 1000)]
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
        [ModbusProperty(SlaveId = 1, Partition = Partition.DiscreteInputs, StartAddress = 1, Timeout = 1000, Quantity = 9)]
        public bool MyProperty22
        {
            get { return this.GetValue<bool>(); }
        }
        #endregion

        #region HoldingRegisters
        /// <summary>
        /// 声明一个来自保持寄存器的short属性。
        /// <para>
        /// 配置：站号、数据区、起始地址、超时时间、端序
        /// </para>
        /// </summary>
        [ModbusProperty(SlaveId = 1, Partition = Partition.HoldingRegisters, StartAddress = 0, Timeout = 1000, EndianType = TouchSocket.Core.EndianType.Big)]
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
        [ModbusProperty(SlaveId = 1, Partition = Partition.HoldingRegisters, StartAddress = 1, Timeout = 1000, EndianType = TouchSocket.Core.EndianType.Big, Quantity = 9)]
        public short[] MyProperty33
        {
            get { return this.GetValueArray<short>(); }
            set { this.SetValueArray(value); }
        }
        #endregion

        #region InputRegisters
        /// <summary>
        /// 声明一个来自输入寄存器的short属性。
        /// <para>
        /// 配置：站号、数据区、起始地址、超时时间、端序
        /// </para>
        /// </summary>
        [ModbusProperty(SlaveId = 1, Partition = Partition.InputRegisters, StartAddress = 0, Timeout = 1000, EndianType = TouchSocket.Core.EndianType.Big)]
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
        [ModbusProperty(SlaveId = 1, Partition = Partition.HoldingRegisters, StartAddress = 0, Timeout = 1000, EndianType = TouchSocket.Core.EndianType.Big, Quantity = 9)]
        public short MyProperty44
        {
            get { return this.GetValue<short>(); }
        }
        #endregion
    }

    class MyModbusObject2 : ModbusObject
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
        #endregion

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
        #endregion

        #region HoldingRegisters
        /// <summary>
        /// 声明一个来自保持寄存器的short属性。
        /// <para>
        /// 配置：站号、数据区、起始地址、超时时间、端序
        /// </para>
        /// </summary>
        [ModbusProperty(SlaveId = 1, Partition = Partition.HoldingRegisters, StartAddress = 1000, Timeout = 1000, EndianType = TouchSocket.Core.EndianType.Big)]
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
        #endregion

        #region InputRegisters
        /// <summary>
        /// 声明一个来自输入寄存器的short属性。
        /// <para>
        /// 配置：站号、数据区、起始地址、超时时间、端序
        /// </para>
        /// </summary>
        [ModbusProperty(SlaveId = 1, Partition = Partition.InputRegisters, StartAddress = 1000, Timeout = 1000, EndianType = TouchSocket.Core.EndianType.Big)]
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
        [ModbusProperty(SlaveId = 1, Partition = Partition.HoldingRegisters, StartAddress = 1000, Timeout = 1000, EndianType = TouchSocket.Core.EndianType.Big, Quantity = 9)]
        public short MyProperty44
        {
            get { return this.GetValue<short>(); }
        }
        #endregion
    }
}