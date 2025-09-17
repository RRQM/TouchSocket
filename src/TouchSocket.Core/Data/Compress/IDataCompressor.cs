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
using System.IO;

namespace TouchSocket.Core;

/// <summary>
/// 定义数据压缩器的接口，提供数据压缩和解压缩的抽象操作。
/// </summary>
/// <remarks>
/// IDataCompressor接口为不同的压缩算法提供统一的接口定义，
/// 支持将数据压缩后写入到字节写入器中，以及从压缩数据中解压缩并写入到字节写入器中。
/// 实现此接口的类可以提供如GZip、Deflate、Brotli等各种压缩算法的支持。
/// </remarks>
public interface IDataCompressor
{
    /// <summary>
    /// 将指定的数据进行压缩，并将压缩结果写入到字节写入器中。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器类型，必须继承自<see cref="IBytesWriter"/>。</typeparam>
    /// <param name="writer">用于写入压缩数据的字节写入器。</param>
    /// <param name="data">要压缩的原始数据。</param>
    /// <remarks>
    /// 此方法将原始数据使用特定的压缩算法进行压缩，
    /// 然后将压缩后的数据写入到提供的字节写入器中。
    /// 压缩算法的选择取决于具体的实现类。
    /// </remarks>
    void Compress<TWriter>(ref TWriter writer, ReadOnlySpan<byte> data) where TWriter : IBytesWriter;

    /// <summary>
    /// 将指定的压缩数据进行解压缩，并将解压缩结果写入到字节写入器中。
    /// </summary>
    /// <typeparam name="TWriter">字节写入器类型，必须继承自<see cref="IBytesWriter"/>。</typeparam>
    /// <param name="writer">用于写入解压缩数据的字节写入器。</param>
    /// <param name="data">要解压缩的压缩数据。</param>
    /// <remarks>
    /// 此方法将压缩数据使用对应的解压缩算法进行解压缩，
    /// 然后将解压缩后的原始数据写入到提供的字节写入器中。
    /// 解压缩算法必须与压缩时使用的算法相匹配。
    /// </remarks>
    /// <exception cref="InvalidDataException">当压缩数据格式无效或损坏时可能抛出。</exception>
    void Decompress<TWriter>(ref TWriter writer, ReadOnlySpan<byte> data) where TWriter : IBytesWriter;
}