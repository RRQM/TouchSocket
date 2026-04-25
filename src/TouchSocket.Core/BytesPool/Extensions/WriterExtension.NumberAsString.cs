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

using System.Globalization;

namespace TouchSocket.Core;

public static partial class WriterExtension
{

    //issue:https://github.com/RRQM/TouchSocket/issues/130

    /// <summary>
    /// 数字字符串格式化所需的最大字符数（ulong.MaxValue/long.MinValue = 20位，double(G17) ≈ 24位，保留32位余量）
    /// </summary>
    public const int NumberStringBufferSize = 32;

    #region Write

#if NET6_0_OR_GREATER
    /// <summary>
    /// 将数值格式化为字符串，并使用指定编码直接写入字节写入器，不含长度前缀。
    /// 零堆分配实现，通过<see cref="ISpanFormattable"/>接口格式化。
    /// </summary>
    /// <typeparam name="TWriter">实现<see cref="IBytesWriter"/>接口的写入器类型。</typeparam>
    /// <typeparam name="TNumber">实现<see cref="ISpanFormattable"/>接口的数值类型。</typeparam>
    /// <param name="writer">字节写入器实例。</param>
    /// <param name="value">要写入的数值。</param>
    /// <param name="encoding">用于将字符转换为字节的编码。</param>
    public static void WriteNumberAsString<TWriter, TNumber>(ref TWriter writer, TNumber value, Encoding encoding)
        where TWriter : IBytesWriter
        where TNumber :unmanaged, ISpanFormattable
    {
        unsafe
        {
            var charBuf = stackalloc char[NumberStringBufferSize];
            value.TryFormat(new Span<char>(charBuf, NumberStringBufferSize), out var charsWritten, default, CultureInfo.InvariantCulture);
            WriteEncodedNumberChars(ref writer, encoding, charBuf, charsWritten);
        }
    }
#else
    /// <summary>
    /// 将数值格式化为字符串，并使用指定编码直接写入字节写入器，不含长度前缀。
    /// 会产生一次<see cref="string"/>分配。
    /// </summary>
    /// <typeparam name="TWriter">实现<see cref="IBytesWriter"/>接口的写入器类型。</typeparam>
    /// <typeparam name="TNumber">实现<see cref="IFormattable"/>接口的数值类型。</typeparam>
    /// <param name="writer">字节写入器实例。</param>
    /// <param name="value">要写入的数值。</param>
    /// <param name="encoding">用于将字符转换为字节的编码。</param>
    public static void WriteNumberAsString<TWriter, TNumber>(ref TWriter writer, TNumber value, Encoding encoding)
        where TWriter : IBytesWriter
        where TNumber : unmanaged, IFormattable
    {
        // float/double 需用 "R"（round-trip）格式，否则 G15 精度不足会导致解析时溢出
        var format = (typeof(TNumber) == typeof(float) || typeof(TNumber) == typeof(double)) ? "R" : null;
        var str = value.ToString(format, CultureInfo.InvariantCulture);
        unsafe
        {
            fixed (char* p = str)
            {
                WriteEncodedNumberChars(ref writer, encoding, p, str.Length);
            }
        }
    }
#endif

    #endregion Write

    #region Private Helpers

    /// <summary>
    /// 将字符缓冲区按指定编码编码后直接写入字节写入器，不含任何长度前缀。
    /// </summary>
    private static unsafe void WriteEncodedNumberChars<TWriter>(
        ref TWriter writer, Encoding encoding, char* charBuf, int charsWritten)
        where TWriter : IBytesWriter
    {
        var byteCount = encoding.GetByteCount(charBuf, charsWritten);
        var span = writer.GetSpan(byteCount);
        fixed (byte* pBytes = &span[0])
        {
            encoding.GetBytes(charBuf, charsWritten, pBytes, byteCount);
        }
        writer.Advance(byteCount);
    }

    #endregion Private Helpers
}
