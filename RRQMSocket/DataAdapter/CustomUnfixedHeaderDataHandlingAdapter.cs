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
    /// 用户自定义固定包头解析器，使用该适配器时，接收方收到的数据中，<see cref="ByteBlock"/>将为null，同时<see cref="IRequestInfo"/>将实现为TUnfixedHeaderRequestInfo。
    /// </summary>
    public abstract class CustomUnfixedHeaderDataHandlingAdapter<TUnfixedHeaderRequestInfo> : CustomDataHandlingAdapter<TUnfixedHeaderRequestInfo> where TUnfixedHeaderRequestInfo : IUnfixedHeaderRequestInfo
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="length"></param>
        /// <param name="beCached"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override FilterResult Filter(ByteBlock byteBlock, int length, bool beCached, ref TUnfixedHeaderRequestInfo request)
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
                TUnfixedHeaderRequestInfo requestInfo = this.GetInstance();
                if (requestInfo.OnParsingHeader(byteBlock,length))
                {
                    request = requestInfo;
                    if (requestInfo.BodyLength > byteBlock.CanReadLen)//body不满足解析，开始缓存，然后保存对象
                    {
                        return FilterResult.Cache;
                    }

                    byteBlock.Read(out byte[] body, requestInfo.BodyLength);
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
                    return FilterResult.Cache;
                }
            }
        }

        /// <summary>
        /// 获取泛型实例。
        /// </summary>
        /// <returns></returns>
        protected abstract TUnfixedHeaderRequestInfo GetInstance();
    }
}
