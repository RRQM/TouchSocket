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
/// 简化版文件存储器
/// </summary>
public sealed partial class FileStorage : IDisposable
{
    private readonly FileStream m_fileStream;
    private readonly SemaphoreSlim m_semaphore = new SemaphoreSlim(1, 1);
    internal int m_referenceCount;
    private bool m_disposed;

    internal FileStorage(string path)
    {
        this.Path = path;
        this.m_fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
    }

    /// <summary>
    /// 文件路径
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// 文件长度
    /// </summary>
    public long Length
    {
        get
        {
            this.m_semaphore.Wait();
            try
            {
                this.ThrowIfDisposed();
                return this.m_fileStream.Length;
            }
            finally
            {
                this.m_semaphore.Release();
            }
        }
    }

    public bool CanRead => this.m_fileStream.CanRead;
    public bool CanSeek => this.m_fileStream.CanSeek;
    public bool CanWrite => this.m_fileStream.CanWrite;

    /// <summary>
    /// 设置文件长度
    /// </summary>
    /// <param name="length">新长度</param>
    public void SetLength(long length)
    {
        this.m_semaphore.Wait();
        try
        {
            this.ThrowIfDisposed();
            this.m_fileStream.SetLength(length);
        }
        finally
        {
            this.m_semaphore.Release();
        }
    }

    /// <summary>
    /// 刷新缓冲区
    /// </summary>
    public void Flush()
    {
        this.m_semaphore.Wait();
        try
        {
            this.ThrowIfDisposed();
            this.m_fileStream.Flush();
        }
        finally
        {
            this.m_semaphore.Release();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // 通过文件池释放，确保引用计数正确
        FilePool.ReleaseFile(this);
    }

    internal void DisposeInternal()
    {
        if (this.m_disposed)
        {
            return;
        }

        // 等待获得独占访问，确保没有读写在进行
        this.m_semaphore.Wait();
        try
        {
            if (this.m_disposed)
            {
                return;
            }

            this.m_disposed = true;
            this.m_fileStream.Dispose();
        }
        finally
        {
            // 释放并处置信号量
            try
            {
                this.m_semaphore.Release();
            }
            catch
            {
                // 忽略释放异常
            }

            this.m_semaphore.Dispose();
        }
    }

    private void ThrowIfDisposed()
    {
        if (this.m_disposed)
        {
            ThrowHelper.ThrowObjectDisposedException(this);
        }
    }

    /// <summary>
    /// 读取数据
    /// </summary>
    /// <param name="position">读取位置</param>
    /// <param name="buffer">缓冲区</param>
    /// <param name="offset">缓冲区偏移量</param>
    /// <param name="count">读取字节数</param>
    /// <returns>实际读取的字节数</returns>
    public int Read(long position, byte[] buffer, int offset, int count)
    {
        this.m_semaphore.Wait();
        try
        {
            this.ThrowIfDisposed();
            this.m_fileStream.Position = position;
            return this.m_fileStream.Read(buffer, offset, count);
        }
        finally
        {
            this.m_semaphore.Release();
        }
    }

    /// <summary>
    /// 写入数据
    /// </summary>
    /// <param name="position">写入位置</param>
    /// <param name="buffer">数据</param>
    /// <param name="offset">缓冲区偏移量</param>
    /// <param name="count">写入字节数</param>
    public void Write(long position, byte[] buffer, int offset, int count)
    {
        this.m_semaphore.Wait();
        try
        {
            this.ThrowIfDisposed();
            this.m_fileStream.Position = position;
            this.m_fileStream.Write(buffer, offset, count);
        }
        finally
        {
            this.m_semaphore.Release();
        }
    }

    /// <summary>
    /// 异步读取数据
    /// </summary>
    /// <param name="position">读取位置</param>
    /// <param name="memory">缓冲区</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>实际读取的字节数</returns>
    public async Task<int> ReadAsync(long position, Memory<byte> memory, CancellationToken cancellationToken = default)
    {
        await this.m_semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            this.ThrowIfDisposed();
            this.m_fileStream.Position = position;
            return await this.m_fileStream.ReadAsync(memory, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            this.m_semaphore.Release();
        }
    }

    /// <summary>
    /// 异步写入数据
    /// </summary>
    /// <param name="position">写入位置</param>
    /// <param name="memory">数据</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task WriteAsync(long position, ReadOnlyMemory<byte> memory, CancellationToken cancellationToken = default)
    {
        await this.m_semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            this.ThrowIfDisposed();
            this.m_fileStream.Position = position;
            await this.m_fileStream.WriteAsync(memory, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            this.m_semaphore.Release();
        }
    }

    /// <summary>
    /// 异步刷新缓冲区
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        await this.m_semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            this.ThrowIfDisposed();
            await this.m_fileStream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            this.m_semaphore.Release();
        }
    }


    /// <summary>
    /// 读取数据
    /// </summary>
    /// <param name="position">读取位置</param>
    /// <param name="buffer">缓冲区</param>
    /// <returns>实际读取的字节数</returns>
    public int Read(long position, Span<byte> buffer)
    {
        this.m_semaphore.Wait();
        try
        {
            this.ThrowIfDisposed();
            this.m_fileStream.Position = position;
            return this.m_fileStream.Read(buffer);
        }
        finally
        {
            this.m_semaphore.Release();
        }
    }

    /// <summary>
    /// 写入数据
    /// </summary>
    /// <param name="position">写入位置</param>
    /// <param name="buffer">数据</param>
    public void Write(long position, ReadOnlySpan<byte> buffer)
    {
        this.m_semaphore.Wait();
        try
        {
            this.ThrowIfDisposed();
            this.m_fileStream.Position = position;
            this.m_fileStream.Write(buffer);
        }
        finally
        {
            this.m_semaphore.Release();
        }
    }
}