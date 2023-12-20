using System.Net;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Modbus
{
    internal class ModbusUdpAdapter : UdpDataHandlingAdapter
    {
        public override bool CanSendRequestInfo => true;

        protected override void PreviewReceived(EndPoint remoteEndPoint, ByteBlock byteBlock)
        {
            var response=new ModbusTcpResponse();

            if (((IFixedHeaderRequestInfo)response).OnParsingHeader(byteBlock.ToArray(0,8))) 
            {
                if (((IFixedHeaderRequestInfo)response).OnParsingBody(byteBlock.ToArray(8)))
                {
                    this.GoReceived(remoteEndPoint,default,response);
                }
            }
        }
    }
}
