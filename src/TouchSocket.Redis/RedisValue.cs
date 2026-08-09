using System.Globalization;
using TouchSocket.Core;

namespace TouchSocket.Redis;

/// <summary>
/// 表示一个 RESP 值。
/// </summary>
public sealed class RedisValue : IRequestInfo, IBytesBuilder
{
    private static readonly byte[] CRLF = new byte[] { (byte)'\r', (byte)'\n' };
    private static readonly byte[] ArrayPrefix = new byte[] { (byte)'*' };
    private static readonly byte[] BulkPrefix = new byte[] { (byte)'$' };
    private static readonly byte[] IntegerPrefix = new byte[] { (byte)':' };
    private static readonly byte[] NullBulkBytes = Encoding.ASCII.GetBytes("$-1\r\n");
    private static readonly byte[] NullArrayBytes = Encoding.ASCII.GetBytes("*-1\r\n");

    /// <summary>
    /// OK 响应。
    /// </summary>
    public static readonly RedisValue Ok = SimpleString("OK");

    /// <summary>
    /// PONG 响应。
    /// </summary>
    public static readonly RedisValue Pong = SimpleString("PONG");

    /// <summary>
    /// 空批量字符串响应。
    /// </summary>
    public static readonly RedisValue NullBulkString = new RedisValue(RedisValueKind.Null, false);

    /// <summary>
    /// Null array 响应。
    /// </summary>
    public static readonly RedisValue NullArray = new RedisValue(RedisValueKind.Null, true);

    private readonly bool m_nullArray;
    private readonly RedisValue[] m_items;
    private readonly byte[] m_bytes;
    private readonly ReadOnlyMemory<byte> m_memory;
    private readonly string m_text;
    private readonly bool m_hasMemory;
    private readonly bool m_textIsBulkString;
    private readonly long m_integer;

    private RedisValue(RedisValueKind kind, bool nullArray)
    {
        this.Kind = kind;
        this.m_nullArray = nullArray;
    }

    private RedisValue(RedisValueKind kind, string text)
    {
        this.Kind = kind;
        this.m_text = text ?? string.Empty;
    }

    private RedisValue(string bulkString)
    {
        this.Kind = RedisValueKind.BulkString;
        this.m_text = bulkString ?? string.Empty;
        this.m_textIsBulkString = true;
    }

    private RedisValue(byte[] bytes)
    {
        this.Kind = RedisValueKind.BulkString;
        this.m_bytes = bytes ?? System.Array.Empty<byte>();
    }

    private RedisValue(ReadOnlyMemory<byte> memory)
    {
        this.Kind = RedisValueKind.BulkString;
        this.m_memory = memory;
        this.m_hasMemory = true;
    }

    private RedisValue(long integer)
    {
        this.Kind = RedisValueKind.Integer;
        this.m_integer = integer;
    }

    private RedisValue(RedisValue[] items)
    {
        this.Kind = RedisValueKind.Array;
        this.m_items = items ?? System.Array.Empty<RedisValue>();
    }

    /// <summary>
    /// 获取 RESP 值类型。
    /// </summary>
    public RedisValueKind Kind { get; }

    /// <summary>
    /// 获取数组元素。
    /// </summary>
    public IReadOnlyList<RedisValue> Items => this.m_items ?? System.Array.Empty<RedisValue>();

    /// <summary>
    /// 获取整数值。
    /// </summary>
    public long Integer => this.m_integer;

    /// <summary>
    /// 获取当前值是否为错误响应。
    /// </summary>
    public bool IsError => this.Kind == RedisValueKind.Error;

    /// <inheritdoc/>
    public int MaxLength => 64 + this.GetPayloadLength();

    /// <summary>
    /// 创建简单字符串。
    /// </summary>
    public static RedisValue SimpleString(string value)
    {
        return new RedisValue(RedisValueKind.SimpleString, value);
    }

    /// <summary>
    /// 创建错误值。
    /// </summary>
    public static RedisValue Error(string value)
    {
        return new RedisValue(RedisValueKind.Error, value);
    }

    /// <summary>
    /// 创建整数值。
    /// </summary>
    public static RedisValue IntegerValue(long value)
    {
        return new RedisValue(value);
    }

    /// <summary>
    /// 使用已拥有的字节创建批量字符串。
    /// </summary>
    public static RedisValue BulkString(byte[] value)
    {
        return value is null ? NullBulkString : new RedisValue(value);
    }

    /// <summary>
    /// 使用字节内存创建批量字符串，发送时不复制。
    /// </summary>
    public static RedisValue BulkString(ReadOnlyMemory<byte> value)
    {
        return new RedisValue(value);
    }

    /// <summary>
    /// 使用 UTF-8 文本创建批量字符串，发送时再直接写入编码结果。
    /// </summary>
    public static RedisValue BulkString(string value)
    {
        return value is null ? NullBulkString : new RedisValue(value);
    }

    /// <summary>
    /// 创建数组。
    /// </summary>
    public static RedisValue Array(params RedisValue[] values)
    {
        return values is null ? NullArray : new RedisValue(values);
    }

    /// <summary>
    /// 使用字符串参数创建 Redis 命令数组。
    /// </summary>
    public static RedisValue Command(params string[] values)
    {
        if (values is null)
        {
            return NullArray;
        }

        var items = new RedisValue[values.Length];
        for (var i = 0; i < values.Length; i++)
        {
            items[i] = BulkString(values[i]);
        }
        return Array(items);
    }

    /// <summary>
    /// 使用字节内存参数创建 Redis 命令数组。
    /// </summary>
    public static RedisValue Command(string command, params ReadOnlyMemory<byte>[] values)
    {
        if (command is null)
        {
            return NullArray;
        }

        var items = new RedisValue[(values?.Length ?? 0) + 1];
        items[0] = BulkString(command);
        if (values != null)
        {
            for (var i = 0; i < values.Length; i++)
            {
                items[i + 1] = BulkString(values[i]);
            }
        }

        return Array(items);
    }

    /// <summary>
    /// 当当前值为 Redis 错误时抛出异常。
    /// </summary>
    public void ThrowIfError()
    {
        if (this.IsError)
        {
            ThrowHelper.ThrowException(this.AsString());
        }
    }

    /// <summary>
    /// 获取批量字符串的字节内容。
    /// </summary>
    public ReadOnlyMemory<byte> AsBytes()
    {
        if (this.Kind != RedisValueKind.BulkString)
        {
            return ReadOnlyMemory<byte>.Empty;
        }

        if (this.m_bytes is not null)
        {
            return this.m_bytes;
        }

        return this.m_hasMemory ? this.m_memory : Encoding.UTF8.GetBytes(this.m_text);
    }

    /// <summary>
    /// 将当前值作为 UTF-8 文本获取。
    /// </summary>
    public string AsString()
    {
        switch (this.Kind)
        {
            case RedisValueKind.SimpleString:
            case RedisValueKind.Error:
                return this.m_text;
            case RedisValueKind.BulkString:
                if (this.m_textIsBulkString)
                {
                    return this.m_text;
                }
                if (this.m_bytes is not null)
                {
                    return Encoding.UTF8.GetString(this.m_bytes, 0, this.m_bytes.Length);
                }
                return this.m_memory.Span.ToString(Encoding.UTF8);
            case RedisValueKind.Integer:
                return this.m_integer.ToString(CultureInfo.InvariantCulture);
            default:
                return null;
        }
    }

    /// <summary>
    /// 判断指定索引的数组参数是否等于给定 ASCII 文本。
    /// </summary>
    public bool ArgumentEquals(int index, string ascii)
    {
        if (this.Kind != RedisValueKind.Array || index < 0 || index >= this.m_items.Length)
        {
            return false;
        }

        return this.m_items[index].AsciiEquals(ascii);
    }

    /// <summary>
    /// 尝试以 UTF-8 字符串获取数组参数。
    /// </summary>
    public bool TryGetStringArgument(int index, out string value)
    {
        value = default;
        if (this.Kind != RedisValueKind.Array || index < 0 || index >= this.m_items.Length)
        {
            return false;
        }

        value = this.m_items[index].AsString();
        return value is not null;
    }

    /// <summary>
    /// 尝试以已拥有字节获取数组参数。
    /// </summary>
    public bool TryGetBytesArgument(int index, out byte[] value)
    {
        value = default;
        if (this.Kind != RedisValueKind.Array || index < 0 || index >= this.m_items.Length)
        {
            return false;
        }

        var item = this.m_items[index];
        if (item.Kind != RedisValueKind.BulkString)
        {
            return false;
        }

        if (item.m_bytes is not null)
        {
            value = item.m_bytes;
        }
        else if (item.m_hasMemory)
        {
            value = item.m_memory.ToArray();
        }
        else
        {
            value = Encoding.UTF8.GetBytes(item.m_text);
        }
        return true;
    }

    /// <summary>
    /// 尝试在不创建字符串的情况下解析整数参数。
    /// </summary>
    public bool TryGetInt64Argument(int index, out long value)
    {
        value = default;
        if (!this.TryGetBytesArgument(index, out var bytes))
        {
            return false;
        }

        return TryParseInt64(bytes, out value);
    }

    /// <inheritdoc/>
    public void Build<TWriter>(ref TWriter writer) where TWriter : IBytesWriter
    {
        switch (this.Kind)
        {
            case RedisValueKind.SimpleString:
                WritePrefixText(ref writer, (byte)'+', this.m_text);
                break;
            case RedisValueKind.Error:
                WritePrefixText(ref writer, (byte)'-', this.m_text);
                break;
            case RedisValueKind.Integer:
                writer.Write(IntegerPrefix);
                WriterExtension.WriteNumberAsString(ref writer, this.m_integer, Encoding.ASCII);
                writer.Write(CRLF);
                break;
            case RedisValueKind.BulkString:
                this.WriteBulkString(ref writer);
                break;
            case RedisValueKind.Array:
                writer.Write(ArrayPrefix);
                WriterExtension.WriteNumberAsString(ref writer, this.m_items.Length, Encoding.ASCII);
                writer.Write(CRLF);
                for (var i = 0; i < this.m_items.Length; i++)
                {
                    this.m_items[i].Build(ref writer);
                }
                break;
            case RedisValueKind.Null:
            default:
                writer.Write(this.m_nullArray ? NullArrayBytes : NullBulkBytes);
                break;
        }
    }

    internal bool AsciiEquals(string ascii)
    {
        if (ascii is null)
        {
            return false;
        }

        if (this.Kind == RedisValueKind.BulkString)
        {
            if (this.m_bytes is not null)
            {
                return AsciiEquals(this.m_bytes, ascii);
            }
            if (this.m_hasMemory)
            {
                return AsciiEquals(this.m_memory.Span, ascii);
            }
            return string.Equals(this.m_text, ascii, StringComparison.OrdinalIgnoreCase);
        }

        if (this.Kind == RedisValueKind.SimpleString)
        {
            return string.Equals(this.m_text, ascii, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    internal static bool TryParseInt64(byte[] bytes, out long value)
    {
        return TryParseInt64((ReadOnlySpan<byte>)bytes, out value);
    }

    internal static bool TryParseInt64(ReadOnlySpan<byte> bytes, out long value)
    {
        value = 0;
        if (bytes.Length == 0)
        {
            return false;
        }

        var index = 0;
        var negative = false;
        if (bytes[0] == (byte)'-')
        {
            negative = true;
            index = 1;
        }

        if (index >= bytes.Length)
        {
            return false;
        }

        long result = 0;
        for (; index < bytes.Length; index++)
        {
            var b = bytes[index];
            if (b < (byte)'0' || b > (byte)'9')
            {
                return false;
            }

            result = (result * 10) + (b - (byte)'0');
        }

        value = negative ? -result : result;
        return true;
    }

    private int GetPayloadLength()
    {
        switch (this.Kind)
        {
            case RedisValueKind.SimpleString:
            case RedisValueKind.Error:
                return Encoding.UTF8.GetByteCount(this.m_text ?? string.Empty);
            case RedisValueKind.BulkString:
                if (this.m_bytes is not null)
                {
                    return this.m_bytes.Length;
                }
                return this.m_hasMemory ? this.m_memory.Length : Encoding.UTF8.GetByteCount(this.m_text ?? string.Empty);
            case RedisValueKind.Array:
                var length = 0;
                for (var i = 0; i < this.m_items.Length; i++)
                {
                    length += this.m_items[i].MaxLength;
                }
                return length;
            default:
                return 0;
        }
    }

    private static bool AsciiEquals(byte[] bytes, string ascii)
    {
        return bytes is not null && AsciiEquals((ReadOnlySpan<byte>)bytes, ascii);
    }

    private static bool AsciiEquals(ReadOnlySpan<byte> bytes, string ascii)
    {
        if (bytes.Length != ascii.Length)
        {
            return false;
        }

        for (var i = 0; i < bytes.Length; i++)
        {
            var left = bytes[i];
            var right = (byte)ascii[i];
            if (left >= (byte)'a' && left <= (byte)'z')
            {
                left = (byte)(left - 32);
            }
            if (right >= (byte)'a' && right <= (byte)'z')
            {
                right = (byte)(right - 32);
            }

            if (left != right)
            {
                return false;
            }
        }

        return true;
    }

    private void WriteBulkString<TWriter>(ref TWriter writer)
        where TWriter : IBytesWriter
    {
        writer.Write(BulkPrefix);
        if (this.m_bytes is not null)
        {
            WriterExtension.WriteNumberAsString(ref writer, this.m_bytes.Length, Encoding.ASCII);
            writer.Write(CRLF);
            writer.Write(this.m_bytes);
        }
        else if (this.m_hasMemory)
        {
            WriterExtension.WriteNumberAsString(ref writer, this.m_memory.Length, Encoding.ASCII);
            writer.Write(CRLF);
            writer.Write(this.m_memory.Span);
        }
        else
        {
            WriterExtension.WriteNumberAsString(ref writer, Encoding.UTF8.GetByteCount(this.m_text), Encoding.ASCII);
            writer.Write(CRLF);
            WriterExtension.WriteNormalString(ref writer, this.m_text, Encoding.UTF8);
        }
        writer.Write(CRLF);
    }

    private static void WritePrefixText<TWriter>(ref TWriter writer, byte prefix, string text)
        where TWriter : IBytesWriter
    {
        var span = writer.GetSpan(1);
        span[0] = prefix;
        writer.Advance(1);
        WriterExtension.WriteNormalString(ref writer, text ?? string.Empty, Encoding.UTF8);
        writer.Write(CRLF);
    }
}
