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
internal sealed class ReadOnlyStream : Stream
{
    private readonly IByteBlockReader m_reader;

    public ReadOnlyStream(IByteBlockReader reader)
    {
        this.m_reader = reader;
    }
    public override bool CanRead => this.m_reader.CanReadLength>0;

    public override bool CanSeek => true;

    public override bool CanWrite => false;

    public override long Length => this.m_reader.Length;

    public override long Position { get => this.m_reader.Position; set => this.m_reader.Position = (int)value; }

    public override void Flush()
    {

    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return this.m_reader.Read(new Span<byte>(buffer, offset, count));
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:
                this.m_reader.Position = (int)offset;
                break;

            case SeekOrigin.Current:
                this.m_reader.Position += (int)offset;
                break;

            case SeekOrigin.End:
                this.m_reader.Position = (int)(this.Length + offset);
                break;
        }

        return this.m_reader.Position;
    }

    public override void SetLength(long value)
    {
        throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }
}
