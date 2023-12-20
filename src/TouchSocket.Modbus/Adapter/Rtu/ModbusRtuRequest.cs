using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Modbus
{
    internal class ModbusRtuRequest : ModbusRtuBase, IRequestInfoBuilder, IRequestInfo
    {
        public ModbusRtuRequest(ModbusRequest request)
        {
            this.SlaveId = request.SlaveId;
            this.FunctionCode = request.FunctionCode;
            this.Quantity = request.Quantity;
            this.StartingAddress = request.StartingAddress;
            this.Data = request.Data;
        }

        /// <inheritdoc/>
        int IRequestInfoBuilder.MaxLength => 1024;

        /// <inheritdoc/>
        public void Build(ByteBlock byteBlock)
        {
            if ((byte)this.FunctionCode <= 4)
            {
                byteBlock.Write(this.SlaveId);
                byteBlock.Write((byte)this.FunctionCode);
                byteBlock.Write(this.StartingAddress, EndianType.Big);
                byteBlock.Write(this.Quantity, EndianType.Big);
            }
            else if (this.FunctionCode == FunctionCode.WriteSingleCoil || this.FunctionCode == FunctionCode.WriteSingleRegister)
            {
                byteBlock.Write(this.SlaveId);
                byteBlock.Write((byte)this.FunctionCode);
                byteBlock.Write(this.StartingAddress, EndianType.Big);
                byteBlock.Write(this.Data);
            }
            else if (this.FunctionCode == FunctionCode.WriteMultipleCoils || this.FunctionCode == FunctionCode.WriteMultipleRegisters)
            {
                byteBlock.Write(this.SlaveId);
                byteBlock.Write((byte)this.FunctionCode);
                byteBlock.Write(this.StartingAddress, EndianType.Big);
                byteBlock.Write(this.Quantity, EndianType.Big);
                byteBlock.Write((byte)this.Data.Length);
                byteBlock.Write(this.Data);
            }

            this.Crc = SRHelper.ToModbusCrc(byteBlock.Buffer, 0, byteBlock.Len);

            byteBlock.Write(this.Crc);
        }
    }
}