using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Modbus
{
    /// <summary>
    /// ModbusTcpBase
    /// </summary>
    internal abstract class ModbusTcpBase: ModbusBase
    {
        /// <summary>
        /// 事务处理标识符。即序号
        /// </summary>
        public ushort TransactionId { get; protected set; }

        /// <summary>
        /// 协议标识符
        /// </summary>
        public ushort ProtocolId { get; protected set; }
    }
}
