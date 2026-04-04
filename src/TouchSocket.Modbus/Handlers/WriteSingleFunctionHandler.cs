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
/// 写单个线圈/保持寄存器功能码处理器（FC05, FC06）
/// </summary>
internal sealed class WriteSingleFunctionHandler : IModbusFunctionHandler
{
    internal WriteSingleFunctionHandler(FunctionCode functionCode)
    {
        this.FunctionCode = functionCode;
    }

    /// <inheritdoc/>
    public FunctionCode FunctionCode { get; }

    /// <inheritdoc/>
    public void BuildRequestPdu<TWriter>(ref TWriter writer, IModbusRequest request) where TWriter : IBytesWriter
    {
        WriterExtension.WriteValue<TWriter, ushort>(ref writer, request.StartingAddress, EndianType.Big);
        writer.Write(request.Data.Span);
    }

    /// <inheritdoc/>
    public int GetRtuResponseAfterFirstByteLength(byte firstByte)
    {
        return 3;
    }

    /// <inheritdoc/>
    public ModbusResponseData ParseResponsePdu(ReadOnlySpan<byte> pduBody)
    {
        var startingAddress = TouchSocketBitConverter.BigEndian.To<ushort>(pduBody);
        return new ModbusResponseData(pduBody.Slice(2).ToArray(), startingAddress, 0);
    }
}
