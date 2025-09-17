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
using System.Buffers;

namespace TouchSocket.Core;

/// <summary>
/// 表示一个字节写入器接口，用于提供高性能的字节缓冲区写入功能。
/// 继承自<see cref="IBufferWriter{T}"/>接口，扩展了版本控制、计数统计和回退支持等功能。
/// </summary>
/// <remarks>
/// IBytesWriter提供了对字节缓冲区的写入操作，支持获取内存、推进位置等基本功能，
/// 同时增加了版本管理、写入计数统计和回退操作支持等高级功能。
/// 适用于需要高性能字节写入和精确控制的场景。
/// </remarks>
public interface IBytesWriter : IBufferWriter<byte>
{
    /// <summary>
    /// 获取写入器的版本号。
    /// </summary>
    /// <value>表示写入器当前版本的短整型数值。</value>
    /// <remarks>
    /// 版本号用于检测写入器的状态变化，通常在支持回退操作时用于验证写入器的一致性。
    /// 当写入器的内部状态发生变化时（如缓冲区重新分配），版本号可能会发生变化。
    /// </remarks>
    short Version { get; }

    /// <summary>
    /// 获取已写入的字节总数。
    /// </summary>
    /// <value>表示从写入器创建以来已写入的字节数量。</value>
    /// <remarks>
    /// 此值会随着写入操作的进行而累积增加，提供了写入操作的统计信息。
    /// 可用于跟踪写入进度或进行性能分析。
    /// </remarks>
    long WrittenCount { get; }

    /// <summary>
    /// 获取一个值，该值指示写入器是否支持回退操作。
    /// </summary>
    /// <value>如果写入器支持回退到之前的位置，则为<see langword="true"/>；否则为<see langword="false"/>。</value>
    /// <remarks>
    /// 回退功能允许写入器返回到之前的写入位置，这在需要重写或修正之前写入内容的场景中非常有用。
    /// 不是所有的写入器实现都支持此功能，具体取决于底层的缓冲区管理策略。
    /// </remarks>
    bool SupportsRewind { get; }

    /// <summary>
    /// 将指定的字节跨度写入到写入器中。
    /// </summary>
    /// <param name="span">要写入的字节只读跨度。</param>
    /// <remarks>
    /// 此方法提供了直接写入字节数据的便捷方式，等效于获取内存、复制数据、然后推进位置的组合操作。
    /// 写入操作会自动更新<see cref="WrittenCount"/>计数。
    /// </remarks>
    void Write(scoped ReadOnlySpan<byte> span);
}
