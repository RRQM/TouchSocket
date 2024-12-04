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

using Microsoft.AspNetCore.Http;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp.AspNetCore
{
    /// <summary>
    /// WebSocketDmtpSessionClient
    /// </summary>
    public class WebSocketDmtpSessionClient : ResolverConfigObject, IWebSocketDmtpSessionClient
    {
        /// <summary>
        /// WebSocketDmtpSessionClient
        /// </summary>
        public WebSocketDmtpSessionClient()
        {
            this.m_receiveCounter = new ValueCounter
            {
                Period = TimeSpan.FromSeconds(1),
                OnPeriod = this.OnReceivePeriod
            };
            this.m_sentCounter = new ValueCounter
            {
                Period = TimeSpan.FromSeconds(1),
                OnPeriod = this.OnSendPeriod
            };
        }

        #region 字段

        private WebSocket m_client;
        private TouchSocketConfig m_config;
        private DmtpActor m_dmtpActor;
        private DmtpAdapter m_dmtpAdapter;
        private IPluginManager m_pluginManager;
        private int m_receiveBufferSize = 1024 * 10;
        private ValueCounter m_receiveCounter;
        private IResolver m_resolver;
        private int m_sendBufferSize = 1024 * 10;
        private ValueCounter m_sentCounter;
        private WebSocketDmtpService m_service;
        private HttpContext m_httpContext;
        private string m_id;
        private readonly Lock m_locker = LockFactory.Create();

        #endregion 字段

        /// <inheritdoc/>
        public override TouchSocketConfig Config => this.m_config;

        /// <inheritdoc/>
        public IDmtpActor DmtpActor => this.m_dmtpActor;

        /// <inheritdoc/>
        public HttpContext HttpContext => this.m_httpContext;

        /// <inheritdoc/>
        public string Id => this.m_id;

        /// <inheritdoc/>
        public bool IsClient => false;

        /// <inheritdoc/>
        public bool Online => this.DmtpActor.Online;

        /// <inheritdoc/>
        public DateTime LastReceivedTime => this.m_receiveCounter.LastIncrement;

        /// <inheritdoc/>
        public DateTime LastSentTime => this.m_sentCounter.LastIncrement;

        /// <inheritdoc/>
        public override IPluginManager PluginManager => this.m_pluginManager;

        /// <inheritdoc/>
        public Protocol Protocol { get; protected set; } = DmtpUtility.DmtpProtocol;

        /// <inheritdoc/>
        public override IResolver Resolver => this.m_resolver;

        /// <inheritdoc/>
        public IWebSocketDmtpService Service => this.m_service;

        /// <inheritdoc/>
        public TimeSpan VerifyTimeout => this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).VerifyTimeout;

        /// <inheritdoc/>
        public string VerifyToken => this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).VerifyToken;

        /// <inheritdoc/>
        public Task CloseAsync(string msg)
        {
            this.Abort(true, msg);
            return EasyTask.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task ResetIdAsync(string newId)
        {
            if (string.IsNullOrEmpty(newId))
            {
                throw new ArgumentException($"“{nameof(newId)}”不能为 null 或空。", nameof(newId));
            }

            if (this.m_id == newId)
            {
                return;
            }

            await this.DmtpActor.ResetIdAsync(newId).ConfigureAwait(false);
            await this.ProtectedResetIdAsync(newId).ConfigureAwait(false);
        }

        internal void InternalSetConfig(TouchSocketConfig config)
        {
            this.m_config = config;
            this.m_dmtpAdapter = new DmtpAdapter()
            {
                ReceivedAsyncCallBack = this.PrivateHandleReceivedData,
                Logger = this.Logger
            };

            this.m_dmtpAdapter.Config(config);
        }

        internal void InternalSetContainer(IResolver resolver)
        {
            this.m_resolver = resolver;
            this.Logger ??= resolver.Resolve<ILog>();
        }

        internal void InternalSetId(string id)
        {
            this.m_id = id;
        }

        internal void InternalSetPluginManager(IPluginManager pluginManager)
        {
            this.m_pluginManager = pluginManager;
        }

        internal void InternalSetService(WebSocketDmtpService service)
        {
            this.m_service = service;
        }

        internal void SetDmtpActor(DmtpActor actor)
        {
            actor.IdChanged = this.ThisOnResetId;
            actor.OutputSendAsync = this.OnDmtpActorSendAsync;
            //actor.OutputSend = this.OnDmtpActorSend;
            actor.Client = this;
            actor.Closing = this.OnDmtpActorClose;
            actor.Routing = this.OnDmtpActorRouting;
            actor.Handshaked = this.OnDmtpActorHandshaked;
            actor.Handshaking = this.OnDmtpActorHandshaking;
            actor.CreatedChannel = this.OnDmtpActorCreateChannel;
            actor.Logger = this.Logger;
            this.m_dmtpActor = actor;
        }

        internal async Task Start(WebSocket webSocket, HttpContext context)
        {
            this.m_client = webSocket;
            this.m_httpContext = context;
            try
            {
                while (true)
                {
                    using (var byteBlock = new ByteBlock(this.m_receiveBufferSize))
                    {
                        var result = await this.m_client.ReceiveAsync(byteBlock.TotalMemory, default).ConfigureAwait(false);

                        if (result.Count == 0)
                        {
                            break;
                        }
                        byteBlock.SetLength(result.Count);
                        this.m_receiveCounter.Increment(result.Count);

                        await this.m_dmtpAdapter.ReceivedInputAsync(byteBlock).ConfigureAwait(false);
                    }
                }

                this.Abort(false, "远程终端主动关闭");
            }
            catch (Exception ex)
            {
                this.Abort(false, ex.Message);
            }
        }

        /// <summary>
        /// 直接重置Id。
        /// </summary>
        /// <param name="targetId"></param>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ClientNotFindException"></exception>
        protected async Task ProtectedResetIdAsync(string targetId)
        {
            var sourceId = this.m_id;
            if (this.m_service.TryRemove(this.m_id, out var sessionClient))
            {
                sessionClient.m_id = targetId;
                if (this.m_service.TryAdd(this.m_id, sessionClient))
                {
                    if (this.PluginManager.Enable)
                    {
                        var e = new IdChangedEventArgs(sourceId, targetId);
                        await this.PluginManager.RaiseAsync(typeof(IIdChangedPlugin), this.Resolver, sessionClient, e).ConfigureAwait(false);
                    }
                    return;
                }
                else
                {
                    sessionClient.m_id = sourceId;
                    if (this.m_service.TryAdd(sessionClient.m_id, sessionClient))
                    {
                        throw new Exception("Id重复");
                    }
                    else
                    {
                        await sessionClient.CloseAsync("修改新Id时操作失败，且回退旧Id时也失败。").ConfigureAwait(false);
                    }
                }
            }
            else
            {
                throw new ClientNotFindException(TouchSocketAspNetCoreResource.ClientNotFind.GetDescription(sourceId));
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Abort(true, $"{nameof(Dispose)}断开链接");
            }
        }

        private void Abort(bool manual, string msg)
        {
            lock (this.m_locker)
            {
                if (this.DisposedValue)
                {
                    return;
                }

                base.Dispose(true);
                this.m_client.SafeDispose();
                this.DmtpActor.SafeDispose();

                if (this.m_service.TryRemove(this.m_id, out _))
                {
                    //if (this.PluginManager.Enable)
                    //{
                    //    this.PluginManager.RaiseAsync(nameof(ITcpDisconnectedPlugin.OnTcpClosed), this, new ClosedEventArgs(manual, msg));
                    //}
                }
            }
        }

        private void OnReceivePeriod(long value)
        {
            this.m_receiveBufferSize = TouchSocketCoreUtility.HitBufferLength(value);
        }

        private void OnSendPeriod(long value)
        {
            this.m_sendBufferSize = TouchSocketCoreUtility.HitBufferLength(value);
        }

        #region 内部委托绑定

        private Task OnDmtpActorClose(DmtpActor actor, string msg)
        {
            this.Abort(true, msg);
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

        //private void OnDmtpActorSend(DmtpActor actor, ArraySegment<byte>[] transferBytes)
        //{
        //    try
        //    {
        //        this.m_semaphoreForSend.Wait();
        //        for (var i = 0; i < transferBytes.Length; i++)
        //        {
        //            Task task;
        //            if (i == transferBytes.Length - 1)
        //            {
        //                task = this.m_client.SendAsync(transferBytes[i], WebSocketMessageType.Binary, true, CancellationToken.None);
        //            }
        //            else
        //            {
        //                task = this.m_client.SendAsync(transferBytes[i], WebSocketMessageType.Binary, false, CancellationToken.None);
        //            }
        //            task.GetFalseAwaitResult();
        //            this.m_sendCounter.Increment(transferBytes[i].Count);
        //        }
        //    }
        //    finally
        //    {
        //        this.m_semaphoreForSend.Release();
        //    }
        //}

        private async Task OnDmtpActorSendAsync(DmtpActor actor, ReadOnlyMemory<byte> memory)
        {
            await this.m_client.SendAsync(memory.GetArray(), WebSocketMessageType.Binary, true, CancellationToken.None).ConfigureAwait(false);
            this.m_sentCounter.Increment(memory.Length);
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

            await this.PluginManager.RaiseAsync(typeof(IDmtpCreatedChannelPlugin), this.Resolver, this, e).ConfigureAwait(false);
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
            await this.PluginManager.RaiseAsync(typeof(IDmtpHandshakedPlugin), this.Resolver, this, e).ConfigureAwait(false);
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
            await this.PluginManager.RaiseAsync(typeof(IDmtpHandshakingPlugin), this.Resolver, this, e).ConfigureAwait(false);
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
            await this.PluginManager.RaiseAsync(typeof(IDmtpRoutingPlugin), this.Resolver, this, e).ConfigureAwait(false);
        }

        #endregion 事件

        private async Task PrivateHandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            var message = (DmtpMessage)requestInfo;
            if (!await this.m_dmtpActor.InputReceivedData(message).ConfigureAwait(false))
            {
                await this.PluginManager.RaiseAsync(typeof(IDmtpReceivedPlugin), this.Resolver, this, new DmtpMessageEventArgs(message)).ConfigureAwait(false);
            }
        }

        private async Task ThisOnResetId(DmtpActor actor, IdChangedEventArgs e)
        {
            await this.ProtectedResetIdAsync(e.NewId).ConfigureAwait(false);
        }
    }
}