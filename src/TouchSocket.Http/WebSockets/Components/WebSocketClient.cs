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

        protected override async Task OnWebSocketClosed(ClosedEventArgs e)
        {
            if (this.Closed != null)
            {
                await this.Closed.Invoke(this, e).ConfigureFalseAwait();
                if (e.Handled)
                {
                    return;
                }
            }
            await this.PluginManager.RaiseAsync(typeof(IWebSocketClosedPlugin), this, e).ConfigureFalseAwait();
        }

        protected override async Task OnWebSocketClosing(ClosedEventArgs e)
        {
            if (this.Closing != null)
            {
                await this.Closing.Invoke(this, e).ConfigureFalseAwait();
                if (e.Handled)
                {
                    return;
                }
            }
            await this.PluginManager.RaiseAsync(typeof(IWebSocketClosingPlugin), this, e).ConfigureFalseAwait();
        }

        /// <inheritdoc/>
        protected override async Task OnWebSocketHandshaked(HttpContextEventArgs e)
        {
            if (this.Handshaked != null)
            {
                await this.Handshaked.Invoke(this, e).ConfigureFalseAwait();
                if (e.Handled)
                {
                    return;
                }
            }

            await this.PluginManager.RaiseAsync(typeof(IWebSocketHandshakedPlugin), this, e).ConfigureFalseAwait();
        }

        /// <inheritdoc/>
        protected override async Task OnWebSocketHandshaking(HttpContextEventArgs e)
        {
            if (this.Handshaking != null)
            {
                await this.Handshaking.Invoke(this, e).ConfigureFalseAwait();
                if (e.Handled)
                {
                    return;
                }
            }

            await this.PluginManager.RaiseAsync(typeof(IWebSocketHandshakingPlugin), this, e).ConfigureFalseAwait();
        }

        /// <inheritdoc/>
        protected override async Task OnWebSocketReceived(WSDataFrameEventArgs e)
        {
            if (this.Received != null)
            {
                await this.Received.Invoke(this, e).ConfigureFalseAwait();
                if (e.Handled)
                {
                    return;
                }
            }
            await this.PluginManager.RaiseAsync(typeof(IWebSocketReceivedPlugin), this, e).ConfigureFalseAwait();
        }

        #endregion 事件

        /// <inheritdoc/>
        public string Version => this.WebSocket.Version;

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
        public Task<IWebSocketReceiveResult> ReadAsync(CancellationToken token)
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

        /// <inheritdoc/>
        public ValueTask<IWebSocketReceiveResult> ValueReadAsync(CancellationToken token)
        {
            return this.WebSocket.ValueReadAsync(token);
        }
    }
}