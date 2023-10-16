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

namespace TouchSocket.Sockets
{
    internal class WaitingClient<TClient> : DisposableObject, IWaitingClient<TClient> where TClient : IClient, ISender
    {
        private readonly SemaphoreSlim m_semaphoreSlim = new SemaphoreSlim(1, 1);
        private volatile bool m_breaked;
        private CancellationTokenSource m_cancellationTokenSource;

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

        #region 发送
        public ResponsedData SendThenResponse(byte[] buffer, int offset, int length, CancellationToken token = default)
        {
            try
            {
                this.m_semaphoreSlim.Wait();
                this.m_breaked = false;
                if (token.CanBeCanceled)
                {
                    this.m_cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
                }
                else
                {
                    this.m_cancellationTokenSource = new CancellationTokenSource(5000);
                }
                using (m_cancellationTokenSource)
                {
                    if (this.WaitingOptions.RemoteIPHost != null && this.Client is IUdpSession session)
                    {
                        using (var receiver = session.CreateReceiver())
                        {
                            session.Send(this.WaitingOptions.RemoteIPHost.EndPoint, buffer, offset, length);

                            while (true)
                            {
                                using (var receiverResult = receiver.ReadAsync(this.m_cancellationTokenSource.Token).GetFalseAwaitResult())
                                {
                                    var response = new ResponsedData(receiverResult.ByteBlock?.ToArray(), receiverResult.RequestInfo);
                                }
                            }
                        }
                    }
                    else
                    {
                        using (var receiver = this.Client.CreateReceiver())
                        {
                            this.Client.Send(buffer, offset, length);
                            while (true)
                            {
                                using (var receiverResult = receiver.ReadAsync(this.m_cancellationTokenSource.Token).GetFalseAwaitResult())
                                {
                                    if (receiverResult.IsClosed)
                                    {
                                        this.m_breaked = true;
                                        this.Cancel();
                                    }
                                    var response = new ResponsedData(receiverResult.ByteBlock?.ToArray(), receiverResult.RequestInfo);

                                    if (this.WaitingOptions.FilterFunc == null)
                                    {
                                        return response;
                                    }
                                    else
                                    {
                                        if (this.WaitingOptions.FilterFunc.Invoke(response))
                                        {
                                            return response;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                return this.WaitingOptions.ThrowBreakException && this.m_breaked ? throw new Exception("等待已终止。可能是客户端已掉线，或者被注销。") : default(ResponsedData);
            }
            finally
            {
                this.m_cancellationTokenSource= null;
                this.m_semaphoreSlim.Release();
            }
        }

        public async Task<ResponsedData> SendThenResponseAsync(byte[] buffer, int offset, int length, CancellationToken token = default)
        {
            try
            {
                await this.m_semaphoreSlim.WaitAsync();
                this.m_breaked = false;
                if (token.CanBeCanceled)
                {
                    this.m_cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
                }
                else
                {
                    this.m_cancellationTokenSource = new CancellationTokenSource(5000);
                }
                using (m_cancellationTokenSource)
                {
                    if (this.WaitingOptions.RemoteIPHost != null && this.Client is IUdpSession session)
                    {
                        using (var receiver = session.CreateReceiver())
                        {
                            await session.SendAsync(this.WaitingOptions.RemoteIPHost.EndPoint, buffer, offset, length);

                            while (true)
                            {
                                using (var receiverResult = await receiver.ReadAsync(m_cancellationTokenSource.Token))
                                {
                                    var response = new ResponsedData(receiverResult.ByteBlock?.ToArray(), receiverResult.RequestInfo);
                                }
                            }
                        }
                    }
                    else
                    {
                        using (var receiver = this.Client.CreateReceiver())
                        {
                            await this.Client.SendAsync(buffer, offset, length);
                            while (true)
                            {
                                using (var receiverResult = await receiver.ReadAsync(this.m_cancellationTokenSource.Token))
                                {
                                    if (receiverResult.IsClosed)
                                    {
                                        this.m_breaked = true;
                                        this.Cancel();
                                    }
                                    var response = new ResponsedData(receiverResult.ByteBlock?.ToArray(), receiverResult.RequestInfo);

                                    if (this.WaitingOptions.FilterFunc == null)
                                    {
                                        return response;
                                    }
                                    else
                                    {
                                        if (this.WaitingOptions.FilterFunc.Invoke(response))
                                        {
                                            return response;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                return this.WaitingOptions.ThrowBreakException && this.m_breaked ? throw new Exception("等待已终止。可能是客户端已掉线，或者被注销。") : default(ResponsedData);
            }
            finally
            {
                this.m_cancellationTokenSource = null;
                this.m_semaphoreSlim.Release();
            }
        }

        public byte[] SendThenReturn(byte[] buffer, int offset, int length, CancellationToken token = default)
        {
            return this.SendThenResponse(buffer, offset, length, token).Data;
        }

        public async Task<byte[]> SendThenReturnAsync(byte[] buffer, int offset, int length, CancellationToken token = default)
        {
            return (await this.SendThenResponseAsync(buffer, offset, length, token)).Data;
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            this.Cancel();
            this.Client = default;
            base.Dispose(disposing);
        }

        private void Cancel()
        {
            try
            {
                this.m_cancellationTokenSource?.Cancel();
            }
            catch
            {
            }
        }
    }
}