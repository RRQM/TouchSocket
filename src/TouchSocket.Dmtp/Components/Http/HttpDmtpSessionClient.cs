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
using TouchSocket.Http;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// Http服务器辅助类
    /// </summary>
    public abstract class HttpDmtpSessionClient : HttpSessionClient, IHttpDmtpSessionClient
    {
        internal Func<DmtpActor> m_internalOnRpcActorInit;
        private DmtpActor m_dmtpActor;

        /// <inheritdoc/>
        public IDmtpActor DmtpActor => this.m_dmtpActor;

        /// <inheritdoc/>
        public override bool Online => this.m_dmtpActor == null ? base.Online : this.m_dmtpActor.Online;

        /// <summary>
        /// 验证超时时间
        /// </summary>
        public TimeSpan VerifyTimeout => this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).VerifyTimeout;

        /// <summary>
        /// 连接令箭
        /// </summary>
        public string VerifyToken => this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).VerifyToken;

        #region 断开

        /// <inheritdoc/>
        public override async Task CloseAsync(string msg)
        {
            if (this.m_dmtpActor != null)
            {
                await this.m_dmtpActor.CloseAsync(msg).ConfigureFalseAwait();
            }

            await base.CloseAsync(msg).ConfigureFalseAwait();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            this.m_dmtpActor?.SafeDispose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="e"></param>
        protected override async Task OnTcpClosed(ClosedEventArgs e)
        {
            await this.OnDmtpClosed(e).ConfigureFalseAwait();
        }

        #endregion 断开

        #region ResetId

        ///<inheritdoc/>
        public override async Task ResetIdAsync(string targetId)
        {
            if (this.m_dmtpActor == null)
            {
                await base.ResetIdAsync(targetId).ConfigureFalseAwait();
                return;
            }
            await this.m_dmtpActor.ResetIdAsync(targetId).ConfigureFalseAwait();
        }

        #endregion ResetId

        private async Task OnDmtpIdChanged(DmtpActor actor, IdChangedEventArgs e)
        {
            await this.ProtectedResetId(e.NewId).ConfigureFalseAwait();
        }

        private void SetRpcActor(DmtpActor actor)
        {
            actor.Id = this.Id;
            actor.IdChanged = this.OnDmtpIdChanged;
            actor.OutputSendAsync = this.ThisDmtpActorOutputSendAsync;
            //actor.OutputSend = this.ThisDmtpActorOutputSend;
            actor.Client = this;
            actor.Closing = this.OnDmtpActorClose;
            actor.Routing = this.OnDmtpActorRouting;
            actor.Handshaked = this.OnDmtpActorHandshaked;
            actor.Handshaking = this.OnDmtpActorHandshaking;
            actor.CreatedChannel = this.OnDmtpActorCreatedChannel;
            actor.Logger = this.Logger;
            this.m_dmtpActor = actor;

            this.Protocol = DmtpUtility.DmtpProtocol;
            this.SetAdapter(new DmtpAdapter());
        }

        //private void ThisDmtpActorOutputSend(DmtpActor actor, ArraySegment<byte>[] transferBytes)
        //{
        //    base.ProtectedSend(transferBytes);
        //}

        private Task ThisDmtpActorOutputSendAsync(DmtpActor actor, ReadOnlyMemory<byte> memory)
        {
            return base.ProtectedDefaultSendAsync(memory);
        }

        #region Override

        /// <inheritdoc/>
        protected override Task OnTcpClosing(ClosingEventArgs e)
        {
            return this.PluginManager.RaiseAsync(typeof(IDmtpClosingPlugin), this, e);
        }

        /// <inheritdoc/>
        protected override async Task OnReceivedHttpRequest(HttpContext httpContext)
        {
            var request = httpContext.Request;
            var response = httpContext.Response;

            if (request.IsMethod(DmtpUtility.Dmtp) && request.IsUpgrade() &&
                string.Equals(request.Headers.Get(HttpHeaders.Upgrade), DmtpUtility.Dmtp, StringComparison.OrdinalIgnoreCase))
            {
                this.SetRpcActor(this.m_internalOnRpcActorInit.Invoke());

                await response.SetStatus(101, "Switching Protocols").AnswerAsync().ConfigureFalseAwait();
                return;
            }
            await base.OnReceivedHttpRequest(httpContext).ConfigureFalseAwait();
        }

        /// <inheritdoc/>
        protected override async Task OnTcpReceived(ReceivedDataEventArgs e)
        {
            if (this.Protocol == DmtpUtility.DmtpProtocol && e.RequestInfo is DmtpMessage message)
            {
                if (!await this.m_dmtpActor.InputReceivedData(message).ConfigureFalseAwait())
                {
                    await this.PluginManager.RaiseAsync(typeof(IDmtpReceivedPlugin), this, new DmtpMessageEventArgs(message)).ConfigureFalseAwait();
                }
            }
            await base.OnTcpReceived(e).ConfigureFalseAwait();
        }

        #endregion Override

        #region 内部委托绑定

        private Task OnDmtpActorClose(DmtpActor actor, string msg)
        {
            base.Abort(false, msg);
            return EasyTask.CompletedTask;
        }

        private Task OnDmtpActorCreatedChannel(DmtpActor actor, CreateChannelEventArgs e)
        {
            return this.OnCreatedChannel(e);
        }

        private Task OnDmtpActorHandshaked(DmtpActor actor, DmtpVerifyEventArgs e)
        {
            return this.OnHandshaked(e);
        }

        private Task OnDmtpActorHandshaking(DmtpActor actor, DmtpVerifyEventArgs e)
        {
            if (e.Token == this.VerifyToken)
            {
                e.IsPermitOperation = true;
            }
            else
            {
                e.Message = "Token不受理";
            }

            return this.OnHandshaking(e);
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
        protected virtual async Task OnCreatedChannel(CreateChannelEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }

            await this.PluginManager.RaiseAsync(typeof(IDmtpCreatedChannelPlugin), this, e).ConfigureFalseAwait();
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
            await this.PluginManager.RaiseAsync(typeof(IDmtpHandshakedPlugin), this, e).ConfigureFalseAwait();
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
            await this.PluginManager.RaiseAsync(typeof(IDmtpHandshakingPlugin), this, e).ConfigureFalseAwait();
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
            await this.PluginManager.RaiseAsync(typeof(IDmtpRoutingPlugin), this, e).ConfigureFalseAwait();
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
            await this.PluginManager.RaiseAsync(typeof(IDmtpClosedPlugin), this, e).ConfigureFalseAwait();
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
            await this.PluginManager.RaiseAsync(typeof(IDmtpClosingPlugin), this, e).ConfigureFalseAwait();
        }

        #endregion 事件
    }
}