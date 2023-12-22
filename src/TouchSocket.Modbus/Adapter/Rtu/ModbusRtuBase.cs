using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Modbus
{
    /// <summary>
    /// ModbusRtuBase
    /// </summary>
    public abstract class ModbusRtuBase: ModbusRequest
    {
        /// <summary>
        /// 校验码
        /// </summary>
        public byte[] Crc { get;set; }
    }
}
