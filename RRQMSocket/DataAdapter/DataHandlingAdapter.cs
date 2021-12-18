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

namespace RRQMSocket
{
    /// <summary>
    /// 数据处理适配器
    /// </summary>
    public abstract class DataHandlingAdapter
    {
        /// <summary>
        /// 拼接发送
        /// </summary>
        public abstract bool CanSplicingSend { get; }

        internal ITcpClientBase owner;

        /// <summary>
        /// 适配器拥有者。
        /// </summary>
        public ITcpClientBase Owner
        {
            get { return owner; }
        }

        /// <summary>
        /// 当接收数据处理完成后，回调该函数执行接收
        /// </summary>
        internal Action<ByteBlock, object> ReceivedCallBack { get; set; }

        /// <summary>
        /// 当接收数据处理完成后，回调该函数执行发送
        /// </summary>
        internal Action<byte[], int, int, bool> SendCallBack { get; set; }

        /// <summary>
        /// 当接收到数据后预先处理数据,然后调用<see cref="GoReceived(ByteBlock,object)"/>处理数据
        /// </summary>
        /// <param name="byteBlock">数据流</param>
        protected abstract void PreviewReceived(ByteBlock byteBlock);

        /// <summary>
        /// 处理已经经过预先处理后的数据
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="obj"></param>
        protected void GoReceived(ByteBlock byteBlock, object obj)
        {
            this.ReceivedCallBack.Invoke(byteBlock, obj);
        }

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

        internal void Received(ByteBlock byteBlock)
        {
            this.PreviewReceived(byteBlock);
        }

        internal void Send(byte[] buffer, int offset, int length, bool isAsync)
        {
            this.PreviewSend(buffer, offset, length, isAsync);
        }

        internal void Send(IList<TransferByte> transferBytes, bool isAsync)
        {
            this.PreviewSend(transferBytes, isAsync);
        }
    }
}