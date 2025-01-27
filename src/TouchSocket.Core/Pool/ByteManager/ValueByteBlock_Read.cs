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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace TouchSocket.Core;

public partial struct ValueByteBlock
{
    #region Read

    /// <inheritdoc/>
    public int Read(Span<byte> span)
    {
        var length = span.Length;
        if (length == 0)
        {
            return 0;
        }
        var len = this.m_length - this.m_position > length ? length : this.CanReadLength;
        Unsafe.CopyBlock(ref span[0], ref this.m_buffer[this.m_position], (uint)len);
        this.m_position += len;
        return len;
    }

    /// <inheritdoc/>
    public ReadOnlySpan<byte> ReadToSpan(int length)
    {
        var span = new ReadOnlySpan<byte>(this.m_buffer, this.m_position, length);

        this.m_position += length;
        return span;
    }

    #endregion Read

    #region ByteBlock

    /// <inheritdoc/>
    public ByteBlock ReadByteBlock()
    {
        var len = (int)this.ReadVarUInt32() - 1;

        if (len < 0)
        {
            return default;
        }

        var byteBlock = new ByteBlock(len);
        byteBlock.Write(new ReadOnlySpan<byte>(this.m_buffer, this.m_position, len));
        byteBlock.SeekToStart();
        this.m_position += len;
        return byteBlock;
    }

    #endregion ByteBlock

    #region Package

    /// <inheritdoc/>
    public TPackage ReadPackage<TPackage>() where TPackage : class, IPackage, new()
    {
        if (this.ReadIsNull())
        {
            return default;
        }
        else
        {
            var package = new TPackage();
            var block = this;
            package.Unpackage(ref block);
            return package;
        }
    }

    #endregion Package

    #region BytesPackage

    /// <inheritdoc/>
    public byte[] ReadBytesPackage()
    {
        var memory = this.ReadBytesPackageMemory();
        return memory.HasValue ? memory.Value.ToArray() : null;
    }

    /// <inheritdoc/>
    public ReadOnlyMemory<byte>? ReadBytesPackageMemory()
    {
        var length = this.ReadInt32();
        if (length < 0)
        {
            return null;
        }

        var memory = new ReadOnlyMemory<byte>(this.m_buffer, this.m_position, length);
        this.m_position += length;
        return memory;
    }

    #endregion BytesPackage

    #region Byte

    /// <inheritdoc/>
    public byte ReadByte()
    {
        var size = 1;
        if (this.CanReadLength < size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
        }

        var value = this.m_buffer[this.m_position];
        this.m_position += size;
        return value;
    }

    #endregion Byte

    #region String

    /// <inheritdoc/>
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

        var str = Encoding.UTF8.GetString(this.m_buffer, this.m_position, len);
        this.m_position += len;
        return str;
    }

    #endregion String

    #region VarUInt32

    /// <inheritdoc/>
    public uint ReadVarUInt32()
    {
        uint value = 0;
        var bytelength = 0;
        while (true)
        {
            var b = this.m_buffer[this.m_position++];
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

    #endregion VarUInt32

    #region T

    /// <inheritdoc/>
    public T ReadT<T>() where T : unmanaged
    {
        var size = Unsafe.SizeOf<T>();
        if (this.CanReadLength < size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
        }
        var value = TouchSocketBitConverter.Default.UnsafeTo<T>(ref this.m_buffer[this.m_position]);
        this.m_position += size;
        return value;
    }

    /// <inheritdoc/>
    public T ReadT<T>(EndianType endianType) where T : unmanaged
    {
        var size = Unsafe.SizeOf<T>();
        if (this.CanReadLength < size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
        }
        var value = TouchSocketBitConverter.GetBitConverter(endianType).UnsafeTo<T>(ref this.m_buffer[this.m_position]);
        this.m_position += size;
        return value;
    }

    /// <inheritdoc/>
    public IEnumerable<T> ToTs<T>() where T : unmanaged
    {
        this.m_position = 0;
        var list = new List<T>();
        var size = Unsafe.SizeOf<T>();
        while (true)
        {
            if (this.m_position + size > this.m_length)
            {
                break;
            }
            list.Add(this.ReadT<T>());
        }
        return list;
    }

    /// <inheritdoc/>
    public IEnumerable<T> ToTs<T>(EndianType endianType) where T : unmanaged
    {
        this.m_position = 0;
        var list = new List<T>();
        var size = Unsafe.SizeOf<T>();
        while (true)
        {
            if (this.m_position + size > this.m_length)
            {
                break;
            }
            list.Add(this.ReadT<T>(endianType));
        }
        return list;
    }

    #endregion T

    #region Int32

    /// <inheritdoc/>
    public int ReadInt32()
    {
        var size = 4;
        if (this.CanReadLength < size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
        }
        var value = TouchSocketBitConverter.Default.UnsafeTo<int>(ref this.m_buffer[this.m_position]);
        this.m_position += size;
        return value;
    }

    /// <inheritdoc/>
    public int ReadInt32(EndianType endianType)
    {
        var size = 4;
        if (this.CanReadLength < size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
        }
        var value = TouchSocketBitConverter.GetBitConverter(endianType).UnsafeTo<int>(ref this.m_buffer[this.m_position]);
        this.m_position += size;
        return value;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public short ReadInt16()
    {
        var size = 2;
        if (this.CanReadLength < size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
        }
        var value = TouchSocketBitConverter.Default.UnsafeTo<short>(ref this.m_buffer[this.m_position]);
        this.m_position += size;
        return value;
    }

    /// <inheritdoc/>
    public short ReadInt16(EndianType endianType)
    {
        var size = 2;
        if (this.CanReadLength < size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
        }
        var value = TouchSocketBitConverter.GetBitConverter(endianType).UnsafeTo<short>(ref this.m_buffer[this.m_position]);
        this.m_position += size;
        return value;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public long ReadInt64()
    {
        var size = 8;
        if (this.CanReadLength < size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
        }
        var value = TouchSocketBitConverter.Default.UnsafeTo<long>(ref this.m_buffer[this.m_position]);
        this.m_position += size;
        return value;
    }

    /// <inheritdoc/>
    public long ReadInt64(EndianType endianType)
    {
        var size = 8;
        if (this.CanReadLength < size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
        }
        var value = TouchSocketBitConverter.GetBitConverter(endianType).UnsafeTo<long>(ref this.m_buffer[this.m_position]);
        this.m_position += size;
        return value;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public bool ReadBoolean()
    {
        var size = 1;
        if (this.CanReadLength < size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
        }
        var value = TouchSocketBitConverter.Default.UnsafeTo<bool>(ref this.m_buffer[this.m_position]);
        this.m_position += size;
        return value;
    }

    /// <inheritdoc/>
    public bool[] ReadBooleans()
    {
        var size = 1;
        if (this.CanReadLength < size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
        }
        var value = TouchSocketBitConverter.Default.ToBooleans(ref this.m_buffer[this.m_position], 1);
        this.m_position += size;
        return value;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public char ReadChar()
    {
        var size = 2;
        if (this.CanReadLength < size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
        }
        var value = TouchSocketBitConverter.Default.UnsafeTo<char>(ref this.m_buffer[this.m_position]);
        this.m_position += size;
        return value;
    }

    /// <inheritdoc/>
    public char ReadChar(EndianType endianType)
    {
        var size = 1;
        if (this.CanReadLength < size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
        }
        var value = TouchSocketBitConverter.GetBitConverter(endianType).UnsafeTo<char>(ref this.m_buffer[this.m_position]);
        this.m_position += size;
        return value;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public double ReadDouble()
    {
        var size = 8;
        if (this.CanReadLength < size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
        }
        var value = TouchSocketBitConverter.Default.UnsafeTo<double>(ref this.m_buffer[this.m_position]);
        this.m_position += size;
        return value;
    }

    /// <inheritdoc/>
    public double ReadDouble(EndianType endianType)
    {
        var size = 8;
        if (this.CanReadLength < size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
        }
        var value = TouchSocketBitConverter.GetBitConverter(endianType).UnsafeTo<double>(ref this.m_buffer[this.m_position]);
        this.m_position += size;
        return value;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public float ReadFloat()
    {
        var size = 4;
        if (this.CanReadLength < size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
        }
        var value = TouchSocketBitConverter.Default.UnsafeTo<float>(ref this.m_buffer[this.m_position]);
        this.m_position += size;
        return value;
    }

    /// <inheritdoc/>
    public float ReadFloat(EndianType endianType)
    {
        var size = 4;
        if (this.CanReadLength < size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
        }
        var value = TouchSocketBitConverter.GetBitConverter(endianType).UnsafeTo<float>(ref this.m_buffer[this.m_position]);
        this.m_position += size;
        return value;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public ushort ReadUInt16()
    {
        var size = 2;
        if (this.CanReadLength < size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
        }
        var value = TouchSocketBitConverter.Default.UnsafeTo<ushort>(ref this.m_buffer[this.m_position]);
        this.m_position += size;
        return value;
    }

    /// <inheritdoc/>
    public ushort ReadUInt16(EndianType endianType)
    {
        var size = 2;
        if (this.CanReadLength < size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
        }
        var value = TouchSocketBitConverter.GetBitConverter(endianType).UnsafeTo<ushort>(ref this.m_buffer[this.m_position]);
        this.m_position += size;
        return value;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public uint ReadUInt32()
    {
        var size = 4;
        if (this.CanReadLength < size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
        }
        var value = TouchSocketBitConverter.Default.UnsafeTo<uint>(ref this.m_buffer[this.m_position]);
        this.m_position += size;
        return value;
    }

    /// <inheritdoc/>
    public uint ReadUInt32(EndianType endianType)
    {
        var size = 4;
        if (this.CanReadLength < size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
        }
        var value = TouchSocketBitConverter.GetBitConverter(endianType).UnsafeTo<uint>(ref this.m_buffer[this.m_position]);
        this.m_position += size;
        return value;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public ulong ReadUInt64()
    {
        var size = 8;
        if (this.CanReadLength < size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
        }
        var value = TouchSocketBitConverter.Default.UnsafeTo<ulong>(ref this.m_buffer[this.m_position]);
        this.m_position += size;
        return value;
    }

    /// <inheritdoc/>
    public ulong ReadUInt64(EndianType endianType)
    {
        var size = 8;
        if (this.CanReadLength < size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
        }
        var value = TouchSocketBitConverter.GetBitConverter(endianType).UnsafeTo<ulong>(ref this.m_buffer[this.m_position]);
        this.m_position += size;
        return value;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public decimal ReadDecimal()
    {
        var size = 16;
        if (this.CanReadLength < size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
        }
        var value = TouchSocketBitConverter.Default.UnsafeTo<decimal>(ref this.m_buffer[this.m_position]);
        this.m_position += size;
        return value;
    }

    /// <inheritdoc/>
    public decimal ReadDecimal(EndianType endianType)
    {
        var size = 16;
        if (this.CanReadLength < size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
        }
        var value = TouchSocketBitConverter.GetBitConverter(endianType).UnsafeTo<decimal>(ref this.m_buffer[this.m_position]);
        this.m_position += size;
        return value;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public bool ReadIsNull()
    {
        var status = this.ReadByte();
        return status == 0 || (status == 1 ? false : throw new Exception("标识既非Null，也非NotNull，可能是流位置发生了错误。"));
    }

    #endregion Null

    #region DateTime

    /// <inheritdoc/>
    public DateTime ReadDateTime()
    {
        var size = 8;
        if (this.CanReadLength < size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
        }
        var value = TouchSocketBitConverter.BigEndian.UnsafeTo<long>(ref this.m_buffer[this.m_position]);
        this.m_position += size;
        return DateTime.FromBinary(value);
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public TimeSpan ReadTimeSpan()
    {
        var size = 8;
        if (this.CanReadLength < size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), this.CanReadLength, size);
        }
        var value = TouchSocketBitConverter.BigEndian.UnsafeTo<long>(ref this.m_buffer[this.m_position]);
        this.m_position += size;
        return TimeSpan.FromTicks(value);
    }

    /// <inheritdoc/>
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

    #region GUID

    /// <inheritdoc/>
    public Guid ReadGuid()
    {
        Guid guid;
#if NET6_0_OR_GREATER
        guid = new Guid(this.Span.Slice(this.m_position, 16));
#else

        var bytes = this.Span.Slice(this.m_position, 16).ToArray();
        guid = new Guid(bytes);
#endif
        this.m_position += 16;
        return guid;
    }

    #endregion GUID
}