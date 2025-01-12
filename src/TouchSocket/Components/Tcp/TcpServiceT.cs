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
    /// 抽象类<see cref="TcpService{TClient}"/>为基于TCP协议的服务提供基础实现。
    /// 它扩展了<see cref="TcpServiceBase{TClient}"/>并实现了<see cref="ITcpService{TClient}"/>接口，其中TClient是<see cref="TcpSessionClient"/>的一个派生类。
    /// 该类旨在为具体的服务类提供一个框架，定义了与TCP客户端会话管理相关的基本功能。
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

        /// <inheritdoc/>
        public ClosedEventHandler<TClient> Closed { get; set; }

        /// <inheritdoc/>
        public ClosingEventHandler<TClient> Closing { get; set; }

        /// <inheritdoc/>
        public ConnectedEventHandler<TClient> Connected { get; set; }

        /// <inheritdoc/>
        public ConnectingEventHandler<TClient> Connecting { get; set; }

        /// <inheritdoc/>
        public ReceivedEventHandler<TClient> Received { get; set; }

        /// <summary>
        /// 客户端断开连接，覆盖父类方法将不会触发事件。
        /// </summary>
        /// <param name="sessionClient">发生断开连接事件的客户端</param>
        /// <param name="e">断开连接事件参数</param>
        protected virtual async Task OnTcpClosed(TClient sessionClient, ClosedEventArgs e)
        {
            // 检查是否已注册断开连接事件
            if (this.Closed != null)
            {
                // 如果已注册，则异步调用事件处理程序，并防止等待结果
                await this.Closed.Invoke(sessionClient, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
        }

        /// <summary>
        /// 即将断开连接(仅主动断开时有效)。
        /// </summary>
        /// <param name="sessionClient">Tcp客户端对象</param>
        /// <param name="e">断开连接事件参数</param>
        protected virtual async Task OnTcpClosing(TClient sessionClient, ClosingEventArgs e)
        {
            // 如果存在已注册的断开连接事件处理程序
            if (this.Closing != null)
            {
                // 异步调用断开连接事件处理程序，并防止等待其完成
                await this.Closing.Invoke(sessionClient, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
        }

        /// <summary>
        /// 客户端连接完成时的异步处理方法。此方法覆盖父类的同名方法，用于处理客户端成功连接到服务器的情况。
        /// 不同之处在于，此方法的调用不会触发事件，而是直接执行连接处理逻辑。
        /// </summary>
        /// <param name="sessionClient">表示客户端的套接字对象。</param>
        /// <param name="e">包含连接过程中相关数据的事件参数对象。</param>
        protected virtual async Task OnTcpConnected(TClient sessionClient, ConnectedEventArgs e)
        {
            // 检查是否已注册连接事件处理程序
            if (this.Connected != null)
            {
                // 如果已注册事件处理程序，则异步调用该处理程序，并使用ConfigureAwait(false)以避免上下文切换
                await this.Connected.Invoke(sessionClient, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
        }

        /// <summary>
        /// 客户端请求连接，覆盖父类方法将不会触发事件。
        /// </summary>
        /// <param name="sessionClient">正在连接的客户端对象</param>
        /// <param name="e">连接事件参数</param>
        protected virtual async Task OnTcpConnecting(TClient sessionClient, ConnectingEventArgs e)
        {
            // 检查是否已注册连接事件
            if (this.Connecting != null)
            {
                // 如果已注册，则调用事件处理程序并等待操作完成
                await this.Connecting.Invoke(sessionClient, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
        }

        /// <summary>
        /// 当接收到TCP数据时触发的异步事件处理器。
        /// </summary>
        /// <param name="sessionClient">发生数据接收的客户端套接字实例。</param>
        /// <param name="e">包含接收数据事件的相关数据。</param>
        /// <remarks>
        /// 此方法不直接处理数据接收，而是作为事件处理器，当数据接收事件被触发时，它会检查是否存在已注册的事件处理程序。
        /// 如果存在，它将异步调用这些事件处理程序。这种设计允许外部代码根据需要自定义数据处理逻辑，
        /// 同时保持了代码的灵活性和可扩展性。
        /// </remarks>
        protected virtual async Task OnTcpReceived(TClient sessionClient, ReceivedDataEventArgs e)
        {
            // 检查是否有已注册的事件处理程序
            if (this.Received != null)
            {
                // 如果有，异步调用事件处理程序，并且不会等待调用完成
                await this.Received.Invoke(sessionClient, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
        }

        private Task OnClientClosing(TcpSessionClient sessionClient, ClosingEventArgs e)
        {
            return this.OnTcpClosing((TClient)sessionClient, e);
        }

        private Task OnClientConnected(TcpSessionClient sessionClient, ConnectedEventArgs e)
        {
            return this.OnTcpConnected((TClient)sessionClient, e);
        }

        private Task OnClientConnecting(TcpSessionClient sessionClient, ConnectingEventArgs e)
        {
            return this.OnTcpConnecting((TClient)sessionClient, e);
        }

        private Task OnClientDisconnected(TcpSessionClient sessionClient, ClosedEventArgs e)
        {
            return this.OnTcpClosed((TClient)sessionClient, e);
        }

        private Task OnClientReceivedData(TcpSessionClient sessionClient, ReceivedDataEventArgs e)
        {
            return this.OnTcpReceived((TClient)sessionClient, e);
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