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
using System;
using System.Collections.Generic;

namespace RRQMSocket
{
    /// <summary>
    /// 数据处理适配器
    /// </summary>
    public abstract class DataHandlingAdapter
    {
        internal ITcpClientBase owner;

        /// <summary>
        /// 当接收数据处理完成后，回调该函数执行接收
        /// </summary>
        internal Action<ByteBlock, IRequestInfo> ReceivedCallBack;

        /// <summary>
        /// 当接收数据处理完成后，回调该函数执行发送
        /// </summary>
        internal Action<byte[], int, int, bool> SendCallBack;

        /// <summary>
        /// 拼接发送
        /// </summary>
        public abstract bool CanSplicingSend { get; }

        /// <summary>
        /// 适配器拥有者。
        /// </summary>
        public ITcpClientBase Owner => this.owner;

        internal void Received(ByteBlock byteBlock)
        {
            try
            {
                this.PreviewReceived(byteBlock);
            }
            catch (Exception ex)
            {
                this.OnReceivingError(new DataResult(ex.Message, DataResultCode.Exception));
            }
        }

        internal void Send(byte[] buffer, int offset, int length, bool isAsync)
        {
            this.PreviewSend(buffer, offset, length, isAsync);
        }

        internal void Send(IList<TransferByte> transferBytes, bool isAsync)
        {
            this.PreviewSend(transferBytes, isAsync);
        }

        /// <summary>
        /// 处理已经经过预先处理后的数据
        /// </summary>
        /// <param name="byteBlock">以二进制形式传递</param>
        /// <param name="requestInfo">以解析实例传递</param>
        protected void GoReceived(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            this.ReceivedCallBack.Invoke(byteBlock, requestInfo);
        }

        /// <summary>
        /// 发送已经经过预先处理后的数据
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="isAsync">是否使用IOCP发送</param>
        protected void GoSend(byte[] buffer, int offset, int length, bool isAsync)
        {
            this.SendCallBack.Invoke(buffer, offset, length, isAsync);
        }

        /// <summary>
        /// 在接收解析时发生错误。
        /// </summary>
        /// <param name="dataResult">错误异常</param>
        /// <returns>返回值指示，是否调用<see cref="Reset"/></returns>
        protected abstract bool OnReceivingError(DataResult dataResult);

        /// <summary>
        /// 当接收到数据后预先处理数据,然后调用<see cref="GoReceived(ByteBlock, IRequestInfo)"/>处理数据
        /// </summary>
        /// <param name="byteBlock"></param>
        protected abstract void PreviewReceived(ByteBlock byteBlock);

        /// <summary>
        /// 当发送数据前预先处理数据
        /// </summary>
        /// <param name="buffer">数据</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <param name="isAsync">是否使用IOCP发送</param>
        protected abstract void PreviewSend(byte[] buffer, int offset, int length, bool isAsync);

        /// <summary>
        /// 组合发送预处理数据，
        /// 当属性SplicingSend实现为True时，系统才会调用该方法。
        /// </summary>
        /// <param name="transferBytes">代发送数据组合</param>
        /// <param name="isAsync">是否使用IOCP发送</param>
        protected abstract void PreviewSend(IList<TransferByte> transferBytes, bool isAsync);

        /// <summary>
        /// 重置解析器到初始状态，一般在<see cref="OnReceivingError(DataResult)"/>被触发时，由返回值指示是否调用。
        /// </summary>
        protected abstract void Reset();
    }
}