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
using System.IO;
using System.IO.Compression;

namespace TouchSocket.Core;

public static partial class GZip
{
    public static void Compress(Stream stream, ReadOnlySpan<byte> span)
    {
        using (var gZipStream = new GZipStream(stream, CompressionMode.Compress, true))
        {
            gZipStream.Write(span);
            gZipStream.Close();
        }
    }
    public static void Compress<TWriter>(ref TWriter writer, ReadOnlySpan<byte> span)
        where TWriter : IBytesWriter
    {
        using (var byteBlock = new ByteBlock(span.Length))
        {
            Compress(byteBlock.AsStream(), span);
            writer.Write(byteBlock.Span);
        }
    }

    public static void Compress(ByteBlock byteBlock, ReadOnlySpan<byte> span)
    {
        Compress(byteBlock.AsStream(), span);
    }
    public static ReadOnlyMemory<byte> Compress(ReadOnlySpan<byte> span)
    {
        using (var byteBlock = new ByteBlock(span.Length))
        {
            Compress(byteBlock.AsStream(), span);
            return byteBlock.ToArray();
        }
    }

    public static void Decompress<TWriter>(ref TWriter writer, ReadOnlySpan<byte> span)
        where TWriter : IBytesWriter
    {
        using (var streamByteBlock = new ByteBlock(span.Length))
        {
            streamByteBlock.Write(span);
            streamByteBlock.SeekToStart();

            using (var gZipStream = new GZipStream(streamByteBlock.AsStream(), CompressionMode.Decompress))
            {
                var bytes = ArrayPool<byte>.Shared.Rent(1024 * 64);
                try
                {
                    int r;
                    while ((r = gZipStream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        writer.Write(new System.ReadOnlySpan<byte>(bytes, 0, r));
                    }
                    gZipStream.Close();
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(bytes);
                }
            }
        }
    }

    public static ReadOnlyMemory<byte> Decompress(ReadOnlySpan<byte> span)
    {
        var byteBlock = new ByteBlock(span.Length);
        try
        {
            Decompress(ref byteBlock, span);
            return byteBlock.ToArray();
        }
        finally
        {
            byteBlock.Dispose();
        }
    }
}