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

using System.Runtime.CompilerServices;

namespace TouchSocket.Core;

/// <summary>
/// 为<see cref="IBytesWriter"/>提供扩展方法的静态类，用于写入各种类型的数据。
/// </summary>
/// <remarks>
/// 此类提供了丰富的扩展方法，支持写入基本数据类型、字符串、字节块、枚举、包对象等。
/// 所有方法都是针对实现了<see cref="IBytesWriter"/>接口的类型进行扩展。
/// </remarks>
public static partial class WriterExtension
{
    private const int ChunkSize = 4096;

    /// <summary>
    /// 将布尔值数组写入字节写入器。
    /// </summary>
    /// <typeparam name="TWriter">实现<see cref="IBytesWriter"/>接口的写入器类型。</typeparam>
    /// <param name="writer">字节写入器实例。</param>
    /// <param name="values">要写入的布尔值只读跨度。</param>
    /// <remarks>
    /// 此方法将布尔值数组转换为字节数组后写入，如果数组为空则不进行任何操作。
    /// </remarks>
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

    /// <summary>
    /// 将<see cref="ByteBlock"/>对象写入字节写入器。
    /// </summary>
    /// <typeparam name="TWriter">实现<see cref="IBytesWriter"/>接口的写入器类型。</typeparam>
    /// <param name="writer">字节写入器实例。</param>
    /// <param name="byteBlock">要写入的<see cref="ByteBlock"/>对象。</param>
    /// <remarks>
    /// 如果<paramref name="byteBlock"/>为 <see langword="null"/>，则写入长度为0；
    /// 否则先写入长度信息（长度+1），再写入字节块内容。
    /// </remarks>
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
            
            var remaining = byteBlock.Length;
            var offset = 0;
            
            while (remaining > 0)
            {
                var bytesToWrite = Math.Min(remaining, ChunkSize);
                var span = writer.GetSpan(bytesToWrite);
                byteBlock.Span.Slice(offset, bytesToWrite).CopyTo(span);
                writer.Advance(bytesToWrite);
                
                offset += bytesToWrite;
                remaining -= bytesToWrite;
            }
        }
    }

    /// <summary>
    /// 将字节跨度写入字节写入器。
    /// </summary>
    /// <typeparam name="TWriter">实现<see cref="IBytesWriter"/>接口的写入器类型。</typeparam>
    /// <param name="writer">字节写入器实例。</param>
    /// <param name="span">要写入的只读字节跨度。</param>
    /// <remarks>
    /// 此方法首先写入长度信息，然后写入字节跨度的内容。如果跨度为空则只写入长度0。
    /// </remarks>
    public static void WriteByteSpan<TWriter>(ref TWriter writer, scoped ReadOnlySpan<byte> span)
        where TWriter : IBytesWriter
    {
        WriteVarUInt32(ref writer, (uint)span.Length);
        if (span.IsEmpty)
        {
            return;
        }

        var remaining = span.Length;
        var offset = 0;
        
        while (remaining > 0)
        {
            var bytesToWrite = Math.Min(remaining, ChunkSize);
            var writerSpan = writer.GetSpan(bytesToWrite);
            span.Slice(offset, bytesToWrite).CopyTo(writerSpan);
            writer.Advance(bytesToWrite);
            
            offset += bytesToWrite;
            remaining -= bytesToWrite;
        }
    }

    /// <summary>
    /// 将枚举值写入字节写入器。
    /// </summary>
    /// <typeparam name="TWriter">实现<see cref="IBytesWriter"/>接口的写入器类型。</typeparam>
    /// <param name="writer">字节写入器实例。</param>
    /// <param name="value">要写入的枚举值。</param>
    /// <exception cref="NotSupportedException">当枚举的底层类型不受支持时抛出。</exception>
    /// <remarks>
    /// 支持的底层类型包括：<see cref="byte"/>、<see cref="sbyte"/>、<see cref="short"/>、
    /// <see cref="ushort"/>、<see cref="int"/>、<see cref="uint"/>、<see cref="long"/>、<see cref="ulong"/>。
    /// </remarks>
    public static void WriteEnum<TWriter>(ref TWriter writer, Enum value)
        where TWriter : IBytesWriter
    {
        var underlyingType = Enum.GetUnderlyingType(value.GetType());
        if (underlyingType == typeof(byte))
        {
            WriteValue(ref writer, Convert.ToByte(value));
        }
        else if (underlyingType == typeof(sbyte))
        {
            WriteValue(ref writer, Convert.ToSByte(value));
        }
        else if (underlyingType == typeof(short))
        {
            WriteValue(ref writer, Convert.ToInt16(value));
        }
        else if (underlyingType == typeof(ushort))
        {
            WriteValue(ref writer, Convert.ToUInt16(value));
        }
        else if (underlyingType == typeof(int))
        {
            WriteValue(ref writer, Convert.ToInt32(value));
        }
        else if (underlyingType == typeof(uint))
        {
            WriteValue(ref writer, Convert.ToUInt32(value));
        }
        else if (underlyingType == typeof(long))
        {
            WriteValue(ref writer, Convert.ToInt64(value));
        }
        else if (underlyingType == typeof(ulong))
        {
            WriteValue(ref writer, Convert.ToUInt64(value));
        }
        else
        {
            ThrowHelper.ThrowNotSupportedException($"Unsupported enum underlying type: {underlyingType}");
        }
    }

    /// <summary>
    /// 写入引用类型的空值标识。
    /// </summary>
    /// <typeparam name="TWriter">实现<see cref="IBytesWriter"/>接口的写入器类型。</typeparam>
    /// <typeparam name="T">引用类型。</typeparam>
    /// <param name="writer">字节写入器实例。</param>
    /// <param name="t">要检查的对象。</param>
    /// <remarks>
    /// 如果对象为 <see langword="null"/> 则写入空值标识，否则写入非空值标识。
    /// </remarks>
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

    /// <summary>
    /// 写入可空值类型的空值标识。
    /// </summary>
    /// <typeparam name="TWriter">实现<see cref="IBytesWriter"/>接口的写入器类型。</typeparam>
    /// <typeparam name="T">值类型。</typeparam>
    /// <param name="writer">字节写入器实例。</param>
    /// <param name="t">要检查的可空值类型。</param>
    /// <remarks>
    /// 如果值有值则写入非空值标识，否则写入空值标识。
    /// </remarks>
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

    /// <summary>
    /// 使用指定编码将字符串写入字节写入器。
    /// </summary>
    /// <typeparam name="TWriter">实现<see cref="IBytesWriter"/>接口的写入器类型。</typeparam>
    /// <param name="writer">字节写入器实例。</param>
    /// <param name="value">要写入的字符串。</param>
    /// <param name="encoding">要使用的字符编码。</param>
    /// <exception cref="ArgumentNullException">当<paramref name="value"/>为 <see langword="null"/>时抛出。</exception>
    /// <remarks>
    /// 此方法直接将字符串按指定编码写入，不包含长度信息。
    /// </remarks>
    public static void WriteNormalString<TWriter>(ref TWriter writer, string value, Encoding encoding)
        where TWriter : IBytesWriter
    {
        ThrowHelper.ThrowIfNull(value, nameof(value));

        // issue https://gitee.com/RRQM_Home/TouchSocket/issues/IDK7W3
        if (value.Length == 0)
        {
            return;
        }

        var remaining = value.Length;
        var offset = 0;

        unsafe
        {
            fixed (char* p = value)
            {
                while (remaining > 0)
                {
                    var charsToProcess = Math.Min(remaining, ChunkSize);
                    var maxByteCount = encoding.GetMaxByteCount(charsToProcess);
                    var span = writer.GetSpan(maxByteCount);

                    fixed (byte* p1 = &span[0])
                    {
                        var bytesWritten = encoding.GetBytes(p + offset, charsToProcess, p1, maxByteCount);
                        writer.Advance(bytesWritten);
                    }

                    offset += charsToProcess;
                    remaining -= charsToProcess;
                }
            }
        }
    }

    /// <summary>
    /// 写入非空值标识。
    /// </summary>
    /// <typeparam name="TWriter">实现<see cref="IBytesWriter"/>接口的写入器类型。</typeparam>
    /// <param name="writer">字节写入器实例。</param>
    /// <remarks>
    /// 写入字节值1，表示非空值。
    /// </remarks>
    public static void WriteNotNull<TWriter>(ref TWriter writer)
        where TWriter : IBytesWriter
    {
        WriteValue<TWriter, byte>(ref writer, 1);
    }

    /// <summary>
    /// 写入空值标识。
    /// </summary>
    /// <typeparam name="TWriter">实现<see cref="IBytesWriter"/>接口的写入器类型。</typeparam>
    /// <param name="writer">字节写入器实例。</param>
    /// <remarks>
    /// 写入字节值0，表示空值。
    /// </remarks>
    public static void WriteNull<TWriter>(ref TWriter writer)
        where TWriter : IBytesWriter
    {
        WriteValue<TWriter, byte>(ref writer, 0);
    }

    /// <summary>
    /// 将包对象写入字节写入器。
    /// </summary>
    /// <typeparam name="TWriter">实现<see cref="IBytesWriter"/>接口的写入器类型。</typeparam>
    /// <typeparam name="TPackage">实现<see cref="IPackage"/>接口的包类型。</typeparam>
    /// <param name="writer">字节写入器实例。</param>
    /// <param name="package">要写入的包对象。</param>
    /// <remarks>
    /// 如果包对象为 <see langword="null"/>，则写入空值标识；否则写入非空值标识并调用包对象的Package方法进行序列化。
    /// </remarks>
    public static void WritePackage<TWriter, TPackage>(ref TWriter writer, TPackage package)
        where TWriter : IBytesWriter
        where TPackage : class, IPackage
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

    /// <summary>
    /// 将UTF-8编码的字符串写入字节写入器。
    /// </summary>
    /// <typeparam name="TWriter">实现<see cref="IBytesWriter"/>接口的写入器类型。</typeparam>
    /// <param name="writer">字节写入器实例。</param>
    /// <param name="value">要写入的字符串。</param>
    /// <param name="headerType">固定头部类型，指定长度字段的大小。默认为<see cref="FixedHeaderType.Int"/>。</param>
    /// <exception cref="ArgumentOutOfRangeException">当字符串长度超过头部类型允许的最大值时抛出。</exception>
    /// <remarks>
    /// 此方法根据指定的头部类型写入长度信息，然后写入UTF-8编码的字符串内容。
    /// 当字符串为 <see langword="null"/> 时，长度字段写入对应类型的最大值；当字符串为空时，长度字段写入0。
    /// </remarks>
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
            var chars = value.AsSpan();

            var headerLength = headerType switch
            {
                FixedHeaderType.Byte => (byte)1,
                FixedHeaderType.Ushort => (byte)2,
                _ => (byte)4,
            };

            var writerAnchor = new WriterAnchor<TWriter>(ref writer, headerLength);

            var totalLen = 0;
            var remaining = chars.Length;
            var offset = 0;

            unsafe
            {
                fixed (char* p = &chars[0])
                {
                    while (remaining > 0)
                    {
                        var charsToProcess = Math.Min(remaining, ChunkSize);
                        var maxByteCount = Encoding.UTF8.GetMaxByteCount(charsToProcess);
                        var bodySpan = writer.GetSpan(maxByteCount);

                        fixed (byte* p1 = &bodySpan[0])
                        {
                            var bytesWritten = Encoding.UTF8.GetBytes(p + offset, charsToProcess, p1, maxByteCount);
                            writer.Advance(bytesWritten);
                            totalLen += bytesWritten;
                        }

                        offset += charsToProcess;
                        remaining -= charsToProcess;
                    }
                }
            }

            var headerSpan = writerAnchor.Rewind(ref writer, out _);
            switch (headerType)
            {
                case FixedHeaderType.Byte:
                    if (totalLen >= byte.MaxValue)
                    {
                        ThrowHelper.ThrowArgumentOutOfRangeException_MoreThan(nameof(value), totalLen, byte.MaxValue);
                    }
                    headerSpan.WriteValue((byte)totalLen);
                    break;

                case FixedHeaderType.Ushort:
                    if (totalLen >= ushort.MaxValue)
                    {
                        ThrowHelper.ThrowArgumentOutOfRangeException_MoreThan(nameof(value), totalLen, ushort.MaxValue);
                    }
                    headerSpan.WriteValue((ushort)totalLen);
                    break;

                case FixedHeaderType.Int:
                default:
                    if (totalLen >= int.MaxValue)
                    {
                        ThrowHelper.ThrowArgumentOutOfRangeException_MoreThan(nameof(value), totalLen, int.MaxValue);
                    }
                    headerSpan.WriteValue(totalLen);
                    break;
            }
        }
    }

    /// <summary>
    /// 将指定类型的值写入字节写入器，使用默认的字节序。
    /// </summary>
    /// <typeparam name="TWriter">实现<see cref="IBytesWriter"/>接口的写入器类型。</typeparam>
    /// <typeparam name="T">要写入的值类型，必须是非托管类型。</typeparam>
    /// <param name="writer">字节写入器实例。</param>
    /// <param name="value">要写入的值。</param>
    /// <remarks>
    /// 此方法使用默认的字节序转换器写入值，并推进写入器位置。
    /// </remarks>
    public static void WriteValue<TWriter, T>(ref TWriter writer, T value)
        where T : unmanaged
        where TWriter : IBytesWriter
    {
        var size = Unsafe.SizeOf<T>();
        var span = writer.GetSpan(size);
        TouchSocketBitConverter.Default.WriteBytes(span, value);
        writer.Advance(size);
    }

    /// <summary>
    /// 将指定类型的值写入字节写入器，使用指定的字节序。
    /// </summary>
    /// <typeparam name="TWriter">实现<see cref="IBytesWriter"/>接口的写入器类型。</typeparam>
    /// <typeparam name="T">要写入的值类型，必须是非托管类型。</typeparam>
    /// <param name="writer">字节写入器实例。</param>
    /// <param name="value">要写入的值。</param>
    /// <param name="endianType">字节序类型。</param>
    /// <remarks>
    /// 此方法使用指定的字节序转换器写入值，并推进写入器位置。
    /// </remarks>
    public static void WriteValue<TWriter, T>(ref TWriter writer, T value, EndianType endianType)
        where T : unmanaged
        where TWriter : IBytesWriter
    {
        var size = Unsafe.SizeOf<T>();
        var span = writer.GetSpan(size);
        TouchSocketBitConverter.GetBitConverter(endianType).WriteBytes(span, value);
        writer.Advance(size);
    }

    /// <summary>
    /// 将可变长度编码的无符号32位整数写入字节写入器。
    /// </summary>
    /// <typeparam name="TWriter">实现<see cref="IBytesWriter"/>接口的写入器类型。</typeparam>
    /// <param name="writer">字节写入器实例。</param>
    /// <param name="value">要写入的<see cref="uint"/>值。</param>
    /// <returns>写入的字节数。</returns>
    /// <remarks>
    /// 此方法实现了VarInt编码，每个字节的最高位作为继续位，
    /// 低7位作为数据位。当最高位为0时表示最后一个字节。最多需要5个字节。
    /// </remarks>
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