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
    /// 即将关闭时触发（主动方与被动方均会触发）。
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
    public CancellationToken ClosedToken => this.m_cancellationTokenSource.Token;

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
    private bool m_online;
    private string m_id;
    // 0=在线，1=关闭中（Closing已触发）；用 Interlocked 操作确保 Closing 最多触发一次
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
        var verifyToken = dmtpOption.VerifyToken;
        var id = dmtpOption.Id;
        var metadata = dmtpOption.Metadata;

        if (this.m_online)
        {
            return;
        }
        var args = new DmtpVerifyEventArgs()
        {
            Token = verifyToken,
            Id = id,
            Metadata = metadata
        };

        this.m_handshakeFinished.Reset();

        await this.OnConnecting(args).ConfigureDefaultAwait();

        var waitVerify = new WaitVerify()
        {
            Token = args.Token,
            Id = args.Id,
            Metadata = args.Metadata
        };

        using (var waitData = this.WaitHandlePool.GetWaitDataAsync(waitVerify))
        {
            await this.SendJsonObjectAsync(P1_Handshake_Request, waitVerify, cancellationToken).ConfigureDefaultAwait();
            switch (await waitData.WaitAsync(cancellationToken).ConfigureDefaultAwait())
            {
                case WaitDataStatus.Success:
                    {
                        var verifyResult = (WaitVerify)waitData.CompletedData;
                        if (verifyResult.Status == 1)
                        {
                            this.m_id = verifyResult.Id;
                            this.m_online = true;
                            Volatile.Write(ref this.m_closeState, 0);
                            _ = EasyTask.SafeRun(this.PrivateOnConnected, new DmtpVerifyEventArgs()
                            {
                                Id = verifyResult.Id,
                                Metadata = verifyResult.Metadata,
                                Token = verifyResult.Token,
                            });

                            this.m_cancellationTokenSource = new CancellationTokenSource();
                            this.m_handshakeFinished.Set();
                            break;
                        }
                        else
                        {
                            this.m_handshakeFinished.Set();
                            //verifyResult.Handle = true;
                            throw new TokenVerifyException(verifyResult.Metadata, verifyResult.Message);
                        }
                    }
                case WaitDataStatus.Overtime:
                    throw new TimeoutException(TouchSocketDmtpStatus.Overtime.GetDescription());
                case WaitDataStatus.Canceled:
                case WaitDataStatus.Disposed:
                default:
                    throw new OperationCanceledException();
            }
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

    private async Task PrivateOnClosed(string msg,Exception ex)
    {
        lock (this.m_syncRoot)
        {
            if (!this.m_online)
            {
                return;
            }
            this.m_online = false;
            this.WaitHandlePool.CancelAll();
            this.m_cancellationTokenSource?.Cancel();
        }
        await this.OnClosed(msg,ex).ConfigureDefaultAwait();
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
        this.LastActiveTime = DateTimeOffset.UtcNow;
        var reader = new BytesReader(message.Memory);
        switch (message.ProtocolFlags)
        {
            case P0_Close:
                {
                    try
                    {
                        var waitClose = ResolveJsonObject<WaitClose>(message.GetBodyString());
                        this.m_closedMessage = waitClose.Message;

                        // 原子抢占：若本端尚未触发 Closing，则在此触发（被动方）
                        // 若本端已主动发起关闭（m_closeState==1），则跳过 Closing，仅回应确认
                        if (Interlocked.CompareExchange(ref this.m_closeState, 1, 0) == 0)
                        {
                            await this.OnClosing(waitClose.Message).ConfigureDefaultAwait();
                        }

                        // 回应关闭确认报文（无论哪方先发起，均需回应）
                        await this.SendJsonObjectAsync(P10_CloseAcknowledge, waitClose).ConfigureDefaultAwait();

                        // 标记为下线并触发 Closed（被动）
                        await this.PrivateOnClosed(waitClose.Message,default).ConfigureDefaultAwait();
                    }
                    catch (Exception ex)
                    {
                        this.Logger?.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                    }
                    return true;
                }
            case P1_Handshake_Request:
                {
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

                        if (args.Id.HasValue())
                        {
                            await this.OnIdChanged(new IdChangedEventArgs(this.m_id, args.Id)).ConfigureDefaultAwait();
                            this.m_id = args.Id;
                        }

                        if (args.IsPermitOperation)
                        {
                            waitVerify.Id = this.m_id;
                            waitVerify.Status = 1;
                            waitVerify.Metadata = args.Metadata;
                            waitVerify.Message = args.Message ?? TouchSocketCoreResource.OperationSuccessful;
                            await this.SendJsonObjectAsync(P2_Handshake_Response, waitVerify).ConfigureDefaultAwait();
                            this.m_online = true;
                            Volatile.Write(ref this.m_closeState, 0);
                            args.Message ??= TouchSocketCoreResource.OperationSuccessful;
                            this.m_cancellationTokenSource = new CancellationTokenSource();
                            _ = EasyTask.SafeRun(this.PrivateOnConnected, args);
                        }
                        else//不允许连接
                        {
                            waitVerify.Status = 2;
                            waitVerify.Metadata = args.Metadata;
                            waitVerify.Message = TouchSocketDmtpResource.RemoteRefuse.Format(args.Message);
                            await this.SendJsonObjectAsync(P2_Handshake_Response, waitVerify).ConfigureDefaultAwait();
                            await this.OnClosed(args.Message,default).ConfigureDefaultAwait();
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Logger?.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                        await this.OnClosed(ex.Message,ex).ConfigureDefaultAwait();
                    }
                    return true;
                }
            case P2_Handshake_Response:
                {
                    try
                    {
                        var waitVerify = ResolveJsonObject<WaitVerify>(message.GetBodyString());
                        this.WaitHandlePool.Set(waitVerify);
                        await this.m_handshakeFinished.WaitOneAsync(TimeSpan.FromSeconds(5)).ConfigureDefaultAwait();
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
                    try
                    {
                        var channelPackage = new ChannelPackage();

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
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Logger?.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
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
        var waitSetId = new WaitSetId(this.m_id, newId);

        var waitData = this.WaitHandlePool.GetWaitDataAsync(waitSetId);

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
                break;

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
        if (this.m_actors.TryGetValue(type, out var actor))
        {
            return (TActor)actor;
        }
        return default;
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
    public async Task<Result> CloseAsync(string msg, CancellationToken cancellationToken = default)
    {
        // 原子抢占关闭权：确保 Closing 最多触发一次
        // 失败方直接返回，由 PrivateOnClosed 内部锁保证 Closed 最多触发一次
        if (Interlocked.CompareExchange(ref this.m_closeState, 1, 0) != 0)
        {
            return Result.Success;
        }

        // 注意：此处不再检查 m_online，原因如下：
        // 在并发场景中，其他失去 CAS 的调用方会立即触发传输层关闭（base.CloseAsync），
        // 进而通过 FinalizeAsync → PrivateOnClosed 将 m_online 置为 false。
        // 若此处检查 m_online，CAS 赢得方会提前返回导致 OnClosing 从不触发（Closing计数=0）。
        // CAS 保证 OnClosing 恰好触发一次，PrivateOnClosed 内部锁保证 OnClosed 恰好触发一次，
        // 两层保护已足够，无需再用 m_online 做额外短路。

        Exception exception;
        try
        {
            // 1. 触发 Closing 相关方法和插件
            await this.OnClosing(msg).ConfigureDefaultAwait();

            // 2. 发送关闭请求报文，并等待对方的关闭确认
            var waitClose = new WaitClose { Message = msg };
            using (var waitData = this.WaitHandlePool.GetWaitDataAsync(waitClose))
            {
                await this.SendJsonObjectAsync(P0_Close, waitClose, cancellationToken).ConfigureDefaultAwait();

                // 等待关闭确认，超时5秒后继续处理
                using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    cts.CancelAfter(TimeSpan.FromSeconds(5));
                    await waitData.WaitAsync(cts.Token).ConfigureDefaultAwait();
                }
            }

           exception = null;
        }
        catch(Exception ex)
        {
            exception = ex;
        }
        // 3. 标记为下线并触发 Closed（主动）
        await this.PrivateOnClosed(msg, exception).ConfigureDefaultAwait();
        return Result.Success;
    }

    /// <summary>
    /// 由传输层驱动的直接关闭，不发送关闭报文，仅完成状态变更并触发 <see cref="Closed"/> 回调。
    /// 用于传输层意外断开时（如 TCP 连接中断）同步 DmtpActor 状态。
    /// </summary>
    /// <param name="msg">关闭消息。</param>
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
            lock (this.m_syncRoot)
            {
                if (!this.m_online)
                {
                    return;
                }
                this.m_online = false;
                foreach (var item in this.m_actors)
                {
                    item.Value.SafeDispose();
                }

                this.m_handshakeFinished.Set();

                this.m_actors.Clear();
                this.WaitHandlePool.CancelAll();
            }
        }

        base.Dispose(disposing);
    }

    #endregion 断开

    #region 协议异步发送

    private readonly SemaphoreSlim m_semaphoreSlim = new SemaphoreSlim(1, 1);

    private void EnsureCanSend(ushort protocol)
    {
        // 允许在未连接状态下发送控制协议（protocol < 20），如握手、关闭、心跳、通道管理等。
        if (!this.Online && protocol >= 20)
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
        if (transportWriter != null)
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

        ThrowHelper.ThrowIfNull(this.OutputSendAsync, nameof(this.OutputSendAsync));
        await this.m_semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            var segmentedWriter = new SegmentedBytesWriter(memory.Length + 8);
            try
            {
                BuildPackage(ref segmentedWriter, protocol, memory);
                foreach (var item in segmentedWriter.Sequence)
                {
                    await this.OutputSendAsync.Invoke(this, item, cancellationToken).ConfigureDefaultAwait();
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
        var transportWriter = this.TransportWriter;
        if (transportWriter != null)
        {
            await transportWriter.WriteLocker.WaitAsync(cancellationToken).ConfigureDefaultAwait();
            try
            {
                var pipeWriter = new PipeBytesWriter(transportWriter.Writer);
                BuildPackage(ref pipeWriter, protocol, package);
                await pipeWriter.FlushAsync(cancellationToken).ConfigureDefaultAwait();
            }
            finally
            {
                transportWriter.WriteLocker.Release();
            }

            return;
        }

        ThrowHelper.ThrowIfNull(this.OutputSendAsync, nameof(this.OutputSendAsync));
        await this.m_semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            var segmentedWriter = new SegmentedBytesWriter(1024);
            try
            {
                BuildPackage(ref segmentedWriter, protocol, package);
                foreach (var item in segmentedWriter.Sequence)
                {
                    await this.OutputSendAsync.Invoke(this, item, cancellationToken).ConfigureDefaultAwait();
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

    private Task SendJsonObjectAsync<T>(ushort protocol, in T obj, CancellationToken cancellationToken = default)
    {
        var bytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(obj, typeof(T), TouchSocketDmtpSourceGenerationContext.Default);
        return this.SendAsync(protocol, bytes, cancellationToken);
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
            if (channelOut.Using)
            {
                channel = null;
                return false;
            }
            channelOut.MakeUsing();
            channel = channelOut;
            return true;
        }
        channel = null;
        return false;
    }

    internal bool RemoveChannel(int id)
    {
        return this.m_userChannels.TryRemove(id, out _);
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
