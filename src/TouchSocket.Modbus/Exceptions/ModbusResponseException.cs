using TouchSocket.Core;
using System;

namespace TouchSocket.Modbus
{
    /// <summary>
    /// Modbus响应异常。
    /// </summary>
    [Serializable]
    public class ModbusResponseException:Exception
    {
        /// <summary>
        /// Modbus响应异常
        /// </summary>
        /// <param name="errorCode"></param>
        public ModbusResponseException(ModbusErrorCode errorCode) : base(errorCode.GetDescription())
        {
            this.ErrorCode = errorCode;
        }

        /// <summary>
        /// 异常码
        /// </summary>
        public ModbusErrorCode ErrorCode { get; }
    }
}
