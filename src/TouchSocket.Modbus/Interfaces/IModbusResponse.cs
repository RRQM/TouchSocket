using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Modbus
{
    /// <summary>
    /// Modbus响应
    /// </summary>
    public interface IModbusResponse
    {
        /// <summary>
        /// 数据
        /// </summary>
        byte[] Data { get;}

        /// <summary>
        /// 功能码
        /// </summary>
        FunctionCode FunctionCode { get;}

        /// <summary>
        /// 错误码
        /// </summary>
        ModbusErrorCode ErrorCode { get;}
    }
}
