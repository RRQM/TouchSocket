//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
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

using RRQMCore.Serialization;
using RRQMCore.XREF.Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace RRQMCore.ByteManager
{
    /// <summary>
    /// 字节块流
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("Len={Len}")]
    public sealed class ByteBlock : Stream, IDisposable
    {
        internal long m_length;
        internal bool m_using;
        private static float m_ratio = 1.5f;
        private byte[] m_buffer;
        private bool m_holding;
        private bool m_needDis;
        private long m_position;

        /// <summary>
        ///  构造函数
        /// </summary>
        /// <param name="byteSize"></param>
        /// <param name="equalSize"></param>
        public ByteBlock(int byteSize = 1024 * 64, bool equalSize = false)
        {
            this.m_needDis = true;
            this.m_buffer = BytePool.GetByteCore(byteSize, equalSize);
            this.m_using = true;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="bytes"></param>
        public ByteBlock(byte[] bytes)
        {
            this.m_buffer = bytes ?? throw new ArgumentNullException(nameof(bytes));
            this.m_length = bytes.Length;
            this.m_using = true;
        }

        /// <summary>
        /// 扩容增长比，默认为1.5，
        /// min：1.5
        /// </summary>
        public static float Ratio
        {
            get => m_ratio;
            set
            {
                if (value < 1.5)
                {
                    value = 1.5f;
                }
                m_ratio = value;
            }
        }

        /// <summary>
        /// 字节实例
        /// </summary>
        public byte[] Buffer => this.m_buffer;

        /// <summary>
        /// 仅当内存块可用，且<see cref="CanReadLen"/>>0时为True。
        /// </summary>
        public override bool CanRead => this.m_using&&this.CanReadLen>0;

        /// <summary>
        /// 还能读取的长度，计算为<see cref="Len"/>与<see cref="Pos"/>的差值。
        /// </summary>
        public int CanReadLen => this.Len - this.Pos;

        /// <summary>
        /// 还能读取的长度，计算为<see cref="Len"/>与<see cref="Pos"/>的差值。
        /// </summary>
        public long CanReadLength => this.m_length - this.m_position;

        /// <summary>
        /// 支持查找
        /// </summary>
        public override bool CanSeek => this.m_using;

        /// <summary>
        /// 可写入
        /// </summary>
        public override bool CanWrite => this.m_using;

        /// <summary>
        /// 容量
        /// </summary>
        public int Capacity => this.m_buffer.Length;

        /// <summary>
        /// 表示持续性持有，为True时，Dispose将调用无效。
        /// </summary>
        public bool Holding => this.m_holding;

        /// <summary>
        /// Int真实长度
        /// </summary>
        public int Len => (int)this.m_length;

        /// <summary>
        /// 真实长度
        /// </summary>
        public override long Length => this.m_length;

        /// <summary>
        /// int型流位置
        /// </summary>
        public int Pos
        {
            get => (int)this.m_position;
            set => this.m_position = value;
        }

        /// <summary>
        /// 流位置
        /// </summary>
        public override long Position
        {
            get => this.m_position;
            set => this.m_position = value;
        }

        /// <summary>
        /// 使用状态
        /// </summary>
        public bool Using => this.m_using;

        /// <summary>
        /// 直接完全释放，游离该对象，然后等待GC
        /// </summary>
        public void AbsoluteDispose()
        {
            this.m_holding = false;
            this.m_using = false;
            this.m_position = 0;
            this.m_length = 0;
            this.m_buffer = null;
        }

        /// <summary>
        /// 清空所有内存数据
        /// </summary>
        /// <exception cref="ObjectDisposedException">内存块已释放</exception>
        public void Clear()
        {
            if (!this.m_using)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            Array.Clear(this.m_buffer, 0, this.m_buffer.Length);
        }

        /// <summary>
        /// 将内存块初始化到刚申请的状态。
        /// <para>仅仅重置<see cref="Position"/>和<see cref="Length"/>属性。</para>
        /// </summary>
        /// <exception cref="ObjectDisposedException">内存块已释放</exception>
        public void Reset()
        {
            if (!this.m_using)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            this.m_position = 0;
            this.m_length = 0;
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
            if (!this.m_using)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            int len = this.m_length - this.m_position > length ? length : this.CanReadLen;
            Array.Copy(this.m_buffer, this.m_position, buffer, offset, len);
            this.m_position += len;
            return len;
        }

        /// <summary>
        /// 读取数据，然后递增Pos
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public int Read(byte[] buffer)
        {
            return this.Read(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 读取数据，然后递增Pos
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public int Read(out byte[] buffer, int length)
        {
            buffer = new byte[length];
            return this.Read(buffer, 0, buffer.Length);
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
            if (!this.m_using)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
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
        /// 重新设置容量
        /// </summary>
        /// <param name="size">新尺寸</param>
        /// <param name="retainedData">是否保留元数据</param>
        /// <exception cref="ObjectDisposedException"></exception>
        public void SetCapacity(int size, bool retainedData = false)
        {
            if (!this.m_using)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            byte[] bytes = new byte[size];

            if (retainedData)
            {
                Array.Copy(this.m_buffer, 0, bytes, 0, this.m_buffer.Length);
            }
            BytePool.Recycle(this.m_buffer);
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
            if (!this.m_using)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            this.m_holding = holding;
            if (!holding)
            {
                this.Dispose();
            }
        }

        /// <summary>
        /// 设置实际长度
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="ObjectDisposedException"></exception>
        public override void SetLength(long value)
        {
            if (!this.m_using)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (value > this.m_buffer.Length)
            {
                throw new RRQMException("设置值超出容量");
            }
            this.m_length = value;
        }

        /// <summary>
        /// 转换为有效内存
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            return this.ToArray(0);
        }

        /// <summary>
        /// 从指定位置转化到有效内存
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public byte[] ToArray(int offset)
        {
            return this.ToArray(offset, (int)(this.m_length - offset));
        }

        /// <summary>
        /// 从指定位置转化到指定长度的有效内存
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public byte[] ToArray(int offset, int length)
        {
            if (!this.m_using)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            byte[] buffer = new byte[length];
            Array.Copy(this.m_buffer, offset, buffer, 0, buffer.Length);
            return buffer;
        }

        /// <summary>
        /// 转换为UTF-8字符
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.ToString(0, this.Len);
        }

        /// <summary>
        /// 转换为UTF-8字符
        /// </summary>
        /// <param name="offset">偏移量</param>
        /// <param name="length">长度</param>
        /// <returns></returns>
        public string ToString(int offset, int length)
        {
            if (!this.m_using)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            return Encoding.UTF8.GetString(this.m_buffer, offset, length);
        }

        /// <summary>
        /// 转换为UTF-8字符
        /// </summary>
        /// <param name="offset">偏移量</param>
        /// <returns></returns>
        public string ToString(int offset)
        {
            if (!this.m_using)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            return Encoding.UTF8.GetString(this.m_buffer, offset, this.Len - offset);
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
            if (count == 0)
            {
                return;
            }
            if (!this.m_using)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (this.m_buffer.Length - this.m_position < count)
            {
                int need = this.m_buffer.Length + count - ((int)(this.m_buffer.Length - this.m_position));
                int lend = this.m_buffer.Length;
                while (need > lend)
                {
                    lend = (int)(lend * m_ratio);
                }
                this.SetCapacity(lend, true);
            }
            Array.Copy(buffer, offset, this.m_buffer, this.m_position, count);
            this.m_position += count;
            this.m_length = Math.Max(this.m_position, this.m_length);
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public void Write(byte[] buffer)
        {
            this.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (this.m_holding)
            {
                return;
            }

            if (this.m_needDis)
            {
                lock (this)
                {
                    if (this.m_using)
                    {
                        GC.SuppressFinalize(this);
                        BytePool.Recycle(this.m_buffer);
                        this.AbsoluteDispose();
                    }
                }
            }

            base.Dispose(disposing);
        }

        #region BytesPackage

        /// <summary>
        /// 从当前流位置读取一个独立的<see cref="byte"/>数组包
        /// </summary>
        public byte[] ReadBytesPackage()
        {
            byte status = this.ReadByte();
            if (status == 0)
            {
                return null;
            }
            int length = this.ReadInt32();
            byte[] data = new byte[length];
            Array.Copy(this.m_buffer, this.m_position, data, 0, length);
            this.m_position += length;
            return data;
        }

        /// <summary>
        /// 尝试获取数据包信息，方便从Buffer操作数据
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public bool TryReadBytesPackageInfo(out int pos, out int len)
        {
            byte status = this.ReadByte();
            if (status == 0)
            {
                pos = 0;
                len = 0;
                return false;
            }
            len = this.ReadInt32();
            pos = (int)this.m_position;
            return true;
        }

        /// <summary>
        /// 写入一个独立的<see cref="byte"/>数组包
        /// </summary>
        /// <param name="value"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public ByteBlock WriteBytesPackage(byte[] value, int offset, int length)
        {
            if (value == null)
            {
                this.Write((byte)0);
            }
            else
            {
                this.Write((byte)1);
                this.Write(length);
                this.Write(value, offset, length);
            }
            return this;
        }

        /// <summary>
        /// 写入一个独立的<see cref="byte"/>数组包
        /// </summary>
        /// <param name="value"></param>
        public ByteBlock WriteBytesPackage(byte[] value)
        {
            if (value == null)
            {
                return this.WriteBytesPackage(value, 0, 0);
            }
            return this.WriteBytesPackage(value, 0, value.Length);
        }

        #endregion BytesPackage

        #region Int32

        /// <summary>
        /// 从当前流位置读取一个<see cref="int"/>值
        /// </summary>
        public int ReadInt32()
        {
            int value = RRQMBitConverter.Default.ToInt32(this.m_buffer, (int)this.m_position);
            this.m_position += 4;
            return value;
        }

        /// <summary>
        /// 写入<see cref="int"/>值
        /// </summary>
        /// <param name="value"></param>
        public ByteBlock Write(int value)
        {
            this.Write(RRQMBitConverter.Default.GetBytes(value));
            return this;
        }

        #endregion Int32

        #region Int16

        /// <summary>
        /// 从当前流位置读取一个<see cref="short"/>值
        /// </summary>
        public short ReadInt16()
        {
            short value = RRQMBitConverter.Default.ToInt16(this.m_buffer, (int)this.m_position);
            this.m_position += 2;
            return value;
        }

        /// <summary>
        /// 写入<see cref="short"/>值
        /// </summary>
        /// <param name="value"></param>
        public ByteBlock Write(short value)
        {
            this.Write(RRQMBitConverter.Default.GetBytes(value));
            return this;
        }

        #endregion Int16

        #region Int64

        /// <summary>
        /// 从当前流位置读取一个<see cref="long"/>值
        /// </summary>
        public long ReadInt64()
        {
            long value = RRQMBitConverter.Default.ToInt64(this.m_buffer, (int)this.m_position);
            this.m_position += 8;
            return value;
        }

        /// <summary>
        /// 写入<see cref="long"/>值
        /// </summary>
        /// <param name="value"></param>
        public ByteBlock Write(long value)
        {
            this.Write(RRQMBitConverter.Default.GetBytes(value));
            return this;
        }

        #endregion Int64

        #region Boolean

        /// <summary>
        /// 从当前流位置读取一个<see cref="bool"/>值
        /// </summary>
        public bool ReadBoolean()
        {
            bool value = RRQMBitConverter.Default.ToBoolean(this.m_buffer, (int)this.m_position);
            this.m_position += 1;
            return value;
        }

        /// <summary>
        /// 写入<see cref="bool"/>值
        /// </summary>
        /// <param name="value"></param>
        public ByteBlock Write(bool value)
        {
            this.Write(RRQMBitConverter.Default.GetBytes(value));
            return this;
        }

        #endregion Boolean

        #region Byte

        /// <summary>
        /// 从当前流位置读取一个<see cref="byte"/>值
        /// </summary>
        public new byte ReadByte()
        {
            byte value = this.m_buffer[this.m_position];
            this.m_position++;
            return value;
        }

        /// <summary>
        /// 写入<see cref="byte"/>值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ByteBlock Write(byte value)
        {
            this.Write(new byte[] { value }, 0, 1);
            return this;
        }

        #endregion Byte

        #region String

        /// <summary>
        /// 从当前流位置读取一个<see cref="string"/>值
        /// </summary>
        public string ReadString()
        {
            byte value = this.ReadByte();
            if (value == 0)
            {
                return null;
            }
            else if (value == 1)
            {
                return string.Empty;
            }
            else
            {
                ushort len = this.ReadUInt16();
                string str = Encoding.UTF8.GetString(this.m_buffer, (int)this.m_position, len);
                this.m_position += len;
                return str;
            }
        }

        /// <summary>
        /// 写入<see cref="string"/>值。
        /// <para>读取时必须使用ReadString</para>
        /// </summary>
        /// <param name="value"></param>
        public ByteBlock Write(string value)
        {
            if (value == null)
            {
                this.Write((byte)0);
            }
            else if (value == string.Empty)
            {
                this.Write((byte)1);
            }
            else
            {
                this.Write((byte)2);
                byte[] buffer = Encoding.UTF8.GetBytes(value);
                if (buffer.Length > ushort.MaxValue)
                {
                    throw new RRQMException("传输长度超长");
                }
                this.Write((ushort)buffer.Length);
                this.Write(buffer);
            }
            return this;
        }

        #endregion String

        #region Char

        /// <summary>
        /// 从当前流位置读取一个<see cref="char"/>值
        /// </summary>
        public char ReadChar()
        {
            char value = RRQMBitConverter.Default.ToChar(this.m_buffer, (int)this.m_position);
            this.m_position += 2;
            return value;
        }

        /// <summary>
        /// 写入<see cref="char"/>值
        /// </summary>
        /// <param name="value"></param>
        public ByteBlock Write(char value)
        {
            this.Write(RRQMBitConverter.Default.GetBytes(value));
            return this;
        }

        #endregion Char

        #region Double

        /// <summary>
        /// 从当前流位置读取一个<see cref="double"/>值
        /// </summary>
        public double ReadDouble()
        {
            double value = RRQMBitConverter.Default.ToDouble(this.m_buffer, (int)this.m_position);
            this.m_position += 8;
            return value;
        }

        /// <summary>
        /// 写入<see cref="double"/>值
        /// </summary>
        /// <param name="value"></param>
        public ByteBlock Write(double value)
        {
            this.Write(RRQMBitConverter.Default.GetBytes(value));
            return this;
        }

        #endregion Double

        #region Float

        /// <summary>
        /// 从当前流位置读取一个<see cref="float"/>值
        /// </summary>
        public float ReadFloat()
        {
            float value = RRQMBitConverter.Default.ToSingle(this.m_buffer, (int)this.m_position);
            this.m_position += 4;
            return value;
        }

        /// <summary>
        /// 写入<see cref="float"/>值
        /// </summary>
        /// <param name="value"></param>
        public ByteBlock Write(float value)
        {
            this.Write(RRQMBitConverter.Default.GetBytes(value));
            return this;
        }

        #endregion Float

        #region UInt16

        /// <summary>
        /// 从当前流位置读取一个<see cref="ushort"/>值
        /// </summary>
        public ushort ReadUInt16()
        {
            ushort value = RRQMBitConverter.Default.ToUInt16(this.m_buffer, (int)this.m_position);
            this.m_position += 2;
            return value;
        }

        /// <summary>
        /// 写入<see cref="ushort"/>值
        /// </summary>
        /// <param name="value"></param>
        public ByteBlock Write(ushort value)
        {
            this.Write(RRQMBitConverter.Default.GetBytes(value));
            return this;
        }

        #endregion UInt16

        #region UInt32

        /// <summary>
        /// 从当前流位置读取一个<see cref="uint"/>值
        /// </summary>
        public uint ReadUInt32()
        {
            uint value = RRQMBitConverter.Default.ToUInt32(this.m_buffer, (int)this.m_position);
            this.m_position += 4;
            return value;
        }

        /// <summary>
        /// 写入<see cref="uint"/>值
        /// </summary>
        /// <param name="value"></param>
        public ByteBlock Write(uint value)
        {
            this.Write(RRQMBitConverter.Default.GetBytes(value));
            return this;
        }

        #endregion UInt32

        #region UInt64

        /// <summary>
        /// 从当前流位置读取一个<see cref="ulong"/>值
        /// </summary>
        public ulong ReadUInt64()
        {
            ulong value = RRQMBitConverter.Default.ToUInt64(this.m_buffer, (int)this.m_position);
            this.m_position += 8;
            return value;
        }

        /// <summary>
        /// 写入<see cref="ulong"/>值
        /// </summary>
        /// <param name="value"></param>
        public ByteBlock Write(ulong value)
        {
            this.Write(RRQMBitConverter.Default.GetBytes(value));
            return this;
        }

        #endregion UInt64

        #region DateTime

        /// <summary>
        /// 从当前流位置读取一个<see cref="DateTime"/>值
        /// </summary>
        public DateTime ReadDateTime()
        {
            long value = RRQMBitConverter.Default.ToInt64(this.m_buffer, (int)this.m_position);
            this.m_position += 8;
            return DateTime.FromBinary(value);
        }

        /// <summary>
        /// 写入<see cref="DateTime"/>值
        /// </summary>
        /// <param name="value"></param>
        public ByteBlock Write(DateTime value)
        {
            this.Write(RRQMBitConverter.Default.GetBytes(value.ToBinary()));
            return this;
        }

        #endregion DateTime

        #region Object

        /// <summary>
        ///  从当前流位置读取一个泛型值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializationType"></param>
        /// <returns></returns>
        public T ReadObject<T>(SerializationType serializationType = SerializationType.RRQMBinary)
        {
            int length = this.ReadInt32();

            if (length == 0)
            {
                return default;
            }

            T obj;

            switch (serializationType)
            {
                case SerializationType.RRQMBinary:
                    {
                        obj = SerializeConvert.RRQMBinaryDeserialize<T>(this.m_buffer, (int)this.m_position);
                    }
                    break;

                case SerializationType.Json:
                    {
                        string jsonString = Encoding.UTF8.GetString(this.m_buffer, (int)this.m_position, length);
                        obj = JsonConvert.DeserializeObject<T>(jsonString);
                    }
                    break;

                default:
                    throw new RRQMException("未定义的序列化类型");
            }

            this.m_position += length;
            return obj;
        }

        /// <summary>
        /// 写入<see cref="object"/>值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="serializationType"></param>
        public ByteBlock WriteObject(object value, SerializationType serializationType = SerializationType.RRQMBinary)
        {
            if (value == null)
            {
                this.Write(0);
                return this;
            }
            byte[] data;
            switch (serializationType)
            {
                case SerializationType.RRQMBinary:
                    {
                        data = SerializeConvert.RRQMBinarySerialize(value);
                    }
                    break;

                case SerializationType.Json:
                    {
                        data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value));
                    }
                    break;

                default:
                    throw new RRQMException("未定义的序列化类型");
            }

            this.Write(data.Length);
            this.Write(data);
            return this;
        }

        #endregion Object
    }
}