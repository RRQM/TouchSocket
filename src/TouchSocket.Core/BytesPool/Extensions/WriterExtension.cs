// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace TouchSocket.Core;

public static partial class WriterExtension
{
    public static void WriteBooleans<TWriter>(ref TWriter writer, ReadOnlySpan<bool> values)
        where TWriter : IBytesWriter
    {
        if (values.IsEmpty)
        {
            return;
        }
        var size = TouchSocketBitConverter.GetConvertedLength<bool, byte>(values.Length);

        var span = writer.GetSpan(size);

        TouchSocketBitConverter.ConvertValues<bool, byte>(values, span);
        writer.Advance(size);
    }

    public static void WriteByteBlock<TWriter>(ref TWriter writer, ByteBlock byteBlock)
        where TWriter : IBytesWriter
    {
        if (byteBlock is null)
        {
            WriteVarUInt32(ref writer, 0);
        }
        else
        {
            WriteVarUInt32(ref writer, (uint)(byteBlock.Length + 1));
            var span = writer.GetSpan(byteBlock.Length);
            byteBlock.Span.CopyTo(span);
            writer.Advance(byteBlock.Length);
        }
    }

    public static void WriteByteSpan<TWriter>(ref TWriter writer, scoped ReadOnlySpan<byte> span)
        where TWriter : IBytesWriter
    {
        WriteVarUInt32(ref writer, (uint)span.Length);
        if (span.IsEmpty)
        {
            return;
        }

        var writerSpan = writer.GetSpan(span.Length);
        span.CopyTo(writerSpan);
        writer.Advance(span.Length);
    }

    public static void WriteIsNull<TWriter, T>(ref TWriter writer, T t)
        where T : class
        where TWriter : IBytesWriter
    {
        if (t == null)
        {
            WriteNull(ref writer);
        }
        else
        {
            WriteNotNull(ref writer);
        }
    }

    public static void WriteIsNull<TWriter, T>(ref TWriter writer, T? t)
        where T : struct
        where TWriter : IBytesWriter
    {
        if (t.HasValue)
        {
            WriteNotNull(ref writer);
        }
        else
        {
            WriteNull(ref writer);
        }
    }

    public static void WriteNormalString<TWriter>(ref TWriter writer, string value, Encoding encoding)
        where TWriter : IBytesWriter
    {
        ThrowHelper.ThrowArgumentNullExceptionIf(value, nameof(value));
        var maxSize = encoding.GetMaxByteCount(value.Length);
        var span = writer.GetSpan(maxSize);
        var chars = value.AsSpan();

        unsafe
        {
            fixed (char* p = &chars[0])
            {
                fixed (byte* p1 = &span[0])
                {
                    var len = Encoding.UTF8.GetBytes(p, chars.Length, p1, maxSize);
                    writer.Advance(len);
                }
            }
        }
    }

    public static void WriteNotNull<TWriter>(ref TWriter writer)
        where TWriter : IBytesWriter
    {
        WriteValue<TWriter, byte>(ref writer, 1);
    }

    public static void WriteNull<TWriter>(ref TWriter writer)
        where TWriter : IBytesWriter
    {
        WriteValue<TWriter, byte>(ref writer, 0);
    }

    public static void WritePackage<TWriter, TPackage>(ref TWriter writer, TPackage package)
        where TWriter : IBytesWriter
        where TPackage :class, IPackage
    {
        if (package is null)
        {
            WriteNull(ref writer);
            return;
        }
        else
        {
            WriteNotNull(ref writer);
            package.Package(ref writer);
        }
    }

    public static void WriteString<TWriter>(ref TWriter writer, string value, FixedHeaderType headerType = FixedHeaderType.Int)
            where TWriter : IBytesWriter
    {
        if (value == null)
        {
            switch (headerType)
            {
                case FixedHeaderType.Byte:
                    WriteValue<TWriter, byte>(ref writer, byte.MaxValue);
                    return;

                case FixedHeaderType.Ushort:
                    WriteValue<TWriter, ushort>(ref writer, ushort.MaxValue);
                    return;

                case FixedHeaderType.Int:
                default:
                    WriteValue<TWriter, int>(ref writer, int.MaxValue);
                    return;
            }
        }
        else if (value == string.Empty)
        {
            switch (headerType)
            {
                case FixedHeaderType.Byte:
                    WriteValue<TWriter, byte>(ref writer, 0);
                    return;

                case FixedHeaderType.Ushort:
                    WriteValue<TWriter, ushort>(ref writer, 0);
                    return;

                case FixedHeaderType.Int:
                default:
                    WriteValue<TWriter, int>(ref writer, 0);
                    return;
            }
        }
        else
        {
            var maxSize = Encoding.UTF8.GetMaxByteCount(value.Length);

            var chars = value.AsSpan();

            var headerLength = headerType switch
            {
                FixedHeaderType.Byte => (byte)1,
                FixedHeaderType.Ushort => (byte)2,
                _ => (byte)4,
            };

            var writerAnchor = new WriterAnchor<TWriter>(ref writer, headerLength);
            //var headerSpan = writer.GetSpan(headerLength);
            //writer.Advance(headerLength);

            var bodySpan = writer.GetSpan(maxSize);

            unsafe
            {
                fixed (char* p = &chars[0])
                {
                    fixed (byte* p1 = &bodySpan[0])
                    {
                        var len = Encoding.UTF8.GetBytes(p, chars.Length, p1, maxSize);

                        writer.Advance(len);

                        var headerSpan = writerAnchor.Rewind(ref writer, out _);
                        switch (headerType)
                        {
                            case FixedHeaderType.Byte:
                                if (len >= byte.MaxValue)
                                {
                                    ThrowHelper.ThrowArgumentOutOfRangeException_MoreThan(nameof(value), len, byte.MaxValue);
                                }

                                headerSpan.WriteValue((byte)len);
                                break;

                            case FixedHeaderType.Ushort:
                                if (len >= ushort.MaxValue)
                                {
                                    ThrowHelper.ThrowArgumentOutOfRangeException_MoreThan(nameof(value), len, ushort.MaxValue);
                                }
                                headerSpan.WriteValue((ushort)len);
                                break;

                            case FixedHeaderType.Int:
                            default:
                                if (len >= int.MaxValue)
                                {
                                    ThrowHelper.ThrowArgumentOutOfRangeException_MoreThan(nameof(value), len, int.MaxValue);
                                }
                                headerSpan.WriteValue(len);
                                break;
                        }
                    }
                }
            }
        }
    }

    public static void WriteValue<TWriter, T>(ref TWriter writer, T value)
        where T : unmanaged
        where TWriter : IBytesWriter
    {
        var size = Unsafe.SizeOf<T>();
        var span = writer.GetSpan(size);
        TouchSocketBitConverter.Default.WriteBytes(span, value);
        writer.Advance(size);
    }

    public static void WriteValue<TWriter, T>(ref TWriter writer, T value, EndianType endianType)
        where T : unmanaged
        where TWriter : IBytesWriter
    {
        var size = Unsafe.SizeOf<T>();
        var span = writer.GetSpan(size);
        TouchSocketBitConverter.GetBitConverter(endianType).WriteBytes(span, value);
        writer.Advance(size);
    }

    public static int WriteVarUInt32<TWriter>(ref TWriter writer, uint value)
                                                where TWriter : IBytesWriter
    {
        var span = writer.GetSpan(5); //最多需要5个字节

        byte byteLength = 0;
        while (value > 0x7F)
        {
            //127=0x7F=0b01111111，大于说明msb=1，即后续还有字节
            var temp = value & 0x7F; //得到数值的后7位,0x7F=0b01111111,0与任何数与都是0,1与任何数与还是任何数
            temp |= 0x80; //后7位不变最高位固定为1,0x80=0b10000000,1与任何数或都是1，0与任何数或都是任何数
            span[byteLength++] = (byte)temp; //存储msb=1的数据
            value >>= 7; //右移已经计算过的7位得到下次需要计算的数值
        }
        span[byteLength++] = (byte)value; //最后一个字节msb=0

        writer.Advance(byteLength);
        return byteLength;
    }
}