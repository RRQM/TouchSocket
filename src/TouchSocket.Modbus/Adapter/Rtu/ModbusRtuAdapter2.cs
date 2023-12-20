using System;
using System.Linq;
using TouchSocket.Core;

namespace TouchSocket.Modbus
{
    internal class ModbusRtuAdapter2:PeriodPackageAdapter
    {
        public ModbusRtuAdapter2()
        {
            this.CacheTimeout = TimeSpan.FromMilliseconds(100);
        }

        public override bool CanSendRequestInfo => true;

        protected override void GoReceived(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            var response = new ModbusRtuResponse();
            response.SetSlaveId(byteBlock[0]);
            response.SetFunctionCode((FunctionCode)byteBlock[1]);

            int crcLen = 0;
            if ((byte)response.FunctionCode <= 4)
            {
                var len = byteBlock[2];
                response.SetValue(byteBlock.Skip(3).Take(len).ToArray());
                response.SetCrc(byteBlock.Skip(3 + len).Take(2).ToArray());
                crcLen = 3 + len;
            }
            else if (response.FunctionCode == FunctionCode.WriteSingleCoil || response.FunctionCode == FunctionCode.WriteSingleRegister)
            {
                response.SetStartingAddress(TouchSocketBitConverter.BigEndian.ToUInt16(byteBlock.Buffer, 2));
                response.SetValue(byteBlock.Skip(4).Take(2).ToArray());
                response.SetCrc(byteBlock.Skip(6).Take(2).ToArray());
                crcLen = 6;
            }
            else if (response.FunctionCode == FunctionCode.WriteMultipleCoils || response.FunctionCode == FunctionCode.WriteMultipleRegisters)
            {
                response.SetStartingAddress (TouchSocketBitConverter.BigEndian.ToUInt16(byteBlock.Buffer, 2));
                response.SetQuantity( TouchSocketBitConverter.BigEndian.ToUInt16(byteBlock.Buffer, 4));
                response.SetCrc(byteBlock.Skip(6).Take(2).ToArray());
                crcLen = 6;
            }

            var crc = SRHelper.ToModbusCrc(byteBlock.Buffer,0,crcLen);
            if (crc.SequenceEqual(response.Crc))
            {
                base.GoReceived(null, response);
            }
        }
    }
}
