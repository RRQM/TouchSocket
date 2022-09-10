//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Threading;
using TouchSocket.Core;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Run;
using TouchSocket.Resources;
using TouchSocket.Sockets;

namespace TouchSocket.Http
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
        private readonly object m_requestLocker = new object();
        private bool m_getContent;
        private readonly WaitData<HttpResponse> m_waitData;

        /// <summary>
        /// 构造函数
        /// </summary>
        public HttpClientBase()
        {
            this.m_waitData = new WaitData<HttpResponse>();
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
            lock (this.m_requestLocker)
            {
                this.m_getContent = false;
                using (ByteBlock byteBlock = new ByteBlock())
                {
                    request.Build(byteBlock);

                    this.m_waitData.Reset();
                    this.m_waitData.SetCancellationToken(token);

                    this.DefaultSend(byteBlock);
                    if (onlyRequest)
                    {
                        return default;
                    }

                    switch (this.m_waitData.Wait(timeout))
                    {
                        case WaitDataStatus.SetRunning:
                            return this.m_waitData.WaitResult;

                        case WaitDataStatus.Overtime:
                            throw new TimeoutException(TouchSocketRes.Overtime.GetDescription());
                        case WaitDataStatus.Canceled:
                            return default;

                        case WaitDataStatus.Default:
                        case WaitDataStatus.Disposed:
                        default:
                            throw new Exception(TouchSocketRes.UnknownError.GetDescription());
                    }
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="request"></param>
        /// <param name="onlyRequest"></param>
        /// <param name="timeout"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public HttpResponse RequestContent(HttpRequest request, bool onlyRequest = false, int timeout = 10 * 1000, CancellationToken token = default)
        {
            lock (this.m_requestLocker)
            {
                this.m_getContent = true;
                using (ByteBlock byteBlock = new ByteBlock())
                {
                    request.Build(byteBlock);

                    this.m_waitData.Reset();
                    this.m_waitData.SetCancellationToken(token);

                    this.DefaultSend(byteBlock);
                    if (onlyRequest)
                    {
                        return default;
                    }

                    switch (this.m_waitData.Wait(timeout))
                    {
                        case WaitDataStatus.SetRunning:
                            return this.m_waitData.WaitResult;

                        case WaitDataStatus.Overtime:
                            throw new TimeoutException(TouchSocketRes.Overtime.GetDescription());
                        case WaitDataStatus.Canceled:
                            return default;

                        case WaitDataStatus.Default:
                        case WaitDataStatus.Disposed:
                        default:
                            throw new Exception(TouchSocketRes.UnknownError.GetDescription());
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
            this.m_waitData?.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public override ITcpClient Connect(int timeout = 5000)
        {
            if (this.Config.GetValue<HttpProxy>(HttpConfigExtensions.HttpProxyProperty) is HttpProxy httpProxy)
            {
                IPHost proxyHost = httpProxy.Host;
                var credential = httpProxy.Credential;
                IPHost remoteHost = this.Config.GetValue<IPHost>(TouchSocketConfigExtension.RemoteIPHostProperty);
                try
                {
                    this.Config.SetRemoteIPHost(proxyHost);
                    base.Connect(timeout);
                    HttpRequest httpRequest = new HttpRequest();
                    httpRequest.InitHeaders()
                        .SetHost(remoteHost.Host)
                        .SetUrl(remoteHost.Host, true)
                        .AsMethod("CONNECT");
                    var response = this.Request(httpRequest, timeout: timeout);
                    if (response.IsProxyAuthenticationRequired)
                    {
                        if (credential is null)
                        {
                            throw new Exception("未指定代理的凭据。");
                        }
                        string authHeader = response.GetHeader(HttpHeaders.ProxyAuthenticate);
                        if (authHeader.IsNullOrEmpty())
                        {
                            throw new Exception("未指定代理身份验证质询。");
                        }

                        var ares = new AuthenticationChallenge(authHeader, credential);

                        httpRequest.SetHeader(HttpHeaders.ProxyAuthorization, ares.ToString());
                        if (response.CloseConnection)
                        {
                            base.Close("代理要求关闭连接，随后重写连接。");
                            base.Connect(timeout);
                        }

                        response = this.Request(httpRequest, timeout: timeout);
                    }

                    if (response.StatusCode != "200")
                    {
                        throw new Exception(response.StatusMessage);
                    }
                }
                finally
                {
                    this.Config.SetRemoteIPHost(remoteHost);
                }
            }
            else
            {
                base.Connect(timeout);
            }
            return this;
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
                if (this.m_getContent)
                {
                    response.TryGetContent(out _);
                }
                this.m_waitData.Set(response);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="e"></param>
        protected override void OnConnecting(ClientConnectingEventArgs e)
        {
            this.Protocol = Protocol.Http;
            this.SetDataHandlingAdapter(new HttpClientDataHandlingAdapter());
            base.OnConnecting(e);
        }
    }
}