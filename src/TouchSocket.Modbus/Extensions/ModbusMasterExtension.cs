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

using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TouchSocket.Modbus
{
    /// <summary>
    /// ModbusClientExtension
    /// </summary>
    public static class ModbusMasterExtension
    {
        #region ReadWrite 默认超时

        /// <summary>
        /// 读写多个寄存器（FC23），默认超时时间为1000ms。
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddressForRead">读取位置（从0开始）</param>
        /// <param name="quantityForRead">读取长度</param>
        /// <param name="startingAddress">写入位置（从0开始）</param>
        /// <param name="bytes">待写入数据</param>
        /// <returns>响应值</returns>
        public static IModbusResponse ReadWriteMultipleRegisters(this IModbusMaster master, byte slaveId, ushort startingAddressForRead, ushort quantityForRead, ushort startingAddress, byte[] bytes)
        {
            return master.ReadWriteMultipleRegisters(slaveId, startingAddressForRead, quantityForRead, startingAddress, bytes, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 读写多个寄存器（FC23），默认超时时间为1000ms。
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddressForRead">读取位置（从0开始）</param>
        /// <param name="quantityForRead">读取长度</param>
        /// <param name="startingAddress">写入位置（从0开始）</param>
        /// <param name="bytes">待写入数据</param>
        /// <returns>响应值</returns>
        public static Task<IModbusResponse> ReadWriteMultipleRegistersAsync(this IModbusMaster master, byte slaveId, ushort startingAddressForRead, ushort quantityForRead, ushort startingAddress, byte[] bytes)
        {
            return master.ReadWriteMultipleRegistersAsync(slaveId, startingAddressForRead, quantityForRead, startingAddress, bytes, 1000, CancellationToken.None);
        }

        #endregion ReadWrite 默认超时

        #region Read 默认超时

        /// <summary>
        /// 从指定站点读取线圈（FC1），默认超时时间为1000ms。
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <returns>读取到的值集合</returns>
        public static bool[] ReadCoils(this IModbusMaster master, byte slaveId, ushort startingAddress, ushort quantity)
        {
            return master.ReadCoils(slaveId, startingAddress, quantity, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 从指定站点读离散输入状态（FC2），默认超时时间为1000ms。
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <returns>读取到的值集合</returns>
        public static bool[] ReadDiscreteInputs(this IModbusMaster master, byte slaveId, ushort startingAddress, ushort quantity)
        {
            return master.ReadDiscreteInputs(slaveId, startingAddress, quantity, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 从指定站点读保持寄存器（FC3），默认超时时间为1000ms。
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <returns>响应值</returns>
        public static IModbusResponse ReadHoldingRegisters(this IModbusMaster master, byte slaveId, ushort startingAddress, ushort quantity)
        {
            return master.ReadHoldingRegisters(slaveId, startingAddress, quantity, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 从指定站点读输入寄存器（FC4），默认超时时间为1000ms。
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <returns>响应值</returns>
        public static IModbusResponse ReadInputRegisters(this IModbusMaster master, byte slaveId, ushort startingAddress, ushort quantity)
        {
            return master.ReadInputRegisters(slaveId, startingAddress, quantity, 1000, CancellationToken.None);
        }

        #endregion Read 默认超时

        #region ReadAsync 默认超时

        /// <summary>
        /// 异步从指定站点读取线圈（FC1），默认超时时间为1000ms。
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <returns>读取到的值集合</returns>
        public static Task<bool[]> ReadCoilsAsync(this IModbusMaster master, byte slaveId, ushort startingAddress, ushort quantity)
        {
            return master.ReadCoilsAsync(slaveId, startingAddress, quantity, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 异步从指定站点读离散输入状态（FC2），默认超时时间为1000ms。
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <returns>读取到的值集合</returns>
        public static Task<bool[]> ReadDiscreteInputsAsync(this IModbusMaster master, byte slaveId, ushort startingAddress, ushort quantity)
        {
            return master.ReadDiscreteInputsAsync(slaveId, startingAddress, quantity, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 异步从指定站点读保持寄存器（FC3），默认超时时间为1000ms。
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <returns>响应值</returns>
        public static Task<IModbusResponse> ReadHoldingRegistersAsync(this IModbusMaster master, byte slaveId, ushort startingAddress, ushort quantity)
        {
            return master.ReadHoldingRegistersAsync(slaveId, startingAddress, quantity, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 异步从指定站点读输入寄存器（FC4），默认超时时间为1000ms。
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <returns>响应值</returns>
        public static Task<IModbusResponse> ReadInputRegistersAsync(this IModbusMaster master, byte slaveId, ushort startingAddress, ushort quantity)
        {
            return master.ReadInputRegistersAsync(slaveId, startingAddress, quantity, 1000, CancellationToken.None);
        }

        #endregion ReadAsync 默认超时

        #region Write 默认超时

        /// <summary>
        /// 向指定站点写入多个线圈（FC15），默认超时时间为1000ms。
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="values">待写入集合</param>
        public static void WriteMultipleCoils(this IModbusMaster master, byte slaveId, ushort startingAddress, bool[] values)
        {
            master.WriteMultipleCoils(slaveId, startingAddress, values, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 向指定站点写入多个寄存器（FC16），默认超时时间为1000ms。
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="bytes">待写入集合</param>
        public static void WriteMultipleRegisters(this IModbusMaster master, byte slaveId, ushort startingAddress, byte[] bytes)
        {
            master.WriteMultipleRegisters(slaveId, startingAddress, bytes, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 向指定站点写入单个线圈（FC5），默认超时时间为1000ms。
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="value">待写入数值</param>
        public static void WriteSingleCoil(this IModbusMaster master, byte slaveId, ushort startingAddress, bool value)
        {
            master.WriteSingleCoil(slaveId, startingAddress, value, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 向指定站点写入单个寄存器（FC6），默认超时时间为1000ms。
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="value">待写入数值</param>
        public static void WriteSingleRegister(this IModbusMaster master, byte slaveId, ushort startingAddress, short value)
        {
            master.WriteSingleRegister(slaveId, startingAddress, value, 1000, CancellationToken.None);
        }

        #endregion Write 默认超时

        #region WriteAsync 默认超时

        /// <summary>
        /// 异步向指定站点写入多个线圈（FC15），默认超时时间为1000ms。
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="values">待写入集合</param>
        public static Task WriteMultipleCoilsAsync(this IModbusMaster master, byte slaveId, ushort startingAddress, bool[] values)
        {
            return master.WriteMultipleCoilsAsync(slaveId, startingAddress, values, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 异步向指定站点写入多个寄存器（FC16），默认超时时间为1000ms。
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="bytes">待写入集合</param>
        public static Task WriteMultipleRegistersAsync(this IModbusMaster master, byte slaveId, ushort startingAddress, byte[] bytes)
        {
            return master.WriteMultipleRegistersAsync(slaveId, startingAddress, bytes, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 异步向指定站点写入单个线圈（FC5），默认超时时间为1000ms。
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="value">待写入数值</param>
        public static Task WriteSingleCoilAsync(this IModbusMaster master, byte slaveId, ushort startingAddress, bool value)
        {
            return master.WriteSingleCoilAsync(slaveId, startingAddress, value, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 异步向指定站点写入单个寄存器（FC6），默认超时时间为1000ms。
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="value">待写入数值</param>
        public static Task WriteSingleRegisterAsync(this IModbusMaster master, byte slaveId, ushort startingAddress, short value)
        {
            return master.WriteSingleRegisterAsync(slaveId, startingAddress, value, 1000, CancellationToken.None);
        }

        #endregion WriteAsync 默认超时

        #region Read

        /// <summary>
        /// 从指定站点读取线圈（FC1）。
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <param name="millisecondsTimeout">超时时间，单位（ms）</param>
        /// <param name="token">可取消令箭</param>
        /// <returns>读取到的值集合</returns>
        public static bool[] ReadCoils(this IModbusMaster master, byte slaveId, ushort startingAddress, ushort quantity, int millisecondsTimeout, CancellationToken token)
        {
            var request = new ModbusRequest(slaveId, FunctionCode.ReadCoils, startingAddress, quantity);

            var response = master.SendModbusRequest(request, millisecondsTimeout, token);
            return response.CreateReader().ToBoolensFromBit().Take(quantity).ToArray();
        }

        /// <summary>
        /// 从指定站点读离散输入状态（FC2）。
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <param name="millisecondsTimeout">超时时间，单位（ms）</param>
        /// <param name="token">可取消令箭</param>
        /// <returns>读取到的值集合</returns>
        public static bool[] ReadDiscreteInputs(this IModbusMaster master, byte slaveId, ushort startingAddress, ushort quantity, int millisecondsTimeout, CancellationToken token)
        {
            var request = new ModbusRequest(slaveId, FunctionCode.ReadDiscreteInputs, startingAddress, quantity);

            var response = master.SendModbusRequest(request, millisecondsTimeout, token);
            return response.CreateReader().ToBoolensFromBit().Take(quantity).ToArray();
        }

        /// <summary>
        /// 从指定站点读保持寄存器（FC3）。
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <param name="millisecondsTimeout">超时时间，单位（ms）</param>
        /// <param name="token">可取消令箭</param>
        /// <returns>响应值</returns>
        public static IModbusResponse ReadHoldingRegisters(this IModbusMaster master, byte slaveId, ushort startingAddress, ushort quantity, int millisecondsTimeout, CancellationToken token)
        {
            var request = new ModbusRequest(slaveId, FunctionCode.ReadHoldingRegisters, startingAddress, quantity);

            var response = master.SendModbusRequest(request, millisecondsTimeout, token);
            return response;
        }

        /// <summary>
        /// 从指定站点读输入寄存器（FC4）。
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <param name="millisecondsTimeout">超时时间，单位（ms）</param>
        /// <param name="token">可取消令箭</param>
        /// <returns>响应值</returns>
        public static IModbusResponse ReadInputRegisters(this IModbusMaster master, byte slaveId, ushort startingAddress, ushort quantity, int millisecondsTimeout, CancellationToken token)
        {
            var request = new ModbusRequest(slaveId, FunctionCode.ReadInputRegisters, startingAddress, quantity);

            var response = master.SendModbusRequest(request, millisecondsTimeout, token);
            return response;
        }

        #endregion Read

        #region ReadAsync

        /// <summary>
        /// 异步从指定站点读取线圈（FC1）。
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <param name="millisecondsTimeout">超时时间，单位（ms）</param>
        /// <param name="token">可取消令箭</param>
        /// <returns>读取到的值集合</returns>
        public static async Task<bool[]> ReadCoilsAsync(this IModbusMaster master, byte slaveId, ushort startingAddress, ushort quantity, int millisecondsTimeout, CancellationToken token)
        {
            var request = new ModbusRequest(slaveId, FunctionCode.ReadCoils, startingAddress, quantity);

            var response = await master.SendModbusRequestAsync(request, millisecondsTimeout, token);
            return response.CreateReader().ToBoolensFromBit().Take(quantity).ToArray();
        }

        /// <summary>
        /// 异步从指定站点读离散输入状态（FC2）。
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <param name="millisecondsTimeout">超时时间，单位（ms）</param>
        /// <param name="token">可取消令箭</param>
        /// <returns>读取到的值集合</returns>
        public static async Task<bool[]> ReadDiscreteInputsAsync(this IModbusMaster master, byte slaveId, ushort startingAddress, ushort quantity, int millisecondsTimeout, CancellationToken token)
        {
            var request = new ModbusRequest(slaveId, FunctionCode.ReadDiscreteInputs, startingAddress, quantity);

            var response = await master.SendModbusRequestAsync(request, millisecondsTimeout, token);
            return response.CreateReader().ToBoolensFromBit().Take(quantity).ToArray();
        }

        /// <summary>
        /// 异步从指定站点读保持寄存器（FC3）。
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <param name="millisecondsTimeout">超时时间，单位（ms）</param>
        /// <param name="token">可取消令箭</param>
        /// <returns>响应值</returns>
        public static async Task<IModbusResponse> ReadHoldingRegistersAsync(this IModbusMaster master, byte slaveId, ushort startingAddress, ushort quantity, int millisecondsTimeout, CancellationToken token)
        {
            var request = new ModbusRequest(slaveId, FunctionCode.ReadHoldingRegisters, startingAddress, quantity);

            var response = await master.SendModbusRequestAsync(request, millisecondsTimeout, token);
            return response;
        }

        /// <summary>
        /// 异步从指定站点读输入寄存器（FC4）。
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <param name="millisecondsTimeout">超时时间，单位（ms）</param>
        /// <param name="token">可取消令箭</param>
        /// <returns>响应值</returns>
        public static async Task<IModbusResponse> ReadInputRegistersAsync(this IModbusMaster master, byte slaveId, ushort startingAddress, ushort quantity, int millisecondsTimeout, CancellationToken token)
        {
            var request = new ModbusRequest(slaveId, FunctionCode.ReadInputRegisters, startingAddress, quantity);

            var response = await master.SendModbusRequestAsync(request, millisecondsTimeout, token);
            return response;
        }

        #endregion ReadAsync

        #region Write

        /// <summary>
        /// 向指定站点写入多个线圈（FC15）。
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="values">待写入集合</param>
        /// <param name="millisecondsTimeout">超时时间，单位（ms）</param>
        /// <param name="token">可取消令箭</param>
        public static void WriteMultipleCoils(this IModbusMaster master, byte slaveId, ushort startingAddress, bool[] values, int millisecondsTimeout, CancellationToken token)
        {
            var request = new ModbusRequest(slaveId, FunctionCode.WriteMultipleCoils);
            request.StartingAddress = startingAddress;
            request.SetValue(values);

            master.SendModbusRequest(request, millisecondsTimeout, token);
        }

        /// <summary>
        /// 向指定站点写入多个寄存器（FC16）。
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="bytes">待写入集合</param>
        /// <param name="millisecondsTimeout">超时时间，单位（ms）</param>
        /// <param name="token">可取消令箭</param>
        public static void WriteMultipleRegisters(this IModbusMaster master, byte slaveId, ushort startingAddress, byte[] bytes, int millisecondsTimeout, CancellationToken token)
        {
            var request = new ModbusRequest(slaveId, FunctionCode.WriteMultipleRegisters);
            request.StartingAddress = startingAddress;
            request.SetValue(bytes);

            master.SendModbusRequest(request, millisecondsTimeout, token);
        }

        /// <summary>
        /// 向指定站点写入单个线圈（FC5）。
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="value">待写入数值</param>
        /// <param name="millisecondsTimeout">超时时间，单位（ms）</param>
        /// <param name="token">可取消令箭</param>
        public static void WriteSingleCoil(this IModbusMaster master, byte slaveId, ushort startingAddress, bool value, int millisecondsTimeout, CancellationToken token)
        {
            var request = new ModbusRequest(slaveId, FunctionCode.WriteSingleCoil);
            request.StartingAddress = startingAddress;
            request.SetValue(value);

            master.SendModbusRequest(request, millisecondsTimeout, token);
        }

        /// <summary>
        /// 向指定站点写入单个寄存器（FC6）
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="value">待写入数值</param>
        /// <param name="millisecondsTimeout">超时时间，单位（ms）</param>
        /// <param name="token">可取消令箭</param>
        public static void WriteSingleRegister(this IModbusMaster master, byte slaveId, ushort startingAddress, short value, int millisecondsTimeout, CancellationToken token)
        {
            var request = new ModbusRequest(slaveId, FunctionCode.WriteSingleRegister);
            request.StartingAddress = startingAddress;
            request.SetValue(value);

            master.SendModbusRequest(request, millisecondsTimeout, token);
        }

        /// <summary>
        /// 向指定站点写入单个寄存器（FC6）
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="value">待写入数值</param>
        /// <param name="millisecondsTimeout">超时时间，单位（ms）</param>
        /// <param name="token">可取消令箭</param>
        public static void WriteSingleRegister(this IModbusMaster master, byte slaveId, ushort startingAddress, ushort value, int millisecondsTimeout, CancellationToken token)
        {
            var request = new ModbusRequest(slaveId, FunctionCode.WriteSingleRegister);
            request.StartingAddress = startingAddress;
            request.SetValue(value);

            master.SendModbusRequest(request, millisecondsTimeout, token);
        }

        #endregion Write

        #region WriteAsync

        /// <summary>
        /// 异步向指定站点写入多个线圈（FC15）。
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="values">待写入集合</param>
        /// <param name="millisecondsTimeout">超时时间，单位（ms）</param>
        /// <param name="token">可取消令箭</param>
        public static async Task WriteMultipleCoilsAsync(this IModbusMaster master, byte slaveId, ushort startingAddress, bool[] values, int millisecondsTimeout, CancellationToken token)
        {
            var request = new ModbusRequest(slaveId, FunctionCode.WriteMultipleCoils);
            request.StartingAddress = startingAddress;
            request.SetValue(values);

            await master.SendModbusRequestAsync(request, millisecondsTimeout, token);
        }

        /// <summary>
        /// 异步向指定站点写入多个寄存器（FC16）。
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="bytes">待写入集合</param>
        /// <param name="millisecondsTimeout">超时时间，单位（ms）</param>
        /// <param name="token">可取消令箭</param>
        public static async Task WriteMultipleRegistersAsync(this IModbusMaster master, byte slaveId, ushort startingAddress, byte[] bytes, int millisecondsTimeout, CancellationToken token)
        {
            var request = new ModbusRequest(slaveId, FunctionCode.WriteMultipleRegisters);
            request.StartingAddress = startingAddress;
            request.SetValue(bytes);

            await master.SendModbusRequestAsync(request, millisecondsTimeout, token);
        }

        /// <summary>
        /// 异步向指定站点写入单个线圈（FC5）。
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="value">待写入数值</param>
        /// <param name="millisecondsTimeout">超时时间，单位（ms）</param>
        /// <param name="token">可取消令箭</param>
        public static async Task WriteSingleCoilAsync(this IModbusMaster master, byte slaveId, ushort startingAddress, bool value, int millisecondsTimeout, CancellationToken token)
        {
            var request = new ModbusRequest(slaveId, FunctionCode.WriteSingleCoil);
            request.StartingAddress = startingAddress;
            request.SetValue(value);

            await master.SendModbusRequestAsync(request, millisecondsTimeout, token);
        }

        /// <summary>
        /// 异步向指定站点写入单个寄存器（FC6）
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="value">待写入数值</param>
        /// <param name="millisecondsTimeout">超时时间，单位（ms）</param>
        /// <param name="token">可取消令箭</param>
        public static async Task WriteSingleRegisterAsync(this IModbusMaster master, byte slaveId, ushort startingAddress, short value, int millisecondsTimeout, CancellationToken token)
        {
            var request = new ModbusRequest(slaveId, FunctionCode.WriteSingleRegister);
            request.StartingAddress = startingAddress;
            request.SetValue(value);

            await master.SendModbusRequestAsync(request, millisecondsTimeout, token);
        }

        /// <summary>
        /// 异步向指定站点写入单个寄存器（FC6）
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="value">待写入数值</param>
        /// <param name="millisecondsTimeout">超时时间，单位（ms）</param>
        /// <param name="token">可取消令箭</param>
        public static async Task WriteSingleRegisterAsync(this IModbusMaster master, byte slaveId, ushort startingAddress, ushort value, int millisecondsTimeout, CancellationToken token)
        {
            var request = new ModbusRequest(slaveId, FunctionCode.WriteSingleRegister);
            request.StartingAddress = startingAddress;
            request.SetValue(value);

            await master.SendModbusRequestAsync(request, millisecondsTimeout, token);
        }

        #endregion WriteAsync

        #region ReadWrite

        /// <summary>
        /// 读写多个寄存器（FC23）
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddressForRead">读取位置（从0开始）</param>
        /// <param name="quantityForRead">读取长度</param>
        /// <param name="startingAddress">写入位置（从0开始）</param>
        /// <param name="bytes">待写入数据</param>
        /// <param name="millisecondsTimeout">超时时间，单位（ms）</param>
        /// <param name="token">可取消令箭</param>
        /// <returns>响应值</returns>
        public static IModbusResponse ReadWriteMultipleRegisters(this IModbusMaster master, byte slaveId, ushort startingAddressForRead, ushort quantityForRead, ushort startingAddress, byte[] bytes, int millisecondsTimeout, CancellationToken token)
        {
            var request = new ModbusRequest(slaveId, FunctionCode.ReadWriteMultipleRegisters);
            request.StartingAddress = startingAddress;
            request.ReadStartAddress = startingAddressForRead;
            request.ReadQuantity = quantityForRead;
            request.SetValue(bytes);
            return master.SendModbusRequest(request, millisecondsTimeout, token);
        }

        /// <summary>
        /// 读写多个寄存器（FC23）
        /// </summary>
        /// <param name="master">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddressForRead">读取位置（从0开始）</param>
        /// <param name="quantityForRead">读取长度</param>
        /// <param name="startingAddress">写入位置（从0开始）</param>
        /// <param name="bytes">待写入数据</param>
        /// <param name="millisecondsTimeout">超时时间，单位（ms）</param>
        /// <param name="token">可取消令箭</param>
        /// <returns>响应值</returns>
        public static async Task<IModbusResponse> ReadWriteMultipleRegistersAsync(this IModbusMaster master, byte slaveId, ushort startingAddressForRead, ushort quantityForRead, ushort startingAddress, byte[] bytes, int millisecondsTimeout, CancellationToken token)
        {
            var request = new ModbusRequest(slaveId, FunctionCode.ReadWriteMultipleRegisters);
            request.StartingAddress = startingAddress;
            request.ReadStartAddress = startingAddressForRead;
            request.ReadQuantity = quantityForRead;
            request.SetValue(bytes);
            return await master.SendModbusRequestAsync(request, millisecondsTimeout, token);
        }

        #endregion ReadWrite
    }
}