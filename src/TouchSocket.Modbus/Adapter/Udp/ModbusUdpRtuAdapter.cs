using System.Linq;
using System.Net;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Modbus
{
    internal class ModbusUdpRtuAdapter : UdpDataHandlingAdapter
    {
        public override bool CanSendRequestInfo => true;

        protected override void PreviewReceived(EndPoint remoteEndPoint, ByteBlock byteBlock)
        {
            var response = new ModbusRtuResponse();
            response.SlaveId = byteBlock[0];
            response.FunctionCode=(FunctionCode)byteBlock[1];

            int crcLen = 0;
            if ((byte)response.FunctionCode <= 4)
            {
                var len = byteBlock[2];
                response.SetValue(byteBlock.Skip(3).Take(len).ToArray());
                response.Crc=(byteBlock.Skip(3 + len).Take(2).ToArray());
                crcLen = 3 + len;
            }
            else if (response.FunctionCode == FunctionCode.WriteSingleCoil || response.FunctionCode == FunctionCode.WriteSingleRegister)
            {
                response.StartingAddress = TouchSocketBitConverter.BigEndian.ToUInt16(byteBlock.Buffer, 2);
                response.SetValue(byteBlock.Skip(4).Take(2).ToArray());
                response.Crc=(byteBlock.Skip(6).Take(2).ToArray());
                crcLen = 6;
            }
            else if (response.FunctionCode == FunctionCode.WriteMultipleCoils || response.FunctionCode == FunctionCode.WriteMultipleRegisters)
            {
                response.StartingAddress = TouchSocketBitConverter.BigEndian.ToUInt16(byteBlock.Buffer, 2);
                response.Quantity = TouchSocketBitConverter.BigEndian.ToUInt16(byteBlock.Buffer, 4);
                response.Crc=(byteBlock.Skip(6).Take(2).ToArray());
                crcLen = 6;
            }

            var crc = TouchSocketModbusUtility.ToModbusCrc(byteBlock.Buffer, 0, crcLen);
            if (crc.SequenceEqual(response.Crc))
            {
                base.GoReceived(remoteEndPoint,null, response);
            }
        }
    }
}
