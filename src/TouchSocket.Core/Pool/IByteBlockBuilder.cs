using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    public interface IByteBlockBuilder
    {
        /// <summary>
        /// 构建数据时，指示内存池的申请长度。
        /// <para>
        /// 建议：该值可以尽可能的设置大一些，这样可以避免内存池扩容。
        /// </para>
        /// </summary>
        int MaxLength { get; }

        /// <summary>
        /// 构建对象到<see cref="ByteBlock"/>
        /// </summary>
        /// <param name="byteBlock"></param>
        void Build<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock;
    }
}
