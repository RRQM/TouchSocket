using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Modbus
{
    internal abstract class ModbusRtuBase:ModbusBase
    {
        public byte[] Crc { get;protected set; }
    }
}
