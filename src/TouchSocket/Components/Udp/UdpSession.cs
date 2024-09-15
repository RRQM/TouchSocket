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
    /// UdpSession 类，继承自 UdpSessionBase 并实现 IUdpSession 接口。
    /// 这个类提供了与 UDP 会话相关的操作和属性，是 UDP 会话管理的核心组件。
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
                await this.Received.Invoke(this, e).ConfigureAwait(false);
                if (e.Handled)
                {
                    return;
                }
            }

            await base.OnUdpReceived(e).ConfigureAwait(false);
        }

        #region 向默认远程异步发送

        /// <inheritdoc/>
        public virtual Task SendAsync(ReadOnlyMemory<byte> memory)
        {
            return this.ProtectedSendAsync(memory);
        }

        /// <inheritdoc/>
        public virtual Task SendAsync(IRequestInfo requestInfo)
        {
            return this.ProtectedSendAsync(requestInfo);
        }

        #endregion 向默认远程异步发送

        #region 向设置的远程异步发送

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public Task SendAsync(IList<ArraySegment<byte>> transferBytes)
        {
            return this.ProtectedSendAsync(transferBytes);
        }

        /// <inheritdoc/>
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