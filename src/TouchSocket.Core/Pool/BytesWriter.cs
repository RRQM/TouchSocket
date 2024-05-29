////------------------------------------------------------------------------------
////  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
////  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
////  CSDN博客：https://blog.csdn.net/qq_40374647
////  哔哩哔哩视频：https://space.bilibili.com/94253567
////  Gitee源代码仓库：https://gitee.com/RRQM_Home
////  Github源代码仓库：https://github.com/RRQM
////  API首页：https://touchsocket.net/
////  交流QQ群：234762506
////  感谢您的下载和使用
////------------------------------------------------------------------------------

//using System;
//using System.Diagnostics;
//using System.IO;
//using System.Runtime.CompilerServices;
//using System.Text;

//namespace TouchSocket.Core
//{
//    /// <summary>
//    /// 字节块流
//    /// </summary>
//    [DebuggerDisplay("Len={Length},Pos={Position},Capacity={Capacity}")]
//    public struct IByteBlock
//    {
//        public IByteBlock(Memory<byte> memory, Func<Memory<byte>, int, Memory<byte>> expansionDelegate)
//        {
//            this.m_memory = memory;
//            this.m_expansionDelegate = expansionDelegate;
//        }

//        public IByteBlock(Memory<byte> memory) : this(memory, default)
//        {
//        }

//        #region 字段

//        private readonly Func<Memory<byte>, int, Memory<byte>> m_expansionDelegate;
//        private int m_length;
//        private Memory<byte> m_memory;
//        private int m_position;

//        #endregion 字段

//        #region 属性

//        /// <summary>
//        /// 容量
//        /// </summary>
//        public readonly int Capacity => this.m_memory.Length;

//        public readonly int Length => this.m_length;

//        /// <summary>
//        /// 流位置
//        /// </summary>
//        public int Position
//        {
//            readonly get => this.m_position;
//            set => this.m_position = value;
//        }

//        public readonly Memory<byte> Memory => this.m_memory;

//        /// <summary>
//        /// 空闲长度，准确掌握该值，可以避免内存扩展，计算为<see cref="Capacity"/>与<see cref="Pos"/>的差值。
//        /// </summary>
//        public readonly int FreeLength => this.m_memory.Length - this.Position;

//        #endregion 属性

//        /// <summary>
//        /// 清空所有内存数据
//        /// </summary>
//        /// <exception cref="ObjectDisposedException">内存块已释放</exception>
//        public readonly void Clear()
//        {
//            this.m_memory.Span.Clear();
//        }

//        #region Seek

//        /// <summary>
//        /// 设置流位置
//        /// </summary>
//        /// <param name="offset"></param>
//        /// <param name="origin"></param>
//        /// <returns></returns>
//        /// <exception cref="ObjectDisposedException"></exception>
//        public int Seek(int offset, SeekOrigin origin)
//        {
//            switch (origin)
//            {
//                case SeekOrigin.Begin:
//                    this.m_position = offset;
//                    break;

//                case SeekOrigin.Current:
//                    this.m_position += offset;
//                    break;

//                case SeekOrigin.End:
//                    this.m_position = this.m_memory.Length + offset;
//                    break;
//            }
//            return this.m_position;
//        }

//        /// <summary>
//        /// 移动游标
//        /// </summary>
//        /// <param name="position"></param>
//        /// <returns></returns>
//        public void Seek(int position)
//        {
//            this.m_position = position;
//        }

//        /// <summary>
//        /// 设置游标到末位
//        /// </summary>
//        /// <returns></returns>
//        public void SeekToEnd()
//        {
//            this.m_position = this.m_memory.Length;
//        }

//        /// <summary>
//        /// 设置游标到首位
//        /// </summary>
//        /// <returns></returns>
//        public void SeekToStart()
//        {
//            this.m_position = 0;
//        }

//        #endregion Seek

//        /// <summary>
//        /// 设置实际长度
//        /// </summary>
//        /// <param name="value"></param>
//        public void SetLength(int value)
//        {
//            if (value > this.m_memory.Length)
//            {
//                throw new Exception("设置值超出容量");
//            }
//            this.m_length = value;
//        }

//        #region ToArray

//        /// <summary>
//        /// 从指定位置转化到指定长度的有效内存。本操作不递增<see cref="Position"/>
//        /// </summary>
//        /// <param name="offset"></param>
//        /// <param name="length"></param>
//        /// <returns></returns>
//        public readonly byte[] ToArray(int offset, int length)
//        {
//            return this.m_memory.Slice(offset, length).ToArray();
//        }

//        /// <summary>
//        /// 转换为有效内存。本操作不递增<see cref="Position"/>
//        /// </summary>
//        /// <returns></returns>
//        public readonly byte[] ToArray()
//        {
//            return this.ToArray(0, this.Length);
//        }

//        /// <summary>
//        /// 从指定位置转为有效内存。本操作不递增<see cref="Position"/>
//        /// </summary>
//        /// <param name="offset"></param>
//        /// <returns></returns>
//        public readonly byte[] ToArray(int offset)
//        {
//            return this.ToArray(offset, this.Length - offset);
//        }

//        /// <summary>
//        /// 将当前<see cref="Position"/>至指定长度转化为有效内存。本操作不递增<see cref="Position"/>
//        /// </summary>
//        /// <param name="length"></param>
//        /// <returns></returns>
//        public readonly byte[] ToArrayTake(int length)
//        {
//            return this.ToArray(this.m_position, length);
//        }

//        /// <summary>
//        /// 将当前<see cref="Position"/>至有效长度转化为有效内存。本操作不递增<see cref="Position"/>
//        /// </summary>
//        /// <returns></returns>
//        public readonly byte[] ToArrayTake()
//        {
//            return this.ToArray(this.m_position, this.m_length - this.m_position);
//        }

//        #endregion ToArray

//        #region AsMemory

//        /// <summary>
//        /// 从指定位置转化到指定长度的有效内存。本操作不递增<see cref="Position"/>
//        /// </summary>
//        /// <param name="offset"></param>
//        /// <param name="length"></param>
//        /// <returns></returns>
//        public readonly Memory<byte> AsMemory(int offset, int length)
//        {
//            return this.m_memory.Slice(offset, length);
//        }

//        /// <summary>
//        /// 转换为有效内存。本操作不递增<see cref="Position"/>
//        /// </summary>
//        /// <returns></returns>
//        public readonly Memory<byte> AsMemory()
//        {
//            return this.AsMemory(0, this.Length);
//        }

//        /// <summary>
//        /// 从指定位置转为有效内存。本操作不递增<see cref="Position"/>
//        /// </summary>
//        /// <param name="offset"></param>
//        /// <returns></returns>
//        public readonly Memory<byte> AsMemory(int offset)
//        {
//            return this.AsMemory(offset, this.Length - offset);
//        }

//        /// <summary>
//        /// 将当前<see cref="Position"/>至指定长度转化为有效内存。本操作不递增<see cref="Position"/>
//        /// </summary>
//        /// <param name="length"></param>
//        /// <returns></returns>
//        public readonly Memory<byte> AsMemoryTake(int length)
//        {
//            return this.AsMemory(this.m_position, length);
//        }

//        /// <summary>
//        /// 将当前<see cref="Position"/>至有效长度转化为有效内存。本操作不递增<see cref="Position"/>
//        /// </summary>
//        /// <returns></returns>
//        public readonly Memory<byte> AsMemoryTake()
//        {
//            return this.AsMemory(this.m_position, this.m_length - this.m_position);
//        }

//        #endregion AsMemory

//        #region ToMemory

//        /// <summary>
//        /// 从指定位置转化到指定长度的有效内存。本操作不递增<see cref="Position"/>
//        /// </summary>
//        /// <param name="offset"></param>
//        /// <param name="length"></param>
//        /// <returns></returns>
//        public readonly Memory<byte> ToMemory(int offset, int length)
//        {
//            return this.m_memory.Slice(offset, length).ToArray();
//        }

//        /// <summary>
//        /// 转换为有效内存。本操作不递增<see cref="Position"/>
//        /// </summary>
//        /// <returns></returns>
//        public readonly Memory<byte> ToMemory()
//        {
//            return this.ToMemory(0, this.Length);
//        }

//        /// <summary>
//        /// 从指定位置转为有效内存。本操作不递增<see cref="Position"/>
//        /// </summary>
//        /// <param name="offset"></param>
//        /// <returns></returns>
//        public readonly Memory<byte> ToMemory(int offset)
//        {
//            return this.ToMemory(offset, this.Length - offset);
//        }

//        /// <summary>
//        /// 将当前<see cref="Position"/>至指定长度转化为有效内存。本操作不递增<see cref="Position"/>
//        /// </summary>
//        /// <param name="length"></param>
//        /// <returns></returns>
//        public readonly Memory<byte> ToMemoryTake(int length)
//        {
//            return this.ToMemory(this.m_position, length);
//        }

//        /// <summary>
//        /// 将当前<see cref="Position"/>至有效长度转化为有效内存。本操作不递增<see cref="Position"/>
//        /// </summary>
//        /// <returns></returns>
//        public readonly Memory<byte> ToMemoryTake()
//        {
//            return this.ToMemory(this.m_position, this.m_length - this.m_position);
//        }

//        #endregion ToMemory

//        #region Write
//        /// <summary>
//        /// 写入
//        /// </summary>
//        /// <exception cref="ObjectDisposedException"></exception>
//        public void Write(Span<byte> span) 
//        {
//            if (span.IsEmpty)
//            {
//                return;
//            }

//            this.WriteSize(span.Length);

//            Unsafe.CopyBlock(ref this.m_memory.Span[this.m_position], ref span[0], (uint)span.Length);
//            this.m_position += span.Length;
//            this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
//        }

//        [MethodImpl(MethodImplOptions.AggressiveInlining)]
//        private void WriteSize(int size)
//        {
//            if (this.m_memory.Length - this.m_position < size)
//            {
//                var need = this.m_memory.Length + size - (this.m_memory.Length - this.m_position);
//                long lend = this.m_memory.Length;
//                while (need > lend)
//                {
//                    lend *= 2;
//                }

//                if (lend > int.MaxValue)
//                {
//                    lend = Math.Min(need + 1024 * 1024 * 100, int.MaxValue);
//                }
//                if (this.m_expansionDelegate == null)
//                {
//                    ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), size, this.Capacity - this.m_position);
//                }
//                this.m_memory = this.m_expansionDelegate.Invoke(this.m_memory, (int)lend);
//            }
//        }
//        #endregion

//        #region Object

//        /// <summary>
//        /// 写入<see cref="object"/>值
//        /// </summary>
//        /// <param name="value"></param>
//        /// <param name="serializationType"></param>
//        public void WriteObject(object value, SerializationType serializationType = SerializationType.FastBinary)
//        {
//            if (value == null)
//            {
//                this.Write(0);
//            }
//            else
//            {
//                byte[] data;
//                switch (serializationType)
//                {
//                    case SerializationType.FastBinary:
//                        {
//                            data = FastBinaryFormatter.Serialize(value);
//                        }
//                        break;

//                    case SerializationType.Json:
//                        {
//                            data = SerializeConvert.JsonSerializeToBytes(value);
//                        }
//                        break;

//                    case SerializationType.Xml:
//                        {
//                            data = Encoding.UTF8.GetBytes(SerializeConvert.XmlSerializeToString(value));
//                        }
//                        break;

//                    case SerializationType.SystemBinary:
//                        {
//                            data = SerializeConvert.BinarySerialize(value);
//                        }
//                        break;

//                    default:
//                        throw new Exception("未定义的序列化类型");
//                }
//                this.Write(data.Length);
//                this.Write(data);
//            }
//        }

//        #endregion Object

//        #region ByteBlock
//        /// <summary>
//        /// 写入<see cref="ByteBlock"/>值
//        /// </summary>
//        public void WriteByteBlock(ByteBlock byteBlock)
//        {
//            var len = byteBlock is null ? -1 : byteBlock.Length;
//            this.Write(len);
//            this.Write(byteBlock.Memory.Span);
//        }

//        #endregion ByteBlock

//        #region Package
//        /// <summary>
//        /// 以包进行写入。允许null值。
//        /// 读取时调用<see cref="IByteBlock.ReadPackage{TPackage}"/>，解包。或者先判断<see cref="IByteBlock.ReadIsNull"/>，然后自行解包。
//        /// </summary>
//        /// <typeparam name="TPackage"></typeparam>
//        /// <param name="package"></param>
//        /// <returns></returns>
//        public void WritePackage<TPackage>(TPackage package) where TPackage : class, IPackage
//        {
//            this.WriteIsNull(package);
//            package?.Package(ref this);
//        }

//        #endregion Package

//        #region Null
//        /// <summary>
//        /// 判断该值是否为Null，然后写入标识值
//        /// </summary>
//        public void WriteIsNull<T>(T t) where T : class
//        {
//            if (t == null)
//            {
//                this.WriteNull();
//            }
//            else
//            {
//                this.WriteNotNull();
//            }
//        }

//        /// <summary>
//        /// 判断该值是否为Null，然后写入标识值
//        /// </summary>
//        /// <typeparam name="T"></typeparam>
//        /// <param name="t"></param>
//        /// <returns></returns>
//        public void WriteIsNull<T>(T? t) where T : struct
//        {
//            if (t.HasValue)
//            {
//                this.WriteNotNull();
//            }
//            else
//            {
//                this.WriteNull();
//            }
//        }

//        /// <summary>
//        /// 写入一个标识非Null值
//        /// </summary>
//        public void WriteNotNull()
//        {
//            this.Write((byte)1);
//        }

//        /// <summary>
//        /// 写入一个标识Null值
//        /// </summary>
//        public void WriteNull()
//        {
//            this.Write((byte)0);
//        }

//        #endregion Null

//        #region BytesPackage

//        /// <summary>
//        /// 写入一个独立的<see cref="byte"/>数组包，值可以为null。
//        /// </summary>
//        /// <param name="value"></param>
//        /// <param name="offset"></param>
//        /// <param name="length"></param>
//        public void WriteBytesPackage(byte[] value, int offset, int length)
//        {
//            if (value == null)
//            {
//                this.Write((int)-1);
//            }
//            else if (length == 0)
//            {
//                this.Write((int)0);
//            }
//            else
//            {
//                this.Write(length);
//                this.Write(new Span<byte>(value, offset, length));
//            }
//        }

//        /// <summary>
//        /// 写入一个独立的<see cref="byte"/>数组包。值可以为null。
//        /// </summary>
//        /// <param name="value"></param>
//        public void WriteBytesPackage(byte[] value)
//        {
//            if (value == null)
//            {
//                this.Write((int)-1);
//            }
//            else if (value.Length == 0)
//            {
//                this.Write((int)0);
//            }
//            else
//            {
//                this.Write(value.Length);
//                this.Write(new Span<byte>(value, 0, value.Length));
//            }
//        }

//        #endregion BytesPackage

//        #region String

//        /// <summary>
//        /// 写入<see cref="string"/>值。值可以为null，或者空。
//        /// <para>注意：该操作不具备通用性，读取时必须使用ReadString。或者得先做出判断，由默认端序的int32值标识，具体如下：</para>
//        /// <list type="bullet">
//        /// <item>小于0，表示字符串为null</item>
//        /// <item>等于0，表示字符串为""</item>
//        /// <item>大于0，表示字符串在utf-8编码下的字节长度。</item>
//        /// </list>
//        /// </summary>
//        /// <param name="value"></param>
//        public void Write(string value)
//        {
//            if (value == null)
//            {
//                this.Write(-1);
//            }
//            else
//            {
//                var buffer = Encoding.UTF8.GetBytes(value);
//                this.Write(buffer.Length);
//                this.Write(buffer);
//            }
//        }

//        /// <summary>
//        /// 写入<see cref="string"/>值。值必须为有效值。可通用解析。
//        /// </summary>
//        /// <param name="value"></param>
//        /// <param name="encoding"></param>
//        public void WriteString(string value, Encoding encoding = null)
//        {
//            this.Write((encoding ?? Encoding.UTF8).GetBytes(value));
//        }

//        #endregion String

//        #region Int32

//        /// <summary>
//        /// 写入默认端序的<see cref="int"/>值
//        /// </summary>
//        /// <param name="value"></param>
//        public void Write(int value)
//        {
//            var size = 4;
//            this.WriteSize(size);
//            TouchSocketBitConverter.Default.GetBytes(ref this.m_memory.Span[this.m_position], value);
//            this.m_position += size;
//            this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
//        }

//        /// <summary>
//        /// 写入指定端序的<see cref="int"/>值
//        /// </summary>
//        /// <param name="value"></param>
//        /// <param name="endianType">指定端序</param>
//        public void Write(int value, EndianType endianType)
//        {
//            var size = 4;
//            this.WriteSize(size);
//            TouchSocketBitConverter.GetBitConverter(endianType).GetBytes(ref this.m_memory.Span[this.m_position], value);
//            this.m_position += size;
//            this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
//        }

//        #endregion Int32

//        #region Int16

//        /// <summary>
//        /// 写入默认端序的<see cref="short"/>值
//        /// </summary>
//        /// <param name="value"></param>
//        public void Write(short value)
//        {
//            var size = 2;
//            this.WriteSize(size);
//            TouchSocketBitConverter.Default.GetBytes(ref this.m_memory.Span[this.m_position], value);
//            this.m_position += size;
//            this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
//        }

//        /// <summary>
//        /// 写入<see cref="short"/>值
//        /// </summary>
//        /// <param name="value"></param>
//        /// <param name="endianType">指定端序</param>
//        public void Write(short value, EndianType endianType)
//        {
//            var size = 2;
//            this.WriteSize(size);
//            TouchSocketBitConverter.GetBitConverter(endianType).GetBytes(ref this.m_memory.Span[this.m_position], value);
//            this.m_position += size;
//            this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
//        }

//        #endregion Int16

//        #region Int64

//        /// <summary>
//        /// 写入默认端序的<see cref="long"/>值
//        /// </summary>
//        /// <param name="value"></param>
//        public void Write(long value)
//        {
//            var size = 8;
//            this.WriteSize(size);
//            TouchSocketBitConverter.Default.GetBytes(ref this.m_memory.Span[this.m_position], value);
//            this.m_position += size;
//            this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
//        }

//        /// <summary>
//        /// 写入<see cref="long"/>值
//        /// </summary>
//        /// <param name="value"></param>
//        /// <param name="endianType">指定端序</param>
//        public void Write(long value, EndianType endianType)
//        {
//            var size = 8;
//            this.WriteSize(size);
//            TouchSocketBitConverter.GetBitConverter(endianType).GetBytes(ref this.m_memory.Span[this.m_position], value);
//            this.m_position += size;
//            this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
//        }

//        #endregion Int64

//        #region Boolean

//        /// <summary>
//        /// 写入<see cref="bool"/>值
//        /// </summary>
//        /// <param name="value"></param>
//        public void Write(bool value)
//        {
//            var size = 1;
//            this.WriteSize(size);
//            TouchSocketBitConverter.Default.GetBytes(ref this.m_memory.Span[this.m_position], value);
//            this.m_position += size;
//            this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
//        }

//        /// <summary>
//        /// 写入bool数组。
//        /// </summary>
//        /// <param name="values"></param>
//        public void Write(bool[] values)
//        {
//            int size;
//            if (values.Length % 8 == 0)
//            {
//                size = values.Length / 8;
//            }
//            else
//            {
//                size = values.Length / 8 + 1;
//            }

//            this.WriteSize(size);
//            TouchSocketBitConverter.Default.GetBytes(ref this.m_memory.Span[this.m_position], values);
//            this.m_position += size;
//            this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
//        }

//        #endregion Boolean

//        #region Byte

//        /// <summary>
//        /// 写入<see cref="byte"/>值
//        /// </summary>
//        /// <param name="value"></param>
//        /// <returns></returns>
//        public void Write(byte value)
//        {
//            var size = 1;
//            this.WriteSize(size);
//            this.m_memory.Span[this.m_position] = value;
//            this.m_position += size;
//            this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
//        }

//        #endregion Byte

//        #region Char

//        /// <summary>
//        /// 写入默认端序的<see cref="char"/>值
//        /// </summary>
//        /// <param name="value"></param>
//        public void Write(char value)
//        {
//            var size = 2;
//            this.WriteSize(size);
//            TouchSocketBitConverter.Default.GetBytes(ref this.m_memory.Span[this.m_position], value);
//            this.m_position += size;
//            this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
//        }

//        /// <summary>
//        /// 写入<see cref="char"/>值
//        /// </summary>
//        /// <param name="value"></param>
//        /// <param name="endianType">指定端序</param>
//        public void Write(char value, EndianType endianType)
//        {
//            var size = 2;
//            this.WriteSize(size);
//            TouchSocketBitConverter.GetBitConverter(endianType).GetBytes(ref this.m_memory.Span[this.m_position], value);
//            this.m_position += size;
//            this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
//        }

//        #endregion Char

//        #region Double

//        /// <summary>
//        /// 写入默认端序的<see cref="double"/>值
//        /// </summary>
//        /// <param name="value"></param>
//        public void Write(double value)
//        {
//            var size = 8;
//            this.WriteSize(size);
//            TouchSocketBitConverter.Default.GetBytes(ref this.m_memory.Span[this.m_position], value);
//            this.m_position += size;
//            this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
//        }

//        /// <summary>
//        /// 写入<see cref="double"/>值
//        /// </summary>
//        /// <param name="value"></param>
//        /// <param name="endianType">指定端序</param>
//        public void Write(double value, EndianType endianType)
//        {
//            var size = 8;
//            this.WriteSize(size);
//            TouchSocketBitConverter.GetBitConverter(endianType).GetBytes(ref this.m_memory.Span[this.m_position], value);
//            this.m_position += size;
//            this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
//        }

//        #endregion Double

//        #region Float

//        /// <summary>
//        /// 写入默认端序的<see cref="float"/>值
//        /// </summary>
//        /// <param name="value"></param>
//        public void Write(float value)
//        {
//            var size = 4;
//            this.WriteSize(size);
//            TouchSocketBitConverter.Default.GetBytes(ref this.m_memory.Span[this.m_position], value);
//            this.m_position += size;
//            this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
//        }

//        /// <summary>
//        /// 写入<see cref="float"/>值
//        /// </summary>
//        /// <param name="value"></param>
//        /// <param name="endianType">指定端序</param>
//        public void Write(float value, EndianType endianType)
//        {
//            var size = 4;
//            this.WriteSize(size);
//            TouchSocketBitConverter.GetBitConverter(endianType).GetBytes(ref this.m_memory.Span[this.m_position], value);
//            this.m_position += size;
//            this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
//        }

//        #endregion Float

//        #region UInt16

//        /// <summary>
//        /// 写入默认端序的<see cref="ushort"/>值
//        /// </summary>
//        /// <param name="value"></param>
//        public void Write(ushort value)
//        {
//            var size = 2;
//            this.WriteSize(size);
//            TouchSocketBitConverter.Default.GetBytes(ref this.m_memory.Span[this.m_position], value);
//            this.m_position += size;
//            this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
//        }

//        /// <summary>
//        /// 写入<see cref="ushort"/>值
//        /// </summary>
//        /// <param name="value"></param>
//        /// <param name="endianType">指定端序</param>
//        public void Write(ushort value, EndianType endianType)
//        {
//            var size = 2;
//            this.WriteSize(size);
//            TouchSocketBitConverter.GetBitConverter(endianType).GetBytes(ref this.m_memory.Span[this.m_position], value);
//            this.m_position += size;
//            this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
//        }

//        #endregion UInt16

//        #region UInt32

//        /// <summary>
//        /// 写入默认端序的<see cref="uint"/>值
//        /// </summary>
//        /// <param name="value"></param>
//        public void Write(uint value)
//        {
//            var size = 4;
//            this.WriteSize(size);
//            TouchSocketBitConverter.Default.GetBytes(ref this.m_memory.Span[this.m_position], value);
//            this.m_position += size;
//            this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
//        }

//        /// <summary>
//        /// 写入<see cref="uint"/>值
//        /// </summary>
//        /// <param name="value"></param>
//        /// <param name="endianType">指定端序</param>
//        public void Write(uint value, EndianType endianType)
//        {
//            var size = 4;
//            this.WriteSize(size);
//            TouchSocketBitConverter.GetBitConverter(endianType).GetBytes(ref this.m_memory.Span[this.m_position], value);
//            this.m_position += size;
//            this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
//        }

//        #endregion UInt32

//        #region UInt64

//        /// <summary>
//        /// 写入默认端序的<see cref="ulong"/>值
//        /// </summary>
//        /// <param name="value"></param>
//        public void Write(ulong value)
//        {
//            var size = 8;
//            this.WriteSize(size);
//            TouchSocketBitConverter.Default.GetBytes(ref this.m_memory.Span[this.m_position], value);
//            this.m_position += size;
//            this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
//        }

//        /// <summary>
//        /// 写入<see cref="ulong"/>值
//        /// </summary>
//        /// <param name="value"></param>
//        /// <param name="endianType">指定端序</param>
//        public void Write(ulong value, EndianType endianType)
//        {
//            var size = 8;
//            this.WriteSize(size);
//            TouchSocketBitConverter.GetBitConverter(endianType).GetBytes(ref this.m_memory.Span[this.m_position], value);
//            this.m_position += size;
//            this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
//        }

//        #endregion UInt64

//        #region Decimal

//        /// <summary>
//        /// 写入默认端序的<see cref="decimal"/>值
//        /// </summary>
//        /// <param name="value"></param>
//        public void Write(decimal value)
//        {
//            var size = 16;
//            this.WriteSize(size);
//            TouchSocketBitConverter.Default.GetBytes(ref this.m_memory.Span[this.m_position], value);
//            this.m_position += size;
//            this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
//        }

//        /// <summary>
//        /// 写入<see cref="decimal"/>值
//        /// </summary>
//        /// <param name="value"></param>
//        /// <param name="endianType">指定端序</param>
//        public void Write(decimal value, EndianType endianType)
//        {
//            var size = 16;
//            this.WriteSize(size);
//            TouchSocketBitConverter.GetBitConverter(endianType).GetBytes(ref this.m_memory.Span[this.m_position], value);
//            this.m_position += size;
//            this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
//        }

//        #endregion Decimal

//        #region DateTime

//        /// <summary>
//        /// 写入<see cref="DateTime"/>值
//        /// </summary>
//        /// <param name="value"></param>
//        public void Write(DateTime value)
//        {
//            var size = 8;
//            this.WriteSize(size);
//            TouchSocketBitConverter.BigEndian.GetBytes(ref this.m_memory.Span[this.m_position], value.ToBinary());
//            this.m_position += size;
//            this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
//        }

//        #endregion DateTime

//        #region TimeSpan

//        /// <summary>
//        /// 写入<see cref="TimeSpan"/>值
//        /// </summary>
//        /// <param name="value"></param>
//        public void Write(TimeSpan value)
//        {
//            var size = 8;
//            this.WriteSize(size);
//            TouchSocketBitConverter.BigEndian.GetBytes(ref this.m_memory.Span[this.m_position], value.Ticks);
//            this.m_position += size;
//            this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
//        }

//        #endregion TimeSpan
//    }
//}