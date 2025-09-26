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

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace TouchSocket.Core;

/// <summary>
/// 字节块流
/// </summary>
[DebuggerDisplay("Len={Length},Pos={Position},Capacity={Capacity}")]
internal sealed partial class ByteBlockStream : Stream
{
    private readonly ByteBlock m_byteBlock;
    private readonly bool m_releaseTogether;

    /// <summary>
    /// 初始化 ByteBlockStream 类的新实例。
    /// </summary>
    /// <param name="byteBlock">一个 ByteBlock 对象，表示字节块。</param>
    /// <param name="releaseTogether">一个布尔值，指示是否在释放流时同时释放字节块。</param>
    public ByteBlockStream(ByteBlock byteBlock, bool releaseTogether)
    {
        this.m_byteBlock = byteBlock;
        this.m_releaseTogether = releaseTogether;
    }

    /// <summary>
    /// 获取此实例关联的 ByteBlock 对象。
    /// </summary>
    public ByteBlock ByteBlock => this.m_byteBlock;

    /// <summary>
    /// 仅当内存块可用，且<see cref="CanReadLength"/>>0时为<see langword="true"/>。
    /// </summary>
    public override bool CanRead => this.m_byteBlock.Using && this.CanReadLength > 0;

    /// <summary>
    /// 还能读取的长度，计算为<see cref="Length"/>与<see cref="Position"/>的差值。
    /// </summary>
    public long CanReadLength => this.m_byteBlock.Length - this.m_byteBlock.Position;

    /// <summary>
    /// 支持查找
    /// </summary>
    public override bool CanSeek => this.m_byteBlock.Using;

    /// <summary>
    /// 可写入
    /// </summary>
    public override bool CanWrite => this.m_byteBlock.Using;

    /// <summary>
    /// 容量
    /// </summary>
    public int Capacity => this.m_byteBlock.Capacity;

    /// <summary>
    /// 空闲长度，准确掌握该值，可以避免内存扩展，计算为<see cref="Capacity"/>与<see cref="Position"/>的差值。
    /// </summary>
    public long FreeLength => this.Capacity - this.Position;

    /// <summary>
    /// 真实长度
    /// </summary>
    public override long Length => this.m_byteBlock.Length;

    /// <summary>
    /// 流位置
    /// </summary>
    public override long Position
    {
        get => this.m_byteBlock.Position;
        set => this.m_byteBlock.Position = (int)value;
    }

    /// <summary>
    /// 无实际效果
    /// </summary>
    public override void Flush()
    {
    }

    /// <summary>
    /// 异步刷新（无实际效果）
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>已完成的任务</returns>
    public override Task FlushAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return CreateCanceledTask(cancellationToken);
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// 读取数据，然后递增Pos
    /// </summary>
    /// <param name="buffer">目标缓冲区</param>
    /// <param name="offset">偏移量</param>
    /// <param name="length">读取长度</param>
    /// <returns>实际读取的字节数</returns>
    /// <exception cref="ObjectDisposedException">对象已释放</exception>
    public override int Read(byte[] buffer, int offset, int length)
    {
        this.ThrowIfDisposed();
        return this.m_byteBlock.Read(new Span<byte>(buffer, offset, length));
    }

    /// <summary>
    /// 异步读取数据
    /// </summary>
    /// <param name="buffer">目标缓冲区</param>
    /// <param name="offset">偏移量</param>
    /// <param name="count">读取字节数</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>实际读取的字节数</returns>
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return CreateCanceledTask<int>(cancellationToken);
        }

        this.ThrowIfDisposed();

        try
        {
            var result = this.Read(buffer, offset, count);
            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            return Task.FromException<int>(ex);
        }
    }

    /// <summary>
    /// 从当前流位置读取一个<see cref="byte"/>值
    /// </summary>
    /// <returns>读取的字节值，如果到达流末尾则返回-1</returns>
    public override int ReadByte()
    {
        this.ThrowIfDisposed();

        if (this.CanReadLength <= 0)
        {
            return -1;
        }

        var value = this.m_byteBlock.GetSpan(1)[0];
        this.m_byteBlock.Advance(1);
        return value;
    }

    /// <summary>
    /// 设置流位置
    /// </summary>
    /// <param name="offset">偏移量</param>
    /// <param name="origin">起始位置</param>
    /// <returns>新的流位置</returns>
    /// <exception cref="ObjectDisposedException">对象已释放</exception>
    public override long Seek(long offset, SeekOrigin origin)
    {
        this.ThrowIfDisposed();
        switch (origin)
        {
            case SeekOrigin.Begin:
                this.m_byteBlock.Position = (int)offset;
                break;

            case SeekOrigin.Current:
                this.m_byteBlock.Position += (int)offset;
                break;

            case SeekOrigin.End:
                this.m_byteBlock.Position = (int)(this.Length + offset);
                break;
        }

        return this.m_byteBlock.Position;
    }

    /// <summary>
    /// 设置实际长度
    /// </summary>
    /// <param name="value">新长度</param>
    /// <exception cref="ObjectDisposedException">对象已释放</exception>
    public override void SetLength(long value)
    {
        this.ThrowIfDisposed();
        this.m_byteBlock.SetLength((int)value);
    }

    /// <summary>
    /// 写入数据
    /// </summary>
    /// <param name="buffer">源数据缓冲区</param>
    /// <param name="offset">偏移量</param>
    /// <param name="count">写入字节数</param>
    /// <exception cref="ObjectDisposedException">对象已释放</exception>
    public override void Write(byte[] buffer, int offset, int count)
    {
        this.ThrowIfDisposed();
        this.m_byteBlock.Write(new ReadOnlySpan<byte>(buffer, offset, count));
    }

    /// <summary>
    /// 异步写入数据
    /// </summary>
    /// <param name="buffer">源数据缓冲区</param>
    /// <param name="offset">偏移量</param>
    /// <param name="count">写入字节数</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>表示异步写入操作的任务</returns>
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return CreateCanceledTask(cancellationToken);
        }

        this.ThrowIfDisposed();

        try
        {
            this.Write(buffer, offset, count);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(ex);
        }
    }

    /// <summary>
    /// 写入单个字节
    /// </summary>
    /// <param name="value">要写入的字节值</param>
    public override void WriteByte(byte value)
    {
        this.ThrowIfDisposed();

        var span = this.m_byteBlock.GetSpan(1);
        span[0] = value;
        this.m_byteBlock.Advance(1);
    }

    /// <summary>
    /// 高效复制到目标流
    /// </summary>
    /// <param name="destination">目标流</param>
    public new void CopyTo(Stream destination)
    {
        this.CopyTo(destination, 81920);
    }

    /// <summary>
    /// 高效复制到目标流
    /// </summary>
    /// <param name="destination">目标流</param>
    /// <param name="bufferSize">缓冲区大小（此参数被忽略，因为我们直接使用内存块）</param>
    public new void CopyTo(Stream destination, int bufferSize)
    {
        this.ThrowIfDisposed();
        if (destination == null) throw new ArgumentNullException(nameof(destination));

        if (!destination.CanWrite)
        {
            throw new NotSupportedException("目标流不支持写入");
        }

        // 直接使用内存块复制，避免额外的缓冲区分配
        var remainingLength = (int)this.CanReadLength;
        if (remainingLength > 0)
        {
            var remainingData = this.m_byteBlock.Memory.Slice(this.m_byteBlock.Position, remainingLength);
            WritePlatformOptimized(destination, remainingData);
            this.m_byteBlock.Position = this.m_byteBlock.Length;
        }
    }

    /// <summary>
    /// 异步复制到目标流
    /// </summary>
    /// <param name="destination">目标流</param>
    /// <param name="bufferSize">缓冲区大小（此参数被忽略，因为我们直接使用内存块）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>表示异步复制操作的任务</returns>
    public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
        this.ThrowIfDisposed();
        if (destination == null) throw new ArgumentNullException(nameof(destination));

        if (cancellationToken.IsCancellationRequested)
        {
            await CreateCanceledTask(cancellationToken).ConfigureAwait(false);
        }

        if (!destination.CanWrite)
        {
            throw new NotSupportedException("目标流不支持写入");
        }

        // 直接使用内存块异步复制
        var remainingLength = (int)this.CanReadLength;
        if (remainingLength > 0)
        {
            var remainingData = this.m_byteBlock.Memory.Slice(this.m_byteBlock.Position, remainingLength);
            await WriteAsyncPlatformOptimized(destination, remainingData, cancellationToken).ConfigureAwait(false);
            this.m_byteBlock.Position = this.m_byteBlock.Length;
        }
    }

    /// <inheritdoc/>
    /// <param name="disposing">是否正在处置托管资源</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && this.m_releaseTogether)
        {
            this.m_byteBlock.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// 检查对象是否已释放，如果已释放则抛出异常
    /// </summary>
    /// <exception cref="ObjectDisposedException">对象已释放</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfDisposed()
    {
        if (!this.m_byteBlock.Using)
        {
            throw new ObjectDisposedException(this.GetType().FullName);
        }
    }
}