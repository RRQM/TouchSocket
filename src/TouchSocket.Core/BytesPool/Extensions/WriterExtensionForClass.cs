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
    /// <summary>
    /// 将布尔值数组写入到字节写入器中。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器类型，必须继承自<see cref="IBytesWriter"/>。</typeparam>
    /// <param name="writer">字节写入器实例。</param>
    /// <param name="values">要写入的布尔值只读跨度。</param>
    /// <remarks>
    /// 布尔值会被压缩存储，每个字节可存储8个布尔值。
    /// 如果值为空，则不执行任何操作。
    /// </remarks>
    public static void WriteBooleans<TWriter>(this TWriter writer, ReadOnlySpan<bool> values)
        where TWriter : class, IBytesWriter
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

    /// <summary>
    /// 将<see cref="ByteBlock"/>实例写入到字节写入器中。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器类型，必须继承自<see cref="IBytesWriter"/>。</typeparam>
    /// <param name="writer">字节写入器实例。</param>
    /// <param name="byteBlock">要写入的<see cref="ByteBlock"/>实例。</param>
    /// <remarks>
    /// 如果<paramref name="byteBlock"/>为<see langword="null"/>，则写入长度值0。
    /// 否则写入长度+1后跟随实际数据。使用变长编码存储长度信息。
    /// </remarks>
    public static void WriteByteBlock<TWriter>(this TWriter writer, ByteBlock byteBlock)
        where TWriter : class, IBytesWriter
    {
        if (byteBlock is null)
        {
            WriteVarUInt32(writer, 0);
        }
        else
        {
            WriteVarUInt32(writer, (uint)(byteBlock.Length + 1));
            var span = writer.GetSpan(byteBlock.Length);
            byteBlock.Span.CopyTo(span);
            writer.Advance(byteBlock.Length);
        }
    }

    /// <summary>
    /// 将字节跨度写入到字节写入器中。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器类型，必须继承自<see cref="IBytesWriter"/>。</typeparam>
    /// <param name="writer">字节写入器实例。</param>
    /// <param name="span">要写入的字节只读跨度。</param>
    /// <remarks>
    /// 首先写入跨度的长度（使用变长编码），然后写入实际数据。
    /// 如果跨度为空，则只写入长度0。
    /// </remarks>
    public static void WriteByteSpan<TWriter>(this TWriter writer, scoped ReadOnlySpan<byte> span)
        where TWriter : class, IBytesWriter
    {
        WriteVarUInt32(writer, (uint)span.Length);
        if (span.IsEmpty)
        {
            return;
        }

        var writerSpan = writer.GetSpan(span.Length);
        span.CopyTo(writerSpan);
        writer.Advance(span.Length);
    }

    /// <summary>
    /// 写入引用类型的空值标记。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器类型，必须继承自<see cref="IBytesWriter"/>。</typeparam>
    /// <typeparam name="T">要检查的引用类型。</typeparam>
    /// <param name="writer">字节写入器实例。</param>
    /// <param name="t">要检查的对象实例。</param>
    /// <remarks>
    /// 如果对象为<see langword="null"/>，写入0（空值标记）；
    /// 否则写入1（非空值标记）。
    /// </remarks>
    public static void WriteIsNull<TWriter, T>(this TWriter writer, T t)
        where T : class
        where TWriter : class, IBytesWriter
    {
        if (t == null)
        {
            WriteNull(writer);
        }
        else
        {
            WriteNotNull(writer);
        }
    }

    /// <summary>
    /// 写入可空值类型的空值标记。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器类型，必须继承自<see cref="IBytesWriter"/>。</typeparam>
    /// <typeparam name="T">要检查的值类型。</typeparam>
    /// <param name="writer">字节写入器实例。</param>
    /// <param name="t">要检查的可空值类型实例。</param>
    /// <remarks>
    /// 如果可空值类型有值，写入1（非空值标记）；
    /// 否则写入0（空值标记）。
    /// </remarks>
    public static void WriteIsNull<TWriter, T>(this TWriter writer, T? t)
        where T : struct
        where TWriter : class, IBytesWriter
    {
        if (t.HasValue)
        {
            WriteNotNull(writer);
        }
        else
        {
            WriteNull(writer);
        }
    }

    /// <summary>
    /// 将字符串以指定编码写入到字节写入器中（不包含长度前缀）。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器类型，必须继承自<see cref="IBytesWriter"/>。</typeparam>
    /// <param name="writer">字节写入器实例。</param>
    /// <param name="value">要写入的字符串。不能为<see langword="null"/>。</param>
    /// <param name="encoding">字符编码方式。</param>
    /// <exception cref="ArgumentNullException">当<paramref name="value"/>为<see langword="null"/>时抛出。</exception>
    /// <remarks>
    /// 直接写入字符串的字节表示，不包含长度信息。
    /// 实际编码使用UTF-8，忽略<paramref name="encoding"/>参数。
    /// </remarks>
    public static void WriteNormalString<TWriter>(this TWriter writer, string value, Encoding encoding)
        where TWriter : class, IBytesWriter
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

    /// <summary>
    /// 写入非空值标记（值为1的字节）。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器类型，必须继承自<see cref="IBytesWriter"/>。</typeparam>
    /// <param name="writer">字节写入器实例。</param>
    public static void WriteNotNull<TWriter>(this TWriter writer)
        where TWriter : class, IBytesWriter
    {
        WriteValue<TWriter, byte>(writer, 1);
    }

    /// <summary>
    /// 写入空值标记（值为0的字节）。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器类型，必须继承自<see cref="IBytesWriter"/>。</typeparam>
    /// <param name="writer">字节写入器实例。</param>
    public static void WriteNull<TWriter>(this TWriter writer)
        where TWriter : class, IBytesWriter
    {
        WriteValue<TWriter, byte>(writer, 0);
    }

    /// <summary>
    /// 将字符串以指定的固定包头类型写入到字节写入器中。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器类型，必须继承自<see cref="IBytesWriter"/>。</typeparam>
    /// <param name="writer">字节写入器实例。</param>
    /// <param name="value">要写入的字符串。</param>
    /// <param name="headerType">固定包头类型，默认为<see cref="FixedHeaderType.Int"/>。</param>
    /// <remarks>
    /// 根据<paramref name="headerType"/>写入不同大小的长度前缀，然后写入UTF-8编码的字符串数据。
    /// <list type="bullet">
    /// <item><description>对于<see langword="null"/>字符串，写入相应类型的最大值作为标记。</description></item>
    /// <item><description>对于空字符串，写入长度0。</description></item>
    /// <item><description>对于普通字符串，写入实际字节长度后跟随数据。</description></item>
    /// </list>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">当字符串编码后的长度超过指定包头类型的最大值时抛出。</exception>
    public static void WriteString<TWriter>(this TWriter writer, string value, FixedHeaderType headerType = FixedHeaderType.Int)
        where TWriter : class, IBytesWriter
    {
        if (value == null)
        {
            switch (headerType)
            {
                case FixedHeaderType.Byte:
                    WriteValue<TWriter, byte>(writer, byte.MaxValue);
                    return;

                case FixedHeaderType.Ushort:
                    WriteValue<TWriter, ushort>(writer, ushort.MaxValue);
                    return;

                case FixedHeaderType.Int:
                default:
                    WriteValue<TWriter, int>(writer, int.MaxValue);
                    return;
            }
        }
        else if (value == string.Empty)
        {
            switch (headerType)
            {
                case FixedHeaderType.Byte:
                    WriteValue<TWriter, byte>(writer, 0);
                    return;

                case FixedHeaderType.Ushort:
                    WriteValue<TWriter, ushort>(writer, 0);
                    return;

                case FixedHeaderType.Int:
                default:
                    WriteValue<TWriter, int>(writer, 0);
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

    /// <summary>
    /// 将非托管类型的值写入到字节写入器中。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器类型，必须继承自<see cref="IBytesWriter"/>。</typeparam>
    /// <typeparam name="T">要写入的值的类型，必须是非托管类型。</typeparam>
    /// <param name="writer">字节写入器实例。</param>
    /// <param name="value">要写入的值。</param>
    /// <remarks>
    /// 使用默认的字节序（<see cref="TouchSocketBitConverter.Default"/>）将值转换为字节序列并写入。
    /// </remarks>
    public static void WriteValue<TWriter, T>(this TWriter writer, T value)
        where T : unmanaged
        where TWriter : class, IBytesWriter
    {
        var size = Unsafe.SizeOf<T>();
        var span = writer.GetSpan(size);
        TouchSocketBitConverter.Default.WriteBytes(span, value);
        writer.Advance(size);
    }

    /// <summary>
    /// 将非托管类型的值以指定字节序写入到字节写入器中。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器类型，必须继承自<see cref="IBytesWriter"/>。</typeparam>
    /// <typeparam name="T">要写入的值的类型，必须是非托管类型。</typeparam>
    /// <param name="writer">字节写入器实例。</param>
    /// <param name="value">要写入的值。</param>
    /// <param name="endianType">字节序类型。</param>
    /// <remarks>
    /// 使用指定的字节序将值转换为字节序列并写入。
    /// </remarks>
    public static void WriteValue<TWriter, T>(this TWriter writer, T value, EndianType endianType)
        where T : unmanaged
        where TWriter : class, IBytesWriter
    {
        var size = Unsafe.SizeOf<T>();
        var span = writer.GetSpan(size);
        TouchSocketBitConverter.GetBitConverter(endianType).WriteBytes(span, value);
        writer.Advance(size);
    }

    /// <summary>
    /// 将32位无符号整数以变长编码格式写入到字节写入器中。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器类型，必须继承自<see cref="IBytesWriter"/>。</typeparam>
    /// <param name="writer">字节写入器实例。</param>
    /// <param name="value">要写入的32位无符号整数值。</param>
    /// <returns>写入的字节数（1-5个字节）。</returns>
    /// <remarks>
    /// 变长编码使用LEB128格式：
    /// <list type="bullet">
    /// <item><description>每个字节的最高位（MSB）表示是否还有后续字节。</description></item>
    /// <item><description>剩余7位存储实际数据。</description></item>
    /// <item><description>最多需要5个字节来存储32位整数。</description></item>
    /// </list>
    /// </remarks>
    public static int WriteVarUInt32<TWriter>(this TWriter writer, uint value)
                                                where TWriter : class, IBytesWriter
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