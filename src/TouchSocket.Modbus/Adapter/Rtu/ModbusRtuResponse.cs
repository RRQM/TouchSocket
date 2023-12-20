using System;
using System.Linq;
using TouchSocket.Core;

namespace TouchSocket.Modbus
{
    internal class ModbusRtuResponse : ModbusRtuBase, IFixedHeaderRequestInfo, IModbusResponse
    {
        private int m_bodyLength;

        /// <summary>
        /// 缓存区
        /// </summary>
        private ByteBlock m_byteBlock;

        int IFixedHeaderRequestInfo.BodyLength => this.m_bodyLength;

        bool IFixedHeaderRequestInfo.OnParsingBody(byte[] body)
        {
            if ((byte)this.FunctionCode <= 4)
            {
                this.Data = body.Take(body.Length - 2).ToArray();
                return true;
            }
            else if (this.FunctionCode == FunctionCode.WriteSingleCoil || this.FunctionCode == FunctionCode.WriteSingleRegister)
            {
                this.StartingAddress = TouchSocketBitConverter.BigEndian.ToUInt16(body, 0);
                this.Data = body.Skip(2).Take(body.Length - 4).ToArray();
                return true;
            }
            else if (this.FunctionCode == FunctionCode.WriteMultipleCoils || this.FunctionCode == FunctionCode.WriteMultipleRegisters)
            {
                this.StartingAddress = TouchSocketBitConverter.BigEndian.ToUInt16(body, 0);
                this.Quantity = TouchSocketBitConverter.BigEndian.ToUInt16(body, 2);
                this.Data = new byte[0];
                return true;
            }
            return false;
        }

        bool IFixedHeaderRequestInfo.OnParsingHeader(byte[] header)
        {
            if (header.Length == 3)
            {
                this.SlaveId = header[0];
                this.FunctionCode = (FunctionCode)header[1];
                if ((byte)this.FunctionCode <= 4)
                {
                    this.m_bodyLength = header[2] + 2;
                    return true;
                }
                else if (this.FunctionCode == FunctionCode.WriteSingleCoil || this.FunctionCode == FunctionCode.WriteSingleRegister)
                {
                    this.m_byteBlock.Pos--;//回退一个游标
                    this.m_bodyLength = 6;
                    return true;
                }
            }

            return false;
        }

        public void SetFunctionCode(FunctionCode functionCode)
        {
            this.FunctionCode = functionCode;
        }

        public void SetQuantity(ushort value)
        {
            this.Quantity = value;
        }

        public void SetSlaveId(byte value)
        {
            this.SlaveId = value;
        }

        public void SetStartingAddress(ushort value)
        {
            this.StartingAddress = value;
        }

        public void SetValue(byte[] bytes)
        {
            this.Data = bytes;
        }

        internal void SetByteBlock(ByteBlock byteBlock)
        {
            this.m_byteBlock = byteBlock;
        }

        internal void SetCrc(byte[] bytes)
        {
            this.Crc = bytes;
        }
    }
}