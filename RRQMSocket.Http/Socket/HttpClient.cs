using RRQMCore;
using RRQMCore.ByteManager;
using RRQMCore.Run;
using System;
using System.Threading;

namespace RRQMSocket.Http
{
    /// <summary>
    /// Http客户端
    /// </summary>
    public class HttpClient : HttpClientBase
    {
    }

    /// <summary>
    /// Http客户端基类
    /// </summary>
    public class HttpClientBase : TcpClientBase, IHttpClient
    {
        private WaitData<HttpResponse> waitData;
        /// <summary>
        /// 构造函数
        /// </summary>
        public HttpClientBase()
        {
            this.Protocol = Protocol.Http;
            this.waitData = new WaitData<HttpResponse>();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="request"><inheritdoc/></param>
        /// <param name="onlyRequest"><inheritdoc/></param>
        /// <param name="timeout"><inheritdoc/></param>
        /// <param name="token"><inheritdoc/></param>
        /// <returns></returns>
        public HttpResponse Request(HttpRequest request, bool onlyRequest = false, int timeout = 10 * 1000, CancellationToken token = default)
        {
            lock (this)
            {
                using (ByteBlock byteBlock = new ByteBlock())
                {
                    request.Build(byteBlock);

                    this.waitData.Reset();
                    this.waitData.SetCancellationToken(token);

                    this.DefaultSend(byteBlock);
                    if (onlyRequest)
                    {
                        return default;
                    }
                   
                    switch (this.waitData.Wait(timeout))
                    {
                        case WaitDataStatus.SetRunning:
                            return this.waitData.WaitResult;

                        case WaitDataStatus.Overtime:
                            throw new TimeoutException(ResType.Overtime.GetResString());
                        case WaitDataStatus.Canceled:
                            return default;

                        case WaitDataStatus.Default:
                        case WaitDataStatus.Disposed:
                        default:
                            throw new RRQMException(ResType.UnknownError.GetResString());
                    }
                }
            }
        }
       
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            this.waitData.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        protected override void HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            if (requestInfo is HttpResponse response)
            {
                this.waitData.Set(response);
            }
            base.HandleReceivedData(byteBlock, requestInfo);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="e"></param>
        protected override void OnConnecting(ClientConnectingEventArgs e)
        {
            e.DataHandlingAdapter = new HttpClientDataHandlingAdapter(this.MaxPackageSize);
            base.OnConnecting(e);
        }
    }
}