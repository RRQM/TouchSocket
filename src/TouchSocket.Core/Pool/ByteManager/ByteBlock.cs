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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace TouchSocket.Core
{
    /// <summary>
    /// 字节块流
    /// </summary>
    [DebuggerDisplay("Length={Length},Position={Position},Capacity={Capacity}")]
    public sealed partial class ByteBlock :DisposableObject, IByteBlock
    {
        private byte[] m_buffer;
        private BytePool m_bytePool;
        private bool m_canReturn;
        private int m_dis;
        private bool m_holding;
        private int m_length;
        private int m_position;

        #region 构造函数

        /// <summary>
        ///  字节块流
        /// </summary>
        /// <param name="byteSize"></param>
        public ByteBlock(int byteSize = 1024 * 64)
        {
            this.m_bytePool = BytePool.Default;
            this.m_buffer = BytePool.Default.Rent(byteSize);
            this.m_canReturn = true;
        }

        /// <summary>
        /// 字节块流
        /// </summary>
        /// <param name="byteSize"></param>
        /// <param name="bytePool"></param>
        public ByteBlock(int byteSize, BytePool bytePool)
        {
            this.m_bytePool = bytePool;
            this.m_buffer = bytePool.Rent(byteSize);
            this.m_canReturn = true;
        }

        /// <summary>
        /// 实例化一个已知内存的对象。且该内存不会被回收。
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        public ByteBlock(byte[] bytes, int length)
        {
            this.m_buffer = bytes ?? throw new ArgumentNullException(nameof(bytes));
            this.m_length = length;
        }

        /// <summary>
        /// 实例化一个已知内存的对象。且该内存不会被回收。
        /// </summary>
        /// <param name="bytes"></param>
        public ByteBlock(byte[] bytes) : this(bytes, bytes.Length)
        {
        }

        #endregion 构造函数

        #region 属性

        ///// <summary>
        ///// 字节实例
        ///// </summary>
        //public byte[] Buffer => this.m_buffer;
        public ReadOnlyMemory<byte> Memory
        {
            get
            {
                this.ThrowIfDisposed();
                return new Memory<byte>(this.m_buffer, 0, this.m_length);
            }
        } 
        
        public Memory<byte> TotalMemory
        {
            get
            {
                this.ThrowIfDisposed();
                return new Memory<byte>(this.m_buffer);
            }
        }

        public ReadOnlySpan<byte> Span
        {
            get
            {
                this.ThrowIfDisposed();
                return new ReadOnlySpan<byte>(this.m_buffer, 0, this.m_length);
            }
        }

        /// <summary>
        /// 仅当内存块可用，且<see cref="CanReadLength"/>>0时为True。
        /// </summary>
        public bool CanRead => this.Using && this.CanReadLength > 0;

        /// <summary>
        /// 还能读取的长度，计算为<see cref="Length"/>与<see cref="Position"/>的差值。
        /// </summary>
        public int CanReadLength => this.m_length - this.m_position;

        /// <summary>
        /// 容量
        /// </summary>
        public int Capacity => this.m_buffer.Length;

        /// <summary>
        /// 空闲长度，准确掌握该值，可以避免内存扩展，计算为<see cref="Capacity"/>与<see cref="Position"/>的差值。
        /// </summary>
        public int FreeLength => this.Capacity - this.m_position;

        /// <summary>
        /// 表示持续性持有，为True时，Dispose将调用无效。
        /// </summary>
        public bool Holding => this.m_holding;

        /// <summary>
        /// Int真实长度
        /// </summary>
        public int Length => this.m_length;

        /// <summary>
        /// int型流位置
        /// </summary>
        public int Position
        {
            get => this.m_position;
            set => this.m_position = value;
        }

        /// <summary>
        /// 使用状态
        /// </summary>
        public bool Using => this.m_dis == 0;

        /// <summary>
        /// 返回或设置索引对应的值。
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public byte this[int index]
        {
            get
            {
                this.ThrowIfDisposed();
                return this.m_buffer[index];
            }
            set
            {
                this.ThrowIfDisposed();
                this.m_buffer[index] = value;
            }
        }

        #endregion 属性

        /// <summary>
        /// 清空所有内存数据
        /// </summary>
        /// <exception cref="ObjectDisposedException">内存块已释放</exception>
        public void Clear()
        {
            this.ThrowIfDisposed();
            Array.Clear(this.m_buffer, 0, this.m_buffer.Length);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.m_holding)
                {
                    return;
                }

                if (Interlocked.Increment(ref this.m_dis) == 1)
                {
                    if (this.m_canReturn)
                    {
                        this.m_bytePool.Return(this.m_buffer);
                    }

                    this.m_holding = false;
                    this.m_position = 0;
                    this.m_length = 0;
                    this.m_buffer = null;
                    this.m_bytePool = null;
                    this.m_canReturn = false;
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// 将内存块初始化到刚申请的状态。
        /// <para>仅仅重置<see cref="Position"/>和<see cref="Length"/>属性。</para>
        /// </summary>
        /// <exception cref="ObjectDisposedException">内存块已释放</exception>
        public void Reset()
        {
            this.ThrowIfDisposed();
            this.m_position = 0;
            this.m_length = 0;
            this.m_holding = false;
        }

        /// <summary>
        /// 重新设置容量
        /// </summary>
        /// <param name="capacity">新尺寸</param>
        /// <param name="retainedData">是否保留原数据</param>
        /// <exception cref="ObjectDisposedException"></exception>
        public void SetCapacity(int capacity, bool retainedData = false)
        {
            this.ThrowIfDisposed();

            if (this.Capacity==capacity)
            {
                return;
            }

            bool canReturn;

            byte[] bytes;
            if (this.m_bytePool == null)
            {
                bytes = new byte[capacity];
                canReturn = false;
            }
            else
            {
                bytes = this.m_bytePool.Rent(capacity);
                canReturn = true;
            }

            if (retainedData)
            {
                Array.Copy(this.m_buffer, 0, bytes, 0, this.m_buffer.Length);
            }

            if (this.m_canReturn)
            {
                this.m_bytePool.Return(this.m_buffer);
            }

            this.m_canReturn = canReturn;
            this.m_buffer = bytes;
        }

        /// <summary>
        /// 设置持续持有属性，当为True时，调用Dispose会失效，表示该对象将长期持有，直至设置为False。
        /// 当为False时，会自动调用Dispose。
        /// </summary>
        /// <param name="holding"></param>
        /// <exception cref="ObjectDisposedException"></exception>
        public void SetHolding(bool holding)
        {
            this.ThrowIfDisposed();
            if (holding)
            {
                this.m_holding = holding;
            }
            else
            {
                this.Dispose();
            }
        }

        /// <summary>
        /// 设置实际长度
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ObjectDisposedException"></exception>
        public void SetLength(int value)
        {
            this.ThrowIfDisposed();
            if (value > this.m_buffer.Length)
            {
                throw new Exception("设置值超出容量");
            }
            this.m_length = value;
        }

        #region Seek

        /// <summary>
        /// 设置流位置
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public int Seek(int offset, SeekOrigin origin)
        {
            this.ThrowIfDisposed();
            switch (origin)
            {
                case SeekOrigin.Begin:
                    this.m_position = offset;
                    break;

                case SeekOrigin.Current:
                    this.m_position += offset;
                    break;

                case SeekOrigin.End:
                    this.m_position = this.m_length + offset;
                    break;
            }
            return this.m_position;
        }

        /// <summary>
        /// 移动游标
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public void Seek(int position)
        {
            this.m_position = position;
        }

        /// <summary>
        /// 设置游标到末位
        /// </summary>
        /// <returns></returns>
        public void SeekToEnd()
        {
            this.m_position = this.m_length;
        }

        /// <summary>
        /// 设置游标到首位
        /// </summary>
        /// <returns></returns>
        public void SeekToStart()
        {
            this.m_position = 0;
        }

        #endregion Seek

        #region ToArray

        /// <summary>
        /// 从指定位置转化到指定长度的有效内存。本操作不递增<see cref="Position"/>
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public byte[] ToArray(int offset, int length)
        {
            this.ThrowIfDisposed();
            var buffer = new byte[length];
            Array.Copy(this.m_buffer, offset, buffer, 0, buffer.Length);
            return buffer;
        }

        /// <summary>
        /// 转换为有效内存。本操作不递增<see cref="Position"/>
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            return this.ToArray(0, this.Length);
        }

        /// <summary>
        /// 从指定位置转为有效内存。本操作不递增<see cref="Position"/>
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public byte[] ToArray(int offset)
        {
            return this.ToArray(offset, this.Length - offset);
        }

        /// <summary>
        /// 将当前<see cref="Position"/>至指定长度转化为有效内存。本操作不递增<see cref="Position"/>
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public byte[] ToArrayTake(int length)
        {
            return this.ToArray(this.m_position, length);
        }

        /// <summary>
        /// 将当前<see cref="Position"/>至有效长度转化为有效内存。本操作不递增<see cref="Position"/>
        /// </summary>
        /// <returns></returns>
        public byte[] ToArrayTake()
        {
            return this.ToArray(this.m_position, this.Length - this.m_position);
        }

        #endregion ToArray

        #region AsSegment

        /// <summary>
        /// 从指定位置转化到指定长度的有效内存。本操作不递增<see cref="Position"/>
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public ArraySegment<byte> AsSegment(int offset, int length)
        {
            this.ThrowIfDisposed();
            return new ArraySegment<byte>(this.m_buffer, offset, length);
        }

        /// <summary>
        /// 转换为有效内存。本操作不递增<see cref="Position"/>
        /// </summary>
        /// <returns></returns>
        public ArraySegment<byte> AsSegment()
        {
            return this.AsSegment(0, this.Length);
        }

        /// <summary>
        /// 从指定位置转为有效内存。本操作不递增<see cref="Position"/>
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public ArraySegment<byte> AsSegment(int offset)
        {
            return this.AsSegment(offset, this.Length - offset);
        }

        /// <summary>
        /// 将当前<see cref="Position"/>至指定长度转化为有效内存。本操作不递增<see cref="Position"/>
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public ArraySegment<byte> AsSegmentTake(int length)
        {
            return this.AsSegment(this.m_position, length);
        }

        /// <summary>
        /// 将当前<see cref="Position"/>至有效长度转化为有效内存。本操作不递增<see cref="Position"/>
        /// </summary>
        /// <returns></returns>
        public ArraySegment<byte> AsSegmentTake()
        {
            return this.AsSegment(this.m_position, this.Length - this.m_position);
        }

        #endregion AsSegment

        #region ToMemory

        /// <summary>
        /// 从指定位置转化到指定长度的有效内存。本操作不递增<see cref="Position"/>
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public Memory<byte> ToMemory(int offset, int length)
        {
            this.ThrowIfDisposed();
            return new Memory<byte>(this.m_buffer, offset, length).ToArray();
        }

        /// <summary>
        /// 转换为有效内存。本操作不递增<see cref="Position"/>
        /// </summary>
        /// <returns></returns>
        public Memory<byte> ToMemory()
        {
            return this.ToMemory(0, this.Length);
        }

        /// <summary>
        /// 从指定位置转为有效内存。本操作不递增<see cref="Position"/>
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public Memory<byte> ToMemory(int offset)
        {
            return this.ToMemory(offset, this.Length - offset);
        }

        /// <summary>
        /// 将当前<see cref="Position"/>至指定长度转化为有效内存。本操作不递增<see cref="Position"/>
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public Memory<byte> ToMemoryTake(int length)
        {
            return this.ToMemory(this.m_position, length);
        }

        /// <summary>
        /// 将当前<see cref="Position"/>至有效长度转化为有效内存。本操作不递增<see cref="Position"/>
        /// </summary>
        /// <returns></returns>
        public Memory<byte> ToMemoryTake()
        {
            return this.ToMemory(this.m_position, this.Length - this.m_position);
        }

        #endregion ToMemory

        #region ToString

        /// <summary>
        /// 从指定位置转化到有效内存
        /// </summary>
        /// <summary>
        /// 转换为UTF-8字符
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.ToString(0, this.Length);
        }

        /// <summary>
        /// 转换为UTF-8字符
        /// </summary>
        /// <param name="offset">偏移量</param>
        /// <param name="length">长度</param>
        /// <returns></returns>
        public string ToString(int offset, int length)
        {
            this.ThrowIfDisposed();

            return Encoding.UTF8.GetString(this.m_buffer, offset, length);
        }

        /// <summary>
        /// 转换为UTF-8字符
        /// </summary>
        /// <param name="offset">偏移量</param>
        /// <returns></returns>
        public string ToString(int offset)
        {
            this.ThrowIfDisposed();

            return Encoding.UTF8.GetString(this.m_buffer, offset, this.Length - offset);
        }

        #endregion ToString

        #region Enumerator

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public IEnumerator<byte> GetEnumerator()
        {
            var pos = 0;
            while (true)
            {
                if (pos < this.m_length)
                {
                    yield return this.m_buffer[pos++];
                }
                else
                {
                    yield break;
                }
            }
        }

        #endregion Enumerator
    }
}