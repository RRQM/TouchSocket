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

namespace TouchSocket.Http.WebSockets
{
    /// <summary>
    /// WebSocketClient用户终端简单实现。
    /// </summary>
    public class WebSocketClient : WebSocketClientBase
    {
        /// <summary>
        /// 收到WebSocket数据
        /// </summary>
        public WSDataFrameEventHandler<WebSocketClient> Received { get; set; }

        /// <inheritdoc/>
        protected override Task OnReceivedWSDataFrame(WSDataFrameEventArgs e)
        {
            if (this.Received != null)
            {
                return this.Received.Invoke(this, e);
            }
            return base.OnReceivedWSDataFrame(e);
        }
    }

    /// <summary>
    /// WebSocket用户终端。
    /// </summary>
    public class WebSocketClientBase : IWebSocketClient
    {
        /// <summary>
        /// WebSocket用户终端
        /// </summary>
        public WebSocketClientBase()
        {
            m_client = new PrivateHttpClient(this.OnReceivedWSDataFrame);
        }

        #region Connect

        /// <summary>
        /// 连接到ws服务器
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        /// <param name="token"></param>
        /// <exception cref="WebSocketConnectException"></exception>
        public virtual void Connect(int millisecondsTimeout, CancellationToken token)
        {
            try
            {
                this.m_semaphoreSlim.Wait(token);
                if (!this.m_client.Online)
                {
                    this.m_client.Connect(millisecondsTimeout, token);
                }

                var option = this.m_client.Config.GetValue(WebSocketConfigExtension.WebSocketOptionProperty);

                var iPHost = this.m_client.Config.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty);
                var url = iPHost.PathAndQuery;
                var request = WSTools.GetWSRequest(this.m_client.RemoteIPHost.Host, url, option.Version, out var base64Key);
                this.OnHandshaking(new HttpContextEventArgs(new HttpContext(request)))
                    .GetFalseAwaitResult();

                var response = this.m_client.Request(request, millisecondsTimeout: millisecondsTimeout, token: token);
                if (response.StatusCode != 101)
                {
                    throw new WebSocketConnectException($"协议升级失败，信息：{response.StatusMessage}，更多信息请捕获WebSocketConnectException异常，获得HttpContext得知。", new HttpContext(request, response));
                }
                var accept = response.Headers.Get("sec-websocket-accept").Trim();
                if (accept.IsNullOrEmpty() || !accept.Equals(WSTools.CalculateBase64Key(base64Key).Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    this.m_client.MainSocket.SafeDispose();
                    throw new WebSocketConnectException($"WS服务器返回的应答码不正确，更多信息请捕获WebSocketConnectException异常，获得HttpContext得知。", new HttpContext(request, response));
                }

                this.m_client.InitWebSocket();

                response.Flag = true;
                Task.Factory.StartNew(this.PrivateOnHandshaked, new HttpContextEventArgs(new HttpContext(request, response)));
            }
            finally
            {
                this.m_semaphoreSlim.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task ConnectAsync(int millisecondsTimeout, CancellationToken token)
        {
            try
            {
                await this.m_semaphoreSlim.WaitAsync(millisecondsTimeout, token);
                if (!this.m_client.Online)
                {
                    await this.m_client.ConnectAsync(millisecondsTimeout, token);
                }

                var option = this.m_client.Config.GetValue(WebSocketConfigExtension.WebSocketOptionProperty);

                var iPHost = this.m_client.Config.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty);
                var url = iPHost.PathAndQuery;
                var request = WSTools.GetWSRequest(this.m_client.RemoteIPHost.Host, url, option.Version, out var base64Key);

                await this.OnHandshaking(new HttpContextEventArgs(new HttpContext(request)));

                var response = await this.m_client.RequestAsync(request, millisecondsTimeout: millisecondsTimeout, token: token);
                if (response.StatusCode != 101)
                {
                    throw new WebSocketConnectException($"协议升级失败，信息：{response.StatusMessage}，更多信息请捕获WebSocketConnectException异常，获得HttpContext得知。", new HttpContext(request, response));
                }
                var accept = response.Headers.Get("sec-websocket-accept").Trim();
                if (accept.IsNullOrEmpty() || !accept.Equals(WSTools.CalculateBase64Key(base64Key).Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    this.m_client.MainSocket.SafeDispose();
                    throw new WebSocketConnectException($"WS服务器返回的应答码不正确，更多信息请捕获WebSocketConnectException异常，获得HttpContext得知。", new HttpContext(request, response));
                }

                this.m_client.InitWebSocket();

                response.Flag = true;
                _ = Task.Factory.StartNew(this.PrivateOnHandshaked, new HttpContextEventArgs(new HttpContext(request, response)));
            }
            finally
            {
                this.m_semaphoreSlim.Release();
            }
        }

        #endregion Connect

        #region 字段

        private readonly SemaphoreSlim m_semaphoreSlim = new SemaphoreSlim(1, 1);
        private PrivateHttpClient m_client;

        #endregion 字段

        ///// <inheritdoc/>
        //public IWebSocket WebSocket => m_client.WebSocket;

        #region 事件

        /// <inheritdoc/>
        public bool AllowAsyncRead { get => this.m_client.WebSocket.AllowAsyncRead; set => this.m_client.WebSocket.AllowAsyncRead = value; }

        /// <inheritdoc/>
        public IHttpClientBase Client => this.m_client;

        /// <inheritdoc/>
        public TouchSocketConfig Config => ((IConfigObject)this.m_client).Config;

        /// <summary>
        /// 表示完成握手后。
        /// </summary>
        public HttpContextEventHandler<WebSocketClientBase> Handshaked { get; set; }

        /// <summary>
        /// 表示在即将握手连接时。
        /// </summary>
        public HttpContextEventHandler<WebSocketClientBase> Handshaking { get; set; }

        /// <inheritdoc/>
        public bool IsHandshaked => this.m_client.WebSocket.IsHandshaked;

        /// <inheritdoc/>
        public DateTime LastReceivedTime => ((IClient)this.m_client).LastReceivedTime;

        /// <inheritdoc/>
        public DateTime LastSendTime => ((IClient)this.m_client).LastSendTime;

        /// <inheritdoc/>
        public ILog Logger { get => ((ILoggerObject)this.m_client).Logger; set => ((ILoggerObject)this.m_client).Logger = value; }

        /// <inheritdoc/>
        public IPluginManager PluginManager => ((IPluginObject)this.m_client).PluginManager;

        /// <inheritdoc/>
        public Protocol Protocol { get => ((IClient)this.m_client).Protocol; set => ((IClient)this.m_client).Protocol = value; }

        /// <inheritdoc/>
        public IResolver Resolver => ((IResolverObject)this.m_client).Resolver;

        /// <inheritdoc/>
        public string Version => this.m_client.WebSocket.Version;

        /// <summary>
        /// 表示完成握手后。
        /// </summary>
        /// <param name="e"></param>
        protected virtual async Task OnHandshaked(HttpContextEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }
            if (this.Handshaked != null)
            {
                await this.Handshaked.Invoke(this, e);
                if (e.Handled)
                {
                    return;
                }
            }
            await this.m_client.PluginManager.RaiseAsync(nameof(IWebSocketHandshakedPlugin.OnWebSocketHandshaked), this, e);
        }

        /// <summary>
        /// 表示在即将握手连接时。
        /// </summary>
        /// <param name="e"></param>
        protected virtual async Task OnHandshaking(HttpContextEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }
            if (this.Handshaking != null)
            {
                await this.Handshaking.Invoke(this, e).ConfigureFalseAwait();
                if (e.Handled)
                {
                    return;
                }
            }
            await this.m_client.PluginManager.RaiseAsync(nameof(IWebSocketHandshakingPlugin.OnWebSocketHandshaking), this, e).ConfigureFalseAwait();
        }

        private Task PrivateOnHandshaked(object obj)
        {
            return this.OnHandshaked((HttpContextEventArgs)obj);
        }

        #endregion 事件

        /// <inheritdoc/>
        public void Close(string msg)
        {
            this.m_client.WebSocket.Close(msg);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            ((IDisposable)this.m_client).Dispose();
        }

        /// <inheritdoc/>
        public TValue GetValue<TValue>(IDependencyProperty<TValue> dp)
        {
            return ((IDependencyObject)this.m_client).GetValue(dp);
        }

        /// <inheritdoc/>
        public bool HasValue<TValue>(IDependencyProperty<TValue> dp)
        {
            return ((IDependencyObject)this.m_client).HasValue(dp);
        }

        /// <inheritdoc/>
        public void Ping()
        {
            this.m_client.WebSocket.Ping();
        }

        /// <inheritdoc/>
        public Task PingAsync()
        {
            return this.m_client.WebSocket.PingAsync();
        }

        /// <inheritdoc/>
        public void Pong()
        {
            this.m_client.WebSocket.Pong();
        }

        /// <inheritdoc/>
        public Task PongAsync()
        {
            return this.m_client.WebSocket.PongAsync();
        }

        /// <inheritdoc/>
        public Task<WebSocketReceiveResult> ReadAsync(CancellationToken token)
        {
            return this.m_client.WebSocket.ReadAsync(token);
        }

        /// <inheritdoc/>
        public DependencyObject RemoveValue<TValue>(IDependencyProperty<TValue> dp)
        {
            return ((IDependencyObject)this.m_client).RemoveValue(dp);
        }

        /// <inheritdoc/>
        public void Send(WSDataFrame dataFrame, bool endOfMessage = true)
        {
            this.m_client.WebSocket.Send(dataFrame, endOfMessage);
        }

        /// <inheritdoc/>
        public void Send(string text, bool endOfMessage = true)
        {
            this.m_client.WebSocket.Send(text, endOfMessage);
        }

        /// <inheritdoc/>
        public void Send(byte[] buffer, int offset, int length, bool endOfMessage = true)
        {
            this.m_client.WebSocket.Send(buffer, offset, length, endOfMessage);
        }

        /// <inheritdoc/>
        public void Send(ByteBlock byteBlock, bool endOfMessage = true)
        {
            this.m_client.WebSocket.Send(byteBlock, endOfMessage);
        }

        public void Send(byte[] buffer, bool endOfMessage = true)
        {
            this.m_client.WebSocket.Send(buffer, endOfMessage);
        }

        /// <inheritdoc/>
        public Task SendAsync(WSDataFrame dataFrame, bool endOfMessage = true)
        {
            return this.m_client.WebSocket.SendAsync(dataFrame, endOfMessage);
        }

        /// <inheritdoc/>
        public Task SendAsync(string text, bool endOfMessage = true)
        {
            return this.m_client.WebSocket.SendAsync(text, endOfMessage);
        }

        /// <inheritdoc/>
        public Task SendAsync(byte[] buffer, bool endOfMessage = true)
        {
            return this.m_client.WebSocket.SendAsync(buffer, endOfMessage);
        }

        /// <inheritdoc/>
        public Task SendAsync(byte[] buffer, int offset, int length, bool endOfMessage = true)
        {
            return this.m_client.WebSocket.SendAsync(buffer, offset, length, endOfMessage);
        }

        /// <inheritdoc/>
        public void Setup(TouchSocketConfig config)
        {
            ((ISetupConfigObject)this.m_client).Setup(config);
        }

        /// <inheritdoc/>
        public Task SetupAsync(TouchSocketConfig config)
        {
            return ((ISetupConfigObject)this.m_client).SetupAsync(config);
        }

        /// <inheritdoc/>
        public DependencyObject SetValue<TValue>(IDependencyProperty<TValue> dp, TValue value)
        {
            return ((IDependencyObject)this.m_client).SetValue(dp, value);
        }

        /// <inheritdoc/>
        public bool TryGetValue<TValue>(IDependencyProperty<TValue> dp, out TValue value)
        {
            return ((IDependencyObject)this.m_client).TryGetValue(dp, out value);
        }

        /// <inheritdoc/>
        public bool TryRemoveValue<TValue>(IDependencyProperty<TValue> dp, out TValue value)
        {
            return ((IDependencyObject)this.m_client).TryRemoveValue(dp, out value);
        }

#if ValueTask
        /// <inheritdoc/>
        public async ValueTask<WebSocketReceiveResult> ValueReadAsync(CancellationToken token)
        {
            return await this.m_client.WebSocket.ValueReadAsync(token);
        }
#endif

        /// <summary>
        /// 当收到WS数据时。
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected virtual async Task OnReceivedWSDataFrame(WSDataFrameEventArgs e)
        {
            await this.m_client.PluginManager.RaiseAsync(nameof(IWebSocketReceivedPlugin.OnWebSocketReceived), this, e).ConfigureFalseAwait();
        }

        #region InternalClass

        private class PrivateHttpClient : HttpClientBase
        {
            private readonly InternalWebSocket m_webSocket;

            private Func<WSDataFrameEventArgs, Task> m_func;

            public PrivateHttpClient(Func<WSDataFrameEventArgs, Task> func)
            {
                this.m_webSocket = new InternalWebSocket(this);
                this.m_func = func;
            }

            public InternalWebSocket WebSocket { get => m_webSocket; }

            public void InitWebSocket()
            {
                this.SetAdapter(new WebSocketDataHandlingAdapter());
                this.Protocol = Protocol.WebSocket;
                this.m_webSocket.IsHandshaked = true;
            }

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            /// <param name="e"></param>
            protected override async Task OnDisconnected(DisconnectEventArgs e)
            {
                this.m_webSocket.IsHandshaked = false;
                if (this.m_webSocket.AllowAsyncRead)
                {
                    _ = this.m_webSocket.TryInputReceiveAsync(null);
                }
                await base.OnDisconnected(e);
            }

            protected override async Task ReceivedData(ReceivedDataEventArgs e)
            {
                if (this.m_webSocket.IsHandshaked)
                {
                    var dataFrame = (WSDataFrame)e.RequestInfo;

                    if (this.m_webSocket.AllowAsyncRead)
                    {
                        if (await this.m_webSocket.TryInputReceiveAsync(dataFrame).ConfigureFalseAwait())
                        {
                            return;
                        }
                    }
                    await this.m_func(new WSDataFrameEventArgs(dataFrame));
                }
                else
                {
                    if (e.RequestInfo is HttpResponse response)
                    {
                        response.Flag = false;
                        await base.ReceivedData(e);
                        SpinWait.SpinUntil(() =>
                        {
                            return (bool)response.Flag;
                        }, 3000);
                    }
                }
                await base.ReceivedData(e);
            }
        }

        #endregion InternalClass
    }
}