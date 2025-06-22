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
using System.Runtime.CompilerServices;
using System.Text;

namespace TouchSocket.Core;

public sealed partial class ByteBlock
{
    #region Write
    /// <inheritdoc/>
    public unsafe void Write(ReadOnlySpan<byte> span)
    {
        this.ThrowIfDisposed();

        if (span.IsEmpty)
        {
            return;
        }

        this.ExtendSize(span.Length);

        fixed (byte* p1 = &span[0])
        {
            fixed (byte* p2 = &this.m_buffer[this.m_position])
            {
                Unsafe.CopyBlock(p2, p1, (uint)span.Length);
            }
        }
        this.m_position += span.Length;
        this.m_length = Math.Max(this.m_position, this.m_length);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ExtendSize(int size)
    {
        if (this.m_buffer.Length - this.m_position < size)
        {
            var need = this.m_buffer.Length + size - (this.m_buffer.Length - this.m_position);
            long lend = this.m_buffer.Length;
            while (need > lend)
            {
                lend *= 2;
            }

            if (lend > int.MaxValue)
            {
                lend = Math.Min(need + 1024 * 1024 * 100, int.MaxValue);
            }
            this.SetCapacity((int)lend, true);
        }
    }

    #endregion Write

    #region ByteBlock

    /// <inheritdoc/>
    public void WriteByteBlock(ByteBlock byteBlock)
    {
        if (byteBlock is null)
        {
            this.WriteVarUInt32(0);
        }
        else
        {
            this.WriteVarUInt32((uint)(byteBlock.Length + 1));
            this.Write(byteBlock.Span);
        }
    }

    #endregion ByteBlock

    #region Package

    /// <inheritdoc/>
    public void WritePackage<TPackage>(TPackage package) where TPackage : class, IPackage
    {
        this.WriteIsNull(package);
        var byteBlock = this;
        package?.Package(ref byteBlock);
    }

    #endregion Package

    #region Null

    /// <inheritdoc/>
    public void WriteIsNull<T>(T t) where T : class
    {
        if (t == null)
        {
            this.WriteNull();
        }
        else
        {
            this.WriteNotNull();
        }
    }

    /// <inheritdoc/>
    public void WriteIsNull<T>(T? t) where T : struct
    {
        if (t.HasValue)
        {
            this.WriteNotNull();
        }
        else
        {
            this.WriteNull();
        }
    }

    /// <inheritdoc/>
    public void WriteNotNull()
    {
        this.WriteByte(1);
    }

    /// <inheritdoc/>
    public void WriteNull()
    {
        this.WriteByte(0);
    }

    #endregion Null

    #region BytesPackage

    /// <inheritdoc/>
    public void WriteBytesPackage(byte[] value, int offset, int length)
    {
        if (value == null)
        {
            this.WriteInt32(-1);
        }
        else if (length == 0)
        {
            this.WriteInt32(0);
        }
        else
        {
            this.WriteInt32(length);
            this.Write(new Span<byte>(value, offset, length));
        }
    }

    /// <inheritdoc/>
    public void WriteBytesPackage(byte[] value)
    {
        if (value == null)
        {
            this.WriteInt32(-1);
        }
        else if (value.Length == 0)
        {
            this.WriteInt32(0);
        }
        else
        {
            this.WriteInt32(value.Length);
            this.Write(new Span<byte>(value, 0, value.Length));
        }
    }

    #endregion BytesPackage

    #region String

    /// <inheritdoc/>
    public void WriteString(string value, FixedHeaderType headerType = FixedHeaderType.Int)
    {
        if (value == null)
        {
            switch (headerType)
            {
                case FixedHeaderType.Byte:
                    this.WriteByte(byte.MaxValue);
                    return;
                case FixedHeaderType.Ushort:
                    this.WriteUInt16(ushort.MaxValue);
                    return;
                case FixedHeaderType.Int:
                default:
                    this.WriteInt32(int.MaxValue);
                    return;
            }

        }
        else if (value == string.Empty)
        {
            switch (headerType)
            {
                case FixedHeaderType.Byte:
                    this.WriteByte(0);
                    return;
                case FixedHeaderType.Ushort:
                    this.WriteUInt16(0);
                    return;
                case FixedHeaderType.Int:
                default:
                    this.WriteInt32(0);
                    return;
            }
        }
        else
        {
            var maxSize = (value.Length + 1) * 3 + 4;
            this.ExtendSize(maxSize);
            var chars = value.AsSpan();

            var offset = headerType switch
            {
                FixedHeaderType.Byte => (byte)1,
                FixedHeaderType.Ushort => (byte)2,
                _ => (byte)4,
            };

            var pos = this.m_position;

            //this.m_position += offset;

            unsafe
            {
                fixed (char* p = &chars[0])
                {
                    fixed (byte* p1 = &this.m_buffer[this.m_position + offset])
                    {
                        var len = Encoding.UTF8.GetBytes(p, chars.Length, p1, maxSize);

                        switch (headerType)
                        {
                            case FixedHeaderType.Byte:
                                if (len >= byte.MaxValue)
                                {
                                    ThrowHelper.ThrowArgumentOutOfRangeException_MoreThan(nameof(value), len, byte.MaxValue);
                                }

                                this.WriteByte((byte)len);
                                break;
                            case FixedHeaderType.Ushort:
                                if (len >= ushort.MaxValue)
                                {
                                    ThrowHelper.ThrowArgumentOutOfRangeException_MoreThan(nameof(value), len, ushort.MaxValue);
                                }
                                this.WriteUInt16((ushort)len);
                                break;
                            case FixedHeaderType.Int:
                            default:
                                if (len >= int.MaxValue)
                                {
                                    ThrowHelper.ThrowArgumentOutOfRangeException_MoreThan(nameof(value), len, int.MaxValue);
                                }
                                this.WriteInt32(len);
                                break;
                        }

                        this.m_position += len;

                        this.m_length = Math.Max(this.m_position, this.m_length);
                    }

                }
            }
        }
    }
    #endregion String

    #region NormalString
    /// <inheritdoc/>
    public void WriteNormalString(string value, Encoding encoding)
    {
        ThrowHelper.ThrowArgumentNullExceptionIf(value, nameof(value));
        var maxSize = encoding.GetMaxByteCount(value.Length);
        this.ExtendSize(maxSize);
        var chars = value.AsSpan();

        unsafe
        {
            fixed (char* p = &chars[0])
            {
                fixed (byte* p1 = &this.m_buffer[this.m_position])
                {
                    var len = Encoding.UTF8.GetBytes(p, chars.Length, p1, maxSize);

                    this.m_position += len;

                    this.m_length = Math.Max(this.m_position, this.m_length);
                }

            }
        }
    }
    #endregion

    #region VarUInt32

    /// <inheritdoc/>
    public int WriteVarUInt32(uint value)
    {
        this.ExtendSize(5);

        byte bytelength = 0;
        while (value > 0x7F)
        {
            //127=0x7F=0b01111111，大于说明msb=1，即后续还有字节
            var temp = value & 0x7F; //得到数值的后7位,0x7F=0b01111111,0与任何数与都是0,1与任何数与还是任何数
            temp |= 0x80; //后7位不变最高位固定为1,0x80=0b10000000,1与任何数或都是1，0与任何数或都是任何数
            this.m_buffer[this.m_position++] = (byte)temp; //存储msb=1的数据
            value >>= 7; //右移已经计算过的7位得到下次需要计算的数值
            bytelength++;
        }
        this.m_buffer[this.m_position++] = (byte)value; //最后一个字节msb=0

        this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
        return bytelength + 1;
    }

    #endregion VarUInt32

    #region Int32

    /// <inheritdoc/>
    public void WriteInt32(int value)
    {
        var size = 4;
        this.ExtendSize(size);
        TouchSocketBitConverter.Default.GetBytes(ref this.m_buffer[this.m_position], value);
        this.m_position += size;
        this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
    }

    /// <inheritdoc/>
    public void WriteInt32(int value, EndianType endianType)
    {
        var size = 4;
        this.ExtendSize(size);
        TouchSocketBitConverter.GetBitConverter(endianType).GetBytes(ref this.m_buffer[this.m_position], value);
        this.m_position += size;
        this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
    }

    #endregion Int32

    #region T

    /// <inheritdoc/>
    public void WriteT<T>(T value) where T : unmanaged
    {
        var size = Unsafe.SizeOf<T>();
        this.ExtendSize(size);
        TouchSocketBitConverter.Default.UnsafeWriteBytes(ref this.m_buffer[this.m_position], value);
        this.m_position += size;
        this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
    }

    /// <inheritdoc/>
    public void WriteT<T>(T value, EndianType endianType) where T : unmanaged
    {
        var size = Unsafe.SizeOf<T>();
        this.ExtendSize(size);
        TouchSocketBitConverter.GetBitConverter(endianType).UnsafeWriteBytes(ref this.m_buffer[this.m_position], value);
        this.m_position += size;
        this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
    }

    #endregion T

    #region Int16

    /// <inheritdoc/>
    public void WriteInt16(short value)
    {
        var size = 2;
        this.ExtendSize(size);
        TouchSocketBitConverter.Default.GetBytes(ref this.m_buffer[this.m_position], value);
        this.m_position += size;
        this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
    }

    /// <inheritdoc/>
    public void WriteInt16(short value, EndianType endianType)
    {
        var size = 2;
        this.ExtendSize(size);
        TouchSocketBitConverter.GetBitConverter(endianType).GetBytes(ref this.m_buffer[this.m_position], value);
        this.m_position += size;
        this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
    }

    #endregion Int16

    #region Int64

    /// <inheritdoc/>
    public void WriteInt64(long value)
    {
        var size = 8;
        this.ExtendSize(size);
        TouchSocketBitConverter.Default.GetBytes(ref this.m_buffer[this.m_position], value);
        this.m_position += size;
        this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
    }

    /// <inheritdoc/>
    public void WriteInt64(long value, EndianType endianType)
    {
        var size = 8;
        this.ExtendSize(size);
        TouchSocketBitConverter.GetBitConverter(endianType).GetBytes(ref this.m_buffer[this.m_position], value);
        this.m_position += size;
        this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
    }

    #endregion Int64

    #region Boolean

    /// <inheritdoc/>
    public void WriteBoolean(bool value)
    {
        var size = 1;
        this.ExtendSize(size);
        TouchSocketBitConverter.Default.GetBytes(ref this.m_buffer[this.m_position], value);
        this.m_position += size;
        this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
    }

    /// <inheritdoc/>
    public void WriteBooleans(ReadOnlySpan<bool> values)
    {
        var size = values.Length % 8 == 0 ? values.Length / 8 : values.Length / 8 + 1;
        this.ExtendSize(size);
        TouchSocketBitConverter.Default.GetBytes(ref this.m_buffer[this.m_position], values);
        this.m_position += size;
        this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
    }

    #endregion Boolean

    #region Byte

    /// <inheritdoc/>
    public void WriteByte(byte value)
    {
        var size = 1;
        this.ExtendSize(size);
        this.m_buffer[this.m_position] = value;
        this.m_position += size;
        this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
    }

    #endregion Byte

    #region Char

    /// <inheritdoc/>
    public void WriteChar(char value)
    {
        var size = 2;
        this.ExtendSize(size);
        TouchSocketBitConverter.Default.GetBytes(ref this.m_buffer[this.m_position], value);
        this.m_position += size;
        this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
    }

    /// <inheritdoc/>
    public void WriteChar(char value, EndianType endianType)
    {
        var size = 2;
        this.ExtendSize(size);
        TouchSocketBitConverter.GetBitConverter(endianType).GetBytes(ref this.m_buffer[this.m_position], value);
        this.m_position += size;
        this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
    }

    #endregion Char

    #region Double

    /// <inheritdoc/>
    public void WriteDouble(double value)
    {
        var size = 8;
        this.ExtendSize(size);
        TouchSocketBitConverter.Default.GetBytes(ref this.m_buffer[this.m_position], value);
        this.m_position += size;
        this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
    }

    /// <inheritdoc/>
    public void WriteDouble(double value, EndianType endianType)
    {
        var size = 8;
        this.ExtendSize(size);
        TouchSocketBitConverter.GetBitConverter(endianType).GetBytes(ref this.m_buffer[this.m_position], value);
        this.m_position += size;
        this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
    }

    #endregion Double

    #region Float

    /// <inheritdoc/>
    public void WriteFloat(float value)
    {
        var size = 4;
        this.ExtendSize(size);
        TouchSocketBitConverter.Default.GetBytes(ref this.m_buffer[this.m_position], value);
        this.m_position += size;
        this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
    }

    /// <inheritdoc/>
    public void WriteFloat(float value, EndianType endianType)
    {
        var size = 4;
        this.ExtendSize(size);
        TouchSocketBitConverter.GetBitConverter(endianType).GetBytes(ref this.m_buffer[this.m_position], value);
        this.m_position += size;
        this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
    }

    #endregion Float

    #region UInt16

    /// <inheritdoc/>
    public void WriteUInt16(ushort value)
    {
        var size = 2;
        this.ExtendSize(size);
        TouchSocketBitConverter.Default.UnsafeWriteBytes(ref this.m_buffer[this.m_position], value);
        this.m_position += size;
        this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
    }

    /// <inheritdoc/>
    public void WriteUInt16(ushort value, EndianType endianType)
    {
        var size = 2;
        this.ExtendSize(size);
        TouchSocketBitConverter.GetBitConverter(endianType).UnsafeWriteBytes(ref this.m_buffer[this.m_position], value);
        this.m_position += size;
        this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
    }

    #endregion UInt16

    #region UInt32

    /// <inheritdoc/>
    public void WriteUInt32(uint value)
    {
        var size = 4;
        this.ExtendSize(size);
        TouchSocketBitConverter.Default.GetBytes(ref this.m_buffer[this.m_position], value);
        this.m_position += size;
        this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
    }

    /// <inheritdoc/>
    public void WriteUInt32(uint value, EndianType endianType)
    {
        var size = 4;
        this.ExtendSize(size);
        TouchSocketBitConverter.GetBitConverter(endianType).GetBytes(ref this.m_buffer[this.m_position], value);
        this.m_position += size;
        this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
    }

    #endregion UInt32

    #region UInt64

    /// <inheritdoc/>
    public void WriteUInt64(ulong value)
    {
        var size = 8;
        this.ExtendSize(size);
        TouchSocketBitConverter.Default.GetBytes(ref this.m_buffer[this.m_position], value);
        this.m_position += size;
        this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
    }

    /// <inheritdoc/>
    public void WriteUInt64(ulong value, EndianType endianType)
    {
        var size = 8;
        this.ExtendSize(size);
        TouchSocketBitConverter.GetBitConverter(endianType).GetBytes(ref this.m_buffer[this.m_position], value);
        this.m_position += size;
        this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
    }

    #endregion UInt64

    #region Decimal

    /// <inheritdoc/>
    public void WriteDecimal(decimal value)
    {
        var size = sizeof(decimal);
        this.ExtendSize(size);
        TouchSocketBitConverter.Default.GetBytes(ref this.m_buffer[this.m_position], value);
        this.m_position += size;
        this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
    }

    /// <inheritdoc/>
    public void WriteDecimal(decimal value, EndianType endianType)
    {
        var size = 16;
        this.ExtendSize(size);
        TouchSocketBitConverter.GetBitConverter(endianType).GetBytes(ref this.m_buffer[this.m_position], value);
        this.m_position += size;
        this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
    }

    #endregion Decimal

    #region DateTime

    /// <inheritdoc/>
    public void WriteDateTime(DateTime value)
    {
        var size = 8;
        this.ExtendSize(size);
        TouchSocketBitConverter.BigEndian.GetBytes(ref this.m_buffer[this.m_position], value.ToBinary());
        this.m_position += size;
        this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
    }

    #endregion DateTime

    #region TimeSpan

    /// <inheritdoc/>
    public void WriteTimeSpan(TimeSpan value)
    {
        var size = 8;
        this.ExtendSize(size);
        TouchSocketBitConverter.BigEndian.GetBytes(ref this.m_buffer[this.m_position], value.Ticks);
        this.m_position += size;
        this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
    }

    #endregion TimeSpan

    #region GUID
    /// <inheritdoc/>
    public void WriteGuid(in Guid value)
    {
        var size = 16;
        this.ExtendSize(size);
#if NET6_0_OR_GREATER
        value.TryWriteBytes(this.TotalMemory.Span.Slice(this.m_position, size));
#else
        var bytes = value.ToByteArray();
        this.Write(bytes);
#endif
        this.m_position += size;
        this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
    }

    #endregion GUID
}