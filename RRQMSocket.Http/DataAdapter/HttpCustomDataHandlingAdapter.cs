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
using RRQMCore.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace RRQMSocket.Http
{
    /// <summary>
    /// Http数据处理适配器
    /// </summary>
    public class HttpCustomDataHandlingAdapter : CustomUnfixedHeaderDataHandlingAdapter<HttpBase>
    {
        private HttpType httpType;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="maxSize"></param>
        /// <param name="httpType"></param>
        public HttpCustomDataHandlingAdapter(int maxSize, HttpType httpType)
        {
            this.MaxSize = maxSize;
            this.httpType = httpType;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanSplicingSend => false;

        /// <summary>
        /// 允许的最大长度
        /// </summary>
        public override int MaxSize { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="length"></param>
        /// <param name="beCached"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        protected override FilterResult Filter(ByteBlock byteBlock, int length, bool beCached, ref HttpBase request)
        {
            int position = byteBlock.Pos;
            HttpBase requestInfo = this.GetInstance();
           var result= requestInfo.OnParsingHeader(byteBlock, length);
            if (result== FilterResult.Success)
            {
                if (requestInfo.BodyLength > byteBlock.CanReadLen)
                {
                    byteBlock.Pos = position;
                    return FilterResult.Cache;
                }
                byteBlock.Pos++;
                DataResult dataResult = requestInfo.OnParsingBody(byteBlock.ToArray(byteBlock.Pos, requestInfo.BodyLength));
                switch (dataResult.ResultCode)
                {
                    case DataResultCode.Success:
                        byteBlock.Pos += requestInfo.BodyLength;
                        request = requestInfo;
                        request.ReadFromBase();
                        return FilterResult.Success;

                    case DataResultCode.Cache:
                        byteBlock.Pos += requestInfo.BodyLength;
                        return FilterResult.Cache;

                    case DataResultCode.Error:
                    case DataResultCode.Exception:
                    default:
                        this.OnReceivingError(dataResult);
                        return FilterResult.Cache;
                }
            }
            else
            {
                return result;
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="dataResult"></param>
        /// <returns></returns>
        protected override bool OnReceivingError(DataResult dataResult)
        {
            this.Owner.Logger.Debug(RRQMCore.Log.LogType.Error, this, dataResult.Message, null);
            return true;
        }

        /// <summary>
        /// 获取实例
        /// </summary>
        /// <returns></returns>
        protected override HttpBase GetInstance()
        {
            switch (this.httpType)
            {
                default:
                case HttpType.Server:
                    return new HttpRequest();

                case HttpType.Client:
                    return new HttpResponse();
            }
        }
    }
}