using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Modbus
{
    /// <summary>
    /// Modbus基类
    /// </summary>
    public abstract class ModbusBase
    {
        /// <summary>
        /// 数量
        /// </summary>
        public ushort Quantity { get; protected set; }

        /// <summary>
        /// 起始位置
        /// </summary>
        public ushort StartingAddress { get; protected set; }

        /// <summary>
        /// 数据
        /// </summary>
        public byte[] Data { get; protected set; }

        /// <summary>
        /// 站点号（单元标识符）
        /// </summary>
        public byte SlaveId { get; protected set; }

        /// <summary>
        /// 功能码
        /// </summary>
        public FunctionCode FunctionCode { get; protected set; }
    }
}
