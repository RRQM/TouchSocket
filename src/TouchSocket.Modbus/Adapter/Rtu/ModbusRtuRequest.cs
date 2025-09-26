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

internal sealed class ModbusRtuRequest : ModbusRtuBase, IRequestInfoBuilder, IRequestInfo
{
    public ModbusRtuRequest(ModbusRequest request)
    {
        this.SlaveId = request.SlaveId;
        this.FunctionCode = request.FunctionCode;
        this.Quantity = request.Quantity;
        this.StartingAddress = request.StartingAddress;
        this.Data = request.Data;
        this.ReadStartAddress = request.ReadStartAddress;
        this.ReadQuantity = request.ReadQuantity;
    }

    /// <inheritdoc/>
    public int MaxLength => 1024;

    /// <inheritdoc/>
    public void Build<TWriter>(ref TWriter writer)
        where TWriter : IBytesWriter

    {
        var memory = writer.GetMemory(this.MaxLength);
        var bytesWriter = new BytesWriter(memory);
        if ((byte)this.FunctionCode <= 4)
        {
            WriterExtension.WriteValue<BytesWriter, byte>(ref bytesWriter, this.SlaveId);
            WriterExtension.WriteValue<BytesWriter, byte>(ref bytesWriter, (byte)this.FunctionCode);
            WriterExtension.WriteValue<BytesWriter, ushort>(ref bytesWriter, this.StartingAddress, EndianType.Big);
            WriterExtension.WriteValue<BytesWriter, ushort>(ref bytesWriter, this.Quantity, EndianType.Big);
        }
        else if (this.FunctionCode == FunctionCode.WriteSingleCoil || this.FunctionCode == FunctionCode.WriteSingleRegister)
        {
            WriterExtension.WriteValue<BytesWriter, byte>(ref bytesWriter, this.SlaveId);
            WriterExtension.WriteValue<BytesWriter, byte>(ref bytesWriter, (byte)this.FunctionCode);
            WriterExtension.WriteValue<BytesWriter, ushort>(ref bytesWriter, this.StartingAddress, EndianType.Big);
            bytesWriter.Write(this.Data.Span);
        }
        else if (this.FunctionCode == FunctionCode.WriteMultipleCoils || this.FunctionCode == FunctionCode.WriteMultipleRegisters)
        {
            WriterExtension.WriteValue<BytesWriter, byte>(ref bytesWriter, this.SlaveId);
            WriterExtension.WriteValue<BytesWriter, byte>(ref bytesWriter, (byte)this.FunctionCode);
            WriterExtension.WriteValue<BytesWriter, ushort>(ref bytesWriter, this.StartingAddress, EndianType.Big);
            WriterExtension.WriteValue<BytesWriter, ushort>(ref bytesWriter, this.Quantity, EndianType.Big);
            WriterExtension.WriteValue<BytesWriter, byte>(ref bytesWriter, (byte)this.Data.Length);
            bytesWriter.Write(this.Data.Span);
        }
        else if (this.FunctionCode == FunctionCode.ReadWriteMultipleRegisters)
        {
            WriterExtension.WriteValue<BytesWriter, byte>(ref bytesWriter, this.SlaveId);
            WriterExtension.WriteValue<BytesWriter, byte>(ref bytesWriter, (byte)this.FunctionCode);
            WriterExtension.WriteValue<BytesWriter, ushort>(ref bytesWriter, this.ReadStartAddress, EndianType.Big);
            WriterExtension.WriteValue<BytesWriter, ushort>(ref bytesWriter, this.ReadQuantity, EndianType.Big);
            WriterExtension.WriteValue<BytesWriter, ushort>(ref bytesWriter, this.StartingAddress, EndianType.Big);
            WriterExtension.WriteValue<BytesWriter, ushort>(ref bytesWriter, this.Quantity, EndianType.Big);
            WriterExtension.WriteValue<BytesWriter, byte>(ref bytesWriter, (byte)this.Data.Length);
            bytesWriter.Write(this.Data.Span);
        }
        else
        {
            throw new System.InvalidOperationException("无法识别的功能码");
        }

        this.Crc = TouchSocketModbusUtility.ToModbusCrcValue(bytesWriter.Span);

        WriterExtension.WriteValue<BytesWriter, ushort>(ref bytesWriter, this.Crc, EndianType.Big);
        writer.Advance((int)bytesWriter.WrittenCount);
    }
}