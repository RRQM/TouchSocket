//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using TouchSocket.Sockets;

namespace TouchSocket.Modbus
{
    /// <summary>
    /// TouchSocketModbusUtility
    /// </summary>
    public static class TouchSocketModbusUtility
    {
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

        /// <summary>
        /// 将布尔值转为2字节数组
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] BoolToBytes(bool value)
        {
            return value ? new byte[] { 255, 0 } : new byte[] { 0, 0 };
        }

        /// <summary>
        /// ModbusTcp
        /// </summary>
        public static readonly Protocol ModbusTcp = new Protocol(nameof(ModbusTcp));

        /// <summary>
        /// ModbusUdp
        /// </summary>
        public static readonly Protocol ModbusUdp = new Protocol(nameof(ModbusUdp));

        /// <summary>
        /// ModbusRtu
        /// </summary>
        public static readonly Protocol ModbusRtu = new Protocol(nameof(ModbusRtu));

        /// <summary>
        /// ModbusRtuOverTcp
        /// </summary>
        public static readonly Protocol ModbusRtuOverTcp = new Protocol(nameof(ModbusRtuOverTcp));

        /// <summary>
        /// ModbusRtuOverUdp
        /// </summary>
        public static readonly Protocol ModbusRtuOverUdp = new Protocol(nameof(ModbusRtuOverUdp));
    }
}