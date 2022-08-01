//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace TouchSocket.Core.ByteManager
//{
//    /// <summary>
//    /// 字节块流
//    /// </summary>
//    [System.Diagnostics.DebuggerDisplay("Len={Len}")]
//    public ref struct ValueByteBlock /*: IByteBlock*/
//    {
//        internal long m_length;
//        internal bool m_using;
//        private static float m_ratio = 1.5f;
//        private byte[] m_buffer;
//        private bool m_holding;
//        private object m_locker;
//        private bool m_needDis;
//        private long m_position;

//        /// <summary>
//        ///  构造函数
//        /// </summary>
//        /// <param name="byteSize"></param>
//        /// <param name="equalSize"></param>
//        public ValueByteBlock(int byteSize = 1024 * 64, bool equalSize = false)
//        {
//            this.m_needDis = true;
//            this.m_buffer = BytePool.GetByteCore(byteSize, equalSize);
//            this.m_using = true;
//            this.m_length = 0;
//            this.m_holding = false;
//            this.m_position = 0;
//            this.m_locker = new object();
//        }

//        /// <summary>
//        /// 构造函数
//        /// </summary>
//        /// <param name="bytes"></param>
//        public ValueByteBlock(byte[] bytes)
//        {
//            this.m_buffer = bytes ?? throw new ArgumentNullException(nameof(bytes));
//            this.m_length = bytes.Length;
//            this.m_using = true;
//            this.m_needDis = false;
//            this.m_length = 0;
//            this.m_holding = false;
//            this.m_position = 0;
//            this.m_locker = new object();
//        }

//        /// <summary>
//        /// 扩容增长比，默认为1.5，
//        /// min：1.5
//        /// </summary>
//        public static float Ratio
//        {
//            get => m_ratio;
//            set
//            {
//                if (value < 1.5)
//                {
//                    value = 1.5f;
//                }
//                m_ratio = value;
//            }
//        }

//        /// <summary>
//        /// 字节实例
//        /// </summary>
//        public byte[] Buffer => this.m_buffer;

//        /// <summary>
//        /// 仅当内存块可用，且<see cref="CanReadLen"/>>0时为True。
//        /// </summary>
//        public bool CanRead => this.m_using && this.CanReadLen > 0;

//        /// <summary>
//        /// 还能读取的长度，计算为<see cref="Len"/>与<see cref="Pos"/>的差值。
//        /// </summary>
//        public int CanReadLen => this.Len - this.Pos;

//        /// <summary>
//        /// 还能读取的长度，计算为<see cref="Len"/>与<see cref="Pos"/>的差值。
//        /// </summary>
//        public long CanReadLength => this.m_length - this.m_position;

//        /// <summary>
//        /// 容量
//        /// </summary>
//        public int Capacity => this.m_buffer.Length;

//        /// <summary>
//        /// 表示持续性持有，为True时，Dispose将调用无效。
//        /// </summary>
//        public bool Holding => this.m_holding;

//        /// <summary>
//        /// Int真实长度
//        /// </summary>
//        public int Len => (int)this.m_length;

//        /// <summary>
//        /// 真实长度
//        /// </summary>
//        public long Length => this.m_length;

//        /// <summary>
//        /// int型流位置
//        /// </summary>
//        public int Pos
//        {
//            get => (int)this.m_position;
//            set => this.m_position = value;
//        }

//        /// <summary>
//        /// 流位置
//        /// </summary>
//        public long Position
//        {
//            get => this.m_position;
//            set => this.m_position = value;
//        }

//        /// <summary>
//        /// 使用状态
//        /// </summary>
//        public bool Using => this.m_using;

//        /// <summary>
//        /// <inheritdoc/>
//        /// </summary>
//        public int FreeLength => this.Capacity - this.Pos;

//        /// <summary>
//        /// 直接完全释放，游离该对象，然后等待GC
//        /// </summary>
//        public void AbsoluteDispose()
//        {
//            this.m_holding = false;
//            this.m_using = false;
//            this.m_position = 0;
//            this.m_length = 0;
//            this.m_buffer = null;
//        }

//        /// <summary>
//        /// 清空所有内存数据
//        /// </summary>
//        /// <exception cref="ObjectDisposedException">内存块已释放</exception>
//        public void Clear()
//        {
//            if (!this.m_using)
//            {
//                throw new ObjectDisposedException(this.GetType().FullName);
//            }
//            Array.Clear(this.m_buffer, 0, this.m_buffer.Length);
//        }

//        /// <summary>
//        /// 释放
//        /// </summary>
//        public void Dispose()
//        {
//            if (this.m_holding)
//            {
//                return;
//            }

//            if (this.m_needDis)
//            {
//                lock (m_locker)
//                {
//                    if (this.m_using)
//                    {
//                        GC.SuppressFinalize(this);
//                        BytePool.Recycle(this.m_buffer);
//                        this.AbsoluteDispose();
//                    }
//                }
//            }
//        }

//        /// <summary>
//        /// 读取数据，然后递增Pos
//        /// </summary>
//        /// <param name="buffer"></param>
//        /// <param name="offset"></param>
//        /// <param name="length"></param>
//        /// <returns></returns>
//        /// <exception cref="ObjectDisposedException"></exception>
//        public int Read(byte[] buffer, int offset, int length)
//        {
//            if (!this.m_using)
//            {
//                throw new ObjectDisposedException(this.GetType().FullName);
//            }
//            int len = this.m_length - this.m_position > length ? length : this.CanReadLen;
//            Array.Copy(this.m_buffer, this.m_position, buffer, offset, len);
//            this.m_position += len;
//            return len;
//        }

//        /// <summary>
//        /// 读取数据，然后递增Pos
//        /// </summary>
//        /// <param name="buffer"></param>
//        /// <returns></returns>
//        public int Read(byte[] buffer)
//        {
//            return this.Read(buffer, 0, buffer.Length);
//        }

//        /// <summary>
//        /// 读取数据，然后递增Pos
//        /// </summary>
//        /// <param name="buffer"></param>
//        /// <param name="length"></param>
//        /// <returns></returns>
//        public int Read(out byte[] buffer, int length)
//        {
//            buffer = new byte[length];
//            return this.Read(buffer, 0, buffer.Length);
//        }

//        /// <summary>
//        /// <inheritdoc/>
//        /// </summary>
//        /// <returns></returns>
//        public int ReadByte()
//        {
//            byte value = this.m_buffer[this.m_position];
//            this.m_position++;
//            return value;
//        }

//        /// <summary>
//        /// 将内存块初始化到刚申请的状态。
//        /// <para>仅仅重置<see cref="Position"/>和<see cref="Length"/>属性。</para>
//        /// </summary>
//        /// <exception cref="ObjectDisposedException">内存块已释放</exception>
//        public void Reset()
//        {
//            if (!this.m_using)
//            {
//                throw new ObjectDisposedException(this.GetType().FullName);
//            }
//            this.m_position = 0;
//            this.m_length = 0;
//        }

//        /// <summary>
//        /// 重新设置容量
//        /// </summary>
//        /// <param name="size">新尺寸</param>
//        /// <param name="retainedData">是否保留元数据</param>
//        /// <exception cref="ObjectDisposedException"></exception>
//        public void SetCapacity(int size, bool retainedData = false)
//        {
//            if (!this.m_using)
//            {
//                throw new ObjectDisposedException(this.GetType().FullName);
//            }
//            byte[] bytes = new byte[size];

//            if (retainedData)
//            {
//                Array.Copy(this.m_buffer, 0, bytes, 0, this.m_buffer.Length);
//            }
//            BytePool.Recycle(this.m_buffer);
//            this.m_buffer = bytes;
//        }

//        /// <summary>
//        /// 设置持续持有属性，当为True时，调用Dispose会失效，表示该对象将长期持有，直至设置为False。
//        /// 当为False时，会自动调用Dispose。
//        /// </summary>
//        /// <param name="holding"></param>
//        /// <exception cref="ObjectDisposedException"></exception>
//        public void SetHolding(bool holding)
//        {
//            if (!this.m_using)
//            {
//                throw new ObjectDisposedException(this.GetType().FullName);
//            }
//            this.m_holding = holding;
//            if (!holding)
//            {
//                this.Dispose();
//            }
//        }

//        /// <summary>
//        /// 设置实际长度
//        /// </summary>
//        /// <param name="value"></param>
//        /// <exception cref="ObjectDisposedException"></exception>
//        public void SetLength(long value)
//        {
//            if (!this.m_using)
//            {
//                throw new ObjectDisposedException(this.GetType().FullName);
//            }
//            if (value > this.m_buffer.Length)
//            {
//                throw new Exception("设置值超出容量");
//            }
//            this.m_length = value;
//        }

//        /// <summary>
//        /// 从指定位置转化到指定长度的有效内存
//        /// </summary>
//        /// <param name="offset"></param>
//        /// <param name="length"></param>
//        /// <returns></returns>
//        public byte[] ToArray(int offset, int length)
//        {
//            if (!this.m_using)
//            {
//                throw new ObjectDisposedException(this.GetType().FullName);
//            }
//            byte[] buffer = new byte[length];
//            Array.Copy(this.m_buffer, offset, buffer, 0, buffer.Length);
//            return buffer;
//        }

//        /// <summary>
//        /// 转换为UTF-8字符
//        /// </summary>
//        /// <returns></returns>
//        public override string ToString()
//        {
//            return this.ToString(0, this.Len);
//        }

//        /// <summary>
//        /// 转换为UTF-8字符
//        /// </summary>
//        /// <param name="offset">偏移量</param>
//        /// <param name="length">长度</param>
//        /// <returns></returns>
//        public string ToString(int offset, int length)
//        {
//            if (!this.m_using)
//            {
//                throw new ObjectDisposedException(this.GetType().FullName);
//            }
//            return Encoding.UTF8.GetString(this.m_buffer, offset, length);
//        }

//        /// <summary>
//        /// 转换为UTF-8字符
//        /// </summary>
//        /// <param name="offset">偏移量</param>
//        /// <returns></returns>
//        public string ToString(int offset)
//        {
//            if (!this.m_using)
//            {
//                throw new ObjectDisposedException(this.GetType().FullName);
//            }
//            return Encoding.UTF8.GetString(this.m_buffer, offset, this.Len - offset);
//        }

//        /// <summary>
//        /// 写入
//        /// </summary>
//        /// <param name="buffer"></param>
//        /// <param name="offset"></param>
//        /// <param name="count"></param>
//        /// <exception cref="ObjectDisposedException"></exception>
//        public void Write(byte[] buffer, int offset, int count)
//        {
//            if (count == 0)
//            {
//                return;
//            }
//            if (!this.m_using)
//            {
//                throw new ObjectDisposedException(this.GetType().FullName);
//            }
//            if (this.m_buffer.Length - this.m_position < count)
//            {
//                int need = this.m_buffer.Length + count - ((int)(this.m_buffer.Length - this.m_position));
//                int lend = this.m_buffer.Length;
//                while (need > lend)
//                {
//                    lend = (int)(lend * m_ratio);
//                }
//                this.SetCapacity(lend, true);
//            }
//            Array.Copy(buffer, offset, this.m_buffer, this.m_position, count);
//            this.m_position += count;
//            this.m_length = Math.Max(this.m_position, this.m_length);
//        }
//    }
//}