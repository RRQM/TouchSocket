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
/// 表示一个GZip数据压缩器，提供基于GZip算法的数据压缩和解压缩功能。
/// 实现了<see cref="IDataCompressor"/>接口。
/// </summary>
/// <remarks>
/// GZipDataCompressor使用GZip压缩算法对数据进行压缩和解压缩操作。
/// GZip是一种广泛使用的无损数据压缩算法，具有良好的压缩率和兼容性。
/// 适用于需要减少数据传输量或存储空间的场景。
/// </remarks>
public sealed partial class GZipDataCompressor : IDataCompressor
{
    /// <inheritdoc/>
    public void Compress<TWriter>(ref TWriter writer, ReadOnlySpan<byte> data) where TWriter : IBytesWriter
    {
        GZip.Compress(ref writer, data);
    }

    /// <inheritdoc/>
    public void Decompress<TWriter>(ref TWriter writer, ReadOnlySpan<byte> data) where TWriter : IBytesWriter
    {
        GZip.Decompress(ref writer, data);
    }
}