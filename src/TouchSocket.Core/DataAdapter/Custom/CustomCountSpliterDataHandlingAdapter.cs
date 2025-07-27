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

namespace TouchSocket.Core;

/// <summary>
/// 自定义计数分隔符数据处理适配器。
/// </summary>
/// <typeparam name="TCountSpliterRequestInfo">请求信息类型。</typeparam>
public abstract class CustomCountSpliterDataHandlingAdapter<TCountSpliterRequestInfo> : CustomDataHandlingAdapter<TCountSpliterRequestInfo> where TCountSpliterRequestInfo : IRequestInfo
{
    /// <summary>
    /// 获取计数。
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// 获取分隔符。
    /// </summary>
    public ReadOnlyMemory<byte> Spliter { get; }

    /// <summary>
    /// 初始化 <see cref="CustomCountSpliterDataHandlingAdapter{TCountSpliterRequestInfo}"/> 类的新实例。
    /// </summary>
    /// <param name="count">计数。</param>
    /// <param name="spliter">分隔符。</param>
    /// <exception cref="ArgumentOutOfRangeException">当计数小于2时抛出。</exception>
    /// <exception cref="ArgumentNullException">当分隔符为空时抛出。</exception>
    public CustomCountSpliterDataHandlingAdapter(int count, ReadOnlyMemory<byte> spliter)
    {
        if (count < 2)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(count), count, 2);
        }
        this.Count = count;
        this.Spliter = spliter;
    }

    /// <inheritdoc/>
    protected override FilterResult Filter<TByteBlock>(ref TByteBlock byteBlock, bool beCached, ref TCountSpliterRequestInfo request, ref int tempCapacity)
    {
        var count = 0;

        var spanAll = byteBlock.Sequence;
        var startIndex = 0L;
        var length = 0L;

        var spliterSpan = this.Spliter.Span;

        while (true)
        {
            if (spanAll.Length <= startIndex + length)
            {
                return FilterResult.Cache;
            }
            var currentSpan = spanAll.Slice(startIndex + length);
            var r = currentSpan.IndexOf(spliterSpan);
            if (r < 0)
            {
                return FilterResult.Cache;
            }

            count++;

            if (count == 1)
            {
                startIndex = r;
            }

            length += r + spliterSpan.Length;

            if (count == this.Count)
            {
                var span = spanAll.Slice(startIndex, length);

                request = this.GetInstance(span);
                byteBlock.Advance((int)length);
                return FilterResult.Success;
            }
        }
    }

    /// <summary>
    /// 获取请求信息实例。
    /// </summary>
    /// <param name="dataSpan">数据跨度。</param>
    /// <returns>请求信息实例。</returns>
    protected abstract TCountSpliterRequestInfo GetInstance(in ReadOnlySpan<byte> dataSpan);
    protected virtual TCountSpliterRequestInfo GetInstance(in ReadOnlySequence<byte> dataSequence)
    {
        using (var memoryBuffer = new ContiguousMemoryBuffer(dataSequence))
        {
            return this.GetInstance(memoryBuffer.Memory.Span);
        }
    }
}