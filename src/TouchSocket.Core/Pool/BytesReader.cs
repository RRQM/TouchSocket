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

using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace TouchSocket.Core
{
    /// <summary>
    /// 字节块流
    /// </summary>
    [DebuggerDisplay("Len={Length},Pos={Position},Capacity={Capacity}")]
    public ref struct BytesReader
    {
        private readonly int m_length;
        private readonly ReadOnlySpan<byte> m_span;
        private int m_position;

        public BytesReader(ReadOnlySpan<byte> span)
        {
            this.m_span = span;
        }

        /// <summary>
        /// 还能读取的长度，计算为<see cref="Len"/>与<see cref="Pos"/>的差值。
        /// </summary>
        public readonly int CanReadLength => this.m_span.Length - this.Position;

        public readonly int Length => this.m_span.Length;

        /// <summary>
        /// 流位置
        /// </summary>
        public int Position
        {
            readonly get => this.m_position;
            set => this.m_position = value;
        }

        public readonly ReadOnlySpan<byte> Span => this.m_span;

        #region Read

        /// <summary>
        /// 读取数据，然后递增Pos
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public unsafe int Read(byte[] buffer, int offset, int length)
        {
            var len = this.m_length - this.m_position > length ? length : this.CanReadLength;

            fixed (byte* p1 = &this.m_span[this.m_position])
            {
                fixed (byte* p2 = &buffer[offset])
                {
                    Unsafe.CopyBlock(p2, p1, (uint)len);
                }
            }
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
        /// 从当前位置读取指定长度的数组。并递增<see cref="Position"/>
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public byte[] ReadToArray(int length)
        {
            var bytes = new byte[length];
            var r = this.Read(bytes, 0, bytes.Length);
            if (r != bytes.Length)
            {
                throw new ArgumentOutOfRangeException();
            }

            return bytes;
        }

        #endregion Read

        #region ToArray

        /// <summary>
        /// 从指定位置转化到指定长度的有效内存。本操作不递增<see cref="Position"/>
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public readonly byte[] ToArray(int offset, int length)
        {
            return this.m_span.Slice(offset, length).ToArray();
        }

        /// <summary>
        /// 转换为有效内存。本操作不递增<see cref="Position"/>
        /// </summary>
        /// <returns></returns>
        public readonly byte[] ToArray()
        {
            return this.ToArray(0, this.Length);
        }

        /// <summary>
        /// 从指定位置转为有效内存。本操作不递增<see cref="Position"/>
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public readonly byte[] ToArray(int offset)
        {
            return this.ToArray(offset, this.Length - offset);
        }

        /// <summary>
        /// 将当前<see cref="Position"/>至指定长度转化为有效内存。本操作不递增<see cref="Position"/>
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public readonly byte[] ToArrayTake(int length)
        {
            return this.ToArray(this.m_position, length);
        }

        /// <summary>
        /// 将当前<see cref="Position"/>至有效长度转化为有效内存。本操作不递增<see cref="Position"/>
        /// </summary>
        /// <returns></returns>
        public readonly byte[] ToArrayTake()
        {
            return this.ToArray(this.m_position, this.m_length - this.m_position);
        }

        #endregion ToArray

        #region VarUInt32
        public uint ReadVarUInt32()
        {
            uint value = 0;
            var bytelength = 0;
            while (true)
            {
                var b = this.m_span[this.m_position++];
                var temp = (b & 0x7F); //取每个字节的后7位
                temp <<= (7 * bytelength); //向左移位，越是后面的字节，移位越多
                value += (uint)temp; //把每个字节的值加起来就是最终的值了
                bytelength++;
                if (b <= 0x7F)
                { //127=0x7F=0b01111111，小于等于说明msb=0，即最后一个字节
                    break;
                }
            }
            return value;
        }
        #endregion

        #region ByteBlock

        /// <summary>
        /// 从当前流位置读取一个<see cref="ByteBlock"/>值。
        /// <para>
        /// 注意，使用该方式读取到的内存块，会脱离释放周期，所以最好在使用完成后自行释放。
        /// </para>
        /// </summary>
        public ByteBlock ReadByteBlock()
        {
            var len = (int)this.ReadVarUInt32() - 1;

            if (len < 0)
            {
                return default;
            }

            var byteBlock = new ByteBlock(len);
            byteBlock.Write(this.m_span.Slice(this.m_position, len));
            byteBlock.SeekToStart();
            this.m_position += len;
            return byteBlock;
        }

        #endregion ByteBlock

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
            this.Position = position;
        }

        /// <summary>
        /// 设置游标到末位
        /// </summary>
        /// <returns></returns>
        public void SeekToEnd()
        {
            this.Position = this.m_length;
        }

        /// <summary>
        /// 设置游标到首位
        /// </summary>
        /// <returns></returns>
        public void SeekToStart()
        {
            this.Position = 0;
        }

        #endregion Seek

        #region BytesPackage

        /// <summary>
        /// 从当前流位置读取一个独立的<see cref="byte"/>数组包
        /// </summary>
        public byte[] ReadBytesPackage()
        {
            var memory = ReadBytesPackageMemory();
            if (memory == null)
            {
                return memory.ToArray();
            }

            return null;
        }

        public ReadOnlySpan<byte> ReadBytesPackageMemory()
        {
            var length = this.ReadInt32();
            if (length < 0)
            {
                return null;
            }

            var memory = this.m_span.Slice(this.m_position, length);
            this.m_position += length;
            return memory;
        }

        #endregion BytesPackage

        #region Byte

        /// <summary>
        /// 从当前流位置读取一个<see cref="byte"/>值
        /// </summary>
        public byte ReadByte()
        {
            var size = 1;
            if (this.CanReadLength < size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
            }

            var value = this.m_span[this.m_position];
            this.m_position += size;
            return value;
        }

        #endregion Byte

        #region String

        /// <summary>
        /// 从当前流位置读取一个<see cref="string"/>值
        /// </summary>
        public string ReadString(FixedHeaderType headerType = FixedHeaderType.Int)
        {
            int len;
            switch (headerType)
            {
                case FixedHeaderType.Byte:
                    len = this.ReadByte();
                    if (len == byte.MaxValue)
                    {
                        return null;
                    }
                    break;

                case FixedHeaderType.Ushort:
                    len = this.ReadUInt16();
                    if (len == ushort.MaxValue)
                    {
                        return null;
                    }
                    break;

                case FixedHeaderType.Int:
                default:
                    len = this.ReadInt32();
                    if (len == int.MaxValue)
                    {
                        return null;
                    }
                    break;
            }

            var str = this.m_span.Slice(this.m_position, len).ToString(Encoding.UTF8);
            this.m_position += len;
            return str;
        }

        #endregion String

        #region Int32

        public int ReadInt32()
        {
            var size = 4;
            if (this.CanReadLength < size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
            }
            var value = TouchSocketBitConverter.Default.To<int>(this.m_span.Slice(this.m_position));
            this.m_position += size;
            return value;
        }

        /// <summary>
        /// 从当前流位置读取一个指定端序的<see cref="int"/>值
        /// </summary>
        /// <param name="endianType"></param>
        public int ReadInt32(EndianType endianType)
        {
            var size = 4;
            if (this.CanReadLength < size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
            }
            var value = TouchSocketBitConverter.GetBitConverter(endianType).To<int>(this.m_span.Slice(this.m_position));
            this.m_position += size;
            return value;
        }

        /// <summary>
        /// 将当前有效内存转为默认端序的<see cref="int"/>集合。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<int> ToInt32s()
        {
            this.m_position = 0;
            var list = new List<int>();
            var size = 4;
            while (true)
            {
                if (this.m_position + size > this.m_length)
                {
                    break;
                }
                list.Add(this.ReadInt32());
            }
            return list;
        }

        /// <summary>
        /// 将当前有效内存转为指定端序的<see cref="int"/>集合。
        /// </summary>
        /// <param name="endianType"></param>
        /// <returns></returns>
        public IEnumerable<int> ToInt32s(EndianType endianType)
        {
            this.m_position = 0;
            var list = new List<int>();
            var size = 4;
            while (true)
            {
                if (this.m_position + size > this.m_length)
                {
                    break;
                }
                list.Add(this.ReadInt32(endianType));
            }
            return list;
        }

        #endregion Int32

        #region Int16

        /// <summary>
        /// 从当前流位置读取一个默认端序的<see cref="short"/>值
        /// </summary>
        public short ReadInt16()
        {
            var size = 2;
            if (this.CanReadLength < size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
            }
            var value = TouchSocketBitConverter.Default.To<short>(this.m_span.Slice(this.m_position));
            this.m_position += size;
            return value;
        }

        /// <summary>
        /// 从当前流位置读取一个<see cref="short"/>值
        /// </summary>
        /// <param name="endianType">指定端序</param>
        public short ReadInt16(EndianType endianType)
        {
            var size = 2;
            if (this.CanReadLength < size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
            }
            var value = TouchSocketBitConverter.GetBitConverter(endianType).To<short>(this.m_span.Slice(this.m_position));
            this.m_position += size;
            return value;
        }

        /// <summary>
        /// 将当前有效内存转为默认端序的<see cref="short"/>集合。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<short> ToInt16s()
        {
            this.m_position = 0;
            var list = new List<short>();
            var size = 2;
            while (true)
            {
                if (this.m_position + size > this.m_length)
                {
                    break;
                }
                list.Add(this.ReadInt16());
            }
            return list;
        }

        /// <summary>
        /// 将当前有效内存转为指定端序的<see cref="short"/>集合。
        /// </summary>
        /// <param name="endianType"></param>
        /// <returns></returns>
        public IEnumerable<short> ToInt16s(EndianType endianType)
        {
            this.m_position = 0;
            var list = new List<short>();
            var size = 2;
            while (true)
            {
                if (this.m_position + size > this.m_length)
                {
                    break;
                }
                list.Add(this.ReadInt16(endianType));
            }
            return list;
        }

        #endregion Int16

        #region Int64

        /// <summary>
        /// 从当前流位置读取一个默认端序的<see cref="long"/>值
        /// </summary>
        public long ReadInt64()
        {
            var size = 8;
            if (this.CanReadLength < size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
            }
            var value = TouchSocketBitConverter.Default.To<long>(this.m_span.Slice(this.m_position));
            this.m_position += size;
            return value;
        }

        /// <summary>
        /// 从当前流位置读取一个<see cref="long"/>值
        /// </summary>
        /// <param name="endianType">指定端序</param>
        public long ReadInt64(EndianType endianType)
        {
            var size = 8;
            if (this.CanReadLength < size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
            }
            var value = TouchSocketBitConverter.GetBitConverter(endianType).To<long>(this.m_span.Slice(this.m_position));
            this.m_position += size;
            return value;
        }

        /// <summary>
        /// 将当前有效内存转为默认端序的<see cref="long"/>集合。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<long> ToInt64s()
        {
            this.m_position = 0;
            var list = new List<long>();
            var size = 8;
            while (true)
            {
                if (this.m_position + size > this.m_length)
                {
                    break;
                }
                list.Add(this.ReadInt64());
            }
            return list;
        }

        /// <summary>
        /// 将当前有效内存转为指定端序的<see cref="long"/>集合。
        /// </summary>
        /// <param name="endianType"></param>
        /// <returns></returns>
        public IEnumerable<long> ToInt64s(EndianType endianType)
        {
            this.m_position = 0;
            var list = new List<long>();
            var size = 8;
            while (true)
            {
                if (this.m_position + size > this.m_length)
                {
                    break;
                }
                list.Add(this.ReadInt64(endianType));
            }
            return list;
        }

        #endregion Int64

        #region Boolean

        /// <summary>
        /// 从当前流位置读取1个<see cref="bool"/>值
        /// </summary>
        public bool ReadBoolean()
        {
            var size = 1;
            if (this.CanReadLength < size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
            }
            var value = TouchSocketBitConverter.Default.To<bool>(this.m_span.Slice(this.m_position));
            this.m_position += size;
            return value;
        }

        /// <summary>
        /// 从当前流位置读取1个字节，按位解析为bool值数组。
        /// </summary>
        /// <returns></returns>
        public bool[] ReadBooleans()
        {
            var size = 1;
            if (this.CanReadLength < size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
            }
            var value = TouchSocketBitConverter.Default.ToBooleans(this.m_span.Slice(this.m_position), 1);
            this.m_position += size;
            return value;
        }

        /// <summary>
        /// 将当前有效内存按位转为<see cref="bool"/>集合。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<bool> ToBoolensFromBit()
        {
            this.m_position = 0;
            var list = new List<bool>();
            var size = 1;
            while (true)
            {
                if (this.m_position + size > this.m_length)
                {
                    break;
                }
                list.AddRange(this.ReadBooleans());
            }
            return list;
        }

        /// <summary>
        /// 将当前有效内存按字节转为<see cref="bool"/>集合。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<bool> ToBoolensFromByte()
        {
            this.m_position = 0;
            var list = new List<bool>();
            var size = 1;
            while (true)
            {
                if (this.m_position + size > this.m_length)
                {
                    break;
                }
                list.Add(this.ReadBoolean());
            }
            return list;
        }

        #endregion Boolean

        #region Char

        /// <summary>
        /// 从当前流位置读取一个默认端序的<see cref="char"/>值
        /// </summary>
        public char ReadChar()
        {
            var size = 2;
            if (this.CanReadLength < size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
            }
            var value = TouchSocketBitConverter.Default.To<char>(this.m_span.Slice(this.m_position));
            this.m_position += size;
            return value;
        }

        /// <summary>
        /// 从当前流位置读取一个<see cref="char"/>值
        /// </summary>
        /// <param name="endianType">指定端序</param>
        public char ReadChar(EndianType endianType)
        {
            var size = 1;
            if (this.CanReadLength < size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
            }
            var value = TouchSocketBitConverter.GetBitConverter(endianType).To<char>(this.m_span.Slice(this.m_position));
            this.m_position += size;
            return value;
        }

        /// <summary>
        /// 将当前有效内存转为默认端序的<see cref="char"/>集合。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<char> ToChars()
        {
            this.m_position = 0;
            var list = new List<char>();
            var size = 2;
            while (true)
            {
                if (this.m_position + size > this.m_length)
                {
                    break;
                }
                list.Add(this.ReadChar());
            }
            return list;
        }

        /// <summary>
        /// 将当前有效内存转为指定端序的<see cref="char"/>集合。
        /// </summary>
        /// <param name="endianType"></param>
        /// <returns></returns>
        public IEnumerable<char> ToChars(EndianType endianType)
        {
            this.m_position = 0;
            var list = new List<char>();
            var size = 2;
            while (true)
            {
                if (this.m_position + size > this.m_length)
                {
                    break;
                }
                list.Add(this.ReadChar(endianType));
            }
            return list;
        }

        #endregion Char

        #region Double

        /// <summary>
        /// 从当前流位置读取一个默认端序的<see cref="double"/>值
        /// </summary>
        public double ReadDouble()
        {
            var size = 8;
            if (this.CanReadLength < size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
            }
            var value = TouchSocketBitConverter.Default.To<double>(this.m_span.Slice(this.m_position));
            this.m_position += size;
            return value;
        }

        /// <summary>
        /// 从当前流位置读取一个<see cref="double"/>值
        /// </summary>
        /// <param name="endianType">指定端序</param>
        public double ReadDouble(EndianType endianType)
        {
            var size = 8;
            if (this.CanReadLength < size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
            }
            var value = TouchSocketBitConverter.GetBitConverter(endianType).To<double>(this.m_span.Slice(this.m_position));
            this.m_position += size;
            return value;
        }

        /// <summary>
        /// 将当前有效内存转为默认端序的<see cref="double"/>集合。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<double> ToDoubles()
        {
            this.m_position = 0;
            var list = new List<double>();
            var size = 8;
            while (true)
            {
                if (this.m_position + size > this.m_length)
                {
                    break;
                }
                list.Add(this.ReadDouble());
            }
            return list;
        }

        /// <summary>
        /// 将当前有效内存转为指定端序的<see cref="double"/>集合。
        /// </summary>
        /// <param name="endianType"></param>
        /// <returns></returns>
        public IEnumerable<double> ToDoubles(EndianType endianType)
        {
            this.m_position = 0;
            var list = new List<double>();
            var size = 8;
            while (true)
            {
                if (this.m_position + size > this.m_length)
                {
                    break;
                }
                list.Add(this.ReadDouble(endianType));
            }
            return list;
        }

        #endregion Double

        #region Float

        /// <summary>
        /// 从当前流位置读取一个默认端序的<see cref="float"/>值
        /// </summary>
        public float ReadFloat()
        {
            var size = 4;
            if (this.CanReadLength < size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
            }
            var value = TouchSocketBitConverter.Default.To<float>(this.m_span.Slice(this.m_position));
            this.m_position += size;
            return value;
        }

        /// <summary>
        /// 从当前流位置读取一个<see cref="float"/>值
        /// </summary>
        /// <param name="endianType">指定端序</param>
        public float ReadFloat(EndianType endianType)
        {
            var size = 4;
            if (this.CanReadLength < size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
            }
            var value = TouchSocketBitConverter.GetBitConverter(endianType).To<float>(this.m_span.Slice(this.m_position));
            this.m_position += size;
            return value;
        }

        /// <summary>
        /// 将当前有效内存转为默认端序的<see cref="float"/>集合。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<float> ToFloats()
        {
            this.m_position = 0;
            var list = new List<float>();
            var size = 4;
            while (true)
            {
                if (this.m_position + size > this.m_length)
                {
                    break;
                }
                list.Add(this.ReadFloat());
            }
            return list;
        }

        /// <summary>
        /// 将当前有效内存转为指定端序的<see cref="float"/>集合。
        /// </summary>
        /// <param name="endianType"></param>
        /// <returns></returns>
        public IEnumerable<float> ToFloats(EndianType endianType)
        {
            this.m_position = 0;
            var list = new List<float>();
            var size = 4;
            while (true)
            {
                if (this.m_position + size > this.m_length)
                {
                    break;
                }
                list.Add(this.ReadFloat(endianType));
            }
            return list;
        }

        #endregion Float

        #region UInt16

        /// <summary>
        /// 从当前流位置读取一个默认端序的<see cref="ushort"/>值
        /// </summary>
        public ushort ReadUInt16()
        {
            var size = 2;
            if (this.CanReadLength < size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
            }
            var value = TouchSocketBitConverter.Default.To<ushort>(this.m_span.Slice(this.m_position));
            this.m_position += size;
            return value;
        }

        /// <summary>
        /// 从当前流位置读取一个<see cref="ushort"/>值
        /// </summary>
        /// <param name="endianType">指定端序</param>
        public ushort ReadUInt16(EndianType endianType)
        {
            var size = 2;
            if (this.CanReadLength < size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
            }
            var value = TouchSocketBitConverter.GetBitConverter(endianType).To<ushort>(this.m_span.Slice(this.m_position));
            this.m_position += size;
            return value;
        }

        /// <summary>
        /// 将当前有效内存转为默认端序的<see cref="ushort"/>集合。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ushort> ToUInt16s()
        {
            this.m_position = 0;
            var list = new List<ushort>();
            var size = 2;
            while (true)
            {
                if (this.m_position + size > this.m_length)
                {
                    break;
                }
                list.Add(this.ReadUInt16());
            }
            return list;
        }

        /// <summary>
        /// 将当前有效内存转为指定端序的<see cref="ushort"/>集合。
        /// </summary>
        /// <param name="endianType"></param>
        /// <returns></returns>
        public IEnumerable<ushort> ToUInt16s(EndianType endianType)
        {
            this.m_position = 0;
            var list = new List<ushort>();
            var size = 2;
            while (true)
            {
                if (this.m_position + size > this.m_length)
                {
                    break;
                }
                list.Add(this.ReadUInt16(endianType));
            }
            return list;
        }

        #endregion UInt16

        #region UInt32

        /// <summary>
        /// 从当前流位置读取一个默认端序的<see cref="uint"/>值
        /// </summary>
        public uint ReadUInt32()
        {
            var size = 4;
            if (this.CanReadLength < size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
            }
            var value = TouchSocketBitConverter.Default.To<uint>(this.m_span.Slice(this.m_position));
            this.m_position += size;
            return value;
        }

        /// <summary>
        /// 从当前流位置读取一个<see cref="uint"/>值
        /// </summary>
        /// <param name="endianType">指定端序</param>
        public uint ReadUInt32(EndianType endianType)
        {
            var size = 4;
            if (this.CanReadLength < size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
            }
            var value = TouchSocketBitConverter.GetBitConverter(endianType).To<uint>(this.m_span.Slice(this.m_position));
            this.m_position += size;
            return value;
        }

        /// <summary>
        /// 将当前有效内存转为默认端序的<see cref="uint"/>集合。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<uint> ToUInt32s()
        {
            this.m_position = 0;
            var list = new List<uint>();
            var size = 4;
            while (true)
            {
                if (this.m_position + size > this.m_length)
                {
                    break;
                }
                list.Add(this.ReadUInt32());
            }
            return list;
        }

        /// <summary>
        /// 将当前有效内存转为指定端序的<see cref="uint"/>集合。
        /// </summary>
        /// <param name="endianType"></param>
        /// <returns></returns>
        public IEnumerable<uint> ToUInt32s(EndianType endianType)
        {
            this.m_position = 0;
            var list = new List<uint>();
            var size = 4;
            while (true)
            {
                if (this.m_position + size > this.m_length)
                {
                    break;
                }
                list.Add(this.ReadUInt32(endianType));
            }
            return list;
        }

        #endregion UInt32

        #region UInt64

        /// <summary>
        /// 从当前流位置读取一个默认端序的<see cref="ulong"/>值
        /// </summary>
        public ulong ReadUInt64()
        {
            var size = 8;
            if (this.CanReadLength < size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
            }
            var value = TouchSocketBitConverter.Default.To<ulong>(this.m_span.Slice(this.m_position));
            this.m_position += size;
            return value;
        }

        /// <summary>
        /// 从当前流位置读取一个<see cref="ulong"/>值
        /// </summary>
        /// <param name="endianType">指定端序</param>
        public ulong ReadUInt64(EndianType endianType)
        {
            var size = 8;
            if (this.CanReadLength < size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
            }
            var value = TouchSocketBitConverter.GetBitConverter(endianType).To<ulong>(this.m_span.Slice(this.m_position));
            this.m_position += size;
            return value;
        }

        /// <summary>
        /// 将当前有效内存转为默认端序的<see cref="ulong"/>集合。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ulong> ToUInt64s()
        {
            this.m_position = 0;
            var list = new List<ulong>();
            var size = 8;
            while (true)
            {
                if (this.m_position + size > this.m_length)
                {
                    break;
                }
                list.Add(this.ReadUInt64());
            }
            return list;
        }

        /// <summary>
        /// 将当前有效内存转为指定端序的<see cref="ulong"/>集合。
        /// </summary>
        /// <param name="endianType"></param>
        /// <returns></returns>
        public IEnumerable<ulong> ToUInt64s(EndianType endianType)
        {
            this.m_position = 0;
            var list = new List<ulong>();
            var size = 8;
            while (true)
            {
                if (this.m_position + size > this.m_length)
                {
                    break;
                }
                list.Add(this.ReadUInt64(endianType));
            }
            return list;
        }

        #endregion UInt64

        #region Decimal

        /// <summary>
        /// 从当前流位置读取一个默认端序的<see cref="decimal"/>值
        /// </summary>
        public decimal ReadDecimal()
        {
            var size = 16;
            if (this.CanReadLength < size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
            }
            var value = TouchSocketBitConverter.Default.To<decimal>(this.m_span.Slice(this.m_position));
            this.m_position += size;
            return value;
        }

        /// <summary>
        /// 从当前流位置读取一个<see cref="decimal"/>值
        /// </summary>
        /// <param name="endianType">指定端序</param>
        public decimal ReadDecimal(EndianType endianType)
        {
            var size = 16;
            if (this.CanReadLength < size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
            }
            var value = TouchSocketBitConverter.GetBitConverter(endianType).To<decimal>(this.m_span.Slice(this.m_position));
            this.m_position += size;
            return value;
        }

        /// <summary>
        /// 将当前有效内存转为默认端序的<see cref="decimal"/>集合。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<decimal> ToDecimals()
        {
            this.m_position = 0;
            var list = new List<decimal>();
            var size = 16;
            while (true)
            {
                if (this.m_position + size > this.m_length)
                {
                    break;
                }
                list.Add(this.ReadDecimal());
            }
            return list;
        }

        /// <summary>
        /// 将当前有效内存转为指定端序的<see cref="decimal"/>集合。
        /// </summary>
        /// <param name="endianType"></param>
        /// <returns></returns>
        public IEnumerable<decimal> ToDecimals(EndianType endianType)
        {
            this.m_position = 0;
            var list = new List<decimal>();
            var size = 16;
            while (true)
            {
                if (this.m_position + size > this.m_length)
                {
                    break;
                }
                list.Add(this.ReadDecimal(endianType));
            }
            return list;
        }

        #endregion Decimal

        #region Null

        /// <summary>
        /// 从当前流位置读取一个标识值，判断是否为null。
        /// </summary>
        public bool ReadIsNull()
        {
            var status = this.ReadByte();
            return status == 0 || (status == 1 ? false : throw new Exception("标识既非Null，也非NotNull，可能是流位置发生了错误。"));
        }

        #endregion Null

        #region DateTime

        /// <summary>
        /// 从当前流位置读取一个<see cref="DateTime"/>值
        /// </summary>
        public DateTime ReadDateTime()
        {
            var size = 8;
            if (this.CanReadLength < size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
            }
            var value = TouchSocketBitConverter.BigEndian.To<long>(this.m_span.Slice(this.m_position));
            this.m_position += size;
            return DateTime.FromBinary(value);
        }

        /// <summary>
        /// 将当前有效内存转为<see cref="DateTime"/>集合。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DateTime> ToDateTimes()
        {
            this.m_position = 0;
            var list = new List<DateTime>();
            var size = 8;
            while (true)
            {
                if (this.m_position + size > this.m_length)
                {
                    break;
                }
                list.Add(this.ReadDateTime());
            }
            return list;
        }

        #endregion DateTime

        #region TimeSpan

        /// <summary>
        /// 从当前流位置读取一个<see cref="TimeSpan"/>值
        /// </summary>
        public TimeSpan ReadTimeSpan()
        {
            var size = 8;
            if (this.CanReadLength < size)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
            }
            var value = TouchSocketBitConverter.BigEndian.To<long>(this.m_span.Slice(this.m_position));
            this.m_position += size;
            return TimeSpan.FromTicks(value);
        }

        /// <summary>
        /// 将当前有效内存转为<see cref="TimeSpan"/>集合。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TimeSpan> ToTimeSpans()
        {
            this.m_position = 0;
            var list = new List<TimeSpan>();
            var size = 8;
            while (true)
            {
                if (this.m_position + size > this.m_length)
                {
                    break;
                }
                list.Add(this.ReadTimeSpan());
            }
            return list;
        }

        #endregion TimeSpan
    }
}