using TouchSocket.Core;

namespace TouchSocket.Redis;

internal static class RedisRespParser
{
    private static readonly byte[] CRLF = new byte[] { (byte)'\r', (byte)'\n' };

    public static bool TryParse<TReader>(ref TReader reader, out RedisValue value)
        where TReader : IBytesReader
    {
        value = default;
        if (reader.BytesRemaining < 1)
        {
            return false;
        }

        var start = reader.BytesRead;
        var prefix = reader.GetSpan(1)[0];
        reader.Advance(1);

        switch (prefix)
        {
            case (byte)'+':
                return TryReadSimpleString(ref reader, RedisValueKind.SimpleString, out value, start);
            case (byte)'-':
                return TryReadSimpleString(ref reader, RedisValueKind.Error, out value, start);
            case (byte)':':
                if (TryReadInt64Line(ref reader, out var number))
                {
                    value = RedisValue.IntegerValue(number);
                    return true;
                }
                break;
            case (byte)'$':
                return TryReadBulkString(ref reader, out value, start);
            case (byte)'*':
                return TryReadArray(ref reader, out value, start);
        }

        reader.BytesRead = start;
        return false;
    }

    private static bool TryReadSimpleString<TReader>(ref TReader reader, RedisValueKind kind, out RedisValue value, long start)
        where TReader : IBytesReader
    {
        value = default;
        if (!TryReadLineString(ref reader, out var text))
        {
            reader.BytesRead = start;
            return false;
        }

        value = kind == RedisValueKind.SimpleString ? RedisValue.SimpleString(text) : RedisValue.Error(text);
        return true;
    }

    private static bool TryReadBulkString<TReader>(ref TReader reader, out RedisValue value, long start)
        where TReader : IBytesReader
    {
        value = default;
        if (!TryReadInt64Line(ref reader, out var length))
        {
            reader.BytesRead = start;
            return false;
        }

        if (length == -1)
        {
            value = RedisValue.NullBulkString;
            return true;
        }

        if (length < 0 || length > int.MaxValue)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(length));
        }

        var byteLength = (int)length;
        if (reader.BytesRemaining < byteLength + 2)
        {
            reader.BytesRead = start;
            return false;
        }

        var spanWithCrlf = reader.GetSpan(byteLength + 2);
        if (spanWithCrlf[byteLength] != (byte)'\r' || spanWithCrlf[byteLength + 1] != (byte)'\n')
        {
            ThrowHelper.ThrowException("Invalid Redis bulk string terminator.");
        }

        var bytes = new byte[byteLength];
        spanWithCrlf.Slice(0, byteLength).CopyTo(bytes);
        reader.Advance(byteLength + 2);
        value = RedisValue.BulkString(bytes);
        return true;
    }

    private static bool TryReadArray<TReader>(ref TReader reader, out RedisValue value, long start)
        where TReader : IBytesReader
    {
        value = default;
        if (!TryReadInt64Line(ref reader, out var length))
        {
            reader.BytesRead = start;
            return false;
        }

        if (length == -1)
        {
            value = RedisValue.NullArray;
            return true;
        }

        if (length < 0 || length > int.MaxValue)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException(nameof(length));
        }

        var count = (int)length;
        var items = new RedisValue[count];
        for (var i = 0; i < count; i++)
        {
            if (!TryParse(ref reader, out var item))
            {
                reader.BytesRead = start;
                return false;
            }

            items[i] = item;
        }

        value = RedisValue.Array(items);
        return true;
    }

    private static bool TryReadInt64Line<TReader>(ref TReader reader, out long value)
        where TReader : IBytesReader
    {
        value = default;
        var index = ReaderExtension.IndexOf(ref reader, CRLF);
        if (index < 0 || index > int.MaxValue)
        {
            return false;
        }

        var length = (int)index;
        if (!RedisValue.TryParseInt64(reader.GetSpan(length), out value))
        {
            ThrowHelper.ThrowException("Invalid Redis integer value.");
        }

        reader.Advance(length + 2);
        return true;
    }

    private static bool TryReadLineBytes<TReader>(ref TReader reader, out byte[] bytes)
        where TReader : IBytesReader
    {
        bytes = default;
        var index = ReaderExtension.IndexOf(ref reader, CRLF);
        if (index < 0 || index > int.MaxValue)
        {
            return false;
        }

        var length = (int)index;
        bytes = new byte[length];
        reader.GetSpan(length).CopyTo(bytes);
        reader.Advance(length + 2);
        return true;
    }

    private static bool TryReadLineString<TReader>(ref TReader reader, out string value)
        where TReader : IBytesReader
    {
        value = default;
        var index = ReaderExtension.IndexOf(ref reader, CRLF);
        if (index < 0 || index > int.MaxValue)
        {
            return false;
        }

        var length = (int)index;
        value = reader.GetSpan(length).ToString(Encoding.UTF8);
        reader.Advance(length + 2);
        return true;
    }
}
