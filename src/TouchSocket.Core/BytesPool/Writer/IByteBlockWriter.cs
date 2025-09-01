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

namespace TouchSocket.Core;

/// <summary>
/// 表示字节块写入器接口，提供字节块的写入功能和容量管理。
/// 继承自<see cref="IBytesWriter"/>和<see cref="IByteBlockCore"/>接口。
/// </summary>
/// <remarks>
/// IByteBlockWriter接口结合了通用字节写入和字节块核心功能，
/// 为字节块的写入操作提供了专门的接口定义，并增加了容量管理能力。
/// </remarks>
public interface IByteBlockWriter : IBytesWriter, IByteBlockCore
{
    /// <summary>
    /// 获取字节块的总容量。
    /// </summary>
    /// <value>表示字节块可以存储的最大字节数。</value>
    /// <remarks>
    /// 容量通常大于或等于<see cref="IByteBlockCore.Length"/>，表示已分配的内存大小。
    /// </remarks>
    int Capacity { get; }

    /// <summary>
    /// 获取字节块的可用空间长度。
    /// </summary>
    /// <value>表示从当前位置到容量末尾还可以写入的字节数。</value>
    /// <remarks>
    /// 此值等于<see cref="Capacity"/>与<see cref="IByteBlockCore.Position"/>的差值。
    /// </remarks>
    int FreeLength { get; }

    /// <summary>
    /// 获取字节块的完整内存表示形式。
    /// </summary>
    /// <value>返回一个<see cref="Memory{T}"/>，表示字节块的完整内存空间，从索引0到<see cref="Capacity"/>。</value>
    /// <remarks>
    /// 与<see cref="IByteBlockCore.Memory"/>不同，此属性返回完整的已分配内存，而不仅仅是有效数据部分。
    /// </remarks>
    Memory<byte> TotalMemory { get; }

    /// <summary>
    /// 设置字节块的有效数据长度。
    /// </summary>
    /// <param name="value">要设置的新长度值。</param>
    /// <exception cref="ArgumentOutOfRangeException">当<paramref name="value"/>超过<see cref="Capacity"/>时抛出。</exception>
    /// <remarks>
    /// 此方法允许直接修改字节块的有效数据长度，通常用于预先知道数据大小的场景。
    /// </remarks>
    void SetLength(int value);
}