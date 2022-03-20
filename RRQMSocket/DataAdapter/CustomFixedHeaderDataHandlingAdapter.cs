//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore.ByteManager;

namespace RRQMSocket
{
    /// <summary>
    /// 用户自定义固定包头解析器，使用该适配器时，接收方收到的数据中，<see cref="ByteBlock"/>将为null，同时<see cref="IRequestInfo"/>将实现为TFixedHeaderRequestInfo。
    /// </summary>
    public abstract class CustomFixedHeaderDataHandlingAdapter<TFixedHeaderRequestInfo> : CustomDataHandlingAdapter<TFixedHeaderRequestInfo> where TFixedHeaderRequestInfo : IFixedHeaderRequestInfo
    {
        /// <summary>
        /// 固定包头的长度。
        /// </summary>
        public abstract int HeaderLength { get; }

        /// <summary>
        /// 筛选解析数据。实例化的TRequest会一直保存，直至解析成功，或手动清除。
        /// <para>当不满足解析条件时，请返回<see cref="FilterResult.Cache"/>，此时会保存<see cref="ByteBlock.CanReadLen"/>的数据</para>
        /// <para>当数据部分异常时，请移动<see cref="ByteBlock.Pos"/>到指定位置，然后返回<see cref="FilterResult.GoOn"/></para>
        /// <para>当完全满足解析条件时，请返回<see cref="FilterResult.Success"/>最后将<see cref="ByteBlock.Pos"/>移至指定位置。</para>
        /// </summary>
        /// <param name="byteBlock">字节块</param>
        /// <param name="length">剩余有效数据长度，计算实质为:ByteBlock.Len和ByteBlock.Pos的差值。</param>
        /// <param name="beCached">是否为上次遗留对象，当该参数为True时，request也将是上次实例化的对象。</param>
        /// <param name="request">对象。</param>
        /// <returns></returns>
        protected override FilterResult Filter(ByteBlock byteBlock, int length, bool beCached, ref TFixedHeaderRequestInfo request)
        {
            if (beCached)
            {
                if (request.BodyLength > byteBlock.CanReadLen)//body不满足解析，开始缓存，然后保存对象
                {
                    return FilterResult.Cache;
                }

                byteBlock.Read(out byte[] body, request.BodyLength);
                if (request.OnParsingBody(body))
                {
                    return FilterResult.Success;
                }
                else
                {
                    request = default;//放弃所有解析
                    return FilterResult.GoOn;
                }
            }
            else
            {
                if (this.HeaderLength > length)
                {
                    return FilterResult.Cache;
                }

                TFixedHeaderRequestInfo requestInfo = this.GetInstance();
                byteBlock.Read(out byte[] header, this.HeaderLength);
                if (requestInfo.OnParsingHeader(header))
                {
                    request = requestInfo;
                    if (requestInfo.BodyLength > byteBlock.CanReadLen)//body不满足解析，开始缓存，然后保存对象
                    {
                        return FilterResult.Cache;
                    }

                    byteBlock.Read(out byte[] body,requestInfo.BodyLength);
                    if (requestInfo.OnParsingBody(body))
                    {
                        return FilterResult.Success;
                    }
                    else
                    {
                        request = default;//放弃所有解析
                        return FilterResult.GoOn;
                    }
                }
                else
                {
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
}