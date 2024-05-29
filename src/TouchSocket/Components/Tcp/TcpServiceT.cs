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
using TouchSocket.Core;

namespace TouchSocket.Sockets
{
    /// <summary>
    /// Tcp泛型服务器，由使用者自己指定<see cref="TcpSessionClient"/>类型。
    /// </summary>
    public abstract class TcpService<TClient> : TcpServiceBase<TClient>, ITcpService<TClient> where TClient : TcpSessionClient
    {
        /// <inheritdoc/>
        protected override void ClientInitialized(TClient client)
        {
            base.ClientInitialized(client);
            client.SetAction(this.OnClientConnecting, this.OnClientConnected, this.OnClientClosing, this.OnClientDisconnected, this.OnClientReceivedData);
        }

        #region 事件

        /// <summary>
        /// 有用户断开连接
        /// </summary>
        public ClosedEventHandler<TClient> Closed { get; set; }

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// </summary>
        public ClosingEventHandler<TClient> Closing { get; set; }

        /// <summary>
        /// 用户连接完成
        /// </summary>
        public ConnectedEventHandler<TClient> Connected { get; set; }

        /// <summary>
        /// 有用户连接的时候
        /// </summary>
        public ConnectingEventHandler<TClient> Connecting { get; set; }

        /// <summary>
        /// 处理数据
        /// </summary>
        public ReceivedEventHandler<TClient> Received { get; set; }

        /// <summary>
        /// 客户端断开连接，覆盖父类方法将不会触发事件。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected virtual async Task OnTcpClosed(TClient socketClient, ClosedEventArgs e)
        {
            if (this.Closed != null)
            {
                await this.Closed.Invoke(socketClient, e).ConfigureFalseAwait();
            }
        }

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected virtual async Task OnTcpClosing(TClient socketClient, ClosingEventArgs e)
        {
            if (this.Closing != null)
            {
                await this.Closing.Invoke(socketClient, e).ConfigureFalseAwait();
            }
        }

        /// <summary>
        /// 客户端连接完成，覆盖父类方法将不会触发事件。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected virtual async Task OnTcpConnected(TClient socketClient, ConnectedEventArgs e)
        {
            if (this.Connected != null)
            {
                await this.Connected.Invoke(socketClient, e).ConfigureFalseAwait();
            }
        }

        /// <summary>
        /// 客户端请求连接，覆盖父类方法将不会触发事件。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected virtual async Task OnTcpConnecting(TClient socketClient, ConnectingEventArgs e)
        {
            if (this.Connecting != null)
            {
                await this.Connecting.Invoke(socketClient, e).ConfigureFalseAwait();
            }
        }

        /// <inheritdoc/>
        protected virtual async Task OnTcpReceived(TClient socketClient, ReceivedDataEventArgs e)
        {
            if (this.Received != null)
            {
                await this.Received.Invoke(socketClient, e).ConfigureFalseAwait();
            }
        }

        private Task OnClientClosing(TcpSessionClient socketClient, ClosingEventArgs e)
        {
            return this.OnTcpClosing((TClient)socketClient, e);
        }

        private Task OnClientConnected(TcpSessionClient socketClient, ConnectedEventArgs e)
        {
            return this.OnTcpConnected((TClient)socketClient, e);
        }

        private Task OnClientConnecting(TcpSessionClient socketClient, ConnectingEventArgs e)
        {
            return this.OnTcpConnecting((TClient)socketClient, e);
        }

        private Task OnClientDisconnected(TcpSessionClient socketClient, ClosedEventArgs e)
        {
            return this.OnTcpClosed((TClient)socketClient, e);
        }

        private Task OnClientReceivedData(TcpSessionClient socketClient, ReceivedDataEventArgs e)
        {
            return this.OnTcpReceived((TClient)socketClient, e);
        }

        #endregion 事件

        #region 发送

        /// <inheritdoc/>
        public Task SendAsync(string id, ReadOnlyMemory<byte> memory)
        {
            return this.GetClientOrThrow(id).SendAsync(memory);
        }

        /// <inheritdoc/>
        public Task SendAsync(string id, IRequestInfo requestInfo)
        {
            return this.GetClientOrThrow(id).SendAsync(requestInfo);
        }

        private TcpSessionClient GetClientOrThrow(string id)
        {
            return this.GetClient(id);
        }

        #endregion 发送
    }
}