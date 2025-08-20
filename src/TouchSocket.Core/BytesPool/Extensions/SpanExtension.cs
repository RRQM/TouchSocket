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

public static class SpanExtension
{
    #region WriteValue

    public static void WriteValue<T>(this ref Span<byte> span, T value)
        where T : unmanaged
    {
        var size = TouchSocketBitConverter.Default.WriteBytes(span, value);
        span = span.Slice(size);
    }

    public static void WriteValue<T>(this ref Span<byte> span, T value, EndianType endianType)
        where T : unmanaged
    {
        var size = TouchSocketBitConverter.GetBitConverter(endianType).WriteBytes(span, value);
        span = span.Slice(size);
    }

    #endregion WriteValue

    #region ReadValue

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

    public static ReadOnlySpan<byte> ReadToSpan(this ref ReadOnlySpan<byte> span, int length)
    {
        var result = span.Slice(0, length);
        span = span.Slice(length);
        return result;
    }

    public static T ReadValue<T>(this ref ReadOnlySpan<byte> span)
        where T : unmanaged
    {
        var size = Unsafe.SizeOf<T>();
        if (span.Length < size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), span.Length, size);
        }
        return TouchSocketBitConverter.Default.To<T>(span);
    }

    public static T ReadValue<T>(this ref ReadOnlySpan<byte> span, EndianType endianType)
        where T : unmanaged
    {
        var size = Unsafe.SizeOf<T>();
        if (span.Length < size)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(size), span.Length, size);
        }
        return TouchSocketBitConverter.GetBitConverter(endianType).To<T>(span);
    }

    #endregion ReadValue
}