using TouchSocket.Core;

namespace TouchSocket.Modbus
{
    internal sealed class ModbusTcpRequest : ModbusTcpBase, IRequestInfoBuilder, IRequestInfo
    {
        public ModbusTcpRequest(ushort transactionId, ModbusRequest request)
        {
            this.TransactionId = transactionId;
            this.ProtocolId = 0;
            this.SlaveId = request.SlaveId;
            this.FunctionCode = request.FunctionCode;
            this.StartingAddress = request.StartingAddress;
            this.Quantity = request.Quantity;
            this.Data = request.Data;
        }

        /// <inheritdoc/>
        int IRequestInfoBuilder.MaxLength => 1024;

        /// <inheritdoc/>
        public void Build(ByteBlock byteBlock)
        {
            byteBlock.Write(this.TransactionId, EndianType.Big);
            byteBlock.Write(this.ProtocolId, EndianType.Big);
            if ((byte)this.FunctionCode <= 4)
            {
                byteBlock.Write((ushort)6, EndianType.Big);
                byteBlock.Write(this.SlaveId);
                byteBlock.Write((byte)this.FunctionCode);
                byteBlock.Write(this.StartingAddress, EndianType.Big);
                byteBlock.Write(this.Quantity, EndianType.Big);
            }
            else if (this.FunctionCode == FunctionCode.WriteSingleCoil || this.FunctionCode == FunctionCode.WriteSingleRegister)
            {
                byteBlock.Write((ushort)6, EndianType.Big);
                byteBlock.Write(this.SlaveId);
                byteBlock.Write((byte)this.FunctionCode);
                byteBlock.Write(this.StartingAddress, EndianType.Big);
                byteBlock.Write(this.Data);
            }
            else if (this.FunctionCode == FunctionCode.WriteMultipleCoils || this.FunctionCode == FunctionCode.WriteMultipleRegisters)
            {
                byteBlock.Write((ushort)(this.Data.Length + 7), EndianType.Big);
                byteBlock.Write(this.SlaveId);
                byteBlock.Write((byte)this.FunctionCode);
                byteBlock.Write(this.StartingAddress, EndianType.Big);
                byteBlock.Write(this.Quantity, EndianType.Big);
                byteBlock.Write((byte)this.Data.Length);
                byteBlock.Write(this.Data);
            }
        }
    }
}