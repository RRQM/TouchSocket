//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
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
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="length"></param>
        /// <param name="beCached"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override FilterResult Filter(ByteBlock byteBlock, int length, bool beCached, ref TFixedHeaderRequestInfo request)
        {
            if (this.HeaderLength > length)
            {
                return FilterResult.Ignore;
            }

            int position = byteBlock.Pos;//先记录初始流位置，防止不能解析时，可以重置流位置。
            TFixedHeaderRequestInfo requestInfo = this.GetInstance();
            if (requestInfo.OnParsingHeader(byteBlock.ToArray(byteBlock.Pos, this.HeaderLength)))
            {
                if (requestInfo.BodyLength > length - this.HeaderLength)
                {
                    byteBlock.Pos = position;
                    return FilterResult.Ignore;
                }

                byteBlock.Pos += this.HeaderLength;
                DataResult dataResult = requestInfo.OnParsingBody(byteBlock.ToArray(byteBlock.Pos, requestInfo.BodyLength));
                switch (dataResult.ResultCode)
                {
                    case DataResultCode.Success:
                        byteBlock.Pos += requestInfo.BodyLength;
                        request = requestInfo;
                        return FilterResult.Success;

                    case DataResultCode.Ignore:
                        byteBlock.Pos += requestInfo.BodyLength;
                        return FilterResult.Ignore;

                    case DataResultCode.Error:
                    case DataResultCode.Exception:
                    default:
                        byteBlock.Pos = position;
                        this.OnReceivingError(dataResult);
                        return FilterResult.Ignore;
                }
            }
            else
            {
                return FilterResult.Ignore;
            }
        }

        /// <summary>
        /// 获取泛型实例。
        /// </summary>
        /// <returns></returns>
        protected abstract TFixedHeaderRequestInfo GetInstance();
    }
}