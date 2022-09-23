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
using System;
using TouchSocket.Sockets;

namespace TouchSocket.Rpc.TouchRpc.Plugins
{
    /// <summary>
    /// 具有委托能力的插件
    /// </summary>
    public sealed class TouchRpcActionPlugin<TClient> : TouchRpcPluginBase where TClient : ITouchRpc
    {
        private Action<TClient, FileTransferStatusEventArgs> m_fileTransfered;
        private Action<TClient, FileOperationEventArgs> m_fileTransfering;
        private Action<TClient, MsgEventArgs> m_handshaked;
        private Action<TClient, VerifyOptionEventArgs> m_handshaking;
        private Action<TClient, ProtocolDataEventArgs> m_receivedProtocolData;
        private Action<TClient, StreamOperationEventArgs> m_streamTransfering;
        private Action<TClient, StreamStatusEventArgs> m_streamTransfered;

        /// <summary>
        /// SetFileTransfered
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public TouchRpcActionPlugin<TClient> SetFileTransfered(Action<TClient, FileTransferStatusEventArgs> action)
        {
            this.m_fileTransfered = action;
            return this;
        }

        /// <summary>
        /// SetFileTransfering
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public TouchRpcActionPlugin<TClient> SetFileTransfering(Action<TClient, FileOperationEventArgs> action)
        {
            this.m_fileTransfering = action;
            return this;
        }

        /// <summary>
        /// SetHandshaked
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public TouchRpcActionPlugin<TClient> SetHandshaked(Action<TClient, MsgEventArgs> action)
        {
            this.m_handshaked = action;
            return this;
        }

        /// <summary>
        /// SetHandshaking
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public TouchRpcActionPlugin<TClient> SetHandshaking(Action<TClient, VerifyOptionEventArgs> action)
        {
            this.m_handshaking = action;
            return this;
        }

        /// <summary>
        /// SetReceivedProtocolData
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public TouchRpcActionPlugin<TClient> SetReceivedProtocolData(Action<TClient, ProtocolDataEventArgs> action)
        {
            this.m_receivedProtocolData = action;
            return this;
        }

        /// <summary>
        /// SetStreamTransfering
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public TouchRpcActionPlugin<TClient> SetStreamTransfering(Action<TClient, StreamOperationEventArgs> action)
        {
            this.m_streamTransfering = action;
            return this;
        }

        /// <summary>
        /// SetStreamTransfered
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public TouchRpcActionPlugin<TClient> SetStreamTransfered(Action<TClient, StreamStatusEventArgs> action)
        {
            this.m_streamTransfered = action;
            return this;
        }

        #region 重写
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected override void OnFileTransfered(ITouchRpc client, FileTransferStatusEventArgs e)
        {
            this.m_fileTransfered?.Invoke((TClient)client, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected override void OnFileTransfering(ITouchRpc client, FileOperationEventArgs e)
        {
            this.m_fileTransfering?.Invoke((TClient)client, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected override void OnHandshaked(ITouchRpc client, MsgEventArgs e)
        {
            this.m_handshaked?.Invoke((TClient)client, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected override void OnHandshaking(ITouchRpc client, VerifyOptionEventArgs e)
        {
            this.m_handshaking?.Invoke((TClient)client, e);
        }


        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected override void OnReceivedProtocolData(ITouchRpc client, ProtocolDataEventArgs e)
        {
            this.m_receivedProtocolData?.Invoke((TClient)client, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected override void OnStreamTransfering(ITouchRpc client, StreamOperationEventArgs e)
        {
            this.m_streamTransfering?.Invoke((TClient)client, e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        protected override void OnStreamTransfered(ITouchRpc client, StreamStatusEventArgs e)
        {
            this.m_streamTransfered?.Invoke((TClient)client, e);
        }
        #endregion

    }
}
