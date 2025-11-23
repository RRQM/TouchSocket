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

namespace TouchSocket.Core;

/// <summary>
/// CRC校验工具类
/// </summary>
public static class Crc
{
    /// <summary>
    /// CRC16查找表
    /// </summary>
    private static readonly ushort[] s_crc16Table = GenerateCrc16Table();

    private static ushort[] GenerateCrc16Table()
    {
        var table = new ushort[256];
        const ushort polynomial = 0xA001;

        for (var i = 0; i < 256; i++)
        {
            var crc = (ushort)i;
            for (var j = 0; j < 8; j++)
            {
                if ((crc & 1) > 0)
                {
                    crc = (ushort)((crc >> 1) ^ polynomial);
                }
                else
                {
                    crc = (ushort)(crc >> 1);
                }
            }
            table[i] = crc;
        }

        return table;
    }

    /// <summary>
    /// 计算CRC16校验值并返回字节数组（大端序）
    /// </summary>
    /// <param name="span">待计算的字节序列</param>
    /// <returns>CRC16校验值的字节数组</returns>
    public static byte[] Crc16(ReadOnlySpan<byte> span)
    {
        var ret = BitConverter.GetBytes(Crc16Value(span));
        Array.Reverse(ret);
        return ret;
    }

    /// <summary>
    /// 计算CRC16校验值
    /// </summary>
    /// <param name="span">待计算的字节序列</param>
    /// <returns>CRC16校验值</returns>
    public static ushort Crc16Value(ReadOnlySpan<byte> span)
    {
        if (span.IsEmpty)
        {
            ThrowHelper.ThrowArgumentNullException(nameof(span));
        }

        ushort crc = 0;
        for (var i = 0; i < span.Length; i++)
        {
            var index = (byte)(crc ^ span[i]);
            crc = (ushort)((crc >> 8) ^ s_crc16Table[index]);
        }

        return crc;
    }

    /// <summary>
    /// 计算CRC16校验值并写入指定的字节序列
    /// </summary>
    /// <param name="storeSpan">用于存储CRC16校验值的字节序列（至少2字节）</param>
    /// <param name="span">待计算的字节序列</param>
    public static void WriteCrc16(Span<byte> storeSpan, ReadOnlySpan<byte> span)
    {
        if (storeSpan.Length < 2)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(storeSpan.Length), storeSpan.Length, 2);
        }

        if (span.IsEmpty)
        {
            return;
        }

        ushort crc = 0;
        for (var i = 0; i < span.Length; i++)
        {
            var index = (byte)(crc ^ span[i]);
            crc = (ushort)((crc >> 8) ^ s_crc16Table[index]);
        }

        TouchSocketBitConverter.BigEndian.WriteBytes(storeSpan, crc);
    }
}