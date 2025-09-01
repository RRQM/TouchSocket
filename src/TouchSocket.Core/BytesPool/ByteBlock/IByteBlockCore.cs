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
/// 表示字节块的核心接口，定义了字节块的基本属性和访问方式。
/// </summary>
/// <remarks>
/// 此接口提供了字节块的核心功能，包括长度信息、位置信息以及内存和跨度的访问方式。
/// 是所有字节块相关接口的基础接口。
/// </remarks>
public interface IByteBlockCore
{
    /// <summary>
    /// 获取字节块中有效数据的长度。
    /// </summary>
    /// <value>表示字节块中包含的有效数据字节数。</value>
    int Length { get; }

    /// <summary>
    /// 获取字节块的只读内存表示形式。
    /// </summary>
    /// <value>返回一个<see cref="ReadOnlyMemory{T}"/>，表示字节块中从索引0到<see cref="Length"/>的有效数据。</value>
    ReadOnlyMemory<byte> Memory { get; }
    
    /// <summary>
    /// 获取或设置字节块中的当前位置。
    /// </summary>
    /// <value>表示字节块中的当前读写位置，从0开始的索引。</value>
    /// <remarks>
    /// 位置用于跟踪读取或写入操作的当前位置，通常在0到<see cref="Length"/>之间。
    /// </remarks>
    int Position { get; set; }
    
    /// <summary>
    /// 获取字节块的只读跨度表示形式。
    /// </summary>
    /// <value>返回一个<see cref="ReadOnlySpan{T}"/>，表示字节块中从索引0到<see cref="Length"/>的有效数据。</value>
    /// <remarks>
    /// 跨度提供了对字节块数据的高性能访问方式，适用于需要直接操作字节数据的场景。
    /// </remarks>
    ReadOnlySpan<byte> Span { get; }
}