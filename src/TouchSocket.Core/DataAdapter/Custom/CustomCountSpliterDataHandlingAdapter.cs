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
/// <typeparam name="TCountSpliterRequestInfo">请求信息类型，必须实现<see cref="IRequestInfo"/>接口。</typeparam>
/// <remarks>
/// 此适配器通过计数特定分隔符的出现次数来分割数据流。
/// 当达到指定的分隔符计数时，将提取相应的数据段并创建请求信息实例。
/// 适用于基于固定分隔符模式的协议解析场景。
/// </remarks>
public abstract class CustomCountSpliterDataHandlingAdapter<TCountSpliterRequestInfo> : CustomDataHandlingAdapter<TCountSpliterRequestInfo> where TCountSpliterRequestInfo : IRequestInfo
{
    /// <summary>
    /// 初始化<see cref="CustomCountSpliterDataHandlingAdapter{TCountSpliterRequestInfo}"/>类的新实例。
    /// </summary>
    /// <param name="count">分隔符的计数阈值，必须大于等于2。</param>
    /// <param name="spliter">用于分割数据的分隔符字节序列。</param>
    /// <exception cref="ArgumentOutOfRangeException">当<paramref name="count"/>小于2时抛出。</exception>
    /// <exception cref="ArgumentNullException">当<paramref name="spliter"/>为空时抛出。</exception>
    /// <remarks>
    /// 构造函数验证参数的有效性。计数必须至少为2才能进行有意义的数据分割，
    /// 分隔符不能为空以确保能够正确识别数据边界。
    /// </remarks>
    public CustomCountSpliterDataHandlingAdapter(int count, ReadOnlyMemory<byte> spliter)
    {
        if (count < 2)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_LessThan(nameof(count), count, 2);
        }
        this.Count = count;
        this.Spliter = spliter;
    }

    /// <summary>
    /// 获取分隔符的计数阈值。
    /// </summary>
    /// <value>当分隔符出现次数达到此值时，将触发数据提取操作。</value>
    /// <remarks>
    /// 此属性定义了适配器在提取数据段之前需要计数的分隔符数量。
    /// 值必须大于等于2，以确保能够识别数据的开始和结束边界。
    /// </remarks>
    public int Count { get; }

    /// <summary>
    /// 获取用于分割数据的分隔符。
    /// </summary>
    /// <value>包含分隔符字节序列的只读内存块。</value>
    /// <remarks>
    /// 此分隔符用于在数据流中标识数据段的边界。
    /// 适配器将在输入流中搜索此字节序列的出现次数。
    /// </remarks>
    public ReadOnlyMemory<byte> Spliter { get; }

    /// <summary>
    /// 筛选解析数据，通过计数分隔符来确定数据边界。
    /// </summary>
    /// <typeparam name="TByteBlock">字节块类型，必须实现<see cref="IBytesReader"/>接口。</typeparam>
    /// <param name="byteBlock">要解析的字节块。</param>
    /// <param name="beCached">指示当前请求对象是否为缓存的上次实例。</param>
    /// <param name="request">输出的请求信息对象。</param>
    /// <returns>
    /// <see cref="FilterResult.Cache"/>：当分隔符计数未达到阈值时，需要缓存更多数据。
    /// <see cref="FilterResult.Success"/>：成功解析出完整的数据段并创建了请求信息实例。
    /// </returns>
    /// <remarks>
    /// <para>此方法实现了核心的数据分割逻辑：</para>
    /// <list type="number">
    /// <item>在字节块中搜索分隔符出现的位置</item>
    /// <item>计数分隔符的出现次数</item>
    /// <item>当计数达到<see cref="Count"/>时，提取相应的数据段</item>
    /// <item>调用<see cref="GetInstance(in ReadOnlySpan{byte})"/>创建请求信息实例</item>
    /// </list>
    /// <para>如果分隔符计数不足，方法返回<see cref="FilterResult.Cache"/>以等待更多数据。</para>
    /// </remarks>
    protected override FilterResult Filter<TByteBlock>(ref TByteBlock byteBlock, bool beCached, ref TCountSpliterRequestInfo request)
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
    /// 从数据跨度创建请求信息实例。
    /// </summary>
    /// <param name="dataSpan">包含请求数据的字节跨度。</param>
    /// <returns>基于提供数据创建的请求信息实例。</returns>
    /// <remarks>
    /// 此抽象方法由派生类实现，用于将解析出的数据段转换为具体的请求信息对象。
    /// 数据跨度包含了从第一个分隔符到最后一个分隔符（包含）的完整数据段。
    /// </remarks>
    protected abstract TCountSpliterRequestInfo GetInstance(in ReadOnlySpan<byte> dataSpan);

    /// <summary>
    /// 从数据序列创建请求信息实例。
    /// </summary>
    /// <param name="dataSequence">包含请求数据的字节序列。</param>
    /// <returns>基于提供数据创建的请求信息实例。</returns>
    /// <remarks>
    /// 此虚拟方法提供了处理非连续内存数据的默认实现。
    /// 它将数据序列转换为连续内存，然后调用<see cref="GetInstance(in ReadOnlySpan{byte})"/>方法。
    /// 派生类可以重写此方法以提供更高效的实现。
    /// </remarks>
    protected virtual TCountSpliterRequestInfo GetInstance(in ReadOnlySequence<byte> dataSequence)
    {
        using (var memoryBuffer = new ContiguousMemoryBuffer(dataSequence))
        {
            return this.GetInstance(memoryBuffer.Memory.Span);
        }
    }
}