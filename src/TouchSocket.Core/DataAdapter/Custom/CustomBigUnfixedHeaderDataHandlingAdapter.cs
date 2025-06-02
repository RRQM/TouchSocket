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
/// 大数据用户自定义固定包头解析器，使用该适配器时，接收方收到的数据中，<see cref="ByteBlock"/>将为null，同时<see cref="IRequestInfo"/>将实现为TFixedHeaderRequestInfo。
/// </summary>
public abstract class CustomBigUnfixedHeaderDataHandlingAdapter<TFixedHeaderRequestInfo> : CustomDataHandlingAdapter<TFixedHeaderRequestInfo>
    where TFixedHeaderRequestInfo : IBigUnfixedHeaderRequestInfo
{
    /// <summary>
    /// 筛选解析数据。实例化的TRequest会一直保存，直至解析成功，或手动清除。
    /// <para>当不满足解析条件时，请返回<see cref="FilterResult.Cache"/>，此时会保存<see cref="ByteBlock.CanReadLength"/>的数据</para>
    /// <para>当数据部分异常时，请移动<see cref="ByteBlock.Position"/>到指定位置，然后返回<see cref="FilterResult.GoOn"/></para>
    /// <para>当完全满足解析条件时，请返回<see cref="FilterResult.Success"/>最后将<see cref="ByteBlock.Position"/>移至指定位置。</para>
    /// </summary>
    /// <param name="byteBlock">字节块</param>
    /// <param name="beCached">是否为上次遗留对象，当该参数为<see langword="true"/>时，request也将是上次实例化的对象。</param>
    /// <param name="request">对象。</param>
    /// <param name="tempCapacity">缓存容量。当需要首次缓存时，指示申请的ByteBlock的容量。合理的值可避免ByteBlock扩容带来的性能消耗。</param>
    /// <returns></returns>
    protected override FilterResult Filter<TByteBlock>(ref TByteBlock byteBlock, bool beCached, ref TFixedHeaderRequestInfo request, ref int tempCapacity)
    {
        if (beCached)
        {
            while (this.m_surLen > 0 && byteBlock.CanReadLength > 0)
            {
                var r = (int)Math.Min(this.m_surLen, byteBlock.CanReadLength);
                var bytes = byteBlock.Span.Slice(byteBlock.Position, r);
                request.OnAppendBody(bytes);
                this.m_surLen -= r;
                byteBlock.Position += r;
                if (this.m_surLen == 0)
                {
                    if (request.OnFinished())
                    {
                        return FilterResult.Success;
                    }
                    request = default;
                    return FilterResult.GoOn;
                }
            }
            return FilterResult.GoOn;
        }
        else
        {
            var requestInfo = this.GetInstance();
            if (requestInfo.OnParsingHeader(ref byteBlock))
            {
                request = requestInfo;
                if (requestInfo.BodyLength == 0)
                {
                    if (requestInfo.OnFinished())
                    {
                        return FilterResult.Success;
                    }
                    request = default;
                    return FilterResult.GoOn;
                }
                this.m_surLen = request.BodyLength;

                while (this.m_surLen > 0 && byteBlock.CanReadLength > 0)
                {
                    var r = (int)Math.Min(this.m_surLen, byteBlock.CanReadLength);
                    var bytes = byteBlock.Span.Slice(byteBlock.Position, r);
                    request.OnAppendBody(bytes);
                    this.m_surLen -= r;
                    byteBlock.Position += r;
                    if (this.m_surLen == 0)
                    {
                        if (request.OnFinished())
                        {
                            return FilterResult.Success;
                        }
                        request = default;
                        return FilterResult.GoOn;
                    }
                }
                return FilterResult.GoOn;
            }
            else
            {
                this.SurLength = int.MaxValue;
                return FilterResult.Cache;
            }
        }
    }

    private long m_surLen;

    /// <summary>
    /// 获取泛型实例。
    /// </summary>
    /// <returns></returns>
    protected abstract TFixedHeaderRequestInfo GetInstance();
}

/// <summary>
/// 用户自定义固定包头请求
/// </summary>
public interface IBigUnfixedHeaderRequestInfo : IRequestInfo
{
    /// <summary>
    /// 协议头长度
    /// </summary>
    int HeaderLength { get; }

    /// <summary>
    /// 数据体长度
    /// </summary>
    long BodyLength { get; }

    /// <summary>
    /// 当收到数据，由框架封送数据，您需要在此函数中，解析自己的数据包头。
    /// <para>如果满足包头的解析，请返回True，并且递增整个包头的长度到<see cref="ByteBlock.Position"/>，然后赋值<see cref="BodyLength"/></para>
    /// <para>如果返回<see langword="false"/>，意味着缓存剩余数据，此时如果仅仅是因为长度不足，则不必修改其他。</para>
    /// <para>但是如果是因为数据错误，则需要修改<see cref="ByteBlock.Position"/>到正确位置，如果都不正确，则设置<see cref="ByteBlock.Position"/>等于<see cref="ByteBlock.Length"/></para>
    /// </summary>
    /// <param name="byteBlock"></param>
    /// <returns>是否满足解析包头</returns>
    bool OnParsingHeader<TByteBlock>(ref TByteBlock byteBlock) where TByteBlock : IByteBlock;

    /// <summary>
    /// 当收到数据，由框架封送数据。
    /// <para>您需要将有效数据自行保存。该方法可能会多次调用。</para>
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns>是否成功有效</returns>
    void OnAppendBody(ReadOnlySpan<byte> buffer);

    /// <summary>
    /// 当完成数据接收时调用。
    /// <para>当返回False时，将不会把该对象向Received传递。</para>
    /// </summary>
    /// <returns>是否成功有效</returns>
    bool OnFinished();
}