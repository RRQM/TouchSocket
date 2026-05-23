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

using System.Text;

namespace TouchSocket.Semi;

/// <summary>
/// 提供 <see cref="HsmsMessage"/> 相关扩展方法。
/// </summary>
public static class HsmsMessageExtensions
{
    /// <summary>
    /// 将 <see cref="HsmsMessage"/> 格式化为 SML（SECS Message Language）标准字符串。
    /// </summary>
    /// <param name="message">要格式化的 HSMS 消息。</param>
    /// <returns>符合 SML 规范的 <c>&lt;&gt;</c> 格式字符串。</returns>
    public static string ToSml(this HsmsMessage message)
    {
        var sb = new StringBuilder();

        sb.Append('S');
        sb.Append(message.S);
        sb.Append('F');
        sb.Append(message.F);

        if (message.ReplyExpected)
        {
            sb.Append(" W");
        }

        if (message.Body != null)
        {
            sb.AppendLine();
            AppendSecsItem(sb, message.Body, 0);
        }

        sb.AppendLine();
        sb.Append('.');

        return sb.ToString();
    }

    private static void AppendSecsItem(StringBuilder sb, SecsItem item, int depth)
    {
        var indent = new string(' ', depth * 2);

        switch (item)
        {
            case ListSecsItem listItem:
            {
                var items = listItem.Items;
                sb.Append(indent);
                sb.Append("<L [");
                sb.Append(items.Length);
                sb.Append(']');
                if (items.Length > 0)
                {
                    foreach (var child in items.Span)
                    {
                        sb.AppendLine();
                        AppendSecsItem(sb, child, depth + 1);
                    }
                    sb.AppendLine();
                    sb.Append(indent);
                }
                sb.Append('>');
                break;
            }
            case BinarySecsItem binaryItem:
            {
                sb.Append(indent);
                sb.Append("<B");
                foreach (var b in binaryItem.Data.Span)
                {
                    sb.Append(" 0x");
                    sb.Append(b.ToString("X2"));
                }
                sb.Append('>');
                break;
            }
            case BooleanSecsItem boolItem:
            {
                sb.Append(indent);
                sb.Append("<BOOLEAN");
                foreach (var b in boolItem.Values.Span)
                {
                    sb.Append(b != 0 ? " T" : " F");
                }
                sb.Append('>');
                break;
            }
            case StringSecsItem stringItem:
            {
                sb.Append(indent);
                sb.Append('<');
                sb.Append(GetFormatName(item.SecsFormat));
                sb.Append(" \"");
                sb.Append(stringItem.Value);
                sb.Append("\">");
                break;
            }
            case I1SecsItem i1Item:
            {
                sb.Append(indent);
                sb.Append("<I1");
                foreach (var v in i1Item.Values.Span)
                {
                    sb.Append(' ');
                    sb.Append(v);
                }
                sb.Append('>');
                break;
            }
            case I2SecsItem i2Item:
            {
                sb.Append(indent);
                sb.Append("<I2");
                foreach (var v in i2Item.Values.Span)
                {
                    sb.Append(' ');
                    sb.Append(v);
                }
                sb.Append('>');
                break;
            }
            case I4SecsItem i4Item:
            {
                sb.Append(indent);
                sb.Append("<I4");
                foreach (var v in i4Item.Values.Span)
                {
                    sb.Append(' ');
                    sb.Append(v);
                }
                sb.Append('>');
                break;
            }
            case I8SecsItem i8Item:
            {
                sb.Append(indent);
                sb.Append("<I8");
                foreach (var v in i8Item.Values.Span)
                {
                    sb.Append(' ');
                    sb.Append(v);
                }
                sb.Append('>');
                break;
            }
            case U1SecsItem u1Item:
            {
                sb.Append(indent);
                sb.Append("<U1");
                foreach (var v in u1Item.Values.Span)
                {
                    sb.Append(' ');
                    sb.Append(v);
                }
                sb.Append('>');
                break;
            }
            case U2SecsItem u2Item:
            {
                sb.Append(indent);
                sb.Append("<U2");
                foreach (var v in u2Item.Values.Span)
                {
                    sb.Append(' ');
                    sb.Append(v);
                }
                sb.Append('>');
                break;
            }
            case U4SecsItem u4Item:
            {
                sb.Append(indent);
                sb.Append("<U4");
                foreach (var v in u4Item.Values.Span)
                {
                    sb.Append(' ');
                    sb.Append(v);
                }
                sb.Append('>');
                break;
            }
            case U8SecsItem u8Item:
            {
                sb.Append(indent);
                sb.Append("<U8");
                foreach (var v in u8Item.Values.Span)
                {
                    sb.Append(' ');
                    sb.Append(v);
                }
                sb.Append('>');
                break;
            }
            case F4SecsItem f4Item:
            {
                sb.Append(indent);
                sb.Append("<F4");
                foreach (var v in f4Item.Values.Span)
                {
                    sb.Append(' ');
                    sb.Append(v);
                }
                sb.Append('>');
                break;
            }
            case F8SecsItem f8Item:
            {
                sb.Append(indent);
                sb.Append("<F8");
                foreach (var v in f8Item.Values.Span)
                {
                    sb.Append(' ');
                    sb.Append(v);
                }
                sb.Append('>');
                break;
            }
            default:
            {
                sb.Append(indent);
                sb.Append('<');
                sb.Append(GetFormatName(item.SecsFormat));
                sb.Append('>');
                break;
            }
        }
    }

    private static string GetFormatName(SecsFormat format)
    {
        return format switch
        {
            SecsFormat.List => "L",
            SecsFormat.Binary => "B",
            SecsFormat.Boolean => "BOOLEAN",
            SecsFormat.ASCII => "A",
            SecsFormat.JIS8 => "JIS8",
            SecsFormat.I1 => "I1",
            SecsFormat.I2 => "I2",
            SecsFormat.I4 => "I4",
            SecsFormat.I8 => "I8",
            SecsFormat.U1 => "U1",
            SecsFormat.U2 => "U2",
            SecsFormat.U4 => "U4",
            SecsFormat.U8 => "U8",
            SecsFormat.F4 => "F4",
            SecsFormat.F8 => "F8",
            _ => format.ToString()
        };
    }

    /// <summary>
    /// 将 SML（SECS Message Language）标准字符串解析为 <see cref="HsmsMessage"/>。
    /// </summary>
    /// <param name="sml">符合 SML 规范的 <c>&lt;&gt;</c> 格式字符串。</param>
    /// <returns>解析结果 <see cref="HsmsMessage"/>。</returns>
    /// <exception cref="FormatException">SML 格式不合法时抛出。</exception>
    public static HsmsMessage ParseSml(ReadOnlySpan<char> sml)
    {
        var pos = 0;
        SkipWhitespace(sml, ref pos);

        if (pos >= sml.Length || char.ToUpperInvariant(sml[pos]) != 'S')
            throw new FormatException($"SML格式错误：位置 {pos} 处缺少 'S'。");
        pos++;

        var s = ParseUInt8(sml, ref pos);

        if (pos >= sml.Length || char.ToUpperInvariant(sml[pos]) != 'F')
            throw new FormatException($"SML格式错误：位置 {pos} 处缺少 'F'。");
        pos++;

        var f = ParseUInt8(sml, ref pos);

        var replyExpected = false;
        SkipInlineSpaces(sml, ref pos);
        if (pos < sml.Length && char.ToUpperInvariant(sml[pos]) == 'W')
        {
            if (pos + 1 >= sml.Length || !char.IsLetterOrDigit(sml[pos + 1]))
            {
                replyExpected = true;
                pos++;
            }
        }

        SkipWhitespace(sml, ref pos);

        SecsItem? body = null;
        if (pos < sml.Length && sml[pos] == '<')
        {
            body = ParseSecsItem(sml, ref pos);
        }

        return new HsmsMessage
        {
            S = s,
            F = f,
            ReplyExpected = replyExpected,
            Body = body
        };
    }

    private static SecsItem ParseSecsItem(ReadOnlySpan<char> span, ref int pos)
    {
        SkipWhitespace(span, ref pos);

        if (pos >= span.Length || span[pos] != '<')
            throw new FormatException($"SML格式错误：位置 {pos} 处缺少 '<'。");
        pos++;

        var nameStart = pos;
        while (pos < span.Length && span[pos] != ' ' && span[pos] != '>' && span[pos] != '[')
            pos++;

        var name = span.Slice(nameStart, pos - nameStart).ToString().ToUpperInvariant();
        SkipInlineSpaces(span, ref pos);

        switch (name)
        {
            case "L":
            {
                if (pos < span.Length && span[pos] == '[')
                {
                    pos++;
                    while (pos < span.Length && span[pos] != ']')
                        pos++;
                    if (pos < span.Length) pos++;
                }

                var children = new List<SecsItem>();
                while (pos < span.Length)
                {
                    SkipWhitespace(span, ref pos);
                    if (pos >= span.Length || span[pos] == '>') break;
                    if (span[pos] == '<')
                        children.Add(ParseSecsItem(span, ref pos));
                    else
                        break;
                }

                SkipWhitespace(span, ref pos);
                if (pos < span.Length && span[pos] == '>') pos++;
                return new ListSecsItem(children.ToArray());
            }
            case "B":
            {
                var bytes = new List<byte>();
                while (pos < span.Length && span[pos] != '>')
                {
                    SkipInlineSpaces(span, ref pos);
                    if (pos >= span.Length || span[pos] == '>') break;

                    if (pos + 1 < span.Length && span[pos] == '0' && (span[pos + 1] == 'x' || span[pos + 1] == 'X'))
                    {
                        pos += 2;
                        var hexStart = pos;
                        while (pos < span.Length && IsHexChar(span[pos])) pos++;
                        bytes.Add(Convert.ToByte(span.Slice(hexStart, pos - hexStart).ToString(), 16));
                    }
                    else if (char.IsDigit(span[pos]))
                    {
                        var numStart = pos;
                        while (pos < span.Length && char.IsDigit(span[pos])) pos++;
                        bytes.Add(byte.Parse(span.Slice(numStart, pos - numStart).ToString()));
                    }
                    else break;
                }
                if (pos < span.Length && span[pos] == '>') pos++;
                return new BinarySecsItem(bytes.ToArray());
            }
            case "BOOLEAN":
            {
                var bools = new List<byte>();
                while (pos < span.Length && span[pos] != '>')
                {
                    SkipInlineSpaces(span, ref pos);
                    if (pos >= span.Length || span[pos] == '>') break;

                    var c = char.ToUpperInvariant(span[pos]);
                    if (c == 'T') { bools.Add(1); pos++; }
                    else if (c == 'F') { bools.Add(0); pos++; }
                    else break;
                }
                if (pos < span.Length && span[pos] == '>') pos++;
                return new BooleanSecsItem(bools.ToArray());
            }
            case "A":
            case "JIS8":
            {
                var value = string.Empty;
                if (pos < span.Length && span[pos] == '"')
                {
                    pos++;
                    var strStart = pos;
                    while (pos < span.Length && span[pos] != '"') pos++;
                    value = span.Slice(strStart, pos - strStart).ToString();
                    if (pos < span.Length) pos++;
                }
                while (pos < span.Length && span[pos] != '>') pos++;
                if (pos < span.Length) pos++;
                return name == "A" ? (SecsItem)new ASCIISecsItem(value) : new JIS8SecsItem(value);
            }
            case "I1":
            {
                var vals = new List<sbyte>();
                while (pos < span.Length && span[pos] != '>')
                {
                    SkipInlineSpaces(span, ref pos);
                    if (pos >= span.Length || span[pos] == '>') break;
                    var numStart = pos;
                    if (span[pos] == '-' || span[pos] == '+') pos++;
                    while (pos < span.Length && char.IsDigit(span[pos])) pos++;
                    if (pos > numStart)
                        vals.Add(sbyte.Parse(span.Slice(numStart, pos - numStart).ToString()));
                    else break;
                }
                if (pos < span.Length && span[pos] == '>') pos++;
                return new I1SecsItem(vals.ToArray());
            }
            case "I2":
            {
                var vals = new List<short>();
                while (pos < span.Length && span[pos] != '>')
                {
                    SkipInlineSpaces(span, ref pos);
                    if (pos >= span.Length || span[pos] == '>') break;
                    var numStart = pos;
                    if (span[pos] == '-' || span[pos] == '+') pos++;
                    while (pos < span.Length && char.IsDigit(span[pos])) pos++;
                    if (pos > numStart)
                        vals.Add(short.Parse(span.Slice(numStart, pos - numStart).ToString()));
                    else break;
                }
                if (pos < span.Length && span[pos] == '>') pos++;
                return new I2SecsItem(vals.ToArray());
            }
            case "I4":
            {
                var vals = new List<int>();
                while (pos < span.Length && span[pos] != '>')
                {
                    SkipInlineSpaces(span, ref pos);
                    if (pos >= span.Length || span[pos] == '>') break;
                    var numStart = pos;
                    if (span[pos] == '-' || span[pos] == '+') pos++;
                    while (pos < span.Length && char.IsDigit(span[pos])) pos++;
                    if (pos > numStart)
                        vals.Add(int.Parse(span.Slice(numStart, pos - numStart).ToString()));
                    else break;
                }
                if (pos < span.Length && span[pos] == '>') pos++;
                return new I4SecsItem(vals.ToArray());
            }
            case "I8":
            {
                var vals = new List<long>();
                while (pos < span.Length && span[pos] != '>')
                {
                    SkipInlineSpaces(span, ref pos);
                    if (pos >= span.Length || span[pos] == '>') break;
                    var numStart = pos;
                    if (span[pos] == '-' || span[pos] == '+') pos++;
                    while (pos < span.Length && char.IsDigit(span[pos])) pos++;
                    if (pos > numStart)
                        vals.Add(long.Parse(span.Slice(numStart, pos - numStart).ToString()));
                    else break;
                }
                if (pos < span.Length && span[pos] == '>') pos++;
                return new I8SecsItem(vals.ToArray());
            }
            case "U1":
            {
                var vals = new List<byte>();
                while (pos < span.Length && span[pos] != '>')
                {
                    SkipInlineSpaces(span, ref pos);
                    if (pos >= span.Length || span[pos] == '>') break;
                    var numStart = pos;
                    while (pos < span.Length && char.IsDigit(span[pos])) pos++;
                    if (pos > numStart)
                        vals.Add(byte.Parse(span.Slice(numStart, pos - numStart).ToString()));
                    else break;
                }
                if (pos < span.Length && span[pos] == '>') pos++;
                return new U1SecsItem(vals.ToArray());
            }
            case "U2":
            {
                var vals = new List<ushort>();
                while (pos < span.Length && span[pos] != '>')
                {
                    SkipInlineSpaces(span, ref pos);
                    if (pos >= span.Length || span[pos] == '>') break;
                    var numStart = pos;
                    while (pos < span.Length && char.IsDigit(span[pos])) pos++;
                    if (pos > numStart)
                        vals.Add(ushort.Parse(span.Slice(numStart, pos - numStart).ToString()));
                    else break;
                }
                if (pos < span.Length && span[pos] == '>') pos++;
                return new U2SecsItem(vals.ToArray());
            }
            case "U4":
            {
                var vals = new List<uint>();
                while (pos < span.Length && span[pos] != '>')
                {
                    SkipInlineSpaces(span, ref pos);
                    if (pos >= span.Length || span[pos] == '>') break;
                    var numStart = pos;
                    while (pos < span.Length && char.IsDigit(span[pos])) pos++;
                    if (pos > numStart)
                        vals.Add(uint.Parse(span.Slice(numStart, pos - numStart).ToString()));
                    else break;
                }
                if (pos < span.Length && span[pos] == '>') pos++;
                return new U4SecsItem(vals.ToArray());
            }
            case "U8":
            {
                var vals = new List<ulong>();
                while (pos < span.Length && span[pos] != '>')
                {
                    SkipInlineSpaces(span, ref pos);
                    if (pos >= span.Length || span[pos] == '>') break;
                    var numStart = pos;
                    while (pos < span.Length && char.IsDigit(span[pos])) pos++;
                    if (pos > numStart)
                        vals.Add(ulong.Parse(span.Slice(numStart, pos - numStart).ToString()));
                    else break;
                }
                if (pos < span.Length && span[pos] == '>') pos++;
                return new U8SecsItem(vals.ToArray());
            }
            case "F4":
            {
                var vals = new List<float>();
                while (pos < span.Length && span[pos] != '>')
                {
                    SkipInlineSpaces(span, ref pos);
                    if (pos >= span.Length || span[pos] == '>') break;
                    var numStart = pos;
                    if (span[pos] == '-' || span[pos] == '+') pos++;
                    while (pos < span.Length && (char.IsDigit(span[pos]) || span[pos] == '.' || span[pos] == 'e' || span[pos] == 'E' || span[pos] == '+' || span[pos] == '-')) pos++;
                    if (pos > numStart)
                        vals.Add(float.Parse(span.Slice(numStart, pos - numStart).ToString(), System.Globalization.CultureInfo.InvariantCulture));
                    else break;
                }
                if (pos < span.Length && span[pos] == '>') pos++;
                return new F4SecsItem(vals.ToArray());
            }
            case "F8":
            {
                var vals = new List<double>();
                while (pos < span.Length && span[pos] != '>')
                {
                    SkipInlineSpaces(span, ref pos);
                    if (pos >= span.Length || span[pos] == '>') break;
                    var numStart = pos;
                    if (span[pos] == '-' || span[pos] == '+') pos++;
                    while (pos < span.Length && (char.IsDigit(span[pos]) || span[pos] == '.' || span[pos] == 'e' || span[pos] == 'E' || span[pos] == '+' || span[pos] == '-')) pos++;
                    if (pos > numStart)
                        vals.Add(double.Parse(span.Slice(numStart, pos - numStart).ToString(), System.Globalization.CultureInfo.InvariantCulture));
                    else break;
                }
                if (pos < span.Length && span[pos] == '>') pos++;
                return new F8SecsItem(vals.ToArray());
            }
            default:
                throw new FormatException($"SML格式错误：未知的数据格式 '{name}'。");
        }
    }

    private static void SkipWhitespace(ReadOnlySpan<char> span, ref int pos)
    {
        while (pos < span.Length && char.IsWhiteSpace(span[pos]))
            pos++;
    }

    private static void SkipInlineSpaces(ReadOnlySpan<char> span, ref int pos)
    {
        while (pos < span.Length && span[pos] == ' ')
            pos++;
    }

    private static byte ParseUInt8(ReadOnlySpan<char> span, ref int pos)
    {
        var start = pos;
        while (pos < span.Length && char.IsDigit(span[pos]))
            pos++;
        return byte.Parse(span.Slice(start, pos - start).ToString());
    }

    private static bool IsHexChar(char c)
    {
        return (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
    }
}
