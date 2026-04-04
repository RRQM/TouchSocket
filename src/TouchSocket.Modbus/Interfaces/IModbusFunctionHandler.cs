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
/// Modbus功能码处理器接口，负责特定功能码的请求PDU构建与响应PDU解析。
/// 通过实现此接口并注册到<see cref="ModbusFunctionHandlerRegistry"/>，可扩展自定义功能码。
/// </summary>
public interface IModbusFunctionHandler
{
    /// <summary>
    /// 获取此处理器支持的功能码
    /// </summary>
    FunctionCode FunctionCode { get; }

    /// <summary>
    /// 构建请求PDU（功能码字节之后的数据，不含站号、功能码、CRC/MBAP帧头尾）
    /// </summary>
    /// <typeparam name="TWriter">字节写入器类型</typeparam>
    /// <param name="writer">字节写入器</param>
    /// <param name="request">Modbus请求</param>
    void BuildRequestPdu<TWriter>(ref TWriter writer, IModbusRequest request) where TWriter : IBytesWriter;

    /// <summary>
    /// RTU模式下，读取功能码后第一个字节（<paramref name="firstByte"/>）之后，
    /// 还需要继续读取的字节数（不含CRC校验的2字节）。
    /// </summary>
    /// <param name="firstByte">功能码之后的第一个字节</param>
    /// <returns>firstByte之后需要继续读取的字节数（不含CRC）</returns>
    int GetRtuResponseAfterFirstByteLength(byte firstByte);

    /// <summary>
    /// 解析响应PDU体。PDU体从功能码之后的第一个字节开始，不含站号、功能码、CRC。
    /// </summary>
    /// <param name="pduBody">PDU体数据（从功能码后第一个字节开始）</param>
    /// <returns>解析后的响应数据</returns>
    ModbusResponseData ParseResponsePdu(ReadOnlySpan<byte> pduBody);
}
