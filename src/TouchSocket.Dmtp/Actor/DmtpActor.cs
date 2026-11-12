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

using System.Collections.Concurrent;
using TouchSocket.Resources;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp;

/// <summary>
/// 提供Dmtp协议的最基础功能件
/// </summary>
public abstract class DmtpActor : DisposableObject, IDmtpActor
{
    #region 委托

    /// <summary>
    /// 即将关闭时触发，仅主动关闭的一方触发。
    /// </summary>
    public Func<DmtpActor, string, Task> Closing { get; init; }

    /// <summary>
    /// 连接已完全关闭后触发（主动方与被动方均会触发）。
    /// </summary>
    public Func<DmtpActor, ClosedEventArgs, Task> Closed { get; init; }

    /// <summary>
    /// 在完成握手连接时
    /// </summary>
    public Func<DmtpActor, DmtpVerifyEventArgs, Task> Connected { get; init; }

    /// <summary>
    /// 握手
    /// </summary>
    public Func<DmtpActor, DmtpVerifyEventArgs, Task> Connecting { get; init; }

    /// <summary>
    /// 当创建通道时
    /// </summary>
    public Func<DmtpActor, CreateChannelEventArgs, Task> CreatedChannel { get; init; }

    /// <summary>
    /// 查找其他IDmtpActor
    /// </summary>
    public Func<string, Task<IDmtpActor>> FindDmtpActor { get; init; }

    /// <summary>
    /// 重设Id
    /// </summary>
    public Func<DmtpActor, IdChangedEventArgs, Task> IdChanged { get; init; }

    /// <summary>
    /// 异步发送数据接口
    /// </summary>
    public Func<DmtpActor, ReadOnlyMemory<byte>, CancellationToken, Task> OutputSendAsync { get; init; }

    /// <summary>
    /// 当需要路由的时候
    /// </summary>
    public Func<DmtpActor, PackageRouterEventArgs, Task> Routing { get; init; }

    /// <summary>
    /// 指定的传输写入器
    /// </summary>
    public ITransportWriter TransportWriter { get; set; }

    /// <summary>
    /// 最大包大小
    /// </summary>
    public long MaxPackageSize { get; set; }

    #endregion 委托

    #region 属性

    /// <inheritdoc/>
    public bool AllowRoute { get; }

    /// <inheritdoc/>
    public IDmtpActorObject Client { get; set; }

    /// <inheritdoc/>
    public CancellationToken ClosedToken => this.m_cancellationTokenSource?.Token ?? CancellationToken.None;

    /// <inheritdoc/>
    public string Id { get => this.m_id; init => this.m_id = value; }

    /// <inheritdoc/>
    public bool IsReliable { get; }

    /// <inheritdoc/>
    public DateTimeOffset LastActiveTime { get; protected set; }

    /// <inheritdoc/>
    public ILog Logger { get; set; }

    /// <summary>
    /// 获取Dmtp关闭时的消息。仅在收到远程关闭报文时有效。
    /// </summary>
    public string ClosedMessage => this.m_closedMessage;

    /// <inheritdoc/>
    public virtual bool Online => this.m_online;

    /// <inheritdoc/>
    public WaitHandlePool<IWaitResult> WaitHandlePool { get; }

    #endregion 属性

    #region 字段

    private readonly Dictionary<Type, IActor> m_actors = new Dictionary<Type, IActor>();
    private readonly AsyncManualResetEvent m_handshakeFinished = new(false);
    private readonly Lock m_syncRoot = new Lock();
    private readonly ConcurrentDictionary<int, InternalChannel> m_userChannels = new ConcurrentDictionary<int, InternalChannel>();
    private CancellationTokenSource m_cancellationTokenSource;
    private string m_closedMessage;
    private volatile bool m_online;
    private bool m_connecting;
    private bool m_finalized;
    private TaskCompletionSource<Result> m_closeCompletion;
    private static readonly AsyncLocal<DmtpActor> s_closingActor = new();
    private string m_id;
    // 0=可用，1=关闭中。
    private int m_closeState;

    #endregion 字段

    /// <summary>
    /// 创建一个Dmtp协议的最基础功能件
    /// </summary>
    /// <param name="allowRoute">是否允许路由</param>
    /// <param name="isReliable">是不是基于可靠协议运行的</param>
    public DmtpActor(bool allowRoute, bool isReliable)
    {
        this.WaitHandlePool = new WaitHandlePool<IWaitResult>();
        this.AllowRoute = allowRoute;
        this.LastActiveTime = DateTimeOffset.UtcNow;
        this.IsReliable = isReliable;
    }

    /// <summary>
    /// 创建一个可靠协议的Dmtp协议的最基础功能件
    /// </summary>
    /// <param name="allowRoute">指示是否允许路由</param>
    public DmtpActor(bool allowRoute) : this(allowRoute, true)
    {
    }

    /// <summary>
    /// 建立对点
    /// </summary>
    public virtual async Task ConnectAsync(DmtpOption dmtpOption, CancellationToken cancellationToken)
    {
        this.ThrowIfDisposed();
        ThrowHelper.ThrowIfNull(dmtpOption, nameof(dmtpOption));
        if (this.Online)
        {
            return;
        }
        this.BeginConnection();
        try
        {
            var args = new DmtpVerifyEventArgs
            {
                Token = dmtpOption.VerifyToken,
                Id = dmtpOption.Id,
                Metadata = dmtpOption.Metadata,
                IsPermitOperation = true
            };
            await this.OnConnecting(args).ConfigureDefaultAwait();
            if (!args.IsPermitOperation)
            {
                throw new TokenVerifyException(args.Metadata, args.Message);
            }
            var waitVerify = new WaitVerify { Token = args.Token, Id = args.Id, Metadata = args.Metadata };
            using (var waitData = this.WaitHandlePool.GetWaitDataAsync(waitVerify))
            {
                await this.SendJsonObjectAsync(P1_Handshake_Request, waitVerify, cancellationToken).ConfigureDefaultAwait();
                switch (await waitData.WaitAsync(cancellationToken).ConfigureDefaultAwait())
                {
                    case WaitDataStatus.Success:
                        var response = (WaitVerify)waitData.CompletedData;
                        if (response.Status != 1)
                        {
                            throw new TokenVerifyException(response.Metadata, response.Message);
                        }
                        lock (this.m_syncRoot)
                        {
                            if (this.m_finalized || Volatile.Read(ref this.m_closeState) != 0)
                            {
                                throw new OperationCanceledException(this.ClosedToken);
                            }
                            this.m_id = response.Id;
                            this.m_online = true;
                            this.m_connecting = false;
                        }
                        this.m_handshakeFinished.Set();
                        _ = EasyTask.SafeRun(this.PrivateOnConnected, new DmtpVerifyEventArgs
                        {
                            Id = response.Id, Metadata = response.Metadata, Token = response.Token
                        });
                        return;
                    case WaitDataStatus.Overtime:
                        throw new TimeoutException(TouchSocketDmtpStatus.Overtime.GetDescription());
                    default:
                        throw new OperationCanceledException(cancellationToken);
                }
            }
        }
        finally
        {
            this.m_handshakeFinished.Set();
        }
    }

    private void BeginConnection()
    {
        lock (this.m_syncRoot)
        {
            this.m_cancellationTokenSource.SafeDispose();
            this.m_cancellationTokenSource = new CancellationTokenSource();
            this.m_connecting = true;
            this.m_finalized = false;
            this.m_closedMessage = null;
            this.m_closeCompletion = null;
            this.m_closeState = 0;
            this.m_handshakeFinished.Reset();
        }
    }

    #region 委托触发

    /// <summary>
    /// 即将关闭
    /// </summary>
    protected virtual Task OnClosing(string msg)
    {
        if (this.Closing != null)
        {
            return this.Closing.Invoke(this, msg);
        }
        return EasyTask.CompletedTask;
    }

    /// <summary>
    /// 连接已关闭
    /// </summary>
    protected virtual Task OnClosed(string msg,Exception ex)
    {
        if (this.Closed != null)
        {
            return this.Closed.Invoke(this, new ClosedEventArgs(msg,ex));
        }
        return EasyTask.CompletedTask;
    }

    private async Task PrivateOnClosed(string msg, Exception ex)
    {
        bool notify;
        IActor[] actors;
        InternalChannel[] channels;
        lock (this.m_syncRoot)
        {
            if (this.m_finalized)
            {
                return;
            }
            this.m_finalized = true;
            notify = this.m_online || this.m_connecting;
            this.m_online = false;
            this.m_connecting = false;
            Interlocked.Exchange(ref this.m_closeState, 1);
            actors = this.m_actors.Values.ToArray();
            this.m_actors.Clear();
            channels = this.m_userChannels.Values.ToArray();
            this.m_userChannels.Clear();
        }
        this.m_handshakeFinished.Set();
        this.WaitHandlePool.CancelAll();
        this.m_cancellationTokenSource.SafeCancel();
        foreach (var channel in channels)
        {
            channel.SafeDispose();
        }
        foreach (var actor in actors)
        {
            if (actor is IReconnectableActor reconnectableActor)
            {
                try
                {
                    reconnectableActor.OnDisconnected();
                }
                catch (Exception exception)
                {
                    this.Logger?.Debug(this, exception);
                }
            }
            else
            {
                actor.SafeDispose();
            }
        }
        if (notify)
        {
            var previous = s_closingActor.Value;
            s_closingActor.Value = this;
            try
            {
                await this.OnClosed(msg, ex).ConfigureDefaultAwait();
            }
            finally
            {
                s_closingActor.Value = previous;
            }
        }
    }

    /// <summary>
    /// 关闭承载当前协议的传输连接。由宿主在传输结束后调用 FinalizeAsync。
    /// </summary>
    /// <param name="message">关闭原因。</param>
    /// <param name="manual">是否由本端主动关闭。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    protected virtual async Task<Result> OnTransportClosing(string message, bool manual, CancellationToken cancellationToken)
    {
        await this.FinalizeAsync(message).ConfigureDefaultAwait();
        return Result.Success;
    }



    /// <summary>
    /// 握手连接完成
    /// </summary>
    protected virtual Task OnConnected(DmtpVerifyEventArgs e)
    {
        if (this.Connected != null)
        {
            return this.Connected.Invoke(this, e);
        }
        return EasyTask.CompletedTask;
    }

    /// <summary>
    /// 正在握手连接
    /// </summary>
    protected virtual Task OnConnecting(DmtpVerifyEventArgs e)
    {
        if (this.Connecting != null)
        {
            return this.Connecting.Invoke(this, e);
        }
        return EasyTask.CompletedTask;
    }

    /// <summary>
    /// 当完成创建通道时
    /// </summary>
    protected virtual Task OnCreatedChannel(CreateChannelEventArgs e)
    {
        if (this.CreatedChannel != null)
        {
            return this.CreatedChannel.Invoke(this, e);
        }
        return EasyTask.CompletedTask;
    }

    /// <summary>
    /// 当Id修改时
    /// </summary>
    protected virtual Task OnIdChanged(IdChangedEventArgs e)
    {
        if (this.IdChanged != null)
        {
            return this.IdChanged.Invoke(this, e);
        }
        return EasyTask.CompletedTask;
    }

    private Task PrivateOnConnected(DmtpVerifyEventArgs e)
    {
        return this.OnConnected(e);
    }

    private async Task PrivateOnCreatedChannel(object obj)
    {
        try
        {
            await this.OnCreatedChannel((CreateChannelEventArgs)obj);
        }
        catch
        {
        }
    }

    #endregion 委托触发

    #region const

    /// <summary>
    /// Close
    /// </summary>
    public const ushort P0_Close = 0;

    /// <summary>
    /// Handshake_Request
    /// </summary>
    public const ushort P1_Handshake_Request = 1;

    /// <summary>
    /// Handshake_Response
    /// </summary>
    public const ushort P2_Handshake_Response = 2;

    /// <summary>
    /// ResetId_Request
    /// </summary>
    public const ushort P3_ResetId_Request = 3;

    /// <summary>
    /// ResetId_Response
    /// </summary>
    public const ushort P4_ResetId_Response = 4;

    /// <summary>
    /// Ping_Request
    /// </summary>
    public const ushort P5_Ping_Request = 5;

    /// <summary>
    /// Ping_Response
    /// </summary>
    public const ushort P6_Ping_Response = 6;

    /// <summary>
    /// CreateChannel_Request
    /// </summary>
    public const ushort P7_CreateChannel_Request = 7;

    /// <summary>
    /// CreateChannel_Response
    /// </summary>
    public const ushort P8_CreateChannel_Response = 8;

    /// <summary>
    /// ChannelPackage
    /// </summary>
    public const ushort P9_ChannelPackage = 9;

    /// <summary>
    /// CloseAcknowledge
    /// </summary>
    public const ushort P10_CloseAcknowledge = 10;

    #endregion const

    /// <summary>
    /// 处理接收数据.
    /// <para>
    /// <list type="table">
    /// <item>0：Close</item>
    /// <item>1：Handshake_Request</item>
    /// <item>2：Handshake_Response</item>
    /// <item>3：ResetId_Request</item>
    /// <item>4：ResetId_Response</item>
    /// <item>5：Ping_Request</item>
    /// <item>6：Ping_Response</item>
    /// <item>7：CreateChannel_Request</item>
    /// <item>8：CreateChannel_Response</item>
    /// <item>9：ChannelPackage</item>
    /// <item>10：CloseAcknowledge</item>
    /// </list>
    /// </para>
    /// </summary>
    public virtual async Task<bool> InputReceivedData(DmtpMessage message)
    {
        this.ThrowIfDisposed();
        if (this.IsReliable && (!this.Online || Volatile.Read(ref this.m_closeState) != 0)
            && message.ProtocolFlags is not P0_Close and not P1_Handshake_Request and not P2_Handshake_Response and not P10_CloseAcknowledge)
        {
            throw new InvalidOperationException("Dmtp握手尚未完成或连接正在关闭。");
        }
        this.LastActiveTime = DateTimeOffset.UtcNow;
        using var readerOwner = new BytesReader(message.Memory);
        var reader = readerOwner;
        switch (message.ProtocolFlags)
        {
            case P0_Close:
                {
                    var waitClose = ResolveJsonObject<WaitClose>(message.GetBodyString());
                    this.m_closedMessage = waitClose.Message;
                    Interlocked.Exchange(ref this.m_closeState, 1);
                    this.CancelPendingOperations();
                    try
                    {
                        await this.SendJsonObjectAsync(P10_CloseAcknowledge, waitClose).ConfigureDefaultAwait();
                    }
                    catch (Exception ex)
                    {
                        this.Logger?.Debug(this, ex);
                    }
                    finally
                    {
                        await this.OnTransportClosing(waitClose.Message, false, CancellationToken.None).ConfigureDefaultAwait();
                    }
                    return true;
                }

            case P1_Handshake_Request:
                {
                    if (this.Online || this.m_connecting || this.m_finalized)
                    {
                        throw new InvalidOperationException("当前连接不允许重复握手。");
                    }
                    this.BeginConnection();
                    try
                    {
                        var waitVerify = ResolveJsonObject<WaitVerify>(message.GetBodyString());

                        var args = new DmtpVerifyEventArgs()
                        {
                            Token = waitVerify.Token,
                            Metadata = waitVerify.Metadata,
                            Id = waitVerify.Id,
                        };
                        await this.OnConnecting(args).ConfigureDefaultAwait();

                        if (args.IsPermitOperation)
                        {
                            if (args.Id.HasValue())
                            {
                                await this.OnIdChanged(new IdChangedEventArgs(this.m_id, args.Id)).ConfigureDefaultAwait();
                                this.m_id = args.Id;
                            }
                            waitVerify.Id = this.m_id;
                            waitVerify.Status = 1;
                            waitVerify.Metadata = args.Metadata;
                            waitVerify.Message = args.Message ?? TouchSocketCoreResource.OperationSuccessful;
                            await this.SendJsonObjectAsync(P2_Handshake_Response, waitVerify).ConfigureDefaultAwait();
                            this.m_online = true;
                            Volatile.Write(ref this.m_closeState, 0);
                            args.Message ??= TouchSocketCoreResource.OperationSuccessful;
                            this.m_connecting = false;
                            _ = EasyTask.SafeRun(this.PrivateOnConnected, args);
                        }
                        else//不允许连接
                        {
                            waitVerify.Status = 2;
                            waitVerify.Metadata = args.Metadata;
                            waitVerify.Message = TouchSocketDmtpResource.RemoteRefuse.Format(args.Message);
                            await this.SendJsonObjectAsync(P2_Handshake_Response, waitVerify).ConfigureDefaultAwait();
                            await this.OnTransportClosing(args.Message, false, CancellationToken.None).ConfigureDefaultAwait();
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Logger?.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                        await this.OnTransportClosing(ex.Message, false, CancellationToken.None).ConfigureDefaultAwait();
                    }
                    return true;
                }
            case P2_Handshake_Response:
                {
                    try
                    {
                        var waitVerify = ResolveJsonObject<WaitVerify>(message.GetBodyString());
                        if (this.WaitHandlePool.Set(waitVerify))
                        {
                            await this.m_handshakeFinished.WaitOneAsync(this.ClosedToken).ConfigureDefaultAwait();
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Logger?.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                    }

                    return true;
                }
            case P3_ResetId_Request:
                {
                    try
                    {
                        var waitSetId = ResolveJsonObject<WaitSetId>(message.GetBodyString());
                        try
                        {
                            await this.OnIdChanged(new IdChangedEventArgs(waitSetId.OldId, waitSetId.NewId)).ConfigureDefaultAwait();
                            this.m_id = waitSetId.NewId;
                            waitSetId.Status = 1;
                        }
                        catch (Exception ex)
                        {
                            waitSetId.Status = 2;
                            waitSetId.Message = ex.Message;
                        }
                        await this.SendJsonObjectAsync(P4_ResetId_Response, waitSetId).ConfigureDefaultAwait();
                    }
                    catch (Exception ex)
                    {
                        this.Logger?.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                    }
                    return true;
                }
            case P4_ResetId_Response:
                {
                    try
                    {
                        this.WaitHandlePool.Set(ResolveJsonObject<WaitSetId>(message.GetBodyString()));
                    }
                    catch (Exception ex)
                    {
                        this.Logger?.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                    }
                    return true;
                }
            case P5_Ping_Request://心跳
                {
                    try
                    {
                        var waitPing = ResolveJsonObject<WaitPing>(message.GetBodyString());

                        if (this.AllowRoute && waitPing.Route)
                        {
                            if (await this.TryRouteAsync(new PackageRouterEventArgs(RouteType.Ping, waitPing)).ConfigureDefaultAwait())
                            {
                                if (await this.TryFindDmtpActor(waitPing.TargetId).ConfigureDefaultAwait() is DmtpActor actor)
                                {
                                    await actor.SendAsync(P5_Ping_Request, message.Memory).ConfigureDefaultAwait();
                                    return true;
                                }
                                else
                                {
                                    waitPing.Status = TouchSocketDmtpStatus.ClientNotFind.ToValue();
                                }
                            }
                            else
                            {
                                waitPing.Status = TouchSocketDmtpStatus.RoutingNotAllowed.ToValue();
                            }
                        }
                        else
                        {
                            waitPing.Status = TouchSocketDmtpStatus.Success.ToValue();
                        }
                        waitPing.SwitchId();
                        await this.SendJsonObjectAsync(P6_Ping_Response, waitPing).ConfigureDefaultAwait();
                    }
                    catch (Exception ex)
                    {
                        this.Logger?.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                    }
                    return true;
                }
            case P6_Ping_Response://心跳
                {
                    try
                    {
                        var waitPing = ResolveJsonObject<WaitPing>(message.GetBodyString());

                        if (this.AllowRoute && waitPing.Route)
                        {
                            if (await this.TryFindDmtpActor(waitPing.TargetId).ConfigureDefaultAwait() is DmtpActor actor)
                            {
                                await actor.SendAsync(P6_Ping_Response, message.Memory).ConfigureDefaultAwait();
                            }
                        }
                        else
                        {
                            this.WaitHandlePool.Set(waitPing);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Logger?.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                    }
                    return true;
                }
            case P7_CreateChannel_Request:
                {
                    try
                    {
                        var waitCreateChannel = new WaitCreateChannelPackage();

                        waitCreateChannel.UnpackageRouter(ref reader);
                        if (this.AllowRoute && waitCreateChannel.Route)
                        {
                            if (await this.TryRouteAsync(new PackageRouterEventArgs(RouteType.CreateChannel, waitCreateChannel)).ConfigureDefaultAwait())
                            {
                                if (await this.TryFindDmtpActor(waitCreateChannel.TargetId).ConfigureDefaultAwait() is DmtpActor actor)
                                {
                                    await actor.SendAsync(P7_CreateChannel_Request, message.Memory).ConfigureDefaultAwait();
                                    return true;
                                }
                                else
                                {
                                    waitCreateChannel.UnpackageBody(ref reader);
                                    waitCreateChannel.Status = TouchSocketDmtpStatus.ClientNotFind.ToValue();
                                }
                            }
                            else
                            {
                                waitCreateChannel.UnpackageBody(ref reader);
                                waitCreateChannel.Status = TouchSocketDmtpStatus.RoutingNotAllowed.ToValue();
                            }
                        }
                        else
                        {
                            waitCreateChannel.UnpackageBody(ref reader);

                            while (true)
                            {
                                if (this.RequestCreateChannel(waitCreateChannel.ChannelId, waitCreateChannel.Route ? waitCreateChannel.SourceId : waitCreateChannel.TargetId, waitCreateChannel.Metadata))
                                {
                                    waitCreateChannel.Status = TouchSocketDmtpStatus.Success.ToValue();
                                    break;
                                }
                                else
                                {
                                    waitCreateChannel.Status = TouchSocketDmtpStatus.ChannelExisted.ToValue();
                                }

                                if (!waitCreateChannel.Random)
                                {
                                    break;
                                }
                                waitCreateChannel.ChannelId = new object().GetHashCode();
                            }
                        }
                        waitCreateChannel.SwitchId();

                        await this.SendAsync(P8_CreateChannel_Response, waitCreateChannel).ConfigureDefaultAwait();
                    }
                    catch (Exception ex)
                    {
                        this.Logger?.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                    }
                    return true;
                }
            case P8_CreateChannel_Response:
                {
                    try
                    {
                        var waitCreateChannel = new WaitCreateChannelPackage();

                        waitCreateChannel.UnpackageRouter(ref reader);
                        if (this.AllowRoute && waitCreateChannel.Route)
                        {
                            if (await this.TryFindDmtpActor(waitCreateChannel.TargetId).ConfigureDefaultAwait() is DmtpActor actor)
                            {
                                await actor.SendAsync(P8_CreateChannel_Response, message.Memory).ConfigureDefaultAwait();
                                return true;
                            }
                        }
                        else
                        {
                            waitCreateChannel.UnpackageBody(ref reader);
                            this.WaitHandlePool.Set(waitCreateChannel);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Logger?.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                    }
                    return true;
                }
            case P9_ChannelPackage:
                {
                    ChannelPackage channelPackage = new ChannelPackage();
                    try
                    {

                        channelPackage.UnpackageRouter(ref reader);
                        if (this.AllowRoute && channelPackage.Route)
                        {
                            if (await this.TryFindDmtpActor(channelPackage.TargetId).ConfigureDefaultAwait() is DmtpActor actor)
                            {
                                await actor.SendAsync(P9_ChannelPackage, message.Memory).ConfigureDefaultAwait();
                            }
                            else
                            {
                                channelPackage.UnpackageBody(ref reader);
                                channelPackage.Message = TouchSocketDmtpStatus.ClientNotFind.GetDescription(channelPackage.TargetId);
                                channelPackage.SwitchId();
                                channelPackage.DataType = ChannelDataType.Canceled;
                                await this.SendAsync(P9_ChannelPackage, channelPackage).ConfigureDefaultAwait();
                            }
                        }
                        else
                        {
                            channelPackage.UnpackageBody(ref reader);
                            await this.QueueChannelPackageAsync(channelPackage).ConfigureDefaultAwait();
                            channelPackage = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Logger?.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                    }
                    finally
                    {
                        channelPackage.SafeDispose();
                    }
                    return true;
                }
            case P10_CloseAcknowledge:
                {
                    try
                    {
                        var waitClose = ResolveJsonObject<WaitClose>(message.GetBodyString());
                        this.WaitHandlePool.Set(waitClose);
                    }
                    catch (Exception ex)
                    {
                        this.Logger?.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                    }
                    return true;
                }
            default:
                {
                    if (message.ProtocolFlags < 20)
                    {
                        return true;
                    }
                    return false;
                }
        }
    }

    /// <inheritdoc/>
    public virtual Task<Result> PingAsync(CancellationToken cancellationToken = default)
    {
        return this.PrivatePingAsync(default, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<Result> PingAsync(string targetId, CancellationToken cancellationToken = default)
    {
        if (this.AllowRoute && await this.TryFindDmtpActor(targetId).ConfigureDefaultAwait() is DmtpActor actor)
        {
            return await actor.PingAsync(cancellationToken).ConfigureDefaultAwait();
        }
        else
        {
            return await this.PrivatePingAsync(targetId, cancellationToken).ConfigureDefaultAwait();
        }
    }

    /// <inheritdoc/>
    public virtual async Task ResetIdAsync(string newId, CancellationToken cancellationToken = default)
    {
        this.EnsureCanSend(P3_ResetId_Request);
        ThrowHelper.ThrowArgumentNullExceptionIfStringIsNullOrEmpty(newId, nameof(newId));
        var waitSetId = new WaitSetId(this.m_id, newId);

        using var waitData = this.WaitHandlePool.GetWaitDataAsync(waitSetId);

        await this.SendJsonObjectAsync(P3_ResetId_Request, waitSetId, cancellationToken).ConfigureDefaultAwait();

        switch (await waitData.WaitAsync(cancellationToken).ConfigureDefaultAwait())
        {
            case WaitDataStatus.Success:
                {
                    if (waitData.CompletedData.Status == 1)
                    {
                        await this.OnIdChanged(new IdChangedEventArgs(this.m_id, newId)).ConfigureDefaultAwait();
                        this.m_id = newId;
                    }
                    else
                    {
                        throw new Exception(waitData.CompletedData.Message);
                    }
                    break;
                }
            case WaitDataStatus.Overtime:
                throw new TimeoutException(TouchSocketDmtpStatus.Overtime.GetDescription());
            case WaitDataStatus.Canceled:
                throw new OperationCanceledException(cancellationToken);

            case WaitDataStatus.Disposed:
            default:
                throw new Exception(TouchSocketDmtpStatus.UnknownError.GetDescription());
        }
    }

    /// <inheritdoc/>
    public virtual async Task<DmtpActor> TryFindDmtpActor(string targetId)
    {
        if (targetId == this.m_id)
        {
            return this;
        }
        if (this.FindDmtpActor != null)
        {
            if (await this.FindDmtpActor.Invoke(targetId).ConfigureDefaultAwait() is DmtpActor newActor)
            {
                return newActor;
            }
        }

        return default;
    }

    /// <inheritdoc/>
    public virtual async Task<bool> TryRouteAsync(PackageRouterEventArgs e)
    {
        try
        {
            if (this.Routing != null)
            {
                await this.Routing.Invoke(this, e).ConfigureDefaultAwait();
                return e.IsPermitOperation;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    private static T ResolveJsonObject<T>(string json)
    {
        return (T)System.Text.Json.JsonSerializer.Deserialize(json, typeof(T), TouchSocketDmtpSourceGenerationContext.Default);
    }

    private async Task<Result> PrivatePingAsync(string targetId, CancellationToken cancellationToken = default)
    {
        if (!this.Online)
        {
            return Result.FromFail(TouchSocketResource.ClientNotConnected);
        }
        var waitPing = new WaitPing
        {
            TargetId = targetId,
            SourceId = this.m_id
        };
        using (var waitData = this.WaitHandlePool.GetWaitDataAsync(waitPing))
        {
            await this.SendJsonObjectAsync(P5_Ping_Request, waitPing, cancellationToken).ConfigureDefaultAwait();
            switch (await waitData.WaitAsync(cancellationToken).ConfigureDefaultAwait())
            {
                case WaitDataStatus.Success:
                    {
                        return waitData.CompletedData.Status.ToStatus() switch
                        {
                            TouchSocketDmtpStatus.Success => Result.Success,
                            _ => Result.UnknownFail,
                        };
                    }
                case WaitDataStatus.Default:
                case WaitDataStatus.Overtime:
                case WaitDataStatus.Canceled:
                case WaitDataStatus.Disposed:
                default:
                    return Result.UnknownFail;
            }
        }
    }

    #region Actor

    /// <inheritdoc/>
    public void AddActor<TActor>(TActor actor) where TActor : class, IActor
    {
        ThrowHelper.ThrowIfNull(actor, nameof(actor));
        var type = typeof(TActor);

        lock (this.m_syncRoot)
        {
            this.m_actors.AddOrUpdate(type, actor);
        }
    }

    /// <inheritdoc/>
    public TActor GetActor<TActor>() where TActor : class, IActor
    {
        var type = typeof(TActor);
        lock (this.m_syncRoot)
        {
            return this.m_actors.TryGetValue(type, out var actor) ? (TActor)actor : default;
        }
    }

    /// <inheritdoc/>
    public bool TryAddActor<TActor>(TActor actor) where TActor : class, IActor
    {
        ThrowHelper.ThrowIfNull(actor, nameof(actor));
        var type = typeof(TActor);

        lock (this.m_syncRoot)
        {
            if (this.m_actors.ContainsKey(type))
            {
                return false;
            }
            this.m_actors.Add(type, actor);
            return true;
        }
    }

    #endregion Actor

    #region 断开

    /// <inheritdoc/>
    public Task<Result> CloseAsync(string msg, CancellationToken cancellationToken = default)
    {
        if (ReferenceEquals(s_closingActor.Value, this))
        {
            return Task.FromResult(Result.Success);
        }
        TaskCompletionSource<Result> completion;
        lock (this.m_syncRoot)
        {
            if (this.m_closeCompletion != null)
            {
                return this.m_closeCompletion.Task;
            }
            completion = new TaskCompletionSource<Result>(TaskCreationOptions.RunContinuationsAsynchronously);
            this.m_closeCompletion = completion;
        }
        _ = this.CloseCoreAsync(msg, cancellationToken, completion);
        return completion.Task;
    }

    private void CancelPendingOperations()
    {
        this.m_cancellationTokenSource.SafeCancel();
        this.WaitHandlePool.CancelAll();
        foreach (var channel in this.m_userChannels.Values)
        {
            channel.SafeDispose();
        }
    }

    private async Task CloseCoreAsync(string msg, CancellationToken cancellationToken, TaskCompletionSource<Result> completion)
    {
        var previous = s_closingActor.Value;
        s_closingActor.Value = this;
        Exception exception = null;
        try
        {
            if (Interlocked.CompareExchange(ref this.m_closeState, 1, 0) == 0 && this.Online && (this.Client is not IOnlineClient client || client.Online))
            {
                try
                {
                    await this.OnClosing(msg).ConfigureDefaultAwait();
                    // 传输层负责排空发送管道；不在接收回调中等待关闭确认，避免等待自身。
                    await this.SendJsonObjectAsync(P0_Close, new WaitClose { Message = msg }, cancellationToken).ConfigureDefaultAwait();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            }
            this.CancelPendingOperations();
            // 即使调用方已取消，也必须完成传输连接与等待项的收尾。
            var result = await this.OnTransportClosing(msg, true, CancellationToken.None).ConfigureDefaultAwait();
            completion.TrySetResult(exception == null ? result : Result.FromException(exception));
        }
        catch (Exception ex)
        {
            completion.TrySetResult(Result.FromException(ex));
        }
        finally
        {
            s_closingActor.Value = previous;
        }
    }

    /// <summary>
    /// 传输连接结束后，取消所有等待、释放通道与功能件，并触发一次关闭通知。
    /// </summary>
    /// <param name="msg">关闭原因。</param>
    /// <param name="exception">关闭时的异常。</param>
    public Task FinalizeAsync(string msg, Exception exception = null)
    {
        return this.PrivateOnClosed(msg, exception);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.WaitHandlePool.CancelAll();
            this.m_cancellationTokenSource.SafeCancel();
            this.m_cancellationTokenSource.SafeDispose();
            this.m_handshakeFinished.Set();
            lock (this.m_syncRoot)
            {
                this.m_online = false;
                foreach (var item in this.m_userChannels.Values)
                {
                    item.SafeDispose();
                }
                this.m_userChannels.Clear();
                foreach (var item in this.m_actors.Values)
                {
                    item.SafeDispose();
                }
                this.m_actors.Clear();
            }
        }
        base.Dispose(disposing);
    }

    #endregion 断开

    #region 协议异步发送

    private readonly SemaphoreSlim m_semaphoreSlim = new SemaphoreSlim(1, 1);

    private void EnsureCanSend(ushort protocol)
    {
        this.ThrowIfDisposed();
        // 只有握手及关闭报文可以在未上线时发送。
        if ((!this.Online || Volatile.Read(ref this.m_closeState) != 0) && protocol is not P0_Close and not P1_Handshake_Request and not P2_Handshake_Response and not P10_CloseAcknowledge)
        {
            ThrowHelper.ThrowClientNotConnectedException();
        }
    }

    private static void BuildPackage<TWriter>(ref TWriter writer, ushort protocol, ReadOnlyMemory<byte> memory)
        where TWriter : IBytesWriter
    {
        writer.Write(DmtpMessage.Head);
        WriterExtension.WriteValue<TWriter, ushort>(ref writer, protocol, EndianType.Big);
        WriterExtension.WriteValue<TWriter, int>(ref writer, memory.Length, EndianType.Big);
        writer.Write(memory.Span);
    }

    private static void BuildPackage<TWriter,TPackage>(ref TWriter writer, ushort protocol, TPackage package)
        where TWriter : IBytesWriter
        where TPackage: IPackage
    {
        writer.Write(DmtpMessage.Head);
        WriterExtension.WriteValue<TWriter, ushort>(ref writer, protocol, EndianType.Big);

        var writerAnchor=new WriterAnchor<TWriter>(ref writer,4);

        package.Package(ref writer);

        var span= writerAnchor.Rewind(ref writer, out var length);
        span.WriteValue(length,EndianType.Big);
    }

    private void CheckMaxPackageSize(int bodyLength)
    {
        if (bodyLength < 0 || bodyLength > int.MaxValue - 8)
        {
            throw new ArgumentOutOfRangeException(nameof(bodyLength));
        }
        var maxPackageSize = this.MaxPackageSize;
        if (maxPackageSize <= 0)
        {
            return;
        }

        var packageLength = bodyLength + 8L;
        if (packageLength > maxPackageSize)
        {
            ThrowHelper.ThrowArgumentOutOfRangeException_MoreThan("memory.Length", packageLength, maxPackageSize);
        }
    }

    /// <inheritdoc/>
    public virtual async Task SendAsync(ushort protocol, ReadOnlyMemory<byte> memory, CancellationToken cancellationToken = default)
    {
        this.EnsureCanSend(protocol);
        this.CheckMaxPackageSize(memory.Length);

        var transportWriter = this.TransportWriter;
        if (transportWriter != null && this.OutputSendAsync == null)
        {
            await transportWriter.WriteLocker.WaitAsync(cancellationToken).ConfigureDefaultAwait();
            try
            {
                var pipeWriter = new PipeBytesWriter(transportWriter.Writer);
                BuildPackage(ref pipeWriter, protocol, memory);
                await pipeWriter.FlushAsync(cancellationToken).ConfigureDefaultAwait();
            }
            finally
            {
                transportWriter.WriteLocker.Release();
            }

            return;
        }

        await this.m_semaphoreSlim.WaitAsync(cancellationToken).ConfigureDefaultAwait();
        try
        {
            var segmentedWriter = new SegmentedBytesWriter(memory.Length + 8);
            try
            {
                BuildPackage(ref segmentedWriter, protocol, memory);
                // 委托可能对应 WebSocket 或 UDP，每次调用必须包含完整的 DMTP 报文。
                using var buffer = new ContiguousMemoryBuffer(segmentedWriter.Sequence);
                this.CheckMaxPackageSize(buffer.Memory.Length - 8);
                if (this.OutputSendAsync != null)
                {
                    await this.OutputSendAsync.Invoke(this, buffer.Memory, cancellationToken).ConfigureDefaultAwait();
                }
                else
                {
                    var transport = this.TransportWriter;
                    ThrowHelper.ThrowIfNull(transport, nameof(this.TransportWriter));
                    await transport.WriteLocker.WaitAsync(cancellationToken).ConfigureDefaultAwait();
                    try
                    {
                        var writer = new PipeBytesWriter(transport.Writer);
                        writer.Write(buffer.Memory.Span);
                        await writer.FlushAsync(cancellationToken).ConfigureDefaultAwait();
                    }
                    finally
                    {
                        transport.WriteLocker.Release();
                    }
                }
            }
            finally
            {
                segmentedWriter.Dispose();
            }
        }
        finally
        {
            this.m_semaphoreSlim.Release();
        }
    }

  
    /// <inheritdoc/>
    public async Task SendAsync<TPackage>(ushort protocol, TPackage package, CancellationToken cancellationToken = default)
        where TPackage : IPackage
    {
        this.EnsureCanSend(protocol);
        await this.m_semaphoreSlim.WaitAsync(cancellationToken).ConfigureDefaultAwait();
        try
        {
            var segmentedWriter = new SegmentedBytesWriter(1024);
            try
            {
                BuildPackage(ref segmentedWriter, protocol, package);
                // 委托可能对应 WebSocket 或 UDP，每次调用必须包含完整的 DMTP 报文。
                using var buffer = new ContiguousMemoryBuffer(segmentedWriter.Sequence);
                this.CheckMaxPackageSize(buffer.Memory.Length - 8);
                if (this.OutputSendAsync != null)
                {
                    await this.OutputSendAsync.Invoke(this, buffer.Memory, cancellationToken).ConfigureDefaultAwait();
                }
                else
                {
                    var transport = this.TransportWriter;
                    ThrowHelper.ThrowIfNull(transport, nameof(this.TransportWriter));
                    await transport.WriteLocker.WaitAsync(cancellationToken).ConfigureDefaultAwait();
                    try
                    {
                        var writer = new PipeBytesWriter(transport.Writer);
                        writer.Write(buffer.Memory.Span);
                        await writer.FlushAsync(cancellationToken).ConfigureDefaultAwait();
                    }
                    finally
                    {
                        transport.WriteLocker.Release();
                    }
                }
            }
            finally
            {
                segmentedWriter.Dispose();
            }
        }
        finally
        {
            this.m_semaphoreSlim.Release();
        }
    }

    
    /// <inheritdoc/>
    public async Task SendAsync(ushort protocol, string value, CancellationToken cancellationToken = default)
    {
        this.EnsureCanSend(protocol);
        var byteBlock = new ByteBlock(1024 * 64);
        try
        {
            WriterExtension.WriteNormalString(ref byteBlock, value, Encoding.UTF8);
            await this.SendAsync(protocol, byteBlock.Memory, cancellationToken).ConfigureDefaultAwait();
        }
        finally
        {
            byteBlock.Dispose();
        }
    }

    private async Task SendJsonObjectAsync<T>(ushort protocol, T obj, CancellationToken cancellationToken = default)
    {
        using var writer = new SegmentedBytesWriter(256);
        using (var jsonWriter = new System.Text.Json.Utf8JsonWriter(writer))
        {
            System.Text.Json.JsonSerializer.Serialize(jsonWriter, obj, typeof(T), TouchSocketDmtpSourceGenerationContext.Default);
            jsonWriter.Flush();
        }
        using var buffer = new ContiguousMemoryBuffer(writer.Sequence);
        await this.SendAsync(protocol, buffer.Memory, cancellationToken).ConfigureDefaultAwait();
    }

    #endregion 协议异步发送

    #region IDmtpChannel

    /// <inheritdoc/>
    public virtual bool ChannelExisted(int id)
    {
        this.CheckChannelShouldBeReliable();
        return this.m_userChannels.ContainsKey(id);
    }

    /// <inheritdoc/>
    public virtual Task<IDmtpChannel> CreateChannelAsync(Metadata metadata = default, CancellationToken cancellationToken = default)
    {
        return this.PrivateCreateChannelAsync(default, true, 0, metadata, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual Task<IDmtpChannel> CreateChannelAsync(int id, Metadata metadata = default, CancellationToken cancellationToken = default)
    {
        return this.PrivateCreateChannelAsync(default, false, id, metadata, cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task<IDmtpChannel> CreateChannelAsync(string targetId, int id, Metadata metadata = default, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(targetId))
        {
            throw new ArgumentException($"“{nameof(targetId)}”不能为 null 或空。", nameof(targetId));
        }
        if (this.AllowRoute && await this.TryFindDmtpActor(targetId).ConfigureDefaultAwait() is DmtpActor actor)
        {
            return await actor.CreateChannelAsync(id, metadata, cancellationToken).ConfigureDefaultAwait();
        }
        else
        {
            return await this.PrivateCreateChannelAsync(targetId, false, id, metadata, cancellationToken).ConfigureDefaultAwait();
        }
    }

    /// <inheritdoc/>
    public virtual async Task<IDmtpChannel> CreateChannelAsync(string targetId, Metadata metadata = default, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(targetId))
        {
            throw new ArgumentException($"“{nameof(targetId)}”不能为 null 或空。", nameof(targetId));
        }

        if (this.AllowRoute && await this.TryFindDmtpActor(targetId).ConfigureDefaultAwait() is DmtpActor actor)
        {
            return await actor.CreateChannelAsync(metadata, cancellationToken).ConfigureDefaultAwait();
        }
        else
        {
            return await this.PrivateCreateChannelAsync(targetId, true, 0, metadata, cancellationToken).ConfigureDefaultAwait();
        }
    }

    /// <inheritdoc/>
    public virtual bool TrySubscribeChannel(int id, out IDmtpChannel channel)
    {
        if (this.m_userChannels.TryGetValue(id, out var channelOut))
        {
            if (!channelOut.TryMakeUsing())
            {
                channel = null;
                return false;
            }
            channel = channelOut;
            return true;
        }
        channel = null;
        return false;
    }

    internal bool RemoveChannel(int id, InternalChannel channel)
    {
        return ((ICollection<KeyValuePair<int, InternalChannel>>)this.m_userChannels)
            .Remove(new KeyValuePair<int, InternalChannel>(id, channel));
    }

    internal async Task SendChannelPackageAsync(ChannelPackage channelPackage, CancellationToken cancellationToken)
    {
        // channel package uses protocol P9_ChannelPackage
        this.EnsureCanSend(P9_ChannelPackage);
        using (var byteBlock = new ByteBlock(channelPackage.GetLen()))
        {
            var block = byteBlock;
            channelPackage.Package(ref block);
            await this.SendAsync(P9_ChannelPackage, byteBlock.Memory, cancellationToken).ConfigureDefaultAwait();
        }
    }

    private void CheckChannelShouldBeReliable()
    {
        if (!this.IsReliable)
        {
            throw new NotSupportedException("Channel不支持在不可靠协议使用。");
        }
    }

    private async Task<IDmtpChannel> PrivateCreateChannelAsync(string targetId, bool random, int id, Metadata metadata, CancellationToken cancellationToken)
    {
        this.CheckChannelShouldBeReliable();
        this.EnsureCanSend(P7_CreateChannel_Request);

        if (random)
        {
            id = new object().GetHashCode();
        }
        else
        {
            if (this.ChannelExisted(id))
            {
                throw new Exception(TouchSocketDmtpStatus.ChannelExisted.GetDescription(id));
            }
        }
        var byteBlock = new ByteBlock(1024 * 64);
        try
        {
            var waitCreateChannel = new WaitCreateChannelPackage()
            {
                Random = random,
                ChannelId = id,
                SourceId = this.m_id,
                TargetId = targetId,
                Metadata = metadata
            };

            using (var waitData = this.WaitHandlePool.GetWaitDataAsync(waitCreateChannel))
            {
                waitCreateChannel.Package(ref byteBlock);

                await this.SendAsync(P7_CreateChannel_Request, byteBlock.Memory, cancellationToken).ConfigureDefaultAwait();
                switch (await waitData.WaitAsync(cancellationToken).ConfigureDefaultAwait())
                {
                    case WaitDataStatus.Success:
                        {
                            var result = (WaitCreateChannelPackage)waitData.CompletedData;
                            switch (result.Status.ToStatus())
                            {
                                case TouchSocketDmtpStatus.Success:
                                    {
                                        var channel = new InternalChannel(this, targetId, result.Metadata);
                                        channel.SetId(result.ChannelId);
                                        channel.MakeUsing();
                                        if (this.m_userChannels.TryAdd(result.ChannelId, channel))
                                        {
                                            return channel;
                                        }
                                        else
                                        {
                                            throw new Exception(TouchSocketDmtpStatus.UnknownError.GetDescription());
                                        }
                                    }
                                case TouchSocketDmtpStatus.ClientNotFind:
                                    {
                                        throw new Exception(TouchSocketDmtpStatus.ClientNotFind.GetDescription(targetId));
                                    }
                                case TouchSocketDmtpStatus.RoutingNotAllowed:
                                default:
                                    {
                                        throw new Exception(result.Status.ToStatus().GetDescription(result.Message));
                                    }
                            }
                        }
                    case WaitDataStatus.Overtime:
                        {
                            throw new TimeoutException(TouchSocketDmtpStatus.Overtime.GetDescription());
                        }
                    case WaitDataStatus.Canceled:
                        throw new OperationCanceledException(cancellationToken);
                    default:
                        {
                            throw new Exception(TouchSocketDmtpStatus.UnknownError.GetDescription());
                        }
                }
            }
        }
        finally
        {
            byteBlock.Dispose();
        }
    }

    private ValueTask QueueChannelPackageAsync(ChannelPackage channelPackage)
    {
        if (this.m_userChannels.TryGetValue(channelPackage.ChannelId, out var channel))
        {
#if NET6_0_OR_GREATER
            return channel.ReceivedDataAsync(channelPackage);
#else
            channel.ReceivedData(channelPackage);
            return default;
#endif
        }
        channelPackage.SafeDispose();
        return default;
    }

    private bool RequestCreateChannel(int id, string targetId, Metadata metadata)
    {
        lock (this.m_syncRoot)
        {
            var channel = new InternalChannel(this, targetId, metadata);
            channel.SetId(id);
            if (this.m_userChannels.TryAdd(id, channel))
            {
                _ = EasyTask.SafeRun(this.PrivateOnCreatedChannel, new CreateChannelEventArgs(id, metadata));
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    #endregion IDmtpChannel
}
