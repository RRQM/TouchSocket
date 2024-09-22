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
    /// 抽象类 HttpDmtpSessionClient 继承自 HttpSessionClient，并实现 IHttpDmtpSessionClient 接口。
    /// 该类提供了与 HTTP DMTP 协议相关的会话客户端功能。
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
                await this.m_dmtpActor.CloseAsync(msg).ConfigureAwait(false);
            }

            await base.CloseAsync(msg).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (this.DisposedValue)
            {
                return;
            }
            base.Dispose(disposing);
            if (disposing && this.m_dmtpActor != null)
            {
                this.m_dmtpActor.Dispose();
            }
        }

        #endregion 断开

        #region ResetId

        ///<inheritdoc/>
        public override async Task ResetIdAsync(string newId)
        {
            if (this.m_dmtpActor == null)
            {
                await base.ResetIdAsync(newId).ConfigureAwait(false);
                return;
            }
            await this.m_dmtpActor.ResetIdAsync(newId).ConfigureAwait(false);
        }

        #endregion ResetId

        private async Task OnDmtpIdChanged(DmtpActor actor, IdChangedEventArgs e)
        {
            await this.ProtectedResetIdAsync(e.NewId).ConfigureAwait(false);
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

        private Task ThisDmtpActorOutputSendAsync(DmtpActor actor, ReadOnlyMemory<byte> memory)
        {
            return base.ProtectedDefaultSendAsync(memory);
        }

        #region Override

        /// <inheritdoc/>
        protected override async Task OnReceivedHttpRequest(HttpContext httpContext)
        {
            var request = httpContext.Request;
            var response = httpContext.Response;

            if (request.IsMethod(DmtpUtility.Dmtp) && request.IsUpgrade() &&
                string.Equals(request.Headers.Get(HttpHeaders.Upgrade), DmtpUtility.Dmtp, StringComparison.OrdinalIgnoreCase))
            {
                this.SetRpcActor(this.m_internalOnRpcActorInit.Invoke());

                await response.SetStatus(101, "Switching Protocols").AnswerAsync().ConfigureAwait(false);
                return;
            }
            await base.OnReceivedHttpRequest(httpContext).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override async Task OnTcpClosed(ClosedEventArgs e)
        {
            await this.OnDmtpClosed(e).ConfigureAwait(false);
            await base.OnTcpClosed(e);
        }

        /// <inheritdoc/>
        protected override async Task OnTcpClosing(ClosingEventArgs e)
        {
            await this.PluginManager.RaiseAsync(typeof(IDmtpClosingPlugin), this, e).ConfigureAwait(false);
            await base.OnTcpClosing(e).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override async Task OnTcpReceived(ReceivedDataEventArgs e)
        {
            if (this.Protocol == DmtpUtility.DmtpProtocol && e.RequestInfo is DmtpMessage message)
            {
                if (!await this.m_dmtpActor.InputReceivedData(message).ConfigureAwait(false))
                {
                    await this.PluginManager.RaiseAsync(typeof(IDmtpReceivedPlugin), this, new DmtpMessageEventArgs(message)).ConfigureAwait(false);
                }
            }
            await base.OnTcpReceived(e).ConfigureAwait(false);
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
        /// 当创建通道时触发的事件处理程序
        /// </summary>
        /// <param name="e">包含通道创建信息的事件参数</param>
        protected virtual async Task OnCreatedChannel(CreateChannelEventArgs e)
        {
            // 如果事件已经被处理，则直接返回
            if (e.Handled)
            {
                return;
            }

            // 异步调用插件管理器，通知所有实现IDmtpCreatedChannelPlugin接口的插件关于通道创建的事件
            await this.PluginManager.RaiseAsync(typeof(IDmtpCreatedChannelPlugin), this, e).ConfigureAwait(false);
        }

        /// <summary>
        /// 当Dmtp关闭以后。
        /// </summary>
        /// <param name="e">事件参数</param>
        /// <returns></returns>
        protected virtual async Task OnDmtpClosed(ClosedEventArgs e)
        {
            //如果事件已经被处理，则直接返回
            if (e.Handled)
            {
                return;
            }
            //通知插件管理器，Dmtp已经关闭
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
        /// <param name="e">提供了关闭事件的相关信息。</param>
        /// <returns>返回一个Task对象，表示异步操作的完成。</returns>
        protected virtual async Task OnDmtpClosing(ClosingEventArgs e)
        {
            // 如果关闭事件已经被处理，则直接返回，不再执行后续操作。
            if (e.Handled)
            {
                return;
            }
            // 通知插件管理器，触发IDmtpClosingPlugin接口的事件处理程序，并传递相关参数。
            await this.PluginManager.RaiseAsync(typeof(IDmtpClosingPlugin), this, e).ConfigureAwait(false);
        }

        /// <summary>
        /// 在完成握手连接时
        /// </summary>
        /// <param name="e">包含握手信息的事件参数</param>
        protected virtual async Task OnHandshaked(DmtpVerifyEventArgs e)
        {
            // 如果握手已经被处理，则不再执行后续操作
            if (e.Handled)
            {
                return;
            }
            // 触发插件管理器中的握手完成插件事件
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
        /// 当需要转发路由包时。
        /// </summary>
        /// <param name="e">包含路由包相关信息的事件参数。</param>
        protected virtual async Task OnRouting(PackageRouterEventArgs e)
        {
            // 如果事件已经被处理，则直接返回。
            if (e.Handled)
            {
                return;
            }
            // 异步调用插件管理器，通知所有实现了IDmtpRoutingPlugin接口的插件处理路由包。
            await this.PluginManager.RaiseAsync(typeof(IDmtpRoutingPlugin), this, e).ConfigureAwait(false);
        }

        #endregion 事件
    }
}