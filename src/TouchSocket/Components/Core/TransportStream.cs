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
using System.IO.Pipelines;

namespace TouchSocket.Sockets;

/// <summary>
/// 基于管道的流实现
/// </summary>
public class TransportStream : Stream
{

    private bool m_disposed;
    private readonly PipeWriter m_writer;
    private readonly PipeReader m_reader;

    /// <summary>
    /// 初始化 <see cref="TransportStream"/> 类的新实例
    /// </summary>
    /// <param name="transport">传输层对象</param>
    public TransportStream(ITransport transport)
    {
        transport = ThrowHelper.ThrowArgumentNullExceptionIf(transport, nameof(transport));
        this.m_writer = transport.Writer;
        this.m_reader = transport.Reader;

    }

    /// <inheritdoc/>
    public override bool CanRead => !this.m_disposed;

    /// <inheritdoc/>
    public override bool CanSeek => false;

    /// <inheritdoc/>
    public override bool CanWrite => !this.m_disposed;

    /// <inheritdoc/>
    public override long Length => throw new NotSupportedException();

    /// <inheritdoc/>
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    /// <inheritdoc/>
    public override void Flush()
    {
        this.ThrowIfDisposed();
        this.FlushAsync().GetAwaiter().GetResult();
    }

    /// <inheritdoc/>
    public override async Task FlushAsync(CancellationToken cancellationToken)
    {
        this.ThrowIfDisposed();

        var result = await this.m_writer.FlushAsync(cancellationToken).ConfigureAwait(false);
        if (result.IsCanceled)
        {
            throw new OperationCanceledException();
        }
    }

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count)
    {
        return this.ReadAsync(buffer, offset, count).GetAwaiter().GetResult();
    }

    /// <inheritdoc/>
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        this.ThrowIfDisposed();

        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));
        if (offset < 0 || offset > buffer.Length)
            throw new ArgumentOutOfRangeException(nameof(offset));
        if (count < 0 || offset + count > buffer.Length)
            throw new ArgumentOutOfRangeException(nameof(count));

        if (count == 0)
            return 0;

        var result = await this.m_reader.ReadAsync(cancellationToken).ConfigureAwait(false);

        if (result.IsCanceled)
        {
            throw new OperationCanceledException();
        }

        var sequence = result.Buffer;
        var bytesToRead = (int)Math.Min(count, sequence.Length);

        if (bytesToRead == 0)
        {
            this.m_reader.AdvanceTo(sequence.Start);
            return result.IsCompleted ? 0 : await this.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
        }

        var sliced = sequence.Slice(0, bytesToRead);
        sliced.CopyTo(new Span<byte>(buffer, offset, bytesToRead));

        this.m_reader.AdvanceTo(sliced.End);

        return bytesToRead;
    }

#if !NETFRAMEWORK && !NETSTANDARD2_0
    /// <inheritdoc/>
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        this.ThrowIfDisposed();

        if (buffer.Length == 0)
            return 0;

        var result = await this.m_reader.ReadAsync(cancellationToken).ConfigureAwait(false);

        if (result.IsCanceled)
        {
            throw new OperationCanceledException();
        }

        var sequence = result.Buffer;
        var bytesToRead = (int)Math.Min(buffer.Length, sequence.Length);

        if (bytesToRead == 0)
        {
            this.m_reader.AdvanceTo(sequence.Start);
            return result.IsCompleted ? 0 : await this.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
        }

        var sliced = sequence.Slice(0, bytesToRead);
        sliced.CopyTo(buffer.Span);

        this.m_reader.AdvanceTo(sliced.End);

        return bytesToRead;
    }
#endif

    /// <inheritdoc/>
    public override int ReadByte()
    {
        var buffer = new byte[1];
        var bytesRead = this.Read(buffer, 0, 1);
        return bytesRead == 0 ? -1 : buffer[0];
    }

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc/>
    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count)
    {
        this.WriteAsync(buffer, offset, count).GetAwaiter().GetResult();
    }

    /// <inheritdoc/>
    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        this.ThrowIfDisposed();

        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));
        if (offset < 0 || offset > buffer.Length)
            throw new ArgumentOutOfRangeException(nameof(offset));
        if (count < 0 || offset + count > buffer.Length)
            throw new ArgumentOutOfRangeException(nameof(count));

        if (count == 0)
            return;

        var memory = this.m_writer.GetMemory(count);
        var source = new ReadOnlySpan<byte>(buffer, offset, count);
        source.CopyTo(memory.Span);
        this.m_writer.Advance(count);

        var result = await this.m_writer.FlushAsync(cancellationToken).ConfigureAwait(false);
        if (result.IsCanceled)
        {
            throw new OperationCanceledException();
        }
    }

#if !NETFRAMEWORK && !NETSTANDARD2_0
    /// <inheritdoc/>
    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        this.ThrowIfDisposed();

        if (buffer.Length == 0)
            return;

        var result = await this.m_writer.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
        if (result.IsCanceled)
        {
            throw new OperationCanceledException();
        }
    }
#endif

    /// <inheritdoc/>
    public override void WriteByte(byte value)
    {
        var buffer = new byte[] { value };
        this.Write(buffer, 0, 1);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (!this.m_disposed && disposing)
        {
            this.m_disposed = true;
        }

        base.Dispose(disposing);
    }

#if !NETFRAMEWORK && !NETSTANDARD2_0
    /// <inheritdoc/>
    public override async ValueTask DisposeAsync()
    {
        if (!this.m_disposed)
        {
            this.m_disposed = true;
        }

        await base.DisposeAsync().ConfigureAwait(false);
    }
#endif

    /// <summary>
    /// 检查对象是否已被释放，如果已释放则抛出异常
    /// </summary>
    /// <exception cref="ObjectDisposedException">对象已被释放</exception>
    private void ThrowIfDisposed()
    {
        if (this.m_disposed)
        {
            throw new ObjectDisposedException(this.GetType().Name);
        }
    }
}
