using System.Linq;
using TouchSocket.Core;

namespace TouchSocket.Modbus
{
    /// <summary>
    /// ModbusTcpRequest
    /// </summary>
    internal sealed class ModbusTcpRequest : ModbusTcpBase, IRequestInfoBuilder, IRequestInfo
    {
        /// <summary>
        /// 从<see cref="ModbusRequest"/>创建一个ModbusTcpRequest
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="request"></param>
        public ModbusTcpRequest(ushort transactionId, ModbusRequest request)
        {
            this.TransactionId = transactionId;
            this.ProtocolId = 0;
            this.SlaveId = request.SlaveId;
            this.FunctionCode = request.FunctionCode;
            this.StartingAddress = request.StartingAddress;
            this.Quantity = request.Quantity;
            this.Data = request.Data;
            this.ReadStartAddress = request.ReadStartAddress;
            this.ReadQuantity = request.ReadQuantity;
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
            else if (this.FunctionCode == FunctionCode.ReadWriteMultipleRegisters)
            {
                byteBlock.Write((ushort)(this.Data.Length + 11), EndianType.Big);
                byteBlock.Write(this.SlaveId);
                byteBlock.Write((byte)this.FunctionCode);
                byteBlock.Write(this.ReadStartAddress, EndianType.Big);
                byteBlock.Write(this.ReadQuantity, EndianType.Big);
                byteBlock.Write(this.StartingAddress, EndianType.Big);
                byteBlock.Write(this.Quantity, EndianType.Big);
                byteBlock.Write((byte)this.Data.Length);
                byteBlock.Write(this.Data);
            }
            else
            {
                throw new System.InvalidOperationException("无法识别的功能码");
            }
        }
    }
}