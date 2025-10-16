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

using System.IO.Pipelines;

namespace TouchSocket.Core;

/// <summary>
/// 表示一个基于<see cref="PipeWriter"/>的字节写入器，提供对管道写入器的高性能包装。
/// 实现了<see cref="IBytesWriter"/>接口，支持异步刷新操作。
/// </summary>
/// <remarks>
/// PipeBytesWriter作为值类型实现，为System.IO.Pipelines的PipeWriter提供了IBytesWriter接口的适配。
/// 适用于需要与管道系统集成的高性能字节写入场景，不支持回退操作。
/// 提供了异步刷新功能，可以与异步I/O操作良好配合。
/// </remarks>
public struct PipeBytesWriter : IBytesWriter
{
    private readonly PipeWriter m_writer;
    private long m_writtenCount;

    /// <summary>
    /// 使用指定的管道写入器初始化<see cref="PipeBytesWriter"/>的新实例。
    /// </summary>
    /// <param name="writer">底层的管道写入器实例。</param>
    public PipeBytesWriter(PipeWriter writer)
    {
        this.m_writer = writer;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// 管道写入器不支持回退操作，始终返回<see langword="false"/>。
    /// </remarks>
    public readonly bool SupportsRewind => false;

    /// <inheritdoc/>
    /// <remarks>
    /// 管道写入器的版本号固定为0，因为不支持版本跟踪。
    /// </remarks>
    public readonly short Version => 0;

    /// <inheritdoc/>
    public readonly long WrittenCount => this.m_writtenCount;

    /// <inheritdoc/>
    /// <remarks>
    /// 推进底层管道写入器的位置，同时更新写入计数。
    /// </remarks>
    public void Advance(int count)
    {
        this.m_writer.Advance(count);
        this.m_writtenCount += count;
    }

    /// <summary>
    /// 异步刷新底层管道写入器，确保缓冲的数据被写入到目标。
    /// </summary>
    /// <param name="cancellationToken">用于取消操作的取消令牌。</param>
    /// <returns>表示异步刷新操作的<see cref="ValueTask{T}"/>，包含刷新结果。</returns>
    /// <remarks>
    /// 此方法提供了对底层PipeWriter.FlushAsync方法的直接访问，
    /// 允许调用方控制何时将缓冲数据刷新到目标流。
    /// </remarks>
    public readonly ValueTask<FlushResult> FlushAsync(CancellationToken cancellationToken = default)
    {
        return this.m_writer.FlushAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public readonly Memory<byte> GetMemory(int sizeHint = 0)
    {
        return this.m_writer.GetMemory(sizeHint);
    }

    /// <inheritdoc/>
    public readonly Span<byte> GetSpan(int sizeHint = 0)
    {
        return this.m_writer.GetSpan(sizeHint);
    }

    /// <inheritdoc/>
    public void Write(scoped ReadOnlySpan<byte> span)
    {
        var length = span.Length;
        if (length == 0)
        {
            return;
        }
        var memory = this.GetMemory(length);
        span.CopyTo(memory.Span);
        this.Advance(length);
    }
}