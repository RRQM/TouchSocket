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
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// HttpDmtpClient
    /// </summary>
    public partial class HttpDmtpClient : HttpClientBase, IHttpDmtpClient
    {
        #region 字段

        private readonly SemaphoreSlim m_semaphoreForConnect = new SemaphoreSlim(1, 1);
        private bool m_allowRoute;
        private DmtpActor m_dmtpActor;
        private Func<string, Task<IDmtpActor>> m_findDmtpActor;

        #endregion 字段

        /// <inheritdoc/>
        public IDmtpActor DmtpActor => this.m_dmtpActor;

        /// <inheritdoc cref="IIdClient.Id"/>
        public string Id => this.m_dmtpActor.Id;

        /// <inheritdoc cref="IOnlineClient.Online"/>
        public override bool Online => this.m_dmtpActor != null && this.m_dmtpActor.Online;

        #region 连接

        /// <summary>
        /// 异步使用基于Http升级的协议，连接Dmtp服务器
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task ConnectAsync(int millisecondsTimeout, CancellationToken token)
        {
            await this.m_semaphoreForConnect.WaitTimeAsync(millisecondsTimeout, token).ConfigureFalseAwait();

            try
            {
                if (this.Online)
                {
                    return;
                }
                if (!base.Online)
                {
                    await base.TcpConnectAsync(millisecondsTimeout, token).ConfigureFalseAwait();
                }

                var request = new HttpRequest(this)
                    .SetHost(this.RemoteIPHost.Host);
                request.Headers.Add(HttpHeaders.Connection, "upgrade");
                request.Headers.Add(HttpHeaders.Upgrade, DmtpUtility.Dmtp.ToLower());

                request.AsMethod(DmtpUtility.Dmtp);

                using (var responseResult = await this.ProtectedRequestContentAsync(request).ConfigureFalseAwait())
                {
                    var response = responseResult.Response;
                    if (response.StatusCode == 101)
                    {
                        this.SwitchProtocolToDmtp();
                        await this.m_dmtpActor.HandshakeAsync(this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).VerifyToken, this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).Id, millisecondsTimeout, this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).Metadata, token).ConfigureFalseAwait();
                    }
                    else
                    {
                        throw new Exception(response.StatusMessage);
                    }
                }
            }
            finally
            {
                this.m_semaphoreForConnect.Release();
            }
        }

        #endregion 连接

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
                await this.m_dmtpActor.SendCloseAsync(msg).ConfigureFalseAwait();
                await this.m_dmtpActor.CloseAsync(msg).ConfigureFalseAwait();
            }
            await base.CloseAsync(msg).ConfigureFalseAwait();
        }

        #endregion 断开

        #region Override

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            this.m_dmtpActor?.SafeDispose();
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override void LoadConfig(TouchSocketConfig config)
        {
            base.LoadConfig(config);
            if (this.Resolver.IsRegistered(typeof(IDmtpRouteService)))
            {
                this.m_allowRoute = true;
                this.m_findDmtpActor = this.Resolver.Resolve<IDmtpRouteService>().FindDmtpActor;
            }
        }

        /// <inheritdoc/>
        protected override async Task OnTcpClosed(ClosedEventArgs e)
        {
            await this.m_dmtpActor.CloseAsync(e.Message).ConfigureFalseAwait();
            await this.OnDmtpClosed(e).ConfigureFalseAwait();
        }

        /// <inheritdoc/>
        protected override Task OnTcpClosing(ClosingEventArgs e)
        {
            return this.PluginManager.RaiseAsync(typeof(IDmtpClosingPlugin), this, e);
        }

        /// <inheritdoc/>
        protected override async Task OnTcpReceived(ReceivedDataEventArgs e)
        {
            if (this.Protocol == DmtpUtility.DmtpProtocol && e.RequestInfo is DmtpMessage message)
            {
                if (!await this.m_dmtpActor.InputReceivedData(message).ConfigureFalseAwait())
                {
                    if (this.PluginManager.Enable)
                    {
                        await this.PluginManager.RaiseAsync(typeof(IDmtpReceivedPlugin), this, new DmtpMessageEventArgs(message)).ConfigureFalseAwait();
                    }
                }
                return;
            }
            await base.OnTcpReceived(e).ConfigureFalseAwait();
        }

        #endregion Override

        #region ResetId

        ///<inheritdoc/>
        public Task ResetIdAsync(string targetId)
        {
            return this.m_dmtpActor.ResetIdAsync(targetId);
        }

        #endregion ResetId

        private void SwitchProtocolToDmtp()
        {
            this.Protocol = DmtpUtility.DmtpProtocol;
            DmtpAdapter adapter = new DmtpAdapter();
            base.SetWarpAdapter(adapter);
            this.SetAdapter(adapter);
            this.m_dmtpActor = new SealedDmtpActor(this.m_allowRoute)
            {
                //OutputSend = this.DmtpActorSend,
                OutputSendAsync = this.DmtpActorSendAsync,
                Routing = this.OnDmtpActorRouting,
                Handshaking = this.OnDmtpActorHandshaking,
                Handshaked = this.OnDmtpActorHandshaked,
                Closing = this.OnDmtpActorClose,
                CreatedChannel = this.OnDmtpActorCreateChannel,
                Logger = this.Logger,
                Client = this,
                FindDmtpActor = this.m_findDmtpActor
            };
        }

        #region 内部委托绑定

        private Task DmtpActorSendAsync(DmtpActor actor, ReadOnlyMemory<byte> memory)
        {
            return base.ProtectedDefaultSendAsync(memory);
        }

        private async Task OnDmtpActorClose(DmtpActor actor, string msg)
        {
            await this.OnDmtpClosing(new ClosingEventArgs(msg)).ConfigureFalseAwait();
            this.Abort(false, msg);
        }

        private Task OnDmtpActorCreateChannel(DmtpActor actor, CreateChannelEventArgs e)
        {
            return this.OnCreateChannel(e);
        }

        private Task OnDmtpActorHandshaked(DmtpActor actor, DmtpVerifyEventArgs e)
        {
            return this.OnHandshaked(e);
        }

        private Task OnDmtpActorHandshaking(DmtpActor actor, DmtpVerifyEventArgs e)
        {
            return this.OnHandshaking(e);
        }

        private Task OnDmtpActorRouting(DmtpActor actor, PackageRouterEventArgs e)
        {
            return this.OnRouting(e);
        }

        #endregion 内部委托绑定

        #region 事件触发

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

            await this.PluginManager.RaiseAsync(typeof(IDmtpCreatedChannelPlugin), this, e).ConfigureFalseAwait();
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
        /// 即将握手连接时
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
        /// 当需要转发路由包时
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

        #endregion 事件触发
    }
}