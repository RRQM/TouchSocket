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
        private WebSocket m_client;
        private DmtpActor m_smtpActor;
        private TcpDmtpAdapter m_smtpAdapter;
        private WebSocketDmtpService m_service;

        /// <inheritdoc/>
        public int BufferLength { get; private set; }

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
        public DateTime LastReceivedTime { get; private set; }

        /// <inheritdoc/>
        public DateTime LastSendTime { get; private set; }

        /// <summary>
        /// 未实现
        /// </summary>
        public Func<ByteBlock, bool> OnHandleRawBuffer { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <summary>
        /// 未实现
        /// </summary>
        public Func<ByteBlock, IRequestInfo, bool> OnHandleReceivedData { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <inheritdoc/>
        public IPluginsManager PluginsManager { get; private set; }

        /// <inheritdoc/>
        public Protocol Protocol { get; set; } = DmtpUtility.DmtpProtocol;

        /// <inheritdoc/>
        public IWebSocketDmtpService Service { get => this.m_service; }

        /// <inheritdoc/>
        public IDmtpActor DmtpActor { get => this.m_smtpActor; }

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

        /// <inheritdoc/>
        public int SetBufferLength(int value)
        {
            this.BufferLength = value;
            return this.BufferLength;
        }

        internal void InternalSetConfig(TouchSocketConfig config)
        {
            this.Config = config;
            this.m_smtpAdapter = new TcpDmtpAdapter()
            {
                ReceivedCallBack = PrivateHandleReceivedData,
                Logger = this.Logger
            };

            this.m_smtpAdapter.Config(config);
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

        #region 内部委托绑定

        private void OnDmtpActorClose(DmtpActor actor, string arg2)
        {
            this.Close(arg2);
        }

        private void OnDmtpActorCreateChannel(DmtpActor actor, CreateChannelEventArgs e)
        {
            this.OnCreateChannel(e);
            if (e.Handled)
            {
                return;
            }

            this.PluginsManager.Raise(nameof(IDmtpCreateChannelPlugin.OnCreateChannel), this, e);
        }

        private void OnDmtpActorHandshaked(DmtpActor actor, DmtpVerifyEventArgs e)
        {
            this.OnHandshaked(e);
            if (e.Handled)
            {
                return;
            }
            if (this.PluginsManager.Raise(nameof(IDmtpHandshakedPlugin.OnDmtpHandshaked), this, e))
            {
                return;
            }
        }

        private void OnDmtpActorHandshaking(DmtpActor actor, DmtpVerifyEventArgs e)
        {
            if (e.Token == this.VerifyToken)
            {
                e.IsPermitOperation = true;
            }
            else
            {
                e.Message = "Token不受理";
            }
            if (this.PluginsManager.Raise(nameof(IDmtpHandshakingPlugin.OnDmtpHandshaking), this, e))
            {
                return;
            }
            this.OnHandshaking(e);
        }

        private void OnDmtpActorRouting(DmtpActor actor, PackageRouterEventArgs e)
        {
            this.OnRouting(e);
            if (e.Handled)
            {
                return;
            }
            if (this.PluginsManager.Raise(nameof(IDmtpRoutingPlugin.OnDmtpRouting), this, e))
            {
                return;
            }
        }

        private void OnDmtpActorSend(DmtpActor actor, ArraySegment<byte>[] transferBytes)
        {
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
                task.ConfigureAwait(false);
                task.GetAwaiter().GetResult();
            }
            this.LastSendTime = DateTime.Now;
        }

        #endregion 内部委托绑定

        #region 事件

        /// <summary>
        /// 当创建通道
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnCreateChannel(CreateChannelEventArgs e)
        {
        }

        /// <summary>
        /// 在完成握手连接时
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnHandshaked(DmtpVerifyEventArgs e)
        {
        }

        /// <summary>
        /// 在验证Token时
        /// </summary>
        /// <param name="e">参数</param>
        protected virtual void OnHandshaking(DmtpVerifyEventArgs e)
        {
        }

        /// <summary>
        /// 在需要转发路由包时。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnRouting(PackageRouterEventArgs e)
        {
        }

        #endregion 事件

        internal void SetDmtpActor(DmtpActor actor)
        {
            actor.OnResetId = this.ThisOnResetId;
            actor.OutputSend = this.OnDmtpActorSend;
            actor.Client = this;
            actor.OnClose = this.OnDmtpActorClose;
            actor.OnRouting = this.OnDmtpActorRouting;
            actor.OnHandshaked = this.OnDmtpActorHandshaked;
            actor.OnHandshaking = this.OnDmtpActorHandshaking;
            actor.OnCreateChannel = this.OnDmtpActorCreateChannel;
            actor.Logger = this.Logger;
            this.m_smtpActor = actor;
        }

        internal async Task Start(WebSocket webSocket, HttpContext context)
        {
            this.m_client = webSocket;
            this.HttpContext = context;
            try
            {
                while (true)
                {
                    using (var byteBlock = new ByteBlock(this.BufferLength))
                    {
                        var result = await this.m_client.ReceiveAsync(new ArraySegment<byte>(byteBlock.Buffer, 0, byteBlock.Capacity), default);
                        if (result.Count == 0)
                        {
                            break;
                        }
                        byteBlock.SetLength(result.Count);
                        this.LastReceivedTime = DateTime.Now;

                        this.m_smtpAdapter.ReceivedInput(byteBlock);
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
            if (!this.m_smtpActor.InputReceivedData(message))
            {
                if (this.PluginsManager.Enable)
                {
                    this.PluginsManager.Raise(nameof(IDmtpReceivedPlugin.OnDmtpReceived), this, new DmtpMessageEventArgs(message));
                }
            }
        }

        private void ThisOnResetId(DmtpActor actor, WaitSetId waitSetId)
        {
            this.DirectResetId(waitSetId.NewId);
        }
    }
}