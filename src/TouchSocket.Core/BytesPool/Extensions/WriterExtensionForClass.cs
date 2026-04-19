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

public static partial class WriterExtension
{
    /// <inheritdoc cref="WriteBooleans{TWriter}(ref TWriter, ReadOnlySpan{bool})"/>
    public static void WriteBooleans<TWriter>(this TWriter writer, ReadOnlySpan<bool> values)
        where TWriter : class, IBytesWriter
    {
        WriteBooleans(ref writer, values);
    }

    /// <inheritdoc cref="WriteByteBlock{TWriter}(ref TWriter, ByteBlock)"/>
    public static void WriteByteBlock<TWriter>(this TWriter writer, ByteBlock byteBlock)
        where TWriter : class, IBytesWriter
    {
        WriteByteBlock(ref writer, byteBlock);
    }

    /// <inheritdoc cref="WriteByteSpan{TWriter}(ref TWriter, ReadOnlySpan{byte})"/>
    public static void WriteByteSpan<TWriter>(this TWriter writer, scoped ReadOnlySpan<byte> span)
        where TWriter : class, IBytesWriter
    {
        WriteByteSpan(ref writer, span);
    }

    /// <inheritdoc cref="WriteIsNull{TWriter, T}(ref TWriter, T)"/>
    public static void WriteIsNull<TWriter, T>(this TWriter writer, T t)
        where T : class
        where TWriter : class, IBytesWriter
    {
        WriteIsNull<TWriter, T>(ref writer, t);
    }

    /// <inheritdoc cref="WriteIsNull{TWriter, T}(ref TWriter, T?)"/>
    public static void WriteIsNull<TWriter, T>(this TWriter writer, T? t)
        where T : struct
        where TWriter : class, IBytesWriter
    {
        WriteIsNull<TWriter, T>(ref writer, t);
    }

    /// <inheritdoc cref="WriteNormalString{TWriter}(ref TWriter, string, Encoding)"/>
    public static void WriteNormalString<TWriter>(this TWriter writer, string value, Encoding encoding)
        where TWriter : class, IBytesWriter
    {
        WriteNormalString(ref writer, value, encoding);
    }

    /// <inheritdoc cref="WriteNormalString{TWriter}(ref TWriter, ReadOnlySpan{char}, Encoding)"/>
    public static void WriteNormalString<TWriter>(this TWriter writer, ReadOnlySpan<char> value, Encoding encoding)
        where TWriter : class, IBytesWriter
    {
        WriteNormalString(ref writer, value, encoding);
    }

    /// <inheritdoc cref="WriteNotNull{TWriter}(ref TWriter)"/>
    public static void WriteNotNull<TWriter>(this TWriter writer)
        where TWriter : class, IBytesWriter
    {
        WriteNotNull(ref writer);
    }

    /// <inheritdoc cref="WriteNull{TWriter}(ref TWriter)"/>
    public static void WriteNull<TWriter>(this TWriter writer)
        where TWriter : class, IBytesWriter
    {
        WriteNull(ref writer);
    }

    /// <inheritdoc cref="WriteString{TWriter}(ref TWriter, string, FixedHeaderType)"/>
    public static void WriteString<TWriter>(this TWriter writer, string value, FixedHeaderType headerType = FixedHeaderType.Int)
        where TWriter : class, IBytesWriter
    {
        WriteString(ref writer, value, headerType);
    }

    /// <inheritdoc cref="WriteString{TWriter}(ref TWriter, ReadOnlySpan{char}, FixedHeaderType)"/>
    public static void WriteString<TWriter>(this TWriter writer, ReadOnlySpan<char> value, FixedHeaderType headerType = FixedHeaderType.Int)
        where TWriter : class, IBytesWriter
    {
        WriteString(ref writer, value, headerType);
    }

    /// <inheritdoc cref="WriteValue{TWriter, T}(ref TWriter, T)"/>
    public static void WriteValue<TWriter, T>(this TWriter writer, T value)
        where T : unmanaged
        where TWriter : class, IBytesWriter
    {
        WriteValue<TWriter, T>(ref writer, value);
    }

    /// <inheritdoc cref="WriteValue{TWriter, T}(ref TWriter, T, EndianType)"/>
    public static void WriteValue<TWriter, T>(this TWriter writer, T value, EndianType endianType)
        where T : unmanaged
        where TWriter : class, IBytesWriter
    {
        WriteValue<TWriter, T>(ref writer, value, endianType);
    }

    /// <inheritdoc cref="WriteVarString{TWriter}(ref TWriter, string)"/>
    public static void WriteVarString<TWriter>(this TWriter writer, string value)
        where TWriter : class, IBytesWriter
    {
        WriteVarString(ref writer, value);
    }

    /// <inheritdoc cref="WriteVarString{TWriter}(ref TWriter, ReadOnlySpan{char})"/>
    public static void WriteVarString<TWriter>(this TWriter writer, ReadOnlySpan<char> value)
        where TWriter : class, IBytesWriter
    {
        WriteVarString(ref writer, value);
    }

    /// <inheritdoc cref="WriteVarUInt32{TWriter}(ref TWriter, uint)"/>
    public static int WriteVarUInt32<TWriter>(this TWriter writer, uint value)
        where TWriter : class, IBytesWriter
    {
        return WriteVarUInt32(ref writer, value);
    }
}