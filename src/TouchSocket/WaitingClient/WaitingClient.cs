//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;

namespace TouchSocket.Sockets
{
    internal class WaitingClient<TClient> : DisposableObject, IWaitingClient<TClient> where TClient : IClient, IDefaultSender, ISender
    {
        private readonly Func<ResponsedData, bool> m_func;
        private readonly SemaphoreSlim m_semaphoreSlim = new SemaphoreSlim(1);
        private readonly WaitData<ResponsedData> m_waitData = new WaitData<ResponsedData>();
        private readonly WaitDataAsync<ResponsedData> m_waitDataAsync = new WaitDataAsync<ResponsedData>();

        private volatile bool m_breaked;

        public WaitingClient(TClient client, WaitingOptions waitingOptions, Func<ResponsedData, bool> func)
        {
            this.Client = client ?? throw new ArgumentNullException(nameof(client));
            this.WaitingOptions = waitingOptions;
            this.m_func = func;
        }

        public WaitingClient(TClient client, WaitingOptions waitingOptions)
        {
            this.Client = client ?? throw new ArgumentNullException(nameof(client));
            this.WaitingOptions = waitingOptions;
        }

        public bool CanSend
        {
            get
            {
                return this.Client is ITcpClientBase tcpClient ? tcpClient.CanSend : this.Client is IUdpSession;
            }
        }

        public TClient Client { get; private set; }

        public WaitingOptions WaitingOptions { get; set; }

        protected override void Dispose(bool disposing)
        {
            this.Client = default;
            this.m_waitData.SafeDispose();
            base.Dispose(disposing);
        }

        private void Cancel()
        {
            this.m_waitData.Cancel();
            this.m_waitDataAsync.Cancel();
        }

        private void OnDisconnected(ITcpClientBase client, DisconnectEventArgs e)
        {
            this.m_breaked = true;
            this.Cancel();
        }

        private bool OnHandleRawBuffer(ByteBlock byteBlock)
        {
            var responsedData = new ResponsedData(byteBlock.ToArray(), null, true);
            if (this.m_func == null || this.m_func.Invoke(responsedData))
            {
                return !this.Set(responsedData);
            }
            else
            {
                return true;
            }
        }

        private bool OnHandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            ResponsedData responsedData;
            if (byteBlock != null)
            {
                responsedData = new ResponsedData(byteBlock.ToArray(), requestInfo, false);
            }
            else
            {
                responsedData = new ResponsedData(null, requestInfo, false);
            }

            if (this.m_func == null || this.m_func.Invoke(responsedData))
            {
                return !this.Set(responsedData);
            }
            else
            {
                return true;
            }
        }

        private void Reset()
        {
            this.m_waitData.Reset();
            this.m_waitDataAsync.Reset();
        }

        #region 同步Response

        public ResponsedData SendThenResponse(byte[] buffer, int offset, int length, int timeout = 1000 * 5, CancellationToken token = default)
        {
            try
            {
                this.m_semaphoreSlim.Wait();
                this.m_breaked = false;
                this.Reset();
                if (this.WaitingOptions.BreakTrigger && this.Client is ITcpClientBase tcpClient)
                {
                    tcpClient.Disconnected += this.OnDisconnected;
                }

                if (this.WaitingOptions.AdapterFilter == AdapterFilter.AllAdapter || this.WaitingOptions.AdapterFilter == AdapterFilter.WaitAdapter)
                {
                    this.Client.OnHandleReceivedData += this.OnHandleReceivedData;
                }
                else
                {
                    this.Client.OnHandleRawBuffer += this.OnHandleRawBuffer;
                }

                if (this.WaitingOptions.RemoteIPHost != null && this.Client is IUdpSession session)
                {
                    if (this.WaitingOptions.AdapterFilter == AdapterFilter.AllAdapter || this.WaitingOptions.AdapterFilter == AdapterFilter.SendAdapter)
                    {
                        session.Send(this.WaitingOptions.RemoteIPHost.EndPoint, buffer, offset, length);
                    }
                    else
                    {
                        session.DefaultSend(this.WaitingOptions.RemoteIPHost.EndPoint, buffer, offset, length);
                    }
                }
                else
                {
                    if (this.WaitingOptions.AdapterFilter == AdapterFilter.AllAdapter || this.WaitingOptions.AdapterFilter == AdapterFilter.SendAdapter)
                    {
                        this.Client.Send(buffer, offset, length);
                    }
                    else
                    {
                        this.Client.DefaultSend(buffer, offset, length);
                    }
                }

                this.m_waitData.SetCancellationToken(token);
                switch (this.m_waitData.Wait(timeout))
                {
                    case WaitDataStatus.SetRunning:
                        return this.m_waitData.WaitResult;

                    case WaitDataStatus.Overtime:
                        throw new TimeoutException();
                    case WaitDataStatus.Canceled:
                        {
                            return this.WaitingOptions.ThrowBreakException && this.m_breaked ? throw new Exception("等待已终止。可能是客户端已掉线，或者被注销。") : (ResponsedData)default;
                        }
                    case WaitDataStatus.Default:
                    case WaitDataStatus.Disposed:
                    default:
                        throw new Exception(TouchSocketCoreResource.UnknownError.GetDescription());
                }
            }
            finally
            {
                this.m_semaphoreSlim.Release();
                if (this.WaitingOptions.BreakTrigger && this.Client is ITcpClientBase tcpClient)
                {
                    tcpClient.Disconnected -= this.OnDisconnected;
                }

                if (this.WaitingOptions.AdapterFilter == AdapterFilter.AllAdapter || this.WaitingOptions.AdapterFilter == AdapterFilter.WaitAdapter)
                {
                    this.Client.OnHandleReceivedData -= this.OnHandleReceivedData;
                }
                else
                {
                    this.Client.OnHandleRawBuffer -= this.OnHandleRawBuffer;
                }
            }
        }

        public ResponsedData SendThenResponse(byte[] buffer, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return this.SendThenResponse(buffer, 0, buffer.Length, timeout, token);
        }

        public ResponsedData SendThenResponse(ByteBlock byteBlock, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return this.SendThenResponse(byteBlock.Buffer, 0, byteBlock.Len, timeout, token);
        }

        #endregion 同步Response

        #region Response异步

        public async Task<ResponsedData> SendThenResponseAsync(byte[] buffer, int offset, int length, int timeout = 1000 * 5, CancellationToken token = default)
        {
            try
            {
                await this.m_semaphoreSlim.WaitAsync();
                this.m_breaked = false;
                this.Reset();
                if (this.WaitingOptions.BreakTrigger && this.Client is ITcpClientBase tcpClient)
                {
                    tcpClient.Disconnected += this.OnDisconnected;
                }

                if (this.WaitingOptions.AdapterFilter == AdapterFilter.AllAdapter || this.WaitingOptions.AdapterFilter == AdapterFilter.WaitAdapter)
                {
                    this.Client.OnHandleReceivedData += this.OnHandleReceivedData;
                }
                else
                {
                    this.Client.OnHandleRawBuffer += this.OnHandleRawBuffer;
                }

                if (this.WaitingOptions.RemoteIPHost != null && this.Client is IUdpSession session)
                {
                    if (this.WaitingOptions.AdapterFilter == AdapterFilter.AllAdapter || this.WaitingOptions.AdapterFilter == AdapterFilter.SendAdapter)
                    {
                        session.Send(this.WaitingOptions.RemoteIPHost.EndPoint, buffer, offset, length);
                    }
                    else
                    {
                        session.DefaultSend(this.WaitingOptions.RemoteIPHost.EndPoint, buffer, offset, length);
                    }
                }
                else
                {
                    if (this.WaitingOptions.AdapterFilter == AdapterFilter.AllAdapter || this.WaitingOptions.AdapterFilter == AdapterFilter.SendAdapter)
                    {
                        this.Client.Send(buffer, offset, length);
                    }
                    else
                    {
                        this.Client.DefaultSend(buffer, offset, length);
                    }
                }

                this.m_waitDataAsync.SetCancellationToken(token);
                switch (await this.m_waitDataAsync.WaitAsync(timeout))
                {
                    case WaitDataStatus.SetRunning:
                        return this.m_waitData.WaitResult;

                    case WaitDataStatus.Overtime:
                        throw new TimeoutException();
                    case WaitDataStatus.Canceled:
                        {
                            return this.WaitingOptions.ThrowBreakException && this.m_breaked ? throw new Exception("等待已终止。可能是客户端已掉线，或者被注销。") : (ResponsedData)default;
                        }
                    case WaitDataStatus.Default:
                    case WaitDataStatus.Disposed:
                    default:
                        throw new Exception(TouchSocketCoreResource.UnknownError.GetDescription());
                }
            }
            finally
            {
                this.m_semaphoreSlim.Release();
                if (this.WaitingOptions.BreakTrigger && this.Client is ITcpClientBase tcpClient)
                {
                    tcpClient.Disconnected -= this.OnDisconnected;
                }

                if (this.WaitingOptions.AdapterFilter == AdapterFilter.AllAdapter || this.WaitingOptions.AdapterFilter == AdapterFilter.WaitAdapter)
                {
                    this.Client.OnHandleReceivedData -= this.OnHandleReceivedData;
                }
                else
                {
                    this.Client.OnHandleRawBuffer -= this.OnHandleRawBuffer;
                }
            }
        }

        public Task<ResponsedData> SendThenResponseAsync(byte[] buffer, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return this.SendThenResponseAsync(buffer, 0, buffer.Length, timeout, token);
        }

        public Task<ResponsedData> SendThenResponseAsync(ByteBlock byteBlock, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return this.SendThenResponseAsync(byteBlock.Buffer, 0, byteBlock.Len, timeout, token);
        }

        #endregion Response异步

        #region 字节同步

        public byte[] SendThenReturn(byte[] buffer, int offset, int length, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return this.SendThenResponse(buffer, offset, length, timeout, token).Data;
        }

        public byte[] SendThenReturn(byte[] buffer, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return this.SendThenReturn(buffer, 0, buffer.Length, timeout, token);
        }

        public byte[] SendThenReturn(ByteBlock byteBlock, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return this.SendThenReturn(byteBlock.Buffer, 0, byteBlock.Len, timeout, token);
        }

        #endregion 字节同步

        #region 字节异步

        public async Task<byte[]> SendThenReturnAsync(byte[] buffer, int offset, int length, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return (await this.SendThenResponseAsync(buffer, offset, length, timeout, token)).Data;
        }

        public async Task<byte[]> SendThenReturnAsync(byte[] buffer, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return (await this.SendThenResponseAsync(buffer, 0, buffer.Length, timeout, token)).Data;
        }

        public async Task<byte[]> SendThenReturnAsync(ByteBlock byteBlock, int timeout = 1000 * 5, CancellationToken token = default)
        {
            return (await this.SendThenResponseAsync(byteBlock.Buffer, 0, byteBlock.Len, timeout, token)).Data;
        }

        #endregion 字节异步

        private bool Set(ResponsedData responsedData)
        {
            this.m_waitData.Set(responsedData);
            this.m_waitDataAsync.Set(responsedData);
            return true;
        }
    }
}