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

using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// 提供Dmtp协议的最基础功能件
    /// </summary>
    public abstract class DmtpActor : DependencyObject, IDmtpActor
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

        ///// <summary>
        ///// 发送数据接口
        ///// </summary>
        //public Action<DmtpActor, ArraySegment<byte>[]> OutputSend { get; set; }

        /// <summary>
        /// 异步发送数据接口
        /// </summary>
        public Func<DmtpActor, ReadOnlyMemory<byte>, Task> OutputSendAsync { get; set; }

        #endregion 委托

        #region 属性

        /// <inheritdoc/>
        public bool AllowRoute { get; }

        /// <inheritdoc/>
        public IDmtpActorObject Client { get; set; }

        /// <inheritdoc/>
        public string Id { get; set; }

        /// <inheritdoc/>
        public bool Online { get; protected set; }

        /// <inheritdoc/>
        public bool IsReliable { get; }

        /// <inheritdoc/>
        public DateTime LastActiveTime { get; protected set; }

        /// <inheritdoc/>
        public ILog Logger { get; set; }

        /// <inheritdoc/>
        public object SyncRoot { get; } = new object();

        /// <inheritdoc/>
        public WaitHandlePool<IWaitResult> WaitHandlePool { get; protected set; }

        #endregion 属性

        /// <summary>
        /// 创建一个Dmtp协议的最基础功能件
        /// </summary>
        /// <param name="allowRoute">是否允许路由</param>
        /// <param name="isReliable">是不是基于可靠协议运行的</param>
        public DmtpActor(bool allowRoute, bool isReliable)
        {
            this.WaitHandlePool = new WaitHandlePool<IWaitResult>();
            this.AllowRoute = allowRoute;
            this.LastActiveTime = DateTime.UtcNow;
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
        public virtual async Task HandshakeAsync(string verifyToken, string id, int millisecondsTimeout, Metadata metadata, CancellationToken token)
        {
            if (this.Online)
            {
                return;
            }
            var args = new DmtpVerifyEventArgs()
            {
                Token = verifyToken,
                Id = id,
                Metadata = metadata
            };

            await this.OnHandshaking(args).ConfigureAwait(false);

            var waitVerify = new WaitVerify()
            {
                Token = args.Token,
                Id = args.Id,
                Metadata = args.Metadata
            };
            var waitData = this.WaitHandlePool.GetWaitDataAsync(waitVerify);
            waitData.SetCancellationToken(token);

            try
            {
                await this.SendJsonObjectAsync(P1_Handshake_Request, waitVerify).ConfigureAwait(false);
                switch (await waitData.WaitAsync(millisecondsTimeout).ConfigureAwait(false))
                {
                    case WaitDataStatus.SetRunning:
                        {
                            var verifyResult = (WaitVerify)waitData.WaitResult;
                            if (verifyResult.Status == 1)
                            {
                                this.Id = verifyResult.Id;
                                this.Online = true;
                                _ = Task.Factory.StartNew(this.PrivateOnHandshaked, new DmtpVerifyEventArgs()
                                {
                                    Id = verifyResult.Id,
                                    Metadata = verifyResult.Metadata,
                                    Token = verifyResult.Token,
                                });
                                verifyResult.Handle = true;
                                break;
                            }
                            else
                            {
                                verifyResult.Handle = true;
                                throw new TokenVerifyException(verifyResult.Message);
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
            finally
            {
                this.WaitHandlePool.Destroy(waitData);
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
            lock (this.SyncRoot)
            {
                if (!this.Online)
                {
                    return;
                }
                this.Online = false;
                this.WaitHandlePool.CancelAll();
            }

            if (manual || this.Closing == null)
            {
                return;
            }
            await this.Closing.Invoke(this, msg).ConfigureAwait(false);
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

        private void PrivateOnHandshaked(object obj)
        {
            this.OnHandshaked((DmtpVerifyEventArgs)obj);
        }

        private void PrivateOnCreatedChannel(object obj)
        {
            this.OnCreatedChannel((CreateChannelEventArgs)obj);
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
            this.LastActiveTime = DateTime.UtcNow;
            var byteBlock = message.BodyByteBlock;
            switch (message.ProtocolFlags)
            {
                case P0_Close:
                    {
                        await this.OnClosed(false, message.GetBodyString()).ConfigureAwait(false);
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
                            await this.OnHandshaking(args).ConfigureAwait(false);

                            if (args.Id.HasValue())
                            {
                                await this.OnIdChanged(new IdChangedEventArgs(this.Id, args.Id)).ConfigureAwait(false);
                                this.Id = args.Id;
                            }

                            if (args.IsPermitOperation)
                            {
                                waitVerify.Id = this.Id;
                                waitVerify.Status = 1;
                                await this.SendJsonObjectAsync(P2_Handshake_Response, waitVerify).ConfigureAwait(false);
                                this.Online = true;
                                args.Message = "Success";
                                _ = Task.Factory.StartNew(this.PrivateOnHandshaked, args);
                            }
                            else//不允许连接
                            {
                                waitVerify.Status = 2;
                                waitVerify.Message = args.Message;
                                await this.SendJsonObjectAsync(P2_Handshake_Response, waitVerify).ConfigureAwait(false);
                                await this.OnClosed(false, args.Message).ConfigureAwait(false);
                            }
                        }
                        catch (Exception ex)
                        {
                            this.Logger?.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                            await this.OnClosed(false, ex.Message).ConfigureAwait(false);
                        }
                        return true;
                    }
                case P2_Handshake_Response:
                    {
                        try
                        {
                            var waitVerify = ResolveJsonObject<WaitVerify>(message.GetBodyString());
                            this.WaitHandlePool.SetRun(waitVerify);
                            SpinWait.SpinUntil(() => waitVerify.Handle, 5000);
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
                                await this.OnIdChanged(new IdChangedEventArgs(waitSetId.OldId, waitSetId.NewId)).ConfigureAwait(false);
                                this.Id = waitSetId.NewId;
                                waitSetId.Status = 1;
                            }
                            catch (Exception ex)
                            {
                                waitSetId.Status = 2;
                                waitSetId.Message = ex.Message;
                            }
                            await this.SendJsonObjectAsync(P4_ResetId_Response, waitSetId).ConfigureAwait(false);
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
                            this.WaitHandlePool.SetRun(ResolveJsonObject<WaitSetId>(message.GetBodyString()));
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
                                if (await this.TryRouteAsync(new PackageRouterEventArgs(RouteType.Ping, waitPing)).ConfigureAwait(false))
                                {
                                    if (await this.TryFindDmtpActor(waitPing.TargetId).ConfigureAwait(false) is DmtpActor actor)
                                    {
                                        await actor.SendAsync(P5_Ping_Request, byteBlock.Memory).ConfigureAwait(false);
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
                            await this.SendJsonObjectAsync(P6_Ping_Response, waitPing).ConfigureAwait(false);
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
                                if (await this.TryFindDmtpActor(waitPing.TargetId).ConfigureAwait(false) is DmtpActor actor)
                                {
                                    await actor.SendAsync(P6_Ping_Response, byteBlock.Memory).ConfigureAwait(false);
                                }
                            }
                            else
                            {
                                this.WaitHandlePool.SetRun(waitPing);
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

                            waitCreateChannel.UnpackageRouter(ref byteBlock);
                            if (this.AllowRoute && waitCreateChannel.Route)
                            {
                                if (await this.TryRouteAsync(new PackageRouterEventArgs(RouteType.CreateChannel, waitCreateChannel)).ConfigureAwait(false))
                                {
                                    if (await this.TryFindDmtpActor(waitCreateChannel.TargetId).ConfigureAwait(false) is DmtpActor actor)
                                    {
                                        await actor.SendAsync(P7_CreateChannel_Request, byteBlock.Memory).ConfigureAwait(false);
                                        return true;
                                    }
                                    else
                                    {
                                        waitCreateChannel.UnpackageBody(ref byteBlock);
                                        waitCreateChannel.Status = TouchSocketDmtpStatus.ClientNotFind.ToValue();
                                    }
                                }
                                else
                                {
                                    waitCreateChannel.UnpackageBody(ref byteBlock);
                                    waitCreateChannel.Status = TouchSocketDmtpStatus.RoutingNotAllowed.ToValue();
                                }
                            }
                            else
                            {
                                waitCreateChannel.UnpackageBody(ref byteBlock);

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
                            byteBlock.Reset();

                            waitCreateChannel.Package(ref byteBlock);

                            await this.SendAsync(P8_CreateChannel_Response, byteBlock.Memory).ConfigureAwait(false);
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

                            waitCreateChannel.UnpackageRouter(ref byteBlock);
                            if (this.AllowRoute && waitCreateChannel.Route)
                            {
                                if (await this.TryFindDmtpActor(waitCreateChannel.TargetId).ConfigureAwait(false) is DmtpActor actor)
                                {
                                    await actor.SendAsync(P8_CreateChannel_Response, byteBlock.Memory).ConfigureAwait(false);
                                    return true;
                                }
                            }
                            else
                            {
                                waitCreateChannel.UnpackageBody(ref byteBlock);
                                this.WaitHandlePool.SetRun(waitCreateChannel);
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

                            channelPackage.UnpackageRouter(ref byteBlock);
                            if (this.AllowRoute && channelPackage.Route)
                            {
                                if (await this.TryFindDmtpActor(channelPackage.TargetId).ConfigureAwait(false) is DmtpActor actor)
                                {
                                    await actor.SendAsync(P9_ChannelPackage, byteBlock.Memory).ConfigureAwait(false);
                                }
                                else
                                {
                                    channelPackage.UnpackageBody(ref byteBlock);
                                    channelPackage.Message = TouchSocketDmtpStatus.ClientNotFind.GetDescription(channelPackage.TargetId);
                                    channelPackage.SwitchId();
                                    channelPackage.RunNow = true;
                                    channelPackage.DataType = ChannelDataType.DisposeOrder;
                                    byteBlock.Reset();

                                    channelPackage.Package(ref byteBlock);

                                    await this.SendAsync(P9_ChannelPackage, byteBlock.Memory).ConfigureAwait(false);
                                }
                            }
                            else
                            {
                                channelPackage.UnpackageBody(ref byteBlock);
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
        public virtual Task<bool> PingAsync(int millisecondsTimeout = 5000)
        {
            return this.PrivatePingAsync(default, millisecondsTimeout);
        }

        /// <inheritdoc/>
        public virtual async Task<bool> PingAsync(string targetId, int millisecondsTimeout = 5000)
        {
            if (this.AllowRoute && await this.TryFindDmtpActor(targetId).ConfigureAwait(false) is DmtpActor actor)
            {
                return await actor.PingAsync(millisecondsTimeout).ConfigureAwait(false);
            }
            else
            {
                return await this.PrivatePingAsync(targetId, millisecondsTimeout).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public virtual async Task ResetIdAsync(string newId)
        {
            var waitSetId = new WaitSetId(this.Id, newId);

            var waitData = this.WaitHandlePool.GetWaitDataAsync(waitSetId);

            await this.SendJsonObjectAsync(P3_ResetId_Request, waitSetId).ConfigureAwait(false);

            switch (await waitData.WaitAsync(5000).ConfigureAwait(false))
            {
                case WaitDataStatus.SetRunning:
                    {
                        if (waitData.WaitResult.Status == 1)
                        {
                            await this.OnIdChanged(new IdChangedEventArgs(this.Id, newId)).ConfigureAwait(false);
                            this.Id = newId;
                        }
                        else
                        {
                            throw new Exception(waitData.WaitResult.Message);
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

        private Task SendJsonObjectAsync<T>(ushort protocol, in T obj)
        {
#if SystemTextJson
            var bytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(obj, typeof(T), TouchSokcetDmtpSourceGenerationContext.Default);
#else
            var bytes = JsonConvert.SerializeObject(obj).ToUTF8Bytes();
#endif

            return this.SendAsync(protocol, bytes);
        }

        private static T ResolveJsonObject<T>(string json)
        {
#if SystemTextJson

            return (T)System.Text.Json.JsonSerializer.Deserialize(json, typeof(T), TouchSokcetDmtpSourceGenerationContext.Default);
#else
            return JsonConvert.DeserializeObject<T>(json);
#endif
        }

        /// <inheritdoc/>
        public virtual async Task SendPackageAsync(ushort protocol, IPackage package)
        {
            using (var byteBlock = new ByteBlock())
            {
                var block = byteBlock;
                package.Package(ref block);

                await this.SendAsync(protocol, byteBlock.Memory).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public virtual Task SendStringAsync(ushort protocol, string value)
        {
            var data = Encoding.UTF8.GetBytes(value);
            return this.SendAsync(protocol, data);
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
                if (await this.FindDmtpActor.Invoke(targetId).ConfigureAwait(false) is DmtpActor newActor)
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
                    await this.Routing.Invoke(this, e).ConfigureAwait(false);
                    return e.IsPermitOperation;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> PrivatePingAsync(string targetId, int millisecondsTimeout)
        {
            var waitPing = new WaitPing
            {
                TargetId = targetId,
                SourceId = this.Id
            };
            var waitData = this.WaitHandlePool.GetWaitDataAsync(waitPing);
            try
            {
                await this.SendJsonObjectAsync(P5_Ping_Request, waitPing).ConfigureAwait(false);
                switch (await waitData.WaitAsync(millisecondsTimeout).ConfigureAwait(false))
                {
                    case WaitDataStatus.SetRunning:
                        {
                            return waitData.WaitResult.Status.ToStatus() switch
                            {
                                TouchSocketDmtpStatus.Success => true,
                                _ => false,
                            };
                        }
                    case WaitDataStatus.Default:
                    case WaitDataStatus.Overtime:
                    case WaitDataStatus.Canceled:
                    case WaitDataStatus.Disposed:
                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                this.WaitHandlePool.Destroy(waitData);
            }
        }

        #region 断开

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (this.SyncRoot)
                {
                    if (!this.Online)
                    {
                        return;
                    }
                    this.Online = false;

                    this.Closing = null;
                    this.Routing = null;
                    this.FindDmtpActor = null;
                    this.Handshaked = null;
                    this.Handshaking = null;
                    this.IdChanged = null;
                    //this.OutputSend = null;

                    this.WaitHandlePool.CancelAll();
                    this.WaitHandlePool.SafeDispose();
                }
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        public async Task CloseAsync(string msg)
        {
            await this.OnClosed(true, msg).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<bool> SendCloseAsync(string msg)
        {
            if (!this.Online)
            {
                return false;
            }
            try
            {
                await this.SendStringAsync(0, msg).ConfigureAwait(false);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion 断开

        #region 协议异步发送

        /// <inheritdoc/>
        public virtual async Task SendAsync(ushort protocol, ReadOnlyMemory<byte> memory)
        {
            using (var byteBlock = new ValueByteBlock(memory.Length + 8))
            {
                byteBlock.Write(DmtpMessage.Head);
                byteBlock.WriteUInt16(protocol, EndianType.Big);
                byteBlock.WriteInt32(memory.Length, EndianType.Big);
                byteBlock.Write(memory.Span);

                await this.OutputSendAsync.Invoke(this, byteBlock.Memory).ConfigureAwait(false);
            }
            //var transferBytes = new ArraySegment<byte>[]
            //{
            //    new ArraySegment<byte>(DmtpMessage.Head),
            //    new ArraySegment<byte>(TouchSocketBitConverter.BigEndian.GetBytes(protocol)),
            //    new ArraySegment<byte>(TouchSocketBitConverter.BigEndian.GetBytes(length)),
            //    new ArraySegment<byte>(buffer,offset,length)
            //};
            this.LastActiveTime = DateTime.UtcNow;
        }

        ///// <inheritdoc/>
        //public virtual Task SendAsync(ushort protocol, ByteBlock byteBlock)
        //{
        //    return this.SendAsync(protocol, byteBlock.Buffer, 0, byteBlock.Length);
        //}

        #endregion 协议异步发送

        #region IDmtpChannel

        private void CheckChannelShouldBeReliable()
        {
            if (!this.IsReliable)
            {
                throw new NotSupportedException("Channel不支持在不可靠协议使用。");
            }
        }

        private readonly ConcurrentDictionary<int, InternalChannel> m_userChannels = new ConcurrentDictionary<int, InternalChannel>();

        /// <inheritdoc/>
        public virtual bool ChannelExisted(int id)
        {
            this.CheckChannelShouldBeReliable();
            return this.m_userChannels.ContainsKey(id);
        }

        ///// <inheritdoc/>
        //public virtual IDmtpChannel CreateChannelAsync(Metadata metadata = default)
        //{
        //    return this.PrivateCreateChannel(default, true, 0, metadata);
        //}

        ///// <inheritdoc/>
        //public virtual IDmtpChannel CreateChannel(int id, Metadata metadata = default)
        //{
        //    return this.PrivateCreateChannel(default, false, id, metadata);
        //}

        ///// <inheritdoc/>
        //public virtual IDmtpChannel CreateChannel(string targetId, int id, Metadata metadata = default)
        //{
        //    if (string.IsNullOrEmpty(targetId))
        //    {
        //        throw new ArgumentException($"“{nameof(targetId)}”不能为 null 或空。", nameof(targetId));
        //    }
        //    if (this.AllowRoute && this.TryFindDmtpActor(targetId).GetFalseAwaitResult() is DmtpActor actor)
        //    {
        //        return actor.CreateChannel(id, metadata);
        //    }
        //    else
        //    {
        //        return this.PrivateCreateChannel(targetId, false, id, metadata);
        //    }
        //}

        ///// <inheritdoc/>
        //public virtual IDmtpChannel CreateChannel(string targetId, Metadata metadata = default)
        //{
        //    if (string.IsNullOrEmpty(targetId))
        //    {
        //        throw new ArgumentException($"“{nameof(targetId)}”不能为 null 或空。", nameof(targetId));
        //    }

        //    if (this.AllowRoute && this.TryFindDmtpActor(targetId).GetFalseAwaitResult() is DmtpActor actor)
        //    {
        //        return actor.CreateChannel(metadata);
        //    }
        //    else
        //    {
        //        return this.PrivateCreateChannel(targetId, true, 0, metadata);
        //    }
        //}

        /// <inheritdoc/>
        public virtual Task<IDmtpChannel> CreateChannelAsync(Metadata metadata = default)
        {
            return this.PrivateCreateChannelAsync(default, true, 0, metadata);
        }

        /// <inheritdoc/>
        public virtual Task<IDmtpChannel> CreateChannelAsync(int id, Metadata metadata = default)
        {
            return this.PrivateCreateChannelAsync(default, false, id, metadata);
        }

        /// <inheritdoc/>
        public virtual async Task<IDmtpChannel> CreateChannelAsync(string targetId, int id, Metadata metadata = default)
        {
            if (string.IsNullOrEmpty(targetId))
            {
                throw new ArgumentException($"“{nameof(targetId)}”不能为 null 或空。", nameof(targetId));
            }
            if (this.AllowRoute && await this.TryFindDmtpActor(targetId).ConfigureAwait(false) is DmtpActor actor)
            {
                return await actor.CreateChannelAsync(id, metadata).ConfigureAwait(false);
            }
            else
            {
                return await this.PrivateCreateChannelAsync(targetId, false, id, metadata).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public virtual async Task<IDmtpChannel> CreateChannelAsync(string targetId, Metadata metadata = default)
        {
            if (string.IsNullOrEmpty(targetId))
            {
                throw new ArgumentException($"“{nameof(targetId)}”不能为 null 或空。", nameof(targetId));
            }

            if (this.AllowRoute && await this.TryFindDmtpActor(targetId).ConfigureAwait(false) is DmtpActor actor)
            {
                return await actor.CreateChannelAsync(metadata).ConfigureAwait(false);
            }
            else
            {
                return await this.PrivateCreateChannelAsync(targetId, true, 0, metadata).ConfigureAwait(false);
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
                channelOut.SetUsing();
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

        //internal async Task SendChannelPackage(ChannelPackage channelPackage)
        //{
        //    using (var byteBlock = new ByteBlock(channelPackage.GetLen()))
        //    {
        //        channelPackage.Package(byteBlock);
        //        await this.SendAsync(P9_ChannelPackage, byteBlock);
        //    }
        //}

        internal async Task SendChannelPackageAsync(ChannelPackage channelPackage)
        {
            using (var byteBlock = new ByteBlock(channelPackage.GetLen()))
            {
                var block = byteBlock;
                channelPackage.Package(ref block);
                await this.SendAsync(P9_ChannelPackage, byteBlock.Memory).ConfigureAwait(false);
            }
        }

        //private IDmtpChannel PrivateCreateChannel(string targetId, bool random, int id, Metadata metadata)
        //{
        //    this.CheckChannelShouldBeReliable();

        //    if (random)
        //    {
        //        id = new object().GetHashCode();
        //    }
        //    else
        //    {
        //        if (this.ChannelExisted(id))
        //        {
        //            throw new Exception(TouchSocketDmtpStatus.ChannelExisted.GetDescription(id));
        //        }
        //    }

        //    var byteBlock = new ByteBlock();
        //    var waitCreateChannel = new WaitCreateChannelPackage()
        //    {
        //        Random = random,
        //        ChannelId = id,
        //        SourceId = this.Id,
        //        TargetId = targetId,
        //        Metadata = metadata,
        //        Route = targetId.HasValue()
        //    };
        //    var waitData = this.WaitHandlePool.GetWaitData(waitCreateChannel);

        //    try
        //    {
        //        waitCreateChannel.Package(byteBlock);
        //        this.123Send(P7_CreateChannel_Request, byteBlock);
        //        switch (waitData.Wait(10 * 1000))
        //        {
        //            case WaitDataStatus.SetRunning:
        //                {
        //                    var result = (WaitCreateChannelPackage)waitData.WaitResult;
        //                    switch (result.Status.ToStatus())
        //                    {
        //                        case TouchSocketDmtpStatus.Success:
        //                            {
        //                                var channel = new InternalChannel(this, targetId, result.Metadata);
        //                                channel.SetId(result.ChannelId);
        //                                channel.SetUsing();
        //                                if (this.m_userChannels.TryAdd(result.ChannelId, channel))
        //                                {
        //                                    return channel;
        //                                }
        //                                else
        //                                {
        //                                    throw new Exception(TouchSocketDmtpStatus.UnknownError.GetDescription());
        //                                }
        //                            }
        //                        case TouchSocketDmtpStatus.ClientNotFind:
        //                            {
        //                                throw new Exception(TouchSocketDmtpStatus.ClientNotFind.GetDescription(targetId));
        //                            }
        //                        case TouchSocketDmtpStatus.RoutingNotAllowed:
        //                        default:
        //                            {
        //                                throw new Exception(result.Status.ToStatus().GetDescription(result.Message));
        //                            }
        //                    }
        //                }
        //            case WaitDataStatus.Overtime:
        //                {
        //                    throw new TimeoutException(TouchSocketDmtpStatus.Overtime.GetDescription());
        //                }
        //            default:
        //                {
        //                    throw new Exception(TouchSocketDmtpStatus.UnknownError.GetDescription());
        //                }
        //        }
        //    }
        //    finally
        //    {
        //        this.WaitHandlePool.Destroy(waitData);
        //        byteBlock.Dispose();
        //    }
        //}

        private async Task<IDmtpChannel> PrivateCreateChannelAsync(string targetId, bool random, int id, Metadata metadata)
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

            var byteBlock = new ByteBlock();
            var waitCreateChannel = new WaitCreateChannelPackage()
            {
                Random = random,
                ChannelId = id,
                SourceId = this.Id,
                TargetId = targetId,
                Metadata = metadata
            };
            var waitData = this.WaitHandlePool.GetWaitDataAsync(waitCreateChannel);

            try
            {
                waitCreateChannel.Package(ref byteBlock);

                await this.SendAsync(P7_CreateChannel_Request, byteBlock.Memory).ConfigureAwait(false);
                switch (await waitData.WaitAsync(10 * 1000).ConfigureAwait(false))
                {
                    case WaitDataStatus.SetRunning:
                        {
                            var result = (WaitCreateChannelPackage)waitData.WaitResult;
                            switch (result.Status.ToStatus())
                            {
                                case TouchSocketDmtpStatus.Success:
                                    {
                                        var channel = new InternalChannel(this, targetId, result.Metadata);
                                        channel.SetId(result.ChannelId);
                                        channel.SetUsing();
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
            finally
            {
                this.WaitHandlePool.Destroy(waitData);
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
            lock (this.SyncRoot)
            {
                var channel = new InternalChannel(this, targetId, metadata);
                channel.SetId(id);
                if (this.m_userChannels.TryAdd(id, channel))
                {
                    Task.Factory.StartNew(this.PrivateOnCreatedChannel, new CreateChannelEventArgs(id, metadata));
                    return true;
                }
                else
                {
                    channel.SafeDispose();
                    return false;
                }
            }
        }

        #endregion IDmtpChannel
    }
}