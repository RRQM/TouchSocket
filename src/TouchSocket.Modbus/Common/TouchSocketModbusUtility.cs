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
using System.Buffers;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Modbus;

/// <summary>
/// TouchSocketModbusUtility
/// </summary>
public static class TouchSocketModbusUtility
{
    /// <summary>
    /// 计算CRC16_Modbus校验值
    /// </summary>
    /// <param name="memory">待计算校验值的字节序列</param>
    /// <returns>包含CRC校验值的字节数组</returns>
    /// <remarks>
    /// CRC16_Modbus是一种循环冗余校验算法，常用于Modbus协议中数据的完整性校验。
    /// 该方法接收一个只读字节内存块，计算并返回其CRC校验值。
    /// 校验值用于确保数据在传输过程中的完整性，接收方会使用同样的算法计算接收到的数据的校验值，
    /// 并与接收到的校验值进行比较，以检测数据传输过程中是否发生了改变。
    /// </remarks>
    public static byte[] ToModbusCrc(ReadOnlyMemory<byte> memory)
    {
        // 获取内存块的数组表示及其相关属性
        var memoryArray = memory.GetArray();
        var byteData = memoryArray.Array;
        var offset = memoryArray.Offset;
        var length = memoryArray.Count;

        // 初始化存放CRC校验值的数组
        var CRC = new byte[2];

        // 初始化CRC寄存器
        ushort wCrc = 0xFFFF;
        // 遍历数据，计算CRC校验值
        for (var i = offset; i < length; i++)
        {
            wCrc ^= Convert.ToUInt16(byteData[i]);
            // 逐位处理字节
            for (var j = 0; j < 8; j++)
            {
                // 根据CRC算法进行位操作
                if ((wCrc & 0x0001) == 1)
                {
                    wCrc >>= 1;
                    wCrc ^= 0xA001; // 异或多项式
                }
                else
                {
                    wCrc >>= 1;
                }
            }
        }
        // 准备返回结果，高位在后，低位在前
        CRC[1] = (byte)((wCrc & 0xFF00) >> 8);
        CRC[0] = (byte)(wCrc & 0x00FF);
        // 返回计算得到的CRC校验值
        return CRC;
    }


    /// <summary>
    /// CRC16_Modbus效验
    /// </summary>
    /// <param name="sourceSpan"></param>
    /// <returns></returns>
    public static ushort ToModbusCrcValue(ReadOnlySpan<byte> sourceSpan)
    {
        Span<byte> crcSpan = stackalloc byte[2];
        ushort wCrc = 0xFFFF;
        for (var i = 0; i < sourceSpan.Length; i++)
        {
            wCrc ^= sourceSpan[i];
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

        crcSpan[1] = (byte)((wCrc & 0xFF00) >> 8);//高位在后
        crcSpan[0] = (byte)(wCrc & 0x00FF);       //低位在前

        return TouchSocketBitConverter.BigEndian.To<ushort>(crcSpan);
    }

    public static ushort ToModbusCrcValue(ReadOnlySequence<byte> sourceSequence)
    {
        using (var buffer = new ContiguousMemoryBuffer(sourceSequence))
        {
            return ToModbusCrcValue(buffer.Memory.Span);
        }
    }

    /// <summary>
    /// 将布尔值转为2字节数组
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static ReadOnlyMemory<byte> BoolToBytes(bool value)
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