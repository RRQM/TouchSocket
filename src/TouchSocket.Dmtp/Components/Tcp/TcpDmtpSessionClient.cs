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

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// 抽象类TcpDmtpSessionClient定义了基于TCP的Dmtp会话客户端的基本行为。
    /// 它扩展了TcpSessionClientBase类，并实现了ITcpDmtpSessionClient接口。
    /// </summary>
    public abstract class TcpDmtpSessionClient : TcpSessionClientBase, ITcpDmtpSessionClient
    {
        private readonly DmtpAdapter m_dmtpAdapter = new DmtpAdapter();
        private DmtpActor m_dmtpActor;
        private Func<TcpDmtpSessionClient, ConnectingEventArgs, Task> m_privateConnecting;

        /// <summary>
        /// 构造函数：初始化TcpDmtpSessionClient实例
        /// </summary>
        /// <remarks>
        /// 设置协议属性为DmtpProtocol，这是应用程序通信使用的协议
        /// </remarks>
        protected TcpDmtpSessionClient()
        {
            this.Protocol = DmtpUtility.DmtpProtocol;
        }

        /// <inheritdoc/>
        public IDmtpActor DmtpActor => this.m_dmtpActor;

        /// <inheritdoc cref="IOnlineClient.Online"/>
        public override bool Online => this.m_dmtpActor != null && this.m_dmtpActor.Online;

        /// <summary>
        /// 验证超时时间,默认为3000ms
        /// </summary>
        public TimeSpan VerifyTimeout => this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).VerifyTimeout;

        /// <summary>
        /// 连接令箭
        /// </summary>
        public string VerifyToken => this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).VerifyToken;

        #region 内部委托绑定

        private Task OnDmtpActorClose(DmtpActor actor, string msg)
        {
            base.Abort(false, msg);
            return EasyTask.CompletedTask;
        }

        private Task OnDmtpActorCreateChannel(DmtpActor actor, CreateChannelEventArgs e)
        {
            return this.OnCreateChannel(e);
        }

        private Task OnDmtpActorHandshaked(DmtpActor actor, DmtpVerifyEventArgs e)
        {
            return this.OnHandshaked(e);
        }

        private async Task OnDmtpActorHandshaking(DmtpActor actor, DmtpVerifyEventArgs e)
        {
            if (e.Token == this.VerifyToken)
            {
                e.IsPermitOperation = true;
            }
            else
            {
                e.Message = "Token不受理";
            }

            await this.OnHandshaking(e).ConfigureAwait(false);
        }

        private Task OnDmtpActorRouting(DmtpActor actor, PackageRouterEventArgs e)
        {
            return this.OnRouting(e);
        }

        #endregion 内部委托绑定

        #region 事件

        /// <summary>
        /// 当创建通道时触发的事件处理方法
        /// 此方法主要用于在创建通道时，触发相关插件的调用
        /// </summary>
        /// <param name="e">包含通道创建信息的事件参数</param>
        protected virtual async Task OnCreateChannel(CreateChannelEventArgs e)
        {
            // 如果事件已经被处理，则直接返回
            if (e.Handled)
            {
                return;
            }

            // 异步调用所有IDmtpCreatedChannelPlugin类型的插件，并传递当前实例和事件参数
            await this.PluginManager.RaiseAsync(typeof(IDmtpCreatedChannelPlugin), this.Resolver, this, e).ConfigureAwait(false);
        }

        /// <summary>
        /// 当Dmtp关闭以后。
        /// </summary>
        /// <param name="e">事件参数</param>
        /// <returns></returns>
        protected virtual async Task OnDmtpClosed(ClosedEventArgs e)
        {
            // 如果事件已经被处理，则直接返回
            if (e.Handled)
            {
                return;
            }
            // 通知插件管理器，Dmtp已经关闭
            await this.PluginManager.RaiseAsync(typeof(IDmtpClosedPlugin), this.Resolver, this, e).ConfigureAwait(false);
        }

        /// <summary>
        /// 当Dmtp即将被关闭时触发。
        /// <para>
        /// 该触发条件有2种：
        /// <list type="number">
        /// <item>终端主动调用<see cref="CloseAsync(string)"/>。</item>
        /// <item>终端收到<see cref="DmtpActor.P0_Close"/>的请求。</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="e">关闭事件参数，包含关闭的原因等信息。</param>
        /// <returns>返回一个异步任务，表示事件处理的完成。</returns>
        protected virtual async Task OnDmtpClosing(ClosingEventArgs e)
        {
            // 如果事件已经被处理，则直接返回
            if (e.Handled)
            {
                return;
            }
            // 通知插件管理器，Dmtp即将被关闭
            await this.PluginManager.RaiseAsync(typeof(IDmtpClosingPlugin), this.Resolver, this, e).ConfigureAwait(false);
        }

        /// <summary>
        /// 在完成握手连接时
        /// </summary>
        /// <param name="e">包含握手信息的事件参数</param>
        protected virtual async Task OnHandshaked(DmtpVerifyEventArgs e)
        {
            // 如果握手已经被处理，则不再继续执行
            if (e.Handled)
            {
                return;
            }
            // 触发插件管理器中的握手完成插件事件
            await this.PluginManager.RaiseAsync(typeof(IDmtpHandshakedPlugin), this.Resolver, this, e).ConfigureAwait(false);
        }

        /// <summary>
        /// 在验证Token时
        /// </summary>
        /// <param name="e">参数</param>
        protected virtual async Task OnHandshaking(DmtpVerifyEventArgs e)
        {
            // 如果事件已经被处理，则直接返回
            if (e.Handled)
            {
                return;
            }
            // 异步调用插件管理器，执行握手验证插件
            await this.PluginManager.RaiseAsync(typeof(IDmtpHandshakingPlugin), this.Resolver, this, e).ConfigureAwait(false);
        }

        /// <summary>
        /// 在需要转发路由包时。
        /// </summary>
        /// <param name="e">路由事件参数</param>
        protected virtual async Task OnRouting(PackageRouterEventArgs e)
        {
            // 如果事件已经被处理，则直接返回
            if (e.Handled)
            {
                return;
            }
            // 异步调用插件管理器，执行路由转发插件
            await this.PluginManager.RaiseAsync(typeof(IDmtpRoutingPlugin), this.Resolver, this, e).ConfigureAwait(false);
        }

        #endregion 事件

        #region 断开

        /// <summary>
        /// 发送<see cref="IDmtpActor"/>关闭消息。
        /// </summary>
        /// <param name="msg">关闭消息的内容</param>
        /// <returns>异步任务</returns>
        public override async Task CloseAsync(string msg)
        {
            // 检查是否已初始化IDmtpActor接口
            if (this.m_dmtpActor != null)
            {
                // 如果已初始化，则调用IDmtpActor的CloseAsync方法发送关闭消息
                await this.m_dmtpActor.CloseAsync(msg).ConfigureAwait(false);
            }

            // 调用基类的CloseAsync方法发送关闭消息
            await base.CloseAsync(msg).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            this.m_dmtpActor?.SafeDispose();
            base.Dispose(disposing);
        }

        #endregion 断开

        #region ResetId

        ///<inheritdoc/>
        public override Task ResetIdAsync(string newId)
        {
            return this.m_dmtpActor.ResetIdAsync(newId);
        }

        #endregion ResetId

        #region Internal

        internal void InternalSetAction(Func<TcpDmtpSessionClient, ConnectingEventArgs, Task> privateConnecting)
        {
            this.m_privateConnecting = privateConnecting;
        }

        internal void InternalSetDmtpActor(DmtpActor actor)
        {
            actor.IdChanged = this.ThisOnResetId;
            //actor.OutputSend = this.ThisDmtpActorOutputSend;
            actor.OutputSendAsync = this.ThisDmtpActorOutputSendAsync;
            actor.Client = this;
            actor.Closing = this.OnDmtpActorClose;
            actor.Routing = this.OnDmtpActorRouting;
            actor.Handshaked = this.OnDmtpActorHandshaked;
            actor.Handshaking = this.OnDmtpActorHandshaking;
            actor.CreatedChannel = this.OnDmtpActorCreateChannel;
            actor.Logger = this.Logger;
            this.m_dmtpActor = actor;

            this.SetAdapter(this.m_dmtpAdapter);
        }

        #endregion Internal

        private Task ThisDmtpActorOutputSendAsync(DmtpActor actor, ReadOnlyMemory<byte> memory)
        {
            if (memory.Length > this.m_dmtpAdapter.MaxPackageSize)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_MoreThan(nameof(memory.Length), memory.Length, this.m_dmtpAdapter.MaxPackageSize);
            }

            return base.ProtectedDefaultSendAsync(memory);
        }

        private async Task ThisOnResetId(DmtpActor rpcActor, IdChangedEventArgs e)
        {
            await this.ProtectedResetIdAsync(e.NewId).ConfigureAwait(false);
        }

        #region Override

        /// <inheritdoc/>
        protected override async Task OnTcpClosed(ClosedEventArgs e)
        {
            await this.OnDmtpClosed(e).ConfigureAwait(false);
            await base.OnTcpClosed(e).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override async Task OnTcpClosing(ClosingEventArgs e)
        {
            await this.PluginManager.RaiseAsync(typeof(IDmtpClosingPlugin), this.Resolver, this, e).ConfigureAwait(false);
            await base.OnTcpClosing(e).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override async Task OnTcpConnected(ConnectedEventArgs e)
        {
            this.m_dmtpActor.Id = this.Id;
            _ = Task.Run(async () =>
            {
                await Task.Delay(this.VerifyTimeout).ConfigureAwait(false);
                if (!this.Online)
                {
                    this.TryShutdown();
                    await base.CloseAsync("握手验证超时").ConfigureAwait(false);
                }
            });

            await base.OnTcpConnected(e).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override async Task OnTcpConnecting(ConnectingEventArgs e)
        {
            await this.m_privateConnecting(this, e).ConfigureAwait(false);
            await base.OnTcpConnecting(e).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override async Task OnTcpReceived(ReceivedDataEventArgs e)
        {
            var message = (DmtpMessage)e.RequestInfo;
            if (!await this.m_dmtpActor.InputReceivedData(message).ConfigureAwait(false))
            {
                await this.PluginManager.RaiseAsync(typeof(IDmtpReceivedPlugin), this.Resolver, this, new DmtpMessageEventArgs(message)).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        protected override async ValueTask<bool> OnTcpReceiving(ByteBlock byteBlock)
        {
            while (byteBlock.CanRead)
            {
                if (this.m_dmtpAdapter.TryParseRequest(ref byteBlock, out var message))
                {
                    using (message)
                    {
                        if (!await this.m_dmtpActor.InputReceivedData(message).ConfigureAwait(false))
                        {
                            await this.PluginManager.RaiseAsync(typeof(IDmtpReceivedPlugin), this.Resolver, this, new DmtpMessageEventArgs(message)).ConfigureAwait(false);
                        }
                    }
                }
            }
            return true;
        }

        #endregion Override
    }
}