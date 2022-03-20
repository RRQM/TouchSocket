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

namespace RRQMSocket
{
    /// <summary>
    /// SimpleProtocolSocketClient
    /// </summary>
    public class SimpleProtocolSocketClient : ProtocolSocketClient
    {
        /// <summary>
        /// 收到消息
        /// </summary>
        public event RRQMProtocolReceivedEventHandler<SimpleProtocolSocketClient> Received;

        /// <summary>
        /// 预处理流
        /// </summary>
        public event RRQMStreamOperationEventHandler<SimpleProtocolSocketClient> BeforeReceiveStream;

        /// <summary>
        /// 收到流数据
        /// </summary>
        public event RRQMStreamStatusEventHandler<SimpleProtocolSocketClient> ReceivedStream;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void OnBreakOut()
        {
            this.Received = null;
            this.BeforeReceiveStream = null;
            this.ReceivedStream = null;
            base.OnBreakOut();
        }

        /// <summary>
        /// 处理协议数据
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="byteBlock"></param>
        protected override void HandleProtocolData(short protocol, ByteBlock byteBlock)
        {
            this.Received?.Invoke(this, protocol, byteBlock);
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