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
using TouchSocket.Sockets;

namespace TouchSocket.Http.WebSockets
{
    /// <summary>
    /// WebSocket用户终端。
    /// </summary>
    public abstract class WebSocketClientBase : HttpClientBase
    {
        /// <summary>
        /// WebSocket用户终端
        /// </summary>
        public WebSocketClientBase()
        {
            this.m_webSocket = new InternalWebSocket(this);
        }

        /// <inheritdoc/>
        public override bool Online => this.m_webSocket.Online;

        #region Connect

        /// <inheritdoc/>
        public virtual async Task ConnectAsync(int millisecondsTimeout, CancellationToken token)
        {
            await this.m_semaphoreSlim.WaitTimeAsync(millisecondsTimeout, token).ConfigureFalseAwait();

            try
            {
                if (!base.Online)
                {
                    await this.TcpConnectAsync(millisecondsTimeout, token).ConfigureFalseAwait();
                }

                var option = this.Config.GetValue(WebSocketConfigExtension.WebSocketOptionProperty);

                var request = WSTools.GetWSRequest(this, option.Version, out var base64Key);

                await this.OnWebSocketHandshaking(new HttpContextEventArgs(new HttpContext(request))).ConfigureFalseAwait();

                using (var responseResult = await this.ProtectedRequestAsync(request, millisecondsTimeout, token).ConfigureFalseAwait())
                {
                    var response = responseResult.Response;
                    if (response.StatusCode != 101)
                    {
                        throw new WebSocketConnectException($"协议升级失败，信息：{response.StatusMessage}，更多信息请捕获WebSocketConnectException异常，获得HttpContext得知。", new HttpContext(request, response));
                    }
                    var accept = response.Headers.Get("sec-websocket-accept").Trim();
                    if (accept.IsNullOrEmpty() || !accept.Equals(WSTools.CalculateBase64Key(base64Key).Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        this.MainSocket.SafeDispose();
                        throw new WebSocketConnectException($"WS服务器返回的应答码不正确，更多信息请捕获WebSocketConnectException异常，获得HttpContext得知。", new HttpContext(request, response));
                    }

                    this.InitWebSocket();

                    _ = Task.Factory.StartNew(this.PrivateOnHandshaked, new HttpContextEventArgs(new HttpContext(request, response)));
                }
            }
            finally
            {
                this.m_semaphoreSlim.Release();
            }
        }

        #endregion Connect

        #region 字段

        private readonly SemaphoreSlim m_semaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly InternalWebSocket m_webSocket;

        #endregion 字段

        #region 事件

        protected virtual Task OnWebSocketClosed(ClosedEventArgs e)
        {
            return EasyTask.CompletedTask;
        }

        protected virtual Task OnWebSocketClosing(ClosedEventArgs e)
        {
            return EasyTask.CompletedTask;
        }

        /// <summary>
        /// 表示完成握手后。
        /// </summary>
        /// <param name="e"></param>
        protected virtual Task OnWebSocketHandshaked(HttpContextEventArgs e)
        {
            return EasyTask.CompletedTask;
        }

        /// <summary>
        /// 表示在即将握手连接时。
        /// </summary>
        /// <param name="e"></param>
        protected virtual Task OnWebSocketHandshaking(HttpContextEventArgs e)
        {
            return EasyTask.CompletedTask;
        }

        private Task PrivateOnHandshaked(object obj)
        {
            return this.OnWebSocketHandshaked((HttpContextEventArgs)obj);
        }

        private async Task PrivateWebSocketClosed(ClosedEventArgs e)
        {
            this.m_webSocket.Online = false;
            if (this.m_webSocket.AllowAsyncRead)
            {
                await this.m_webSocket.Complete(e.Message).ConfigureFalseAwait();
            }
            await this.OnWebSocketClosed(e).ConfigureFalseAwait();
        }

        private Task PrivateWebSocketClosing(ClosedEventArgs e)
        {
            return this.OnWebSocketClosing(e);
        }

        private async Task PrivateWebSocketReceived(WSDataFrame dataFrame)
        {
            if (dataFrame.IsClose)
            {
                var msg = dataFrame.PayloadData?.ToString();
                await this.PrivateWebSocketClosing(new ClosedEventArgs(false, msg)).ConfigureFalseAwait();
                await this.m_webSocket.CloseAsync(msg).ConfigureFalseAwait();
                return;
            }
            if (this.m_webSocket.AllowAsyncRead)
            {
                await this.m_webSocket.InputReceiveAsync(dataFrame).ConfigureFalseAwait();
                return;
            }

            await this.OnWebSocketReceived(new WSDataFrameEventArgs(dataFrame)).ConfigureFalseAwait();
        }

        #endregion 事件

        /// <summary>
        /// 当收到WS数据时。
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected abstract Task OnWebSocketReceived(WSDataFrameEventArgs e);

        private void InitWebSocket()
        {
            var adapter = new WebSocketDataHandlingAdapter();
            base.SetWarpAdapter(adapter);
            this.SetAdapter(adapter);
            this.Protocol = Protocol.WebSocket;
            this.m_webSocket.Online = true;
        }

        #region Properties

        /// <summary>
        /// 实际通讯的WebSocket。
        /// </summary>
        protected IWebSocket WebSocket => this.m_webSocket;

        #endregion Properties

        #region Override

        /// <inheritdoc/>
        protected override async Task OnTcpClosed(ClosedEventArgs e)
        {
            await this.PrivateWebSocketClosed(e).ConfigureFalseAwait();
            await base.OnTcpClosed(e).ConfigureFalseAwait();
        }

        /// <inheritdoc/>
        protected override async Task OnTcpReceived(ReceivedDataEventArgs e)
        {
            if (this.m_webSocket.Online)
            {
                var dataFrame = (WSDataFrame)e.RequestInfo;

                await this.PrivateWebSocketReceived(dataFrame).ConfigureFalseAwait();
            }
            else
            {
                if (e.RequestInfo is HttpResponse)
                {
                    await base.OnTcpReceived(e).ConfigureFalseAwait();
                }
            }
            await base.OnTcpReceived(e).ConfigureFalseAwait();
        }

        #endregion Override
    }
}