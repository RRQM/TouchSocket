using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TouchSocket.Modbus
{
    /// <summary>
    /// ModbusClientExtension
    /// </summary>
    public static class ModbusClientExtension
    {
        #region Read 默认超时

        /// <summary>
        /// 从指定站点读取线圈（FC1），默认超时时间为1000ms。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <returns>读取到的值集合</returns>
        public static bool[] ReadCoils(this IModbusClient client, byte slaveId, ushort startingAddress, ushort quantity)
        {
            return client.ReadCoils(slaveId, startingAddress, quantity, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 从指定站点读离散输入状态（FC2），默认超时时间为1000ms。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <returns>读取到的值集合</returns>
        public static bool[] ReadDiscreteInputs(this IModbusClient client, byte slaveId, ushort startingAddress, ushort quantity)
        {
            return client.ReadDiscreteInputs(slaveId, startingAddress, quantity, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 从指定站点读保持寄存器（FC3），默认超时时间为1000ms。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <returns>响应值</returns>
        public static IModbusResponse ReadHoldingRegisters(this IModbusClient client, byte slaveId, ushort startingAddress, ushort quantity)
        {
            return client.ReadHoldingRegisters(slaveId, startingAddress, quantity, 1000, CancellationToken.None);
        }


        /// <summary>
        /// 从指定站点读输入寄存器（FC4），默认超时时间为1000ms。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <returns>响应值</returns>
        public static IModbusResponse ReadInputRegisters(this IModbusClient client, byte slaveId, ushort startingAddress, ushort quantity)
        {
            return client.ReadInputRegisters(slaveId, startingAddress, quantity, 1000, CancellationToken.None);
        }

        #endregion Read 默认超时

        #region ReadAsync 默认超时

        /// <summary>
        /// 异步从指定站点读取线圈（FC1），默认超时时间为1000ms。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <returns>读取到的值集合</returns>
        public static Task<bool[]> ReadCoilsAsync(this IModbusClient client, byte slaveId, ushort startingAddress, ushort quantity)
        {
            return client.ReadCoilsAsync(slaveId, startingAddress, quantity, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 异步从指定站点读离散输入状态（FC2），默认超时时间为1000ms。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <returns>读取到的值集合</returns>
        public static Task<bool[]> ReadDiscreteInputsAsync(this IModbusClient client, byte slaveId, ushort startingAddress, ushort quantity)
        {
            return client.ReadDiscreteInputsAsync(slaveId, startingAddress, quantity, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 异步从指定站点读保持寄存器（FC3），默认超时时间为1000ms。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <returns>响应值</returns>
        public static Task<IModbusResponse> ReadHoldingRegistersAsync(this IModbusClient client, byte slaveId, ushort startingAddress, ushort quantity)
        {
            return client.ReadHoldingRegistersAsync(slaveId, startingAddress, quantity, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 异步从指定站点读输入寄存器（FC4），默认超时时间为1000ms。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <returns>响应值</returns>
        public static Task<IModbusResponse> ReadInputRegistersAsync(this IModbusClient client, byte slaveId, ushort startingAddress, ushort quantity)
        {
            return client.ReadInputRegistersAsync(slaveId, startingAddress, quantity, 1000, CancellationToken.None);
        }

        #endregion ReadAsync 默认超时

        #region Write 默认超时

        /// <summary>
        /// 向指定站点写入多个线圈（FC15），默认超时时间为1000ms。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="values">待写入集合</param>
        public static void WriteMultipleCoils(this IModbusClient client, byte slaveId, ushort startingAddress, bool[] values)
        {
            client.WriteMultipleCoils(slaveId, startingAddress, values, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 向指定站点写入多个寄存器（FC16），默认超时时间为1000ms。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="bytes">待写入集合</param>
        public static void WriteMultipleRegisters(this IModbusClient client, byte slaveId, ushort startingAddress, byte[] bytes)
        {
            client.WriteMultipleRegisters(slaveId, startingAddress, bytes, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 向指定站点写入单个线圈（FC5），默认超时时间为1000ms。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="value">待写入数值</param>
        public static void WriteSingleCoil(this IModbusClient client, byte slaveId, ushort startingAddress, bool value)
        {
            client.WriteSingleCoil(slaveId, startingAddress, value, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 向指定站点写入单个寄存器（FC6），默认超时时间为1000ms。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="value">待写入数值</param>
        public static void WriteSingleRegister(this IModbusClient client, byte slaveId, ushort startingAddress, short value)
        {
            client.WriteSingleRegister(slaveId, startingAddress, value, 1000, CancellationToken.None);
        }

        #endregion Write  默认超时

        #region WriteAsync 默认超时

        /// <summary>
        /// 异步向指定站点写入多个线圈（FC15），默认超时时间为1000ms。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="values">待写入集合</param>
        public static Task WriteMultipleCoilsAsync(this IModbusClient client, byte slaveId, ushort startingAddress, bool[] values)
        {
            return client.WriteMultipleCoilsAsync(slaveId, startingAddress, values, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 异步向指定站点写入多个寄存器（FC16），默认超时时间为1000ms。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="bytes">待写入集合</param>
        public static Task WriteMultipleRegistersAsync(this IModbusClient client, byte slaveId, ushort startingAddress, byte[] bytes)
        {
            return client.WriteMultipleRegistersAsync(slaveId, startingAddress, bytes, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 异步向指定站点写入单个线圈（FC5），默认超时时间为1000ms。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="value">待写入数值</param>
        public static Task WriteSingleCoilAsync(this IModbusClient client, byte slaveId, ushort startingAddress, bool value)
        {
            return client.WriteSingleCoilAsync(slaveId, startingAddress, value, 1000, CancellationToken.None);
        }

        /// <summary>
        /// 异步向指定站点写入单个寄存器（FC6），默认超时时间为1000ms。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="value">待写入数值</param>
        public static Task WriteSingleRegisterAsync(this IModbusClient client, byte slaveId, ushort startingAddress, short value)
        {
            return client.WriteSingleRegisterAsync(slaveId, startingAddress, value, 1000, CancellationToken.None);
        }

        #endregion WriteAsync  默认超时

        #region Read
        /// <summary>
        /// 从指定站点读取线圈（FC1）。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <param name="timeout">超时时间，单位（ms）</param>
        /// <param name="token">可取消令箭</param>
        /// <returns>读取到的值集合</returns>
        public static bool[] ReadCoils(this IModbusClient client, byte slaveId, ushort startingAddress, ushort quantity, int timeout, CancellationToken token)
        {
            var request = new ModbusRequest(slaveId, FunctionCode.ReadCoils, startingAddress, quantity);

            var response = client.SendModbusRequest(request, timeout, token);
            SRHelper.ThrowIfNotSuccess(response.GetErrorCode());
            return response.CreateReader().ToBoolensFromBit().Take(quantity).ToArray();
        }

        /// <summary>
        /// 从指定站点读离散输入状态（FC2）。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <param name="timeout">超时时间，单位（ms）</param>
        /// <param name="token">可取消令箭</param>
        /// <returns>读取到的值集合</returns>
        public static bool[] ReadDiscreteInputs(this IModbusClient client, byte slaveId, ushort startingAddress, ushort quantity, int timeout, CancellationToken token)
        {
            var request = new ModbusRequest(slaveId, FunctionCode.ReadDiscreteInputs, startingAddress, quantity);

            var response = client.SendModbusRequest(request, timeout, token);
            return response.CreateReader().ToBoolensFromBit().Take(quantity).ToArray();
        }

        /// <summary>
        /// 从指定站点读保持寄存器（FC3）。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <param name="timeout">超时时间，单位（ms）</param>
        /// <param name="token">可取消令箭</param>
        /// <returns>响应值</returns>
        public static IModbusResponse ReadHoldingRegisters(this IModbusClient client, byte slaveId, ushort startingAddress, ushort quantity, int timeout, CancellationToken token)
        {
            var request = new ModbusRequest(slaveId, FunctionCode.ReadHoldingRegisters, startingAddress, quantity);

            var response = client.SendModbusRequest(request, timeout, token);
            return response;
        }

        /// <summary>
        /// 从指定站点读输入寄存器（FC4）。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <param name="timeout">超时时间，单位（ms）</param>
        /// <param name="token">可取消令箭</param>
        /// <returns>响应值</returns>
        public static IModbusResponse ReadInputRegisters(this IModbusClient client, byte slaveId, ushort startingAddress, ushort quantity, int timeout, CancellationToken token)
        {
            var request = new ModbusRequest(slaveId, FunctionCode.ReadInputRegisters, startingAddress, quantity);

            var response = client.SendModbusRequest(request, timeout, token);
            return response;
        }

        #endregion Read

        #region ReadAsync

        /// <summary>
        /// 异步从指定站点读取线圈（FC1）。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <param name="timeout">超时时间，单位（ms）</param>
        /// <param name="token">可取消令箭</param>
        /// <returns>读取到的值集合</returns>
        public static async Task<bool[]> ReadCoilsAsync(this IModbusClient client, byte slaveId, ushort startingAddress, ushort quantity, int timeout, CancellationToken token)
        {
            var request = new ModbusRequest(slaveId, FunctionCode.ReadCoils, startingAddress, quantity);

            var response = await client.SendModbusRequestAsync(request, timeout, token);
            return response.CreateReader().ToBoolensFromBit().Take(quantity).ToArray();
        }

        /// <summary>
        /// 异步从指定站点读离散输入状态（FC2）。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <param name="timeout">超时时间，单位（ms）</param>
        /// <param name="token">可取消令箭</param>
        /// <returns>读取到的值集合</returns>
        public static async Task<bool[]> ReadDiscreteInputsAsync(this IModbusClient client, byte slaveId, ushort startingAddress, ushort quantity, int timeout, CancellationToken token)
        {
            var request = new ModbusRequest(slaveId, FunctionCode.ReadDiscreteInputs, startingAddress, quantity);

            var response = await client.SendModbusRequestAsync(request, timeout, token);
            return response.CreateReader().ToBoolensFromBit().Take(quantity).ToArray();
        }

        /// <summary>
        /// 异步从指定站点读保持寄存器（FC3）。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <param name="timeout">超时时间，单位（ms）</param>
        /// <param name="token">可取消令箭</param>
        /// <returns>响应值</returns>
        public static async Task<IModbusResponse> ReadHoldingRegistersAsync(this IModbusClient client, byte slaveId, ushort startingAddress, ushort quantity, int timeout, CancellationToken token)
        {
            var request = new ModbusRequest(slaveId, FunctionCode.ReadHoldingRegisters, startingAddress, quantity);

            var response = await client.SendModbusRequestAsync(request, timeout, token);
            return response;
        }

        /// <summary>
        /// 异步从指定站点读输入寄存器（FC4）。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="quantity">读取数量</param>
        /// <param name="timeout">超时时间，单位（ms）</param>
        /// <param name="token">可取消令箭</param>
        /// <returns>响应值</returns>
        public static async Task<IModbusResponse> ReadInputRegistersAsync(this IModbusClient client, byte slaveId, ushort startingAddress, ushort quantity, int timeout, CancellationToken token)
        {
            var request = new ModbusRequest(slaveId, FunctionCode.ReadInputRegisters, startingAddress, quantity);

            var response = await client.SendModbusRequestAsync(request, timeout, token);
            return response;
        }

        #endregion ReadAsync

        #region Write
        /// <summary>
        /// 向指定站点写入多个线圈（FC15）。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="values">待写入集合</param>
        /// <param name="timeout">超时时间，单位（ms）</param>
        /// <param name="token">可取消令箭</param>
        public static void WriteMultipleCoils(this IModbusClient client, byte slaveId, ushort startingAddress, bool[] values, int timeout, CancellationToken token)
        {
            var request = new ModbusRequest(slaveId, FunctionCode.WriteMultipleCoils);
            request.SetStartingAddress(startingAddress);
            request.SetValue(values);

            client.SendModbusRequest(request, timeout, token);
        }

        /// <summary>
        /// 向指定站点写入多个寄存器（FC16）。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="bytes">待写入集合</param>
        /// <param name="timeout">超时时间，单位（ms）</param>
        /// <param name="token">可取消令箭</param>
        public static void WriteMultipleRegisters(this IModbusClient client, byte slaveId, ushort startingAddress, byte[] bytes, int timeout, CancellationToken token)
        {
            var request = new ModbusRequest(slaveId, FunctionCode.WriteMultipleRegisters);
            request.SetStartingAddress(startingAddress);
            request.SetValue(bytes);

            client.SendModbusRequest(request, timeout, token);
        }

        /// <summary>
        /// 向指定站点写入单个线圈（FC5）。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="value">待写入数值</param>
        /// <param name="timeout">超时时间，单位（ms）</param>
        /// <param name="token">可取消令箭</param>
        public static void WriteSingleCoil(this IModbusClient client, byte slaveId, ushort startingAddress, bool value, int timeout, CancellationToken token)
        {
            var request = new ModbusRequest(slaveId, FunctionCode.WriteSingleCoil);
            request.SetStartingAddress(startingAddress);
            request.SetValue(value);

            client.SendModbusRequest(request, timeout, token);
        }

        /// <summary>
        /// 向指定站点写入单个寄存器（FC6）
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="value">待写入数值</param>
        /// <param name="timeout">超时时间，单位（ms）</param>
        /// <param name="token">可取消令箭</param>
        public static void WriteSingleRegister(this IModbusClient client, byte slaveId, ushort startingAddress, short value, int timeout, CancellationToken token)
        {
            var request = new ModbusRequest(slaveId, FunctionCode.WriteSingleRegister);
            request.SetStartingAddress(startingAddress);
            request.SetValue(value);

            client.SendModbusRequest(request, timeout, token);
        }

        #endregion Write

        #region WriteAsync

        /// <summary>
        /// 异步向指定站点写入多个线圈（FC15）。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="values">待写入集合</param>
        /// <param name="timeout">超时时间，单位（ms）</param>
        /// <param name="token">可取消令箭</param>
        public static async Task WriteMultipleCoilsAsync(this IModbusClient client, byte slaveId, ushort startingAddress, bool[] values, int timeout, CancellationToken token)
        {
            var request = new ModbusRequest(slaveId, FunctionCode.WriteMultipleCoils);
            request.SetStartingAddress(startingAddress);
            request.SetValue(values);

            await client.SendModbusRequestAsync(request, timeout, token);
        }

        /// <summary>
        /// 异步向指定站点写入多个寄存器（FC16）。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="bytes">待写入集合</param>
        /// <param name="timeout">超时时间，单位（ms）</param>
        /// <param name="token">可取消令箭</param>
        public static async Task WriteMultipleRegistersAsync(this IModbusClient client, byte slaveId, ushort startingAddress, byte[] bytes, int timeout, CancellationToken token)
        {
            var request = new ModbusRequest(slaveId, FunctionCode.WriteMultipleRegisters);
            request.SetStartingAddress(startingAddress);
            request.SetValue(bytes);

            await client.SendModbusRequestAsync(request, timeout, token);
        }

        /// <summary>
        /// 异步向指定站点写入单个线圈（FC5）。
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="value">待写入数值</param>
        /// <param name="timeout">超时时间，单位（ms）</param>
        /// <param name="token">可取消令箭</param>
        public static async Task WriteSingleCoilAsync(this IModbusClient client, byte slaveId, ushort startingAddress, bool value, int timeout, CancellationToken token)
        {
            var request = new ModbusRequest(slaveId, FunctionCode.WriteSingleCoil);
            request.SetStartingAddress(startingAddress);
            request.SetValue(value);

            await client.SendModbusRequestAsync(request, timeout, token);
        }

        /// <summary>
        /// 异步向指定站点写入单个寄存器（FC6）
        /// </summary>
        /// <param name="client">通讯客户端</param>
        /// <param name="slaveId">站点号</param>
        /// <param name="startingAddress">起始位置（从0开始）</param>
        /// <param name="value">待写入数值</param>
        /// <param name="timeout">超时时间，单位（ms）</param>
        /// <param name="token">可取消令箭</param>
        public static async Task WriteSingleRegisterAsync(this IModbusClient client, byte slaveId, ushort startingAddress, short value, int timeout, CancellationToken token)
        {
            var request = new ModbusRequest(slaveId, FunctionCode.WriteSingleRegister);
            request.SetStartingAddress(startingAddress);
            request.SetValue(value);

            await client.SendModbusRequestAsync(request, timeout, token);
        }

        #endregion WriteAsync
    }
}