using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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

        /// <summary>
        /// 解析
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public unsafe bool Parse(ReadOnlySpan<byte> span)
        {
            var length = span.Length;
            if (length > 11)
            {
                this.Id = TouchSocketBitConverter.Default.To<long>(span);
                this.SN = TouchSocketBitConverter.Default.To<ushort>(span.Slice(8));
                this.FIN = span[10].GetBit(7) == 1;
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
