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

namespace RRQMSocket
{
    /// <summary>
    /// 协议客户端
    /// </summary>
    public class SimpleProtocolClient : ProtocolClient
    {
        /// <summary>
        /// 预处理流
        /// </summary>
        public event RRQMStreamOperationEventHandler<SimpleProtocolClient> BeforeReceiveStream;

        /// <summary>
        /// 接收到数据
        /// </summary>
        public event RRQMProtocolReceivedEventHandler<SimpleProtocolClient> Received;

        /// <summary>
        /// 收到流数据
        /// </summary>
        public event RRQMStreamStatusEventHandler<SimpleProtocolClient> ReceivedStream;

        /// <summary>
        /// 处理协议数据
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="byteBlock"></param>
        protected  override void HandleProtocolData(short? procotol, ByteBlock byteBlock)
        {
            this.Received.Invoke(this,procotol, byteBlock);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="args"></param>
        protected override void HandleStream(StreamStatusEventArgs args)
        {
            this.ReceivedStream?.Invoke(this, args);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="args"></param>
        protected override void PreviewHandleStream(StreamOperationEventArgs args)
        {
            this.BeforeReceiveStream?.Invoke(this, args);
        }
    }
}