using System;
using System.Collections.Generic;
using System.Linq;
using TouchSocket.Core;
using TouchSocket.Sockets;
using System.Threading.Tasks;
using System.Threading;
using System.Net.WebSockets;

namespace TouchSocket.Http.WebSockets
{
    /// <summary>
    /// SetupClientWebSocket
    /// </summary>
    public abstract class SetupClientWebSocket : SetupConfigObject, IClient, ICloseObject, IConnectObject
    {
        /// <summary>
        /// SetupClientWebSocket
        /// </summary>
        public SetupClientWebSocket()
        {
            this.m_receiveCounter = new ValueCounter
            {
                Period = TimeSpan.FromSeconds(1),
                OnPeriod = this.OnReceivePeriod
            };
            this.m_sendCounter = new ValueCounter
            {
                Period = TimeSpan.FromSeconds(1),
                OnPeriod = this.OnSendPeriod
            };
        }

        #region 字段

        private readonly SemaphoreSlim m_semaphoreForConnect = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim m_semaphoreForSend = new SemaphoreSlim(1, 1);
        private ClientWebSocket m_client;
        private bool m_isHandshaked;
        private int m_receiveBufferSize = 1024 * 10;
        private ValueCounter m_receiveCounter;
        private int m_sendBufferSize = 1024 * 10;
        private ValueCounter m_sendCounter;

        #endregion 字段

        #region 连接

        /// <inheritdoc/>
        public virtual void Connect(int millisecondsTimeout, CancellationToken token)
        {
            this.ConnectAsync(millisecondsTimeout, token).GetFalseAwaitResult();
        }

        /// <inheritdoc/>
        public virtual async Task ConnectAsync(int millisecondsTimeout, CancellationToken token)
        {
            try
            {
                await this.m_semaphoreForConnect.WaitAsync();
                if (this.m_isHandshaked)
                {
                    return;
                }

                if (this.m_client == null || this.m_client.State != WebSocketState.Open)
                {
                    this.m_client.SafeDispose();
                    this.m_client = new ClientWebSocket();
                    await this.m_client.ConnectAsync(this.RemoteIPHost, token);
                    _ = this.BeginReceive();
                }

                this.m_isHandshaked = true;
            }
            finally
            {
                this.m_semaphoreForConnect.Release();
            }
        }

        #endregion 连接

        /// <inheritdoc/>
        public DateTime LastReceivedTime => this.m_receiveCounter.LastIncrement;

        /// <inheritdoc/>
        public DateTime LastSendTime => this.m_sendCounter.LastIncrement;

        /// <inheritdoc/>
        public Protocol Protocol { get; set; } = Protocol.WebSocket;

        /// <inheritdoc/>
        public IPHost RemoteIPHost { get; private set; }

        /// <summary>
        /// 是否已完成连接
        /// </summary>
        protected bool ProtectedIsHandshaked { get => this.m_isHandshaked;}

        /// <summary>
        /// 通讯实际客户端
        /// </summary>
        protected ClientWebSocket Client { get => this.m_client;}

        /// <inheritdoc/>
        public virtual void Close(string msg)
        {
            this.BreakOut(msg, true);
        }

        /// <summary>
        /// 中断连接
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="manual"></param>
        protected void BreakOut(string msg, bool manual)
        {
            lock (this.m_semaphoreForConnect)
            {
                if (this.m_isHandshaked)
                {
                    this.m_isHandshaked = false;
                    this.m_client.CloseAsync(WebSocketCloseStatus.NormalClosure, msg, CancellationToken.None);
                    this.m_client.SafeDispose();

                    this.OnDisconnected(new DisconnectEventArgs(manual, msg));
                }
            }
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
                this.BreakOut($"调用{nameof(Dispose)}", true);
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="config"></param>
        protected override void LoadConfig(TouchSocketConfig config)
        {
            this.RemoteIPHost = config.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty);
        }

        /// <summary>
        /// 已断开连接。
        /// </summary>
        /// <param name="e"></param>
        protected abstract void OnDisconnected(DisconnectEventArgs e);

        /// <summary>
        /// 收到数据
        /// </summary>
        /// <param name="result"></param>
        /// <param name="byteBlock"></param>
        /// <returns></returns>
        protected abstract Task OnReceived(System.Net.WebSockets.WebSocketReceiveResult result, ByteBlock byteBlock);

        private async Task BeginReceive()
        {
            try
            {
                while (true)
                {
                    using (var byteBlock = new ByteBlock(this.m_receiveBufferSize))
                    {
                        var result = await this.m_client.ReceiveAsync(new ArraySegment<byte>(byteBlock.Buffer, 0, byteBlock.Capacity), default);
                        if (result.Count == 0)
                        {
                            break;
                        }
                        byteBlock.SetLength(result.Count);
                        this.m_receiveCounter.Increment(result.Count);

                        await this.OnReceived(result, byteBlock).ConfigureFalseAwait();
                    }
                }

                this.BreakOut("远程终端主动关闭", false);
            }
            catch (Exception ex)
            {
                this.BreakOut(ex.Message, false);
            }
        }

        private void OnReceivePeriod(long value)
        {
            this.m_receiveBufferSize = TouchSocketUtility.HitBufferLength(value);
        }

        private void OnSendPeriod(long value)
        {
            this.m_sendBufferSize = TouchSocketUtility.HitBufferLength(value);
        }
    }
}