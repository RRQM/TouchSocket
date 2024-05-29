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

namespace TouchSocket.Http
{
    /// <summary>
    /// Http客户端基类
    /// </summary>
    public abstract class HttpClientBase : TcpClientBase, IHttpSession
    {
        #region 字段

        private readonly SemaphoreSlim m_semaphoreForRequest = new SemaphoreSlim(1, 1);
        private readonly WaitData<HttpResponse> m_waitData = new WaitData<HttpResponse>();
        private readonly WaitDataAsync<HttpResponse> m_waitDataAsync = new WaitDataAsync<HttpResponse>();
        private bool m_getContent;

        #endregion 字段

        //internal void InternalSend(byte[] buffer, int offset, int count)
        //{
        //    this.ProtectedDefaultSend(buffer, offset, count);
        //}

        internal Task InternalSendAsync(ReadOnlyMemory<byte>  memory)
        {
            return this.ProtectedDefaultSendAsync(memory);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.m_waitData.SafeDispose();
                this.m_waitDataAsync.SafeDispose();
            }
            base.Dispose(disposing);
        }

        #region Request

        //protected async Task<Stream> ProtectedGetStreamAsync()
        //{
        //}

        private void ReleaseLock()
        {
            this.m_semaphoreForRequest.Release();
        }

        ///// <summary>
        ///// 发送Http请求，并仅等待响应头
        ///// </summary>
        ///// <param name="request"></param>
        ///// <param name="millisecondsTimeout"></param>
        ///// <param name="token"></param>
        ///// <returns></returns>
        ///// <exception cref="TimeoutException"></exception>
        ///// <exception cref="OperationCanceledException"></exception>
        ///// <exception cref="Exception"></exception>
        //protected HttpResponseResult ProtectedRequest(HttpRequest request, int millisecondsTimeout = 10 * 1000, CancellationToken token = default)
        //{
        //    this.m_semaphoreForRequest.WaitTime(millisecondsTimeout, token);
        //    try
        //    {
        //        this.m_getContent = false;
        //        using (var byteBlock = new ByteBlock())
        //        {
        //            request.Build(byteBlock);

        //            this.Reset(token);
        //            this.ProtectedDefaultSend(byteBlock.Buffer, 0, byteBlock.Length);

        //            var status = this.m_waitData.Wait(millisecondsTimeout);

        //            status.ThrowIfNotRunning();

        //            return new HttpResponseResult(this.m_waitData.WaitResult, this.ReleaseLock);
        //        }
        //    }
        //    catch
        //    {
        //        this.m_semaphoreForRequest.Release();
        //        throw;
        //    }
        //}

        /// <summary>
        /// 异步发送Http请求，并仅等待响应头
        /// </summary>
        /// <param name="request"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        /// <exception cref="Exception"></exception>
        protected async Task<HttpResponseResult> ProtectedRequestAsync(HttpRequest request, int millisecondsTimeout = 10 * 1000, CancellationToken token = default)
        {
            await this.m_semaphoreForRequest.WaitTimeAsync(millisecondsTimeout, token);
            try
            {
                this.m_getContent = false;
                using (var byteBlock = new ByteBlock())
                {
                    request.Build(byteBlock);

                    this.Reset(token);
                    await this.ProtectedDefaultSendAsync(byteBlock.Memory);

                    var status =await this.m_waitDataAsync.WaitAsync(millisecondsTimeout).ConfigureFalseAwait();

                    status.ThrowIfNotRunning();

                    return new HttpResponseResult(this.m_waitData.WaitResult, this.ReleaseLock);
                }
            }
            catch
            {
                this.m_semaphoreForRequest.Release();
                throw;
            }
        }

        ///// <summary>
        ///// 发送Http请求，并等待全部响应
        ///// </summary>
        ///// <param name="request"></param>
        ///// <param name="millisecondsTimeout"></param>
        ///// <param name="token"></param>
        ///// <returns></returns>
        ///// <exception cref="TimeoutException"></exception>
        ///// <exception cref="OperationCanceledException"></exception>
        ///// <exception cref="Exception"></exception>
        //protected HttpResponseResult ProtectedRequestContent(HttpRequest request, int millisecondsTimeout = 10 * 1000, CancellationToken token = default)
        //{
        //    this.m_semaphoreForRequest.WaitTime(millisecondsTimeout, token);
        //    try
        //    {
        //        this.m_getContent = true;
        //        using (var byteBlock = new ByteBlock())
        //        {
        //            request.Build(byteBlock);

        //            this.Reset(token);

        //            this.ProtectedDefaultSend(byteBlock.Buffer, 0, byteBlock.Length);

        //            var status = this.m_waitData.Wait(millisecondsTimeout);

        //            status.ThrowIfNotRunning();

        //            return new HttpResponseResult(this.m_waitData.WaitResult, this.ReleaseLock);
        //        }
        //    }
        //    catch
        //    {
        //        this.m_semaphoreForRequest.Release();
        //        throw;
        //    }
        //}

        /// <summary>
        /// 异步发送Http请求，并等待全部响应
        /// </summary>
        /// <param name="request"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        /// <exception cref="Exception"></exception>
        protected async Task<HttpResponseResult> ProtectedRequestContentAsync(HttpRequest request, int millisecondsTimeout = 10 * 1000, CancellationToken token = default)
        {
            await this.m_semaphoreForRequest.WaitTimeAsync(millisecondsTimeout, token).ConfigureFalseAwait();
            try
            {
                this.m_getContent = true;
                using (var byteBlock = new ByteBlock())
                {
                    request.Build(byteBlock);

                    this.Reset(token);

                    await this.ProtectedDefaultSendAsync(byteBlock.Memory);

                    var status =await this.m_waitDataAsync.WaitAsync(millisecondsTimeout).ConfigureFalseAwait();

                    status.ThrowIfNotRunning();

                    return new HttpResponseResult(this.m_waitData.WaitResult, this.ReleaseLock);
                }
            }
            catch
            {
                this.m_semaphoreForRequest.Release();
                throw;
            }
        }

        #endregion Request

        #region override

        /// <inheritdoc/>
        protected override Task OnTcpConnecting(ConnectingEventArgs e)
        {
            this.Protocol = Protocol.Http;
            this.SetAdapter(new HttpClientDataHandlingAdapter());
            return EasyTask.CompletedTask;
        }

        /// <inheritdoc/>
        protected override Task OnTcpClosed(ClosedEventArgs e)
        {
            this.m_waitData?.Cancel();
            this.m_waitDataAsync?.Cancel();
            return EasyTask.CompletedTask;
        }

        /// <inheritdoc/>
        protected override async Task OnTcpReceived(ReceivedDataEventArgs e)
        {
            if (e.RequestInfo is HttpResponse response)
            {
                if (this.m_getContent)
                {
                    await response.GetContentAsync(CancellationToken.None);
                }
                this.Set(response);
            }
        }

        #endregion override

        private void Reset(CancellationToken token)
        {
            this.m_waitData.Reset();
            this.m_waitDataAsync.Reset();
            this.m_waitData.SetCancellationToken(token);
            this.m_waitDataAsync.SetCancellationToken(token);
        }

        private void Set(HttpResponse response)
        {
            this.m_waitData.Set(response);
            this.m_waitDataAsync.Set(response);
        }
    }
}