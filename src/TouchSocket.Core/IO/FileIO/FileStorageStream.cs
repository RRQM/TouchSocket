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


namespace TouchSocket.Core;

/// <summary>
/// 文件存储流
/// </summary>
internal sealed partial class FileStorageStream : Stream
{
    private long m_position;
    private FileStorage m_storage;
    private bool m_disposed;

    internal FileStorageStream(FileStorage storage)
    {
        this.m_storage = storage;
    }

    /// <inheritdoc/>
    public override bool CanRead => this.m_storage.CanRead;

    /// <inheritdoc/>
    public override bool CanSeek => this.m_storage.CanSeek;

    /// <inheritdoc/>
    public override bool CanWrite => this.m_storage.CanWrite;

    /// <inheritdoc/>
    public override long Length
    {
        get
        {
            this.ThrowIfDisposed();
            return this.m_storage.Length;
        }
    }

    /// <inheritdoc/>
    public override long Position
    {
        get => this.m_position;
        set => this.m_position = value;
    }

    /// <inheritdoc/>
    public override void Flush()
    {
        this.ThrowIfDisposed();
        this.m_storage.Flush();
    }

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count)
    {
        this.ThrowIfDisposed();
#if NET6_0_OR_GREATER
        var span = buffer.AsSpan(offset, count);
        var readCount = this.m_storage.Read(this.m_position, span);
        this.m_position += readCount;
        return readCount;
#else
        return this.ReadCore(buffer, offset, count);
#endif
    }

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin)
    {
        this.ThrowIfDisposed();
        this.m_position = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => this.m_position + offset,
            SeekOrigin.End => this.m_storage.Length + offset,
            _ => throw new ArgumentException("Invalid seek origin", nameof(origin))
        };
        return this.m_position;
    }

    /// <inheritdoc/>
    public override void SetLength(long value)
    {
        this.ThrowIfDisposed();
        this.m_storage.SetLength(value);
    }

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count)
    {
        this.ThrowIfDisposed();
#if NET6_0_OR_GREATER
        var span = buffer.AsSpan(offset, count);
        this.m_storage.Write(this.m_position, span);
        this.m_position += count;
#else
        this.WriteCore(buffer, offset, count);
#endif
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (this.m_disposed)
        {
            return;
        }
        this.m_disposed = true;

        if (disposing && this.m_storage != null)
        {
            FilePool.ReleaseFile(this.m_storage);
            this.m_storage = null;
        }

        base.Dispose(disposing);
    }

    private void ThrowIfDisposed()
    {
        if (this.m_disposed)
        {
            ThrowHelper.ThrowObjectDisposedException(this);
        }
    }
}

#if NET6_0_OR_GREATER
internal sealed partial class FileStorageStream
{
    /// <inheritdoc/>
    public override int Read(Span<byte> buffer)
    {
        this.ThrowIfDisposed();
        var count = this.m_storage.Read(this.m_position, buffer);
        this.m_position += count;
        return count;
    }

    /// <inheritdoc/>
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        this.ThrowIfDisposed();
        this.m_storage.Write(this.m_position, buffer);
        this.m_position += buffer.Length;
    }
    /// <inheritdoc/>
    public override Task FlushAsync(CancellationToken cancellationToken)
    {
        this.ThrowIfDisposed();
        return this.m_storage.FlushAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        this.ThrowIfDisposed();
        var memory = buffer.AsMemory(offset, count);
        var readCount = await this.m_storage.ReadAsync(this.m_position, memory, cancellationToken).ConfigureAwait(false);
        this.m_position += readCount;
        return readCount;
    }

    /// <inheritdoc/>
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        this.ThrowIfDisposed();
        var count = await this.m_storage.ReadAsync(this.m_position, buffer, cancellationToken).ConfigureAwait(false);
        this.m_position += count;
        return count;
    }

    /// <inheritdoc/>
    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        this.ThrowIfDisposed();
        var memory = buffer.AsMemory(offset, count);
        await this.m_storage.WriteAsync(this.m_position, memory, cancellationToken).ConfigureAwait(false);
        this.m_position += count;
    }

    /// <inheritdoc/>
    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        this.ThrowIfDisposed();
        await this.m_storage.WriteAsync(this.m_position, buffer, cancellationToken).ConfigureAwait(false);
        this.m_position += buffer.Length;
    }
}

#endif

#if NETSTANDARD2_0 || NETSTANDARD2_1 || NET462


internal sealed partial class FileStorageStream
{
    private int ReadCore(byte[] buffer, int offset, int count)
    {
        var readCount = this.m_storage.Read(this.m_position, buffer, offset, count);
        this.m_position += readCount;
        return readCount;
    }

    private void WriteCore(byte[] buffer, int offset, int count)
    {
        this.m_storage.Write(this.m_position, buffer, offset, count);
        this.m_position += count;
    }

    /// <inheritdoc/>
    public override Task FlushAsync(CancellationToken cancellationToken)
    {
        this.ThrowIfDisposed();
        return this.m_storage.FlushAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        this.ThrowIfDisposed();
        var readCount = await this.m_storage.ReadAsync(this.m_position, new Memory<byte>(buffer, offset, count), cancellationToken).ConfigureAwait(false);
        this.m_position += readCount;
        return readCount;
    }

    /// <inheritdoc/>
    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        this.ThrowIfDisposed();
        await this.m_storage.WriteAsync(this.m_position, new ReadOnlyMemory<byte>(buffer, offset, count), cancellationToken).ConfigureAwait(false);
        this.m_position += count;
    }
}

#endif
