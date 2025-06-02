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
using TouchSocket.Resources;

namespace TouchSocket.Core;

/// <summary>
/// 用户自定义固定包头内存池解析器，使用该适配器时，接收方收到的数据中，<see cref="ByteBlock"/>将为null，同时<see cref="IRequestInfo"/>将实现为TFixedHeaderRequestInfo。
/// </summary>
public abstract class CustomFixedHeaderByteBlockDataHandlingAdapter<TFixedHeaderRequestInfo> : CustomDataHandlingAdapter<TFixedHeaderRequestInfo> where TFixedHeaderRequestInfo : IFixedHeaderByteBlockRequestInfo
{
    /// <summary>
    /// 固定包头的长度。
    /// </summary>
    public abstract int HeaderLength { get; }

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
            if (request.BodyLength > byteBlock.CanReadLength)//body不满足解析，开始缓存，然后保存对象
            {
                tempCapacity = request.BodyLength + this.HeaderLength;
                this.SurLength = request.BodyLength - byteBlock.CanReadLength;
                return FilterResult.Cache;
            }


            var block = new ByteBlock(request.BodyLength);

            block.Write(byteBlock.Span.Slice(byteBlock.Position, request.BodyLength));
            //block.Write(byteBlock.Buffer, byteBlock.Position, request.BodyLength);
            block.SeekToStart();
            if (request.OnParsingBody(block))
            {
                byteBlock.Position += request.BodyLength;
                return FilterResult.Success;
            }
            else
            {
                byteBlock.Position += 1;
                return FilterResult.GoOn;
            }
        }
        else
        {
            if (this.HeaderLength > byteBlock.CanReadLength)
            {
                this.SurLength = this.HeaderLength - byteBlock.CanReadLength;
                return FilterResult.Cache;
            }

            var requestInfo = this.GetInstance();
            var header = byteBlock.Span.Slice(byteBlock.Position, this.HeaderLength);
            if (requestInfo.OnParsingHeader(header))
            {
                byteBlock.Position += this.HeaderLength;
                if (requestInfo.BodyLength > this.MaxPackageSize)
                {
                    this.OnError(default, TouchSocketCoreResource.ValueMoreThan.Format(nameof(requestInfo.BodyLength), requestInfo.BodyLength, this.MaxPackageSize), true, true);
                    return FilterResult.GoOn;
                }
                request = requestInfo;
                if (requestInfo.BodyLength > byteBlock.CanReadLength)//body不满足解析，开始缓存，然后保存对象
                {
                    tempCapacity = requestInfo.BodyLength + this.HeaderLength;
                    this.SurLength = requestInfo.BodyLength - byteBlock.CanReadLength;
                    return FilterResult.Cache;
                }

                var block = new ByteBlock(request.BodyLength);
                block.Write(byteBlock.Span.Slice(byteBlock.Position, request.BodyLength));
                block.SeekToStart();

                if (requestInfo.OnParsingBody(block))
                {
                    byteBlock.Position += request.BodyLength;
                    return FilterResult.Success;
                }
                else
                {
                    byteBlock.Position += 1;
                    return FilterResult.GoOn;
                }
            }
            else
            {
                byteBlock.Position += 1;
                return FilterResult.GoOn;
            }
        }
    }

    /// <summary>
    /// 获取泛型实例。
    /// </summary>
    /// <returns></returns>
    protected abstract TFixedHeaderRequestInfo GetInstance();
}

/// <summary>
/// 用户自定义固定包头请求
/// </summary>
public interface IFixedHeaderByteBlockRequestInfo : IRequestInfo
{
    /// <summary>
    /// 数据体长度
    /// </summary>
    int BodyLength { get; }

    /// <summary>
    /// 当收到数据，由框架封送固定协议头。
    /// <para>您需要在此函数中，解析自己的固定包头，并且对<see cref="BodyLength"/>赋值后续数据的长度，然后返回True。</para>
    /// <para>如果返回<see langword="false"/>，则意味着放弃本次解析</para>
    /// </summary>
    /// <param name="header"></param>
    /// <returns></returns>
    bool OnParsingHeader(ReadOnlySpan<byte> header);

    /// <summary>
    /// 当收到数据，由框架封送有效载荷数据。
    /// <para>如果返回<see langword="false"/>，意味着放弃本次解析的所有数据，包括已经解析完成的Header</para>
    /// </summary>
    /// <param name="byteBlock">载荷数据，注意：该字节块生命周期不受框架控制，请一定自行调用Dispose</param>
    /// <returns>是否成功有效</returns>
    bool OnParsingBody(ByteBlock byteBlock);
}