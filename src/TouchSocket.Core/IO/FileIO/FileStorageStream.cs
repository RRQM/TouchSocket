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

using System.IO;
using System.Threading;

namespace TouchSocket.Core;

/// <summary>
/// FileStorageStream。
/// </summary>
public partial class FileStorageStream : Stream
{
    private long m_position;
    private int m_dis = 1;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="fileStorage"></param>
    public FileStorageStream(FileStorage fileStorage)
    {
        this.FileStorage = ThrowHelper.ThrowArgumentNullExceptionIf(fileStorage, nameof(fileStorage));
    }

    /// <summary>
    /// 析构函数
    /// </summary>
    ~FileStorageStream()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        this.Dispose(disposing: false);
    }

    /// <inheritdoc/>
    public override bool CanRead => this.FileStorage.FileStream.CanRead;

    /// <inheritdoc/>
    public override bool CanSeek => this.FileStorage.FileStream.CanSeek;

    /// <inheritdoc/>
    public override bool CanWrite => this.FileStorage.FileStream.CanWrite;

    /// <summary>
    /// 文件存储器
    /// </summary>
    public FileStorage FileStorage { get; private set; }

    /// <inheritdoc/>
    public override long Length => this.FileStorage.FileStream.Length;

    /// <inheritdoc/>
    public override long Position { get => this.m_position; set => this.m_position = value; }

    /// <inheritdoc/>

    public override void Flush()
    {
        this.FileStorage.Flush();
    }

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count)
    {
        var r = this.FileStorage.Read(this.m_position, new System.Span<byte>(buffer, offset, count));
        this.m_position += r;
        return r;
    }

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.Begin:
                this.m_position = offset;
                break;

            case SeekOrigin.Current:
                this.m_position += offset;
                break;

            case SeekOrigin.End:
                this.m_position = this.Length + offset;
                break;
        }
        return this.m_position;
    }

    /// <inheritdoc/>
    public override void SetLength(long value)
    {
        this.FileStorage.FileStream.SetLength(value);
    }

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count)
    {
        this.FileStorage.Write(this.m_position, new System.ReadOnlySpan<byte>(buffer, offset, count));
        this.m_position += count;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (Interlocked.Decrement(ref this.m_dis) == 0)
        {
            FilePool.TryReleaseFile(this.FileStorage.Path);
            this.FileStorage = null;
        }
        base.Dispose(disposing);
    }
}