using System.Collections.Generic;
using System;
using TouchSocket.Core;

namespace TouchSocket.Modbus
{
    /// <summary>
    /// ModbusResponseExtension
    /// </summary>
    public static class ModbusResponseExtension
    {
        /// <summary>
        /// 获取Modbus错误代码。如果没有错误，会返回<see cref="ModbusErrorCode.Success"/>
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static ModbusErrorCode GetErrorCode(this IModbusResponse response)
        {
            var value = (byte)(response.FunctionCode);
            if ((value & 0x80) == 0)
            {
                return 0;
            }

            value = value.SetBit(7, 0);
            return (ModbusErrorCode)value;
        }

        /// <summary>
        /// 获取一个读取器。
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static ValueByteBlock CreateReader(this IModbusResponse response)
        {
            return new ValueByteBlock(response.Data);
        }
    }
}
