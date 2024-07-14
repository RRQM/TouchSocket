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
using TouchSocket.Resources;
using TouchSocket.Sockets;

namespace TouchSocket.Http
{
    /// <summary>
    /// http辅助类
    /// </summary>
    public partial class HttpSessionClient : TcpSessionClientBase, IHttpSessionClient
    {
        private InternalWebSocket m_webSocket;

        /// <inheritdoc/>
        public IWebSocket WebSocket => this.m_webSocket;

        #region 事件

        private Task PrivateWebSocketHandshaking(IWebSocket webSocket, HttpContextEventArgs e)
        {
            return this.OnWebSocketHandshaking(webSocket, e);
        }

        private Task PrivateWebSocketHandshaked(IWebSocket webSocket, HttpContextEventArgs e)
        {
            return this.OnWebSocketHandshaked(webSocket, e);
        }

        private async Task PrivateWebSocketReceived(WSDataFrame dataFrame)
        {
            if (dataFrame.IsClose && this.GetValue(WebSocketFeature.AutoCloseProperty))
            {
                var msg = dataFrame.PayloadData?.ToString();
                await this.PrivateWebSocketClosing(new MsgEventArgs(msg)).ConfigureFalseAwait();
                await this.m_webSocket.CloseAsync(msg).ConfigureFalseAwait();
                return;
            }
            if (dataFrame.IsPing && this.GetValue(WebSocketFeature.AutoPongProperty))
            {
                await this.m_webSocket.PongAsync();
                return;
            }

            if (this.m_webSocket.AllowAsyncRead)
            {
                await this.m_webSocket.InputReceiveAsync(dataFrame).ConfigureFalseAwait();
                return;
            }

            await this.OnWebSocketReceived(this.m_webSocket, new WSDataFrameEventArgs(dataFrame)).ConfigureFalseAwait();
        }

        private Task PrivateWebSocketClosing(MsgEventArgs e)
        {
            return this.OnWebSocketClosing(this.m_webSocket, e);
        }

        private async Task PrivateWebSocketClosed(MsgEventArgs e)
        {
            this.m_webSocket.Online = false;
            if (this.m_webSocket.AllowAsyncRead)
            {
                await this.m_webSocket.Complete(e.Message).ConfigureFalseAwait();
            }
            await this.OnWebSocketClosed(this.m_webSocket, e).ConfigureFalseAwait();
        }

        protected virtual Task OnWebSocketHandshaking(IWebSocket webSocket, HttpContextEventArgs e)
        {
            return this.PluginManager.RaiseAsync(typeof(IWebSocketHandshakingPlugin), webSocket, e);
        }

        protected virtual Task OnWebSocketHandshaked(IWebSocket webSocket, HttpContextEventArgs e)
        {
            return Task.Run(() => this.PluginManager.RaiseAsync(typeof(IWebSocketHandshakedPlugin), webSocket, e));
        }

        protected virtual Task OnWebSocketReceived(IWebSocket webSocket, WSDataFrameEventArgs e)
        {
            return this.PluginManager.RaiseAsync(typeof(IWebSocketReceivedPlugin), webSocket, e);
        }

        protected virtual Task OnWebSocketClosing(IWebSocket webSocket, MsgEventArgs e)
        {
            return this.PluginManager.RaiseAsync(typeof(IWebSocketClosingPlugin), webSocket, e);
        }

        protected virtual Task OnWebSocketClosed(IWebSocket webSocket, MsgEventArgs e)
        {
            return this.PluginManager.RaiseAsync(typeof(IWebSocketClosedPlugin), webSocket, e);
        }

        #endregion 事件

        /// <inheritdoc/>
        public async Task<bool> SwitchProtocolToWebSocketAsync(HttpContext httpContext)
        {
            if (this.m_webSocket is not null)
            {
                return true;
            }
            if (this.Protocol == Protocol.Http)
            {
                if (WSTools.TryGetResponse(httpContext.Request, httpContext.Response))
                {
                    var e = new HttpContextEventArgs(this.m_httpContext)
                    {
                        IsPermitOperation = true
                    };

                    var webSocket = new InternalWebSocket(this);

                    await this.PrivateWebSocketHandshaking(webSocket, e).ConfigureFalseAwait();
                    
                    if (this.m_httpContext.Response.Responsed)
                    {
                        return false;
                    }

                    if (e.IsPermitOperation)
                    {
                        this.InitWebSocket(webSocket);

                        await this.m_httpContext.Response.AnswerAsync().ConfigureFalseAwait();

                        _ = this.PrivateWebSocketHandshaked(webSocket, new HttpContextEventArgs(httpContext));
                        return true;
                    }
                    else
                    {
                        this.m_httpContext.Response.SetStatus(403, "Forbidden");
                        await this.m_httpContext.Response.AnswerAsync();
                        await this.CloseAsync(TouchSocketHttpResource.RefuseWebSocketConnection.Format(e.Message)).ConfigureFalseAwait();
                    }
                }
                else
                {
                    await this.CloseAsync(TouchSocketHttpResource.WebSocketConnectionProtocolIsIncorrect).ConfigureFalseAwait();
                }
            }
            return false;
        }

        private void InitWebSocket(InternalWebSocket webSocket)
        {
            this.SetAdapter(new WebSocketDataHandlingAdapter());
            this.Protocol = Protocol.WebSocket;
            this.m_webSocket = webSocket;
            webSocket.Online = true;
        }
    }
}