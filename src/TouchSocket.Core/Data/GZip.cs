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

using System.Buffers;
using System.IO.Compression;

namespace TouchSocket.Core;

/// <summary>
/// 提供GZip压缩和解压缩功能的静态工具类。
/// </summary>
/// <remarks>
/// 此类封装了基于<see cref="GZipStream"/>的压缩和解压缩操作，
/// 支持对字节数据、流、字节块和字节写入器进行GZip格式的压缩和解压缩处理。
/// </remarks>
public static partial class GZip
{
    /// <summary>
    /// 将字节跨度压缩并写入到指定流中。
    /// </summary>
    /// <param name="stream">要写入压缩数据的流。</param>
    /// <param name="span">要压缩的只读字节跨度。</param>
    /// <remarks>
    /// 此方法使用GZip压缩算法将数据压缩后写入流，压缩完成后会关闭GZip流但不关闭底层流。
    /// </remarks>
    public static void Compress(Stream stream, ReadOnlySpan<byte> span)
    {
        using (var gZipStream = new GZipStream(stream, CompressionMode.Compress, true))
        {
            gZipStream.Write(span);
            gZipStream.Close();
        }
    }

    /// <summary>
    /// 将字节跨度压缩并写入到字节写入器中。
    /// </summary>
    /// <typeparam name="TWriter">实现<see cref="IBytesWriter"/>接口的写入器类型。</typeparam>
    /// <param name="writer">字节写入器实例。</param>
    /// <param name="span">要压缩的只读字节跨度。</param>
    /// <remarks>
    /// 此方法内部使用临时<see cref="ByteBlock"/>进行压缩操作，压缩完成后将结果写入指定的写入器。
    /// </remarks>
    public static void Compress<TWriter>(ref TWriter writer, ReadOnlySpan<byte> span)
        where TWriter : IBytesWriter
    {
        using (var byteBlock = new ByteBlock(span.Length))
        {
            Compress(byteBlock.AsStream(), span);
            writer.Write(byteBlock.Span);
        }
    }

    /// <summary>
    /// 将字节跨度压缩并写入到字节块中。
    /// </summary>
    /// <param name="byteBlock">要写入压缩数据的<see cref="ByteBlock"/>。</param>
    /// <param name="span">要压缩的只读字节跨度。</param>
    /// <remarks>
    /// 此方法将压缩数据直接写入指定的字节块中。
    /// </remarks>
    public static void Compress(ByteBlock byteBlock, ReadOnlySpan<byte> span)
    {
        Compress(byteBlock.AsStream(), span);
    }

    /// <summary>
    /// 压缩字节跨度并返回压缩后的数据。
    /// </summary>
    /// <param name="span">要压缩的只读字节跨度。</param>
    /// <returns>包含压缩数据的<see cref="ReadOnlyMemory{T}"/>。</returns>
    /// <remarks>
    /// 此方法返回压缩后数据的副本，调用者负责管理返回的内存。
    /// </remarks>
    public static ReadOnlyMemory<byte> Compress(ReadOnlySpan<byte> span)
    {
        using (var byteBlock = new ByteBlock(span.Length))
        {
            Compress(byteBlock.AsStream(), span);
            return byteBlock.ToArray();
        }
    }

    /// <summary>
    /// 解压缩字节跨度并将结果写入到字节写入器中。
    /// </summary>
    /// <typeparam name="TWriter">实现<see cref="IBytesWriter"/>接口的写入器类型。</typeparam>
    /// <param name="writer">字节写入器实例。</param>
    /// <param name="span">要解压缩的只读字节跨度。</param>
    /// <remarks>
    /// 此方法使用64KB的缓冲区进行解压缩操作，通过流式读取方式处理大数据。
    /// 内部使用<see cref="ArrayPool{T}.Shared"/>来管理缓冲区内存。
    /// </remarks>
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

    public static void Decompress<TWriter>(ref TWriter writer, ReadOnlyMemory<byte> memory)
        where TWriter : IBytesWriter
    {
        using (var gZipStream = new GZipStream(new ReadOnlyMemoryStream(memory), CompressionMode.Decompress))
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

    /// <summary>
    /// 解压缩字节跨度并返回解压缩后的数据。
    /// </summary>
    /// <param name="span">要解压缩的只读字节跨度。</param>
    /// <returns>包含解压缩数据的<see cref="ReadOnlyMemory{T}"/>。</returns>
    /// <remarks>
    /// 此方法返回解压缩后数据的副本，调用者负责管理返回的内存。
    /// </remarks>
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