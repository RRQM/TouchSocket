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

        /// <summary>
        /// CRC16_Modbus效验
        /// </summary>
        /// <param name="byteData">要进行计算的字节数组</param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns>计算后的数组</returns>
        public static byte[] ToModbusCrc(byte[] byteData, int offset, int length)
        {
            byte[] CRC = new byte[2];

            ushort wCrc = 0xFFFF;
            for (var i = offset; i < length; i++)
            {
                wCrc ^= Convert.ToUInt16(byteData[i]);
                for (var j = 0; j < 8; j++)
                {
                    if ((wCrc & 0x0001) == 1)
                    {
                        wCrc >>= 1;
                        wCrc ^= 0xA001;//异或多项式
                    }
                    else
                    {
                        wCrc >>= 1;
                    }
                }
            }

            CRC[1] = (byte)((wCrc & 0xFF00) >> 8);//高位在后
            CRC[0] = (byte)(wCrc & 0x00FF);       //低位在前
            return CRC;

        }
    }
}
