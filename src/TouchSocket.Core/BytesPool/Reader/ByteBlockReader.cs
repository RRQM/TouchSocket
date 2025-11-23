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

//namespace TouchSocket.Core;

//[DebuggerDisplay("Length={Length},Position={Position}")]
//public sealed class ByteBlockReader : IByteBlockReader
//{
//    private readonly ReadOnlyMemory<byte> m_memory;

//    public ByteBlockReader(ReadOnlyMemory<byte> memory)
//    {
//        this.m_memory = memory;
//    }

//    public long BytesRead { get => this.Position; set => this.Position = (int)value; }
//    public long BytesRemaining => this.CanReadLength;
//    public int CanReadLength => this.Length - this.Position;

//    public int Length => this.m_memory.Length;

//    public ReadOnlyMemory<byte> Memory => this.m_memory;

//    public int Position { get; set; }

//    public ReadOnlySequence<byte> Sequence => this.TotalSequence.Slice(this.Position);
//    public ReadOnlySpan<byte> Span => this.m_memory.Span;
//    public ReadOnlySequence<byte> TotalSequence => new ReadOnlySequence<byte>(this.Memory);

//    public void Advance(int count)
//    {
//        this.Position += count;
//    }

//    public ReadOnlyMemory<byte> GetMemory(int count)
//    {
//        return this.m_memory.Slice(this.Position, count);
//    }

//    public ReadOnlySpan<byte> GetSpan(int count)
//    {
//        return this.Span.Slice(this.Position, count);
//    }

//    public int Read(Span<byte> span)
//    {
//        if (span.IsEmpty)
//        {
//            return 0;
//        }

//        var length = Math.Min(this.CanReadLength, span.Length);

//        this.Span.Slice(this.Position, length).CopyTo(span);
//        this.Position += length;
//        return length;
//    }

//    public void SeekToEnd()
//    {
//        this.Position = this.Length;
//    }

//    public void SeekToStart()
//    {
//        this.Position = 0;
//    }
//}