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

    public Task AckAsync(int packetId, object[] data,CancellationToken cancellationToken=default)
    {
        return this.m_socketIo.AckAsync(packetId, data, cancellationToken);
    }

    public Task EmitAsync(string eventName, object[] data, CancellationToken cancellationToken = default)
    {
        return this.m_socketIo.EmitAsync(eventName, data, cancellationToken);
    }

    public async Task<ISocketIoResponse> EmitWithAckAsync(string eventName, object[] data, CancellationToken token)
    {
        return await this.m_socketIo.EmitWithAckAsync(eventName, data, token);
    }

    #endregion Emit

    #region 事件

    /// <summary>
    /// 在即将建立 Socket.IO 连接时触发，可用于验证或拦截。
    /// </summary>
    protected virtual async Task OnConnecting(SocketIoVerifyEventArgs e)
    {
        await this.PluginManager.RaiseISocketIoConnectingPluginAsync(this.Resolver, this, e);
    }

    /// <summary>
    /// 在 Socket.IO 连接成功后触发。
    /// </summary>
    protected virtual async Task OnConnected()
    {
        try
        {
            var e = new SocketIoConnectedEventArgs();
            await this.PluginManager.RaiseISocketIoConnectedPluginAsync(this.Resolver, this, e);
        }
        catch
        {
        }
    }

    /// <summary>
    /// 在 Socket.IO 连接即将关闭时触发（仅主动关闭时有效）。
    /// </summary>
    protected virtual async Task OnClosing(SocketIoClosedEventArgs e)
    {
        try
        {
            await this.PluginManager.RaiseISocketIoClosingPluginAsync(this.Resolver, this, e);
        }
        catch
        {
        }
    }

    /// <summary>
    /// 在 Socket.IO 连接已关闭后触发。
    /// </summary>
    protected virtual async Task OnClosed(SocketIoClosedEventArgs e)
    {
        try
        {
            await this.PluginManager.RaiseISocketIoClosedPluginAsync(this.Resolver, this, e);
        }
        catch
        {
        }
    }

    #endregion 事件

    /// <summary>
    /// 构建连接 URL。
    /// </summary>
    /// <param name="isWebSocket">是否为 WebSocket 连接。</param>
    public string BuildUrl(bool isWebSocket)
    {
        Uri baseUri = this.RemoteIpHost;
        var urlBuilder = new StringBuilder();

        var scheme = isWebSocket ? (baseUri.Scheme == "https" ? "wss" : "ws") : baseUri.Scheme;
        urlBuilder.Append(scheme).Append("://").Append(baseUri.Host);

        if (baseUri.Port != 80 && baseUri.Port != 443)
        {
            urlBuilder.Append(':').Append(baseUri.Port);
        }

        var path = baseUri.AbsolutePath;
        if (string.IsNullOrEmpty(path) || path == "/")
        {
            path = "/socket.io/";
        }
        urlBuilder.Append(path);

        urlBuilder.Append("?EIO=").Append((int)this.EIO)
            .Append("&transport=").Append(this.TransportType.ToString().ToLower())
            .Append("&t=").Append(DateTimeOffset.Now.ToUnsignedMillis());

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
        await this.m_semaphoreSlimForConnect.WaitAsync(token).ConfigureDefaultAwait();
        try
        {
            if (this.m_online)
            {
                return;
            }

            this.m_closedTokenSource = new CancellationTokenSource();
            await this.OnConnecting(new SocketIoVerifyEventArgs()).ConfigureDefaultAwait();
            if (this.TransportType == EngineIoTransportType.Polling)
            {
                //使用长轮询
                if (this.EIO == EngineIoVersion.V3)
                {
                    await this.ConnectWithHttpEIO3(token).ConfigureDefaultAwait();
                }
                else
                {
                    await this.ConnectWithHttpEIO4(token).ConfigureDefaultAwait();
                }
            }
            else if (this.TransportType == EngineIoTransportType.WebSocket)
            {
                //直接使用ws
                await this.ConnectWithWebSocket(token).ConfigureDefaultAwait();
            }
            this.m_online = true;
            await this.OnConnected().ConfigureDefaultAwait();
        }
        finally
        {
            this.m_semaphoreSlimForConnect.Release();
        }
    }

    /// <inheritdoc/>
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

    private async Task SendNamespaceConnectAsync(HttpClient httpClient)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, this.BuildUrl(false));
        request.Content = new StringContent($"40{this.m_option.Namespace}");
        var response = await httpClient.SendAsync(request).ConfigureDefaultAwait();
        response.EnsureSuccessStatusCode();
    }

    private async Task ConnectWithHttpEIO3(CancellationToken token)
    {
        var httpClient = this.CreateHttpClient();
        var response = await httpClient.SendAsync(this.GetRequest(HttpMethod.Get), token).ConfigureDefaultAwait();
        response.EnsureSuccessStatusCode();

        var bodyBytes = await response.Content.ReadAsByteArrayAsync().ConfigureDefaultAwait();
        if (bodyBytes.Length == 0)
        {
            throw new ArgumentNullException(nameof(bodyBytes), "获得的握手响应数据为空，可能对方并不是SocketIo服务器。");
        }
        var values = SocketIoUtility.SplitEIO3(bodyBytes.AsMemory());

        var eioMessage = this.m_socketIo.Decode(values[0]);
        if (eioMessage.MessageType != EngineIoMessageType.Open)
        {
            return;
        }

        var handshakeMessage = this.m_socketIo.DeserializeHandshakeMessage(eioMessage);
        if (handshakeMessage.UpgradeWebSocket() && this.m_option.AutoUpgrade)
        {
            this.m_socketIo.TransportType = EngineIoTransportType.WebSocket;
            var webSocketClient = this.GetWebSocket();
            await webSocketClient.ConnectAsync(new Uri(this.BuildUrl(true)), token).ConfigureDefaultAwait();
            try
            {
                await this.SetupWebSocketTransportAsync(webSocketClient, token).ConfigureDefaultAwait();
            }
            catch
            {
                webSocketClient.SafeDispose();
                throw;
            }
        }
        else
        {
            this.m_socketIo.Sid = handshakeMessage.Sid;
            this.m_transport = new Eio3HttpSockeIoTransport(this.m_socketIo, httpClient, this.GetRequest, this.ReceivedSocketIoMessage);
            _ = Task.Run(() => this.m_transport.BeginPolling(this.m_closedTokenSource.Token));
        }
    }

    private async Task ConnectWithHttpEIO4(CancellationToken token)
    {
        var httpClient = this.CreateHttpClient();
        var response = await httpClient.SendAsync(this.GetRequest(HttpMethod.Get), token).ConfigureDefaultAwait();
        response.EnsureSuccessStatusCode();

        var bodyBytes = await response.Content.ReadAsByteArrayAsync().ConfigureDefaultAwait();
        if (bodyBytes.Length == 0)
        {
            throw new ArgumentNullException(nameof(bodyBytes), "获得的握手响应数据为空，可能对方并不是SocketIo服务器。");
        }

        var eioMessage = this.m_socketIo.Decode(bodyBytes.AsMemory());
        if (eioMessage.MessageType != EngineIoMessageType.Open)
        {
            throw new InvalidOperationException("握手响应类型不正确，可能对方并不是SocketIo服务器。");
        }

        var handshakeMessage = this.m_socketIo.DeserializeHandshakeMessage(eioMessage);
        if (handshakeMessage.UpgradeWebSocket() && this.m_option.AutoUpgrade)
        {
            this.m_socketIo.TransportType = EngineIoTransportType.WebSocket;
            var webSocketClient = this.GetWebSocket();
            await webSocketClient.ConnectAsync(new Uri(this.BuildUrl(true)), token).ConfigureDefaultAwait();
            try
            {
                await this.SetupWebSocketTransportAsync(webSocketClient, token).ConfigureDefaultAwait();
            }
            catch
            {
                webSocketClient.SafeDispose();
                throw;
            }
        }
        else
        {
            this.m_socketIo.Sid = handshakeMessage.Sid;

            var getTask = httpClient.SendAsync(this.GetRequest(HttpMethod.Get), token);
            var postTask = this.SendNamespaceConnectAsync(httpClient);
            var getResponse = await getTask.ConfigureDefaultAwait();
            getResponse.EnsureSuccessStatusCode();
            await postTask.ConfigureDefaultAwait();

            this.m_transport = new Eio4HttpSockeIoTransport(this.m_socketIo, httpClient, this.GetRequest, this.ReceivedSocketIoMessage);
            _ = Task.Run(() => this.m_transport.BeginPolling(this.m_closedTokenSource.Token));
        }
    }

    private async Task ConnectWithWebSocket(CancellationToken token)
    {
        this.m_socketIo.TransportType = EngineIoTransportType.WebSocket;
        var webSocketClient = this.GetWebSocket();
        await webSocketClient.ConnectAsync(new Uri(this.BuildUrl(true)), token).ConfigureDefaultAwait();
        try
        {
            await this.SetupWebSocketTransportAsync(webSocketClient, token).ConfigureDefaultAwait();
        }
        catch
        {
            webSocketClient.SafeDispose();
            throw;
        }
    }

    private async Task SetupWebSocketTransportAsync(ClientWebSocket webSocketClient, CancellationToken token)
    {
        var bodyBytes = await webSocketClient.ReadAsBytesAsync(token).ConfigureDefaultAwait();
        var eioMessage = this.m_socketIo.Decode(bodyBytes.AsMemory());
        var handshakeMessage = this.m_socketIo.DeserializeHandshakeMessage(eioMessage);
        this.m_socketIo.Sid = handshakeMessage.Sid;

        this.m_transport = new WebSocketTransport(this.m_socketIo, webSocketClient, this.ReceivedSocketIoMessage);
        _ = Task.Run(() => this.m_transport.BeginPolling(this.m_closedTokenSource.Token));
        var nsp = this.m_socketIo.Namespace;
        if (!string.IsNullOrEmpty(nsp))
        {
            // 自定义命名空间：需要显式发送 connect 包
            await webSocketClient.SendAsync($"40{nsp},", token).ConfigureDefaultAwait();
        }
        else if (this.m_socketIo.EIO == EngineIoVersion.V4)
        {
            // EIO v4 + 默认命名空间：服务端不会自动连接，需要显式发送 connect 包
            await webSocketClient.SendAsync("40", token).ConfigureDefaultAwait();
        }
        // EIO v3 + 默认命名空间：服务端会自动连接到默认命名空间，无需显式发送 connect 包
    }

    private HttpClient CreateHttpClient()
    {
        this.m_httpClient ??= new HttpClient();
        return this.m_httpClient;
    }

    private HttpRequestMessage GetRequest(HttpMethod method)
    {
        var request = new HttpRequestMessage
        {
            Method = method,
            RequestUri = new Uri(this.BuildUrl(false))
        };
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
                    await this.PluginManager.RaiseISocketIoReceivedPluginAsync(this.Resolver, this, new SocketIoEventArgs(socketIOMessage, this.m_socketIo));
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

    private Task SendAsync(List<DataItem> dataItems,CancellationToken cancellationToken)
    {
        this.LastSentTime = DateTimeOffset.Now;
        return this.m_transport.SendAsync(dataItems, cancellationToken);
    }

    #endregion Send

    /// <inheritdoc/>
    public async Task<Result> CloseAsync(string msg, CancellationToken cancellationToken = default)
    {
        try
        {
            var closedArgs = new SocketIoClosedEventArgs(msg);
            await this.OnClosing(closedArgs).ConfigureDefaultAwait();

            this.m_online = false;
            this.m_closedTokenSource.SafeCancel();

            if (this.m_clientWebSocket != null && this.m_clientWebSocket.State == WebSocketState.Open)
            {
                await this.m_clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, msg, cancellationToken);
            }

            await this.OnClosed(closedArgs).ConfigureDefaultAwait();
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