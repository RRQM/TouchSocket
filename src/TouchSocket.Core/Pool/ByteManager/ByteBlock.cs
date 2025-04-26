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
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Buffers;

namespace TouchSocket.Core;

/// <summary>
/// 字节块流
/// </summary>
[DebuggerDisplay("Length={Length},Position={Position},Capacity={Capacity}")]
public sealed partial class ByteBlock : DisposableObject, IByteBlock
{
    private byte[] m_buffer;
    private ArrayPool<byte> m_bytePool;
    private int m_dis;
    private bool m_holding;
    private int m_length;
    private int m_position;

    #region 构造函数

    /// <summary>
    /// 使用 ValueByteBlock 初始化 ByteBlock 对象。
    /// </summary>
    /// <param name="valueByteBlock">包含字节数据的 ValueByteBlock 对象。</param>
    public ByteBlock(in ValueByteBlock valueByteBlock)
    {
        // 直接使用 ValueByteBlock 中的内存数组和字节池。
        this.m_buffer = valueByteBlock.TotalMemory.GetArray().Array;
        this.m_bytePool = valueByteBlock.BytePool;
        // 初始化位置和长度信息。
        this.m_position = valueByteBlock.Position;
        this.m_length = valueByteBlock.Length;
        // 设置一个固定的游标距离。
        this.m_dis = 1;
    }

    /// <summary>
    /// 无参数构造函数，初始化一个具有默认大小的 ByteBlock 对象。
    /// </summary>
    /// <param name="byteSize">ByteBlock 的初始大小。</param>
    public ByteBlock(int byteSize)
    {
        // 使用默认字节池初始化。
        this.m_bytePool = ArrayPool<byte>.Shared;
        // 从字节池租用指定大小的字节数组。
        this.m_buffer = this.m_bytePool.Rent(byteSize);
    }

    /// <summary>
    /// 使用指定的字节池和大小初始化 ByteBlock 对象。
    /// </summary>
    /// <param name="byteSize">ByteBlock 的初始大小。</param>
    /// <param name="bytePool">用于 ByteBlock 的 BytePool 实例。</param>
    public ByteBlock(int byteSize, ArrayPool<byte> bytePool)
    {
        // 确保字节池不为空。
        this.m_bytePool = ThrowHelper.ThrowArgumentNullExceptionIf(bytePool, nameof(bytePool));
        // 从指定的字节池租用字节数组。
        this.m_buffer = bytePool.Rent(byteSize);
    }
    /// <summary>
    /// 实例化一个已知内存的对象。且该内存不会被回收。
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="length"></param>
    public ByteBlock(byte[] bytes, int length)
    {
        this.m_buffer = bytes ?? throw new ArgumentNullException(nameof(bytes));
        this.m_length = length;
    }

    /// <summary>
    /// 实例化一个已知内存的对象。且该内存不会被回收。
    /// </summary>
    /// <param name="bytes"></param>
    public ByteBlock(byte[] bytes) : this(bytes, bytes.Length)
    {
    }

    #endregion 构造函数

    #region 属性

    /// <inheritdoc/>
    public ReadOnlyMemory<byte> Memory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            this.ThrowIfDisposed();
            return new ReadOnlyMemory<byte>(this.m_buffer, 0, this.m_length);
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
    public ReadOnlySpan<byte> Span
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            this.ThrowIfDisposed();
            return new ReadOnlySpan<byte>(this.m_buffer, 0, this.m_length);
        }
    }

    /// <inheritdoc/>
    public bool CanRead => this.Using && this.CanReadLength > 0;

    /// <inheritdoc/>
    public int CanReadLength => this.m_length - this.m_position;

    /// <inheritdoc/>
    public int Capacity => this.m_buffer.Length;

    /// <inheritdoc/>
    public int FreeLength => this.Capacity - this.m_position;

    /// <inheritdoc/>
    public bool Holding => this.m_holding;

    /// <inheritdoc/>
    public int Length => this.m_length;

    /// <inheritdoc/>
    public int Position
    {
        get => this.m_position;
        set => this.m_position = value;
    }

    /// <inheritdoc/>
    public bool Using => this.m_dis == 0;

    /// <inheritdoc/>
    public bool IsStruct => false;

    /// <inheritdoc/>
    public ArrayPool<byte> BytePool => this.m_bytePool;

    /// <inheritdoc/>
    public byte this[int index]
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
    public void Clear()
    {
        this.ThrowIfDisposed();
        Array.Clear(this.m_buffer, 0, this.m_buffer.Length);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (this.DisposedValue)
        {
            return;
        }

        if (disposing)
        {
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
        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    public void Reset()
    {
        this.ThrowIfDisposed();
        this.m_position = 0;
        this.m_length = 0;
        this.m_holding = false;
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
        if (holding)
        {
            this.m_holding = holding;
        }
        else
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
    #endregion
}