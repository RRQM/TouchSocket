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
        //private readonly AsyncAutoResetEvent m_waitRelease = new AsyncAutoResetEvent();
        private readonly WaitDataAsync<HttpResponse> m_waitResponseDataAsync = new WaitDataAsync<HttpResponse>();
        private bool m_getContent;
        private HttpClientDataHandlingAdapter m_dataHandlingAdapter;
        #endregion 字段

        internal Task InternalSendAsync(ReadOnlyMemory<byte> memory)
        {
            return this.ProtectedDefaultSendAsync(memory);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.m_semaphoreForRequest.Dispose();
                //this.m_waitRelease.Dispose();
                this.m_waitResponseDataAsync.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// 设置用于处理单流数据的转换适配器
        /// </summary>
        /// <param name="adapter">要设置的SingleStreamDataHandlingAdapter实例</param>
        protected void SetWarpAdapter(SingleStreamDataHandlingAdapter adapter)
        {
            // 将提供的适配器设置为当前数据处理适配器的WarpAdapter
            this.m_dataHandlingAdapter.WarpAdapter = adapter;
        }

        #region Request

        private void ReleaseLock()
        {
            this.m_semaphoreForRequest.Release();
            this.m_dataHandlingAdapter.SetCompleteLock();
            //this.m_waitRelease.Set();
        }

        /// <summary>
        /// 异步发送Http请求，并仅等待响应头
        /// </summary>
        /// <param name="request">要发送的HttpRequest对象</param>
        /// <param name="millisecondsTimeout">超时时间，单位为毫秒，默认为10秒</param>
        /// <param name="token">用于取消操作的CancellationToken</param>
        /// <returns>返回HttpResponseResult对象，包含响应结果和释放锁的方法</returns>
        /// <exception cref="TimeoutException">当操作超时时抛出</exception>
        /// <exception cref="OperationCanceledException">当操作被取消时抛出</exception>
        /// <exception cref="Exception">当发生其他异常时抛出</exception>
        protected async Task<HttpResponseResult> ProtectedRequestAsync(HttpRequest request, int millisecondsTimeout = 10 * 1000, CancellationToken token = default)
        {
            // 等待信号量，以控制并发请求的数量，超时和取消策略
            await this.m_semaphoreForRequest.WaitTimeAsync(millisecondsTimeout, token);
            try
            {
                // 标记不获取响应内容
                this.m_getContent = false;

                // 使用ByteBlock来构建请求数据
                using (var byteBlock = new ByteBlock())
                {
                    // 构建请求数据
                    request.Build(byteBlock);

                    // 重置等待状态，并使用取消令牌
                    this.Reset(token);

                    // 异步发送请求
                    await this.ProtectedDefaultSendAsync(byteBlock.Memory).ConfigureAwait(false);

                    // 等待响应状态，超时设定
                    var status = await this.m_waitResponseDataAsync.WaitAsync(millisecondsTimeout).ConfigureAwait(false);

                    // 如果响应状态不是运行中，抛出异常
                    status.ThrowIfNotRunning();

                    // 返回响应结果对象
                    return new HttpResponseResult(this.m_waitResponseDataAsync.WaitResult, this.ReleaseLock);
                }
            }
            catch
            {
                // 发生异常时，释放信号量
                this.m_semaphoreForRequest.Release();
                throw;
            }
        }

        /// <summary>
        /// 异步发送Http请求，并等待全部响应
        /// </summary>
        /// <param name="request">Http请求对象</param>
        /// <param name="millisecondsTimeout">超时时间，单位为毫秒，默认为10秒</param>
        /// <param name="token">取消令牌</param>
        /// <returns>返回Http响应结果</returns>
        /// <exception cref="TimeoutException">当操作超时时抛出</exception>
        /// <exception cref="OperationCanceledException">当操作被取消时抛出</exception>
        /// <exception cref="Exception">当发生其他异常时抛出</exception>
        protected async Task<HttpResponseResult> ProtectedRequestContentAsync(HttpRequest request, int millisecondsTimeout = 10 * 1000, CancellationToken token = default)
        {
            // 使用信号量控制并发，确保系统稳定性
            await this.m_semaphoreForRequest.WaitTimeAsync(millisecondsTimeout, token).ConfigureAwait(false);
            try
            {
                // 标记为获取内容状态
                this.m_getContent = true;
                // 使用ByteBlock来构建请求数据
                using (var byteBlock = new ByteBlock())
                {
                    // 构建请求数据
                    request.Build(byteBlock);

                    // 重置状态，为发送请求做准备
                    this.Reset(token);

                    // 异步发送请求数据
                    await this.ProtectedDefaultSendAsync(byteBlock.Memory).ConfigureAwait(false);

                    // 等待响应数据，超时时间为millisecondsTimeout
                    var status = await this.m_waitResponseDataAsync.WaitAsync(millisecondsTimeout).ConfigureAwait(false);

                    // 如果状态不是运行中，则抛出异常
                    status.ThrowIfNotRunning();

                    // 返回响应结果
                    return new HttpResponseResult(this.m_waitResponseDataAsync.WaitResult, this.ReleaseLock);
                }
            }
            catch
            {
                // 发生异常时，释放信号量
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
            this.m_dataHandlingAdapter = new HttpClientDataHandlingAdapter();
            this.SetAdapter(this.m_dataHandlingAdapter);
            return EasyTask.CompletedTask;
        }

        /// <inheritdoc/>
        protected override Task OnTcpClosed(ClosedEventArgs e)
        {
            this.m_waitResponseDataAsync.Cancel();
            return EasyTask.CompletedTask;
        }

        /// <inheritdoc/>
        protected override async Task OnTcpReceived(ReceivedDataEventArgs e)
        {
            if (e.RequestInfo is HttpResponse response)
            {
                if (this.m_getContent)
                {
                    await response.GetContentAsync(CancellationToken.None).ConfigureAwait(false);
                }
                this.m_waitResponseDataAsync.Set(response);
                //await this.SetAsync(response).ConfigureAwait(false);
            }
        }

        #endregion override

        private void Reset(in CancellationToken token)
        {
            //this.m_waitRelease.Reset();
            this.m_waitResponseDataAsync.Reset();
            this.m_waitResponseDataAsync.SetCancellationToken(token);
        }
    }
}