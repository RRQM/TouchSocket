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
using System.Threading.Tasks;

namespace TouchSocket.Core
{
    /// <summary>
    /// 用户自定义数据处理适配器，使用该适配器时，接收方收到的数据中，<see cref="ByteBlock"/>将为null，
    /// 同时<see cref="IRequestInfo"/>将实现为TRequest，发送数据直接发送。
    /// </summary>
    public abstract class CustomDataHandlingAdapter<TRequest> : SingleStreamDataHandlingAdapter where TRequest : IRequestInfo
    {

        private ValueByteBlock m_tempByteBlock;

        private readonly Type m_requestType;

        /// <summary>
        /// 初始化自定义数据处理适配器。
        /// </summary>
        /// <remarks>
        /// 该构造函数在创建<see cref="CustomDataHandlingAdapter{TRequest}"/>实例时，会指定请求类型。
        /// </remarks>
        public CustomDataHandlingAdapter()
        {
            this.m_requestType = typeof(TRequest);
        }


        private TRequest m_tempRequest;

        /// <inheritdoc/>
        public override bool CanSendRequestInfo => false;

        /// <summary>
        /// 默认不支持拼接发送
        /// </summary>
        public override bool CanSplicingSend => false;

        /// <summary>
        /// 指示需要解析当前包的剩余长度。如果不能得知，请赋值<see cref="int.MaxValue"/>。
        /// </summary>
        protected int SurLength { get; set; }

        /// <summary>
        /// 尝试解析请求数据块。
        /// </summary>
        /// <typeparam name="TByteBlock">字节块类型，必须实现IByteBlock接口。</typeparam>
        /// <param name="byteBlock">待解析的字节块。</param>
        /// <param name="request">解析出的请求对象。</param>
        /// <returns>解析是否成功。</returns>
        public bool TryParseRequest<TByteBlock>(ref TByteBlock byteBlock, out TRequest request) where TByteBlock : IByteBlock
        {
            // 检查缓存是否超时，如果超时则清除缓存。
            if (this.CacheTimeoutEnable && DateTime.UtcNow - this.LastCacheTime > this.CacheTimeout)
            {
                this.Reset();
            }

            // 如果临时字节块为空，则尝试直接解析。
            if (this.m_tempByteBlock.IsEmpty)
            {
                return this.Single(ref byteBlock, out request) == FilterResult.Success;
            }
            else
            {
                // 如果剩余长度小于等于0，则抛出异常。
                if (this.SurLength <= 0)
                {
                    throw new Exception();
                }

                // 计算本次可以读取的长度。
                var len = Math.Min(this.SurLength, byteBlock.CanReadLength);

                // 从输入字节块中读取数据到临时字节块中。
                var block = this.m_tempByteBlock;
                block.Write(byteBlock.Span.Slice(byteBlock.Position, len));
                byteBlock.Position += len;
                this.SurLength -= len;

                // 重置临时字节块并准备下一次使用。
                this.m_tempByteBlock = ValueByteBlock.Empty;

                // 回到字节块的起始位置。
                block.SeekToStart();
                try
                {
                    // 尝试解析字节块。
                    var filterResult = this.Single(ref block, out request);
                    switch (filterResult)
                    {
                        case FilterResult.Cache:
                            {
                                // 如果临时字节块不为空，则继续缓存。
                                if (!this.m_tempByteBlock.IsEmpty)
                                {
                                    byteBlock.Position += this.m_tempByteBlock.Length;
                                }
                                return false;
                            }
                        case FilterResult.Success:
                            {
                                // 如果字节块中还有剩余数据，则回退指针。
                                if (block.CanReadLength > 0)
                                {
                                    byteBlock.Position -= block.CanReadLength;
                                }
                                return true;
                            }
                        case FilterResult.GoOn:
                        default:
                            // 对于需要继续解析的情况，也回退指针。
                            if (block.CanReadLength > 0)
                            {
                                byteBlock.Position -= block.CanReadLength;
                            }
                            return false;
                    }
                }
                finally
                {
                    // 释放字节块资源。
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

        /// <inheritdoc/>
        /// <param name="byteBlock"></param>
        protected override async Task PreviewReceivedAsync(ByteBlock byteBlock)
        {
            if (this.CacheTimeoutEnable && DateTime.UtcNow - this.LastCacheTime > this.CacheTimeout)
            {
                this.Reset();
            }
            if (this.m_tempByteBlock.IsEmpty)
            {
                await this.SingleAsync(byteBlock, false).ConfigureAwait(false);
            }
            else
            {
                this.m_tempByteBlock.Write(byteBlock.Span);
                var block = this.m_tempByteBlock;
                this.m_tempByteBlock = ValueByteBlock.Empty;
                await this.SingleAsync(block, true).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        protected override void Reset()
        {
            this.m_tempByteBlock.SafeDispose();
            this.m_tempByteBlock = ValueByteBlock.Empty;
            this.m_tempRequest = default;
            this.SurLength = 0;
            base.Reset();
        }

        /// <summary>
        /// 判断请求对象是否应该被缓存。
        /// </summary>
        /// <param name="request">请求对象。</param>
        /// <returns>返回布尔值，指示请求对象是否应该被缓存。</returns>
        protected virtual bool IsBeCached(in TRequest request)
        {
            // 如果请求对象类型是值类型，则判断其哈希码是否与默认值不同；
            // 如果是引用类型，则判断对象本身是否为null。
            return this.m_requestType.IsValueType ? request.GetHashCode() != default(TRequest).GetHashCode() : request != null;
        }

        /// <summary>
        /// 处理单个字节块，提取请求对象。
        /// </summary>
        /// <typeparam name="TByteBlock">字节块类型，需要实现IByteBlock接口。</typeparam>
        /// <param name="byteBlock">字节块，将被解析以提取请求对象。</param>
        /// <param name="request">输出参数，提取出的请求对象。</param>
        /// <returns>返回过滤结果，指示处理的状态。</returns>
        protected FilterResult Single<TByteBlock>(ref TByteBlock byteBlock, out TRequest request) where TByteBlock : IByteBlock
        {
            // 初始化临时缓存容量。
            var tempCapacity = 1024 * 64;
            // 执行过滤操作，根据是否应该缓存来决定如何处理字节块和请求对象。
            var filterResult = this.Filter(ref byteBlock, this.IsBeCached(this.m_tempRequest), ref this.m_tempRequest, ref tempCapacity);
            switch (filterResult)
            {
                case FilterResult.Success:
                    // 如果过滤结果是成功，则设置请求对象并重置临时请求对象为默认值。
                    request = this.m_tempRequest;
                    this.m_tempRequest = default;
                    return filterResult;

                case FilterResult.Cache:
                    // 如果过滤结果需要缓存，则创建一个新的字节块来缓存数据。
                    if (byteBlock.CanReadLength > 0)
                    {
                        this.m_tempByteBlock = new ValueByteBlock(tempCapacity);
                        this.m_tempByteBlock.Write(byteBlock.Span.Slice(byteBlock.Position, byteBlock.CanReadLength));

                        // 如果缓存的数据长度超过设定的最大包大小，则抛出异常。
                        if (this.m_tempByteBlock.Length > this.MaxPackageSize)
                        {
                            throw new Exception("缓存的数据长度大于设定值的情况下未收到解析信号");
                        }

                        // 将字节块指针移到末尾。
                        byteBlock.SeekToEnd();
                    }
                    // 更新缓存时间。
                    if (this.UpdateCacheTimeWhenRev)
                    {
                        this.LastCacheTime = DateTime.UtcNow;
                    }
                    request = default;
                    return filterResult;

                case FilterResult.GoOn:
                default:
                    // 对于继续或默认的过滤结果，更新缓存时间。
                    if (this.UpdateCacheTimeWhenRev)
                    {
                        this.LastCacheTime = DateTime.UtcNow;
                    }
                    request = default;
                    return filterResult;
            }
        }

        private async Task SingleAsync<TByteBlock>(TByteBlock byteBlock, bool temp) where TByteBlock : IByteBlock
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
                            await this.GoReceivedAsync(null, this.m_tempRequest).ConfigureAwait(false);
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
                            this.LastCacheTime = DateTime.UtcNow;
                        }
                        return;

                    case FilterResult.GoOn:
                        if (this.UpdateCacheTimeWhenRev)
                        {
                            this.LastCacheTime = DateTime.UtcNow;
                        }
                        break;
                }
            }
        }
    }
}