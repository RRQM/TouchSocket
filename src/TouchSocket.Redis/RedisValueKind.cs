namespace TouchSocket.Redis;

/// <summary>
/// RESP 值类型。
/// </summary>
public enum RedisValueKind
{
    /// <summary>
    /// 空批量字符串或空数组。
    /// </summary>
    Null,

    /// <summary>
    /// 简单字符串，例如：+OK。
    /// </summary>
    SimpleString,

    /// <summary>
    /// 错误，例如：-ERR。
    /// </summary>
    Error,

    /// <summary>
    /// 整数，例如：:1。
    /// </summary>
    Integer,

    /// <summary>
    /// 批量字符串，例如：$3\r\nGET。
    /// </summary>
    BulkString,

    /// <summary>
    /// 数组，例如：*2。
    /// </summary>
    Array
}
