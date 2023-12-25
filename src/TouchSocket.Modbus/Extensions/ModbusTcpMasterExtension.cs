//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TouchSocket.Modbus
{
    /// <summary>
    /// ModbusTcpClientExtension
    /// </summary>
    public static class ModbusTcpMasterExtension
    {
        #region ReadWrite 默认超时

        /// <summary>
        /// 读写多个寄存器（FC23），默认超时时间为1000ms。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="startingAddressForRead">读取位置（从0开始）</param>
        /// <param name="quantityForRead">读取长度</param>
        /// <param name="startingAddress">写入位置（从0开始）</param>
        /// <param name="bytes">待写入数据</param>
        /// <returns>响应值</returns>
        public static IModbusResponse ReadWriteMultipleRegisters(this IModbusTcpMaster client,ushort startingAddressForRead, ushort quantityForRead, ushort startingAddress, byte[] bytes)
        {
            return client.ReadWriteMultipleRegisters(1, startingAddressForRead, quantityForRead, startingAddress, bytes, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 读写多个寄存器（FC23），默认超时时间为1000ms。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="startingAddressForRead">读取位置（从0开始）</param>
        /// <param name="quantityForRead">读取长度</param>
        /// <param name="startingAddress">写入位置（从0开始）</param>
        /// <param name="bytes">待写入数据</param>
        /// <returns>响应值</returns>
        public static Task<IModbusResponse> ReadWriteMultipleRegistersAsync(this IModbusTcpMaster client, ushort startingAddressForRead, ushort quantityForRead, ushort startingAddress, byte[] bytes)
        {
            return client.ReadWriteMultipleRegistersAsync(1, startingAddressForRead, quantityForRead, startingAddress, bytes, 1000, CancellationToken.None);
        }
        #endregion

        #region Read 默认超时
        /// <summary>
        /// 忽略站点（默认站号为1）读取线圈（FC1），默认超时时间为1000ms。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <returns>读取到的值集合</returns>
        public static bool[] ReadCoils(this IModbusTcpMaster client, ushort startingAddress, ushort quantity)
        {
            return client.ReadCoils(1, startingAddress, quantity, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 忽略站点（默认站号为1）读离散输入状态（FC2），默认超时时间为1000ms。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <returns>读取到的值集合</returns>
        public static bool[] ReadDiscreteInputs(this IModbusTcpMaster client, ushort startingAddress, ushort quantity)
        {
            return client.ReadDiscreteInputs(1, startingAddress, quantity, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 忽略站点（默认站号为1）读保持寄存器（FC3），默认超时时间为1000ms。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <returns>响应值</returns>
        public static IModbusResponse ReadHoldingRegisters(this IModbusTcpMaster client, ushort startingAddress, ushort quantity)
        {
            return client.ReadHoldingRegisters(1, startingAddress, quantity, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 忽略站点（默认站号为1）读输入寄存器（FC4），默认超时时间为1000ms。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <returns>响应值</returns>
        public static IModbusResponse ReadInputRegisters(this IModbusTcpMaster client, ushort startingAddress, ushort quantity)
        {
            return client.ReadInputRegisters(1, startingAddress, quantity, 1000, CancellationToken.None);
        }

        #endregion Read 默认超时

        #region ReadAsync 默认超时

        /// <summary>
        /// 异步忽略站点（默认站号为1）读取线圈（FC1），默认超时时间为1000ms。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <returns>读取到的值集合</returns>
        public static Task<bool[]> ReadCoilsAsync(this IModbusTcpMaster client, ushort startingAddress, ushort quantity)
        {
            return client.ReadCoilsAsync(1, startingAddress, quantity, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 异步忽略站点（默认站号为1）读离散输入状态（FC2），默认超时时间为1000ms。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <returns>读取到的值集合</returns>
        public static Task<bool[]> ReadDiscreteInputsAsync(this IModbusTcpMaster client, ushort startingAddress, ushort quantity)
        {
            return client.ReadDiscreteInputsAsync(1, startingAddress, quantity, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 异步忽略站点（默认站号为1）读保持寄存器（FC3），默认超时时间为1000ms。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <returns>响应值</returns>
        public static Task<IModbusResponse> ReadHoldingRegistersAsync(this IModbusTcpMaster client, ushort startingAddress, ushort quantity)
        {
            return client.ReadHoldingRegistersAsync(1, startingAddress, quantity, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 异步忽略站点（默认站号为1）读输入寄存器（FC4），默认超时时间为1000ms。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <returns>响应值</returns>
        public static Task<IModbusResponse> ReadInputRegistersAsync(this IModbusTcpMaster client, ushort startingAddress, ushort quantity)
        {
            return client.ReadInputRegistersAsync(1, startingAddress, quantity, 1000, CancellationToken.None);
        }

        #endregion ReadAsync 默认超时

        #region Write 默认超时

        /// <summary>
        /// 忽略站点（默认站号为1）写入多个线圈（FC15），默认超时时间为1000ms。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="values">待写入集合</param>
        public static void WriteMultipleCoils(this IModbusTcpMaster client, ushort startingAddress, bool[] values)
        {
            client.WriteMultipleCoils(1, startingAddress, values, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 忽略站点（默认站号为1）写入多个寄存器（FC16），默认超时时间为1000ms。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="bytes">待写入集合</param>
        public static void WriteMultipleRegisters(this IModbusTcpMaster client, ushort startingAddress, byte[] bytes)
        {
            client.WriteMultipleRegisters(1, startingAddress, bytes, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 忽略站点（默认站号为1）写入单个线圈（FC5），默认超时时间为1000ms。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="value">待写入数值</param>
        public static void WriteSingleCoil(this IModbusTcpMaster client, ushort startingAddress, bool value)
        {
            client.WriteSingleCoil(1, startingAddress, value, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 忽略站点（默认站号为1）写入单个寄存器（FC6），默认超时时间为1000ms。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="value">待写入数值</param>
        public static void WriteSingleRegister(this IModbusTcpMaster client, ushort startingAddress, short value)
        {
            client.WriteSingleRegister(1, startingAddress, value, 1000, CancellationToken.None);
        }

        #endregion Write 默认超时

        #region WriteAsync 默认超时

        /// <summary>
        /// 异步忽略站点（默认站号为1）写入多个线圈（FC15），默认超时时间为1000ms。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="values">待写入集合</param>
        public static Task WriteMultipleCoilsAsync(this IModbusTcpMaster client, ushort startingAddress, bool[] values)
        {
            return client.WriteMultipleCoilsAsync(1, startingAddress, values, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 异步忽略站点（默认站号为1）写入多个寄存器（FC16），默认超时时间为1000ms。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="bytes">待写入集合</param>
        public static Task WriteMultipleRegistersAsync(this IModbusTcpMaster client, ushort startingAddress, byte[] bytes)
        {
            return client.WriteMultipleRegistersAsync(1, startingAddress, bytes, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 异步忽略站点（默认站号为1）写入单个线圈（FC5），默认超时时间为1000ms。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="value">待写入数值</param>
        public static Task WriteSingleCoilAsync(this IModbusTcpMaster client, ushort startingAddress, bool value)
        {
            return client.WriteSingleCoilAsync(1, startingAddress, value, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 异步忽略站点（默认站号为1）写入单个寄存器（FC6），默认超时时间为1000ms。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="value">待写入数值</param>
        public static Task WriteSingleRegisterAsync(this IModbusTcpMaster client, ushort startingAddress, short value)
        {
            return client.WriteSingleRegisterAsync(1, startingAddress, value, 1000, CancellationToken.None);
        }

        #endregion WriteAsync 默认超时
    }
}