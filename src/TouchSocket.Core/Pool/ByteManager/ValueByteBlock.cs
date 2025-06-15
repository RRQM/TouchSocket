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

using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace TouchSocket.Core;

/// <summary>
/// 字节块流
/// </summary>
[DebuggerDisplay("Length={Length},Position={Position},Capacity={Capacity}")]
public partial struct ValueByteBlock : IByteBlock, IEquatable<ValueByteBlock>
{
    private static ValueByteBlock s_empty = new ValueByteBlock();
    private byte[] m_buffer;
    private ArrayPool<byte> m_bytePool;
    private int m_dis;
    private bool m_holding;
    private int m_length;
    private int m_position;

    #region 构造函数


    /// <summary>
    /// 初始化 ValueByteBlock 类的新实例，从默认字节池租用指定大小的字节。
    /// </summary>
    /// <param name="byteSize">要从字节池租用的字节数。</param>
    public ValueByteBlock(int byteSize)
    {
        if (byteSize < 1)
        {
            byteSize = 16;
        }
        this.m_bytePool = ArrayPool<byte>.Shared;
        this.m_buffer = this.m_bytePool.Rent(byteSize);
    }

    /// <summary>
    /// 初始化 ValueByteBlock 类的新实例，从指定的字节池租用指定大小的字节。
    /// </summary>
    /// <param name="byteSize">要从字节池租用的字节数。</param>
    /// <param name="bytePool">用于租用字节的 BytePool 实例。</param>
    public ValueByteBlock(int byteSize, ArrayPool<byte> bytePool)
    {
        if (byteSize < 1)
        {
            byteSize = 16;
        }
        this.m_bytePool = bytePool;
        this.m_buffer = bytePool.Rent(byteSize);
    }

    /// <summary>
    /// 初始化 ValueByteBlock 类的新实例，使用现有的字节数组。
    /// </summary>
    /// <param name="bytes">包含字节的数组。</param>
    /// <param name="length">数组中要使用的字节数。</param>
    /// <exception cref="ArgumentNullException">如果 bytes 参数为 null，则引发此异常。</exception>
    public ValueByteBlock(byte[] bytes, int length)
    {
        this.m_buffer = bytes ?? throw new ArgumentNullException(nameof(bytes));
        this.m_length = length;
    }

    /// <summary>
    /// 初始化 ValueByteBlock 类的新实例，使用现有的字节数组，并使用整个数组的长度。
    /// </summary>
    /// <param name="bytes">要使用的字节数组。</param>
    /// <exception cref="ArgumentNullException">如果 bytes 参数为 null，则引发此异常。</exception>
    public ValueByteBlock(byte[] bytes) : this(bytes, bytes.Length)
    {
    }

    #endregion 构造函数

    #region 属性

    /// <inheritdoc/>
    public static ValueByteBlock Empty => s_empty;

    /// <inheritdoc/>
    public readonly ArrayPool<byte> BytePool => this.m_bytePool;

    /// <inheritdoc/>
    public bool CanRead => this.Using && this.CanReadLength > 0;

    /// <inheritdoc/>
    public readonly int CanReadLength => this.m_length - this.m_position;

    /// <inheritdoc/>
    public readonly int Capacity => this.m_buffer.Length;

    /// <inheritdoc/>
    public readonly int FreeLength => this.Capacity - this.m_position;

    /// <inheritdoc/>
    public readonly bool Holding => this.m_holding;

    /// <inheritdoc/>
    public readonly bool IsEmpty => this.m_buffer == null;

    /// <inheritdoc/>
    public readonly bool IsStruct => true;

    /// <inheritdoc/>
    public readonly int Length => this.m_length;

    /// <inheritdoc/>
    public ReadOnlyMemory<byte> Memory
    {
        get
        {
            this.ThrowIfDisposed();
            return new ReadOnlyMemory<byte>(this.m_buffer, 0, this.m_length);
        }
    }

    /// <inheritdoc/>
    public int Position
    {
        get => this.m_position;
        set => this.m_position = value;
    }

    /// <inheritdoc/>
    public ReadOnlySpan<byte> Span
    {
        get
        {
            this.ThrowIfDisposed();
            return new ReadOnlySpan<byte>(this.m_buffer, 0, this.m_length);
        }
    }

    /// <inheritdoc/>
    public Memory<byte> TotalMemory
    {
        get
        {
            this.ThrowIfDisposed();
            return new Memory<byte>(this.m_buffer);
        }
    }

    /// <inheritdoc/>
    public readonly bool Using => this.m_dis == 0;

    /// <inheritdoc/>
    public readonly byte this[int index]
    {
        get
        {
            this.ThrowIfDisposed();
            return this.m_buffer[index];
        }
        set
        {
            this.ThrowIfDisposed();
            this.m_buffer[index] = value;
        }
    }

    #endregion 属性

    /// <inheritdoc/>
    public readonly void Clear()
    {
        this.ThrowIfDisposed();
        Array.Clear(this.m_buffer, 0, this.m_buffer.Length);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        var dis = this.m_dis;
        if (dis != 0)
        {
            return;
        }

        if (this.m_holding)
        {
            return;
        }

        if (Interlocked.Increment(ref this.m_dis) == 1)
        {
            this.m_bytePool?.Return(this.m_buffer);
            this.m_holding = false;
            this.m_position = 0;
            this.m_length = 0;
            this.m_buffer = null;
            this.m_bytePool = null;
        }
    }

    /// <inheritdoc/>
    public readonly bool Equals(ValueByteBlock other)
    {
        return this.m_buffer == other.m_buffer;
    }

    /// <inheritdoc/>
    public void Reset()
    {
        this.ThrowIfDisposed();
        this.m_position = 0;
        this.m_length = 0;
    }

    /// <inheritdoc/>
    public void SetCapacity(int capacity, bool retainedData = false)
    {
        this.ThrowIfDisposed();

        if (this.Capacity == capacity)
        {
            return;
        }

        byte[] bytes;
        bool canReturn;
        if (this.m_bytePool == null)
        {
            this.m_bytePool = ArrayPool<byte>.Shared;
            canReturn = false;
        }
        else
        {
            canReturn = true;
        }

        bytes = this.m_bytePool.Rent(capacity);

        if (retainedData)
        {
            Array.Copy(this.m_buffer, 0, bytes, 0, this.m_buffer.Length);
        }

        if (canReturn)
        {
            this.m_bytePool.Return(this.m_buffer);
        }
        this.m_buffer = bytes;
    }

    /// <inheritdoc/>
    public void SetHolding(bool holding)
    {
        this.ThrowIfDisposed();
        this.m_holding = holding;
        if (!holding)
        {
            this.Dispose();
        }
    }

    /// <inheritdoc/>
    public void SetLength(int value)
    {
        this.ThrowIfDisposed();
        if (value > this.m_buffer.Length)
        {
            throw new Exception("设置值超出容量");
        }
        this.m_length = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly bool ThrowIfDisposed()
    {
        return this.m_dis != 0;
    }

    #region Seek

    /// <inheritdoc/>
    public int Seek(int offset, SeekOrigin origin)
    {
        this.ThrowIfDisposed();
        switch (origin)
        {
            case SeekOrigin.Begin:
                this.m_position = offset;
                break;

            case SeekOrigin.Current:
                this.m_position += offset;
                break;

            case SeekOrigin.End:
                this.m_position = this.m_length + offset;
                break;
        }
        return this.m_position;
    }

    /// <inheritdoc/>
    public void Seek(int position)
    {
        this.m_position = position;
    }

    /// <inheritdoc/>
    public void SeekToEnd()
    {
        this.m_position = this.m_length;
    }

    /// <inheritdoc/>
    public void SeekToStart()
    {
        this.m_position = 0;
    }

    #endregion Seek

    #region ToString

    /// <inheritdoc/>
    public override string ToString()
    {
        return this.ToString(0, this.Length);
    }

    /// <inheritdoc/>
    public string ToString(int offset, int length)
    {
        this.ThrowIfDisposed();

        return Encoding.UTF8.GetString(this.m_buffer, offset, length);
    }

    /// <inheritdoc/>
    public string ToString(int offset)
    {
        this.ThrowIfDisposed();

        return Encoding.UTF8.GetString(this.m_buffer, offset, this.Length - offset);
    }

    #endregion ToString

    #region BufferWriter

    /// <inheritdoc/>
    public void Advance(int count)
    {
        this.m_position += count;
        this.m_length = this.m_position > this.m_length ? this.m_position : this.m_length;
    }

    /// <inheritdoc/>
    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        this.ExtendSize(sizeHint);
        return new Memory<byte>(this.m_buffer, this.m_position, this.m_buffer.Length - this.m_position);
    }

    /// <inheritdoc/>
    public Span<byte> GetSpan(int sizeHint = 0)
    {
        this.ExtendSize(sizeHint);
        return new Span<byte>(this.m_buffer, this.m_position, this.m_buffer.Length - this.m_position);
    }

    #endregion BufferWriter
}