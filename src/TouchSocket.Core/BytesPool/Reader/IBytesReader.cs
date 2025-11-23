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

using System.Buffers;

namespace TouchSocket.Core;

/// <summary>
/// 表示字节读取器的接口，提供对字节序列的读取、跳过和获取操作。
/// </summary>
public interface IBytesReader
{
    /// <summary>
    /// 获取或设置已读取的字节数。
    /// </summary>
    long BytesRead { get; set; }

    /// <summary>
    /// 获取剩余可读取的字节数。
    /// </summary>
    long BytesRemaining { get; }

    /// <summary>
    /// 获取当前可读取的字节序列。
    /// </summary>
    ReadOnlySequence<byte> Sequence { get; }

    /// <summary>
    /// 获取总的字节序列。
    /// </summary>
    ReadOnlySequence<byte> TotalSequence { get; }

    /// <summary>
    /// 推进指定数量的字节。
    /// </summary>
    /// <param name="count">要推进的字节数。</param>
    void Advance(int count);

    /// <summary>
    /// 获取指定数量的只读内存字节块。
    /// </summary>
    /// <param name="count">要获取的字节数。</param>
    /// <returns>只读内存字节块。</returns>
    /// <remarks>
    /// 字节块最大生命期与当前<see cref="IBytesReader"/>的生命周期一致。
    /// 但是当多次调用时，最后一次调用的返回值会覆盖之前的返回值，导致之前的返回值失效。
    /// 所以请确保在下次获取返回值之前使用完返回的内存，避免在多次调用后使用旧的内存块。
    /// </remarks>
    ReadOnlyMemory<byte> GetMemory(int count);

    /// <summary>
    /// 获取指定数量的只读字节跨度。
    /// </summary>
    /// <param name="count">要获取的字节数。</param>
    /// <returns>只读字节跨度。</returns>
    /// <remarks>
    /// 字节块最大生命期与当前<see cref="IBytesReader"/>的生命周期一致。
    /// 但是当多次调用时，最后一次调用的返回值会覆盖之前的返回值，导致之前的返回值失效。
    /// 所以请确保在下次获取返回值之前使用完返回的内存，避免在多次调用后使用旧的内存块。
    /// </remarks>
    ReadOnlySpan<byte> GetSpan(int count);

    /// <summary>
    /// 读取字节到指定的跨度中。
    /// </summary>
    /// <param name="span">目标跨度。</param>
    /// <returns>实际读取的字节数。</returns>
    int Read(Span<byte> span);
}