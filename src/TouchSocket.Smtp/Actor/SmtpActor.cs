using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Resources;

namespace TouchSocket.Smtp
{
    /// <summary>
    /// 提供SMTP协议的最基础功能件
    /// </summary>
    public class SmtpActor : DependencyObject, ISmtpActor
    {
        #region 委托

        /// <summary>
        /// 请求关闭
        /// </summary>
        public Action<SmtpActor, string> OnClose { get; set; }

        /// <summary>
        /// 当创建通道时
        /// </summary>
        public Action<SmtpActor, CreateChannelEventArgs> OnCreateChannel { get; set; }

        /// <summary>
        /// 查找其他ISmtpActor
        /// </summary>
        public Func<string, ISmtpActor> OnFindSmtpActor { get; set; }

        /// <summary>
        /// 在完成握手连接时
        /// </summary>
        public Action<SmtpActor, SmtpVerifyEventArgs> OnHandshaked { get; set; }

        /// <summary>
        /// 握手
        /// </summary>
        public Action<SmtpActor, SmtpVerifyEventArgs> OnHandshaking { get; set; }

        /// <summary>
        /// 重设Id
        /// </summary>
        public Action<SmtpActor, WaitSetId> OnResetId { get; set; }

        /// <summary>
        /// 当需要路由的时候
        /// </summary>
        public Action<SmtpActor, PackageRouterEventArgs> OnRouting { get; set; }

        /// <summary>
        /// 发送数据接口
        /// </summary>
        public Action<SmtpActor, ArraySegment<byte>[]> OutputSend { get; set; }

        #endregion 委托

        #region 属性

        /// <inheritdoc/>
        public bool AllowRoute { get; private set; }

        /// <inheritdoc/>
        public ISmtpActorObject Client { get; set; }

        /// <inheritdoc/>
        public string Id { get; set; }

        /// <inheritdoc/>
        public bool IsHandshaked { get; private set; }

        /// <inheritdoc/>
        public DateTime LastActiveTime { get; private set; }

        /// <inheritdoc/>
        public ILog Logger { get; set; }

        /// <inheritdoc/>
        public object SyncRoot { get; } = new object();

        /// <inheritdoc/>
        public WaitHandlePool<IWaitResult> WaitHandlePool { get; private set; }

        #endregion 属性

        /// <summary>
        /// 构造函数
        /// </summary>
        public SmtpActor(bool allowRoute)
        {
            this.WaitHandlePool = new WaitHandlePool<IWaitResult>();
            this.AllowRoute = allowRoute;
            this.LastActiveTime = DateTime.Now;
        }

        /// <inheritdoc/>
        public void Close(bool sendClose, string message)
        {
            try
            {
                if (this.IsHandshaked)
                {
                    try
                    {
                        if (sendClose)
                        {
                            this.SendString(0, message);
                        }
                    }
                    catch
                    {
                    }
                    this.IsHandshaked = false;
                    this.WaitHandlePool.CancelAll();
                    this.OnClose?.Invoke(this, message);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// 建立对点
        /// </summary>
        /// <exception cref="Exception"></exception>
        /// <exception cref="TokenVerifyException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public void Handshake(string verifyToken, string id, int timeout, Metadata metadata, CancellationToken token)
        {
            if (this.IsHandshaked)
            {
                return;
            }

            var args = new SmtpVerifyEventArgs()
            {
                Token = verifyToken,
                Id = id,
                Metadata = metadata
            };

            this.OnHandshaking?.Invoke(this, args);

            var waitVerify = new WaitVerify()
            {
                Token = args.Token,
                Id = args.Id,
                Metadata = args.Metadata
            };

            var waitData = this.WaitHandlePool.GetWaitData(waitVerify);
            waitData.SetCancellationToken(token);

            try
            {
                this.SendJsonObject(P1_Handshake_Request, waitVerify);
                switch (waitData.Wait(timeout))
                {
                    case WaitDataStatus.SetRunning:
                        {
                            var verifyResult = (WaitVerify)waitData.WaitResult;
                            if (verifyResult.Status == 1)
                            {
                                this.Id = verifyResult.Id;
                                this.IsHandshaked = true;
                                this.PrivateHandshaked(new SmtpVerifyEventArgs()
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
                                this.Close(false, verifyResult.Message);
                                throw new TokenVerifyException(verifyResult.Message);
                            }
                        }
                    case WaitDataStatus.Overtime:
                        this.Close(false, TouchSocketSmtpStatus.Overtime.GetDescription());
                        throw new TimeoutException(TouchSocketSmtpStatus.Overtime.GetDescription());
                    case WaitDataStatus.Canceled:
                    case WaitDataStatus.Disposed:
                    default:
                        this.Close(false, null);
                        return;
                }
            }
            finally
            {
                this.WaitHandlePool.Destroy(waitData);
            }
        }

        /// <summary>
        /// 建立对点
        /// </summary>
        /// <exception cref="Exception"></exception>
        /// <exception cref="TokenVerifyException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public async Task HandshakeAsync(string verifyToken, string id, int timeout, Metadata metadata, CancellationToken token)
        {
            if (this.IsHandshaked)
            {
                return;
            }
            var args = new SmtpVerifyEventArgs()
            {
                Token = verifyToken,
                Id = id,
                Metadata = metadata
            };

            this.OnHandshaking?.Invoke(this, args);

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
                this.SendJsonObject(P1_Handshake_Request, waitVerify);
                switch (await waitData.WaitAsync(timeout))
                {
                    case WaitDataStatus.SetRunning:
                        {
                            var verifyResult = (WaitVerify)waitData.WaitResult;
                            if (verifyResult.Status == 1)
                            {
                                this.Id = verifyResult.Id;
                                this.IsHandshaked = true;
                                this.PrivateHandshaked(new SmtpVerifyEventArgs()
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
                                this.Close(false, verifyResult.Message);
                                throw new TokenVerifyException(verifyResult.Message);
                            }
                        }
                    case WaitDataStatus.Overtime:
                        this.Close(false, TouchSocketSmtpStatus.Overtime.GetDescription());
                        throw new TimeoutException(TouchSocketSmtpStatus.Overtime.GetDescription());
                    case WaitDataStatus.Canceled:
                    case WaitDataStatus.Disposed:
                    default:
                        this.Close(false, null);
                        return;
                }
            }
            finally
            {
                this.WaitHandlePool.Destroy(waitData);
            }
        }

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
        public bool InputReceivedData(SmtpMessage message)
        {
            this.LastActiveTime = DateTime.Now;
            var byteBlock = message.BodyByteBlock;
            switch (message.ProtocolFlags)
            {
                case P0_Close:
                    {
                        this.Close(false, message.GetBodyString());
                        return true;
                    }
                case P1_Handshake_Request:
                    {
                        try
                        {
                            var waitVerify = message.GetBodyString().FromJson<WaitVerify>();
                            var args = new SmtpVerifyEventArgs()
                            {
                                Token = waitVerify.Token,
                                Metadata = waitVerify.Metadata,
                                Id = waitVerify.Id,
                            };
                            this.OnHandshaking?.Invoke(this, args);

                            if (args.Id.HasValue())
                            {
                                this.OnResetId?.Invoke(this, new WaitSetId(this.Id, args.Id));
                                this.Id = args.Id;
                            }

                            if (args.IsPermitOperation)
                            {
                                waitVerify.Id = this.Id;
                                waitVerify.Status = 1;
                                this.SendJsonObject(P2_Handshake_Response, waitVerify);
                                this.IsHandshaked = true;
                                args.Message = "Success";
                                this.PrivateHandshaked(args);
                            }
                            else//不允许连接
                            {
                                waitVerify.Status = 2;
                                waitVerify.Message = args.Message;
                                this.SendJsonObject(P2_Handshake_Response, waitVerify);
                                this.Close(false, args.Message);
                            }
                        }
                        catch (Exception ex)
                        {
                            this.Close(false, ex.Message);
                        }
                        return true;
                    }
                case P2_Handshake_Response:
                    {
                        try
                        {
                            var waitVerify = message.GetBodyString().FromJson<WaitVerify>();
                            this.WaitHandlePool.SetRun(waitVerify);
                            SpinWait.SpinUntil(() =>
                            {
                                return waitVerify.Handle;
                            }, 5000);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                        }

                        return true;
                    }
                case P3_ResetId_Request:
                    {
                        try
                        {
                            var waitSetId = message.GetBodyString().FromJson<WaitSetId>();
                            try
                            {
                                this.OnResetId?.Invoke(this, waitSetId);
                                this.Id = waitSetId.NewId;
                                waitSetId.Status = 1;
                            }
                            catch (Exception ex)
                            {
                                waitSetId.Status = 2;
                                waitSetId.Message = ex.Message;
                            }
                            this.SendJsonObject(P4_ResetId_Response, waitSetId);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                        }
                        return true;
                    }
                case P4_ResetId_Response:
                    {
                        try
                        {
                            this.WaitHandlePool.SetRun(message.GetBodyString().FromJson<WaitSetId>());
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                        }
                        return true;
                    }
                case P5_Ping_Request://心跳
                    {
                        try
                        {
                            var waitPing = message.GetBodyString().FromJson<WaitPingPackage>();

                            if (this.AllowRoute && waitPing.Route)
                            {
                                if (this.TryRoute(RouteType.Ping, waitPing))
                                {
                                    if (this.TryFindSmtpActor(waitPing.TargetId, out var actor))
                                    {
                                        actor.Send(P5_Ping_Request, byteBlock);
                                        return true;
                                    }
                                    else
                                    {
                                        waitPing.Status = TouchSocketSmtpStatus.ClientNotFind.ToValue();
                                    }
                                }
                                else
                                {
                                    waitPing.Status = TouchSocketSmtpStatus.RoutingNotAllowed.ToValue();
                                }
                            }
                            else
                            {
                                waitPing.Status = TouchSocketSmtpStatus.Success.ToValue();
                            }
                            waitPing.SwitchId();
                            this.SendJsonObject(P6_Ping_Response, waitPing);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                        }
                        return true;
                    }
                case P6_Ping_Response://心跳
                    {
                        try
                        {
                            var waitPing = message.GetBodyString().FromJson<WaitPingPackage>();

                            if (this.AllowRoute && waitPing.Route)
                            {
                                if (this.TryFindSmtpActor(waitPing.TargetId, out var actor))
                                {
                                    actor.Send(P6_Ping_Response, byteBlock);
                                }
                            }
                            else
                            {
                                this.WaitHandlePool.SetRun(waitPing);
                            }
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                        }
                        return true;
                    }
                case P7_CreateChannel_Request:
                    {
                        try
                        {
                            var waitCreateChannel = new WaitCreateChannelPackage();
                            waitCreateChannel.UnpackageRouter(byteBlock);
                            if (this.AllowRoute && waitCreateChannel.Route)
                            {
                                if (this.TryRoute(RouteType.CreateChannel, waitCreateChannel))
                                {
                                    if (this.TryFindSmtpActor(waitCreateChannel.TargetId, out var actor))
                                    {
                                        actor.Send(P7_CreateChannel_Request, byteBlock);
                                        return true;
                                    }
                                    else
                                    {
                                        waitCreateChannel.UnpackageBody(byteBlock);
                                        waitCreateChannel.Status = TouchSocketSmtpStatus.ClientNotFind.ToValue();
                                    }
                                }
                                else
                                {
                                    waitCreateChannel.UnpackageBody(byteBlock);
                                    waitCreateChannel.Status = TouchSocketSmtpStatus.RoutingNotAllowed.ToValue();
                                }
                            }
                            else
                            {
                                waitCreateChannel.UnpackageBody(byteBlock);

                                while (true)
                                {
                                    if (this.RequestCreateChannel(waitCreateChannel.ChannelId, waitCreateChannel.Route ? waitCreateChannel.SourceId : waitCreateChannel.TargetId, waitCreateChannel.Metadata))
                                    {
                                        waitCreateChannel.Status = TouchSocketSmtpStatus.Success.ToValue();
                                        break;
                                    }
                                    else
                                    {
                                        waitCreateChannel.Status = TouchSocketSmtpStatus.ChannelExisted.ToValue();
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
                            waitCreateChannel.Package(byteBlock);
                            this.Send(P8_CreateChannel_Response, byteBlock);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                        }
                        return true;
                    }
                case P8_CreateChannel_Response:
                    {
                        try
                        {
                            var waitCreateChannel = new WaitCreateChannelPackage();
                            waitCreateChannel.UnpackageRouter(byteBlock);
                            if (this.AllowRoute && waitCreateChannel.Route)
                            {
                                if (this.TryFindSmtpActor(waitCreateChannel.TargetId, out var actor))
                                {
                                    actor.Send(P8_CreateChannel_Response, byteBlock);
                                    return true;
                                }
                            }
                            else
                            {
                                waitCreateChannel.UnpackageBody(byteBlock);
                                this.WaitHandlePool.SetRun(waitCreateChannel);
                            }
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                        }
                        return true;
                    }
                case P9_ChannelPackage:
                    {
                        try
                        {
                            var channelPackage = new ChannelPackage();
                            channelPackage.UnpackageRouter(byteBlock);
                            if (this.AllowRoute && channelPackage.Route)
                            {
                                if (this.TryFindSmtpActor(channelPackage.TargetId, out var actor))
                                {
                                    actor.Send(P9_ChannelPackage, byteBlock);
                                }
                                else
                                {
                                    channelPackage.UnpackageBody(byteBlock);
                                    channelPackage.Message = TouchSocketSmtpStatus.ClientNotFind.GetDescription(channelPackage.TargetId);
                                    channelPackage.SwitchId();
                                    channelPackage.RunNow = true;
                                    channelPackage.DataType = ChannelDataType.DisposeOrder;
                                    byteBlock.Reset();
                                    channelPackage.Package(byteBlock);
                                    this.Send(P9_ChannelPackage, byteBlock);
                                }
                            }
                            else
                            {
                                channelPackage.UnpackageBody(byteBlock);
                                this.QueueChannelPackage(channelPackage);
                            }
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Error(this, $"在protocol={message.ProtocolFlags}中发生错误。信息:{ex.Message}");
                        }
                        return true;
                    }
                default:
                    {
                        if (message.ProtocolFlags<20)
                        {
                            return true;
                        }
                        return false;
                    }
            }
        }

        /// <inheritdoc/>
        public bool Ping(int timeout = 5000)
        {
            return this.PrivatePing(default, timeout);
        }

        /// <inheritdoc/>
        public bool Ping(string targetId, int timeout = 5000)
        {
            if (this.AllowRoute && this.TryFindSmtpActor(targetId, out var actor))
            {
                return actor.Ping(timeout);
            }
            return this.PrivatePing(targetId, timeout);
        }

        /// <inheritdoc/>
        public Task<bool> PingAsync(int timeout = 5000)
        {
            return this.PrivatePingAsync(default, timeout);
        }

        /// <inheritdoc/>
        public Task<bool> PingAsync(string targetId, int timeout = 5000)
        {
            if (this.AllowRoute && this.TryFindSmtpActor(targetId, out var actor))
            {
                return actor.PingAsync(timeout);
            }
            else
            {
                return this.PrivatePingAsync(targetId, timeout);
            }
        }

        /// <inheritdoc/>
        public void ResetId(string id)
        {
            var waitSetId = new WaitSetId(this.Id, id);

            var waitData = this.WaitHandlePool.GetWaitData(waitSetId);

            this.SendJsonObject(P3_ResetId_Request, waitSetId);

            switch (waitData.Wait(5000))
            {
                case WaitDataStatus.SetRunning:
                    {
                        if (waitData.WaitResult.Status == 1)
                        {
                            this.OnResetId?.Invoke(this, new WaitSetId(this.Id, id));
                            this.Id = id;
                        }
                        else
                        {
                            throw new Exception(waitData.WaitResult.Message);
                        }

                        break;
                    }
                case WaitDataStatus.Overtime:
                    throw new TimeoutException(TouchSocketSmtpStatus.Overtime.GetDescription());
                case WaitDataStatus.Canceled:
                    break;

                case WaitDataStatus.Disposed:
                default:
                    throw new Exception(TouchSocketSmtpStatus.UnknownError.GetDescription());
            }
        }

        /// <inheritdoc/>
        public async Task ResetIdAsync(string id)
        {
            var waitSetId = new WaitSetId(this.Id, id);

            var waitData = this.WaitHandlePool.GetWaitDataAsync(waitSetId);

            this.SendJsonObject(P3_ResetId_Request, waitSetId);

            switch (await waitData.WaitAsync(5000))
            {
                case WaitDataStatus.SetRunning:
                    {
                        if (waitData.WaitResult.Status == 1)
                        {
                            this.OnResetId?.Invoke(this, new WaitSetId(this.Id, id));
                            this.Id = id;
                        }
                        else
                        {
                            throw new Exception(waitData.WaitResult.Message);
                        }
                        break;
                    }
                case WaitDataStatus.Overtime:
                    throw new TimeoutException(TouchSocketSmtpStatus.Overtime.GetDescription());
                case WaitDataStatus.Canceled:
                    break;

                case WaitDataStatus.Disposed:
                default:
                    throw new Exception(TouchSocketSmtpStatus.UnknownError.GetDescription());
            }
        }

        /// <inheritdoc/>
        public void SendFastObject<T>(ushort protocol, T obj)
        {
            using (var byteBlock = new ByteBlock())
            {
                byteBlock.WriteObject(obj, SerializationType.FastBinary);
                this.Send(protocol, byteBlock);
            }
        }

        /// <inheritdoc/>
        public void SendJsonObject<T>(ushort protocol, T obj)
        {
            using (var byteBlock = new ByteBlock())
            {
                byteBlock.Write(SerializeConvert.JsonSerializeToBytes(obj));
                this.Send(protocol, byteBlock);
            }
        }

        /// <inheritdoc/>
        public void SendPackage(ushort protocol, IPackage package)
        {
            using (var byteBlock = new ByteBlock())
            {
                package.Package(byteBlock);
                this.Send(protocol, byteBlock);
            }
        }

        /// <inheritdoc/>
        public void SendString(ushort protocol, string value)
        {
            var data = Encoding.UTF8.GetBytes(value);
            this.Send(protocol, data, 0, data.Length);
        }

        /// <inheritdoc/>
        public Task SendStringAsync(ushort protocol, string value)
        {
            var data = Encoding.UTF8.GetBytes(value);
            return this.SendAsync(protocol, data, 0, data.Length);
        }

        /// <inheritdoc/>
        public bool TryFindSmtpActor(string targetId, out SmtpActor actor)
        {
            if (targetId == this.Id)
            {
                actor = this;
                return true;
            }
            if (this.OnFindSmtpActor?.Invoke(targetId) is SmtpActor newActor)
            {
                actor = newActor;
                return true;
            }

            actor = default;
            return false;
        }

        /// <inheritdoc/>
        public bool TryRoute(RouteType routerType, RouterPackage routerPackage)
        {
            try
            {
                var args = new PackageRouterEventArgs(routerType, routerPackage);
                this.OnRouting?.Invoke(this, args);
                return args.IsPermitOperation;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public bool TryRoute(RouteType routerType, WaitRouterPackage routerPackage)
        {
            try
            {
                var args = new PackageRouterEventArgs(routerType, routerPackage);
                this.OnRouting?.Invoke(this, args);
                routerPackage.Message = args.Message;
                return args.IsPermitOperation;
            }
            catch
            {
                return false;
            }
        }

        private void PrivateHandshaked(SmtpVerifyEventArgs e)
        {
            this.OnHandshaked?.Invoke(this, e);
        }

        private bool PrivatePing(string targetId, int timeout)
        {
            var waitPing = new WaitPingPackage
            {
                TargetId = targetId,
                SourceId = Id,
                Route = targetId.HasValue()
            };
            var waitData = this.WaitHandlePool.GetWaitData(waitPing);
            try
            {
                this.SendJsonObject(P5_Ping_Request, waitPing);
                switch (waitData.Wait(timeout))
                {
                    case WaitDataStatus.SetRunning:
                        {
                            switch (waitData.WaitResult.Status.ToStatus())
                            {
                                case TouchSocketSmtpStatus.Success: return true;
                                default:
                                    return false;
                            }
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

        private async Task<bool> PrivatePingAsync(string targetId, int timeout)
        {
            var waitPing = new WaitPingPackage
            {
                TargetId = targetId,
                SourceId = Id,
                Route = targetId.HasValue()
            };
            var waitData = this.WaitHandlePool.GetWaitDataAsync(waitPing);
            try
            {
                this.SendJsonObject(P5_Ping_Request, waitPing);
                switch (await waitData.WaitAsync(timeout))
                {
                    case WaitDataStatus.SetRunning:
                        {
                            switch (waitData.WaitResult.Status.ToStatus())
                            {
                                case TouchSocketSmtpStatus.Success: return true;
                                default:
                                    return false;
                            }
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

        #region 重写

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            this.OnClose = null;
            this.OnRouting = null;
            this.OnFindSmtpActor = null;
            this.OnHandshaked = null;
            this.OnHandshaking = null;
            this.OnResetId = null;

            this.OutputSend = null;
            this.WaitHandlePool.SafeDispose();
            this.Close(false, nameof(Dispose));
            base.Dispose(disposing);
        }

        #endregion 重写

        #region 协议同步发送

        /// <inheritdoc/>
        public void Send(ushort protocol, byte[] buffer, int offset, int length)
        {
            var transferBytes = new ArraySegment<byte>[]
           {
            new ArraySegment<byte>(TouchSocketBitConverter.BigEndian.GetBytes(protocol)),
            new ArraySegment<byte>(TouchSocketBitConverter.BigEndian.GetBytes(length)),
            new ArraySegment<byte>(buffer,offset,length)
           };
            this.OutputSend?.Invoke(this, transferBytes);
            this.LastActiveTime = DateTime.Now;
        }

        /// <inheritdoc/>
        public void Send(ushort protocol, ByteBlock byteBlock)
        {
            this.Send(protocol, byteBlock.Buffer, 0, byteBlock.Len);
        }

        #endregion 协议同步发送

        #region 协议异步发送

        /// <inheritdoc/>
        public Task SendAsync(ushort protocol, byte[] buffer, int offset, int length)
        {
            return Task.Run(() =>
            {
                this.Send(protocol, buffer, offset, length);
            });
        }

        #endregion 协议异步发送

        #region ISmtpChannel

        private readonly ConcurrentDictionary<int, InternalChannel> m_userChannels = new ConcurrentDictionary<int, InternalChannel>();

        /// <inheritdoc/>
        public bool ChannelExisted(int id)
        {
            return this.m_userChannels.ContainsKey(id);
        }

        /// <inheritdoc/>
        public ISmtpChannel CreateChannel(Metadata metadata = default)
        {
            return this.PrivateCreateChannel(default, true, 0, metadata);
        }

        /// <inheritdoc/>
        public ISmtpChannel CreateChannel(int id, Metadata metadata = default)
        {
            return this.PrivateCreateChannel(default, false, id, metadata);
        }

        /// <inheritdoc/>
        public ISmtpChannel CreateChannel(string targetId, int id, Metadata metadata = default)
        {
            if (string.IsNullOrEmpty(targetId))
            {
                throw new ArgumentException($"“{nameof(targetId)}”不能为 null 或空。", nameof(targetId));
            }
            if (this.AllowRoute && this.TryFindSmtpActor(targetId, out var actor))
            {
                return actor.CreateChannel(id, metadata);
            }
            else
            {
                return this.PrivateCreateChannel(targetId, false, id, metadata);
            }
        }

        /// <inheritdoc/>
        public ISmtpChannel CreateChannel(string targetId, Metadata metadata = default)
        {
            if (string.IsNullOrEmpty(targetId))
            {
                throw new ArgumentException($"“{nameof(targetId)}”不能为 null 或空。", nameof(targetId));
            }

            if (this.AllowRoute && this.TryFindSmtpActor(targetId, out var actor))
            {
                return actor.CreateChannel(metadata);
            }
            else
            {
                return this.PrivateCreateChannel(targetId, true, 0, metadata);
            }
        }

        /// <inheritdoc/>
        public Task<ISmtpChannel> CreateChannelAsync(Metadata metadata = default)
        {
            return this.PrivateCreateChannelAsync(default, true, 0, metadata);
        }

        /// <inheritdoc/>
        public Task<ISmtpChannel> CreateChannelAsync(int id, Metadata metadata = default)
        {
            return this.PrivateCreateChannelAsync(default, false, id, metadata);
        }

        /// <inheritdoc/>
        public Task<ISmtpChannel> CreateChannelAsync(string targetId, int id, Metadata metadata = default)
        {
            if (string.IsNullOrEmpty(targetId))
            {
                throw new ArgumentException($"“{nameof(targetId)}”不能为 null 或空。", nameof(targetId));
            }
            if (this.AllowRoute && this.TryFindSmtpActor(targetId, out var actor))
            {
                return actor.CreateChannelAsync(id, metadata);
            }
            else
            {
                return this.PrivateCreateChannelAsync(targetId, false, id, metadata);
            }
        }

        /// <inheritdoc/>
        public Task<ISmtpChannel> CreateChannelAsync(string targetId, Metadata metadata = default)
        {
            if (string.IsNullOrEmpty(targetId))
            {
                throw new ArgumentException($"“{nameof(targetId)}”不能为 null 或空。", nameof(targetId));
            }

            if (this.AllowRoute && this.TryFindSmtpActor(targetId, out var actor))
            {
                return actor.CreateChannelAsync(metadata);
            }
            else
            {
                return this.PrivateCreateChannelAsync(targetId, true, 0, metadata);
            }
        }

        /// <inheritdoc/>
        public bool TrySubscribeChannel(int id, out ISmtpChannel channel)
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

        internal void SendChannelPackage(ChannelPackage channelPackage)
        {
            using (var byteBlock = new ByteBlock(channelPackage.GetLen()))
            {
                channelPackage.Package(byteBlock);
                this.Send(P9_ChannelPackage, byteBlock);
            }
        }

        private ISmtpChannel PrivateCreateChannel(string targetId, bool random, int id, Metadata metadata)
        {
            if (random)
            {
                id = new object().GetHashCode();
            }
            else
            {
                if (this.ChannelExisted(id))
                {
                    throw new Exception(TouchSocketSmtpStatus.ChannelExisted.GetDescription(id));
                }
            }

            var byteBlock = new ByteBlock();
            var waitCreateChannel = new WaitCreateChannelPackage()
            {
                Random = random,
                ChannelId = id,
                SourceId = this.Id,
                TargetId = targetId,
                Metadata = metadata,
                Route = targetId.HasValue()
            };
            var waitData = this.WaitHandlePool.GetWaitData(waitCreateChannel);

            try
            {
                waitCreateChannel.Package(byteBlock);
                this.Send(P7_CreateChannel_Request, byteBlock);
                switch (waitData.Wait(10 * 1000))
                {
                    case WaitDataStatus.SetRunning:
                        {
                            var result = (WaitCreateChannelPackage)waitData.WaitResult;
                            switch (result.Status.ToStatus())
                            {
                                case TouchSocketSmtpStatus.Success:
                                    {
                                        var channel = new InternalChannel(this, targetId, result.Metadata);
                                        channel.SetId(result.ChannelId);
                                        channel.SetUsing();
                                        return this.m_userChannels.TryAdd(result.ChannelId, channel)
                                            ? (ISmtpChannel)channel
                                            : throw new Exception(TouchSocketSmtpStatus.UnknownError.GetDescription());
                                    }
                                case TouchSocketSmtpStatus.ClientNotFind:
                                    {
                                        throw new Exception(TouchSocketSmtpStatus.ClientNotFind.GetDescription(targetId));
                                    }
                                case TouchSocketSmtpStatus.RoutingNotAllowed:
                                default:
                                    {
                                        throw new Exception(result.Status.ToStatus().GetDescription(result.Message));
                                    }
                            }
                        }
                    case WaitDataStatus.Overtime:
                        {
                            throw new TimeoutException(TouchSocketSmtpStatus.Overtime.GetDescription());
                        }
                    default:
                        {
                            throw new Exception(TouchSocketSmtpStatus.UnknownError.GetDescription());
                        }
                }
            }
            finally
            {
                this.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        private async Task<ISmtpChannel> PrivateCreateChannelAsync(string targetId, bool random, int id, Metadata metadata)
        {
            if (random)
            {
                id = new object().GetHashCode();
            }
            else
            {
                if (this.ChannelExisted(id))
                {
                    throw new Exception(TouchSocketSmtpStatus.ChannelExisted.GetDescription(id));
                }
            }

            var byteBlock = new ByteBlock();
            var waitCreateChannel = new WaitCreateChannelPackage()
            {
                Random = random,
                ChannelId = id,
                SourceId = this.Id,
                TargetId = targetId,
                Metadata = metadata,
                Route = targetId.HasValue()
            };
            var waitData = this.WaitHandlePool.GetWaitDataAsync(waitCreateChannel);

            try
            {
                waitCreateChannel.Package(byteBlock);
                this.Send(P7_CreateChannel_Request, byteBlock);
                switch (await waitData.WaitAsync(10 * 1000))
                {
                    case WaitDataStatus.SetRunning:
                        {
                            var result = (WaitCreateChannelPackage)waitData.WaitResult;
                            switch (result.Status.ToStatus())
                            {
                                case TouchSocketSmtpStatus.Success:
                                    {
                                        var channel = new InternalChannel(this, targetId, result.Metadata);
                                        channel.SetId(result.ChannelId);
                                        channel.SetUsing();
                                        return this.m_userChannels.TryAdd(result.ChannelId, channel)
                                            ? (ISmtpChannel)channel
                                            : throw new Exception(TouchSocketSmtpStatus.UnknownError.GetDescription());
                                    }
                                case TouchSocketSmtpStatus.ClientNotFind:
                                    {
                                        throw new Exception(TouchSocketSmtpStatus.ClientNotFind.GetDescription(targetId));
                                    }
                                case TouchSocketSmtpStatus.RoutingNotAllowed:
                                default:
                                    {
                                        throw new Exception(result.Status.ToStatus().GetDescription(result.Message));
                                    }
                            }
                        }
                    case WaitDataStatus.Overtime:
                        {
                            throw new TimeoutException(TouchSocketSmtpStatus.Overtime.GetDescription());
                        }
                    default:
                        {
                            throw new Exception(TouchSocketSmtpStatus.UnknownError.GetDescription());
                        }
                }
            }
            finally
            {
                this.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        private bool QueueChannelPackage(ChannelPackage channelPackage)
        {
            if (this.m_userChannels.TryGetValue(channelPackage.ChannelId, out var channel))
            {
                channel.ReceivedData(channelPackage);
                return true;
            }

            return false;
        }

        private bool RequestCreateChannel(int id, string targetId, Metadata metadata)
        {
            lock (this.SyncRoot)
            {
                var channel = new InternalChannel(this, targetId, metadata);
                channel.SetId(id);
                if (this.m_userChannels.TryAdd(id, channel))
                {
                    Task.Factory.StartNew(this.ThisRequestCreateChannel, new CreateChannelEventArgs(id, metadata));
                    return true;
                }
                else
                {
                    channel.SafeDispose();
                    return false;
                }
            }
        }

        private void ThisRequestCreateChannel(object state)
        {
            try
            {
                this.OnCreateChannel?.Invoke(this, (CreateChannelEventArgs)state);
            }
            catch
            {
            }
        }

        #endregion ISmtpChannel
    }
}