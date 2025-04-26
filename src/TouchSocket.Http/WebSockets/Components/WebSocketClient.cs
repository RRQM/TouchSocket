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

namespace TouchSocket.Http.WebSockets;

/// <summary>
/// WebSocketClient用户终端简单实现。
/// </summary>
public partial class WebSocketClient : WebSocketClientBase, IWebSocketClient
{
    /// <inheritdoc/>
    public bool AllowAsyncRead { get => this.WebSocket.AllowAsyncRead; set => this.WebSocket.AllowAsyncRead = value; }

    /// <inheritdoc/>
    public IHttpSession Client => this;

    #region 事件

    /// <inheritdoc/>
    public ClosedEventHandler<IWebSocketClient> Closed { get; set; }

    /// <inheritdoc/>
    public ClosingEventHandler<IWebSocketClient> Closing { get; set; }

    /// <inheritdoc/>
    public HttpContextEventHandler<IWebSocketClient> Handshaked { get; set; }

    /// <inheritdoc/>
    public HttpContextEventHandler<IWebSocketClient> Handshaking { get; set; }

    /// <inheritdoc/>
    public WSDataFrameEventHandler<IWebSocketClient> Received { get; set; }

    /// <summary>
    /// 当WebSocket连接关闭时执行的任务。
    /// </summary>
    /// <param name="e">包含关闭事件相关信息的参数。</param>
    /// <returns>一个等待完成的任务。</returns>
    protected override async Task OnWebSocketClosed(ClosedEventArgs e)
    {
        // 检查是否已注册关闭事件处理程序
        if (this.Closed != null)
        {
            // 如果已注册，则调用处理程序并传递事件参数
            await this.Closed.Invoke(this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            // 如果事件已被处理，则直接返回
            if (e.Handled)
            {
                return;
            }
        }
        await base.OnWebSocketClosed(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 当WebSocket即将关闭时，执行异步任务。
    /// </summary>
    /// <param name="e">提供了关闭事件的相关信息。</param>
    /// <returns>返回一个异步任务。</returns>
    protected override async Task OnWebSocketClosing(ClosingEventArgs e)
    {
        // 检查是否注册了关闭事件的处理程序
        if (this.Closing != null)
        {
            // 如果已注册，调用处理程序并传递事件参数
            await this.Closing.Invoke(this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            // 如果事件已被处理，则直接返回，不再执行后续代码
            if (e.Handled)
            {
                return;
            }
        }
        await base.OnWebSocketClosing(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <inheritdoc/>
    protected override async Task OnWebSocketHandshaked(HttpContextEventArgs e)
    {
        if (this.Handshaked != null)
        {
            await this.Handshaked.Invoke(this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            if (e.Handled)
            {
                return;
            }
        }

        await base.OnWebSocketHandshaked(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <inheritdoc/>
    protected override async Task OnWebSocketHandshaking(HttpContextEventArgs e)
    {
        if (this.Handshaking != null)
        {
            await this.Handshaking.Invoke(this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            if (e.Handled)
            {
                return;
            }
        }

        await base.OnWebSocketHandshaking(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <inheritdoc/>
    protected override async Task OnWebSocketReceived(WSDataFrameEventArgs e)
    {
        if (this.Received != null)
        {
            await this.Received.Invoke(this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            if (e.Handled)
            {
                return;
            }
        }
        await base.OnWebSocketReceived(e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    #endregion 事件

    /// <inheritdoc/>
    public string Version => this.WebSocket.Version;

    /// <inheritdoc/>
    public WebSocketCloseStatus CloseStatus => this.WebSocket.CloseStatus;

    /// <inheritdoc/>
    public Task<Result> CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription,CancellationToken token=default)
    {
        return this.WebSocket.CloseAsync(closeStatus, statusDescription,token);
    }

    /// <inheritdoc/>
    public Task PingAsync()
    {
        return this.WebSocket.PingAsync();
    }

    /// <inheritdoc/>
    public Task PongAsync()
    {
        return this.WebSocket.PongAsync();
    }

    /// <inheritdoc/>
    public ValueTask<IWebSocketReceiveResult> ReadAsync(CancellationToken token)
    {
        return this.WebSocket.ReadAsync(token);
    }

    /// <inheritdoc/>
    public Task SendAsync(WSDataFrame dataFrame, bool endOfMessage = true)
    {
        return this.WebSocket.SendAsync(dataFrame, endOfMessage);
    }

    /// <inheritdoc/>
    public Task SendAsync(string text, bool endOfMessage = true)
    {
        return this.WebSocket.SendAsync(text, endOfMessage);
    }

    /// <inheritdoc/>
    public Task SendAsync(ReadOnlyMemory<byte> memory, bool endOfMessage = true)
    {
        return this.WebSocket.SendAsync(memory, endOfMessage);
    }
}