using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Modbus
{
    internal class ModbusRtuAdapter : CustomFixedHeaderDataHandlingAdapter<ModbusRtuResponse>
    {
        public override bool CanSendRequestInfo => true;
        public override int HeaderLength => 3;

        protected override ModbusRtuResponse GetInstance()
        {
            var response = new ModbusRtuResponse();
            response.SetByteBlock(m_byteBlock);
            return response;
        }

        private ByteBlock m_byteBlock;

        protected override FilterResult Filter(in ByteBlock byteBlock, bool beCached, ref ModbusRtuResponse request, ref int tempCapacity)
        {
            this.m_byteBlock = byteBlock;

            if (beCached)
            {
                request.SetByteBlock(byteBlock);
            }

            return base.Filter(byteBlock, beCached, ref request, ref tempCapacity);
        }
    }
}
