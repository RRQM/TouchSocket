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
// --- -------------------------------------------------------------------------

using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace TouchSocket.Core;

/// <summary>
/// 为<see cref="Span{T}"/>和<see cref="ReadOnlySpan{T}"/>提供扩展方法的静态类，用于读写各种类型的数据。
/// </summary>
/// <remarks>
/// 此类提供了针对字节跨度的高性能读写操作，支持基本数据类型、字符串等的序列化和反序列化。
/// 所有方法都会自动推进跨度的位置，确保连续的读写操作。
/// </remarks>
public static class SpanExtension
{
    #region WriteValue

    /// <summary>
    /// 将指定类型的值写入字节跨度，使用默认的字节序。
    /// </summary>
    /// <typeparam name="T">要写入的值类型，必须是非托管类型。</typeparam>
    /// <param name="span">要写入的字节跨度，写入后会自动推进位置。</param>
    /// <param name="value">要写入的值。</param>
    /// <remarks>
    /// 此方法使用默认的字节序转换器写入值，并自动推进跨度位置。
    /// </remarks>
    public static void WriteValue<T>(this ref Span<byte> span, T value)
        where T : unmanaged
    {
        var size = TouchSocketBitConverter.Default.WriteBytes(span, value);
        span = span.Slice(size);
    }

    /// <summary>
    /// 将指定类型的值写入字节跨度，使用指定的字节序。
    /// </summary>
    /// <typeparam name="T">要写入的值类型，必须是非托管类型。</typeparam>
    /// <param name="span">要写入的字节跨度，写入后会自动推进位置。</param>
    /// <param name="value">要写入的值。</param>
    /// <param name="endianType">字节序类型。</param>
    /// <remarks>
    /// 此方法使用指定的字节序转换器写入值，并自动推进跨度位置。
    /// </remarks>
    public static void WriteValue<T>(this ref Span<byte> span, T value, EndianType endianType)
        where T : unmanaged
    {
        var size = TouchSocketBitConverter.GetBitConverter(endianType).WriteBytes(span, value);
        span = span.Slice(size);
    }

    #endregion WriteValue

    #region ReadValue

    /// <summary>
    /// 从只读字节跨度中读取UTF-8编码的字符串。
    /// </summary>
    /// <param name="span">要读取的只读字节跨度，读取后会自动推进位置。</param>
    /// <param name="headerType">固定头部类型，指定长度字段的大小。默认为<see cref="FixedHeaderType.Int"/>。</param>
    /// <returns>读取的字符串，如果为空则返回 <see langword="null"/>。</returns>
    /// <remarks>
    /// 此方法根据指定的头部类型读取长度信息，然后读取对应长度的字节并转换为UTF-8字符串。
    /// 当长度字段为最大值时，表示字符串为空。
    /// </remarks>
    public static string ReadString(this ref ReadOnlySpan<byte> span, FixedHeaderType headerType = FixedHeaderType.Int)
    {
        int len;
        switch (headerType)
        {
            case FixedHeaderType.Byte:
                len = ReadValue<byte>(ref span);
                if (len == byte.MaxValue)
                {
                    return null;
                }
                break;

            case FixedHeaderType.Ushort:
                len = ReadValue<ushort>(ref span);
                if (len == ushort.MaxValue)
                {
                    return null;
                }
                break;

            case FixedHeaderType.Int:
            default:
                len = ReadValue<int>(ref span);
                if (len == int.MaxValue)
                {
                    return null;
                }
                break;
        }

        var spanString = span.Slice(0, len);
        var str = spanString.ToString(Encoding.UTF8);
        span = span.Slice(len);
        return str;
    }

    /// <summary>
    /// 从只读字节跨度中读取指定长度的字节跨度。
    /// </summary>
    /// <param name="span">要读取的只读字节跨度，读取后会自动推进位置。</param>
    /// <param name="length">要读取的字节长度。</param>
    /// <returns>读取的只读字节跨度。</returns>
    /// <remarks>
    /// 此方法读取指定长度的字节并推进跨度位置。
    /// </remarks>
    public static ReadOnlySpan<byte> ReadToSpan(this ref ReadOnlySpan<byte> span, int length)
    {
        var result = span.Slice(0, length);
        span = span.Slice(length);
        return result;
    }

    /// <summary>
    /// 从只读字节跨度中读取指定类型的值，使用默认的字节序。
    /// </summary>
    /// <typeparam name="T">要读取的值类型，必须是非托管类型。</typeparam>
    /// <param name="span">要读取的只读字节跨度，读取后会自动推进位置。</param>
    /// <returns>读取的值。</returns>
    /// <exception cref="ArgumentOutOfRangeException">当跨度长度不足以读取指定类型时抛出。</exception>
    /// <remarks>
    /// 此方法使用默认的字节序转换器读取值，并自动推进跨度位置。
    /// </remarks>
    public static T ReadValue<T>(this ref ReadOnlySpan<byte> span)
        where T : unmanaged
    {
        var size = Unsafe.SizeOf<T>();
        if (span.Length < size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), span.Length, size);
        }
        var value= TouchSocketBitConverter.Default.To<T>(span);
        span = span.Slice(size);
        return value;
    }

    /// <summary>
    /// 从只读字节跨度中读取指定类型的值，使用指定的字节序。
    /// </summary>
    /// <typeparam name="T">要读取的值类型，必须是非托管类型。</typeparam>
    /// <param name="span">要读取的只读字节跨度，读取后会自动推进位置。</param>
    /// <param name="endianType">字节序类型。</param>
    /// <returns>读取的值。</returns>
    /// <exception cref="ArgumentOutOfRangeException">当跨度长度不足以读取指定类型时抛出。</exception>
    /// <remarks>
    /// 此方法使用指定的字节序转换器读取值，并自动推进跨度位置。
    /// </remarks>
    public static T ReadValue<T>(this ref ReadOnlySpan<byte> span, EndianType endianType)
        where T : unmanaged
    {
        var size = Unsafe.SizeOf<T>();
        if (span.Length < size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), span.Length, size);
        }
        var value = TouchSocketBitConverter.GetBitConverter(endianType).To<T>(span);
        span = span.Slice(size);
        return value;
    }

    #endregion ReadValue
}