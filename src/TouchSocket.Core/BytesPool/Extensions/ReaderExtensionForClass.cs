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
/// 为引用类型字节读取器提供扩展方法的静态类。
/// </summary>
/// <remarks>
/// 此类包含专门针对引用类型字节读取器的扩展方法，提供高效的数据读取功能。
/// 支持各种数据类型的读取操作，包括基本类型、字符串、字节块等。
/// </remarks>
public static partial class ReaderExtension
{
    /// <summary>
    /// 从字节读取器中读取指定长度的数据到只读字节跨度。
    /// </summary>
    /// <typeparam name="TReader">实现<see cref="IBytesReader"/>接口的字节读取器类型，必须为引用类型。</typeparam>
    /// <param name="reader">要读取数据的字节读取器。</param>
    /// <param name="length">要读取的数据长度。</param>
    /// <returns>包含读取数据的只读字节跨度。</returns>
    /// <remarks>
    /// 此方法从读取器的当前位置读取指定长度的数据，并自动推进读取器的位置。
    /// 返回的跨度引用读取器的内部缓冲区，因此其生命周期与读取器相关。
    /// </remarks>
    public static ReadOnlySpan<byte> ReadToSpan<TReader>(this TReader reader, int length)
        where TReader : class, IBytesReader
    {
        var span = reader.GetSpan(length).Slice(0, length);
        reader.Advance(length);
        return span;
    }

    /// <summary>
    /// 从字节读取器中读取一个字节块对象。
    /// </summary>
    /// <typeparam name="TReader">实现<see cref="IBytesReader"/>接口的字节读取器类型，必须为引用类型。</typeparam>
    /// <param name="reader">要读取数据的字节读取器。</param>
    /// <returns>读取的<see cref="ByteBlock"/>对象，如果长度小于0则返回默认值。</returns>
    /// <remarks>
    /// 此方法首先读取可变长度的32位无符号整数作为长度信息，然后读取相应长度的数据创建字节块。
    /// 创建的字节块会自动设置到起始位置。
    /// </remarks>
    public static ByteBlock ReadByteBlock<TReader>(this TReader reader)
        where TReader : class, IBytesReader
    {
        var len = (int)ReadVarUInt32(reader) - 1;

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
    /// 从字节读取器中读取一个可变长度32位无符号整数。
    /// </summary>
    /// <typeparam name="TReader">实现<see cref="IBytesReader"/>接口的字节读取器类型，必须为引用类型。</typeparam>
    /// <param name="reader">要读取数据的字节读取器。</param>
    /// <returns>读取的32位无符号整数值。</returns>
    /// <remarks>
    /// 此方法使用变长编码格式读取32位无符号整数，每个字节的最高位用作继续标志。
    /// 当字节值小于等于0x7F时表示这是最后一个字节。
    /// </remarks>
    public static uint ReadVarUInt32<TReader>(this TReader reader)
        where TReader : class, IBytesReader
    {
        uint value = 0;
        var byteLength = 0;
        while (true)
        {
            var b = ReadValue<TReader, byte>(reader);
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
    /// 从字节读取器中读取字节跨度数据。
    /// </summary>
    /// <typeparam name="TReader">实现<see cref="IBytesReader"/>接口的字节读取器类型，必须为引用类型。</typeparam>
    /// <param name="reader">要读取数据的字节读取器。</param>
    /// <returns>读取的只读字节跨度。</returns>
    /// <remarks>
    /// 此方法首先读取可变长度32位无符号整数作为数据长度，然后读取相应长度的字节数据。
    /// 返回的跨度引用读取器的内部缓冲区。
    /// </remarks>
    public static ReadOnlySpan<byte> ReadByteSpan<TReader>(this TReader reader)
        where TReader : class, IBytesReader
    {
        var length = (int)ReadVarUInt32(reader);
        var span = reader.GetSpan(length).Slice(0, length);
        reader.Advance(length);
        return span;
    }

    /// <summary>
    /// 从字节读取器中读取<see langword="null"/>状态标识。
    /// </summary>
    /// <typeparam name="TReader">实现<see cref="IBytesReader"/>接口的字节读取器类型，必须为引用类型。</typeparam>
    /// <param name="reader">要读取数据的字节读取器。</param>
    /// <returns>如果读取到<see langword="null"/>标识则返回<see langword="true"/>；如果读取到非<see langword="null"/>标识则返回<see langword="false"/>。</returns>
    /// <exception cref="Exception">当读取的标识既非<see langword="null"/>也非非<see langword="null"/>时抛出异常。</exception>
    /// <remarks>
    /// 此方法读取一个字节作为<see langword="null"/>状态标识，0表示<see langword="null"/>，1表示非<see langword="null"/>。
    /// 如果读取到的值不是0或1，则认为流位置发生了错误。
    /// </remarks>
    public static bool ReadIsNull<TReader>(this TReader reader)
        where TReader : class, IBytesReader
    {
        var status = ReadValue<TReader, byte>(reader);
        return status == 0 || (status == 1 ? false : throw new Exception("标识既非Null，也非NotNull，可能是流位置发生了错误。"));
    }

    /// <summary>
    /// 从字节读取器中读取指定类型的值，使用指定的字节序。
    /// </summary>
    /// <typeparam name="TReader">实现<see cref="IBytesReader"/>接口的字节读取器类型，必须为引用类型。</typeparam>
    /// <typeparam name="T">要读取的值类型，必须为非托管类型。</typeparam>
    /// <param name="reader">要读取数据的字节读取器。</param>
    /// <param name="endianType">字节序类型。</param>
    /// <returns>读取的值。</returns>
    /// <remarks>
    /// 此方法根据类型的大小从读取器中读取相应字节数的数据，并根据指定的字节序进行转换。
    /// </remarks>
    public static T ReadValue<TReader, T>(this TReader reader, EndianType endianType)
        where T : unmanaged
        where TReader : class, IBytesReader
    {
        var size = Unsafe.SizeOf<T>();
        var span = reader.GetSpan(size);
        var value = TouchSocketBitConverter.GetBitConverter(endianType).To<T>(span);
        reader.Advance(size);
        return value;
    }

    /// <summary>
    /// 从字节读取器中读取指定类型的值，使用默认字节序。
    /// </summary>
    /// <typeparam name="TReader">实现<see cref="IBytesReader"/>接口的字节读取器类型，必须为引用类型。</typeparam>
    /// <typeparam name="T">要读取的值类型，必须为非托管类型。</typeparam>
    /// <param name="reader">要读取数据的字节读取器。</param>
    /// <returns>读取的值。</returns>
    /// <remarks>
    /// 此方法根据类型的大小从读取器中读取相应字节数的数据，并使用默认字节序进行转换。
    /// </remarks>
    public static T ReadValue<TReader, T>(this TReader reader)
        where T : unmanaged
         where TReader : class, IBytesReader
    {
        var size = Unsafe.SizeOf<T>();
        var span = reader.GetSpan(size);
        var value = TouchSocketBitConverter.Default.To<T>(span);
        reader.Advance(size);
        return value;
    }

    /// <summary>
    /// 从字节读取器中读取字符串，使用指定的固定头部类型。
    /// </summary>
    /// <typeparam name="TReader">实现<see cref="IBytesReader"/>接口的字节读取器类型，必须为引用类型。</typeparam>
    /// <param name="reader">要读取数据的字节读取器。</param>
    /// <param name="headerType">固定头部类型，默认为<see cref="FixedHeaderType.Int"/>。</param>
    /// <returns>读取的字符串，如果长度为对应类型的最大值则返回<see langword="null"/>。</returns>
    /// <remarks>
    /// 此方法根据指定的头部类型读取长度信息，然后读取相应长度的UTF-8编码字符串数据。
    /// 当长度为对应数据类型的最大值时，表示<see langword="null"/>字符串。
    /// </remarks>
    public static string ReadString<TReader>(this TReader reader, FixedHeaderType headerType = FixedHeaderType.Int)
        where TReader : class, IBytesReader
    {
        int len;
        switch (headerType)
        {
            case FixedHeaderType.Byte:
                len = ReadValue<TReader, byte>(reader);
                if (len == byte.MaxValue)
                {
                    return null;
                }
                break;
            case FixedHeaderType.Ushort:
                len = ReadValue<TReader, ushort>(reader);
                if (len == ushort.MaxValue)
                {
                    return null;
                }
                break;
            case FixedHeaderType.Int:
            default:
                len = ReadValue<TReader, int>(reader);
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
}
