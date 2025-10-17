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

namespace TouchSocket.Modbus;

/// <summary>
/// ModbusMaster扩展类
/// 该类提供了一些对ModbusMaster实例进行操作的扩展方法
/// </summary>
public static class ModbusMasterExtension
{
    #region 同步
    /// <summary>
    /// 向Modbus从机设备发送一个Modbus请求。
    /// </summary>
    /// <param name="master">Modbus主控接口。</param>
    /// <param name="request">要发送的Modbus请求。</param>
    /// <param name="millisecondsTimeout">操作超时的时间，以毫秒为单位。</param>
    /// <param name="cancellationToken">用于取消操作的取消令牌。</param>
    /// <returns>返回从Modbus从机设备接收到的响应。</returns>
    [AsyncToSyncWarning]
    public static IModbusResponse SendModbusRequest(this IModbusMaster master, ModbusRequest request, int millisecondsTimeout, CancellationToken cancellationToken)
    {
        // 使用异步方法 SendModbusRequestAsync 发送请求，并直接获取结果，而不等待异步操作完成。
        // 这样做是因为我们假设调用者已经处理了异步相关的逻辑，这里只是一个直接的同步包装。
        return master.SendModbusRequestAsync(request, millisecondsTimeout, cancellationToken).GetFalseAwaitResult();
    }
    #endregion

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
    [AsyncToSyncWarning]
    public static IModbusResponse ReadWriteMultipleRegisters(this IModbusMaster master, byte slaveId, ushort startingAddressForRead, ushort quantityForRead, ushort startingAddress, ReadOnlyMemory<byte> bytes)
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
    public static Task<IModbusResponse> ReadWriteMultipleRegistersAsync(this IModbusMaster master, byte slaveId, ushort startingAddressForRead, ushort quantityForRead, ushort startingAddress, ReadOnlyMemory<byte> bytes)
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
    [AsyncToSyncWarning]
    public static ReadOnlyMemory<bool> ReadCoils(this IModbusMaster master, byte slaveId, ushort startingAddress, ushort quantity)
    {
        return master.ReadCoilsAsync(slaveId, startingAddress, quantity, 1000, CancellationToken.None).GetFalseAwaitResult();
    }

    /// <summary>
    /// 从指定站点读离散输入状态（FC2），默认超时时间为1000ms。
    /// </summary>
    /// <param name="master">通讯客户端</param>
    /// <param name="slaveId">站点号</param>
    /// <param name="startingAddress">起始位置（从0开始）</param>
    /// <param name="quantity">读取数量</param>
    /// <returns>读取到的值集合</returns>
    [AsyncToSyncWarning]
    public static ReadOnlyMemory<bool> ReadDiscreteInputs(this IModbusMaster master, byte slaveId, ushort startingAddress, ushort quantity)
    {
        return master.ReadDiscreteInputsAsync(slaveId, startingAddress, quantity, 1000, CancellationToken.None).GetFalseAwaitResult();
    }

    /// <summary>
    /// 从指定站点读保持寄存器（FC3），默认超时时间为1000ms。
    /// </summary>
    /// <param name="master">通讯客户端</param>
    /// <param name="slaveId">站点号</param>
    /// <param name="startingAddress">起始位置（从0开始）</param>
    /// <param name="quantity">读取数量</param>
    /// <returns>响应值</returns>
    [AsyncToSyncWarning]
    public static IModbusResponse ReadHoldingRegisters(this IModbusMaster master, byte slaveId, ushort startingAddress, ushort quantity)
    {
        return master.ReadHoldingRegistersAsync(slaveId, startingAddress, quantity, 1000, CancellationToken.None).GetFalseAwaitResult();
    }

    /// <summary>
    /// 从指定站点读输入寄存器（FC4），默认超时时间为1000ms。
    /// </summary>
    /// <param name="master">通讯客户端</param>
    /// <param name="slaveId">站点号</param>
    /// <param name="startingAddress">起始位置（从0开始）</param>
    /// <param name="quantity">读取数量</param>
    /// <returns>响应值</returns>
    [AsyncToSyncWarning]
    public static IModbusResponse ReadInputRegisters(this IModbusMaster master, byte slaveId, ushort startingAddress, ushort quantity)
    {
        return master.ReadInputRegistersAsync(slaveId, startingAddress, quantity, 1000, CancellationToken.None).GetFalseAwaitResult();
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
    public static Task<ReadOnlyMemory<bool>> ReadCoilsAsync(this IModbusMaster master, byte slaveId, ushort startingAddress, ushort quantity)
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
    public static Task<ReadOnlyMemory<bool>> ReadDiscreteInputsAsync(this IModbusMaster master, byte slaveId, ushort startingAddress, ushort quantity)
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
    [AsyncToSyncWarning]
    public static IModbusResponse WriteMultipleCoils(this IModbusMaster master, byte slaveId, ushort startingAddress, ReadOnlyMemory<bool> values)
    {
        return master.WriteMultipleCoilsAsync(slaveId, startingAddress, values, 1000, CancellationToken.None).GetFalseAwaitResult();
    }

    /// <summary>
    /// 向指定站点写入多个寄存器（FC16），默认超时时间为1000ms。
    /// </summary>
    /// <param name="master">通讯客户端</param>
    /// <param name="slaveId">站点号</param>
    /// <param name="startingAddress">起始位置（从0开始）</param>
    /// <param name="bytes">待写入集合</param>
    [AsyncToSyncWarning]
    public static IModbusResponse WriteMultipleRegisters(this IModbusMaster master, byte slaveId, ushort startingAddress, ReadOnlyMemory<byte> bytes)
    {
        return master.WriteMultipleRegistersAsync(slaveId, startingAddress, bytes, 1000, CancellationToken.None).GetFalseAwaitResult();
    }

    /// <summary>
    /// 向指定站点写入单个线圈（FC5），默认超时时间为1000ms。
    /// </summary>
    /// <param name="master">通讯客户端</param>
    /// <param name="slaveId">站点号</param>
    /// <param name="startingAddress">起始位置（从0开始）</param>
    /// <param name="value">待写入数值</param>
    [AsyncToSyncWarning]
    public static IModbusResponse WriteSingleCoil(this IModbusMaster master, byte slaveId, ushort startingAddress, bool value)
    {
        return master.WriteSingleCoilAsync(slaveId, startingAddress, value, 1000, CancellationToken.None).GetFalseAwaitResult();
    }

    /// <summary>
    /// 向指定站点写入单个寄存器（FC6），默认超时时间为1000ms。
    /// </summary>
    /// <param name="master">通讯客户端</param>
    /// <param name="slaveId">站点号</param>
    /// <param name="startingAddress">起始位置（从0开始）</param>
    /// <param name="value">待写入数值</param>
    [AsyncToSyncWarning]
    public static IModbusResponse WriteSingleRegister(this IModbusMaster master, byte slaveId, ushort startingAddress, short value)
    {
        return master.WriteSingleRegisterAsync(slaveId, startingAddress, value, 1000, CancellationToken.None).GetFalseAwaitResult();
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
    public static Task<IModbusResponse> WriteMultipleCoilsAsync(this IModbusMaster master, byte slaveId, ushort startingAddress, ReadOnlyMemory<bool> values)
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
    public static Task<IModbusResponse> WriteMultipleRegistersAsync(this IModbusMaster master, byte slaveId, ushort startingAddress, ReadOnlyMemory<byte> bytes)
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
    public static Task<IModbusResponse> WriteSingleCoilAsync(this IModbusMaster master, byte slaveId, ushort startingAddress, bool value)
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
    public static Task<IModbusResponse> WriteSingleRegisterAsync(this IModbusMaster master, byte slaveId, ushort startingAddress, short value)
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
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>读取到的值集合</returns>
    [AsyncToSyncWarning]
    public static ReadOnlyMemory<bool> ReadCoils(this IModbusMaster master, byte slaveId, ushort startingAddress, ushort quantity, int millisecondsTimeout, CancellationToken cancellationToken)
    {
        var request = new ModbusRequest(slaveId, FunctionCode.ReadCoils, startingAddress, quantity);

        var response = master.SendModbusRequestAsync(request, millisecondsTimeout, cancellationToken).GetFalseAwaitResult();
        return TouchSocketBitConverter.Default.ToValues<bool>(response.Data.Span).Slice(0, quantity);
    }

    /// <summary>
    /// 从指定站点读离散输入状态（FC2）。
    /// </summary>
    /// <param name="master">通讯客户端</param>
    /// <param name="slaveId">站点号</param>
    /// <param name="startingAddress">起始位置（从0开始）</param>
    /// <param name="quantity">读取数量</param>
    /// <param name="millisecondsTimeout">超时时间，单位（ms）</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>读取到的值集合</returns>
    [AsyncToSyncWarning]
    public static ReadOnlyMemory<bool> ReadDiscreteInputs(this IModbusMaster master, byte slaveId, ushort startingAddress, ushort quantity, int millisecondsTimeout, CancellationToken cancellationToken)
    {
        var request = new ModbusRequest(slaveId, FunctionCode.ReadDiscreteInputs, startingAddress, quantity);

        var response = master.SendModbusRequestAsync(request, millisecondsTimeout, cancellationToken).GetFalseAwaitResult();
        return TouchSocketBitConverter.Default.ToValues<bool>(response.Data.Span).Slice(0, quantity);
    }

    /// <summary>
    /// 从指定站点读保持寄存器（FC3）。
    /// </summary>
    /// <param name="master">通讯客户端</param>
    /// <param name="slaveId">站点号</param>
    /// <param name="startingAddress">起始位置（从0开始）</param>
    /// <param name="quantity">读取数量</param>
    /// <param name="millisecondsTimeout">超时时间，单位（ms）</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>响应值</returns>
    [AsyncToSyncWarning]
    public static IModbusResponse ReadHoldingRegisters(this IModbusMaster master, byte slaveId, ushort startingAddress, ushort quantity, int millisecondsTimeout, CancellationToken cancellationToken)
    {
        var request = new ModbusRequest(slaveId, FunctionCode.ReadHoldingRegisters, startingAddress, quantity);

        var response = master.SendModbusRequestAsync(request, millisecondsTimeout, cancellationToken).GetFalseAwaitResult();
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
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>响应值</returns>
    [AsyncToSyncWarning]
    public static IModbusResponse ReadInputRegisters(this IModbusMaster master, byte slaveId, ushort startingAddress, ushort quantity, int millisecondsTimeout, CancellationToken cancellationToken)
    {
        var request = new ModbusRequest(slaveId, FunctionCode.ReadInputRegisters, startingAddress, quantity);

        var response = master.SendModbusRequestAsync(request, millisecondsTimeout, cancellationToken).GetFalseAwaitResult();
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
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>读取到的值集合</returns>
    public static async Task<ReadOnlyMemory<bool>> ReadCoilsAsync(this IModbusMaster master, byte slaveId, ushort startingAddress, ushort quantity, int millisecondsTimeout, CancellationToken cancellationToken)
    {
        var request = new ModbusRequest(slaveId, FunctionCode.ReadCoils, startingAddress, quantity);

        var response = await master.SendModbusRequestAsync(request, millisecondsTimeout, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        return TouchSocketBitConverter.Default.ToValues<bool>(response.Data.Span).Slice(0, quantity);
    }

    /// <summary>
    /// 异步从指定站点读离散输入状态（FC2）。
    /// </summary>
    /// <param name="master">通讯客户端</param>
    /// <param name="slaveId">站点号</param>
    /// <param name="startingAddress">起始位置（从0开始）</param>
    /// <param name="quantity">读取数量</param>
    /// <param name="millisecondsTimeout">超时时间，单位（ms）</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>读取到的值集合</returns>
    public static async Task<ReadOnlyMemory<bool>> ReadDiscreteInputsAsync(this IModbusMaster master, byte slaveId, ushort startingAddress, ushort quantity, int millisecondsTimeout, CancellationToken cancellationToken)
    {
        var request = new ModbusRequest(slaveId, FunctionCode.ReadDiscreteInputs, startingAddress, quantity);

        var response = await master.SendModbusRequestAsync(request, millisecondsTimeout, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        return TouchSocketBitConverter.Default.ToValues<bool>(response.Data.Span).Slice(0, quantity);
    }

    /// <summary>
    /// 异步从指定站点读保持寄存器（FC3）。
    /// </summary>
    /// <param name="master">通讯客户端</param>
    /// <param name="slaveId">站点号</param>
    /// <param name="startingAddress">起始位置（从0开始）</param>
    /// <param name="quantity">读取数量</param>
    /// <param name="millisecondsTimeout">超时时间，单位（ms）</param>
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>响应值</returns>
    public static async Task<IModbusResponse> ReadHoldingRegistersAsync(this IModbusMaster master, byte slaveId, ushort startingAddress, ushort quantity, int millisecondsTimeout, CancellationToken cancellationToken)
    {
        var request = new ModbusRequest(slaveId, FunctionCode.ReadHoldingRegisters, startingAddress, quantity);

        var response = await master.SendModbusRequestAsync(request, millisecondsTimeout, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>响应值</returns>
    public static async Task<IModbusResponse> ReadInputRegistersAsync(this IModbusMaster master, byte slaveId, ushort startingAddress, ushort quantity, int millisecondsTimeout, CancellationToken cancellationToken)
    {
        var request = new ModbusRequest(slaveId, FunctionCode.ReadInputRegisters, startingAddress, quantity);

        var response = await master.SendModbusRequestAsync(request, millisecondsTimeout, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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
    /// <param name="cancellationToken">可取消令箭</param>
    [AsyncToSyncWarning]
    public static IModbusResponse WriteMultipleCoils(this IModbusMaster master, byte slaveId, ushort startingAddress, ReadOnlyMemory<bool> values, int millisecondsTimeout, CancellationToken cancellationToken)
    {
        var request = new ModbusRequest(slaveId, FunctionCode.WriteMultipleCoils);
        request.StartingAddress = startingAddress;
        request.SetValue(values.Span);

        return master.SendModbusRequestAsync(request, millisecondsTimeout, cancellationToken).GetFalseAwaitResult();
    }

    /// <summary>
    /// 向指定站点写入多个寄存器（FC16）。
    /// </summary>
    /// <param name="master">通讯客户端</param>
    /// <param name="slaveId">站点号</param>
    /// <param name="startingAddress">起始位置（从0开始）</param>
    /// <param name="bytes">待写入集合</param>
    /// <param name="millisecondsTimeout">超时时间，单位（ms）</param>
    /// <param name="cancellationToken">可取消令箭</param>
    [AsyncToSyncWarning]
    public static IModbusResponse WriteMultipleRegisters(this IModbusMaster master, byte slaveId, ushort startingAddress, ReadOnlyMemory<byte> bytes, int millisecondsTimeout, CancellationToken cancellationToken)
    {
        var request = new ModbusRequest(slaveId, FunctionCode.WriteMultipleRegisters);
        request.StartingAddress = startingAddress;
        request.SetValue(bytes);

        return master.SendModbusRequestAsync(request, millisecondsTimeout, cancellationToken).GetFalseAwaitResult();
    }

    /// <summary>
    /// 向指定站点写入单个线圈（FC5）。
    /// </summary>
    /// <param name="master">通讯客户端</param>
    /// <param name="slaveId">站点号</param>
    /// <param name="startingAddress">起始位置（从0开始）</param>
    /// <param name="value">待写入数值</param>
    /// <param name="millisecondsTimeout">超时时间，单位（ms）</param>
    /// <param name="cancellationToken">可取消令箭</param>
    [AsyncToSyncWarning]
    public static IModbusResponse WriteSingleCoil(this IModbusMaster master, byte slaveId, ushort startingAddress, bool value, int millisecondsTimeout, CancellationToken cancellationToken)
    {
        var request = new ModbusRequest(slaveId, FunctionCode.WriteSingleCoil);
        request.StartingAddress = startingAddress;
        request.SetValue(value);

        return master.SendModbusRequestAsync(request, millisecondsTimeout, cancellationToken).GetFalseAwaitResult();
    }

    /// <summary>
    /// 向指定站点写入单个寄存器（FC6）
    /// </summary>
    /// <param name="master">通讯客户端</param>
    /// <param name="slaveId">站点号</param>
    /// <param name="startingAddress">起始位置（从0开始）</param>
    /// <param name="value">待写入数值</param>
    /// <param name="millisecondsTimeout">超时时间，单位（ms）</param>
    /// <param name="cancellationToken">可取消令箭</param>
    [AsyncToSyncWarning]
    public static IModbusResponse WriteSingleRegister(this IModbusMaster master, byte slaveId, ushort startingAddress, short value, int millisecondsTimeout, CancellationToken cancellationToken)
    {
        var request = new ModbusRequest(slaveId, FunctionCode.WriteSingleRegister);
        request.StartingAddress = startingAddress;
        request.SetValue(value);

        return master.SendModbusRequestAsync(request, millisecondsTimeout, cancellationToken).GetFalseAwaitResult();
    }

    /// <summary>
    /// 向指定站点写入单个寄存器（FC6）
    /// </summary>
    /// <param name="master">通讯客户端</param>
    /// <param name="slaveId">站点号</param>
    /// <param name="startingAddress">起始位置（从0开始）</param>
    /// <param name="value">待写入数值</param>
    /// <param name="millisecondsTimeout">超时时间，单位（ms）</param>
    /// <param name="cancellationToken">可取消令箭</param>
    [AsyncToSyncWarning]
    public static IModbusResponse WriteSingleRegister(this IModbusMaster master, byte slaveId, ushort startingAddress, ushort value, int millisecondsTimeout, CancellationToken cancellationToken)
    {
        var request = new ModbusRequest(slaveId, FunctionCode.WriteSingleRegister);
        request.StartingAddress = startingAddress;
        request.SetValue(value);

        return master.SendModbusRequestAsync(request, millisecondsTimeout, cancellationToken).GetFalseAwaitResult();
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
    /// <param name="cancellationToken">可取消令箭</param>
    public static async Task<IModbusResponse> WriteMultipleCoilsAsync(this IModbusMaster master, byte slaveId, ushort startingAddress, ReadOnlyMemory<bool> values, int millisecondsTimeout, CancellationToken cancellationToken)
    {
        var request = new ModbusRequest(slaveId, FunctionCode.WriteMultipleCoils);
        request.StartingAddress = startingAddress;
        request.SetValue(values.Span);

        return await master.SendModbusRequestAsync(request, millisecondsTimeout, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 异步向指定站点写入多个寄存器（FC16）。
    /// </summary>
    /// <param name="master">通讯客户端</param>
    /// <param name="slaveId">站点号</param>
    /// <param name="startingAddress">起始位置（从0开始）</param>
    /// <param name="bytes">待写入集合</param>
    /// <param name="millisecondsTimeout">超时时间，单位（ms）</param>
    /// <param name="cancellationToken">可取消令箭</param>
    public static async Task<IModbusResponse> WriteMultipleRegistersAsync(this IModbusMaster master, byte slaveId, ushort startingAddress, ReadOnlyMemory<byte> bytes, int millisecondsTimeout, CancellationToken cancellationToken)
    {
        var request = new ModbusRequest(slaveId, FunctionCode.WriteMultipleRegisters);
        request.StartingAddress = startingAddress;
        request.SetValue(bytes);

        return await master.SendModbusRequestAsync(request, millisecondsTimeout, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 异步向指定站点写入单个线圈（FC5）。
    /// </summary>
    /// <param name="master">通讯客户端</param>
    /// <param name="slaveId">站点号</param>
    /// <param name="startingAddress">起始位置（从0开始）</param>
    /// <param name="value">待写入数值</param>
    /// <param name="millisecondsTimeout">超时时间，单位（ms）</param>
    /// <param name="cancellationToken">可取消令箭</param>
    public static async Task<IModbusResponse> WriteSingleCoilAsync(this IModbusMaster master, byte slaveId, ushort startingAddress, bool value, int millisecondsTimeout, CancellationToken cancellationToken)
    {
        var request = new ModbusRequest(slaveId, FunctionCode.WriteSingleCoil);
        request.StartingAddress = startingAddress;
        request.SetValue(value);

        return await master.SendModbusRequestAsync(request, millisecondsTimeout, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 异步向指定站点写入单个寄存器（FC6）
    /// </summary>
    /// <param name="master">通讯客户端</param>
    /// <param name="slaveId">站点号</param>
    /// <param name="startingAddress">起始位置（从0开始）</param>
    /// <param name="value">待写入数值</param>
    /// <param name="millisecondsTimeout">超时时间，单位（ms）</param>
    /// <param name="cancellationToken">可取消令箭</param>
    public static async Task<IModbusResponse> WriteSingleRegisterAsync(this IModbusMaster master, byte slaveId, ushort startingAddress, short value, int millisecondsTimeout, CancellationToken cancellationToken)
    {
        var request = new ModbusRequest(slaveId, FunctionCode.WriteSingleRegister);
        request.StartingAddress = startingAddress;
        request.SetValue(value);

        return await master.SendModbusRequestAsync(request, millisecondsTimeout, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 异步向指定站点写入单个寄存器（FC6）
    /// </summary>
    /// <param name="master">通讯客户端</param>
    /// <param name="slaveId">站点号</param>
    /// <param name="startingAddress">起始位置（从0开始）</param>
    /// <param name="value">待写入数值</param>
    /// <param name="millisecondsTimeout">超时时间，单位（ms）</param>
    /// <param name="cancellationToken">可取消令箭</param>
    public static async Task<IModbusResponse> WriteSingleRegisterAsync(this IModbusMaster master, byte slaveId, ushort startingAddress, ushort value, int millisecondsTimeout, CancellationToken cancellationToken)
    {
        var request = new ModbusRequest(slaveId, FunctionCode.WriteSingleRegister);
        request.StartingAddress = startingAddress;
        request.SetValue(value);

        return await master.SendModbusRequestAsync(request, millisecondsTimeout, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>响应值</returns>
    public static IModbusResponse ReadWriteMultipleRegisters(this IModbusMaster master, byte slaveId, ushort startingAddressForRead, ushort quantityForRead, ushort startingAddress, ReadOnlyMemory<byte> bytes, int millisecondsTimeout, CancellationToken cancellationToken)
    {
        var request = new ModbusRequest(slaveId, FunctionCode.ReadWriteMultipleRegisters);
        request.StartingAddress = startingAddress;
        request.ReadStartAddress = startingAddressForRead;
        request.ReadQuantity = quantityForRead;
        request.SetValue(bytes);
        return master.SendModbusRequestAsync(request, millisecondsTimeout, cancellationToken).GetFalseAwaitResult();
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
    /// <param name="cancellationToken">可取消令箭</param>
    /// <returns>响应值</returns>
    public static async Task<IModbusResponse> ReadWriteMultipleRegistersAsync(this IModbusMaster master, byte slaveId, ushort startingAddressForRead, ushort quantityForRead, ushort startingAddress, ReadOnlyMemory<byte> bytes, int millisecondsTimeout, CancellationToken cancellationToken)
    {
        var request = new ModbusRequest(slaveId, FunctionCode.ReadWriteMultipleRegisters);
        request.StartingAddress = startingAddress;
        request.ReadStartAddress = startingAddressForRead;
        request.ReadQuantity = quantityForRead;
        request.SetValue(bytes);
        return await master.SendModbusRequestAsync(request, millisecondsTimeout, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    #endregion ReadWrite

    #region SendModbusRequestAsync
    public static async Task<IModbusResponse> SendModbusRequestAsync(this IModbusMaster master, ModbusRequest request, int millisecondsTimeout, CancellationToken cancellationToken)
    {
        if (millisecondsTimeout == Timeout.Infinite)
        {
            return await master.SendModbusRequestAsync(request, cancellationToken).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }

        using (var timeoutCts = new CancellationTokenSource(millisecondsTimeout))
        {
            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token))
            {
                try
                {
                    return await master.SendModbusRequestAsync(request, linkedCts.Token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
                catch (OperationCanceledException)
                {
                    if (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
                    {
                        throw new TimeoutException("The operation has timed out.");
                    }
                    throw;
                }
            }
        }
    }
    #endregion
}