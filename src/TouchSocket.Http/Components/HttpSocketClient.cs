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

using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http.WebSockets;
using TouchSocket.Sockets;

namespace TouchSocket.Http
{
    /// <summary>
    /// http辅助类
    /// </summary>
    public class HttpSocketClient : SocketClient, IHttpSocketClient
    {
        private HttpContext m_httpContext;
        private InternalWebSocket m_webSocket;

        /// <summary>
        /// 构造函数
        /// </summary>
        public HttpSocketClient()
        {
            this.Protocol = Protocol.Http;
        }

        /// <inheritdoc/>
        public IWebSocket WebSocket { get => m_webSocket; }

        /// <inheritdoc/>
        public async Task<bool> SwitchProtocolToWebSocket(HttpContext httpContext)
        {
            if (this.m_webSocket is not null)
            {
                return true;
            }
            if (this.Protocol == Protocol.Http)
            {
                if (WSTools.TryGetResponse(httpContext.Request, httpContext.Response))
                {
                    var args = new HttpContextEventArgs(m_httpContext)
                    {
                        IsPermitOperation = true
                    };

                    var webSocket = new InternalWebSocket(this);

                    await this.PluginManager.RaiseAsync(nameof(IWebSocketHandshakingPlugin.OnWebSocketHandshaking), webSocket, args).ConfigureAwait(false);

                    if (m_httpContext.Response.Responsed)
                    {
                        return false;
                    }
                    if (args.IsPermitOperation)
                    {
                        this.InitWebSocket(webSocket);
                        using (var byteBlock = new ByteBlock())
                        {
                            m_httpContext.Response.Build(byteBlock);
                            await this.DefaultSendAsync(byteBlock).ConfigureAwait(false);
                        }
                        _ = this.PluginManager.RaiseAsync(nameof(IWebSocketHandshakedPlugin.OnWebSocketHandshaked), webSocket, new HttpContextEventArgs(httpContext))
                            .ConfigureAwait(false);
                        return true;
                    }
                    else
                    {
                        m_httpContext.Response.SetStatus(403, "Forbidden");
                        using (var byteBlock = new ByteBlock())
                        {
                            m_httpContext.Response.Build(byteBlock);
                            await this.DefaultSendAsync(byteBlock).ConfigureAwait(false);
                        }

                        this.Close("主动拒绝WebSocket连接");
                    }
                }
                else
                {
                    this.Close("WebSocket连接协议不正确");
                }
            }
            return false;
        }

        /// <inheritdoc/>
        protected override async Task OnConnecting(ConnectingEventArgs e)
        {
            this.SetAdapter(new HttpServerDataHandlingAdapter());
            await base.OnConnecting(e).ConfigureFalseAwait();
        }

        /// <inheritdoc/>
        protected override async Task OnDisconnected(DisconnectEventArgs e)
        {
            if (this.m_webSocket != null)
            {
                this.m_webSocket.IsHandshaked = false;
                _ = this.m_webSocket.TryInputReceiveAsync(null);
            }
            await base.OnDisconnected(e);
        }

        /// <summary>
        /// 当收到到Http请求时。覆盖父类方法将不会触发插件。
        /// </summary>
        protected virtual async Task OnReceivedHttpRequest(HttpRequest request)
        {
            if (this.PluginManager.GetPluginCount(nameof(IHttpPlugin.OnHttpRequest)) > 0)
            {
                var e = new HttpContextEventArgs(m_httpContext);

                await this.PluginManager.RaiseAsync(nameof(IHttpPlugin.OnHttpRequest), this, e).ConfigureFalseAwait();
            }
        }

        /// <inheritdoc/>
        protected override async Task ReceivedData(ReceivedDataEventArgs e)
        {
            if (e.RequestInfo is HttpRequest request)
            {
                m_httpContext ??= new HttpContext(request);
                await this.OnReceivedHttpRequest(request).ConfigureFalseAwait();
                m_httpContext.Response.Reset();
            }
            else if (this.m_webSocket != null && e.RequestInfo is WSDataFrame dataFrame)
            {
                e.Handled = true;
                await this.OnHandleWSDataFrame(dataFrame);
                return;
            }
            await base.ReceivedData(e).ConfigureFalseAwait();
        }

        private void InitWebSocket(InternalWebSocket webSocket)
        {
            this.SetAdapter(new WebSocketDataHandlingAdapter());
            this.Protocol = Protocol.WebSocket;
            this.m_webSocket = webSocket;
        }

        private async Task OnHandleWSDataFrame(WSDataFrame dataFrame)
        {
            if (dataFrame.IsClose && this.GetValue(WebSocketFeature.AutoCloseProperty))
            {
                var msg = dataFrame.PayloadData?.ToString();
                await this.PluginManager.RaiseAsync(nameof(IWebSocketClosingPlugin.OnWebSocketClosing), this.m_webSocket, new MsgPermitEventArgs() { Message = msg });
                this.m_webSocket.Close(msg);
                return;
            }
            if (dataFrame.IsPing && this.GetValue(WebSocketFeature.AutoPongProperty))
            {
                this.m_webSocket.Pong();
                return;
            }
            if (this.m_webSocket.AllowAsyncRead)
            {
                if (await this.m_webSocket.TryInputReceiveAsync(dataFrame))
                {
                    return;
                }
            }
            await this.PluginManager.RaiseAsync(nameof(IWebSocketReceivedPlugin.OnWebSocketReceived), this.m_webSocket, new WSDataFrameEventArgs(dataFrame));
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.m_httpContext?.Request.SafeDispose();
                this.m_httpContext?.Response.SafeDispose();
                this.m_webSocket.SafeDispose();
            }
            base.Dispose(disposing);
        }
    }
}