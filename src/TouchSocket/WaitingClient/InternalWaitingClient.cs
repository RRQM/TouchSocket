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

namespace TouchSocket.Sockets
{
    internal sealed class InternalWaitingClient<TClient, TResult> : DisposableObject, IWaitingClient<TClient, TResult>
        where TClient : IReceiverClient<TResult>, ISender, IRequestInfoSender
        where TResult : IReceiverResult
    {
        private readonly SemaphoreSlim m_semaphoreSlim = new SemaphoreSlim(1, 1);

        public InternalWaitingClient(TClient client, WaitingOptions waitingOptions)
        {
            this.Client = client ?? throw new ArgumentNullException(nameof(client));
            this.WaitingOptions = waitingOptions;
        }

        public TClient Client { get; private set; }

        public WaitingOptions WaitingOptions { get; set; }

        #region 发送

        public async Task<ResponsedData> SendThenResponseAsync(ReadOnlyMemory<byte> memory, CancellationToken token = default)
        {
            await this.m_semaphoreSlim.WaitAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            try
            {
                if (this.WaitingOptions.RemoteIPHost != null && this.Client is IUdpSession session)
                {
                    using (var receiver = session.CreateReceiver())
                    {
                        await session.SendAsync(this.WaitingOptions.RemoteIPHost.EndPoint, memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

                        while (true)
                        {
                            using (var receiverResult = await receiver.ReadAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
                            {
                                var byteBlock = receiverResult.ByteBlock;
                                ByteBlock newByteBlock = default;
                                if (byteBlock != null)
                                {
                                    newByteBlock = new ByteBlock(byteBlock.ToArray());
                                }
                                var response = new ResponsedData(newByteBlock, receiverResult.RequestInfo);

                                var filterFunc = this.WaitingOptions.FilterFuncAsync;
                                if (filterFunc == null)
                                {
                                    return response;
                                }
                                else
                                {
                                    if (await filterFunc.Invoke(response).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
                                    {
                                        return response;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    using (var receiver = this.Client.CreateReceiver())
                    {
                        await this.Client.SendAsync(memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        while (true)
                        {
                            using (var receiverResult = await receiver.ReadAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
                            {
                                if (receiverResult.IsCompleted)
                                {
                                    ThrowHelper.ThrowClientNotConnectedException();
                                }

                                var byteBlock = receiverResult.ByteBlock;
                                ByteBlock newByteBlock = default;
                                if (byteBlock != null)
                                {
                                    newByteBlock = new ByteBlock(byteBlock.ToArray());
                                }
                                var response = new ResponsedData(newByteBlock, receiverResult.RequestInfo);

                                var filterFunc = this.WaitingOptions.FilterFuncAsync;
                                if (filterFunc == null)
                                {
                                    return response;
                                }
                                else
                                {
                                    if (await filterFunc.Invoke(response).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
                                    {
                                        return response;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                this.m_semaphoreSlim.Release();
            }
        }

        public async Task<ResponsedData> SendThenResponseAsync(IRequestInfo requestInfo, CancellationToken token)
        {
            await this.m_semaphoreSlim.WaitAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

            try
            {
                if (this.WaitingOptions.RemoteIPHost != null && this.Client is IUdpSession session)
                {
                    using (var receiver = session.CreateReceiver())
                    {
                        await session.SendAsync(this.WaitingOptions.RemoteIPHost.EndPoint, requestInfo).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

                        while (true)
                        {
                            using (var receiverResult = await receiver.ReadAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
                            {
                                var response = new ResponsedData(receiverResult.ByteBlock, receiverResult.RequestInfo);

                                var filterFunc = this.WaitingOptions.FilterFuncAsync;
                                if (filterFunc == null)
                                {
                                    return response;
                                }
                                else
                                {
                                    if (await filterFunc.Invoke(response).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
                                    {
                                        return response;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    using (var receiver = this.Client.CreateReceiver())
                    {
                        await this.Client.SendAsync(requestInfo).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        while (true)
                        {
                            using (var receiverResult = await receiver.ReadAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
                            {
                                if (receiverResult.IsCompleted)
                                {
                                    ThrowHelper.ThrowClientNotConnectedException();
                                }
                                var response = new ResponsedData(receiverResult.ByteBlock, receiverResult.RequestInfo);

                                var filterFunc = this.WaitingOptions.FilterFuncAsync;
                                if (filterFunc == null)
                                {
                                    return response;
                                }
                                else
                                {
                                    if (await filterFunc.Invoke(response).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
                                    {
                                        return response;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                this.m_semaphoreSlim.Release();
            }
        }

        #endregion 发送
    }
}