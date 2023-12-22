using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Modbus
{
    /// <summary>
    /// Modbus请求接口
    /// </summary>
    public interface IModbusRequest
    {
        /// <summary>
        /// 数量
        /// </summary>
        ushort Quantity { get;}

        /// <summary>
        /// 起始位置
        /// </summary>
        ushort StartingAddress { get;}

        /// <summary>
        /// 数据
        /// </summary>
        byte[] Data { get; }

        /// <summary>
        /// 站点号（单元标识符）
        /// </summary>
        byte SlaveId { get;}

        /// <summary>
        /// 功能码
        /// </summary>
        FunctionCode FunctionCode { get;}

        /// <summary>
        /// 在读起始位置。
        /// </summary>
        ushort ReadStartAddress { get;}

        /// <summary>
        /// 读取长度
        /// </summary>
        ushort ReadQuantity { get;}
    }
}
