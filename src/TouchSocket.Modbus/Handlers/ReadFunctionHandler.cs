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
/// 读线圈/离散输入/保持寄存器/输入寄存器功能码处理器（FC01, FC02, FC03, FC04）
/// </summary>
internal sealed class ReadFunctionHandler : IModbusFunctionHandler
{
    internal ReadFunctionHandler(FunctionCode functionCode)
    {
        this.FunctionCode = functionCode;
    }

    /// <inheritdoc/>
    public FunctionCode FunctionCode { get; }

    /// <inheritdoc/>
    public void BuildRequestPdu<TWriter>(ref TWriter writer, IModbusRequest request) where TWriter : IBytesWriter
    {
        WriterExtension.WriteValue<TWriter, ushort>(ref writer, request.StartingAddress, EndianType.Big);
        WriterExtension.WriteValue<TWriter, ushort>(ref writer, request.Quantity, EndianType.Big);
    }

    /// <inheritdoc/>
    public int GetRtuResponseAfterFirstByteLength(byte firstByte)
    {
        return firstByte;
    }

    /// <inheritdoc/>
    public ModbusResponseData ParseResponsePdu(ReadOnlySpan<byte> pduBody)
    {
        return new ModbusResponseData(pduBody.Slice(1).ToArray());
    }
}
