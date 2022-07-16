//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在TouchSocket.Core.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 传输字节
    /// </summary>
    public readonly struct TransferByte
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public TransferByte(byte[] buffer, int offset, int length)
        {
            this.Offset = offset;
            this.Length = length;
            this.Buffer = buffer;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="buffer"></param>
        public TransferByte(byte[] buffer) : this(buffer, 0, buffer.Length)
        {
        }

        /// <summary>
        /// 数据内存
        /// </summary>
        public byte[] Buffer { get; }

        /// <summary>
        /// 偏移
        /// </summary>
        public int Offset { get; }

        /// <summary>
        /// 长度
        /// </summary>
        public int Length { get; }
    }
}