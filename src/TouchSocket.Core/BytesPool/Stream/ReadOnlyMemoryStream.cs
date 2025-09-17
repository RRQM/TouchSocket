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

/// <summary>
/// 表示一个基于只读内存的流，提供对<see cref="ReadOnlyMemory{T}"/>的流式访问。
/// 继承自<see cref="Stream"/>类，仅支持读取和定位操作。
/// </summary>
/// <remarks>
/// ReadOnlyMemoryStream为只读流实现，不支持写入操作。
/// 提供了对内存数据的高性能流式访问，适用于需要将内存数据作为流处理的场景。
/// 支持随机访问和定位操作。
/// </remarks>
public sealed class ReadOnlyMemoryStream : Stream
{
    private readonly ReadOnlyMemory<byte> m_memory;
    private int m_position;

    /// <summary>
    /// 使用指定的只读内存初始化<see cref="ReadOnlyMemoryStream"/>的新实例。
    /// </summary>
    /// <param name="memory">作为流数据源的只读内存。</param>
    public ReadOnlyMemoryStream(ReadOnlyMemory<byte> memory)
    {
        this.m_memory = memory;
        this.m_position = 0;
    }

    /// <summary>
    /// 获取一个值，该值指示当前流是否支持读取。
    /// </summary>
    /// <value>始终返回<see langword="true"/>，因为此流支持读取。</value>
    public override bool CanRead => true;

    /// <summary>
    /// 获取一个值，该值指示当前流是否支持查找。
    /// </summary>
    /// <value>始终返回<see langword="true"/>，因为此流支持查找。</value>
    public override bool CanSeek => true;

    /// <summary>
    /// 获取一个值，该值指示当前流是否支持写入。
    /// </summary>
    /// <value>始终返回<see langword="false"/>，因为此流为只读流。</value>
    public override bool CanWrite => false;

    /// <summary>
    /// 获取流的长度（以字节为单位）。
    /// </summary>
    /// <value>返回底层内存的长度。</value>
    public override long Length => this.m_memory.Length;

    /// <summary>
    /// 获取或设置流内的当前位置。
    /// </summary>
    /// <value>流内的当前位置。</value>
    /// <exception cref="ArgumentOutOfRangeException">当设置的值小于0或大于流长度时抛出。</exception>
    public override long Position
    {
        get => this.m_position;
        set
        {
            if (value < 0 || value > this.m_memory.Length)
                throw new ArgumentOutOfRangeException(nameof(value));
            this.m_position = (int)value;
        }
    }

    /// <summary>
    /// 清除此流的缓冲区，并使得任何缓冲数据都被写入到基础设备。
    /// </summary>
    /// <remarks>
    /// 对于只读流，此操作为无操作（no-op）。
    /// </remarks>
    public override void Flush()
    {
        // No-op for read-only stream
    }

    /// <summary>
    /// 从当前流读取字节序列，并将流内的位置提升读取的字节数。
    /// </summary>
    /// <param name="buffer">字节数组。当此方法返回时，该缓冲区包含从当前源读取的字节。</param>
    /// <param name="offset"><paramref name="buffer"/>中的从零开始的字节偏移量，从此处开始存储从当前流中读取的数据。</param>
    /// <param name="count">要从当前流中读取的最大字节数。</param>
    /// <returns>读入缓冲区中的总字节数。如果当前可用的字节数少于所请求的字节数，则总字节数可能会少于所请求的字节数；如果已到达流的末尾，则为零。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="buffer"/>为<see langword="null"/>。</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/>或<paramref name="count"/>为负数。</exception>
    /// <exception cref="ArgumentException"><paramref name="offset"/>和<paramref name="count"/>的组合无效。</exception>
    public override int Read(byte[] buffer, int offset, int count)
    {
        if (buffer == null)
        {
            throw new ArgumentNullException(nameof(buffer));
        }

        if (offset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }

        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        if (offset + count > buffer.Length)
        {
            throw new ArgumentException("Invalid offset and count combination");
        }

        var remainingBytes = this.m_memory.Length - this.m_position;
        var bytesToRead = Math.Min(count, remainingBytes);

        if (bytesToRead <= 0)
        {
            return 0;
        }

        var span = this.m_memory.Span.Slice(this.m_position, bytesToRead);
        span.CopyTo(buffer.AsSpan(offset, bytesToRead));
        this.m_position += bytesToRead;

        return bytesToRead;
    }

    /// <summary>
    /// 设置当前流内的位置。
    /// </summary>
    /// <param name="offset">相对于<paramref name="origin"/>参数的字节偏移量。</param>
    /// <param name="origin"><see cref="SeekOrigin"/>类型的值，指示用于获取新位置的参考点。</param>
    /// <returns>当前流中的新位置。</returns>
    /// <exception cref="ArgumentException">当<paramref name="origin"/>为无效值时抛出。</exception>
    /// <exception cref="ArgumentOutOfRangeException">当计算出的新位置超出流范围时抛出。</exception>
    public override long Seek(long offset, SeekOrigin origin)
    {
        var newPosition = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => this.m_position + offset,
            SeekOrigin.End => this.m_memory.Length + offset,
            _ => throw new ArgumentException("Invalid seek origin", nameof(origin))
        };

        if (newPosition < 0 || newPosition > this.m_memory.Length)
            throw new ArgumentOutOfRangeException(nameof(offset));

        this.m_position = (int)newPosition;
        return this.m_position;
    }

    /// <summary>
    /// 设置当前流的长度。
    /// </summary>
    /// <param name="value">所需的当前流的长度（以字节表示）。</param>
    /// <exception cref="NotSupportedException">只读流不支持设置长度操作。</exception>
    /// <remarks>
    /// 只读流不支持修改长度，调用此方法将抛出<see cref="NotSupportedException"/>。
    /// </remarks>
    public override void SetLength(long value)
    {
        throw new NotSupportedException("Cannot set length on a read-only stream");
    }

    /// <summary>
    /// 将字节序列写入当前流，并将此流中的当前位置提升写入的字节数。
    /// </summary>
    /// <param name="buffer">包含要写入当前流的数据的字节数组。</param>
    /// <param name="offset"><paramref name="buffer"/>中的从零开始的字节偏移量，从此处开始将字节复制到当前流。</param>
    /// <param name="count">要写入当前流的字节数。</param>
    /// <exception cref="NotSupportedException">只读流不支持写入操作。</exception>
    /// <remarks>
    /// 只读流不支持写入操作，调用此方法将抛出<see cref="NotSupportedException"/>。
    /// </remarks>
    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException("Cannot write to a read-only stream");
    }
}