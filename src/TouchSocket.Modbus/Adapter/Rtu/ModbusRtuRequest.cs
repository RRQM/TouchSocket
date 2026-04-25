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

internal sealed class ModbusRtuRequest : ModbusRtuBase, IBytesBuilder, IRequestInfo
{
    private readonly ModbusFunctionHandlerRegistry m_registry;

    public ModbusRtuRequest(IModbusRequest request, ModbusFunctionHandlerRegistry registry)
    {
        this.m_registry = registry;
        this.SlaveId = request.SlaveId;
        this.FunctionCode = request.FunctionCode;
        this.Quantity = request.Quantity;
        this.StartingAddress = request.StartingAddress;
        this.Data = request.Data;

        if (request is IModbusReadWriteRequest readWriteRequest)
        {
            this.ReadStartAddress = readWriteRequest.ReadStartAddress;
            this.ReadQuantity = readWriteRequest.ReadQuantity;
        }
    }

    /// <inheritdoc/>
    public int MaxLength => 1024;

    /// <inheritdoc/>
    public void Build<TWriter>(ref TWriter writer)
        where TWriter : IBytesWriter
    {
        var handler = this.m_registry.GetHandler(this.FunctionCode)
            ?? throw new InvalidOperationException($"不支持的Modbus功能码: {this.FunctionCode}");

        var memory = writer.GetMemory(this.MaxLength);
        var bytesWriter = new BytesWriter(memory);

        WriterExtension.WriteValue<BytesWriter, byte>(ref bytesWriter, this.SlaveId);
        WriterExtension.WriteValue<BytesWriter, byte>(ref bytesWriter, (byte)this.FunctionCode);
        handler.BuildRequestPdu(ref bytesWriter, this);

        this.Crc = TouchSocketModbusUtility.ToModbusCrcValue(bytesWriter.Span);
        WriterExtension.WriteValue<BytesWriter, ushort>(ref bytesWriter, this.Crc, EndianType.Big);
        writer.Advance((int)bytesWriter.WrittenCount);
    }
}