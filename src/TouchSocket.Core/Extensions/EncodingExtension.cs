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

#if NETSTANDARD2_0 || NET462

using System.Runtime.InteropServices;
using System.Text;

namespace TouchSocket.Core;

/// <summary>
/// 为旧版目标框架提供 <see cref="Encoding"/> 的 <see cref="System.ReadOnlySpan{T}"/> 相关重载的垫片扩展方法。
/// </summary>
internal static class EncodingExtension
{
    /// <summary>
    /// 将指定字符跨度中的字符编码为字节跨度。
    /// </summary>
    /// <param name="encoding">使用的字符编码。</param>
    /// <param name="chars">要编码的字符只读跨度。</param>
    /// <param name="bytes">用于存储编码结果的字节跨度。</param>
    /// <returns>写入 <paramref name="bytes"/> 的实际字节数。</returns>
    internal static unsafe int GetBytes(this Encoding encoding, ReadOnlySpan<char> chars, Span<byte> bytes)
    {
        if (chars.IsEmpty)
        {
            return 0;
        }

        fixed (char* c = &MemoryMarshal.GetReference(chars))
        fixed (byte* b = &MemoryMarshal.GetReference(bytes))
        {
            return encoding.GetBytes(c, chars.Length, b, bytes.Length);
        }
    }

    /// <summary>
    /// 计算将指定字符跨度中的字符编码后所需的字节数。
    /// </summary>
    /// <param name="encoding">使用的字符编码。</param>
    /// <param name="chars">要计算的字符只读跨度。</param>
    /// <returns>编码所需的字节数。</returns>
    internal static unsafe int GetByteCount(this Encoding encoding, ReadOnlySpan<char> chars)
    {
        if (chars.IsEmpty)
        {
            return 0;
        }

        fixed (char* c = &MemoryMarshal.GetReference(chars))
        {
            return encoding.GetByteCount(c, chars.Length);
        }
    }
}

#endif
