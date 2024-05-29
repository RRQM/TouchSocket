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
using TouchSocket.Sockets;

namespace TouchSocket.NamedPipe
{
    /// <summary>
    /// 泛型命名管道服务器。
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    public abstract class NamedPipeService<TClient> : NamedPipeServiceBase<TClient>, INamedPipeService<TClient> where TClient : NamedPipeSessionClient
    {
        /// <inheritdoc/>
        protected override void ClientInitialized(TClient client)
        {
            client.SetAction(this.OnClientConnecting, this.OnClientConnected, this.OnClientClosing, this.OnClientClosed, this.OnClientReceivedData);
            base.ClientInitialized(client);
        }

        #region 事件

        /// <inheritdoc/>
        public ConnectedEventHandler<TClient> Connected { get; set; }

        /// <inheritdoc/>
        public ConnectingEventHandler<TClient> Connecting { get; set; }

        /// <inheritdoc/>
        public ClosedEventHandler<TClient> Closed { get; set; }

        /// <inheritdoc/>
        public ClosingEventHandler<TClient> Closing { get; set; }

        /// <inheritdoc/>
        public ReceivedEventHandler<TClient> Received { get; set; }

        /// <summary>
        /// 客户端连接完成，覆盖父类方法将不会触发事件。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected virtual Task OnNamedPipeConnected(TClient socketClient, ConnectedEventArgs e)
        {
            if (this.Connected != null)
            {
                return this.Connected.Invoke(socketClient, e);
            }
            return EasyTask.CompletedTask;
        }

        /// <summary>
        /// 客户端请求连接，覆盖父类方法将不会触发事件。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected virtual Task OnNamedPipeConnecting(TClient socketClient, ConnectingEventArgs e)
        {
            if (this.Connecting != null)
            {
                return this.Connecting.Invoke(socketClient, e);
            }
            return EasyTask.CompletedTask;
        }

        /// <summary>
        /// 客户端断开连接，覆盖父类方法将不会触发事件。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected virtual Task OnNamedPipeClosed(TClient socketClient, ClosedEventArgs e)
        {
            if (this.Closed != null)
            {
                return this.Closed.Invoke(socketClient, e);
            }
            return EasyTask.CompletedTask;
        }

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected virtual Task OnNamedPipeClosing(TClient socketClient, ClosingEventArgs e)
        {
            if (this.Closing != null)
            {
                return this.Closing.Invoke(socketClient, e);
            }
            return EasyTask.CompletedTask;
        }

        /// <summary>
        /// 当收到适配器数据。
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected virtual Task OnNamedPipeReceived(TClient socketClient, ReceivedDataEventArgs e)
        {
            if (this.Received != null)
            {
                return this.Received.Invoke(socketClient, e);
            }
            return EasyTask.CompletedTask;
        }

        private Task OnClientConnected(NamedPipeSessionClient socketClient, ConnectedEventArgs e)
        {
            return this.OnNamedPipeConnected((TClient)socketClient, e);
        }

        private Task OnClientConnecting(NamedPipeSessionClient socketClient, ConnectingEventArgs e)
        {
            return this.OnNamedPipeConnecting((TClient)socketClient, e);
        }

        private Task OnClientClosed(NamedPipeSessionClient socketClient, ClosedEventArgs e)
        {
            return this.OnNamedPipeClosed((TClient)socketClient, e);
        }

        private Task OnClientClosing(NamedPipeSessionClient socketClient, ClosingEventArgs e)
        {
            return this.OnNamedPipeClosing((TClient)socketClient, e);
        }

        private Task OnClientReceivedData(NamedPipeSessionClient socketClient, ReceivedDataEventArgs e)
        {
            return this.OnNamedPipeReceived((TClient)socketClient, e);
        }

        #endregion 事件

        #region 发送

        ///// <inheritdoc/>
        //public void 123Send(string id, byte[] buffer, int offset, int length)
        //{
        //    this.GetClientOrThrow(id).123Send(buffer, offset, length);
        //}

        ///// <inheritdoc/>
        //public void 123Send(string id, IRequestInfo requestInfo)
        //{
        //    this.GetClientOrThrow(id).123Send(requestInfo);
        //}

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

        private NamedPipeSessionClient GetClientOrThrow(string id)
        {
            return this.GetClient(id);
        }

        #endregion 发送
    }
}