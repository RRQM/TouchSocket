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

public static partial class ReaderExtension
{
    #region Read

#if NET7_0_OR_GREATER
    /// <summary>
    /// 从字节读取器中读取指定长度的字节，按指定编码解码后解析为<typeparamref name="TNumber"/>。
    /// 零堆分配实现，通过<see cref="ISpanParsable{TSelf}"/>接口解析。
    /// </summary>
    /// <typeparam name="TReader">实现<see cref="IBytesReader"/>接口的读取器类型。</typeparam>
    /// <typeparam name="TNumber">实现<see cref="ISpanParsable{TSelf}"/>接口的数值类型。</typeparam>
    /// <param name="reader">字节读取器实例。</param>
    /// <param name="byteLength">要读取的字节数。</param>
    /// <param name="encoding">用于解码字节的编码。</param>
    /// <returns>解析得到的<typeparamref name="TNumber"/>值。</returns>
    public static TNumber ReadNumberAsString<TReader, TNumber>(ref TReader reader, int byteLength, Encoding encoding)
        where TReader : IBytesReader
        where TNumber : unmanaged, ISpanParsable<TNumber>
    {
        var byteSpan = reader.GetSpan(byteLength).Slice(0, byteLength);
        Span<char> charBuf = stackalloc char[64];
        var charsDecoded = encoding.GetChars(byteSpan, charBuf);
        if (!TNumber.TryParse(charBuf.Slice(0, charsDecoded), CultureInfo.InvariantCulture, out var result))
        {
            ThrowHelper.ThrowException($"无法将字符串解析为{typeof(TNumber).Name}。");
        }
        reader.Advance(byteLength);
        return result;
    }
#elif NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    /// <summary>
    /// 从字节读取器中读取指定长度的字节，按指定编码解码后解析为<typeparamref name="TNumber"/>。
    /// </summary>
    /// <typeparam name="TReader">实现<see cref="IBytesReader"/>接口的读取器类型。</typeparam>
    /// <typeparam name="TNumber">实现<see cref="IConvertible"/>接口的数值类型。</typeparam>
    /// <param name="reader">字节读取器实例。</param>
    /// <param name="byteLength">要读取的字节数。</param>
    /// <param name="encoding">用于解码字节的编码。</param>
    /// <returns>解析得到的<typeparamref name="TNumber"/>值。</returns>
    public static TNumber ReadNumberAsString<TReader, TNumber>(ref TReader reader, int byteLength, Encoding encoding)
        where TReader : IBytesReader
        where TNumber : unmanaged, IConvertible
    {
        var byteSpan = reader.GetSpan(byteLength).Slice(0, byteLength);
        Span<char> charBuf = stackalloc char[64];
        var charsDecoded = encoding.GetChars(byteSpan, charBuf);
        var str = new string(charBuf.Slice(0, charsDecoded));
        var result = (TNumber)Convert.ChangeType(str, typeof(TNumber), CultureInfo.InvariantCulture);
        reader.Advance(byteLength);
        return result;
    }
#else
    /// <summary>
    /// 从字节读取器中读取指定长度的字节，按指定编码解码后解析为<typeparamref name="TNumber"/>。
    /// </summary>
    /// <typeparam name="TReader">实现<see cref="IBytesReader"/>接口的读取器类型。</typeparam>
    /// <typeparam name="TNumber">实现<see cref="IConvertible"/>接口的数值类型。</typeparam>
    /// <param name="reader">字节读取器实例。</param>
    /// <param name="byteLength">要读取的字节数。</param>
    /// <param name="encoding">用于解码字节的编码。</param>
    /// <returns>解析得到的<typeparamref name="TNumber"/>值。</returns>
    public static TNumber ReadNumberAsString<TReader, TNumber>(ref TReader reader, int byteLength, Encoding encoding)
        where TReader : IBytesReader
        where TNumber : unmanaged, IConvertible
    {
        var byteSpan = reader.GetSpan(byteLength).Slice(0, byteLength);
        var str = byteSpan.ToString(encoding);
        var result = (TNumber)Convert.ChangeType(str, typeof(TNumber), CultureInfo.InvariantCulture);
        reader.Advance(byteLength);
        return result;
    }
#endif

    #endregion Read
}