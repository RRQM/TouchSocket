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

namespace TouchSocket.CoAP;

/// <summary>
/// 表示一个 CoAP 选项（RFC 7252 Section 3.1）。
/// </summary>
public sealed class CoAPOption
{
    /// <summary>
    /// 初始化 <see cref="CoAPOption"/> 类的新实例，使用原始字节值。
    /// </summary>
    /// <param name="number">选项编号。</param>
    /// <param name="value">选项值的字节序列。</param>
    public CoAPOption(CoAPOptionNumber number, ReadOnlyMemory<byte> value)
    {
        this.Number = number;
        this.Value = value;
    }

    /// <summary>
    /// 初始化 <see cref="CoAPOption"/> 类的新实例，使用 UTF-8 字符串值。
    /// </summary>
    /// <param name="number">选项编号。</param>
    /// <param name="value">选项值字符串，将使用 UTF-8 编码。</param>
    public CoAPOption(CoAPOptionNumber number, string value)
        : this(number, Encoding.UTF8.GetBytes(value))
    {
    }

    /// <summary>
    /// 初始化 <see cref="CoAPOption"/> 类的新实例，使用无符号整数值（可变长大端编码）。
    /// </summary>
    /// <param name="number">选项编号。</param>
    /// <param name="value">选项值，将使用最小字节数的大端整数编码。</param>
    public CoAPOption(CoAPOptionNumber number, uint value)
        : this(number, EncodeUInt(value))
    {
    }

    /// <summary>
    /// 获取选项编号。
    /// </summary>
    public CoAPOptionNumber Number { get; }

    /// <summary>
    /// 获取选项的原始字节值。
    /// </summary>
    public ReadOnlyMemory<byte> Value { get; }

    /// <summary>
    /// 获取选项值的字符串表示（UTF-8 解码）。
    /// </summary>
    public string GetStringValue()
    {
        return Encoding.UTF8.GetString(this.Value.ToArray());
    }

    /// <summary>
    /// 获取选项值的无符号 32 位整数表示（大端解码）。
    /// </summary>
    public uint GetUInt32Value()
    {
        var result = 0u;
        foreach (var b in this.Value.Span)
        {
            result = (result << 8) | b;
        }

        return result;
    }

    private static byte[] EncodeUInt(uint value)
    {
        if (value == 0)
        {
            return Array.Empty<byte>();
        }

        if (value <= 0xFF)
        {
            return new[] { (byte)value };
        }

        if (value <= 0xFFFF)
        {
            return new[] { (byte)(value >> 8), (byte)value };
        }

        if (value <= 0xFFFFFF)
        {
            return new[] { (byte)(value >> 16), (byte)(value >> 8), (byte)value };
        }

        return new[] { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value };
    }
}
