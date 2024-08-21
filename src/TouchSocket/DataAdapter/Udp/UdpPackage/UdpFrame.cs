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
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
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

        public unsafe bool Parse(ReadOnlySpan<byte> span)
        {
            var length = span.Length;
            if (length > 11)
            {
                this.Id = TouchSocketBitConverter.Default.To<long>(span);
                this.SN = TouchSocketBitConverter.Default.To<ushort>(span.Slice(8));
                this.FIN = span[10].GetBit(7) == true;
                if (this.FIN)
                {
                    this.Data = length > 13 ? (new byte[length - 13]) : (new byte[0]);
                    this.Crc = new byte[2] { span[length - 2], span[length - 1] };
                }
                else
                {
                    this.Data = new byte[length - 11];
                }

                fixed (byte* p1 = &span[11])
                {
                    fixed (byte* p2 = &this.Data[0])
                    {
                        Unsafe.CopyBlock(p2, p1, (uint)this.Data.Length);
                    }
                }

                return true;
            }
            return false;
        }
    }
}
