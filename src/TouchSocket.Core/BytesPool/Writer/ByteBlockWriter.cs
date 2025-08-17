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

//using System;
//using System.Buffers;
//using System.Diagnostics;
//using System.Runtime.CompilerServices;

//namespace TouchSocket.Core;

//[DebuggerDisplay("Length={Length},Position={Position},Capacity={Capacity}")]
//public sealed class ByteBlockWriter : SafetyDisposableObject, IByteBlockWriter
//{
//    private readonly IMemoryOwner<byte> m_memoryOwner;
//    private int m_length;
//    private Memory<byte> m_memory;

//    public ByteBlockWriter(Memory<byte> memory)
//    {
//        this.m_memory = memory;
//    }

//    public ByteBlockWriter(int capacity)
//    {
//        capacity = Math.Max(capacity, 1024);
//        this.m_memoryOwner = MemoryPool<byte>.Shared.Rent(capacity);
//        this.m_memory = this.m_memoryOwner.Memory;
//    }

//    public bool CanSeek => true;
//    public int Capacity => this.m_memory.Length;
//    public int FreeLength => this.Capacity - this.Position;
//    public bool IsEmpty => this.m_memory.IsEmpty;
//    public bool IsStruct => true;
//    public int Length => this.m_length;
//    public ReadOnlyMemory<byte> Memory => this.m_memory.Slice(0, this.m_length);
//    public int Position { get; set; }
//    public ReadOnlySpan<byte> Span => this.Memory.Span;
//    public bool SupportsRewind => true;
//    public Memory<byte> TotalMemory => this.m_memory;
//    public short Version => 0;
//    public long WrittenCount => Math.Max(this.m_length, this.Position);

//    public void Advance(int count)
//    {
//        this.Position += count;
//        this.m_length = this.Position > this.m_length ? this.Position : this.m_length;
//    }

//    [MethodImpl(MethodImplOptions.AggressiveInlining)]
//    public void ExtendSize(int size)
//    {
//        if (this.FreeLength < size)
//        {
//            ThrowHelper.ThrowNotSupportedException("不支持扩容");
//        }
//    }

//    public Memory<byte> GetMemory(int sizeHint = 0)
//    {
//        this.ExtendSize(sizeHint);
//        return this.m_memory.Slice(this.Position, this.FreeLength);
//    }

//    public Span<byte> GetSpan(int sizeHint = 0)
//    {
//        return this.GetMemory(sizeHint).Span;
//    }

//    public void SeekToEnd()
//    {
//        this.Position = this.m_length;
//    }

//    public void SeekToStart()
//    {
//        this.Position = 0;
//    }

//    public void SetLength(int value)
//    {
//        if (value > this.m_memory.Length)
//        {
//            ThrowHelper.ThrowException("设置的长度超过了内存的长度。");
//        }
//        this.m_length = value;
//    }

//    public void Write(scoped ReadOnlySpan<byte> span)
//    {
//        if (span.IsEmpty)
//        {
//            return;
//        }

//        this.ExtendSize(span.Length);

//        var currentSpan = this.GetCurrentSpan();
//        span.CopyTo(currentSpan);
//        this.Position += span.Length;
//        this.m_length = Math.Max(this.Position, this.m_length);
//    }

//    protected override void SafetyDispose(bool disposing)
//    {
//        if (disposing)
//        {
//            this.m_memory = default;
//            this.m_length = 0;
//            this.Position = 0;
//            this.m_memoryOwner?.Dispose();
//        }
//    }

//    private Span<byte> GetCurrentSpan()
//    {
//        return this.m_memory.Span.Slice(this.Position);
//    }
//}