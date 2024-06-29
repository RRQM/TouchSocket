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

namespace TouchSocket.Core
{
    /// <summary>
    /// 区间数据包处理适配器，支持以任意字符、字节数组起始与结尾的数据包。
    /// </summary>
    public abstract class CustomBetweenAndDataHandlingAdapter<TBetweenAndRequestInfo> : CustomDataHandlingAdapter<TBetweenAndRequestInfo> where TBetweenAndRequestInfo : IBetweenAndRequestInfo
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
            if (beCached)
            {
                var len = 0;
                var pos = byteBlock.Position;
                while (true)
                {
                    var indexEnd = byteBlock.Span.Slice(byteBlock.Position, byteBlock.CanReadLength).IndexOf(this.EndCode);
                    if (indexEnd == -1)
                    {
                        byteBlock.Position = pos;
                        return FilterResult.Cache;
                    }
                    else
                    {
                        len += indexEnd;
                        byteBlock.Position += indexEnd;
                        if (len >= this.MinSize)
                        {
                            break;
                        }

                        len += this.EndCode.Length;
                        byteBlock.Position += this.EndCode.Length;
                    }

                }

                //byteBlock.Position = pos;
                request.OnParsingBody(byteBlock.Span.Slice(pos, len));
                byteBlock.Position = len+pos;

                if (request.OnParsingEndCode(byteBlock.Span.Slice(byteBlock.Position, this.EndCode.Length)))
                {
                    byteBlock.Position += this.EndCode.Length;
                    return FilterResult.Success;
                }
                else
                {
                    byteBlock.Position += this.EndCode.Length;
                    return FilterResult.GoOn;
                }
            }
            else
            {
                var indexStart = byteBlock.Span.Slice(byteBlock.Position, byteBlock.CanReadLength).IndexOf(this.StartCode);
                if (indexStart == -1)
                {
                    return FilterResult.Cache;
                }

                var requestInfo = this.GetInstance();

                if (requestInfo.OnParsingStartCode(byteBlock.Span.Slice(byteBlock.Position, this.StartCode.Length)))
                {
                    byteBlock.Position += this.StartCode.Length;
                }
                else
                {
                    byteBlock.Position += 1;
                    return FilterResult.GoOn;
                }

                request = requestInfo;

                var len = 0;
                var pos = byteBlock.Position;
                while (true)
                {
                    var indexEnd = byteBlock.Span.Slice(byteBlock.Position, byteBlock.CanReadLength).IndexOf(this.EndCode);
                    if (indexEnd == -1)
                    {
                        byteBlock.Position = pos;
                        return FilterResult.Cache;
                    }
                    else
                    {
                        len += indexEnd;
                        byteBlock.Position += indexEnd;

                        if (len >= this.MinSize)
                        {
                            break;
                        }

                        len += this.EndCode.Length;
                        byteBlock.Position += this.EndCode.Length;
                    }
                }

                //byteBlock.Position = pos;
                request.OnParsingBody(byteBlock.Span.Slice(pos, len));
                byteBlock.Position = len + pos;

                if (request.OnParsingEndCode(byteBlock.Span.Slice(byteBlock.Position, this.EndCode.Length)))
                {
                    byteBlock.Position += this.EndCode.Length;
                    return FilterResult.Success;
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
        protected abstract TBetweenAndRequestInfo GetInstance();
    }

    /// <summary>
    /// 区间类型的适配器数据模型接口。
    /// </summary>
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
}