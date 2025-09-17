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

using System;

namespace TouchSocket.Core;

/// <summary>
/// 表示字节写入器的锚点，用于记录特定位置和获取对应的字节跨度。
/// </summary>
/// <typeparam name="TWriter">实现<see cref="IBytesWriter"/>接口的字节写入器类型。</typeparam>
/// <remarks>
/// WriterAnchor是一个只读的ref结构体，用于在字节写入操作中标记特定位置，
/// 并在后续操作中能够回退到该位置或获取从该位置开始的字节跨度。
/// 支持版本检查以确保数据一致性，并提供回退功能用于高级字节写入场景。
/// </remarks>
public readonly ref struct WriterAnchor<TWriter>
    where TWriter : IBytesWriter
{
    private readonly long m_position;
    private readonly Span<byte> m_span;
    private readonly short m_version;
    private readonly int m_size;

    /// <summary>
    /// 初始化写入器锚点的新实例。
    /// </summary>
    /// <param name="writer">字节写入器引用。</param>
    /// <param name="size">要预留的字节跨度大小。</param>
    /// <remarks>
    /// 此构造函数会记录当前写入器的位置和版本信息，并预留指定大小的字节跨度。
    /// 同时会推进写入器的位置以确保后续写入操作不会覆盖预留的空间。
    /// </remarks>
    public WriterAnchor(ref TWriter writer, int size)
    {
        this.m_version = writer.Version;
        var span = writer.GetSpan(size).Slice(0, size);
        this.m_span = span;
        writer.Advance(size);
        this.m_position = writer.WrittenCount;
        this.m_size = size;
    }

    /// <summary>
    /// 回退到锚点位置并获取对应的字节跨度。
    /// </summary>
    /// <param name="writer">字节写入器引用。</param>
    /// <param name="length">输出参数，表示从锚点位置到当前位置的数据长度。</param>
    /// <returns>对应锚点位置的字节跨度。</returns>
    /// <exception cref="InvalidOperationException">当写入器版本不匹配或不支持回退操作时抛出异常。</exception>
    /// <remarks>
    /// 此方法会计算从锚点创建时到当前位置的数据长度。
    /// 如果写入器版本未改变，则直接返回缓存的字节跨度；
    /// 如果版本已改变但写入器支持回退操作，则通过回退重新获取字节跨度；
    /// 否则抛出异常表示操作无效。
    /// </remarks>
    public Span<byte> Rewind(ref TWriter writer, out int length)
    {
        length = (int)(writer.WrittenCount - this.m_position);

        if (this.m_version == writer.Version)
        {
            return this.m_span;
        }
        else if (writer.SupportsRewind)
        {
            writer.Advance(-(length+m_size));
            var span = writer.GetSpan(this.m_span.Length).Slice(0, this.m_span.Length);
            writer.Advance(length + m_size);
            return span;
        }
        else
        {
            ThrowHelper.ThrowInvalidOperationException("Writer version mismatch or does not support rewind.");
            return [];
        }
    }
}