//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
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
            this.waitData?.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        protected override void HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            base.HandleReceivedData(byteBlock, requestInfo);

            if (requestInfo is HttpResponse response)
            {
                this.waitData.Set(response);
            } 
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