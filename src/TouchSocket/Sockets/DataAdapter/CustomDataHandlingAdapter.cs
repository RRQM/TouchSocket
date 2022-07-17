//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System.Collections.Generic;
using TouchSocket.Core.ByteManager;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// 用户自定义数据处理适配器，使用该适配器时，接收方收到的数据中，<see cref="ByteBlock"/>将为null，
    /// 同时<see cref="IRequestInfo"/>将实现为TRequest，发送数据直接发送。
    /// <para>此处设计思路借鉴SuperSocket。</para>
    /// </summary>
    public abstract class CustomDataHandlingAdapter<TRequest> : DataHandlingAdapter where TRequest : class, IRequestInfo
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
        /// <param name="beCached">是否为上次遗留对象，当该参数为True时，request也将是上次实例化的对象。</param>
        /// <param name="request">对象。</param>
        /// <param name="tempCapacity">缓存容量。当需要首次缓存时，指示申请的ByteBlock的容量。合理的值可避免ByteBlock扩容带来的性能消耗。</param>
        /// <returns></returns>
        protected abstract FilterResult Filter(ByteBlock byteBlock, bool beCached, ref TRequest request, ref int tempCapacity);

        private int a;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        protected override void PreviewReceived(ByteBlock byteBlock)
        {
            this.a += byteBlock.Len;
            if (this.tempByteBlock == null)
            {
                this.Single(byteBlock, false);
            }
            else
            {
                this.tempByteBlock.Write(byteBlock.Buffer, 0, byteBlock.Len);
                ByteBlock block = this.tempByteBlock;
                this.tempByteBlock = null;
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

        private void Single(ByteBlock byteBlock, bool temp)
        {
            int position = byteBlock.Pos;
            byteBlock.Pos = 0;
            bool neverSucceed = true;
            while (byteBlock.Pos < byteBlock.Len)
            {
                int tempCapacity = 1024 * 64;
                FilterResult filterResult = this.Filter(byteBlock, this.tempRequest != null, ref this.tempRequest, ref tempCapacity);
                switch (filterResult)
                {
                    case FilterResult.Success:
                        this.GoReceived(null, this.tempRequest);
                        this.OnReceivedSuccess(this.tempRequest);
                        this.tempRequest = default;
                        neverSucceed = false;
                        break;

                    case FilterResult.Cache:
                        if (byteBlock.CanReadLen > 0)
                        {
                            if (temp)
                            {
                                if (neverSucceed)
                                {
                                    byteBlock.Pos = position;
                                    this.tempByteBlock = byteBlock;
                                }
                                else
                                {
                                    this.tempByteBlock = new ByteBlock(tempCapacity);
                                    this.tempByteBlock.Write(byteBlock.Buffer, byteBlock.Pos, byteBlock.CanReadLen);
                                    byteBlock.Dispose();
                                }
                            }
                            else
                            {
                                this.tempByteBlock = new ByteBlock(tempCapacity);
                                this.tempByteBlock.Write(byteBlock.Buffer, byteBlock.Pos, byteBlock.CanReadLen);
                            }

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