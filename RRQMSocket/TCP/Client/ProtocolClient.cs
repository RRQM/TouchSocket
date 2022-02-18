//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using RRQMCore;
using RRQMCore.ByteManager;
using RRQMCore.Exceptions;
using RRQMCore.Log;
using RRQMCore.Run;
using RRQMCore.Serialization;
using RRQMSocket.Helper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 协议客户端
    /// </summary>
    public abstract class ProtocolClient : TokenClient, IProtocolClient
    {
        private readonly ConcurrentDictionary<short, ProtocolSubscriberCollection> protocolSubscriberCollection;
        private readonly Dictionary<short, string> usedProtocol;
        private readonly ConcurrentDictionary<int, Channel> userChannels;
        private LoopAction heartbeatLoopAction;

        /// <summary>
        /// 构造函数
        /// </summary>
        public ProtocolClient()
        {
            this.usedProtocol = new Dictionary<short, string>();
            this.protocolSubscriberCollection = new ConcurrentDictionary<short, ProtocolSubscriberCollection>();
            this.userChannels = new ConcurrentDictionary<int, Channel>();
        }

        /// <summary>
        /// 添加协议订阅
        /// </summary>
        /// <param name="subscriber"></param>
        public void AddProtocolSubscriber(SubscriberBase subscriber)
        {
            if (subscriber.client != null)
            {
                throw new RRQMException($"该实例已订阅其他服务");
            }
            if (subscriber.Protocol > 0)
            {
                if (this.usedProtocol.ContainsKey(subscriber.Protocol))
                {
                    throw new RRQMException($"该协议已被类协议使用，描述为：{this.usedProtocol[subscriber.Protocol]}");
                }
                else
                {
                    ProtocolSubscriberCollection protocolSubscribers = this.protocolSubscriberCollection.GetOrAdd(subscriber.Protocol, (p) =>
                      {
                          return new ProtocolSubscriberCollection();
                      });

                    subscriber.client = this;
                    protocolSubscribers.Add(subscriber);
                }
            }
            else
            {
                throw new RRQMException("协议不能小于0");
            }
        }

        /// <summary>
        /// 创建通道
        /// </summary>
        /// <returns></returns>
        public Channel CreateChannel()
        {
            while (true)
            {
                Channel channel = new Channel(this, null) { @using = true };
                if (this.userChannels.TryAdd(channel.GetHashCode(), channel))
                {
                    channel.id = channel.GetHashCode();
                    this.SocketSend(-2, RRQMBitConverter.Default.GetBytes(channel.GetHashCode()));
                    return channel;
                }
            }
        }

        internal void RemoveChannel(int id)
        {
            this.userChannels.TryRemove(id, out _);
        }

        /// <summary>
        /// 创建通道
        /// </summary>
        /// <param name="id">指定ID</param>
        /// <returns></returns>
        public Channel CreateChannel(int id)
        {
            Channel channel = new Channel(this, null) { @using = true };
            if (this.userChannels.TryAdd(id, channel))
            {
                channel.id = id;
                this.SocketSend(-2, RRQMBitConverter.Default.GetBytes(id));
                return channel;
            }
            throw new RRQMException("指定ID已存在");
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="clientID"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public Channel CreateChannel(string clientID, int id)
        {
            if (string.IsNullOrEmpty(clientID))
            {
                throw new ArgumentException($"“{nameof(clientID)}”不能为 null 或空。", nameof(clientID));
            }

            if (this.ChannelExisted(id))
            {
                throw new RRQMException("指定ID已存在");
            }

            ByteBlock byteBlock = new ByteBlock();
            WaitCreateChannel waitCreateChannel = new WaitCreateChannel()
            {
                RandomID = false,
                ChannelID = id,
                ClientID = clientID,
            };
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(waitCreateChannel);

            try
            {
                byteBlock.WriteObject(waitCreateChannel);
                this.SocketSend(-12, byteBlock);
                switch (waitData.Wait(5 * 1000))
                {
                    case WaitDataStatus.SetRunning:
                        {
                            WaitCreateChannel result = (WaitCreateChannel)waitData.WaitResult;
                            if (result.Status == 1)
                            {
                                Channel channel = new Channel(this, result.ClientID) { @using = true };
                                channel.id = result.ChannelID;
                                if (this.userChannels.TryAdd(result.ChannelID, channel))
                                {
                                    return channel;
                                }
                                throw new RRQMException(ResType.UnknownError);
                            }
                            else
                            {
                                throw new RRQMException(result.Message);
                            }
                        }
                    case WaitDataStatus.Overtime:
                        {
                            throw new RRQMTimeoutException("等待结果超时");
                        }
                    default:
                        {
                            throw new RRQMException(ResType.UnknownError);
                        }
                }
            }
            finally
            {
                this.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="clientID"></param>
        /// <returns></returns>
        public Channel CreateChannel(string clientID)
        {
            if (string.IsNullOrEmpty(clientID))
            {
                throw new ArgumentException($"“{nameof(clientID)}”不能为 null 或空。", nameof(clientID));
            }

            ByteBlock byteBlock = new ByteBlock();
            WaitCreateChannel waitCreateChannel = new WaitCreateChannel()
            {
                RandomID = true,
                ClientID = clientID,
            };
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(waitCreateChannel);

            try
            {
                byteBlock.WriteObject(waitCreateChannel);
                this.SocketSend(-12, byteBlock);
                switch (waitData.Wait(5 * 1000))
                {
                    case WaitDataStatus.SetRunning:
                        {
                            WaitCreateChannel result = (WaitCreateChannel)waitData.WaitResult;
                            if (result.Status == 1)
                            {
                                Channel channel = new Channel(this, result.ClientID) { @using = true };
                                channel.id = result.ChannelID;
                                if (this.userChannels.TryAdd(result.ChannelID, channel))
                                {
                                    return channel;
                                }
                                throw new RRQMException(ResType.UnknownError);
                            }
                            else
                            {
                                throw new RRQMException(result.Message);
                            }
                        }
                    case WaitDataStatus.Overtime:
                        {
                            throw new RRQMTimeoutException("等待结果超时");
                        }
                    default:
                        {
                            throw new RRQMException(ResType.UnknownError);
                        }
                }
            }
            finally
            {
                this.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
        }

        /// <summary>
        /// 移除协议订阅
        /// </summary>
        /// <param name="subscriber"></param>
        public void RemoveProtocolSubscriber(SubscriberBase subscriber)
        {
            ProtocolSubscriberCollection protocolSubscribers = this.protocolSubscriberCollection.GetOrAdd(subscriber.Protocol, (p) =>
            {
                return new ProtocolSubscriberCollection();
            });
            if (protocolSubscribers.Remove(subscriber))
            {
                subscriber.client = null;
            }
        }

        /// <summary>
        /// 重新设置ID,并且同步到服务器
        /// </summary>
        /// <param name="id"></param>
        public override void ResetID(string id)
        {
            this.ResetID(id, default);
        }

        /// <summary>
        /// 重置ID
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        public void ResetID(string id, CancellationToken cancellationToken)
        {
            WaitSetID waitSetID = new WaitSetID();
            waitSetID.OldID = this.ID;
            waitSetID.NewID = id;

            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(waitSetID);
            waitData.SetCancellationToken(cancellationToken);

            ByteBlock byteBlock = new ByteBlock(1024);
            byteBlock.WriteObject(waitSetID);

            this.SocketSend(0, byteBlock.Buffer, 0, byteBlock.Len);

            switch (waitData.Wait(5000))
            {
                case WaitDataStatus.SetRunning:
                    {
                        if (waitData.WaitResult.Status != 1)
                        {
                            throw new RRQMException(waitData.WaitResult.Message);
                        }
                        break;
                    }
                case WaitDataStatus.Overtime:
                    throw new RRQMException("同步ID超时");
                case WaitDataStatus.Canceled:
                    break;

                case WaitDataStatus.Disposed:
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// 订阅通道
        /// </summary>
        /// <param name="id"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        public bool TrySubscribeChannel(int id, out Channel channel)
        {
            if (this.userChannels.TryGetValue(id, out channel))
            {
                if (channel.@using)
                {
                    channel = null;
                    return false;
                }
                channel.@using = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 添加已被使用的协议
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="describe"></param>
        protected void AddUsedProtocol(short procotol, string describe)
        {
            this.usedProtocol.Add(procotol, describe);
        }

        /// <summary>
        /// 收到协议数据，由于性能考虑，
        /// byteBlock数据源并未剔除协议数据，
        /// 所以真实数据起点为2，
        /// 长度为Length-2。
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="byteBlock"></param>
        protected abstract void HandleProtocolData(short procotol, ByteBlock byteBlock);

        /// <summary>
        /// 流数据处理
        /// </summary>
        /// <param name="args"></param>
        protected abstract void HandleStream(StreamStatusEventArgs args);

        /// <summary>
        /// 密封方法
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="requestInfo"></param>
        protected sealed override void HandleTokenReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            short procotol = RRQMBitConverter.Default.ToInt16(byteBlock.Buffer, 0);
            switch (procotol)
            {
                case 0:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            WaitSetID waitSetID = byteBlock.ReadObject<WaitSetID>();
                            if (waitSetID.Status == 1)
                            {
                                base.ResetID(waitSetID.NewID);
                            }
                            this.WaitHandlePool.SetRun(waitSetID);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Debug(LogType.Error, this, "重置ID错误", ex);
                        }
                        break;
                    }
                case -1:
                    {
                        try
                        {
                            this.HandleProtocolData(-1, byteBlock);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Debug(LogType.Error, this, "处理无协议数据异常", ex);
                        }
                        break;
                    }
                case -2:
                    {
                        try
                        {
                            int id = RRQMBitConverter.Default.ToInt32(byteBlock.Buffer, 2);
                            this.RequestCreateChannel(id, null);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Debug(LogType.Error, this, "创建通道异常", ex);
                        }

                        break;
                    }
                case -3:
                case -4:
                case -5:
                case -6:
                case -10:
                case -11:
                case -14:
                case -15:
                case -16:
                case -17:
                case -18:
                case -19:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            int id = byteBlock.ReadInt32();
                            this.ReceivedChannelData(id, procotol, byteBlock);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Debug(LogType.Error, this, "通道接收异常", ex);
                        }

                        break;
                    }
                case -7://心跳
                    {
                        try
                        {
                            this.OnPong();
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Debug(LogType.Error, this, "在OnPong发送异常。", ex);
                        }
                        break;
                    }
                case -8://StreamStatusToThis
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            WaitStream waitStream = byteBlock.ReadObject<WaitStream>();
                            this.WaitHandlePool.SetRun(waitStream);
                        }
                        catch (Exception ex)
                        {
                            this.logger.Debug(LogType.Error, this, "在StreamStatusToThis中发生错误。", ex);
                        }
                        break;
                    }
                case -9://StreamToThis
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            this.P_9_RequestStreamToThis(byteBlock.ReadObject<WaitStream>(SerializationType.Json));
                        }
                        catch (Exception ex)
                        {
                            this.logger.Debug(LogType.Error, this, "在P_8_RequestStreamToThis中发生错误。", ex);
                        }
                    }
                    break;
                case -12://create channel to client return
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            this.WaitHandlePool.SetRun(byteBlock.ReadObject<WaitCreateChannel>());
                        }
                        catch (Exception ex)
                        {
                            this.logger.Debug(LogType.Error, this, $"在{procotol}中发生错误。", ex);
                        }
                        break;
                    }
                case -13:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            string clientID = byteBlock.ReadString();
                            int id = byteBlock.ReadInt32();
                            this.RequestCreateChannel(id, clientID);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Debug(LogType.Error, this, "创建通道异常", ex);
                        }

                        break;
                    }
                default:
                    {
                        try
                        {
                            if (this.protocolSubscriberCollection.TryGetValue(procotol, out ProtocolSubscriberCollection subscribers))
                            {
                                ProtocolSubscriberEventArgs args = new ProtocolSubscriberEventArgs(byteBlock);
                                foreach (var protocolSubscriber in subscribers)
                                {
                                    try
                                    {
                                        protocolSubscriber.OnInternalReceived(args);
                                        if (args.Handled)
                                        {
                                            return;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        this.Logger.Debug(LogType.Error, this, "处理订阅协议数据异常", ex);
                                    }
                                }
                            }
                            this.HandleProtocolData(procotol, byteBlock);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Debug(LogType.Error, this, "处理协议数据异常", ex);
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="e"></param>
        protected override void OnConnected(MesEventArgs e)
        {
            int heartbeatFrequency = this.ClientConfig.GetValue<int>(ProtocolClientConfig.HeartbeatFrequencyProperty);

            if (heartbeatFrequency > 0)
            {
                if (this.heartbeatLoopAction != null)
                {
                    this.heartbeatLoopAction.Dispose();
                }
                this.heartbeatLoopAction = LoopAction.CreateLoopAction(-1, heartbeatFrequency, (loop) =>
                 {
                     try
                     {
                         this.SocketSend(-7);
                     }
                     catch (Exception)
                     {
                         this.logger.Debug(LogType.Warning, this, "心跳包发送失败。");
                     }
                 });
                this.heartbeatLoopAction.RunAsync();
            }

            base.OnConnected(e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="e"></param>
        protected override void OnConnecting(ClientConnectingEventArgs e)
        {
            if (e.DataHandlingAdapter == null)
            {
                e.DataHandlingAdapter = new FixedHeaderPackageAdapter();
            }
            base.OnConnecting(e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDisconnected(MesEventArgs e)
        {
            base.OnDisconnected(e);
            foreach (var item in this.userChannels.Values)
            {
                item.Dispose();
            }
            if (this.heartbeatLoopAction != null)
            {
                this.heartbeatLoopAction.Dispose();
                this.heartbeatLoopAction = null;
            }
        }

        /// <summary>
        /// 收到服务器回应
        /// </summary>
        protected virtual void OnPong()
        {
        }

        /// <summary>
        /// 预处理流数据
        /// </summary>
        /// <param name="args"></param>
        protected abstract void PreviewHandleStream(StreamOperationEventArgs args);

        private void P_9_RequestStreamToThis(WaitStream waitStream)
        {
            StreamOperator streamOperator = new StreamOperator();
            StreamInfo streamInfo = new StreamInfo(waitStream.Size, waitStream.StreamType);
            StreamOperationEventArgs args = new StreamOperationEventArgs(streamOperator, waitStream.Metadata, streamInfo);

            try
            {
                this.PreviewHandleStream(args);

                if (args.IsPermitOperation)
                {
                    if (args.Bucket != null)
                    {
                        waitStream.Status = 1;
                        Channel channel = this.CreateChannel();
                        waitStream.ChannelID = channel.ID;
                        Task.Run(() =>
                        {
                            Stream stream = args.Bucket;
                            while (channel.MoveNext())
                            {
                                if (streamOperator.Token.IsCancellationRequested)
                                {
                                    channel.Cancel();
                                    streamOperator.SetStreamResult(new Result(ResultCode.Canceled));
                                    break;
                                }
                                ByteBlock block = channel.GetCurrentByteBlock();
                                block.Pos = 6;

                                if (block.TryReadBytesPackageInfo(out int pos, out int len))
                                {
                                    stream.Write(block.Buffer, pos, len);
                                    streamOperator.AddStreamFlow(len, waitStream.Size);
                                }
                                block.SetHolding(false);
                            }
                            this.HandleStream(new StreamStatusEventArgs(streamOperator.SetStreamResult(new Result(channel.Status.ToResultCode())),
                                args.Metadata, args.StreamInfo)
                            { Bucket = stream, Message = args.Message });
                        });
                    }
                    else
                    {
                        streamOperator.SetStreamResult(new Result(ResultCode.Error, ResType.StreamBucketNull.GetResString()));
                        waitStream.Status = 3;
                        waitStream.Message = "未设置流容器";
                    }
                }
                else
                {
                    streamOperator.SetStreamResult(new Result(ResultCode.Error, args.Message));
                    waitStream.Status = 2;
                    waitStream.Message = args.Message;
                }
            }
            catch (Exception ex)
            {
                streamOperator.SetStreamResult(new Result(ResultCode.Error, ex.Message));
                waitStream.Status = 3;
                waitStream.Message = ex.Message;

                this.logger.Debug(LogType.Error, this, $"在{nameof(P_9_RequestStreamToThis)}中发生错误。", ex);
            }

            waitStream.Metadata = null;
            ByteBlock byteBlock = new ByteBlock(1024).WriteObject(waitStream);
            this.SocketSend(-9, byteBlock.Buffer, 0, byteBlock.Pos);
        }

        #region 普通同步发送

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public sealed override void Send(byte[] buffer, int offset, int length)
        {
            this.SocketSend(-1, buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"><inheritdoc/></param>
        public sealed override void Send(byte[] buffer)
        {
            this.Send(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        public sealed override void Send(ByteBlock byteBlock)
        {
            this.Send(byteBlock.Buffer, 0, byteBlock.Len);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"></param>
        public sealed override void Send(IList<TransferByte> transferBytes)
        {
            transferBytes.Insert(0, new TransferByte(RRQMBitConverter.Default.GetBytes(-1)));
            base.Send(transferBytes);
        }

        #endregion 普通同步发送

        #region 普通异步发送

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        public sealed override void SendAsync(byte[] buffer)
        {
            this.SendAsync(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="byteBlock"></param>
        public override void SendAsync(ByteBlock byteBlock)
        {
            this.SendAsync(byteBlock.Buffer, 0, byteBlock.Len);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public sealed override void SendAsync(byte[] buffer, int offset, int length)
        {
            this.SocketSend(-1, buffer, offset, length);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="transferBytes"></param>
        public sealed override void SendAsync(IList<TransferByte> transferBytes)
        {
            transferBytes.Insert(0, new TransferByte(RRQMBitConverter.Default.GetBytes(-1)));
            base.SendAsync(transferBytes);
        }

        #endregion 普通异步发送

        #region 协议同步发送

        /// <summary>
        /// 发送协议状态
        /// </summary>
        /// <param name="procotol"></param>
        public void Send(short procotol)
        {
            this.Send(procotol, new byte[0], 0, 0);
        }

        /// <summary>
        /// 发送字节
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="buffer"></param>
        public void Send(short procotol, byte[] buffer)
        {
            this.Send(procotol, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 发送字节
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void Send(short procotol, byte[] buffer, int offset, int length)
        {
            if (this.usedProtocol.ContainsKey(procotol))
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (var item in this.usedProtocol.Keys)
                {
                    stringBuilder.AppendLine($"协议{item}已被使用，描述为：{this.usedProtocol[item]}");
                }
                throw new RRQMException(stringBuilder.ToString());
            }
            else
            {
                this.InternalSend(procotol, buffer, offset, length);
            }
        }

        /// <summary>
        /// 发送协议流
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="dataByteBlock"></param>
        public void Send(short procotol, ByteBlock dataByteBlock)
        {
            this.Send(procotol, dataByteBlock.Buffer, 0, (int)dataByteBlock.Length);
        }

        #endregion 协议同步发送

        #region 协议异步发送

        /// <summary>
        /// 异步发送协议状态
        /// </summary>
        /// <param name="procotol"></param>
        public void SendAsync(short procotol)
        {
            this.Send(procotol, new byte[0], 0, 0);
        }

        /// <summary>
        /// 异步发送字节
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="buffer"></param>
        public void SendAsync(short procotol, byte[] buffer)
        {
            this.Send(procotol, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 异步发送字节
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public void SendAsync(short procotol, byte[] buffer, int offset, int length)
        {
            if (!this.usedProtocol.ContainsKey(procotol))
            {
                this.InternalSend(procotol, buffer, offset, length);
            }
            else
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (var item in this.usedProtocol.Keys)
                {
                    stringBuilder.AppendLine($"协议{item}已被使用，描述为：{this.usedProtocol[item]}");
                }
                throw new RRQMException(stringBuilder.ToString());
            }
        }

        /// <summary>
        /// 异步发送协议流
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="dataByteBlock"></param>
        public void SendAsync(short procotol, ByteBlock dataByteBlock)
        {
            this.Send(procotol, dataByteBlock.Buffer, 0, dataByteBlock.Len);
        }

        #endregion 协议异步发送

        #region 内部同步发送

        /// <summary>
        /// 内部发送，不会检测用户协议
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        protected void InternalSend(short procotol, byte[] buffer, int offset, int length)
        {
            if (procotol > 0)
            {
                this.SocketSend(procotol, buffer, offset, length);
            }
            else
            {
                throw new RRQMException("小等于0的协议为系统使用协议");
            }
        }

        /// <summary>
        /// 内部发送，不会检测用户协议
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="buffer"></param>
        protected void InternalSend(short procotol, byte[] buffer)
        {
            if (procotol > 0)
            {
                this.SocketSend(procotol, buffer, 0, buffer.Length);
            }
            else
            {
                throw new RRQMException("小等于0的协议为系统使用协议");
            }
        }

        /// <summary>
        /// 内部发送，不会检测用户协议
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="byteBlock"></param>
        protected void InternalSend(short procotol, ByteBlock byteBlock)
        {
            this.InternalSend(procotol, byteBlock.Buffer, 0, byteBlock.Len);
        }

        #endregion 内部同步发送

        #region 内部异步发送

        /// <summary>
        /// 内部发送，不会检测用户协议
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        protected void InternalSendAsync(short procotol, byte[] buffer, int offset, int length)
        {
            if (procotol > 0)
            {
                this.SocketSendAsync(procotol, buffer, offset, length);
            }
            else
            {
                throw new RRQMException("小等于0的协议为系统使用协议");
            }
        }

        /// <summary>
        /// 内部发送，不会检测用户协议
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="buffer"></param>
        protected void InternalSendAsync(short procotol, byte[] buffer)
        {
            if (procotol > 0)
            {
                this.SocketSendAsync(procotol, buffer, 0, buffer.Length);
            }
            else
            {
                throw new RRQMException("小等于0的协议为系统使用协议");
            }
        }

        /// <summary>
        /// 内部发送，不会检测用户协议
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="byteBlock"></param>
        protected void InternalSendAsync(short procotol, ByteBlock byteBlock)
        {
            this.InternalSendAsync(procotol, byteBlock.Buffer, 0, byteBlock.Len);
        }

        #endregion 内部异步发送

        #region Socket同步直发

        internal void SocketSend(short procotol, byte[] dataBuffer)
        {
            this.SocketSend(procotol, dataBuffer, 0, dataBuffer.Length);
        }

        internal void SocketSend(short procotol, ByteBlock byteBlock)
        {
            this.SocketSend(procotol, byteBlock.Buffer, 0, byteBlock.Len);
        }

        internal void SocketSend(short procotol)
        {
            this.SocketSend(procotol, new byte[0], 0, 0);
        }

        internal void SocketSend(short procotol, byte[] dataBuffer, int offset, int length)
        {
            TransferByte[] transferBytes = new TransferByte[]
            {
            new TransferByte(RRQMBitConverter.Default.GetBytes(procotol)),
            new TransferByte(dataBuffer,offset,length)
            };
            base.Send(transferBytes);
        }

        #endregion Socket同步直发

        #region Socket异步直发

        internal void SocketSendAsync(short procotol, byte[] dataBuffer)
        {
            this.SocketSendAsync(procotol, dataBuffer, 0, dataBuffer.Length);
        }

        internal void SocketSendAsync(short procotol)
        {
            this.SocketSendAsync(procotol, new byte[0], 0, 0);
        }

        internal void SocketSendAsync(short procotol, byte[] dataBuffer, int offset, int length)
        {
            TransferByte[] transferBytes = new TransferByte[]
             {
            new TransferByte(RRQMBitConverter.Default.GetBytes(procotol)),
            new TransferByte(dataBuffer,offset,length)
             };
            base.SendAsync(transferBytes);
        }

        #endregion Socket异步直发

        #region Stream发送

        /// <summary>
        /// 发送流数据
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamOperator"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public Result SendStream(Stream stream, StreamOperator streamOperator, Metadata metadata = default)
        {
            WaitStream waitStream = new WaitStream();
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(waitStream);
            waitStream.Metadata = metadata;
            long size = stream.Length - stream.Position;
            waitStream.Size = size;
            waitStream.StreamType = stream.GetType().FullName;
            int length = streamOperator.PackageSize;
            ByteBlock byteBlock = BytePool.GetByteBlock(length).WriteObject(waitStream, SerializationType.Json);
            try
            {
                this.SocketSend(-8, byteBlock.Buffer, 0, byteBlock.Len);

                waitData.SetCancellationToken(streamOperator.Token);

                waitData.Wait(60 * 1000);

                switch (waitData.Status)
                {
                    case WaitDataStatus.SetRunning:
                        {
                            WaitStream waitStreamResult = (WaitStream)waitData.WaitResult;
                            if (waitStreamResult.Status == 1)
                            {
                                if (this.TrySubscribeChannel(waitStreamResult.ChannelID, out Channel channel))
                                {
                                    while (true)
                                    {
                                        if (streamOperator.Token.IsCancellationRequested)
                                        {
                                            channel.Cancel();
                                            return streamOperator.SetStreamResult(new Result(ResultCode.Canceled));
                                        }
                                        int r = stream.Read(byteBlock.Buffer, 0, length);
                                        if (r <= 0)
                                        {
                                            channel.Complete();
                                            return streamOperator.SetStreamResult(new Result(ResultCode.Success));
                                        }

                                        channel.Write(byteBlock.Buffer, 0, r);
                                        streamOperator.AddStreamFlow(r, size);
                                    }
                                }
                                else
                                {
                                    return streamOperator.SetStreamResult(new Result(ResultCode.Error, ResType.SetChannelFail.GetResString()));
                                }
                            }
                            else if (waitStreamResult.Status == 2)
                            {
                                return streamOperator.SetStreamResult(new Result(ResultCode.Error, ResType.RemoteRefuse.GetResString()));
                            }
                            else if (waitStreamResult.Status == 3)
                            {
                                return streamOperator.SetStreamResult(new Result(ResultCode.Error, ResType.RemoteException.GetResString()));
                            }
                            else
                            {
                                return streamOperator.SetStreamResult(new Result(ResultCode.Error));
                            }
                        }
                    case WaitDataStatus.Overtime:
                        {
                            return streamOperator.SetStreamResult(new Result(ResultCode.Overtime));
                        }
                    case WaitDataStatus.Canceled:
                        {
                            return streamOperator.SetStreamResult(new Result(ResultCode.Canceled));
                        }
                    case WaitDataStatus.Disposed:
                    default:
                        {
                            return streamOperator.SetStreamResult(new Result(ResultCode.Error));
                        }
                }
            }
            catch (Exception ex)
            {
                return streamOperator.SetStreamResult(new Result(ResultCode.Error, ex.Message));
            }
            finally
            {
                byteBlock.Dispose();
            }
        }

        /// <summary>
        /// 发送流数据
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamOperator"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public Task<Result> SendStreamAsync(Stream stream, StreamOperator streamOperator, Metadata metadata = default)
        {
            return Task.Run(() =>
             {
                 return this.SendStream(stream, streamOperator, metadata);
             });
        }

        #endregion Stream发送

        private void ReceivedChannelData(int id, short type, ByteBlock byteBlock)
        {
            if (this.userChannels.TryGetValue(id, out Channel channel))
            {
                ChannelData channelData = new ChannelData();
                channelData.type = type;

                byteBlock.SetHolding(true);
                channelData.byteBlock = byteBlock;
                channel.ReceivedData(channelData);
            }
        }

        private void RequestCreateChannel(int id, string clientID)
        {
            if (!this.userChannels.ContainsKey(id))
            {
                Channel channel = new Channel(this, clientID);
                channel.id = id;
                if (!this.userChannels.TryAdd(id, channel))
                {
                    channel.Dispose();
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool ChannelExisted(int id)
        {
            return this.userChannels.ContainsKey(id);
        }
    }
}