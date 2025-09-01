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

/// <summary>
/// 为<see cref="IBytesReader"/>提供扩展方法的静态类，用于读取各种类型的数据。
/// </summary>
/// <remarks>
/// 此类提供了丰富的扩展方法，支持读取基本数据类型、字符串、字节块、枚举等。
/// 所有方法都是针对实现了<see cref="IBytesReader"/>接口的类型进行扩展。
/// </remarks>
public static partial class ReaderExtension
{
    /// <summary>
    /// 在字节读取器中查找指定字节序列的位置。
    /// </summary>
    /// <typeparam name="TReader">实现<see cref="IBytesReader"/>接口的读取器类型。</typeparam>
    /// <param name="reader">字节读取器实例。</param>
    /// <param name="span">要查找的字节序列。</param>
    /// <returns>如果找到，则返回第一次出现的位置；否则返回-1。</returns>
    public static long IndexOf<TReader>(ref TReader reader, ReadOnlySpan<byte> span)
        where TReader : IBytesReader
    {
        var sequence = reader.TotalSequence.Slice(reader.BytesRead);
        return sequence.IndexOf(span);
    }

    /// <summary>
    /// 从字节读取器中读取一个<see cref="ByteBlock"/>对象。
    /// </summary>
    /// <typeparam name="TReader">实现<see cref="IBytesReader"/>接口的读取器类型。</typeparam>
    /// <param name="reader">字节读取器实例。</param>
    /// <returns>读取的<see cref="ByteBlock"/>对象，如果数据为空则返回默认值。</returns>
    /// <remarks>
    /// 此方法首先读取长度信息，然后根据长度创建<see cref="ByteBlock"/>并填充数据。
    /// </remarks>
    public static ByteBlock ReadByteBlock<TReader>(ref TReader reader)
        where TReader : IBytesReader
    {
        var len = (int)ReadVarUInt32(ref reader) - 1;

        if (len < 0)
        {
            return default;
        }

        var byteBlock = new ByteBlock(len);
        byteBlock.Write(reader.GetSpan(len).Slice(0, len));
        byteBlock.SeekToStart();
        reader.Advance(len);
        return byteBlock;
    }

    /// <summary>
    /// 从字节读取器中读取一个字节跨度。
    /// </summary>
    /// <typeparam name="TReader">实现<see cref="IBytesReader"/>接口的读取器类型。</typeparam>
    /// <param name="reader">字节读取器实例。</param>
    /// <returns>读取的只读字节跨度。</returns>
    /// <remarks>
    /// 此方法首先读取长度信息，然后返回对应长度的字节跨度。
    /// </remarks>
    public static ReadOnlySpan<byte> ReadByteSpan<TReader>(ref TReader reader)
        where TReader : IBytesReader
    {
        var length = (int)ReadVarUInt32(ref reader);
        var span = reader.GetSpan(length).Slice(0, length);
        reader.Advance(length);
        return span;
    }

    /// <summary>
    /// 从字节读取器中读取指定类型的枚举值。
    /// </summary>
    /// <typeparam name="TReader">实现<see cref="IBytesReader"/>接口的读取器类型。</typeparam>
    /// <param name="reader">字节读取器实例。</param>
    /// <param name="enumType">枚举的<see cref="Type"/>。</param>
    /// <returns>读取的枚举值。</returns>
    /// <exception cref="NotSupportedException">当枚举的底层类型不受支持时抛出。</exception>
    /// <remarks>
    /// 支持的底层类型包括：<see cref="byte"/>、<see cref="sbyte"/>、<see cref="short"/>、
    /// <see cref="ushort"/>、<see cref="int"/>、<see cref="uint"/>、<see cref="long"/>、<see cref="ulong"/>。
    /// </remarks>
    public static Enum ReadEnum<TReader>(ref TReader reader, Type enumType)
       where TReader : IBytesReader
    {
        var underlyingType = Enum.GetUnderlyingType(enumType);
        if (underlyingType == typeof(byte))
        {
            var value = ReadValue<TReader, byte>(ref reader);
            return (Enum)Enum.ToObject(enumType, value);
        }
        else if (underlyingType == typeof(sbyte))
        {
            var value = ReadValue<TReader, sbyte>(ref reader);
            return (Enum)Enum.ToObject(enumType, value);
        }
        else if (underlyingType == typeof(short))
        {
            var value = ReadValue<TReader, short>(ref reader);
            return (Enum)Enum.ToObject(enumType, value);
        }
        else if (underlyingType == typeof(ushort))
        {
            var value = ReadValue<TReader, ushort>(ref reader);
            return (Enum)Enum.ToObject(enumType, value);
        }
        else if (underlyingType == typeof(int))
        {
            var value = ReadValue<TReader, int>(ref reader);
            return (Enum)Enum.ToObject(enumType, value);
        }
        else if (underlyingType == typeof(uint))
        {
            var value = ReadValue<TReader, uint>(ref reader);
            return (Enum)Enum.ToObject(enumType, value);
        }
        else if (underlyingType == typeof(long))
        {
            var value = ReadValue<TReader, long>(ref reader);
            return (Enum)Enum.ToObject(enumType, value);
        }
        else if (underlyingType == typeof(ulong))
        {
            var value = ReadValue<TReader, ulong>(ref reader);
            return (Enum)Enum.ToObject(enumType, value);
        }
        else
        {
            throw new NotSupportedException($"不支持枚举的底层类型{underlyingType}。");
        }
    }

    /// <summary>
    /// 从字节读取器中读取空值标识。
    /// </summary>
    /// <typeparam name="TReader">实现<see cref="IBytesReader"/>接口的读取器类型。</typeparam>
    /// <param name="reader">字节读取器实例。</param>
    /// <returns>如果值为空则返回 <see langword="true"/>；否则返回 <see langword="false"/>。</returns>
    /// <exception cref="Exception">当标识既非Null也非NotNull时抛出。</exception>
    /// <remarks>
    /// 此方法读取一个字节作为空值标识，0表示空值，1表示非空值。
    /// </remarks>
    public static bool ReadIsNull<TReader>(ref TReader reader)
        where TReader : IBytesReader
    {
        var status = ReadValue<TReader, byte>(ref reader);
        return status == 0 || (status == 1 ? false : throw new Exception("标识既非Null，也非NotNull，可能是流位置发生了错误。"));
    }

    /// <summary>
    /// 从字节读取器中读取一个包对象。
    /// </summary>
    /// <typeparam name="TReader">实现<see cref="IBytesReader"/>接口的读取器类型。</typeparam>
    /// <typeparam name="TPackage">实现<see cref="IPackage"/>接口的包类型。</typeparam>
    /// <param name="reader">字节读取器实例。</param>
    /// <returns>读取的包对象，如果为空则返回默认值。</returns>
    /// <remarks>
    /// 此方法首先检查空值标识，如果不为空则创建包对象并调用其Unpackage方法进行反序列化。
    /// </remarks>
    public static TPackage ReadPackage<TReader, TPackage>(ref TReader reader)
        where TReader : IBytesReader
        where TPackage : class, IPackage, new()
    {
        if (ReadIsNull(ref reader))
        {
            return default;
        }
        else
        {
            var package = new TPackage();
            package.Unpackage(ref reader);
            return package;
        }
    }

    /// <summary>
    /// 从字节读取器中读取UTF-8编码的字符串。
    /// </summary>
    /// <typeparam name="TReader">实现<see cref="IBytesReader"/>接口的读取器类型。</typeparam>
    /// <param name="reader">字节读取器实例。</param>
    /// <param name="headerType">固定头部类型，指定长度字段的大小。默认为<see cref="FixedHeaderType.Int"/>。</param>
    /// <returns>读取的字符串，如果为空则返回 <see langword="null"/>。</returns>
    /// <remarks>
    /// 此方法根据指定的头部类型读取长度信息，然后读取对应长度的字节并转换为UTF-8字符串。
    /// 当长度字段为最大值时，表示字符串为空。
    /// </remarks>
    public static string ReadString<TReader>(ref TReader reader, FixedHeaderType headerType = FixedHeaderType.Int)
        where TReader : IBytesReader
    {
        int len;
        switch (headerType)
        {
            case FixedHeaderType.Byte:
                len = ReadValue<TReader, byte>(ref reader);
                if (len == byte.MaxValue)
                {
                    return null;
                }
                break;

            case FixedHeaderType.Ushort:
                len = ReadValue<TReader, ushort>(ref reader);
                if (len == ushort.MaxValue)
                {
                    return null;
                }
                break;

            case FixedHeaderType.Int:
            default:
                len = ReadValue<TReader, int>(ref reader);
                if (len == int.MaxValue)
                {
                    return null;
                }
                break;
        }

        var span = reader.GetSpan(len).Slice(0, len);
        var str = span.ToString(Encoding.UTF8);
        reader.Advance(len);
        return str;
    }

    /// <summary>
    /// 从字节读取器中读取指定长度的字节跨度。
    /// </summary>
    /// <typeparam name="TReader">实现<see cref="IBytesReader"/>接口的读取器类型。</typeparam>
    /// <param name="reader">字节读取器实例。</param>
    /// <param name="length">要读取的字节长度。</param>
    /// <returns>读取的只读字节跨度。</returns>
    /// <remarks>
    /// 此方法读取指定长度的字节并推进读取器位置。
    /// </remarks>
    public static ReadOnlySpan<byte> ReadToSpan<TReader>(ref TReader reader, int length)
                            where TReader : IBytesReader
    {
        var span = reader.GetSpan(length).Slice(0, length);
        reader.Advance(length);
        return span;
    }

    /// <summary>
    /// 从字节读取器中读取指定类型的值，使用指定的字节序。
    /// </summary>
    /// <typeparam name="TReader">实现<see cref="IBytesReader"/>接口的读取器类型。</typeparam>
    /// <typeparam name="T">要读取的值类型，必须是非托管类型。</typeparam>
    /// <param name="reader">字节读取器实例。</param>
    /// <param name="endianType">字节序类型。</param>
    /// <returns>读取的值。</returns>
    /// <remarks>
    /// 此方法使用指定的字节序转换器读取值，并推进读取器位置。
    /// </remarks>
    public static T ReadValue<TReader, T>(ref TReader reader, EndianType endianType)
        where T : unmanaged
        where TReader : IBytesReader
    {
        var size = Unsafe.SizeOf<T>();
        var span = reader.GetSpan(size);
        var value = TouchSocketBitConverter.GetBitConverter(endianType).To<T>(span);
        reader.Advance(size);
        return value;
    }

    /// <summary>
    /// 从字节读取器中读取指定类型的值，使用默认的字节序。
    /// </summary>
    /// <typeparam name="TReader">实现<see cref="IBytesReader"/>接口的读取器类型。</typeparam>
    /// <typeparam name="T">要读取的值类型，必须是非托管类型。</typeparam>
    /// <param name="reader">字节读取器实例。</param>
    /// <returns>读取的值。</returns>
    /// <remarks>
    /// 此方法使用默认的字节序转换器读取值，并推进读取器位置。
    /// </remarks>
    public static T ReadValue<TReader, T>(ref TReader reader)
        where T : unmanaged
         where TReader : IBytesReader
    {
        var size = Unsafe.SizeOf<T>();
        var span = reader.GetSpan(size);
        var value = TouchSocketBitConverter.Default.To<T>(span);
        reader.Advance(size);
        return value;
    }

    /// <summary>
    /// 从字节读取器中读取可变长度编码的无符号32位整数。
    /// </summary>
    /// <typeparam name="TReader">实现<see cref="IBytesReader"/>接口的读取器类型。</typeparam>
    /// <param name="reader">字节读取器实例。</param>
    /// <returns>读取的<see cref="uint"/>值。</returns>
    /// <remarks>
    /// 此方法实现了VarInt编码的解码，每个字节的最高位作为继续位，
    /// 低7位作为数据位。当最高位为0时表示最后一个字节。
    /// </remarks>
    public static uint ReadVarUInt32<TReader>(ref TReader reader)
                where TReader : IBytesReader
    {
        uint value = 0;
        var byteLength = 0;
        while (true)
        {
            var b = ReadValue<TReader, byte>(ref reader);
            var temp = (b & 0x7F); //取每个字节的后7位
            temp <<= (7 * byteLength); //向左移位，越是后面的字节，移位越多
            value += (uint)temp; //把每个字节的值加起来就是最终的值了
            byteLength++;
            if (b <= 0x7F)
            { //127=0x7F=0b01111111，小于等于说明msb=0，即最后一个字节
                break;
            }
        }
        return value;
    }

    /// <summary>
    /// 将字节读取器的位置设置到数据末尾。
    /// </summary>
    /// <typeparam name="TReader">实现<see cref="IBytesReader"/>接口的读取器类型。</typeparam>
    /// <param name="reader">字节读取器实例。</param>
    /// <remarks>
    /// 此方法将读取器的位置设置为总序列的长度，即数据的末尾位置。
    /// </remarks>
    public static void SeekToEnd<TReader>(ref TReader reader)
        where TReader : IBytesReader
    {
        reader.BytesRead = reader.TotalSequence.Length;
    }

    /// <summary>
    /// 将字节读取器的位置设置到数据开头。
    /// </summary>
    /// <typeparam name="TReader">实现<see cref="IBytesReader"/>接口的读取器类型。</typeparam>
    /// <param name="reader">字节读取器实例。</param>
    /// <remarks>
    /// 此方法将读取器的位置重置为0，即数据的开头位置。
    /// </remarks>
    public static void SeekToStart<TReader>(ref TReader reader)
                                                        where TReader : IBytesReader
    {
        reader.BytesRead = 0;
    }
}