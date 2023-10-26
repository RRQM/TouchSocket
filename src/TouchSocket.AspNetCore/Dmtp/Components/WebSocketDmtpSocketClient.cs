//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
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
    /// WebSocketDmtpSocketClient
    /// </summary>
    public class WebSocketDmtpSocketClient : BaseSocket, IWebSocketDmtpSocketClient
    {
        /// <summary>
        /// WebSocketDmtpSocketClient
        /// </summary>
        public WebSocketDmtpSocketClient()
        {
            this.m_receiveCounter = new ValueCounter
            {
                Period = TimeSpan.FromSeconds(1),
                OnPeriod = this.OnReceivePeriod
            };
            this.m_sendCounter = new ValueCounter
            {
                Period = TimeSpan.FromSeconds(1),
                OnPeriod = this.OnSendPeriod
            };
        }

        #region 字段

        private WebSocket m_client;
        private DmtpActor m_dmtpActor;
        private TcpDmtpAdapter m_dmtpAdapter;
        private WebSocketDmtpService m_service;
        private ValueCounter m_receiveCounter;
        private ValueCounter m_sendCounter;
        private int m_receiveBufferSize = 1024 * 10;
        private int m_sendBufferSize = 1024 * 10;
        private readonly SemaphoreSlim m_semaphoreForSend = new SemaphoreSlim(1, 1);

        #endregion 字段

        /// <inheritdoc/>
        public override int ReceiveBufferSize => this.m_receiveBufferSize;

        /// <inheritdoc/>
        public override int SendBufferSize => this.m_sendBufferSize;

        /// <inheritdoc/>
        public TouchSocketConfig Config { get; private set; }

        /// <inheritdoc/>
        public IContainer Container { get; private set; }

        /// <inheritdoc/>
        public HttpContext HttpContext { get; private set; }

        /// <inheritdoc/>
        public string Id { get; private set; }

        /// <inheritdoc/>
        public bool IsHandshaked => this.DmtpActor.IsHandshaked;

        /// <inheritdoc/>
        public DateTime LastReceivedTime => this.m_receiveCounter.LastIncrement;

        /// <inheritdoc/>
        public DateTime LastSendTime => this.m_sendCounter.LastIncrement;

        /// <inheritdoc/>
        public IPluginsManager PluginsManager { get; private set; }

        /// <inheritdoc/>
        public Protocol Protocol { get; set; } = DmtpUtility.DmtpProtocol;

        /// <inheritdoc/>
        public IWebSocketDmtpService Service { get => this.m_service; }

        /// <inheritdoc/>
        public IDmtpActor DmtpActor { get => this.m_dmtpActor; }

        /// <inheritdoc/>
        public int VerifyTimeout => this.Config.GetValue(DmtpConfigExtension.VerifyTimeoutProperty);

        /// <inheritdoc/>
        public string VerifyToken => this.Config.GetValue(DmtpConfigExtension.VerifyTokenProperty);

        /// <summary>
        /// 关闭通信
        /// </summary>
        /// <param name="msg"></param>
        public void Close(string msg)
        {
            this.BreakOut(msg, true);
        }

        /// <inheritdoc/>
        public void ResetId(string newId)
        {
            if (string.IsNullOrEmpty(newId))
            {
                throw new ArgumentException($"“{nameof(newId)}”不能为 null 或空。", nameof(newId));
            }

            if (this.Id == newId)
            {
                return;
            }

            this.DmtpActor.ResetId(newId);
            this.DirectResetId(newId);
        }

        internal void InternalSetConfig(TouchSocketConfig config)
        {
            this.Config = config;
            this.m_dmtpAdapter = new TcpDmtpAdapter()
            {
                ReceivedCallBack = this.PrivateHandleReceivedData,
                Logger = this.Logger
            };

            this.m_dmtpAdapter.Config(config);
        }

        internal void InternalSetContainer(IContainer container)
        {
            this.Container = container;
            this.Logger ??= container.Resolve<ILog>();
        }

        internal void InternalSetId(string id)
        {
            this.Id = id;
        }

        internal void InternalSetPluginsManager(IPluginsManager pluginsManager)
        {
            this.PluginsManager = pluginsManager;
        }

        internal void InternalSetService(WebSocketDmtpService service)
        {
            this.m_service = service;
        }

        private void OnReceivePeriod(long value)
        {
            this.m_receiveBufferSize = TouchSocketUtility.HitBufferLength(value);
        }

        private void OnSendPeriod(long value)
        {
            this.m_sendBufferSize = TouchSocketUtility.HitBufferLength(value);
        }

        #region 内部委托绑定

        private Task OnDmtpActorClose(DmtpActor actor, string msg)
        {
            this.BreakOut(msg, true);
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
            await this.OnHandshaking(e);
        }

        private Task OnDmtpActorRouting(DmtpActor actor, PackageRouterEventArgs e)
        {
            return this.OnRouting(e);
        }

        private void OnDmtpActorSend(DmtpActor actor, ArraySegment<byte>[] transferBytes)
        {
            try
            {
                this.m_semaphoreForSend.Wait();
                for (var i = 0; i < transferBytes.Length; i++)
                {
                    Task task;
                    if (i == transferBytes.Length - 1)
                    {
                        task = this.m_client.SendAsync(transferBytes[i], WebSocketMessageType.Binary, true, CancellationToken.None);
                    }
                    else
                    {
                        task = this.m_client.SendAsync(transferBytes[i], WebSocketMessageType.Binary, false, CancellationToken.None);
                    }
                    task.GetFalseAwaitResult();
                    this.m_sendCounter.Increment(transferBytes[i].Count);
                }
            }
            finally
            {
                this.m_semaphoreForSend.Release();
            }
        }

        private async Task OnDmtpActorSendAsync(DmtpActor actor, ArraySegment<byte>[] transferBytes)
        {
            try
            {
                await this.m_semaphoreForSend.WaitAsync();
                for (var i = 0; i < transferBytes.Length; i++)
                {
                    if (i == transferBytes.Length - 1)
                    {
                        await this.m_client.SendAsync(transferBytes[i], WebSocketMessageType.Binary, true, CancellationToken.None);
                    }
                    else
                    {
                        await this.m_client.SendAsync(transferBytes[i], WebSocketMessageType.Binary, false, CancellationToken.None);
                    }
                    this.m_sendCounter.Increment(transferBytes[i].Count);
                }
            }
            finally
            {
                this.m_semaphoreForSend.Release();
            }
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

            await this.PluginsManager.RaiseAsync(nameof(IDmtpCreateChannelPlugin.OnCreateChannel), this, e);
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
            await this.PluginsManager.RaiseAsync(nameof(IDmtpHandshakedPlugin.OnDmtpHandshaked), this, e);
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
            await this.PluginsManager.RaiseAsync(nameof(IDmtpHandshakingPlugin.OnDmtpHandshaking), this, e);
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
            await this.PluginsManager.RaiseAsync(nameof(IDmtpRoutingPlugin.OnDmtpRouting), this, e);
        }

        #endregion 事件

        #region Receiver

        /// <summary>
        /// 不支持该功能
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public IReceiver CreateReceiver()
        {
            throw new NotSupportedException("不支持该功能");
        }

        /// <summary>
        /// 不支持该功能
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        public void ClearReceiver()
        {
            throw new NotSupportedException("不支持该功能");
        }

        #endregion Receiver

        internal void SetDmtpActor(DmtpActor actor)
        {
            actor.IdChanged = this.ThisOnResetId;
            actor.OutputSendAsync = this.OnDmtpActorSendAsync;
            actor.OutputSend = this.OnDmtpActorSend;
            actor.Client = this;
            actor.Closed = this.OnDmtpActorClose;
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
            this.HttpContext = context;
            try
            {
                while (true)
                {
                    using (var byteBlock = new ByteBlock(this.ReceiveBufferSize))
                    {
                        var result = await this.m_client.ReceiveAsync(new ArraySegment<byte>(byteBlock.Buffer, 0, byteBlock.Capacity), default);
                        if (result.Count == 0)
                        {
                            break;
                        }
                        byteBlock.SetLength(result.Count);
                        this.m_receiveCounter.Increment(result.Count);

                        this.m_dmtpAdapter.ReceivedInput(byteBlock);
                    }
                }

                this.BreakOut("远程终端主动关闭", false);
            }
            catch (Exception ex)
            {
                this.BreakOut(ex.Message, false);
            }
        }

        /// <summary>
        /// 直接重置Id。
        /// </summary>
        /// <param name="newId"></param>
        /// <exception cref="Exception"></exception>
        /// <exception cref="ClientNotFindException"></exception>
        protected void DirectResetId(string newId)
        {
            var oldId = this.Id;
            if (this.m_service.TryRemove(this.Id, out var socketClient))
            {
                socketClient.Id = newId;
                if (this.m_service.TryAdd(this.Id, socketClient))
                {
                    if (this.PluginsManager.Enable)
                    {
                        var e = new IdChangedEventArgs(oldId, newId);
                        this.PluginsManager.Raise(nameof(IIdChangedPlugin.OnIdChanged), socketClient, e);
                    }
                    return;
                }
                else
                {
                    socketClient.Id = oldId;
                    if (this.m_service.TryAdd(socketClient.Id, socketClient))
                    {
                        throw new Exception("Id重复");
                    }
                    else
                    {
                        socketClient.Close("修改新Id时操作失败，且回退旧Id时也失败。");
                    }
                }
            }
            else
            {
                throw new ClientNotFindException(TouchSocketAspNetCoreResource.ClientNotFind.GetDescription(oldId));
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            this.BreakOut($"{nameof(Dispose)}断开链接", true);
        }

        private void BreakOut(string msg, bool manual)
        {
            lock (this.SyncRoot)
            {
                if (this.DisposedValue)
                {
                    return;
                }

                base.Dispose(true);
                this.m_client.SafeDispose();
                this.DmtpActor.SafeDispose();

                if (this.m_service.TryRemove(this.Id, out _))
                {
                    if (this.PluginsManager.Enable)
                    {
                        this.PluginsManager.RaiseAsync(nameof(ITcpDisconnectedPlugin.OnTcpDisconnected), this, new DisconnectEventArgs(manual, msg));
                    }
                }
            }
        }

        private void PrivateHandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            var message = (DmtpMessage)requestInfo;
            if (!this.m_dmtpActor.InputReceivedData(message).GetFalseAwaitResult())
            {
                this.PluginsManager.Raise(nameof(IDmtpReceivedPlugin.OnDmtpReceived), this, new DmtpMessageEventArgs(message));
            }
        }

        private Task ThisOnResetId(DmtpActor actor, IdChangedEventArgs e)
        {
            this.DirectResetId(e.NewId);
            return EasyTask.CompletedTask;
        }
    }
}