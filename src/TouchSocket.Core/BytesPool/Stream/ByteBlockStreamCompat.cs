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
using System.Threading;
using System.Threading.Tasks;

namespace TouchSocket.Core;

/// <summary>
/// ByteBlockStream的兼容性扩展和条件编译方法
/// </summary>
internal sealed partial class ByteBlockStream
{
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    /// <summary>
    /// 读取数据到Span
    /// </summary>
    /// <param name="buffer">目标缓冲区</param>
    /// <returns>实际读取的字节数</returns>
    public override int Read(Span<byte> buffer)
    {
        this.ThrowIfDisposed();
        return this.m_byteBlock.Read(buffer);
    }

    /// <summary>
    /// 异步读取数据到Memory
    /// </summary>
    /// <param name="buffer">目标缓冲区</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>实际读取的字节数</returns>
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return CreateCanceledValueTask<int>(cancellationToken);
        }

        this.ThrowIfDisposed();

        try
        {
            var result = this.m_byteBlock.Read(buffer.Span);
            return new ValueTask<int>(result);
        }
        catch (Exception ex)
        {
            return new ValueTask<int>(Task.FromException<int>(ex));
        }
    }

    /// <summary>
    /// 写入Span数据
    /// </summary>
    /// <param name="buffer">源数据</param>
    public override void Write(ReadOnlySpan<byte> buffer)
    {
        this.ThrowIfDisposed();
        this.m_byteBlock.Write(buffer);
    }

    /// <summary>
    /// 异步写入Memory数据
    /// </summary>
    /// <param name="buffer">源数据</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>表示异步写入操作的任务</returns>
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return CreateCanceledValueTask(cancellationToken);
        }

        this.ThrowIfDisposed();

        try
        {
            this.m_byteBlock.Write(buffer.Span);
            return default;
        }
        catch (Exception ex)
        {
            return new ValueTask(Task.FromException(ex));
        }
    }

    /// <summary>
    /// 异步释放资源
    /// </summary>
    /// <returns>表示异步释放操作的任务</returns>
    public override ValueTask DisposeAsync()
    {
        try
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
            return default;
        }
        catch (Exception ex)
        {
            return new ValueTask(Task.FromException(ex));
        }
    }
#endif

    /// <summary>
    /// 创建已取消的ValueTask（兼容性方法）
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>已取消的ValueTask</returns>
    private static ValueTask CreateCanceledValueTask(CancellationToken cancellationToken)
    {
#if NET6_0_OR_GREATER
        return ValueTask.FromCanceled(cancellationToken);
#else
        return new ValueTask(CreateCanceledTask(cancellationToken));
#endif
    }

    /// <summary>
    /// 创建已取消的ValueTask&lt;T&gt;（兼容性方法）
    /// </summary>
    /// <typeparam name="T">返回类型</typeparam>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>已取消的ValueTask&lt;T&gt;</returns>
    private static ValueTask<T> CreateCanceledValueTask<T>(CancellationToken cancellationToken)
    {
#if NET6_0_OR_GREATER
        return ValueTask.FromCanceled<T>(cancellationToken);
#else
        return new ValueTask<T>(CreateCanceledTask<T>(cancellationToken));
#endif
    }

    /// <summary>
    /// 创建已取消的Task（兼容性方法）
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>已取消的Task</returns>
    private static Task CreateCanceledTask(CancellationToken cancellationToken)
    {
#if NET6_0_OR_GREATER
        return Task.FromCanceled(cancellationToken);
#else
        return TaskExtensions.FromCanceled(cancellationToken);
#endif
    }

    /// <summary>
    /// 创建已取消的Task&lt;T&gt;（兼容性方法）
    /// </summary>
    /// <typeparam name="T">返回类型</typeparam>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>已取消的Task&lt;T&gt;</returns>
    private static Task<T> CreateCanceledTask<T>(CancellationToken cancellationToken)
    {
#if NET6_0_OR_GREATER
        return Task.FromCanceled<T>(cancellationToken);
#else
        return TaskExtensions.FromCanceled<T>(cancellationToken);
#endif
    }

    /// <summary>
    /// 高效的跨平台复制实现
    /// </summary>
    /// <param name="destination">目标流</param>
    /// <param name="data">要复制的数据</param>
    private static void WritePlatformOptimized(Stream destination, ReadOnlyMemory<byte> data)
    {
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        destination.Write(data.Span);
#else
        // 对于不支持Span的版本，使用传统方式
        var array = data.ToArray();
        destination.Write(array, 0, array.Length);
#endif
    }

    /// <summary>
    /// 高效的跨平台异步复制实现
    /// </summary>
    /// <param name="destination">目标流</param>
    /// <param name="data">要复制的数据</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>异步操作任务</returns>
    private static Task WriteAsyncPlatformOptimized(Stream destination, ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
    {
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
        return destination.WriteAsync(data, cancellationToken).AsTask();
#else
        // 对于不支持Memory的版本，使用传统方式
        var array = data.ToArray();
        return destination.WriteAsync(array, 0, array.Length, cancellationToken);
#endif
    }
}

#if !NET6_0_OR_GREATER
/// <summary>
/// Task扩展方法，用于兼容性
/// </summary>
internal static class TaskExtensions
{
    /// <summary>
    /// 创建已取消的Task
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>已取消的Task</returns>
    public static Task FromCanceled(CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<object>();
        tcs.SetCanceled();
        return tcs.Task;
    }

    /// <summary>
    /// 创建已取消的Task&lt;T&gt;
    /// </summary>
    /// <typeparam name="T">返回类型</typeparam>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>已取消的Task&lt;T&gt;</returns>
    public static Task<T> FromCanceled<T>(CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<T>();
        tcs.SetCanceled();
        return tcs.Task;
    }
}
#endif