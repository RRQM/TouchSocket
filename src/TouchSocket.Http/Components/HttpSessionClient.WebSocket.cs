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
                await this.PrivateWebSocketClosing(new MsgEventArgs(msg)).ConfigureAwait(false);
                await this.m_webSocket.CloseAsync(msg).ConfigureAwait(false);
                return;
            }
            if (dataFrame.IsPing && this.GetValue(WebSocketFeature.AutoPongProperty))
            {
                await this.m_webSocket.PongAsync();
                return;
            }

            if (this.m_webSocket.AllowAsyncRead)
            {
                await this.m_webSocket.InputReceiveAsync(dataFrame).ConfigureAwait(false);
                return;
            }

            await this.OnWebSocketReceived(this.m_webSocket, new WSDataFrameEventArgs(dataFrame)).ConfigureAwait(false);
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
                await this.m_webSocket.Complete(e.Message).ConfigureAwait(false);
            }
            await this.OnWebSocketClosed(this.m_webSocket, e).ConfigureAwait(false);
        }

        /// <summary>
        /// 在WebSocket握手过程中触发插件执行异步任务
        /// </summary>
        /// <param name="webSocket">WebSocket对象，用于进行WebSocket通信</param>
        /// <param name="e">HTTP上下文参数，提供关于HTTP请求和响应的信息</param>
        /// <returns>返回一个任务，该任务在插件处理完成后结束</returns>
        protected virtual async Task OnWebSocketHandshaking(IWebSocket webSocket, HttpContextEventArgs e)
        {
            // 提前WebSocket握手过程中的插件执行
            await this.PluginManager.RaiseAsync(typeof(IWebSocketHandshakingPlugin), webSocket, e).ConfigureAwait(false);
        }

        /// <summary>
        /// 当WebSocket握手成功时，触发相关插件的异步事件。
        /// </summary>
        /// <param name="webSocket">WebSocket对象，表示握手成功的WebSocket连接。</param>
        /// <param name="e">HttpContextEventArgs对象，包含HTTP上下文信息。</param>
        /// <returns>一个表示事件处理完成的Task对象。</returns>
        protected virtual Task OnWebSocketHandshaked(IWebSocket webSocket, HttpContextEventArgs e)
        {
            // 在一个任务中异步调用插件管理器的RaiseAsync方法，传递WebSocket和HTTP上下文参数
            return Task.Run(() => this.PluginManager.RaiseAsync(typeof(IWebSocketHandshakedPlugin), webSocket, e));
        }

        /// <summary>
        /// 虚拟异步方法：当WebSocket接收到数据时触发
        /// </summary>
        /// <param name="webSocket">提供数据接收的WebSocket实例</param>
        /// <param name="e">包含接收数据的事件参数</param>
        /// <remarks>
        /// 此方法通过调用插件管理器来通知所有实现了<see cref="IWebSocketReceivedPlugin"/>接口的插件，
        /// 使它们能够处理接收到的WebSocket数据。这样做可以扩展数据处理逻辑，而无需直接修改此方法。
        /// </remarks>
        protected virtual async Task OnWebSocketReceived(IWebSocket webSocket, WSDataFrameEventArgs e)
        {
            await this.PluginManager.RaiseAsync(typeof(IWebSocketReceivedPlugin), webSocket, e).ConfigureAwait(false);
        }

        /// <summary>
        /// 调用前触发WebSocket关闭时的插件
        /// </summary>
        /// <param name="webSocket">当前的WebSocket实例</param>
        /// <param name="e">关闭事件的参数</param>
        /// <returns>异步任务</returns>
        protected virtual async Task OnWebSocketClosing(IWebSocket webSocket, MsgEventArgs e)
        {
            // 提前通知所有IWebSocketClosingPlugin插件，WebSocket即将关闭
            await this.PluginManager.RaiseAsync(typeof(IWebSocketClosingPlugin), webSocket, e).ConfigureAwait(false);
        }

        /// <summary>
        /// 虚拟异步方法：处理WebSocket关闭事件
        /// </summary>
        /// <param name="webSocket">关闭的WebSocket实例</param>
        /// <param name="e">WebSocket关闭事件的附加参数</param>
        /// <remarks>
        /// 此方法通过调用插件管理器，触发IWebSocketClosedPlugin接口的实现来处理WebSocket关闭事件
        /// </remarks>
        protected virtual async Task OnWebSocketClosed(IWebSocket webSocket, MsgEventArgs e)
        {
            await this.PluginManager.RaiseAsync(typeof(IWebSocketClosedPlugin), webSocket, e).ConfigureAwait(false);
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

                    await this.PrivateWebSocketHandshaking(webSocket, e).ConfigureAwait(false);

                    if (this.m_httpContext.Response.Responsed)
                    {
                        return false;
                    }

                    if (e.IsPermitOperation)
                    {
                        this.InitWebSocket(webSocket);

                        await this.m_httpContext.Response.AnswerAsync().ConfigureAwait(false);

                        _ = this.PrivateWebSocketHandshaked(webSocket, new HttpContextEventArgs(httpContext));
                        return true;
                    }
                    else
                    {
                        this.m_httpContext.Response.SetStatus(403, "Forbidden");
                        await this.m_httpContext.Response.AnswerAsync();
                        await this.CloseAsync(TouchSocketHttpResource.RefuseWebSocketConnection.Format(e.Message)).ConfigureAwait(false);
                    }
                }
                else
                {
                    await this.CloseAsync(TouchSocketHttpResource.WebSocketConnectionProtocolIsIncorrect).ConfigureAwait(false);
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