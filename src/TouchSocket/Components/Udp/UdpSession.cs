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
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// UDP基类服务器。
    /// </summary>
    public class UdpSession : UdpSessionBase, IUdpSession
    {
        /// <inheritdoc/>
        public UdpDataHandlingAdapter DataHandlingAdapter => this.ProtectedDataHandlingAdapter;

        /// <inheritdoc/>
        public UdpReceivedEventHandler<IUdpSession> Received { get; set; }

        /// <inheritdoc/>
        protected override async Task OnUdpReceived(UdpReceivedDataEventArgs e)
        {
            if (this.Received != null)
            {
                await this.Received.Invoke(this, e).ConfigureFalseAwait();
                if (e.Handled)
                {
                    return;
                }
            }
            await this.PluginManager.RaiseAsync(typeof(IUdpReceivedPlugin), this, e).ConfigureFalseAwait();
        }

        #region 向默认远程同步发送

        ///// <summary>
        ///// 向默认终结点发送
        ///// </summary>
        ///// <param name="buffer"></param>
        ///// <param name="offset"></param>
        ///// <param name="length"></param>
        //public virtual void 123Send(byte[] buffer, int offset, int length)
        //{
        //    this.ProtectedSend(buffer, offset, length);
        //}

        ///// <summary>
        ///// 向默认终结点发送
        ///// </summary>
        ///// <param name="requestInfo"></param>
        ///// <exception cref="OverlengthException"></exception>
        ///// <exception cref="Exception"></exception>
        //public virtual void 123Send(IRequestInfo requestInfo)
        //{
        //    this.ProtectedSend(requestInfo);
        //}

        #endregion 向默认远程同步发送

        #region 向默认远程异步发送

        /// <inheritdoc/>
        public virtual Task SendAsync(ReadOnlyMemory<byte> memory)
        {
            return this.ProtectedSendAsync(memory);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="requestInfo"></param>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public virtual Task SendAsync(IRequestInfo requestInfo)
        {
            return this.ProtectedSendAsync(requestInfo);
        }

        #endregion 向默认远程异步发送

        #region 向设置的远程同步发送

        ///// <summary>
        ///// 向设置的远程同步发送
        ///// </summary>
        ///// <param name="remoteEP"></param>
        ///// <param name="buffer"></param>
        ///// <param name="offset"></param>
        ///// <param name="length"></param>
        ///// <exception cref="ClientNotConnectedException"></exception>
        ///// <exception cref="OverlengthException"></exception>
        ///// <exception cref="Exception"></exception>
        //public virtual void Send(EndPoint remoteEP, byte[] buffer, int offset, int length)
        //{
        //    this.ProtectedSend(remoteEP, buffer, offset, length);
        //}

        ///// <summary>
        ///// 向设置终结点发送
        ///// </summary>
        ///// <param name="endPoint"></param>
        ///// <param name="requestInfo"></param>
        ///// <exception cref="OverlengthException"></exception>
        ///// <exception cref="Exception"></exception>
        //public virtual void Send(EndPoint endPoint, IRequestInfo requestInfo)
        //{
        //    this.ProtectedSend(endPoint, requestInfo);
        //}

        #endregion 向设置的远程同步发送

        #region 向设置的远程异步发送

        /// <summary>
        /// 向设置的远程异步发送
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <exception cref="ClientNotConnectedException"></exception>
        /// <exception cref="OverlengthException"></exception>
        /// <exception cref="Exception"></exception>
        public virtual Task SendAsync(EndPoint endPoint, ReadOnlyMemory<byte> memory)
        {
            return this.ProtectedSendAsync(endPoint, memory);
        }

        /// <inheritdoc/>
        public virtual Task SendAsync(EndPoint endPoint, IRequestInfo requestInfo)
        {
            return this.ProtectedSendAsync(endPoint, requestInfo);
        }

        #endregion 向设置的远程异步发送

        #region 组合发送

        ///// <summary>
        ///// <inheritdoc/>
        ///// </summary>
        ///// <param name="transferBytes"></param>
        //public void 123Send(IList<ArraySegment<byte>> transferBytes)
        //{
        //    this.ProtectedSend(transferBytes);
        //}

        ///// <summary>
        ///// <inheritdoc/>
        ///// </summary>
        ///// <param name="endPoint"></param>
        ///// <param name="transferBytes"></param>
        //public void Send(EndPoint endPoint, IList<ArraySegment<byte>> transferBytes)
        //{
        //    this.ProtectedSend(endPoint, transferBytes);
        //}

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"></param>
        public Task SendAsync(IList<ArraySegment<byte>> transferBytes)
        {
            return this.ProtectedSendAsync(transferBytes);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="endPoint"></param>
        /// <param name="transferBytes"></param>
        public Task SendAsync(EndPoint endPoint, IList<ArraySegment<byte>> transferBytes)
        {
            return this.ProtectedSendAsync(endPoint, transferBytes);
        }

        #endregion 组合发送

        #region Receiver

        /// <inheritdoc/>
        public void ClearReceiver()
        {
            this.ProtectedClearReceiver();
        }

        /// <inheritdoc/>
        public IReceiver<IUdpReceiverResult> CreateReceiver()
        {
            return this.ProtectedCreateReceiver(this);
        }

        #endregion Receiver
    }
}