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
using System.IO;
using System.Text;

namespace TouchSocket.Core;

/// <summary>
/// 提供字节块扩展方法的静态类。
/// </summary>
/// <remarks>
/// 此类为字节块类型提供了丰富的扩展方法，包括类型转换、数据读写、数组操作等功能。
/// 支持与<see cref="ByteBlock"/>、<see cref="ValueByteBlock"/>等字节块类型的操作。
/// </remarks>
public static class ByteBlockExtension
{
    /// <summary>
    /// 将值类型的字节块转换为普通的字节块。
    /// </summary>
    /// <param name="valueByteBlock">要转换的值类型字节块。</param>
    /// <returns>一个新的<see cref="ByteBlock"/>对象。</returns>
    /// <remarks>
    /// 此方法会创建一个新的字节块对象，并复制值类型字节块的数据和状态。
    /// 包括位置、长度等信息都会被保留。
    /// </remarks>
    public static ByteBlock AsByteBlock(this in ValueByteBlock valueByteBlock)
    {
        var byteBlock = new ByteBlock(valueByteBlock.TotalMemory.Slice(0, valueByteBlock.Length));
        byteBlock.Position = valueByteBlock.Position;
        byteBlock.SetLength(valueByteBlock.Length);
        return byteBlock;
    }

    /// <summary>
    /// 将字节块转换为字节块流。
    /// </summary>
    /// <param name="byteBlock">要转换的字节块。</param>
    /// <param name="releaseTogether">是否在释放字节块时一起释放关联的资源，默认为<see langword="true"/>。</param>
    /// <returns>一个新的<see cref="Stream"/>对象，具体为<see cref="ByteBlockStream"/>实例。</returns>
    /// <remarks>
    /// 此方法将字节块包装为流对象，使其能够与标准的<see cref="Stream"/>API兼容。
    /// 当<paramref name="releaseTogether"/>为<see langword="true"/>时，流被释放时会同时释放底层的字节块。
    /// </remarks>
    public static Stream AsStream(this ByteBlock byteBlock, bool releaseTogether = true)
    {
        return new ByteBlockStream(byteBlock, releaseTogether);
    }

    #region ToArray

    /// <summary>
    /// 将指定的字节块转换为【新】字节数组。
    /// </summary>
    /// <typeparam name="TByteBlock">实现<see cref="IByteBlockCore"/>接口的字节块类型。</typeparam>
    /// <param name="byteBlock">字节块对象。</param>
    /// <param name="offset">起始偏移量。</param>
    /// <param name="length">要转换为数组的长度。</param>
    /// <returns>包含指定范围数据的【新】字节数组。</returns>
    /// <remarks>
    /// 此方法会创建一个新的字节数组，从字节块的指定位置复制数据。
    /// 原始字节块的内容不会被修改。
    /// </remarks>
    public static byte[] ToArray<TByteBlock>(this TByteBlock byteBlock, int offset, int length) where TByteBlock : IByteBlockCore
    {
        return byteBlock.Span.Slice(offset, length).ToArray();
    }

    /// <summary>
    /// 将指定的字节块转换为【新】字节数组，从指定偏移量开始，直到字节块的末尾。
    /// </summary>
    /// <typeparam name="TByteBlock">实现<see cref="IByteBlockCore"/>接口的字节块类型。</typeparam>
    /// <param name="byteBlock">字节块对象。</param>
    /// <param name="offset">起始偏移量。</param>
    /// <returns>从指定偏移量到字节块末尾的【新】字节数组。</returns>
    public static byte[] ToArray<TByteBlock>(this TByteBlock byteBlock, int offset) where TByteBlock : IByteBlockCore
    {
        return ToArray(byteBlock, offset, byteBlock.Length - offset);
    }

    /// <summary>
    /// 将指定的字节块转换为【新】字节数组，从索引0开始，直到字节块的末尾。
    /// </summary>
    /// <typeparam name="TByteBlock">实现<see cref="IByteBlockCore"/>接口的字节块类型。</typeparam>
    /// <param name="byteBlock">字节块对象。</param>
    /// <returns>整个字节块的【新】字节数组。</returns>
    public static byte[] ToArray<TByteBlock>(this TByteBlock byteBlock) where TByteBlock : IByteBlockCore
    {
        return ToArray(byteBlock, 0, byteBlock.Length);
    }

    /// <summary>
    /// 将指定的字节块从当前位置<see cref="IByteBlockCore.Position"/>转换为【新】字节数组，直到字节块的末尾。
    /// </summary>
    /// <typeparam name="TByteBlock">实现<see cref="IByteBlockReader"/>接口的字节块类型。</typeparam>
    /// <param name="byteBlock">字节块对象。</param>
    /// <returns>从当前位置到字节块末尾的【新】字节数组。</returns>
    /// <remarks>
    /// 此方法基于字节块的当前位置进行数组转换，适用于读取操作后获取剩余数据的场景。
    /// </remarks>
    public static byte[] ToArrayTake<TByteBlock>(this TByteBlock byteBlock) where TByteBlock : IByteBlockReader
    {
        return ToArray(byteBlock, byteBlock.Position, byteBlock.CanReadLength);
    }

    /// <summary>
    /// 将指定的字节块从当前位置<see cref="IByteBlockCore.Position"/>转换为【新】字节数组，指定长度。
    /// </summary>
    /// <typeparam name="TByteBlock">实现<see cref="IByteBlockCore"/>接口的字节块类型。</typeparam>
    /// <param name="byteBlock">字节块对象。</param>
    /// <param name="length">要转换为数组的长度。</param>
    /// <returns>从当前位置开始，指定长度的【新】字节数组。</returns>
    public static byte[] ToArrayTake<TByteBlock>(this TByteBlock byteBlock, int length) where TByteBlock : IByteBlockCore
    {
        return ToArray(byteBlock, byteBlock.Position, length);
    }

    #endregion ToArray

    #region Write

    /// <summary>
    /// 向字节块写入一个字节值。
    /// </summary>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">要写入的字节值。</param>
    public static void WriteByte(this ByteBlock byteBlock, byte value)
    {
        WriterExtension.WriteValue<ByteBlock, byte>(ref byteBlock, value);
    }

    /// <summary>
    /// 向字节块写入一个日期时间值。
    /// </summary>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">要写入的<see cref="DateTime"/>值。</param>
    public static void WriteDateTime(this ByteBlock byteBlock, DateTime value)
    {
        WriterExtension.WriteValue<ByteBlock, DateTime>(ref byteBlock, value);
    }

    /// <summary>
    /// 向字节块写入一个十进制数值。
    /// </summary>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">要写入的<see cref="decimal"/>值。</param>
    public static void WriteDecimal(this ByteBlock byteBlock, decimal value)
    {
        WriterExtension.WriteValue<ByteBlock, decimal>(ref byteBlock, value);
    }

    /// <summary>
    /// 向字节块写入一个十进制数值，指定字节序。
    /// </summary>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">要写入的<see cref="decimal"/>值。</param>
    /// <param name="endianType">字节序类型。</param>
    public static void WriteDecimal(this ByteBlock byteBlock, decimal value, EndianType endianType)
    {
        WriterExtension.WriteValue<ByteBlock, decimal>(ref byteBlock, value, endianType);
    }

    /// <summary>
    /// 向字节块写入一个双精度浮点数值。
    /// </summary>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">要写入的<see cref="double"/>值。</param>
    public static void WriteDouble(this ByteBlock byteBlock, double value)
    {
        WriterExtension.WriteValue<ByteBlock, double>(ref byteBlock, value);
    }

    /// <summary>
    /// 向字节块写入一个双精度浮点数值，指定字节序。
    /// </summary>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">要写入的<see cref="double"/>值。</param>
    /// <param name="endianType">字节序类型。</param>
    public static void WriteDouble(this ByteBlock byteBlock, double value, EndianType endianType)
    {
        WriterExtension.WriteValue<ByteBlock, double>(ref byteBlock, value, endianType);
    }

    /// <summary>
    /// 向字节块写入一个单精度浮点数值。
    /// </summary>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">要写入的<see cref="float"/>值。</param>
    public static void WriteFloat(this ByteBlock byteBlock, float value)
    {
        WriterExtension.WriteValue<ByteBlock, float>(ref byteBlock, value);
    }

    /// <summary>
    /// 向字节块写入一个单精度浮点数值，指定字节序。
    /// </summary>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">要写入的<see cref="float"/>值。</param>
    /// <param name="endianType">字节序类型。</param>
    public static void WriteFloat(this ByteBlock byteBlock, float value, EndianType endianType)
    {
        WriterExtension.WriteValue<ByteBlock, float>(ref byteBlock, value, endianType);
    }

    /// <summary>
    /// 向字节块写入一个全局唯一标识符。
    /// </summary>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">要写入的<see cref="Guid"/>值。</param>
    public static void WriteGuid(this ByteBlock byteBlock, Guid value)
    {
        WriterExtension.WriteValue<ByteBlock, Guid>(ref byteBlock, value);
    }

    /// <summary>
    /// 向字节块写入一个16位有符号整数。
    /// </summary>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">要写入的<see cref="short"/>值。</param>
    public static void WriteInt16(this ByteBlock byteBlock, short value)
    {
        WriterExtension.WriteValue<ByteBlock, short>(ref byteBlock, value);
    }

    /// <summary>
    /// 向字节块写入一个16位有符号整数，指定字节序。
    /// </summary>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">要写入的<see cref="short"/>值。</param>
    /// <param name="endianType">字节序类型。</param>
    public static void WriteInt16(this ByteBlock byteBlock, short value, EndianType endianType)
    {
        WriterExtension.WriteValue<ByteBlock, short>(ref byteBlock, value, endianType);
    }

    /// <summary>
    /// 向字节块写入一个32位有符号整数。
    /// </summary>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">要写入的<see cref="int"/>值。</param>
    public static void WriteInt32(this ByteBlock byteBlock, int value)
    {
        WriterExtension.WriteValue<ByteBlock, int>(ref byteBlock, value);
    }

    /// <summary>
    /// 向字节块写入一个32位有符号整数，指定字节序。
    /// </summary>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">要写入的<see cref="int"/>值。</param>
    /// <param name="endianType">字节序类型。</param>
    public static void WriteInt32(this ByteBlock byteBlock, int value, EndianType endianType)
    {
        WriterExtension.WriteValue<ByteBlock, int>(ref byteBlock, value, endianType);
    }

    /// <summary>
    /// 向字节块写入一个64位有符号整数。
    /// </summary>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">要写入的<see cref="long"/>值。</param>
    public static void WriteInt64(this ByteBlock byteBlock, long value)
    {
        WriterExtension.WriteValue<ByteBlock, long>(ref byteBlock, value);
    }

    /// <summary>
    /// 向字节块写入一个64位有符号整数，指定字节序。
    /// </summary>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">要写入的<see cref="long"/>值。</param>
    /// <param name="endianType">字节序类型。</param>
    public static void WriteInt64(this ByteBlock byteBlock, long value, EndianType endianType)
    {
        WriterExtension.WriteValue<ByteBlock, long>(ref byteBlock, value, endianType);
    }

    /// <summary>
    /// 向字节块写入一个8位有符号整数。
    /// </summary>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">要写入的<see cref="sbyte"/>值。</param>
    public static void WriteSByte(this ByteBlock byteBlock, sbyte value)
    {
        WriterExtension.WriteValue<ByteBlock, sbyte>(ref byteBlock, value);
    }

    /// <summary>
    /// 向字节块写入一个字符串。
    /// </summary>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">要写入的字符串值。</param>
    public static void WriteString(this ByteBlock byteBlock, string value)
    {
        WriterExtension.WriteString(ref byteBlock, value);
    }

    /// <summary>
    /// 向字节块写入一个字符串，使用指定的固定头部类型。
    /// </summary>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">要写入的字符串值。</param>
    /// <param name="headerType">固定头部类型，用于指示字符串长度的编码方式。</param>
    public static void WriteString(this ByteBlock byteBlock, string value, FixedHeaderType headerType)
    {
        WriterExtension.WriteString(ref byteBlock, value, headerType);
    }

    /// <summary>
    /// 向字节块写入一个时间跨度值。
    /// </summary>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">要写入的<see cref="TimeSpan"/>值。</param>
    public static void WriteTimeSpan(this ByteBlock byteBlock, TimeSpan value)
    {
        WriterExtension.WriteValue<ByteBlock, TimeSpan>(ref byteBlock, value);
    }

    /// <summary>
    /// 向字节块写入一个16位无符号整数。
    /// </summary>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">要写入的<see cref="ushort"/>值。</param>
    public static void WriteUInt16(this ByteBlock byteBlock, ushort value)
    {
        WriterExtension.WriteValue<ByteBlock, ushort>(ref byteBlock, value);
    }

    /// <summary>
    /// 向字节块写入一个16位无符号整数，指定字节序。
    /// </summary>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">要写入的<see cref="ushort"/>值。</param>
    /// <param name="endianType">字节序类型。</param>
    public static void WriteUInt16(this ByteBlock byteBlock, ushort value, EndianType endianType)
    {
        WriterExtension.WriteValue<ByteBlock, ushort>(ref byteBlock, value, endianType);
    }

    /// <summary>
    /// 向字节块写入一个32位无符号整数。
    /// </summary>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">要写入的<see cref="uint"/>值。</param>
    public static void WriteUInt32(this ByteBlock byteBlock, uint value)
    {
        WriterExtension.WriteValue<ByteBlock, uint>(ref byteBlock, value);
    }

    /// <summary>
    /// 向字节块写入一个32位无符号整数，指定字节序。
    /// </summary>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">要写入的<see cref="uint"/>值。</param>
    /// <param name="endianType">字节序类型。</param>
    public static void WriteUInt32(this ByteBlock byteBlock, uint value, EndianType endianType)
    {
        WriterExtension.WriteValue<ByteBlock, uint>(ref byteBlock, value, endianType);
    }

    /// <summary>
    /// 向字节块写入一个64位无符号整数。
    /// </summary>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">要写入的<see cref="ulong"/>值。</param>
    public static void WriteUInt64(this ByteBlock byteBlock, ulong value)
    {
        WriterExtension.WriteValue<ByteBlock, ulong>(ref byteBlock, value);
    }

    /// <summary>
    /// 向字节块写入一个64位无符号整数，指定字节序。
    /// </summary>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">要写入的<see cref="ulong"/>值。</param>
    /// <param name="endianType">字节序类型。</param>
    public static void WriteUInt64(this ByteBlock byteBlock, ulong value, EndianType endianType)
    {
        WriterExtension.WriteValue<ByteBlock, ulong>(ref byteBlock, value, endianType);
    }

    /// <summary>
    /// 向字节块写入引用类型对象的<see langword="null"/>状态标识。
    /// </summary>
    /// <typeparam name="T">引用类型。</typeparam>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="t">要检查<see langword="null"/>状态的对象。</param>
    public static void WriteIsNull<T>(this ByteBlock byteBlock, T t)
         where T : class
    {
        WriterExtension.WriteIsNull<ByteBlock, T>(ref byteBlock, t);
    }

    /// <summary>
    /// 向字节块写入值类型对象的<see langword="null"/>状态标识。
    /// </summary>
    /// <typeparam name="T">值类型。</typeparam>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="t">要检查<see langword="null"/>状态的可空值类型对象。</param>
    public static void WriteIsNull<T>(this ByteBlock byteBlock, T? t)
        where T : struct
    {
        WriterExtension.WriteIsNull<ByteBlock, T>(ref byteBlock, t);
    }

    /// <summary>
    /// 向字节块写入非<see langword="null"/>标识。
    /// </summary>
    /// <param name="byteBlock">要写入的字节块。</param>
    public static void WriteNotNull(this ByteBlock byteBlock)
    {
        WriterExtension.WriteNotNull<ByteBlock>(ref byteBlock);
    }

    /// <summary>
    /// 向字节块写入<see langword="null"/>标识。
    /// </summary>
    /// <param name="byteBlock">要写入的字节块。</param>
    public static void WriteNull(this ByteBlock byteBlock)
    {
        WriterExtension.WriteNull<ByteBlock>(ref byteBlock);
    }

    /// <summary>
    /// 向字节块写入一个可变长度32位无符号整数。
    /// </summary>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">要写入的<see cref="uint"/>值。</param>
    /// <returns>写入的字节数。</returns>
    public static int WriteVarUInt32(this ByteBlock byteBlock, uint value)
    {
        return WriterExtension.WriteVarUInt32<ByteBlock>(ref byteBlock, value);
    }

    /// <summary>
    /// 向字节块写入普通字符串，使用指定编码。
    /// </summary>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">要写入的字符串值。</param>
    /// <param name="encoding">字符串编码。</param>
    public static void WriteNormalString(this ByteBlock byteBlock, string value, Encoding encoding)
    {
        WriterExtension.WriteNormalString<ByteBlock>(ref byteBlock, value, encoding);
    }

    /// <summary>
    /// 向字节块写入字节跨度数据。
    /// </summary>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="span">要写入的字节跨度。</param>
    public static void WriteByteSpan(this ByteBlock byteBlock, scoped ReadOnlySpan<byte> span)
    {
        WriterExtension.WriteByteSpan<ByteBlock>(ref byteBlock, span);
    }

    /// <summary>
    /// 向字节块写入另一个字节块的数据。
    /// </summary>
    /// <param name="byteBlock">要写入的目标字节块。</param>
    /// <param name="value">要写入的源字节块。</param>
    public static void WriteByteBlock(this ByteBlock byteBlock, ByteBlock value)
    {
        WriterExtension.WriteByteBlock<ByteBlock>(ref byteBlock, value);
    }

    /// <summary>
    /// 向字节块写入一个数据包对象。
    /// </summary>
    /// <typeparam name="TPackage">数据包类型，必须实现<see cref="IPackage"/>接口。</typeparam>
    /// <param name="byteBlock">要写入的字节块。</param>
    /// <param name="value">要写入的数据包对象。</param>
    public static void WritePackage<TPackage>(this ByteBlock byteBlock, TPackage value)
        where TPackage : class, IPackage
    {
        WriterExtension.WritePackage(ref byteBlock, value);
    }
    #endregion Write

    #region Read

    /// <summary>
    /// 从字节块读取一个字节值。
    /// </summary>
    /// <param name="byteBlock">要读取的字节块。</param>
    /// <returns>读取的字节值。</returns>
    public static byte ReadByte(this ByteBlock byteBlock)
    {
        return ReaderExtension.ReadValue<ByteBlock, byte>(ref byteBlock);
    }

    /// <summary>
    /// 从字节块读取一个日期时间值。
    /// </summary>
    /// <param name="byteBlock">要读取的字节块。</param>
    /// <returns>读取的<see cref="DateTime"/>值。</returns>
    public static DateTime ReadDateTime(this ByteBlock byteBlock)
    {
        return ReaderExtension.ReadValue<ByteBlock, DateTime>(ref byteBlock);
    }

    /// <summary>
    /// 从字节块读取一个十进制数值。
    /// </summary>
    /// <param name="byteBlock">要读取的字节块。</param>
    /// <returns>读取的<see cref="decimal"/>值。</returns>
    public static decimal ReadDecimal(this ByteBlock byteBlock)
    {
        return ReaderExtension.ReadValue<ByteBlock, decimal>(ref byteBlock);
    }

    /// <summary>
    /// 从字节块读取一个十进制数值，指定字节序。
    /// </summary>
    /// <param name="byteBlock">要读取的字节块。</param>
    /// <param name="endianType">字节序类型。</param>
    /// <returns>读取的<see cref="decimal"/>值。</returns>
    public static decimal ReadDecimal(this ByteBlock byteBlock, EndianType endianType)
    {
        return ReaderExtension.ReadValue<ByteBlock, decimal>(ref byteBlock, endianType);
    }

    /// <summary>
    /// 从字节块读取一个双精度浮点数值。
    /// </summary>
    /// <param name="byteBlock">要读取的字节块。</param>
    /// <returns>读取的<see cref="double"/>值。</returns>
    public static double ReadDouble(this ByteBlock byteBlock)
    {
        return ReaderExtension.ReadValue<ByteBlock, double>(ref byteBlock);
    }

    /// <summary>
    /// 从字节块读取一个双精度浮点数值，指定字节序。
    /// </summary>
    /// <param name="byteBlock">要读取的字节块。</param>
    /// <param name="endianType">字节序类型。</param>
    /// <returns>读取的<see cref="double"/>值。</returns>
    public static double ReadDouble(this ByteBlock byteBlock, EndianType endianType)
    {
        return ReaderExtension.ReadValue<ByteBlock, double>(ref byteBlock, endianType);
    }

    /// <summary>
    /// 从字节块读取一个单精度浮点数值。
    /// </summary>
    /// <param name="byteBlock">要读取的字节块。</param>
    /// <returns>读取的<see cref="float"/>值。</returns>
    public static float ReadFloat(this ByteBlock byteBlock)
    {
        return ReaderExtension.ReadValue<ByteBlock, float>(ref byteBlock);
    }

    /// <summary>
    /// 从字节块读取一个单精度浮点数值，指定字节序。
    /// </summary>
    /// <param name="byteBlock">要读取的字节块。</param>
    /// <param name="endianType">字节序类型。</param>
    /// <returns>读取的<see cref="float"/>值。</returns>
    public static float ReadFloat(this ByteBlock byteBlock, EndianType endianType)
    {
        return ReaderExtension.ReadValue<ByteBlock, float>(ref byteBlock, endianType);
    }

    /// <summary>
    /// 从字节块读取一个全局唯一标识符。
    /// </summary>
    /// <param name="byteBlock">要读取的字节块。</param>
    /// <returns>读取的<see cref="Guid"/>值。</returns>
    public static Guid ReadGuid(this ByteBlock byteBlock)
    {
        return ReaderExtension.ReadValue<ByteBlock, Guid>(ref byteBlock);
    }

    /// <summary>
    /// 从字节块读取一个16位有符号整数。
    /// </summary>
    /// <param name="byteBlock">要读取的字节块。</param>
    /// <returns>读取的<see cref="short"/>值。</returns>
    public static short ReadInt16(this ByteBlock byteBlock)
    {
        return ReaderExtension.ReadValue<ByteBlock, short>(ref byteBlock);
    }

    /// <summary>
    /// 从字节块读取一个16位有符号整数，指定字节序。
    /// </summary>
    /// <param name="byteBlock">要读取的字节块。</param>
    /// <param name="endianType">字节序类型。</param>
    /// <returns>读取的<see cref="short"/>值。</returns>
    public static short ReadInt16(this ByteBlock byteBlock, EndianType endianType)
    {
        return ReaderExtension.ReadValue<ByteBlock, short>(ref byteBlock, endianType);
    }

    /// <summary>
    /// 从字节块读取一个32位有符号整数。
    /// </summary>
    /// <param name="byteBlock">要读取的字节块。</param>
    /// <returns>读取的<see cref="int"/>值。</returns>
    public static int ReadInt32(this ByteBlock byteBlock)
    {
        return ReaderExtension.ReadValue<ByteBlock, int>(ref byteBlock);
    }

    /// <summary>
    /// 从字节块读取一个32位有符号整数，指定字节序。
    /// </summary>
    /// <param name="byteBlock">要读取的字节块。</param>
    /// <param name="endianType">字节序类型。</param>
    /// <returns>读取的<see cref="int"/>值。</returns>
    public static int ReadInt32(this ByteBlock byteBlock, EndianType endianType)
    {
        return ReaderExtension.ReadValue<ByteBlock, int>(ref byteBlock, endianType);
    }

    /// <summary>
    /// 从字节块读取一个64位有符号整数。
    /// </summary>
    /// <param name="byteBlock">要读取的字节块。</param>
    /// <returns>读取的<see cref="long"/>值。</returns>
    public static long ReadInt64(this ByteBlock byteBlock)
    {
        return ReaderExtension.ReadValue<ByteBlock, long>(ref byteBlock);
    }

    /// <summary>
    /// 从字节块读取一个64位有符号整数，指定字节序。
    /// </summary>
    /// <param name="byteBlock">要读取的字节块。</param>
    /// <param name="endianType">字节序类型。</param>
    /// <returns>读取的<see cref="long"/>值。</returns>
    public static long ReadInt64(this ByteBlock byteBlock, EndianType endianType)
    {
        return ReaderExtension.ReadValue<ByteBlock, long>(ref byteBlock, endianType);
    }

    /// <summary>
    /// 从字节块读取一个8位有符号整数。
    /// </summary>
    /// <param name="byteBlock">要读取的字节块。</param>
    /// <returns>读取的<see cref="sbyte"/>值。</returns>
    public static sbyte ReadSByte(this ByteBlock byteBlock)
    {
        return ReaderExtension.ReadValue<ByteBlock, sbyte>(ref byteBlock);
    }

    /// <summary>
    /// 从字节块读取一个字符串。
    /// </summary>
    /// <param name="byteBlock">要读取的字节块。</param>
    /// <returns>读取的字符串值。</returns>
    public static string ReadString(this ByteBlock byteBlock)
    {
        return ReaderExtension.ReadString(ref byteBlock);
    }

    /// <summary>
    /// 从字节块读取一个字符串，使用指定的固定头部类型。
    /// </summary>
    /// <param name="byteBlock">要读取的字节块。</param>
    /// <param name="headerType">固定头部类型，用于指示字符串长度的编码方式。</param>
    /// <returns>读取的字符串值。</returns>
    public static string ReadString(this ByteBlock byteBlock, FixedHeaderType headerType)
    {
        return ReaderExtension.ReadString(ref byteBlock, headerType);
    }

    /// <summary>
    /// 从字节块读取一个时间跨度值。
    /// </summary>
    /// <param name="byteBlock">要读取的字节块。</param>
    /// <returns>读取的<see cref="TimeSpan"/>值。</returns>
    public static TimeSpan ReadTimeSpan(this ByteBlock byteBlock)
    {
        return ReaderExtension.ReadValue<ByteBlock, TimeSpan>(ref byteBlock);
    }

    /// <summary>
    /// 从字节块读取一个16位无符号整数。
    /// </summary>
    /// <param name="byteBlock">要读取的字节块。</param>
    /// <returns>读取的<see cref="ushort"/>值。</returns>
    public static ushort ReadUInt16(this ByteBlock byteBlock)
    {
        return ReaderExtension.ReadValue<ByteBlock, ushort>(ref byteBlock);
    }

    /// <summary>
    /// 从字节块读取一个16位无符号整数，指定字节序。
    /// </summary>
    /// <param name="byteBlock">要读取的字节块。</param>
    /// <param name="endianType">字节序类型。</param>
    /// <returns>读取的<see cref="ushort"/>值。</returns>
    public static ushort ReadUInt16(this ByteBlock byteBlock, EndianType endianType)
    {
        return ReaderExtension.ReadValue<ByteBlock, ushort>(ref byteBlock, endianType);
    }

    /// <summary>
    /// 从字节块读取一个32位无符号整数。
    /// </summary>
    /// <param name="byteBlock">要读取的字节块。</param>
    /// <returns>读取的<see cref="uint"/>值。</returns>
    public static uint ReadUInt32(this ByteBlock byteBlock)
    {
        return ReaderExtension.ReadValue<ByteBlock, uint>(ref byteBlock);
    }

    /// <summary>
    /// 从字节块读取一个32位无符号整数，指定字节序。
    /// </summary>
    /// <param name="byteBlock">要读取的字节块。</param>
    /// <param name="endianType">字节序类型。</param>
    /// <returns>读取的<see cref="uint"/>值。</returns>
    public static uint ReadUInt32(this ByteBlock byteBlock, EndianType endianType)
    {
        return ReaderExtension.ReadValue<ByteBlock, uint>(ref byteBlock, endianType);
    }

    /// <summary>
    /// 从字节块读取一个64位无符号整数。
    /// </summary>
    /// <param name="byteBlock">要读取的字节块。</param>
    /// <returns>读取的<see cref="ulong"/>值。</returns>
    public static ulong ReadUInt64(this ByteBlock byteBlock)
    {
        return ReaderExtension.ReadValue<ByteBlock, ulong>(ref byteBlock);
    }

    /// <summary>
    /// 从字节块读取一个64位无符号整数，指定字节序。
    /// </summary>
    /// <param name="byteBlock">要读取的字节块。</param>
    /// <param name="endianType">字节序类型。</param>
    /// <returns>读取的<see cref="ulong"/>值。</returns>
    public static ulong ReadUInt64(this ByteBlock byteBlock, EndianType endianType)
    {
        return ReaderExtension.ReadValue<ByteBlock, ulong>(ref byteBlock, endianType);
    }

    /// <summary>
    /// 从字节块读取<see langword="null"/>状态标识。
    /// </summary>
    /// <param name="byteBlock">要读取的字节块。</param>
    /// <returns>如果读取到<see langword="null"/>标识则返回<see langword="true"/>；否则返回<see langword="false"/>。</returns>
    public static bool ReadIsNull(this ByteBlock byteBlock)
    {
        return ReaderExtension.ReadIsNull<ByteBlock>(ref byteBlock);
    }

    /// <summary>
    /// 从字节块读取一个可变长度32位无符号整数。
    /// </summary>
    /// <param name="byteBlock">要读取的字节块。</param>
    /// <returns>读取的<see cref="uint"/>值。</returns>
    public static uint ReadVarUInt32(this ByteBlock byteBlock)
    {
        return ReaderExtension.ReadVarUInt32<ByteBlock>(ref byteBlock);
    }

    /// <summary>
    /// 从字节块读取一个字节块对象。
    /// </summary>
    /// <param name="byteBlock">要读取的字节块。</param>
    /// <returns>读取的<see cref="ByteBlock"/>对象。</returns>
    public static ByteBlock ReadByteBlock(this ByteBlock byteBlock)
    {
        return ReaderExtension.ReadByteBlock(ref byteBlock);
    }

    /// <summary>
    /// 从字节块读取一个数据包对象。
    /// </summary>
    /// <typeparam name="TPackage">数据包类型，必须实现<see cref="IPackage"/>接口并具有无参构造函数。</typeparam>
    /// <param name="byteBlock">要读取的字节块。</param>
    /// <returns>读取的数据包对象。</returns>
    public static TPackage ReadPackage<TPackage>(this ByteBlock byteBlock)
        where TPackage : class, IPackage, new()
    {
        return ReaderExtension.ReadPackage<ByteBlock, TPackage>(ref byteBlock);
    }
    #endregion Read
}