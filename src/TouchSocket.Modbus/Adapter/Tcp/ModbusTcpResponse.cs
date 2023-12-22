using TouchSocket.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TouchSocket.Modbus
{
    internal sealed class ModbusTcpResponse : ModbusTcpBase, IFixedHeaderRequestInfo, IWaitHandle, IModbusResponse
    {
        private int m_bodyLength;
        private bool m_isError;
        int IFixedHeaderRequestInfo.BodyLength => this.m_bodyLength;

        public ModbusErrorCode ErrorCode { get; private set; }

        long IWaitHandle.Sign { get => this.TransactionId; set => this.TransactionId = (ushort)value; }

        bool IFixedHeaderRequestInfo.OnParsingBody(byte[] body)
        {
            if (this.m_isError)
            {
                this.ErrorCode = (ModbusErrorCode)body.First();
                return true;
            }
            if ((byte)this.FunctionCode <= 4 || this.FunctionCode == FunctionCode.ReadWriteMultipleRegisters)
            {
                var len = body.First();

                if (body.Length - 1 == len)
                {
                    this.Data = body.Skip(1).ToArray();
                    return true;
                }
            }
            else if (this.FunctionCode == FunctionCode.WriteSingleCoil || this.FunctionCode == FunctionCode.WriteSingleRegister)
            {
                this.StartingAddress = TouchSocketBitConverter.BigEndian.ToUInt16(body, 0);
                this.Data = body.Skip(2).ToArray();
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
            if (header.Length == 8)
            {
                this.TransactionId = TouchSocketBitConverter.BigEndian.ToUInt16(header, 0);
                this.ProtocolId = TouchSocketBitConverter.BigEndian.ToUInt16(header, 2);
                this.m_bodyLength = TouchSocketBitConverter.BigEndian.ToUInt16(header, 4) - 2;
                this.SlaveId = header[6];

                var code = header[7];
                if ((code & 0x80) == 0)
                {
                    this.FunctionCode = (FunctionCode)code;
                }
                else
                {
                    code = code.SetBit(7, 0);
                    this.FunctionCode = (FunctionCode)code;
                    this.m_isError = true;
                }
                return true;
            }

            return false;
        }
    }
}