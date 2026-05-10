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

using System.Net.Http;
using System.Net.WebSockets;

namespace TouchSocket.SocketIo;

/// <summary>
/// Socket.IO 客户端实现。
/// </summary>
public class SocketIoClient : SetupConfigObject, ISocketIoClient
{
    private readonly SocketIoCore m_socketIo;
    private CancellationTokenSource m_closedTokenSource = new CancellationTokenSource();
    private bool m_online;
    private SocketIoOption m_option;
    private ISocketIoTransport m_transport;
    private readonly SemaphoreSlim m_semaphoreSlimForConnect = new SemaphoreSlim(1, 1);
    private HttpClient m_httpClient;
    private ClientWebSocket m_clientWebSocket;

    public SocketIoClient()
    {
        this.m_socketIo = new SocketIoCore()
        {
            SendAsyncAction = this.SendAsync
        };
    }

    #region 属性

    public EngineIoVersion EIO => this.m_socketIo.EIO;
    public string Namespace => this.m_socketIo.Namespace;
    public bool Online => this.m_online;
    public IPHost RemoteIpHost { get; private set; }
    public ISocketIoTransport Transport => this.m_transport;
    public EngineIoTransportType TransportType => this.m_socketIo.TransportType;

    public string Id => this.m_socketIo.Sid;

    /// <inheritdoc/>
    public Protocol Protocol => Protocol.WebSocket;

    /// <inheritdoc/>
    public DateTimeOffset LastReceivedTime { get; private set; }

    /// <inheritdoc/>
    public DateTimeOffset LastSentTime { get; private set; }

    /// <inheritdoc/>
    public bool IsClient => true;

    /// <inheritdoc/>
    public CancellationToken ClosedToken => this.m_closedTokenSource.Token;

    #endregion 属性

    #region Emit

    public Task AckAsync(int packetId, params object[] data)
    {
        return this.m_socketIo.AckAsync(packetId, data);
    }

    public Task EmitAsync(string eventName, params object[] data)
    {
        return this.m_socketIo.EmitAsync(eventName, data);
    }

    public async Task<ISocketIoResponse> EmitWithAckAsync(string eventName, object[] data, int millisecondsTimeout, CancellationToken token)
    {
        return await this.m_socketIo.EmitWithAckAsync(eventName, data, millisecondsTimeout, token);
    }

    #endregion Emit

    #region 事件

    protected virtual async Task OnHandshaked()
    {
        try
        {
            var e = new SocketIoVerifyEventArgs();
            await this.PluginManager.RaiseISocketIoHandshakedPluginAsync(this.Resolver, this, e);
        }
        catch
        {
        }

    }

    protected virtual async Task OnHandshaking(SocketIoVerifyEventArgs e)
    {
        await this.PluginManager.RaiseISocketIoHandshakingPluginAsync(this.Resolver, this, e);
    }

    #endregion 事件

    public string BuildUrl(bool isWebSocket)
    {
        Uri baseUri = this.RemoteIpHost;
        var urlBuilder = new StringBuilder();

        // Determine the scheme based on whether it's a WebSocket or not.
        var scheme = isWebSocket ? (baseUri.Scheme == "https" ? "wss" : "ws") : baseUri.Scheme;
        urlBuilder.Append(scheme);
        urlBuilder.Append("://");

        // Append the host and port if necessary.
        urlBuilder.Append(baseUri.Host);
        if (baseUri.Port != 80 && baseUri.Port != 443)
        {
            urlBuilder.Append(":").Append(baseUri.Port);
        }

        // Append the path and query string.
        var path = baseUri.AbsolutePath;
        if (string.IsNullOrEmpty(path) || path == "/")
        {
            path = "/socket.io/";
        }
        urlBuilder.Append(path);

        urlBuilder.Append($"?EIO={(int)this.EIO}&");
        urlBuilder.Append($"transport={this.TransportType.ToString().ToLower()}&");
        urlBuilder.Append($"t={DateTimeOffset.Now.ToUnsignedMillis()}");

        if (this.m_option.Query != null)
        {
            foreach (var kv in this.m_option.Query)
            {
                urlBuilder.Append('&').Append(kv.Key).Append('=').Append(kv.Value);
            }
        }

        if (this.m_socketIo.Sid.HasValue())
        {
            urlBuilder.Append("&sid=").Append(this.m_socketIo.Sid);
        }

        return urlBuilder.ToString();
    }

    /// <inheritdoc/>
    public async Task ConnectAsync(CancellationToken token)
    {
        await this.m_semaphoreSlimForConnect.WaitAsync(token).ConfigureAwait(false);
        try
        {
            if (this.m_online)
            {
                return;
            }

            this.m_closedTokenSource = new CancellationTokenSource();
            await this.OnHandshaking(new SocketIoVerifyEventArgs()).ConfigureAwait(false);
            if (this.TransportType == EngineIoTransportType.Polling)
            {
                //使用长轮询
                if (this.EIO == EngineIoVersion.V3)
                {
                    await this.ConnectWithHttpEIO3(token).ConfigureAwait(false);
                }
                else
                {
                    await this.ConnectWithHttpEIO4(token).ConfigureAwait(false);
                }
            }
            else if (this.TransportType == EngineIoTransportType.WebSocket)
            {
                //直接使用ws
                await this.ConnectWithWebSocket(token).ConfigureAwait(false);
            }
            this.m_online = true;
            _ = Task.Factory.StartNew(this.OnHandshaked);
        }
        finally
        {
            this.m_semaphoreSlimForConnect.Release();
        }
    }

    protected override void LoadConfig(TouchSocketConfig config)
    {
        this.RemoteIpHost = config.GetValue(TouchSocketConfigExtension.RemoteIPHostProperty);
        var option = config.GetValue(SocketIoConfigExtension.SocketIoOptionProperty) ?? throw new ArgumentNullException(nameof(SocketIoConfigExtension.SocketIoOptionProperty));

        this.m_socketIo.EIO = option.EIO;
        this.m_socketIo.TransportType = option.Transport;
        this.m_option = option;
        this.m_socketIo.Namespace = option.Namespace;
        base.LoadConfig(config);
    }

    private async Task Connect2(HttpClient httpClient)
    {
        await Task.Yield();
        var request = new HttpRequestMessage(HttpMethod.Post, this.BuildUrl(false));
        request.Content = new StringContent($"40{this.m_option.Namespace}");

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
    }

    private async Task ConnectWithHttpEIO3(CancellationToken token)
    {
        var httpClient = this.CreateHttpClient();

        var request = this.GetRequest(HttpMethod.Get);

        var response = await httpClient.SendAsync(request, token);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        if (body.IsNullOrEmpty())
        {
            throw new ArgumentNullException(nameof(body), "获得的握手响应数据为空，可能对方并不是SocketIo服务器。");
        }
        var values = SocketIoUtility.SplitEIO3(body);

        var eioMessage = this.m_socketIo.Decode(values[0]);
        if (eioMessage.MessageType == EngineIoMessageType.Open)
        {
            var handshakeMessage = this.m_socketIo.DeserializeHandshakeMessage(eioMessage);

            if (handshakeMessage.UpgradeWebSocket() && this.m_option.AutoUpgrade)
            {
                this.m_socketIo.TransportType = EngineIoTransportType.WebSocket;
                var webSocketClient = this.GetWebSocket();
                //webSocketClient.Setup(this.Config.Clone());
                await webSocketClient.ConnectAsync(new Uri(this.BuildUrl(true)), token);
                try
                {
                    body = await webSocketClient.ReadAsStringAsync(token);
                    var eioMessage11 = this.m_socketIo.Decode(body);
                    handshakeMessage = this.m_socketIo.DeserializeHandshakeMessage(eioMessage11);
                    this.m_socketIo.Sid = handshakeMessage.Sid;

                    this.m_transport = new WebSocketTransport(this.m_socketIo, webSocketClient, this.ReceivedSocketIoMessage);
                    _ = Task.Factory.StartNew(this.m_transport.BeginPolling);
                    await webSocketClient.SendAsync("40", token);
                }
                catch
                {
                    webSocketClient.SafeDispose();
                    throw;
                }
            }

            if (this.m_socketIo.Decode(values[1]).MessageType == EngineIoMessageType.Message)
            {
                this.m_socketIo.Sid = handshakeMessage.Sid;

                this.m_transport = new Eio3HttpSockeIoTransport(this.m_socketIo, httpClient, this.GetRequest, this.ReceivedSocketIoMessage);
                _ = Task.Factory.StartNew(this.m_transport.BeginPolling);
            }
        }
    }

    private async Task ConnectWithHttpEIO4(CancellationToken token)
    {
        var httpClient = this.CreateHttpClient();

        var request = this.GetRequest(HttpMethod.Get);

        var response = await httpClient.SendAsync(request, token);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        if (body.IsNullOrEmpty())
        {
            throw new ArgumentNullException(nameof(body), "获得的握手响应数据为空，可能对方并不是SocketIo服务器。");
        }

        var eioMessage = this.m_socketIo.Decode(body);
        if (eioMessage.MessageType != EngineIoMessageType.Open)
        {
            throw new Exception();
        }

        var handshakeMessage = this.m_socketIo.DeserializeHandshakeMessage(eioMessage);

        if (handshakeMessage.UpgradeWebSocket() && this.m_option.AutoUpgrade)
        {
            this.m_socketIo.TransportType = EngineIoTransportType.WebSocket;
            var webSocketClient = this.GetWebSocket();
            await webSocketClient.ConnectAsync(new Uri(this.BuildUrl(true)), token);
            try
            {
                body = await webSocketClient.ReadAsStringAsync(token);
                var eioMessage11 = this.m_socketIo.Decode(body);
                handshakeMessage = this.m_socketIo.DeserializeHandshakeMessage(eioMessage11);
                this.m_socketIo.Sid = handshakeMessage.Sid;

                this.m_transport = new WebSocketTransport(this.m_socketIo, webSocketClient, this.ReceivedSocketIoMessage);
                _ = Task.Factory.StartNew(this.m_transport.BeginPolling);
                await webSocketClient.SendAsync("40");
            }
            catch
            {
                webSocketClient.SafeDispose();
                webSocketClient.SafeDispose();
                throw;
            }
        }
        else
        {
            this.m_socketIo.Sid = handshakeMessage.Sid;

            var url = this.BuildUrl(false);

            request = new HttpRequestMessage(HttpMethod.Get, url);

            var task = this.Connect2(httpClient);
            var response2 = await httpClient.SendAsync(request, token);
            response2.EnsureSuccessStatusCode();

            await task;
            this.m_transport = new Eio4HttpSockeIoTransport(this.m_socketIo, httpClient, this.GetRequest, this.ReceivedSocketIoMessage);

            _ = Task.Factory.StartNew(this.m_transport.BeginPolling);
        }
    }

    private async Task ConnectWithWebSocket(CancellationToken token)
    {
        this.m_socketIo.TransportType = EngineIoTransportType.WebSocket;
        var webSocketClient = this.GetWebSocket();

        await webSocketClient.ConnectAsync(new Uri(this.BuildUrl(true)), token);

        try
        {
            var body = await webSocketClient.ReadAsStringAsync(token);
            var eioMessage = this.m_socketIo.Decode(body);
            var handshakeMessage = this.m_socketIo.DeserializeHandshakeMessage(eioMessage);
            this.m_socketIo.Sid = handshakeMessage.Sid;

            this.m_transport = new WebSocketTransport(this.m_socketIo, webSocketClient, this.ReceivedSocketIoMessage);
            _ = Task.Factory.StartNew(this.m_transport.BeginPolling);
            await webSocketClient.SendAsync("40", token);
            //this.m_client = webSocketClient;
        }
        catch
        {
            webSocketClient.SafeDispose();
            webSocketClient.SafeDispose();
            throw;
        }
    }

    private HttpClient CreateHttpClient()
    {
        this.m_httpClient ??= new HttpClient();
        return this.m_httpClient;
    }

    private HttpRequestMessage GetRequest(HttpMethod method)
    {
        var request = new HttpRequestMessage();
        request.Method = method;
        request.RequestUri = new Uri(this.BuildUrl(false));
        return request;
    }

    private ClientWebSocket GetWebSocket()
    {
        if (this.m_clientWebSocket == null || this.m_clientWebSocket.State != WebSocketState.None)
        {
            this.m_clientWebSocket?.Dispose();
            this.m_clientWebSocket = new ClientWebSocket();
        }

        return this.m_clientWebSocket;
    }

    private async Task ReceivedSocketIoMessage(ISocketIoMessage socketIOMessage)
    {
        this.LastReceivedTime = DateTimeOffset.Now;

        switch (socketIOMessage.MessageType)
        {
            case SocketIoMessageType.Connected:
                break;

            case SocketIoMessageType.Disconnected:
                break;

            case SocketIoMessageType.Binary:
            case SocketIoMessageType.Event:
                {
                    await this.PluginManager.RaiseISocketIoEventPluginAsync(this.Resolver, this, new SocketIoEventArgs(socketIOMessage, this.m_socketIo));
                }
                break;

            case SocketIoMessageType.Error:
                break;

            case SocketIoMessageType.Ack:
                {
                    await this.m_socketIo.ReceivedAck(socketIOMessage);
                    break;
                }
            case SocketIoMessageType.BinaryAck:
                {
                    await this.m_socketIo.ReceivedBinaryAck(socketIOMessage);
                    break;
                }

            default:
                break;
        }
    }

    #region Send

    private Task SendAsync(List<DataItem> dataItems)
    {
        this.LastSentTime = DateTimeOffset.Now;
        return this.m_transport.SendAsync(dataItems);
    }

    #endregion Send

    /// <inheritdoc/>
    public async Task<Result> CloseAsync(string msg, CancellationToken cancellationToken = default)
    {
        try
        {
            this.m_online = false;
            this.m_closedTokenSource.SafeCancel();

            if (this.m_clientWebSocket != null && this.m_clientWebSocket.State == WebSocketState.Open)
            {
                await this.m_clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, msg, cancellationToken);
            }

            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    /// <inheritdoc/>
    protected override void SafetyDispose(bool disposing)
    {
        this.m_online = false;
        if (disposing)
        {
            this.m_closedTokenSource.SafeCancel();
            this.m_closedTokenSource.SafeDispose();
            this.m_clientWebSocket?.Dispose();
            this.m_httpClient?.Dispose();
            this.m_semaphoreSlimForConnect?.Dispose();
        }

        base.SafetyDispose(disposing);
    }

    /// <inheritdoc/>
    public Task ResetIdAsync(string newId, CancellationToken cancellationToken = default)
    {
        this.m_socketIo.Sid = newId;
        return EasyTask.CompletedTask;
    }
}