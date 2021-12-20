//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.Exceptions;
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
    public sealed class ByteBlock : Stream, IDisposable
    {
        internal bool @using;

        internal long length;

        private static float ratio = 1.5f;

        private byte[] _buffer;

        private bool holding;

        private long position;

        /// <summary>
        ///  构造函数
        /// </summary>
        /// <param name="capacity"></param>
        /// <param name="equalSize"></param>
        public ByteBlock(int capacity = 1024 * 10, bool equalSize = false) : this(BytePool.GetByteCore(capacity, false))
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="bytes"></param>
        internal ByteBlock(byte[] bytes)
        {
            this.@using = true;
            this._buffer = bytes;
        }

        /// <summary>
        /// 清空数据
        /// </summary>
        public void Clear()
        {
            if (!this.@using)
            {
                throw new RRQMException("内存块已释放");
            }
            Array.Clear(this._buffer, 0, this._buffer.Length);
        }

        /// <summary>
        /// 扩容增长比，默认为1.5，
        /// min：1
        /// </summary>
        public static float Ratio
        {
            get { return ratio; }
            set
            {
                if (value < 1)
                {
                    value = 1;
                }
                ratio = value;
            }
        }

        /// <summary>
        /// 字节实例
        /// </summary>
        public byte[] Buffer
        {
            get { return _buffer; }
        }

        /// <summary>
        /// 可读取
        /// </summary>
        public override bool CanRead => this.@using;

        /// <summary>
        /// 支持查找
        /// </summary>
        public override bool CanSeek => this.@using;

        /// <summary>
        /// 可写入
        /// </summary>
        public override bool CanWrite => this.@using;

        /// <summary>
        /// 容量
        /// </summary>
        public int Capacity => this._buffer.Length;

        /// <summary>
        /// 表示持续性持有，为True时，Dispose将调用无效。
        /// </summary>
        public bool Holding
        {
            get { return holding; }
        }

        /// <summary>
        /// Int真实长度
        /// </summary>
        public int Len
        { get { return (int)length; } }

        /// <summary>
        /// 真实长度
        /// </summary>
        public override long Length
        { get { return length; } }

        /// <summary>
        /// int型流位置
        /// </summary>
        public int Pos
        {
            get { return (int)position; }
            set { position = value; }
        }

        /// <summary>
        /// 流位置
        /// </summary>
        public override long Position
        {
            get { return position; }
            set { position = value; }
        }

        /// <summary>
        /// 使用状态
        /// </summary>
        public bool Using
        {
            get { return @using; }
        }

        /// <summary>
        /// 直接完全释放，游离该对象，然后等待GC
        /// </summary>
        public void AbsoluteDispose()
        {
            this.holding = false;
            this.@using = false;
            this.position = 0;
            this.length = 0;
            this._buffer = default;
        }

        /// <summary>
        /// 回收资源
        /// </summary>
        public new void Dispose()
        {
            if (this.holding)
            {
                return;
            }
            if (this.@using)
            {
                lock (this)
                {
                    if (this.@using)
                    {
                        GC.SuppressFinalize(this);
                        BytePool.Recycle(this._buffer);
                        this.AbsoluteDispose();
                    }
                }
            }
        }

        /// <summary>
        /// 无实际效果
        /// </summary>
        public override void Flush()
        {
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!this.@using)
            {
                throw new RRQMException("内存块已释放");
            }
            int len = this._buffer.Length - this.position > count ? count : this._buffer.Length - (int)this.position;
            Array.Copy(this._buffer, this.position, buffer, offset, len);
            this.position += len;
            return len;
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public int Read(byte[] buffer)
        {
            return Read(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 设置流位置
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!this.@using)
            {
                throw new RRQMException("内存块已释放");
            }
            switch (origin)
            {
                case SeekOrigin.Begin:
                    this.position = offset;
                    break;

                case SeekOrigin.Current:
                    this.position = offset;
                    break;

                case SeekOrigin.End:
                    this.position = this.length + offset;
                    break;
            }
            return this.position;
        }

        /// <summary>
        /// 重新设置容量
        /// </summary>
        /// <param name="size">新尺寸</param>
        /// <param name="retainedData">是否保留元数据</param>
        public void SetCapacity(int size, bool retainedData = false)
        {
            if (!this.@using)
            {
                throw new RRQMException("内存块已释放");
            }
            byte[] bytes = new byte[size];

            if (retainedData)
            {
                Array.Copy(this._buffer, 0, bytes, 0, this.Len);
            }
            BytePool.Recycle(this._buffer);
            this._buffer = bytes;
        }

        /// <summary>
        /// 设置持续持有属性，当为True时，调用Dispose会失效，表示该对象将长期持有，直至设置为False。
        /// 当为False时，会自动调用Dispose。
        /// </summary>
        /// <param name="holding"></param>
        public void SetHolding(bool holding)
        {
            if (!this.@using)
            {
                throw new RRQMException("内存块已释放");
            }
            this.holding = holding;
            if (!holding)
            {
                this.Dispose();
            }
        }

        /// <summary>
        /// 设置实际长度
        /// </summary>
        /// <param name="value"></param>
        public override void SetLength(long value)
        {
            if (!this.@using)
            {
                throw new RRQMException("内存块已释放");
            }
            if (value > this._buffer.Length)
            {
                throw new RRQMException("设置值超出容量");
            }
            this.length = value;
        }

        /// <summary>
        /// 转换为有效内存
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            return ToArray(0);
        }

        /// <summary>
        /// 从指定位置转化到有效内存
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public byte[] ToArray(int offset)
        {
            if (!this.@using)
            {
                throw new RRQMException("内存块已释放");
            }
            byte[] buffer = new byte[this.length - offset];
            Array.Copy(this._buffer, offset, buffer, 0, buffer.Length);
            return buffer;
        }

        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!this.@using)
            {
                throw new RRQMException("内存块已释放");
            }
            if (this._buffer.Length - this.position < count)
            {
                this.SetCapacity(this._buffer.Length + (int)((count + this.position - this._buffer.Length) * ratio), true);
            }
            Array.Copy(buffer, offset, _buffer, this.position, count);
            this.position += count;
            this.length += count;
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
            Array.Copy(this._buffer, this.position, data, 0, length);
            this.position += length;
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
            pos = (int)this.position;
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
            int value = BitConverter.ToInt32(this._buffer, (int)this.position);
            this.position += 4;
            return value;
        }

        /// <summary>
        /// 写入<see cref="int"/>值
        /// </summary>
        /// <param name="value"></param>
        public ByteBlock Write(int value)
        {
            this.Write(BitConverter.GetBytes(value));
            return this;
        }

        #endregion Int32

        #region Int16

        /// <summary>
        /// 从当前流位置读取一个<see cref="short"/>值
        /// </summary>
        public short ReadInt16()
        {
            short value = BitConverter.ToInt16(this._buffer, (int)this.position);
            this.position += 2;
            return value;
        }

        /// <summary>
        /// 写入<see cref="short"/>值
        /// </summary>
        /// <param name="value"></param>
        public ByteBlock Write(short value)
        {
            this.Write(BitConverter.GetBytes(value));
            return this;
        }

        #endregion Int16

        #region Int64

        /// <summary>
        /// 从当前流位置读取一个<see cref="long"/>值
        /// </summary>
        public long ReadInt64()
        {
            long value = BitConverter.ToInt64(this._buffer, (int)this.position);
            this.position += 8;
            return value;
        }

        /// <summary>
        /// 写入<see cref="long"/>值
        /// </summary>
        /// <param name="value"></param>
        public ByteBlock Write(long value)
        {
            this.Write(BitConverter.GetBytes(value));
            return this;
        }

        #endregion Int64

        #region Boolean

        /// <summary>
        /// 从当前流位置读取一个<see cref="bool"/>值
        /// </summary>
        public bool ReadBoolean()
        {
            bool value = BitConverter.ToBoolean(this._buffer, (int)this.position);
            this.position += 1;
            return value;
        }

        /// <summary>
        /// 写入<see cref="bool"/>值
        /// </summary>
        /// <param name="value"></param>
        public ByteBlock Write(bool value)
        {
            this.Write(BitConverter.GetBytes(value));
            return this;
        }

        #endregion Boolean

        #region Byte

        /// <summary>
        /// 从当前流位置读取一个<see cref="byte"/>值
        /// </summary>
        public new byte ReadByte()
        {
            byte value = this._buffer[this.position];
            this.position++;
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
                string str = Encoding.UTF8.GetString(this._buffer, (int)this.position, len);
                this.position += len;
                return str;
            }
        }

        /// <summary>
        /// 写入<see cref="string"/>值
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
            char value = BitConverter.ToChar(this._buffer, (int)this.position);
            this.position += 2;
            return value;
        }

        /// <summary>
        /// 写入<see cref="char"/>值
        /// </summary>
        /// <param name="value"></param>
        public ByteBlock Write(char value)
        {
            this.Write(BitConverter.GetBytes(value));
            return this;
        }

        #endregion Char

        #region Double

        /// <summary>
        /// 从当前流位置读取一个<see cref="double"/>值
        /// </summary>
        public double ReadDouble()
        {
            double value = BitConverter.ToDouble(this._buffer, (int)this.position);
            this.position += 8;
            return value;
        }

        /// <summary>
        /// 写入<see cref="double"/>值
        /// </summary>
        /// <param name="value"></param>
        public ByteBlock Write(double value)
        {
            this.Write(BitConverter.GetBytes(value));
            return this;
        }

        #endregion Double

        #region Float

        /// <summary>
        /// 从当前流位置读取一个<see cref="float"/>值
        /// </summary>
        public double ReadFloat()
        {
            float value = BitConverter.ToSingle(this._buffer, (int)this.position);
            this.position += 4;
            return value;
        }

        /// <summary>
        /// 写入<see cref="float"/>值
        /// </summary>
        /// <param name="value"></param>
        public ByteBlock Write(float value)
        {
            this.Write(BitConverter.GetBytes(value));
            return this;
        }

        #endregion Float

        #region UInt16

        /// <summary>
        /// 从当前流位置读取一个<see cref="ushort"/>值
        /// </summary>
        public ushort ReadUInt16()
        {
            ushort value = BitConverter.ToUInt16(this._buffer, (int)this.position);
            this.position += 2;
            return value;
        }

        /// <summary>
        /// 写入<see cref="ushort"/>值
        /// </summary>
        /// <param name="value"></param>
        public ByteBlock Write(ushort value)
        {
            this.Write(BitConverter.GetBytes(value));
            return this;
        }

        #endregion UInt16

        #region UInt32

        /// <summary>
        /// 从当前流位置读取一个<see cref="uint"/>值
        /// </summary>
        public uint ReadUInt32()
        {
            uint value = BitConverter.ToUInt32(this._buffer, (int)this.position);
            this.position += 4;
            return value;
        }

        /// <summary>
        /// 写入<see cref="uint"/>值
        /// </summary>
        /// <param name="value"></param>
        public ByteBlock Write(uint value)
        {
            this.Write(BitConverter.GetBytes(value));
            return this;
        }

        #endregion UInt32

        #region UInt64

        /// <summary>
        /// 从当前流位置读取一个<see cref="ulong"/>值
        /// </summary>
        public ulong ReadUInt64()
        {
            ulong value = BitConverter.ToUInt64(this._buffer, (int)this.position);
            this.position += 8;
            return value;
        }

        /// <summary>
        /// 写入<see cref="ulong"/>值
        /// </summary>
        /// <param name="value"></param>
        public ByteBlock Write(ulong value)
        {
            this.Write(BitConverter.GetBytes(value));
            return this;
        }

        #endregion UInt64

        #region DateTime

        /// <summary>
        /// 从当前流位置读取一个<see cref="DateTime"/>值
        /// </summary>
        public DateTime ReadDateTime()
        {
            long value = BitConverter.ToInt64(this._buffer, (int)this.position);
            this.position += 8;
            return DateTime.FromBinary(value);
        }

        /// <summary>
        /// 写入<see cref="DateTime"/>值
        /// </summary>
        /// <param name="value"></param>
        public ByteBlock Write(DateTime value)
        {
            this.Write(BitConverter.GetBytes(value.ToBinary()));
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
                        obj = SerializeConvert.RRQMBinaryDeserialize<T>(this._buffer, (int)this.position);
                    }
                    break;

                case SerializationType.Json:
                    {
                        string jsonString = Encoding.UTF8.GetString(this._buffer, (int)this.position, length);
                        obj = JsonConvert.DeserializeObject<T>(jsonString);
                    }
                    break;

                default:
                    throw new RRQMException("未定义的序列化类型");
            }

            this.position += length;
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
                        data = SerializeConvert.RRQMBinarySerialize(value, true);
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