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
        private static readonly Dictionary<short, string> usedProtocol;
        private readonly ConcurrentDictionary<int, Channel> userChannels;
        private readonly ConcurrentDictionary<short, ProtocolSubscriberCollection> protocolSubscriberCollection;
        private RRQMWaitHandlePool<IWaitResult> waitHandlePool;

        static ProtocolClient()
        {
            usedProtocol = new Dictionary<short, string>();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ProtocolClient()
        {
            this.protocolSubscriberCollection = new ConcurrentDictionary<short, ProtocolSubscriberCollection>();
            this.userChannels = new ConcurrentDictionary<int, Channel>();
            this.waitHandlePool = new RRQMWaitHandlePool<IWaitResult>();
        }

        /// <summary>
        /// 创建通道
        /// </summary>
        /// <returns></returns>
        public Channel CreateChannel()
        {
            while (true)
            {
                Channel channel = new Channel(this);
                if (this.userChannels.TryAdd(channel.GetHashCode(), channel))
                {
                    channel.id = channel.GetHashCode();
                    channel.parent = this.userChannels;
                    this.SocketSend(-2, BitConverter.GetBytes(channel.GetHashCode()));
                    return channel;
                }
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

            WaitData<IWaitResult> waitData = this.waitHandlePool.GetWaitData(waitSetID);
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
            return this.userChannels.TryGetValue(id, out channel);
        }

        /// <summary>
        /// 添加已被使用的协议
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="describe"></param>
        protected static void AddUsedProtocol(short procotol, string describe)
        {
            usedProtocol.Add(procotol, describe);
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
                if (usedProtocol.ContainsKey(subscriber.Protocol))
                {
                    throw new RRQMException($"该协议已被类协议使用，描述为：{usedProtocol[subscriber.Protocol]}");
                }
                else
                {
                    ProtocolSubscriberCollection protocolSubscribers= this.protocolSubscriberCollection.GetOrAdd(subscriber.Protocol,(p)=> 
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
        /// 收到协议数据，由于性能考虑，
        /// byteBlock数据源并未剔除协议数据，
        /// 所以真实数据起点为2，
        /// 长度为Length-2。
        /// </summary>
        /// <param name="procotol"></param>
        /// <param name="byteBlock"></param>
        protected abstract void HandleProtocolData(short? procotol, ByteBlock byteBlock);

        /// <summary>
        /// 密封方法
        /// </summary>
        /// <param name="byteBlock"></param>
        /// <param name="obj"></param>
        protected sealed override void HandleReceivedData(ByteBlock byteBlock, object obj)
        {
            short procotol = BitConverter.ToInt16(byteBlock.Buffer, 0);
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
                            this.waitHandlePool.SetRun(waitSetID);
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
                            HandleProtocolData(null, byteBlock);
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
                            int id = BitConverter.ToInt32(byteBlock.Buffer, 2);
                            this.RequestCreateChannel(id);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Debug(LogType.Error, this, "创建通道异常", ex);
                        }

                        break;
                    }
                case -3:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            int id = byteBlock.ReadInt32();
                            //byte[] data = byteBlock.ReadBytesPackage();
                            this.ReceivedChannelData(id, -3, byteBlock);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Debug(LogType.Error, this, "通道接收异常", ex);
                        }

                        break;
                    }
                case -4:
                case -5:
                case -6:
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            int id = byteBlock.ReadInt32();
                            this.ReceivedChannelData(id, procotol, null);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.Debug(LogType.Error, this, "通道接收异常", ex);
                        }

                        break;
                    }
                case -7://心跳
                    break;

                case -8://StreamStatusToThis
                    {
                        try
                        {
                            byteBlock.Pos = 2;
                            WaitStream waitStream = byteBlock.ReadObject<WaitStream>();
                            this.waitHandlePool.SetRun(waitStream);
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
                            HandleProtocolData(procotol, byteBlock);
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
        /// 流数据处理
        /// </summary>
        /// <param name="args"></param>
        protected abstract void HandleStream(StreamStatusEventArgs args);

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="e"></param>
        protected override void OnConnectedService(MesEventArgs e)
        {
            base.OnConnectedService(e);
            int heartbeatFrequency = this.ClientConfig.GetValue<int>(ProtocolClientConfig.HeartbeatFrequencyProperty);

            if (heartbeatFrequency > 0)
            {
                Thread thread = new Thread(() =>
                {
                    while (true)
                    {
                        if (!this.Online)
                        {
                            break;
                        }
                        try
                        {
                            this.SocketSend(-7);
                        }
                        catch (Exception)
                        {
                            this.logger.Debug(LogType.Warning, this, "心跳包发送失败。");
                        }

                        Thread.Sleep(heartbeatFrequency);
                    }
                });
                thread.Name = "Heartbeat";
                thread.IsBackground = true;
                thread.Start();
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDisconnectedService(MesEventArgs e)
        {
            base.OnDisconnectedService(e);
            foreach (var item in this.userChannels.Values)
            {
                item.Dispose();
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void OnInitCompleted()
        {
            if (this.DataHandlingAdapter == null)
            {
                this.SetDataHandlingAdapter(new FixedHeaderDataHandlingAdapter());
            }
        }
       
        /// <summary>
        /// 预处理流数据
        /// </summary>
        /// <param name="args"></param>
        protected abstract void PreviewHandleStream(StreamOperationEventArgs args);

        private void P_9_RequestStreamToThis(WaitStream waitStream)
        {
            StreamOperator streamOperator = new StreamOperator();
            StreamInfo streamInfo = new StreamInfo(waitStream.Size,waitStream.StreamType);
            StreamOperationEventArgs args = new StreamOperationEventArgs(streamOperator,waitStream.Metadata, streamInfo);

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
                                    break;
                                }
                                ByteBlock block = channel.GetCurrentByteBlock();
                                block.Pos = 6;

                                if (block.TryReadBytesPackageInfo(out int pos, out int len))
                                {
                                    stream.Write(block.Buffer, pos, len);
                                    streamOperator.speedTemp += len;
                                    streamOperator.completedLength += len;
                                    streamOperator.progress = (float)((double)streamOperator.completedLength / waitStream.Size);
                                }
                                block.SetHolding(false);
                            }

                            HandleStream(new StreamStatusEventArgs(channel.Status,args.Metadata, args.StreamInfo) { Bucket = stream, Message = args.Message});
                        });
                    }
                    else
                    {
                        waitStream.Status = 3;
                        waitStream.Message = "未设置流容器";
                    }
                }
                else
                {
                    waitStream.Status = 2;
                    waitStream.Message = args.Message;
                }
            }
            catch (Exception ex)
            {
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
            transferBytes.Insert(0, new TransferByte(BitConverter.GetBytes(-1)));
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
            transferBytes.Insert(0, new TransferByte(BitConverter.GetBytes(-1)));
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
            if (usedProtocol.ContainsKey(procotol))
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (var item in usedProtocol.Keys)
                {
                    stringBuilder.AppendLine($"协议{item}已被使用，描述为：{usedProtocol[item]}");
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
            if (!usedProtocol.ContainsKey(procotol))
            {
                this.InternalSend(procotol, buffer, offset, length);
            }
            else
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (var item in usedProtocol.Keys)
                {
                    stringBuilder.AppendLine($"协议{item}已被使用，描述为：{usedProtocol[item]}");
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

        internal void SocketSend(short procotol)
        {
            this.SocketSend(procotol, new byte[0], 0, 0);
        }

        internal void SocketSend(short procotol, byte[] dataBuffer, int offset, int length)
        {
            TransferByte[] transferBytes = new TransferByte[]
            {
            new TransferByte(BitConverter.GetBytes(procotol)),
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
            new TransferByte(BitConverter.GetBytes(procotol)),
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
        public AsyncResult SendStream(Stream stream, StreamOperator streamOperator, Metadata metadata = default)
        {
            WaitStream waitStream = new WaitStream();
            WaitData<IWaitResult> waitData = this.waitHandlePool.GetWaitData(waitStream);
            waitStream.Metadata = metadata;
            long size = stream.Length - stream.Position;
            waitStream.Size = size;
            waitStream.StreamType = stream.GetType().FullName;
            int length = streamOperator.PackageSize;
            ByteBlock byteBlock = this.BytePool.GetByteBlock(length).WriteObject(waitStream, SerializationType.Json);

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
                                            return new AsyncResult(false, "任务已取消");
                                        }
                                        int r = stream.Read(byteBlock.Buffer, 0, length);
                                        if (r <= 0)
                                        {
                                            channel.Complete();
                                            return new AsyncResult(true, $"成功发送");
                                        }

                                        channel.Write(byteBlock.Buffer, 0, r);

                                        streamOperator.speedTemp += r;
                                        streamOperator.completedLength += r;
                                        streamOperator.progress = (float)((double)streamOperator.completedLength / size);
                                    }

                                }
                                else
                                {
                                    return new AsyncResult(false, $"建立通道失败。");
                                }
                            }
                            else if (waitStreamResult.Status == 2)
                            {
                                return new AsyncResult(false, $"服务器拒绝接收，反馈信息：{waitStreamResult.Message}");
                            }
                            else if (waitStreamResult.Status == 3)
                            {
                                return new AsyncResult(false, $"服务器异常，信息：{waitStreamResult.Message}");
                            }
                            else
                            {
                                return new AsyncResult(false, "未知状态返回");
                            }
                        }
                    case WaitDataStatus.Overtime:
                        {
                            throw new RRQMTimeoutException();
                        }
                    case WaitDataStatus.Canceled:
                        {
                            return new AsyncResult(false, "任务已取消");
                        }
                    case WaitDataStatus.Waiting:
                    case WaitDataStatus.Disposed:
                    default:
                        {
                            return new AsyncResult(false, "未知错误");
                        }
                }
            }
            catch (Exception ex)
            {
                return new AsyncResult(false, ex.Message);
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
        public Task<AsyncResult> SendStreamAsync(Stream stream, StreamOperator streamOperator, Metadata metadata = default)
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

                if (byteBlock != null)
                {
                    byteBlock.SetHolding(true);
                    channelData.byteBlock = byteBlock;
                }
                channel.ReceivedData(channelData);
            }
        }

        private void RequestCreateChannel(int id)
        {
            if (!this.userChannels.ContainsKey(id))
            {
                Channel channel = new Channel(this);
                channel.id = id;
                channel.parent = this.userChannels;
                this.userChannels.TryAdd(id, channel);
            }
        }
    }
}