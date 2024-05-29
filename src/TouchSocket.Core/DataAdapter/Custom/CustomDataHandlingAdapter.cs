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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 用户自定义数据处理适配器，使用该适配器时，接收方收到的数据中，<see cref="ByteBlock"/>将为null，
    /// 同时<see cref="IRequestInfo"/>将实现为TRequest，发送数据直接发送。
    /// </summary>
    public abstract class CustomDataHandlingAdapter<TRequest> : SingleStreamDataHandlingAdapter where TRequest :IRequestInfo
    {
       
        private ValueByteBlock m_tempByteBlock;

        private readonly Type m_requestType;

        public CustomDataHandlingAdapter()
        {
            this.m_requestType = typeof(TRequest);
        }


        private TRequest m_tempRequest;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool CanSendRequestInfo => false;

        /// <summary>
        /// 默认不支持拼接发送
        /// </summary>
        public override bool CanSplicingSend => false;

        protected int SurLength { get; set; }

        public bool TryParseRequest<TByteBlock>(ref TByteBlock byteBlock, out TRequest request) where TByteBlock : IByteBlock
        {
            if (this.CacheTimeoutEnable && DateTime.Now - this.LastCacheTime > this.CacheTimeout)
            {
                this.Reset();
            }

            if (this.m_tempByteBlock.IsEmpty)
            {
                return this.Single(ref byteBlock, out request);
            }
            else
            {
                if (this.SurLength <= 0)
                {
                    throw new Exception();
                }

                var len = Math.Min(this.SurLength, byteBlock.CanReadLength);

                this.m_tempByteBlock.Write(byteBlock.Span.Slice(byteBlock.Position, len));
                byteBlock.Position += len;
                this.SurLength -= len;
                var block = this.m_tempByteBlock;
                this.m_tempByteBlock = ValueByteBlock.Empty;
                try
                {
                    block.SeekToStart();
                    var success = this.Single(ref block, out request);
                    if (!success&& (!this.m_tempByteBlock.IsEmpty))
                    {
                        byteBlock.Position += this.m_tempByteBlock.Length;
                    }
                    return success;
                }
                finally
                {
                    block.Dispose();
                }
            }

        }

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
        protected abstract FilterResult Filter<TByteBlock>(ref TByteBlock byteBlock, bool beCached, ref TRequest request, ref int tempCapacity)
            where TByteBlock : IByteBlock;

        /// <summary>
        /// 成功执行接收以后。
        /// </summary>
        /// <param name="request"></param>
        protected virtual void OnReceivedSuccess(TRequest request)
        {
        }

        /// <summary>
        /// 即将执行<see cref="SingleStreamDataHandlingAdapter.GoReceivedAsync(ByteBlock, IRequestInfo)"/>。
        /// </summary>
        /// <param name="request"></param>
        /// <returns>返回值标识是否继续执行</returns>
        protected virtual bool OnReceivingSuccess(TRequest request)
        {
            return true;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        protected override async Task PreviewReceivedAsync(ByteBlock byteBlock)
        {
            if (this.CacheTimeoutEnable && DateTime.Now - this.LastCacheTime > this.CacheTimeout)
            {
                this.Reset();
            }
            if (this.m_tempByteBlock.IsEmpty)
            {
                await this.Single(byteBlock, false).ConfigureFalseAwait();
            }
            else
            {
                this.m_tempByteBlock.Write(byteBlock.Span);
                var block = this.m_tempByteBlock;
                this.m_tempByteBlock = ValueByteBlock.Empty;
                await this.Single(block, true).ConfigureFalseAwait();
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Reset()
        {
            this.m_tempByteBlock.SafeDispose();
            this.m_tempByteBlock = ValueByteBlock.Empty;
            this.m_tempRequest = default;
            this.SurLength = 0;
            base.Reset();
        }

        protected virtual bool IsBeCached(in TRequest request)
        {
            if (this.m_requestType.IsValueType)
            {
                return request.GetHashCode()!=default(TRequest).GetHashCode();
            }
            return request != null;
        }

        private bool Single<TByteBlock>(ref TByteBlock byteBlock, out TRequest request) where TByteBlock : IByteBlock
        {
            if (this.m_tempRequest is null)
            {

            }
            var tempCapacity = 1024 * 64;
            var filterResult = this.Filter(ref byteBlock,  this.IsBeCached(this.m_tempRequest), ref this.m_tempRequest, ref tempCapacity);
            switch (filterResult)
            {
                case FilterResult.Success:

                    request = this.m_tempRequest;
                    this.m_tempRequest = default;
                    return true;

                case FilterResult.Cache:
                    if (byteBlock.CanReadLength > 0)
                    {
                        this.m_tempByteBlock = new ValueByteBlock(tempCapacity);
                        this.m_tempByteBlock.Write(byteBlock.Span.Slice(byteBlock.Position, byteBlock.CanReadLength));

                        if (this.m_tempByteBlock.Length > this.MaxPackageSize)
                        {
                            throw new Exception("缓存的数据长度大于设定值的情况下未收到解析信号");
                        }

                        byteBlock.SeekToEnd();
                    }
                    if (this.UpdateCacheTimeWhenRev)
                    {
                        this.LastCacheTime = DateTime.Now;
                    }
                    request = default;
                    return false;

                case FilterResult.GoOn:
                default:
                    if (this.UpdateCacheTimeWhenRev)
                    {
                        this.LastCacheTime = DateTime.Now;
                    }
                    request = default;
                    return false;
            }
        }

        private async Task Single<TByteBlock>(TByteBlock byteBlock, bool temp)where TByteBlock:IByteBlock
        {
            byteBlock.Position = 0;
            while (byteBlock.Position < byteBlock.Length)
            {
                if (this.DisposedValue)
                {
                    return;
                }
                var tempCapacity = 1024 * 64;
                var filterResult = this.Filter(ref byteBlock, this.IsBeCached(this.m_tempRequest), ref this.m_tempRequest, ref tempCapacity);

                switch (filterResult)
                {
                    case FilterResult.Success:
                        if (this.OnReceivingSuccess(this.m_tempRequest))
                        {
                            await this.GoReceivedAsync(null, this.m_tempRequest).ConfigureFalseAwait();
                            this.OnReceivedSuccess(this.m_tempRequest);
                        }
                        this.m_tempRequest = default;
                        break;

                    case FilterResult.Cache:
                        if (byteBlock.CanReadLength > 0)
                        {
                            if (temp)
                            {
                                this.m_tempByteBlock = new ValueByteBlock(tempCapacity);
                                this.m_tempByteBlock.Write(byteBlock.Span.Slice(byteBlock.Position, byteBlock.CanReadLength));
                                byteBlock.Dispose();
                            }
                            else
                            {
                                this.m_tempByteBlock = new ValueByteBlock(tempCapacity);
                                this.m_tempByteBlock.Write(byteBlock.Span.Slice(byteBlock.Position, byteBlock.CanReadLength));
                            }

                            if (this.m_tempByteBlock.Length > this.MaxPackageSize)
                            {
                                this.OnError(default, "缓存的数据长度大于设定值的情况下未收到解析信号", true, true);
                            }
                        }
                        if (this.UpdateCacheTimeWhenRev)
                        {
                            this.LastCacheTime = DateTime.Now;
                        }
                        return;

                    case FilterResult.GoOn:
                        if (this.UpdateCacheTimeWhenRev)
                        {
                            this.LastCacheTime = DateTime.Now;
                        }
                        break;
                }
            }
        }
    }
}