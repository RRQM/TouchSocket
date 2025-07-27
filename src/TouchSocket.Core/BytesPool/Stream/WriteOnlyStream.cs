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
using System.IO;

namespace TouchSocket.Core;
internal sealed class WriteOnlyStream : Stream
{
    private readonly IByteBlockWriter m_writer;

    public WriteOnlyStream(IByteBlockWriter writer)
    {
        this.m_writer = writer;
    }

    public override bool CanRead => false;

    public override bool CanSeek => true;

    public override bool CanWrite => true;

    public override long Length => this.m_writer.Length;

    public override long Position { get => this.m_writer.Position; set => this.m_writer.Position = (int)value; }

    public override void Flush()
    {

    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:
                this.m_writer.Position = (int)offset;
                break;

            case SeekOrigin.Current:
                this.m_writer.Position += (int)offset;
                break;

            case SeekOrigin.End:
                this.m_writer.Position = (int)(this.Length + offset);
                break;
        }

        return this.m_writer.Position;
    }

    public override void SetLength(long value)
    {
        this.m_writer.SetLength((int)value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        this.m_writer.Write(new ReadOnlySpan<byte>(buffer, offset, count));
    }
}
