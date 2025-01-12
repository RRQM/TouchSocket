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
    /// SetupClientWebSocket
    /// </summary>
    public abstract class SetupClientWebSocket : SetupConfigObject, IClosableClient
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
        public virtual async Task ConnectAsync(int millisecondsTimeout, CancellationToken token)
        {
            await this.m_semaphoreForConnect.WaitTimeAsync(millisecondsTimeout, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            try
            {
                if (this.m_isHandshaked)
                {
                    return;
                }

                if (this.m_client == null || this.m_client.State != WebSocketState.Open)
                {
                    this.m_client.SafeDispose();
                    this.m_client = new ClientWebSocket();
                    await this.m_client.ConnectAsync(this.RemoteIPHost, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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
        public IPHost RemoteIPHost => this.Config.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty);

        /// <summary>
        /// 是否已完成连接
        /// </summary>
        protected bool ProtectedIsHandshaked => this.m_isHandshaked;

        /// <summary>
        /// 通讯实际客户端
        /// </summary>
        protected ClientWebSocket Client => this.m_client;

        /// <inheritdoc/>
        public virtual Task CloseAsync(string msg)
        {
            this.Abort(true, msg);
            return EasyTask.CompletedTask;
        }

        /// <summary>
        /// 中断连接
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="manual"></param>
        protected void Abort(bool manual, string msg)
        {
            lock (this.m_semaphoreForConnect)
            {
                if (this.m_isHandshaked)
                {
                    this.m_isHandshaked = false;
                    this.m_client.CloseAsync(WebSocketCloseStatus.NormalClosure, msg, CancellationToken.None);
                    this.m_client.SafeDispose();

                    this.OnDisconnected(new ClosedEventArgs(manual, msg));
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
                this.Abort(true, $"调用{nameof(Dispose)}");
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// 已断开连接。
        /// </summary>
        /// <param name="e"></param>
        protected abstract void OnDisconnected(ClosedEventArgs e);

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
                        var result = await this.m_client.ReceiveAsync(byteBlock.TotalMemory.GetArray(), default).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        if (result.Count == 0)
                        {
                            break;
                        }
                        byteBlock.SetLength(result.Count);
                        this.m_receiveCounter.Increment(result.Count);

                        await this.OnReceived(result, byteBlock).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    }
                }

                this.Abort(false, "远程终端主动关闭");
            }
            catch (Exception ex)
            {
                this.Abort(false, ex.Message);
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
    }
}