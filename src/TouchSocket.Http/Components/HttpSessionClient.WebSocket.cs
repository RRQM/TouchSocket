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

using System.Diagnostics;
using System.Net.WebSockets;
using TouchSocket.Http.WebSockets;
using TouchSocket.Resources;
using TouchSocket.Sockets;

namespace TouchSocket.Http;

/// <summary>
/// http辅助类
/// </summary>
public partial class HttpSessionClient : TcpSessionClientBase, IHttpSessionClient
{
    private InternalWebSocket m_webSocket;

    /// <inheritdoc/>
    public IWebSocket WebSocket => this.m_webSocket;

    #region 事件

    private Task PrivateWebSocketConnecting(IWebSocket webSocket, HttpContextEventArgs e)
    {
        return this.OnWebSocketConnecting(webSocket, e);
    }

    private Task PrivateWebSocketConnected(IWebSocket webSocket, HttpContextEventArgs e)
    {
        return this.OnWebSocketConnected(webSocket, e);
    }
   
    private async Task PrivateWebSocketReceived(WSDataFrame dataFrame)
    {
        if (dataFrame.IsClose && this.GetValue(WebSocketFeature.AutoCloseProperty))
        {
            var payloadMemory = dataFrame.PayloadData;
            var payloadSpan = payloadMemory.Span;
            if (payloadSpan.Length >= 2)
            {
                var closeStatus = (WebSocketCloseStatus)payloadSpan.ReadValue<ushort>(EndianType.Big);
                this.m_webSocket.CloseStatus = closeStatus;
            }

            var msg = payloadSpan.ToString(System.Text.Encoding.UTF8);

            await this.PrivateWebSocketClosing(new ClosingEventArgs(msg))
                .ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            await this.m_webSocket.CloseAsync(msg ?? "Auto closed successful")
                .ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return;
        }
        if (dataFrame.IsPing && this.GetValue(WebSocketFeature.AutoPongProperty))
        {
            await this.m_webSocket.PongAsync()
                .ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return;
        }

        if (this.m_webSocket.AllowAsyncRead)
        {
            await this.m_webSocket.InputReceiveAsync(dataFrame, this.ClosedToken)
              .ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return;
        }

        await this.OnWebSocketReceived(this.m_webSocket, new WSDataFrameEventArgs(dataFrame))
            .ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    private Task PrivateWebSocketClosing(ClosingEventArgs e)
    {
        return this.OnWebSocketClosing(this.m_webSocket, e);
    }

    private async Task PrivateWebSocketClosed(ClosedEventArgs e)
    {
        this.m_webSocket.Online = false;
        if (this.m_webSocket.AllowAsyncRead)
        {
            this.m_webSocket.Complete(e.Message);
        }
        await this.OnWebSocketClosed(this.m_webSocket, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 在WebSocket握手过程中触发插件执行异步任务
    /// </summary>
    /// <param name="webSocket">WebSocket对象，用于进行WebSocket通信</param>
    /// <param name="e">HTTP上下文参数，提供关于HTTP请求和响应的信息</param>
    /// <returns>返回一个任务，该任务在插件处理完成后结束</returns>
    protected virtual async Task OnWebSocketConnecting(IWebSocket webSocket, HttpContextEventArgs e)
    {
        // 提前WebSocket握手过程中的插件执行
        await this.PluginManager.RaiseIWebSocketConnectingPluginAsync(this.Resolver, webSocket, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 当WebSocket握手成功时，触发相关插件的异步事件。
    /// </summary>
    /// <param name="webSocket">WebSocket对象，表示握手成功的WebSocket连接。</param>
    /// <param name="e">HttpContextEventArgs对象，包含HTTP上下文信息。</param>
    /// <returns>一个表示事件处理完成的Task对象。</returns>
    protected virtual async Task OnWebSocketConnected(IWebSocket webSocket, HttpContextEventArgs e)
    {
        await this.PluginManager.RaiseIWebSocketConnectedPluginAsync(this.Resolver, webSocket, e)
            .ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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
        await this.PluginManager.RaiseIWebSocketReceivedPluginAsync(this.Resolver, webSocket, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 调用前触发WebSocket关闭时的插件
    /// </summary>
    /// <param name="webSocket">当前的WebSocket实例</param>
    /// <param name="e">关闭事件的参数</param>
    /// <returns>异步任务</returns>
    protected virtual async Task OnWebSocketClosing(IWebSocket webSocket, ClosingEventArgs e)
    {
        // 提前通知所有IWebSocketClosingPlugin插件，WebSocket即将关闭
        await this.PluginManager.RaiseIWebSocketClosingPluginAsync(this.Resolver, webSocket, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 虚拟异步方法：处理WebSocket关闭事件
    /// </summary>
    /// <param name="webSocket">关闭的WebSocket实例</param>
    /// <param name="e">WebSocket关闭事件的附加参数</param>
    /// <remarks>
    /// 此方法通过调用插件管理器，触发IWebSocketClosedPlugin接口的实现来处理WebSocket关闭事件
    /// </remarks>
    protected virtual async Task OnWebSocketClosed(IWebSocket webSocket, ClosedEventArgs e)
    {
        await this.PluginManager.RaiseIWebSocketClosedPluginAsync(this.Resolver, webSocket, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    #endregion 事件

    /// <inheritdoc/>
    public async Task<Result> SwitchProtocolToWebSocketAsync(HttpContext httpContext)
    {
        if (this.m_webSocket is not null)
        {
            return Result.Success;
        }
        if (this.Protocol != Protocol.Http)
        {
            return Result.FromFail(TouchSocketHttpResource.ProtocolIsIncorrect);
        }

        var response = httpContext.Response;
        try
        {
            if (WSTools.TryGetResponse(httpContext.Request, response))
            {
                var e = new HttpContextEventArgs(this.m_httpContext)
                {
                    IsPermitOperation = true
                };

                var webSocket = new InternalWebSocket(this);

                await this.PrivateWebSocketConnecting(webSocket, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

                if (response.Responsed)
                {
                    return Result.FromFail(TouchSocketHttpResource.RequestHasBeenResponded);
                }

                if (e.IsPermitOperation)
                {
                    this.InitWebSocket(webSocket);

                    await response.AnswerAsync().ConfigureAwait(EasyTask.ContinueOnCapturedContext);

                    _ = EasyTask.SafeRun(this.PrivateWebSocketConnected, webSocket, new HttpContextEventArgs(httpContext));

                    return Result.Success;
                }
                else
                {
                    response.SetStatus(403, "Forbidden");
                    await response.AnswerAsync();

                    var msg = TouchSocketHttpResource.RefuseWebSocketConnection.Format(e.Message);
                    await this.CloseAsync(msg).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    return Result.FromFail(msg);
                }
            }
            else
            {
                await this.CloseAsync(TouchSocketHttpResource.WebSocketConnectionProtocolIsIncorrect).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                return Result.FromFail(TouchSocketHttpResource.WebSocketConnectionProtocolIsIncorrect);
            }
        }
        catch (System.Exception ex)
        {
            return Result.FromException(ex);
        }

    }

    private void InitWebSocket(InternalWebSocket webSocket)
    {
        this.SetAdapter(this.m_webSocketAdapter);
        this.Protocol = Protocol.WebSocket;
        this.m_webSocket = webSocket;
        webSocket.Online = true;
    }
}