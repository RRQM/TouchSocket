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

using System.Runtime.CompilerServices;

namespace TouchSocket.Http;

static class TouchSocketHttpUtility
{
    public const int MaxReadSize = 1024 * 1024;

    public const byte COLON = (byte)':';
    public const byte SPACE = (byte)' ';
    public const byte TAB = (byte)'\t';

    public static ReadOnlySpan<byte> CRLF => "\r\n"u8;
    public static ReadOnlySpan<byte> CRLFCRLF => "\r\n\r\n"u8;

    private static readonly byte[] s_http11Response = "HTTP/1.1 "u8.ToArray();
    private static readonly byte[] s_http10Response = "HTTP/1.0 "u8.ToArray();
    
    private static readonly string[] s_statusCodeCache = new string[600];
    private static readonly byte[][] s_statusCodeBytesCache = new byte[600][];
    
    static TouchSocketHttpUtility()
    {
        for (var i = 0; i < 600; i++)
        {
            s_statusCodeCache[i] = i.ToString();
            s_statusCodeBytesCache[i] = Encoding.UTF8.GetBytes(s_statusCodeCache[i]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ReadOnlySpan<byte> GetStatusCodeBytes(int statusCode)
    {
        if (statusCode >= 0 && statusCode < 600)
        {
            return s_statusCodeBytesCache[statusCode];
        }
        return Encoding.UTF8.GetBytes(statusCode.ToString());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ReadOnlySpan<byte> GetHttpVersionBytes(string version)
    {
        if (version == "1.1")
        {
            return s_http11Response.AsSpan(0, 8);
        }
        if (version == "1.0")
        {
            return s_http10Response.AsSpan(0, 8);
        }
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AppendAnd<TWriter>(ref TWriter writer) where TWriter : IBytesWriter
    {
        writer.Write("&"u8);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AppendColon<TWriter>(ref TWriter writer) where TWriter : IBytesWriter
    {
        writer.Write(":"u8);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AppendEqual<TWriter>(ref TWriter writer) where TWriter : IBytesWriter
    {
        writer.Write("="u8);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AppendHTTP<TWriter>(ref TWriter writer) where TWriter : IBytesWriter
    {
        writer.Write("HTTP"u8);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AppendQuestionMark<TWriter>(ref TWriter writer) where TWriter : IBytesWriter
    {
        writer.Write("?"u8);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AppendRn<TWriter>(ref TWriter writer) where TWriter : IBytesWriter
    {
        writer.Write(CRLF);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AppendSlash<TWriter>(ref TWriter writer) where TWriter : IBytesWriter
    {
        writer.Write("/"u8);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AppendSpace<TWriter>(ref TWriter writer) where TWriter : IBytesWriter
    {
        writer.Write(StringExtension.DefaultSpaceUtf8Span);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AppendUtf8String<TWriter>(ref TWriter writer, string value) where TWriter : IBytesWriter
    {
        WriterExtension.WriteNormalString(ref writer, value, Encoding.UTF8);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AppendHex<TWriter>(ref TWriter writer, int value) where TWriter : IBytesWriter
    {
        AppendUtf8String(ref writer, $"{value:X}");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool IsWhitespace(byte b) => b == SPACE || b == TAB;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ReadOnlySpan<byte> TrimWhitespace(ReadOnlySpan<byte> span)
    {
        var start = 0;
        var end = span.Length - 1;

        while (start <= end && IsWhitespace(span[start]))
        {
            start++;
        }

        while (end >= start && IsWhitespace(span[end]))
        {
            end--;
        }
        return start > end ? [] : span[start..(end + 1)];
    }

    internal static string UnescapeDataString(ReadOnlySpan<byte> urlSpan)
    {
#if NET9_0_OR_GREATER
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
            keySpan = kvSpan;
            valueSpan = [];
        }

        if (!keySpan.IsEmpty)
        {
            var key = TouchSocketHttpUtility.UnescapeDataString(keySpan);
            var value = TouchSocketHttpUtility.UnescapeDataString(valueSpan);
            parameters.Add(key, value);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool EqualsIgnoreCaseAscii(ReadOnlySpan<byte> span1, ReadOnlySpan<byte> span2)
    {
        if (span1.Length != span2.Length)
        {
            return false;
        }

        for (var i = 0; i < span1.Length; i++)
        {
            var c1 = span1[i];
            var c2 = span2[i];

            if (c1 == c2)
            {
                continue;
            }

            if ((uint)(c1 - 'A') <= 'Z' - 'A')
            {
                c1 = (byte)(c1 | 0x20);
            }

            if ((uint)(c2 - 'A') <= 'Z' - 'A')
            {
                c2 = (byte)(c2 | 0x20);
            }

            if (c1 != c2)
            {
                return false;
            }
        }

        return true;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool SupportsMultipleValues(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return false;
        }

        var length = key.Length;
        var firstChar = char.ToLowerInvariant(key[0]);

        return firstChar switch
        {
            'a' => length switch
            {
                5 => key.Equals("Allow", StringComparison.OrdinalIgnoreCase),
                6 => key.Equals("Accept", StringComparison.OrdinalIgnoreCase),
                13 => key.Equals("Accept-Charset", StringComparison.OrdinalIgnoreCase),
                14 => key.Equals("Accept-Encoding", StringComparison.OrdinalIgnoreCase),
                15 => key.Equals("Accept-Langauge", StringComparison.OrdinalIgnoreCase) || key.Equals("Accept-Language", StringComparison.OrdinalIgnoreCase),
                _ => false
            },
            'c' => length switch
            {
                10 => key.Equals("Connection", StringComparison.OrdinalIgnoreCase),
                13 => key.Equals("Cache-Control", StringComparison.OrdinalIgnoreCase),
                15 => key.Equals("Content-Encoding", StringComparison.OrdinalIgnoreCase),
                _ => false
            },
            't' => length switch
            {
                2 => key.Equals("TE", StringComparison.OrdinalIgnoreCase),
                7 => key.Equals("Trailer", StringComparison.OrdinalIgnoreCase),
                17 => key.Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase),
                _ => false
            },
            'u' => length == 7 && key.Equals("Upgrade", StringComparison.OrdinalIgnoreCase),
            'v' => length switch
            {
                3 => key.Equals("Via", StringComparison.OrdinalIgnoreCase),
                4 => key.Equals("Vary", StringComparison.OrdinalIgnoreCase),
                _ => false
            },
            'w' => length == 7 && key.Equals("Warning", StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool SupportsMultipleValues(ReadOnlySpan<byte> keySpan)
    {
        //issue:https://github.com/RRQM/TouchSocket/issues/108

        // 根据 HTTP 规范，以下头支持使用逗号分隔多个值
        // Accept, Accept-Charset, Accept-Encoding, Accept-Language
        // Cache-Control, Connection, Content-Encoding, TE, Trailer, Transfer-Encoding, Upgrade, Via, Warning
        // Allow, Vary

        var length = keySpan.Length;
        if (length == 0)
        {
            return false;
        }

        var firstChar = (byte)(keySpan[0] | 0x20); // 转换为小写

        return firstChar switch
        {
            (byte)'a' => length switch
            {
                5 => EqualsIgnoreCaseAscii(keySpan, "Allow"u8),
                6 => EqualsIgnoreCaseAscii(keySpan, "Accept"u8),
                13 => EqualsIgnoreCaseAscii(keySpan, "Accept-Charset"u8),
                14 => EqualsIgnoreCaseAscii(keySpan, "Accept-Encoding"u8),
                15 => EqualsIgnoreCaseAscii(keySpan, "Accept-Langauge"u8) || EqualsIgnoreCaseAscii(keySpan, "Accept-Language"u8),
                _ => false
            },
            (byte)'c' => length switch
            {
                10 => EqualsIgnoreCaseAscii(keySpan, "Connection"u8),
                13 => EqualsIgnoreCaseAscii(keySpan, "Cache-Control"u8),
                15 => EqualsIgnoreCaseAscii(keySpan, "Content-Encoding"u8),
                _ => false
            },
            (byte)'t' => length switch
            {
                2 => EqualsIgnoreCaseAscii(keySpan, "TE"u8),
                7 => EqualsIgnoreCaseAscii(keySpan, "Trailer"u8),
                17 => EqualsIgnoreCaseAscii(keySpan, "Transfer-Encoding"u8),
                _ => false
            },
            (byte)'u' => length == 7 && EqualsIgnoreCaseAscii(keySpan, "Upgrade"u8),
            (byte)'v' => length switch
            {
                3 => EqualsIgnoreCaseAscii(keySpan, "Via"u8),
                4 => EqualsIgnoreCaseAscii(keySpan, "Vary"u8),
                _ => false
            },
            (byte)'w' => length == 7 && EqualsIgnoreCaseAscii(keySpan, "Warning"u8),
            _ => false
        };
    }
}