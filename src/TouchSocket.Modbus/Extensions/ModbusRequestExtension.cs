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
/// ModbusRequestExtension
/// </summary>
public static class ModbusRequestExtension
{
    /// <summary>
    /// 将<see cref="IModbusRequest"/>构建为ModbusTcp完整请求报文。
    /// </summary>
    /// <param name="request">Modbus请求。</param>
    /// <param name="transactionId">事务处理标识符。</param>
    /// <param name="registry">功能码处理器注册表。</param>
    public static ReadOnlyMemory<byte> ToModbusTcpRequestBytes(this IModbusRequest request, ushort transactionId, ModbusFunctionHandlerRegistry registry = default)
    {
        return CreateTcpRequestBuilder(request, transactionId, registry).BuildAsBytes();
    }

    /// <summary>
    /// 将<see cref="IModbusRequest"/>构建为ModbusUdp完整请求报文。
    /// </summary>
    /// <param name="request">Modbus请求。</param>
    /// <param name="transactionId">事务处理标识符。</param>
    /// <param name="registry">功能码处理器注册表。</param>
    public static ReadOnlyMemory<byte> ToModbusUdpRequestBytes(this IModbusRequest request, ushort transactionId, ModbusFunctionHandlerRegistry registry = default)
    {
        return CreateTcpRequestBuilder(request, transactionId, registry).BuildAsBytes();
    }

    /// <summary>
    /// 将<see cref="IModbusRequest"/>构建为ModbusRtu完整请求报文。
    /// </summary>
    /// <param name="request">Modbus请求。</param>
    /// <param name="registry">功能码处理器注册表。</param>
    public static ReadOnlyMemory<byte> ToModbusRtuRequestBytes(this IModbusRequest request, ModbusFunctionHandlerRegistry registry = default)
    {
        return CreateRtuRequestBuilder(request, registry).BuildAsBytes();
    }

    /// <summary>
    /// 将<see cref="IModbusRequest"/>构建为ModbusRtuOverTcp完整请求报文。
    /// </summary>
    /// <param name="request">Modbus请求。</param>
    /// <param name="registry">功能码处理器注册表。</param>
    public static ReadOnlyMemory<byte> ToModbusRtuOverTcpRequestBytes(this IModbusRequest request, ModbusFunctionHandlerRegistry registry = default)
    {
        return CreateRtuRequestBuilder(request, registry).BuildAsBytes();
    }

    /// <summary>
    /// 将<see cref="IModbusRequest"/>构建为ModbusRtuOverUdp完整请求报文。
    /// </summary>
    /// <param name="request">Modbus请求。</param>
    /// <param name="registry">功能码处理器注册表。</param>
    public static ReadOnlyMemory<byte> ToModbusRtuOverUdpRequestBytes(this IModbusRequest request, ModbusFunctionHandlerRegistry registry = default)
    {
        return CreateRtuRequestBuilder(request, registry).BuildAsBytes();
    }

    /// <summary>
    /// 将<see cref="IModbusRequest"/>写入为ModbusTcp完整请求报文。
    /// </summary>
    /// <typeparam name="TWriter">写入器类型。</typeparam>
    /// <param name="request">Modbus请求。</param>
    /// <param name="writer">字节写入器。</param>
    /// <param name="transactionId">事务处理标识符。</param>
    /// <param name="registry">功能码处理器注册表。</param>
    public static void BuildModbusTcpRequest<TWriter>(this IModbusRequest request, ref TWriter writer, ushort transactionId, ModbusFunctionHandlerRegistry registry = default)
        where TWriter : IBytesWriter
    {
        CreateTcpRequestBuilder(request, transactionId, registry).Build(ref writer);
    }

    /// <summary>
    /// 将<see cref="IModbusRequest"/>写入为ModbusUdp完整请求报文。
    /// </summary>
    /// <typeparam name="TWriter">写入器类型。</typeparam>
    /// <param name="request">Modbus请求。</param>
    /// <param name="writer">字节写入器。</param>
    /// <param name="transactionId">事务处理标识符。</param>
    /// <param name="registry">功能码处理器注册表。</param>
    public static void BuildModbusUdpRequest<TWriter>(this IModbusRequest request, ref TWriter writer, ushort transactionId, ModbusFunctionHandlerRegistry registry = default)
        where TWriter : IBytesWriter
    {
        CreateTcpRequestBuilder(request, transactionId, registry).Build(ref writer);
    }

    /// <summary>
    /// 将<see cref="IModbusRequest"/>写入为ModbusRtu完整请求报文。
    /// </summary>
    /// <typeparam name="TWriter">写入器类型。</typeparam>
    /// <param name="request">Modbus请求。</param>
    /// <param name="writer">字节写入器。</param>
    /// <param name="registry">功能码处理器注册表。</param>
    public static void BuildModbusRtuRequest<TWriter>(this IModbusRequest request, ref TWriter writer, ModbusFunctionHandlerRegistry registry = default)
        where TWriter : IBytesWriter
    {
        CreateRtuRequestBuilder(request, registry).Build(ref writer);
    }

    /// <summary>
    /// 将<see cref="IModbusRequest"/>写入为ModbusRtuOverTcp完整请求报文。
    /// </summary>
    /// <typeparam name="TWriter">写入器类型。</typeparam>
    /// <param name="request">Modbus请求。</param>
    /// <param name="writer">字节写入器。</param>
    /// <param name="registry">功能码处理器注册表。</param>
    public static void BuildModbusRtuOverTcpRequest<TWriter>(this IModbusRequest request, ref TWriter writer, ModbusFunctionHandlerRegistry registry = default)
        where TWriter : IBytesWriter
    {
        CreateRtuRequestBuilder(request, registry).Build(ref writer);
    }

    /// <summary>
    /// 将<see cref="IModbusRequest"/>写入为ModbusRtuOverUdp完整请求报文。
    /// </summary>
    /// <typeparam name="TWriter">写入器类型。</typeparam>
    /// <param name="request">Modbus请求。</param>
    /// <param name="writer">字节写入器。</param>
    /// <param name="registry">功能码处理器注册表。</param>
    public static void BuildModbusRtuOverUdpRequest<TWriter>(this IModbusRequest request, ref TWriter writer, ModbusFunctionHandlerRegistry registry = default)
        where TWriter : IBytesWriter
    {
        CreateRtuRequestBuilder(request, registry).Build(ref writer);
    }

    private static IBytesBuilder CreateTcpRequestBuilder(IModbusRequest request, ushort transactionId, ModbusFunctionHandlerRegistry registry)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }
        return request is IBytesBuilder builder
            ? builder
            : new ModbusTcpRequest(transactionId, request, registry ?? ModbusFunctionHandlerRegistry.Default);
    }

    private static IBytesBuilder CreateRtuRequestBuilder(IModbusRequest request, ModbusFunctionHandlerRegistry registry)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }
        return request is IBytesBuilder builder
            ? builder
            : new ModbusRtuRequest(request, registry ?? ModbusFunctionHandlerRegistry.Default);
    }
}
