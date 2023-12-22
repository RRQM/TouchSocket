using System;

namespace TouchSocket.Modbus
{
    internal class SRHelper
    {
        public static void ThrowIfNotSuccess(ModbusErrorCode errorCode)
        {
            switch (errorCode)
            {
                case ModbusErrorCode.Success:
                    break;
                default:
                    throw new ModbusResponseException(errorCode);
            }
        }
    }
}
