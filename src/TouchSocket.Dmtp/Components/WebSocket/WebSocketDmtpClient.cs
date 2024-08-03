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
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// WebSocketDmtpClient
    /// </summary>
    public class WebSocketDmtpClient : SetupConfigObject, IWebSocketDmtpClient
    {
        /// <summary>
        /// WebSocketDmtpClient
        /// </summary>
        public WebSocketDmtpClient()
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

        private readonly SemaphoreSlim m_semaphoreForConnect = new SemaphoreSlim(1, 1);
        private ClientWebSocket m_client;
        private SealedDmtpActor m_dmtpActor;
        private DmtpAdapter m_dmtpAdapter;
        private Func<string, Task<IDmtpActor>> m_findDmtpActor;
        private int m_receiveBufferSize = 1024 * 10;
        private ValueCounter m_receiveCounter;
        private int m_sendBufferSize = 1024 * 10;
        private ValueCounter m_sentCounter;
        private Task m_receiveTask;
        private bool m_online;

        #endregion 字段

        #region 连接

        /// <inheritdoc/>
        public async Task ConnectAsync(int millisecondsTimeout, CancellationToken token)
        {
            await this.m_semaphoreForConnect.WaitTimeAsync(millisecondsTimeout, token).ConfigureAwait(false);
            try
            {
                if (this.Online)
                {
                    return;
                }

                if (this.m_client == null || this.m_client.State != WebSocketState.Open)
                {
                    this.m_client.SafeDispose();
                    this.m_client = new ClientWebSocket();
                    await this.m_client.ConnectAsync(this.RemoteIPHost, token).ConfigureAwait(false);

                    this.m_dmtpActor = new SealedDmtpActor(false)
                    {
                        //OutputSend = this.OnDmtpActorSend,
                        OutputSendAsync = this.OnDmtpActorSendAsync,
                        Routing = this.OnDmtpActorRouting,
                        Handshaking = this.OnDmtpActorHandshaking,
                        Handshaked = this.OnDmtpActorHandshaked,
                        Closing = this.OnDmtpActorClose,
                        Logger = this.Logger,
                        Client = this,
                        FindDmtpActor = this.m_findDmtpActor,
                        CreatedChannel = this.OnDmtpActorCreateChannel
                    }; ;

                    this.m_dmtpAdapter = new DmtpAdapter();
                    this.m_dmtpAdapter.Config(this.Config);

                    this.m_receiveTask = this.BeginReceive();
                    this.m_receiveTask.FireAndForget();
                }

                var option = this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty);

                await this.m_dmtpActor.HandshakeAsync(option.VerifyToken, option.Id, millisecondsTimeout, option.Metadata, token).ConfigureAwait(false);
                this.m_online = true;
            }
            finally
            {
                this.m_semaphoreForConnect.Release();
            }
        }

        #endregion 连接

        /// <inheritdoc/>
        public IDmtpActor DmtpActor => this.m_dmtpActor;

        /// <inheritdoc/>
        public string Id => this.m_dmtpActor?.Id;

        /// <inheritdoc/>
        public bool IsClient => true;

        /// <inheritdoc/>
        public DateTime LastReceivedTime => this.m_receiveCounter.LastIncrement;

        /// <inheritdoc/>
        public DateTime LastSentTime => this.m_sentCounter.LastIncrement;

        /// <inheritdoc/>
        public bool Online => this.m_online;

        /// <inheritdoc/>
        public Protocol Protocol { get; protected set; } = DmtpUtility.DmtpProtocol;

        /// <inheritdoc/>
        public IPHost RemoteIPHost { get; private set; }

        /// <summary>
        /// 发送<see cref="IDmtpActor"/>关闭消息。
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public async Task CloseAsync(string msg)
        {
            if (this.m_dmtpActor != null)
            {
                await this.m_dmtpActor.CloseAsync(msg).ConfigureAwait(false);
            }
            if (this.m_online)
            {
                await this.OnDmtpClosing(new ClosingEventArgs(msg));
                this.Abort(true, msg);
            }
        }

        /// <inheritdoc/>
        public Task ResetIdAsync(string newId)
        {
            return this.m_dmtpActor.ResetIdAsync(newId);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (this.DisposedValue)
            {
                return;
            }
            if (disposing)
            {
                this.Abort(true, $"调用{nameof(Dispose)}");
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override void LoadConfig(TouchSocketConfig config)
        {
            this.RemoteIPHost = config.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty);
            if (this.Resolver.IsRegistered(typeof(IDmtpRouteService)))
            {
                this.m_findDmtpActor = this.Resolver.Resolve<IDmtpRouteService>().FindDmtpActor;
            }
        }

        private void Abort(bool manual, string msg)
        {
            lock (this.m_semaphoreForConnect)
            {
                if (this.m_online)
                {
                    this.m_online = false;
                    this.m_client.SafeDispose();
                    this.DmtpActor.SafeDispose();
                    this.m_dmtpAdapter.SafeDispose();
                    Task.Factory.StartNew(this.PrivateOnDmtpClosed, new ClosedEventArgs(manual, msg));
                }
            }
        }

        private async Task BeginReceive()
        {
            var byteBlock = new ByteBlock(this.m_receiveBufferSize);
            try
            {
                while (true)
                {
                    try
                    {
#if NET6_0_OR_GREATER
                        var result = await this.m_client.ReceiveAsync(byteBlock.TotalMemory, default).ConfigureAwait(false);
#else
                        var segmen = byteBlock.TotalMemory.GetArray();

                        var result = await this.m_client.ReceiveAsync(segmen, default).ConfigureAwait(false);
#endif

                        if (result.Count == 0)
                        {
                            break;
                        }
                        byteBlock.SetLength(result.Count);
                        this.m_receiveCounter.Increment(result.Count);

                        //处理数据
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
                    }
                    catch (Exception ex)
                    {
                        this.Logger?.Exception(ex);
                    }
                    finally
                    {
                        if (byteBlock.Holding || byteBlock.DisposedValue)
                        {
                            byteBlock.Dispose();//释放上个内存
                            byteBlock = new ByteBlock(this.m_receiveBufferSize);
                        }
                        else
                        {
                            byteBlock.Reset();
                            if (this.m_receiveBufferSize > byteBlock.Capacity)
                            {
                                byteBlock.SetCapacity(this.m_receiveBufferSize);
                            }
                        }
                    }
                }

                this.Abort(false, "远程终端主动关闭");
            }
            catch (Exception ex)
            {
                this.Abort(false, ex.Message);
            }
            finally
            {
                byteBlock.Dispose();
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

        private async Task OnDmtpActorClose(DmtpActor actor, string msg)
        {
            await this.OnDmtpClosing(new ClosingEventArgs(msg)).ConfigureAwait(false);
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

        #region 事件触发

        private async Task PrivateOnDmtpClosed(object obj)
        {
            var e = (ClosedEventArgs)obj;
            await this.m_receiveTask.ConfigureAwait(false);

            await this.OnDmtpClosed(e);
        }

        /// <summary>
        /// 已断开连接。
        /// </summary>
        /// <param name="e"></param>
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
        /// 即将握手连接时
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
        /// 当需要转发路由包时
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

        #endregion 事件触发
    }
}