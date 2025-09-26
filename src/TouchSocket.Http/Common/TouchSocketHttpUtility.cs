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

namespace TouchSocket.Http;

/// <summary>
/// TouchSocketHttp辅助工具类
/// </summary>
public static class TouchSocketHttpUtility
{
    public const int MaxReadSize = 1024 * 1024;

    /// <summary>
    /// 获取一个只读的字节序列，表示回车换行(CRLF)。
    /// </summary>
    /// <value>
    /// 一个包含回车和换行字节的只读字节序列。
    /// </value>
    public static ReadOnlySpan<byte> CRLF => new byte[] { (byte)'\r', (byte)'\n' };

    /// <summary>
    /// 在 <see cref="IByteBlock"/> 中追加 "&amp;" 符号。
    /// </summary>
    /// <typeparam name="TWriter">实现了 <see cref="IByteBlock"/> 的类型。</typeparam>
    /// <param name="writer">字节块实例。</param>
    public static void AppendAnd<TWriter>(ref TWriter writer) where TWriter : IBytesWriter
    {
        writer.Write("&"u8);
    }

    /// <summary>
    /// 在 <see cref="IByteBlock"/> 中追加 ":" 符号。
    /// </summary>
    /// <typeparam name="TWriter">实现了 <see cref="IByteBlock"/> 的类型。</typeparam>
    /// <param name="writer">字节块实例。</param>
    public static void AppendColon<TWriter>(ref TWriter writer) where TWriter : IBytesWriter
    {
        writer.Write(":"u8);
    }

    /// <summary>
    /// 在 <see cref="IByteBlock"/> 中追加 "=" 符号。
    /// </summary>
    /// <typeparam name="TWriter">实现了 <see cref="IByteBlock"/> 的类型。</typeparam>
    /// <param name="writer">字节块实例。</param>
    public static void AppendEqual<TWriter>(ref TWriter writer) where TWriter : IBytesWriter
    {
        writer.Write("="u8);
    }

    /// <summary>
    /// 在 <see cref="IByteBlock"/> 中追加 "HTTP" 字符串。
    /// </summary>
    /// <typeparam name="TWriter">实现了 <see cref="IByteBlock"/> 的类型。</typeparam>
    /// <param name="writer">字节块实例。</param>
    public static void AppendHTTP<TWriter>(ref TWriter writer) where TWriter : IBytesWriter
    {
        writer.Write("HTTP"u8);
    }

    /// <summary>
    /// 在 <see cref="IByteBlock"/> 中追加 "?" 符号。
    /// </summary>
    /// <typeparam name="TWriter">实现了 <see cref="IByteBlock"/> 的类型。</typeparam>
    /// <param name="writer">字节块实例。</param>
    public static void AppendQuestionMark<TWriter>(ref TWriter writer) where TWriter : IBytesWriter
    {
        writer.Write("?"u8);
    }

    /// <summary>
    /// 在 <see cref="IByteBlock"/> 中追加回车换行符 "\r\n"。
    /// </summary>
    /// <typeparam name="TWriter">实现了 <see cref="IByteBlock"/> 的类型。</typeparam>
    /// <param name="writer">字节块实例。</param>
    public static void AppendRn<TWriter>(ref TWriter writer) where TWriter : IBytesWriter
    {
        writer.Write("\r\n"u8);
    }

    /// <summary>
    /// 在 <see cref="IByteBlock"/> 中追加 "/" 符号。
    /// </summary>
    /// <typeparam name="TWriter">实现了 <see cref="IByteBlock"/> 的类型。</typeparam>
    /// <param name="writer">字节块实例。</param>
    public static void AppendSlash<TWriter>(ref TWriter writer) where TWriter : IBytesWriter
    {
        writer.Write("/"u8);
    }

    /// <summary>
    /// 在 <see cref="IByteBlock"/> 中追加空格符。
    /// </summary>
    /// <typeparam name="TWriter">实现了 <see cref="IByteBlock"/> 的类型。</typeparam>
    /// <param name="writer">字节块实例。</param>
    public static void AppendSpace<TWriter>(ref TWriter writer) where TWriter : IBytesWriter
    {
        writer.Write(StringExtension.DefaultSpaceUtf8Span);
    }

    /// <summary>
    /// 在 <see cref="IByteBlock"/> 中追加指定的 UTF-8 编码字符串。
    /// </summary>
    /// <typeparam name="TWriter">实现了 <see cref="IByteBlock"/> 的类型。</typeparam>
    /// <param name="writer">字节块实例。</param>
    /// <param name="value">要追加的字符串。</param>
    public static void AppendUtf8String<TWriter>(ref TWriter writer, string value) where TWriter : IBytesWriter
    {
        WriterExtension.WriteNormalString(ref writer, value, Encoding.UTF8);
    }

    /// <summary>
    /// 在 <see cref="IByteBlock"/> 中追加指定整数的十六进制表示。
    /// </summary>
    /// <typeparam name="TWriter">实现了 <see cref="IByteBlock"/> 的类型。</typeparam>
    /// <param name="writer">字节块实例。</param>
    /// <param name="value">要追加的整数值。</param>
    public static void AppendHex<TWriter>(ref TWriter writer, int value) where TWriter : IBytesWriter
    {
        AppendUtf8String(ref writer, $"{value:X}");
    }

    internal static string UnescapeDataString(ReadOnlySpan<byte> urlSpan)
    {
#if NET9_0_OR_GREATER
        // 直接处理字节的URL解码
        Span<char> charBuffer = stackalloc char[urlSpan.Length];
        var charCount = Encoding.UTF8.GetChars(urlSpan, charBuffer);
        return Uri.UnescapeDataString(charBuffer.Slice(0, charCount));
#else
        return Uri.UnescapeDataString(urlSpan.ToString(Encoding.UTF8));
#endif

    }

    internal static string UnescapeDataString(ReadOnlySpan<char> urlSpan)
    {
#if NET9_0_OR_GREATER
        return Uri.UnescapeDataString(urlSpan);
#else
        return Uri.UnescapeDataString(urlSpan.ToString());
#endif

    }

    internal static bool IsWhitespace(byte b) => b == ' ' || b == '\t';


    internal static int FindNextWhitespace(ReadOnlySpan<byte> span, int start)
    {
        for (var i = start; i < span.Length; i++)
        {
            if (TouchSocketHttpUtility.IsWhitespace(span[i]))
            {
                return i;
            }
        }
        return -1;
    }

    internal static int SkipSpaces(ReadOnlySpan<byte> span, int start)
    {
        while (start < span.Length && TouchSocketHttpUtility.IsWhitespace(span[start]))
        {
            start++;
        }

        return start;
    }

    internal static void ProcessKeyValuePair(ReadOnlySpan<char> kvSpan, InternalHttpParams parameters)
    {
        var eqIndex = kvSpan.IndexOf('=');
        ReadOnlySpan<char> keySpan, valueSpan;

        if (eqIndex >= 0)
        {
            keySpan = kvSpan.Slice(0, eqIndex);
            valueSpan = kvSpan.Slice(eqIndex + 1);
        }
        else
        {
            // 处理没有值的键
            keySpan = kvSpan;
            valueSpan = [];
        }

        if (!keySpan.IsEmpty)
        {
            var key = TouchSocketHttpUtility.UnescapeDataString(keySpan);
            var value = TouchSocketHttpUtility.UnescapeDataString(valueSpan);
            parameters.AddOrUpdate(key, value);
        }
    }
}