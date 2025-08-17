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
using System.Runtime.CompilerServices;

namespace TouchSocket.Core;

/// <summary>
/// UDP数据帧
/// </summary>
public struct UdpFrame
{
    /// <summary>
    /// Crc校验
    /// </summary>
    public byte[] Crc { get; set; }

    /// <summary>
    /// 数据
    /// </summary>
    public byte[] Data { get; set; }

    /// <summary>
    /// 是否为终结帧
    /// </summary>
    public bool FIN { get; set; }

    /// <summary>
    /// 数据Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 帧序号
    /// </summary>
    public ushort SN { get; set; }

    /// <summary>
    /// 解析给定的只读字节跨度数据。
    /// </summary>
    /// <param name="span">待解析的只读字节跨度。</param>
    /// <returns>如果解析成功，则返回<see langword="true"/>；否则返回<see langword="false"/>。</returns>
    public unsafe bool Parse(ReadOnlySpan<byte> span)
    {
        // 获取输入跨度的长度
        var length = span.Length;
        // 检查输入长度是否大于最小包长度11字节
        if (length > 11)
        {
            // 解析ID字段，占用8字节
            this.Id = TouchSocketBitConverter.Default.To<long>(span);
            // 解析序列号SN字段，从第8字节开始，占用2字节
            this.SN = TouchSocketBitConverter.Default.To<ushort>(span.Slice(8));
            // 检查FIN标志位，位于第10字节的第7位
            this.FIN = span[10].GetBit(7) == true;
            // 根据FIN标志位决定数据字段的长度和内容
            if (this.FIN)
            {
                // FIN标志位为真时，数据字段长度为总长度减去头部长度和CRC校验码长度
                this.Data = length > 13 ? (new byte[length - 13]) : (new byte[0]);
                // 读取CRC校验码，位于数据末尾2字节
                this.Crc = new byte[2] { span[length - 2], span[length - 1] };
            }
            else
            {
                // FIN标志位为假时，数据字段长度为总长度减去头部长度
                this.Data = new byte[length - 11];
            }

            // 使用指针操作，快速复制数据字段
            fixed (byte* p1 = &span[11])
            {
                fixed (byte* p2 = &this.Data[0])
                {
                    Unsafe.CopyBlock(p2, p1, (uint)this.Data.Length);
                }
            }

            // 成功解析输入跨度数据，返回<see langword="true"/>
            return true;
        }
        // 输入长度不足，无法解析，返回<see langword="false"/>
        return false;
    }
}