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

public readonly ref struct WriterAnchor<TWriter>
    where TWriter : IBytesWriter
{
    private readonly long m_position;
    private readonly Span<byte> m_span;
    private readonly short m_version;

    public WriterAnchor(ref TWriter writer, int size)
    {
        this.m_version = writer.Version;
        this.m_position = writer.WrittenCount;
        var span = writer.GetSpan(size).Slice(0, size);
        this.m_span = span;
        writer.Advance(size);
    }

    public Span<byte> Rewind(ref TWriter writer, out int length)
    {
        length = (int)(writer.WrittenCount - this.m_position);

        if (this.m_version == writer.Version)
        {
            return this.m_span;
        }
        else if (writer.SupportsRewind)
        {
            writer.Advance(-length);
            var span = writer.GetSpan(this.m_span.Length).Slice(0, this.m_span.Length);
            writer.Advance(length);
            return span;
        }
        else
        {
            ThrowHelper.ThrowInvalidOperationException("Writer version mismatch or does not support rewind.");
            return [];
        }
    }
}