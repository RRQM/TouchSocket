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
    /// TcpDmtpSessionClient
    /// </summary>
    public abstract class TcpDmtpSessionClient : TcpSessionClientBase, ITcpDmtpSessionClient
    {
        private DmtpActor m_dmtpActor;
        private Func<TcpDmtpSessionClient, ConnectingEventArgs, Task> m_privateConnecting;
        private readonly DmtpAdapter m_dmtpAdapter = new DmtpAdapter();

        /// <summary>
        /// TcpDmtpSessionClient
        /// </summary>
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
        /// 当创建通道
        /// </summary>
        /// <param name="e"></param>
        protected virtual async Task OnCreateChannel(CreateChannelEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }

            await this.PluginManager.RaiseAsync(typeof(IDmtpCreatedChannelPlugin), this, e).ConfigureAwait(false);
        }

        /// <summary>
        /// 在完成握手连接时
        /// </summary>
        /// <param name="e"></param>
        protected virtual async Task OnHandshaked(DmtpVerifyEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }
            await this.PluginManager.RaiseAsync(typeof(IDmtpHandshakedPlugin), this, e).ConfigureAwait(false);
        }

        /// <summary>
        /// 在验证Token时
        /// </summary>
        /// <param name="e">参数</param>
        protected virtual async Task OnHandshaking(DmtpVerifyEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }
            await this.PluginManager.RaiseAsync(typeof(IDmtpHandshakingPlugin), this, e).ConfigureAwait(false);
        }

        /// <summary>
        /// 在需要转发路由包时。
        /// </summary>
        /// <param name="e"></param>
        protected virtual async Task OnRouting(PackageRouterEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }
            await this.PluginManager.RaiseAsync(typeof(IDmtpRoutingPlugin), this, e).ConfigureAwait(false);
        }

        /// <summary>
        /// 当Dmtp关闭以后。
        /// </summary>
        /// <param name="e">事件参数</param>
        /// <returns></returns>
        protected virtual async Task OnDmtpClosed(ClosedEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }
            await this.PluginManager.RaiseAsync(typeof(IDmtpClosedPlugin), this, e).ConfigureAwait(false);
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
        /// <param name="e"></param>
        /// <returns></returns>
        protected virtual async Task OnDmtpClosing(ClosingEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }
            await this.PluginManager.RaiseAsync(typeof(IDmtpClosingPlugin), this, e).ConfigureAwait(false);
        }

        #endregion 事件

        #region 断开

        /// <summary>
        /// 发送<see cref="IDmtpActor"/>关闭消息。
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public override async Task CloseAsync(string msg)
        {
            if (this.m_dmtpActor != null)
            {
                await this.m_dmtpActor.CloseAsync(msg).ConfigureAwait(false);
            }

            await base.CloseAsync(msg).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            this.m_dmtpActor?.SafeDispose();
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override async Task OnTcpClosed(ClosedEventArgs e)
        {
            await this.OnDmtpClosed(e).ConfigureAwait(false);
        }

        #endregion 断开

        #region ResetId

        ///<inheritdoc/>
        public override Task ResetIdAsync(string targetId)
        {
            return this.m_dmtpActor.ResetIdAsync(targetId);
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

        private Task ThisDmtpActorOutputSendAsync(DmtpActor actor,ReadOnlyMemory<byte> memory)
        {
            if (memory.Length > this.m_dmtpAdapter.MaxPackageSize)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException_MoreThan(nameof(memory.Length), memory.Length, this.m_dmtpAdapter.MaxPackageSize);
            }

            return base.ProtectedDefaultSendAsync(memory);
        }

        private async Task ThisOnResetId(DmtpActor rpcActor, IdChangedEventArgs e)
        {
            await this.ProtectedResetId(e.NewId).ConfigureAwait(false);
        }

        #region Override

        /// <inheritdoc/>
        protected override async Task OnTcpClosing(ClosingEventArgs e)
        {
            await this.PluginManager.RaiseAsync(typeof(IDmtpClosingPlugin), this, e).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override Task OnTcpConnected(ConnectedEventArgs e)
        {
            this.m_dmtpActor.Id = this.Id;
            _ = Task.Run(async () =>
            {
                await Task.Delay(this.VerifyTimeout).ConfigureAwait(false);
                if (!this.Online)
                {
                    this.TryShutdown();
                    await base.CloseAsync("Handshak验证超时").ConfigureAwait(false);
                }
            });

            return EasyTask.CompletedTask;
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
                await this.PluginManager.RaiseAsync(typeof(IDmtpReceivedPlugin), this, new DmtpMessageEventArgs(message)).ConfigureAwait(false);
            }
        }

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
                            await this.PluginManager.RaiseAsync(typeof(IDmtpReceivedPlugin), this, new DmtpMessageEventArgs(message)).ConfigureAwait(false);
                        }
                    }
                }
            }
            return true;
        }

        #endregion Override
    }
}