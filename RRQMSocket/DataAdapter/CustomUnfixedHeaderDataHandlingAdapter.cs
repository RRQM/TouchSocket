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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            int position = byteBlock.Pos;//先记录初始流位置，防止不能解析时，可以重置流位置。
            if (beCached)
            {
                if (request.BodyLength > byteBlock.CanReadLen)
                {
                    return FilterResult.GoOn;
                }
                else
                {
                    DataResult dataResult = request.OnParsingBody(byteBlock.ToArray(byteBlock.Pos, request.BodyLength));
                    switch (dataResult.ResultCode)
                    {
                        case DataResultCode.Success:
                            byteBlock.Pos += request.BodyLength;
                            return FilterResult.Success;
                        case DataResultCode.Cache:
                            byteBlock.Pos += request.BodyLength;
                            return FilterResult.Cache;
                        case DataResultCode.Error:
                        case DataResultCode.Exception:
                        default:
                            byteBlock.Pos = position;
                            this.OnReceivingError(dataResult);
                            return FilterResult.Cache;
                    }
                }
            }
            else
            {
                TUnfixedHeaderRequestInfo requestInfo = this.GetInstance();
                if (requestInfo.OnParsingHeader(byteBlock, length))
                {
                    if (requestInfo.BodyLength > 0)
                    {
                        if (requestInfo.BodyLength > byteBlock.CanReadLen)
                        {
                            request = requestInfo;
                            return FilterResult.GoOn;
                        }
                        else
                        {
                            DataResult dataResult = requestInfo.OnParsingBody(byteBlock.ToArray(byteBlock.Pos, requestInfo.BodyLength));
                            switch (dataResult.ResultCode)
                            {
                                case DataResultCode.Success:
                                    byteBlock.Pos += requestInfo.BodyLength;
                                    request = requestInfo;
                                    return FilterResult.Success;

                                case DataResultCode.Cache:
                                    byteBlock.Pos += requestInfo.BodyLength;
                                    return FilterResult.Cache;

                                case DataResultCode.Error:
                                case DataResultCode.Exception:
                                default:
                                    byteBlock.Pos = position;
                                    this.OnReceivingError(dataResult);
                                    return FilterResult.Cache;
                            }
                        }
                    }
                    else
                    {
                        request = requestInfo;
                        return FilterResult.Success;
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
