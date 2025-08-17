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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
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
    /// 请求关闭
    /// </summary>
    public Func<DmtpActor, string, Task> Closing { get; set; }

    /// <summary>
    /// 当创建通道时
    /// </summary>
    public Func<DmtpActor, CreateChannelEventArgs, Task> CreatedChannel { get; set; }

    /// <summary>
    /// 查找其他IDmtpActor
    /// </summary>
    public Func<string, Task<IDmtpActor>> FindDmtpActor { get; set; }

    /// <summary>
    /// 在完成握手连接时
    /// </summary>
    public Func<DmtpActor, DmtpVerifyEventArgs, Task> Handshaked { get; set; }

    /// <summary>
    /// 握手
    /// </summary>
    public Func<DmtpActor, DmtpVerifyEventArgs, Task> Handshaking { get; set; }

    /// <summary>
    /// 重设Id
    /// </summary>
    public Func<DmtpActor, IdChangedEventArgs, Task> IdChanged { get; set; }

    /// <summary>
    /// 当需要路由的时候
    /// </summary>
    public Func<DmtpActor, PackageRouterEventArgs, Task> Routing { get; set; }

    /// <summary>
    /// 异步发送数据接口
    /// </summary>
    public Func<DmtpActor, ReadOnlyMemory<byte>, CancellationToken, Task> OutputSendAsync { get; set; }

    //public ITransport Transport { get; set; }
    #endregion 委托

    #region 属性

    /// <inheritdoc/>
    public bool AllowRoute { get; }

    /// <inheritdoc/>
    public IDmtpActorObject Client { get; set; }

    /// <inheritdoc/>
    public string Id { get; set; }

    /// <inheritdoc/>
    public virtual bool Online => this.m_online;

    /// <inheritdoc/>
    public bool IsReliable { get; }

    /// <inheritdoc/>
    public DateTimeOffset LastActiveTime { get; protected set; }

    /// <inheritdoc/>
    public ILog Logger { get; set; }

    /// <inheritdoc/>
    public WaitHandlePool<IWaitResult> WaitHandlePool { get; }

    /// <inheritdoc/>
    public CancellationToken ClosedToken => this.m_cancellationTokenSource.Token;

    #endregion 属性

    #region 字段
    private readonly ConcurrentDictionary<int, InternalChannel> m_userChannels = new ConcurrentDictionary<int, InternalChannel>();
    private readonly AsyncManualResetEvent m_handshakeFinished = new(false);
    private CancellationTokenSource m_cancellationTokenSource;
    private bool m_online;
    private readonly Lock m_syncRoot = new Lock();
    private readonly Dictionary<Type, IActor> m_actors = new Dictionary<Type, IActor>();
    #endregion

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
    /// <exception cref="Exception"></exception>
    /// <exception cref="TokenVerifyException"></exception>
    /// <exception cref="TimeoutException"></exception>
    public virtual async Task HandshakeAsync(string verifyToken, string id, Metadata metadata, CancellationToken token)
    {
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

        await this.OnHandshaking(args).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        var waitVerify = new WaitVerify()
        {
            Token = args.Token,
            Id = args.Id,
            Metadata = args.Metadata
        };

        using (var waitData = this.WaitHandlePool.GetWaitDataAsync(waitVerify))
        {
            await this.SendJsonObjectAsync(P1_Handshake_Request, waitVerify, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            switch (await waitData.WaitAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
            {
                case WaitDataStatus.Success:
                    {
                        var verifyResult = (WaitVerify)waitData.CompletedData;
                        if (verifyResult.Status == 1)
                        {
                            this.Id = verifyResult.Id;
                            this.m_online = true;
                            _ = EasyTask.SafeRun(this.PrivateOnHandshaked, new DmtpVerifyEventArgs()
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
    /// 当关闭后
    /// </summary>
    /// <param name="manual"></param>
    /// <param name="msg"></param>
    protected virtual async Task OnClosed(bool manual, string msg)
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

        if (manual || this.Closing == null)
        {
            return;
        }
        await this.Closing.Invoke(this, msg).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    }

    /// <summary>
    /// 正在握手连接
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    protected virtual Task OnHandshaking(DmtpVerifyEventArgs e)
    {
        if (this.Handshaking != null)
        {
            return this.Handshaking.Invoke(this, e);
        }
        return EasyTask.CompletedTask;
    }

    /// <summary>
    /// 握手连接完成
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    protected virtual Task OnHandshaked(DmtpVerifyEventArgs e)
    {
        if (this.Handshaked != null)
        {
            return this.Handshaked.Invoke(this, e);
        }
        return EasyTask.CompletedTask;
    }

    /// <summary>
    /// 当Id修改时
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    protected virtual Task OnIdChanged(IdChangedEventArgs e)
    {
        if (this.IdChanged != null)
        {
            return this.IdChanged.Invoke(this, e);
        }
        return EasyTask.CompletedTask;
    }

    /// <summary>
    /// 当完成创建通道时
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    protected virtual Task OnCreatedChannel(CreateChannelEventArgs e)
    {
        if (this.CreatedChannel != null)
        {
            return this.CreatedChannel.Invoke(this, e);
        }
        return EasyTask.CompletedTask;
    }

    private async Task PrivateOnHandshaked(DmtpVerifyEventArgs e)
    {
        await this.OnHandshaked(e);
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
    /// </list>
    /// </para>
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public virtual async Task<bool> InputReceivedData(DmtpMessage message)
    {
        this.LastActiveTime = DateTimeOffset.UtcNow;
        var reader = new BytesReader(message.Memory);
        switch (message.ProtocolFlags)
        {
            case P0_Close:
                {
                    await this.OnClosed(false, message.GetBodyString()).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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
                        await this.OnHandshaking(args).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

                        if (args.Id.HasValue())
                        {
                            await this.OnIdChanged(new IdChangedEventArgs(this.Id, args.Id)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                            this.Id = args.Id;
                        }

                        if (args.IsPermitOperation)
                        {
                            waitVerify.Id = this.Id;
                            waitVerify.Status = 1;
                            waitVerify.Metadata = args.Metadata;
                            waitVerify.Message = args.Message ?? TouchSocketCoreResource.OperationSuccessful;
                            await this.SendJsonObjectAsync(P2_Handshake_Response, waitVerify).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                            this.m_online = true;
                            args.Message ??= TouchSocketCoreResource.OperationSuccessful;
                            this.m_cancellationTokenSource = new CancellationTokenSource();
                            _ = EasyTask.SafeRun(this.PrivateOnHandshaked, args);
                        }
                        else//不允许连接
                        {
                            waitVerify.Status = 2;
                            waitVerify.Metadata = args.Metadata;
                            waitVerify.Message = TouchSocketDmtpResource.RemoteRefuse.Format(args.Message);
                            await this.SendJsonObjectAsync(P2_Handshake_Response, waitVerify).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                            await this.OnClosed(false, args.Message).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Logger?.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                        await this.OnClosed(false, ex.Message).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                    }
                    return true;
                }
            case P2_Handshake_Response:
                {
                    try
                    {
                        var waitVerify = ResolveJsonObject<WaitVerify>(message.GetBodyString());
                        this.WaitHandlePool.Set(waitVerify);
                        await this.m_handshakeFinished.WaitOneAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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
                            await this.OnIdChanged(new IdChangedEventArgs(waitSetId.OldId, waitSetId.NewId)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                            this.Id = waitSetId.NewId;
                            waitSetId.Status = 1;
                        }
                        catch (Exception ex)
                        {
                            waitSetId.Status = 2;
                            waitSetId.Message = ex.Message;
                        }
                        await this.SendJsonObjectAsync(P4_ResetId_Response, waitSetId).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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
                            if (await this.TryRouteAsync(new PackageRouterEventArgs(RouteType.Ping, waitPing)).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
                            {
                                if (await this.TryFindDmtpActor(waitPing.TargetId).ConfigureAwait(EasyTask.ContinueOnCapturedContext) is DmtpActor actor)
                                {
                                    await actor.SendAsync(P5_Ping_Request, message.Memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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
                        await this.SendJsonObjectAsync(P6_Ping_Response, waitPing).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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
                            if (await this.TryFindDmtpActor(waitPing.TargetId).ConfigureAwait(EasyTask.ContinueOnCapturedContext) is DmtpActor actor)
                            {
                                await actor.SendAsync(P6_Ping_Response, message.Memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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
                            if (await this.TryRouteAsync(new PackageRouterEventArgs(RouteType.CreateChannel, waitCreateChannel)).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
                            {
                                if (await this.TryFindDmtpActor(waitCreateChannel.TargetId).ConfigureAwait(EasyTask.ContinueOnCapturedContext) is DmtpActor actor)
                                {
                                    await actor.SendAsync(P7_CreateChannel_Request, message.Memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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

                        await this.SendAsync(P8_CreateChannel_Response, waitCreateChannel).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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
                            if (await this.TryFindDmtpActor(waitCreateChannel.TargetId).ConfigureAwait(EasyTask.ContinueOnCapturedContext) is DmtpActor actor)
                            {
                                await actor.SendAsync(P8_CreateChannel_Response, message.Memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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
                            if (await this.TryFindDmtpActor(channelPackage.TargetId).ConfigureAwait(EasyTask.ContinueOnCapturedContext) is DmtpActor actor)
                            {
                                await actor.SendAsync(P9_ChannelPackage, message.Memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                            }
                            else
                            {
                                channelPackage.UnpackageBody(ref reader);
                                channelPackage.Message = TouchSocketDmtpStatus.ClientNotFind.GetDescription(channelPackage.TargetId);
                                channelPackage.SwitchId();
                                channelPackage.DataType = ChannelDataType.Canceled;
                                await this.SendAsync(P9_ChannelPackage, channelPackage).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                            }
                        }
                        else
                        {
                            channelPackage.UnpackageBody(ref reader);
                            this.QueueChannelPackage(channelPackage);
                        }
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
    public virtual Task<Result> PingAsync(CancellationToken token = default)
    {
        return this.PrivatePingAsync(default, token);
    }

    /// <inheritdoc/>
    public virtual async Task<Result> PingAsync(string targetId, CancellationToken token = default)
    {
        if (this.AllowRoute && await this.TryFindDmtpActor(targetId).ConfigureAwait(EasyTask.ContinueOnCapturedContext) is DmtpActor actor)
        {
            return await actor.PingAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        else
        {
            return await this.PrivatePingAsync(targetId, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }

    /// <inheritdoc/>
    public virtual async Task ResetIdAsync(string newId, CancellationToken token)
    {
        var waitSetId = new WaitSetId(this.Id, newId);

        var waitData = this.WaitHandlePool.GetWaitDataAsync(waitSetId);

        await this.SendJsonObjectAsync(P3_ResetId_Request, waitSetId, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

        switch (await waitData.WaitAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
        {
            case WaitDataStatus.Success:
                {
                    if (waitData.CompletedData.Status == 1)
                    {
                        await this.OnIdChanged(new IdChangedEventArgs(this.Id, newId)).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                        this.Id = newId;
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

    private Task SendJsonObjectAsync<T>(ushort protocol, in T obj, CancellationToken token = default)
    {
        var bytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(obj, typeof(T), TouchSocketDmtpSourceGenerationContext.Default);
        return this.SendAsync(protocol, bytes, token);
    }

    private static T ResolveJsonObject<T>(string json)
    {
        return (T)System.Text.Json.JsonSerializer.Deserialize(json, typeof(T), TouchSocketDmtpSourceGenerationContext.Default);
    }


    /// <inheritdoc/>
    public virtual async Task<DmtpActor> TryFindDmtpActor(string targetId)
    {
        if (targetId == this.Id)
        {
            return this;
        }
        if (this.FindDmtpActor != null)
        {
            if (await this.FindDmtpActor.Invoke(targetId).ConfigureAwait(EasyTask.ContinueOnCapturedContext) is DmtpActor newActor)
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
                await this.Routing.Invoke(this, e).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                return e.IsPermitOperation;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    private async Task<Result> PrivatePingAsync(string targetId, CancellationToken token = default)
    {
        if (!this.Online)
        {
            return Result.FromFail(TouchSocketResource.ClientNotConnected);
        }
        var waitPing = new WaitPing
        {
            TargetId = targetId,
            SourceId = this.Id
        };
        using (var waitData = this.WaitHandlePool.GetWaitDataAsync(waitPing))
        {
            await this.SendJsonObjectAsync(P5_Ping_Request, waitPing, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            switch (await waitData.WaitAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
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
    public bool TryAddActor<TActor>(TActor actor) where TActor : class, IActor
    {
        ThrowHelper.ThrowArgumentNullExceptionIf(actor, nameof(actor));
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

    /// <inheritdoc/>
    public void AddActor<TActor>(TActor actor) where TActor : class, IActor
    {
        ThrowHelper.ThrowArgumentNullExceptionIf(actor, nameof(actor));
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

    #endregion

    #region 断开

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

                this.Closing = null;
                this.Routing = null;
                this.FindDmtpActor = null;
                this.Handshaked = null;
                this.Handshaking = null;
                this.IdChanged = null;
                //this.OutputSend = null;

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

    /// <inheritdoc/>
    public async Task<Result> CloseAsync(string msg, CancellationToken token = default)
    {
        try
        {
            await this.OnClosed(true, msg).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex.Message);
        }
    }

    /// <summary>
    /// 异步发送关闭消息
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<Result> SendCloseAsync(string msg, CancellationToken token = default)
    {
        if (!this.m_online)
        {
            return Result.FromFail(TouchSocketResource.ClientNotConnected);
        }
        try
        {
            await this.SendAsync(0, msg, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            return Result.Success;
        }
        catch (Exception ex)
        {
            return Result.FromException(ex);
        }
    }

    #endregion 断开

    #region 协议异步发送

    ///// <inheritdoc/>
    //public virtual async Task SendAsync(ushort protocol, ReadOnlyMemory<byte> memory, CancellationToken token = default)
    //{
    //    var writer = await this.Transport.CreateWriter(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

    //    try
    //    {
    //        writer.Write(DmtpMessage.Head);
    //        WriterExtension.WriteValue(ref writer, protocol, EndianType.Big);
    //        WriterExtension.WriteValue(ref writer, memory.Length, EndianType.Big);
    //        writer.Write(memory.Span);
    //        await writer.FlushAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    //        this.LastActiveTime = DateTimeOffset.UtcNow;
    //    }
    //    finally
    //    {
    //        writer.Dispose();
    //    }
    //}

    //public async Task SendAsync<TPackage>(ushort protocol, TPackage package, CancellationToken token = default)
    //    where TPackage:IPackage
    //{
    //    var writer = await this.Transport.CreateWriter(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

    //    try
    //    {
    //        writer.Write(DmtpMessage.Head);
    //        WriterExtension.WriteValue(ref writer, protocol, EndianType.Big);
    //        var writerAnchor = new WriterAnchor<PipeBytesWriter>(ref writer,4);
    //        package.Package(ref writer);
    //        var lengthSpan=writerAnchor.Rewind(ref writer, out var length);
    //        lengthSpan.WriteValue<int>(length, EndianType.Big);
    //        await writer.FlushAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    //        this.LastActiveTime = DateTimeOffset.UtcNow;
    //    }
    //    finally
    //    {
    //        writer.Dispose();
    //    }
    //}

    ///// <inheritdoc/>
    //public async Task SendAsync(ushort protocol, string value, CancellationToken token = default)
    //{
    //    var writer = await this.Transport.CreateWriter(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);

    //    try
    //    {
    //        writer.Write(DmtpMessage.Head);
    //        WriterExtension.WriteValue(ref writer, protocol, EndianType.Big);
    //        var writerAnchor = new WriterAnchor<PipeBytesWriter>(ref writer, 4);
    //        WriterExtension.WriteNormalString(ref writer, value, Encoding.UTF8);
    //        var lengthSpan = writerAnchor.Rewind(ref writer, out var length);
    //        lengthSpan.WriteValue<int>(length, EndianType.Big);
    //        await writer.FlushAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
    //        this.LastActiveTime = DateTimeOffset.UtcNow;
    //    }
    //    finally
    //    {
    //        writer.Dispose();
    //    }
    //}

    /// <inheritdoc/>
    public virtual async Task SendAsync(ushort protocol, ReadOnlyMemory<byte> memory, CancellationToken token = default)
    {
        var writer = new SegmentedBytesWriter(memory.Length + 8);
        await this.m_semaphoreSlim.WaitAsync(token);
        try
        {
            writer.Write(DmtpMessage.Head);
            writer.WriteValue(protocol, EndianType.Big);
            WriterExtension.WriteValue<SegmentedBytesWriter, int>(ref writer, memory.Length, EndianType.Big);
            writer.Write(memory.Span);

            foreach (var item in writer.Sequence)
            {
                await this.OutputSendAsync.Invoke(this, item, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
            }
        }
        finally
        {
            writer.Dispose();
            this.m_semaphoreSlim.Release();
        }
    }
    private readonly SemaphoreSlim m_semaphoreSlim = new SemaphoreSlim(1, 1);
    public async Task SendAsync<TPackage>(ushort protocol, TPackage package, CancellationToken token = default)
        where TPackage : IPackage
    {
        var byteBlock = new ByteBlock(1024 * 64);
        try
        {
            package.Package(ref byteBlock);
            await this.SendAsync(protocol, byteBlock.Memory, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        finally
        {
            byteBlock.Dispose();
        }
    }

    /// <inheritdoc/>
    public async Task SendAsync(ushort protocol, string value, CancellationToken token = default)
    {
        var byteBlock = new ByteBlock(1024 * 64);
        try
        {
            WriterExtension.WriteNormalString(ref byteBlock, value, Encoding.UTF8);
            await this.SendAsync(protocol, byteBlock.Memory, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        finally
        {
            byteBlock.Dispose();
        }
    }
    #endregion 协议异步发送

    #region IDmtpChannel

    private void CheckChannelShouldBeReliable()
    {
        if (!this.IsReliable)
        {
            throw new NotSupportedException("Channel不支持在不可靠协议使用。");
        }
    }

    /// <inheritdoc/>
    public virtual bool ChannelExisted(int id)
    {
        this.CheckChannelShouldBeReliable();
        return this.m_userChannels.ContainsKey(id);
    }

    /// <inheritdoc/>
    public virtual Task<IDmtpChannel> CreateChannelAsync(Metadata metadata = default, CancellationToken token = default)
    {
        return this.PrivateCreateChannelAsync(default, true, 0, metadata);
    }

    /// <inheritdoc/>
    public virtual Task<IDmtpChannel> CreateChannelAsync(int id, Metadata metadata = default, CancellationToken token = default)
    {
        return this.PrivateCreateChannelAsync(default, false, id, metadata);
    }

    /// <inheritdoc/>
    public virtual async Task<IDmtpChannel> CreateChannelAsync(string targetId, int id, Metadata metadata = default, CancellationToken token = default)
    {
        if (string.IsNullOrEmpty(targetId))
        {
            throw new ArgumentException($"“{nameof(targetId)}”不能为 null 或空。", nameof(targetId));
        }
        if (this.AllowRoute && await this.TryFindDmtpActor(targetId).ConfigureAwait(EasyTask.ContinueOnCapturedContext) is DmtpActor actor)
        {
            return await actor.CreateChannelAsync(id, metadata, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        else
        {
            return await this.PrivateCreateChannelAsync(targetId, false, id, metadata).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }

    /// <inheritdoc/>
    public virtual async Task<IDmtpChannel> CreateChannelAsync(string targetId, Metadata metadata = default, CancellationToken token = default)
    {
        if (string.IsNullOrEmpty(targetId))
        {
            throw new ArgumentException($"“{nameof(targetId)}”不能为 null 或空。", nameof(targetId));
        }

        if (this.AllowRoute && await this.TryFindDmtpActor(targetId).ConfigureAwait(EasyTask.ContinueOnCapturedContext) is DmtpActor actor)
        {
            return await actor.CreateChannelAsync(metadata, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
        else
        {
            return await this.PrivateCreateChannelAsync(targetId, true, 0, metadata).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
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

    internal async Task SendChannelPackageAsync(ChannelPackage channelPackage)
    {
        using (var byteBlock = new ByteBlock(channelPackage.GetLen()))
        {
            var block = byteBlock;
            channelPackage.Package(ref block);
            await this.SendAsync(P9_ChannelPackage, byteBlock.Memory).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
        }
    }

    private async Task<IDmtpChannel> PrivateCreateChannelAsync(string targetId, bool random, int id, Metadata metadata, CancellationToken token = default)
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
                SourceId = this.Id,
                TargetId = targetId,
                Metadata = metadata
            };

            using (var waitData = this.WaitHandlePool.GetWaitDataAsync(waitCreateChannel))
            {
                waitCreateChannel.Package(ref byteBlock);

                await this.SendAsync(P7_CreateChannel_Request, byteBlock.Memory, token).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                switch (await waitData.WaitAsync(token).ConfigureAwait(EasyTask.ContinueOnCapturedContext))
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

    private void QueueChannelPackage(ChannelPackage channelPackage)
    {
        if (this.m_userChannels.TryGetValue(channelPackage.ChannelId, out var channel))
        {
            channel.ReceivedData(channelPackage);
        }
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