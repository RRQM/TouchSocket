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
/// ModbusTcpRequest
/// </summary>
internal sealed class ModbusTcpRequest : ModbusTcpBase, IBytesBuilder, IRequestInfo
{
    private readonly ModbusFunctionHandlerRegistry m_registry;

    /// <summary>
    /// 从<see cref="ModbusRequest"/>创建一个ModbusTcpRequest
    /// </summary>
    public ModbusTcpRequest(ushort transactionId, IModbusRequest request, ModbusFunctionHandlerRegistry registry)
    {
        this.m_registry = registry;
        this.TransactionId = transactionId;
        this.ProtocolId = 0;
        this.SlaveId = request.SlaveId;
        this.FunctionCode = request.FunctionCode;
        this.StartingAddress = request.StartingAddress;
        this.Quantity = request.Quantity;
        this.Data = request.Data;

        if (request is IModbusReadWriteRequest readWriteRequest)
        {
            this.ReadStartAddress = readWriteRequest.ReadStartAddress;
            this.ReadQuantity = readWriteRequest.ReadQuantity;
        }
    }

    /// <inheritdoc/>
    public int MaxLength => 2048;

    /// <inheritdoc/>
    public void Build<TWriter>(ref TWriter writer)
        where TWriter : IBytesWriter
    {
        var handler = this.m_registry.GetHandler(this.FunctionCode)
            ?? throw new InvalidOperationException($"不支持的Modbus功能码: {this.FunctionCode}");

        // Modbus单帧不超过2048字节，直接预留，无需提前计算PDU长度
        var memory = writer.GetMemory(2048);
        var bytesWriter = new BytesWriter(memory);

        // TCP帧格式：TransactionId(2) + ProtocolId(2) + Length(2) + SlaveId(1) + FuncCode(1) + PDU
        WriterExtension.WriteValue<BytesWriter, ushort>(ref bytesWriter, this.TransactionId, EndianType.Big);
        WriterExtension.WriteValue<BytesWriter, ushort>(ref bytesWriter, this.ProtocolId, EndianType.Big);
        // 占位Length字段（2字节），写完PDU后回填
        var lengthAnchor = new WriterAnchor<BytesWriter>(ref bytesWriter, 2);
        WriterExtension.WriteValue<BytesWriter, byte>(ref bytesWriter, this.SlaveId);
        WriterExtension.WriteValue<BytesWriter, byte>(ref bytesWriter, (byte)this.FunctionCode);
        handler.BuildRequestPdu(ref bytesWriter, this);

        // length = SlaveId(1) + FuncCode(1) + PDU body，正好是MBAP Length字段的值
        var lengthSpan = lengthAnchor.Rewind(ref bytesWriter, out var length);
        lengthSpan[0] = (byte)(length >> 8);
        lengthSpan[1] = (byte)length;

        writer.Advance((int)bytesWriter.WrittenCount);
    }
}
