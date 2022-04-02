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
using System.Collections.Generic;

namespace RRQMSocket
{
    /// <summary>
    /// 用户自定义数据处理适配器，使用该适配器时，接收方收到的数据中，<see cref="ByteBlock"/>将为null，
    /// 同时<see cref="IRequestInfo"/>将实现为TRequest，发送数据直接发送。
    /// <para>此处设计思路借鉴SuperSocket。</para>
    /// </summary>
    public abstract class CustomDataHandlingAdapter<TRequest> : DataHandlingAdapter where TRequest : IRequestInfo
    {
        /// <summary>
        /// 缓存数据，如果需要手动释放，请先判断，然后到调用<see cref="ByteBlock.Dispose"/>后，再置空；
        /// </summary>
        protected ByteBlock tempByteBlock;

        /// <summary>
        /// 默认不支持拼接发送
        /// </summary>
        public override bool CanSplicingSend => false;

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
        protected abstract FilterResult Filter(ByteBlock byteBlock, int length, bool beCached, ref TRequest request);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        protected override void PreviewReceived(ByteBlock byteBlock)
        {
            if (this.tempByteBlock == null)
            {
                byteBlock.Pos = 0;
                this.Single(byteBlock, false);
            }
            else
            {
                this.tempByteBlock.Write(byteBlock.Buffer, 0, byteBlock.Len);
                ByteBlock block = this.tempByteBlock;
                this.tempByteBlock = null;
                block.Pos = 0;
                this.Single(block, true);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer">数据</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <param name="isAsync"></param>
        protected override void PreviewSend(byte[] buffer, int offset, int length, bool isAsync)
        {
            this.GoSend(buffer, offset, length, isAsync);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"></param>
        /// <param name="isAsync"></param>
        protected override void PreviewSend(IList<TransferByte> transferBytes, bool isAsync)
        {
            throw new System.NotImplementedException();//因为设置了不支持拼接发送，所以该方法可以不实现。
        }

        /// <summary>
        /// 缓存对象。
        /// </summary>
        protected TRequest tempRequest;

        /// <summary>
        /// 成功执行接收以后。
        /// </summary>
        /// <param name="request"></param>
        protected virtual void OnReceivedSuccess(TRequest request)
        {
        }

        private void Single(ByteBlock byteBlock, bool dis)
        {
            try
            {
                while (byteBlock.Pos < byteBlock.Len)
                {
                    FilterResult filterResult = this.Filter(byteBlock, byteBlock.Len - byteBlock.Pos, (IRequestInfo)this.tempRequest == default ? false : true, ref this.tempRequest);
                    switch (filterResult)
                    {
                        case FilterResult.Success:
                            this.GoReceived(null, this.tempRequest);
                            this.OnReceivedSuccess(this.tempRequest);
                            this.tempRequest = default;
                            break;

                        case FilterResult.Cache:
                            if (byteBlock.CanReadLen> 0)
                            {
                                this.tempByteBlock = new ByteBlock();
                                this.tempByteBlock.Write(byteBlock.Buffer, byteBlock.Pos, byteBlock.CanReadLen);
                                if (this.tempByteBlock.Len > this.MaxPackageSize)
                                {
                                    this.OnError("缓存的数据长度大于设定值的情况下未收到解析信号");
                                }
                            }
                            return;

                        case FilterResult.GoOn:
                            break;
                    }
                }
            }
            finally
            {
                if (dis)
                {
                    byteBlock.Dispose();
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Reset()
        {
            if (this.tempByteBlock != null)
            {
                this.tempByteBlock.Dispose();
                this.tempByteBlock = null;
            }
            this.tempRequest = default;
        }
    }
}