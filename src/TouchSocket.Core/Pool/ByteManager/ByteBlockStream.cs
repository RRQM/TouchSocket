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
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace TouchSocket.Core
{
    /// <summary>
    /// 字节块流
    /// </summary>
    [DebuggerDisplay("Len={Length},Pos={Position},Capacity={Capacity}")]
    public sealed partial class ByteBlockStream : Stream
    {
        private readonly ByteBlock m_byteBlock;
        private readonly bool m_releaseTogether;


        /// <summary>
        /// 初始化 ByteBlockStream 类的新实例。
        /// </summary>
        /// <param name="byteBlock">一个 ByteBlock 对象，表示字节块。</param>
        /// <param name="releaseTogether">一个布尔值，指示是否在释放流时同时释放字节块。</param>
        public ByteBlockStream(ByteBlock byteBlock, bool releaseTogether)
        {
            this.m_byteBlock = byteBlock;
            this.m_releaseTogether = releaseTogether;
        }

        /// <summary>
        /// 获取此实例关联的 ByteBlock 对象。
        /// </summary>
        public ByteBlock ByteBlock => this.m_byteBlock;

        /// <summary>
        /// 仅当内存块可用，且<see cref="CanReadLength"/>>0时为True。
        /// </summary>
        public override bool CanRead => this.Using && this.CanReadLength > 0;

        /// <summary>
        /// 还能读取的长度，计算为<see cref="Length"/>与<see cref="Position"/>的差值。
        /// </summary>
        public long CanReadLength => this.m_byteBlock.Length - this.m_byteBlock.Position;

        /// <summary>
        /// 支持查找
        /// </summary>
        public override bool CanSeek => this.Using;

        /// <summary>
        /// 可写入
        /// </summary>
        public override bool CanWrite => this.Using;

        /// <summary>
        /// 容量
        /// </summary>
        public int Capacity => this.m_byteBlock.Capacity;

        /// <summary>
        /// 空闲长度，准确掌握该值，可以避免内存扩展，计算为<see cref="Capacity"/>与<see cref="Position"/>的差值。
        /// </summary>
        public long FreeLength => this.Capacity - this.Position;

        /// <summary>
        /// 真实长度
        /// </summary>
        public override long Length => this.m_byteBlock.Length;

        /// <summary>
        /// 流位置
        /// </summary>
        public override long Position
        {
            get => this.m_byteBlock.Position;
            set => this.m_byteBlock.Position = (int)value;
        }

        /// <summary>
        /// 使用状态
        /// </summary>
        public bool Using => this.m_byteBlock.Using;

        /// <summary>
        /// 清空所有内存数据
        /// </summary>
        /// <exception cref="ObjectDisposedException">内存块已释放</exception>
        public void Clear()
        {
            this.ThrowIfDisposed();
            this.m_byteBlock.Clear();
        }

        /// <summary>
        /// 无实际效果
        /// </summary>
        public override void Flush()
        {
        }

        /// <summary>
        /// 读取数据，然后递增Pos
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public override int Read(byte[] buffer, int offset, int length)
        {
            this.ThrowIfDisposed();
            return this.m_byteBlock.Read(new Span<byte>(buffer, offset, length));
        }

        /// <summary>
        /// 从当前流位置读取一个<see cref="byte"/>值
        /// </summary>
        public override int ReadByte()
        {
            return this.m_byteBlock.ReadByte();
        }

        /// <summary>
        /// 设置流位置
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            this.ThrowIfDisposed();
            return this.m_byteBlock.Seek((int)offset, origin);
        }

        /// <summary>
        /// 移动游标
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public void Seek(int position)
        {
            this.Position = position;
        }

        /// <summary>
        /// 设置游标到末位
        /// </summary>
        /// <returns></returns>
        public void SeekToEnd()
        {
            this.Position = this.m_byteBlock.Length;
        }

        /// <summary>
        /// 设置游标到首位
        /// </summary>
        /// <returns></returns>
        public void SeekToStart()
        {
            this.Position = 0;
        }

        /// <summary>
        /// 设置实际长度
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ObjectDisposedException"></exception>
        public override void SetLength(long value)
        {
            this.ThrowIfDisposed();
            this.m_byteBlock.SetLength((int)value);
        }

        /// <summary>
        /// 从指定位置转化到指定长度的有效内存。本操作不递增<see cref="Position"/>
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public byte[] ToArray(int offset, int length)
        {
            this.ThrowIfDisposed();
            return this.m_byteBlock.ToArray(offset, length);
        }

        /// <summary>
        /// 转换为有效内存。本操作不递增<see cref="Position"/>
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            return this.ToArray(0, (int)this.Length);
        }

        /// <summary>
        /// 从指定位置转为有效内存。本操作不递增<see cref="Position"/>
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public byte[] ToArray(int offset)
        {
            return this.ToArray(offset, (int)(this.Length - offset));
        }

        /// <summary>
        /// 将当前<see cref="Position"/>至指定长度转化为有效内存。本操作不递增<see cref="Position"/>
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public byte[] ToArrayTake(int length)
        {
            return this.ToArray((int)this.Position, length);
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <exception cref="ObjectDisposedException"></exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            this.ThrowIfDisposed();
            this.m_byteBlock.Write(new ReadOnlySpan<byte>(buffer, offset, count));
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && this.m_releaseTogether)
            {
                this.m_byteBlock.Dispose();
            }

            base.Dispose(disposing);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfDisposed()
        {
            if (!this.Using)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
        }
    }
}