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

namespace TouchSocket.Core;

/// <summary>
/// 区间数据包处理适配器，支持以任意字符、字节数组起始与结尾的数据包。
/// </summary>
public abstract class CustomBetweenAndDataHandlingAdapter<TBetweenAndRequestInfo> : CustomDataHandlingAdapter<TBetweenAndRequestInfo> where TBetweenAndRequestInfo : IRequestInfo
{
    /// <summary>
    /// 起始字符，不可以为null，可以为0长度
    /// </summary>
    public abstract byte[] StartCode { get; }

    /// <summary>
    /// 即使找到了终止因子，也不会结束，默认0
    /// </summary>
    public int MinSize { get; set; }

    /// <summary>
    /// 结束字符，不可以为null，不可以为0长度，必须具有有效值。
    /// </summary>
    public abstract byte[] EndCode { get; }

    /// <summary>
    /// 筛选解析数据。实例化的TRequest会一直保存，直至解析成功，或手动清除。
    /// <para>当不满足解析条件时，请返回<see cref="FilterResult.Cache"/>，此时会保存<see cref="ByteBlock.CanReadLength"/>的数据</para>
    /// <para>当数据部分异常时，请移动<see cref="ByteBlock.Position"/>到指定位置，然后返回<see cref="FilterResult.GoOn"/></para>
    /// <para>当完全满足解析条件时，请返回<see cref="FilterResult.Success"/>最后将<see cref="ByteBlock.Position"/>移至指定位置。</para>
    /// </summary>
    /// <param name="byteBlock">字节块</param>
    /// <param name="beCached">是否为上次遗留对象，当该参数为True时，request也将是上次实例化的对象。</param>
    /// <param name="request">对象。</param>
    /// <param name="tempCapacity">缓存容量。当需要首次缓存时，指示申请的ByteBlock的容量。合理的值可避免ByteBlock扩容带来的性能消耗。</param>
    /// <returns></returns>
    protected override FilterResult Filter<T>(ref T byteBlock, bool beCached, ref TBetweenAndRequestInfo request, ref int tempCapacity)
    {
        ReadOnlySpan<byte> startCode = this.StartCode ?? ReadOnlySpan<byte>.Empty;
        ReadOnlySpan<byte> endCode = this.EndCode ?? ReadOnlySpan<byte>.Empty;
        // 检查终止字符是否为空
        if (endCode.IsEmpty)
        {
            ThrowHelper.ThrowException("区间字符适配器的终止字符不能为空");
        }
        // 获取可读取的字节范围
        var canReadSpan = byteBlock.Span.Slice(byteBlock.Position, byteBlock.CanReadLength);
        // 查找起始码的索引
        var startCodeIndex = canReadSpan.IndexOf(startCode);
        if (startCodeIndex < 0)
        {
            // 未找到起始码，缓存所有数据
            return FilterResult.Cache;
        }
        // 从起始码之后的位置开始查找
        var searchSpan = canReadSpan.Slice(startCodeIndex + startCode.Length);
        var currentSearchOffset = 0;
        while (true)
        {
            // 查找终止码的索引
            var endCodeIndex = searchSpan.IndexOf(endCode);
            if (endCodeIndex < 0)
            {
                // 未找到终止码，缓存所有数据
                return FilterResult.Cache;
            }
            // 计算区间长度
            var bodyLength = endCodeIndex + currentSearchOffset;
            if (bodyLength >= this.MinSize)
            {
                // 区间长度满足要求，提取数据
                var body = canReadSpan.Slice(startCodeIndex + startCode.Length, bodyLength);
                request = this.GetInstance(body);
                // 更新字节块的位置
                byteBlock.Position += startCodeIndex + startCode.Length + bodyLength + endCode.Length;
                return FilterResult.Success;
            }
            else
            {
                // 区间长度不满足要求，继续从当前终止码之后查找
                currentSearchOffset += endCodeIndex + endCode.Length;
                searchSpan = searchSpan.Slice(endCodeIndex + endCode.Length);
            }
        }
    }

    /// <summary>
    /// 获取泛型实例。
    /// </summary>
    /// <param name="body">数据体</param>
    /// <returns>泛型实例</returns>
    protected abstract TBetweenAndRequestInfo GetInstance(ReadOnlySpan<byte> body);
}

/// <summary>
/// 区间类型的适配器数据模型接口。
/// </summary>
[Obsolete("此接口已被弃用，请使用IRequestInfo代替约束。具体数据会在CustomBetweenAndDataHandlingAdapter.GetInstance(ReadOnlySpan<byte> body)直接投递。",true)]
public interface IBetweenAndRequestInfo : IRequestInfo
{
    /// <summary>
    /// 当解析到起始字符时。
    /// </summary>
    /// <param name="startCode"></param>
    /// <returns></returns>
    bool OnParsingStartCode(ReadOnlySpan<byte> startCode);

    /// <summary>
    /// 当解析数据体。
    /// <para>在此方法中，您必须手动保存Body内容</para>
    /// </summary>
    /// <param name="body"></param>
    void OnParsingBody(ReadOnlySpan<byte> body);

    /// <summary>
    /// 当解析到起始字符时。
    /// </summary>
    /// <param name="endCode"></param>
    /// <returns></returns>
    bool OnParsingEndCode(ReadOnlySpan<byte> endCode);
}