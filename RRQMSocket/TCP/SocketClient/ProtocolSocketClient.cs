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
using RRQMCore.ByteManager;
using RRQMCore.Exceptions;
using RRQMCore.Log;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace RRQMSocket
{
    /// <summary>
    /// 协议辅助类
    /// </summary>
    public abstract class ProtocolSocketClient : TokenSocketClient, IProtocolClient
    {
        private static readonly Dictionary<short, string> usedProtocol = new Dictionary<short, string>();
        private readonly ConcurrentDictionary<int, Channel> userChannels = new ConcurrentDictionary<int, Channel>();

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
        /// <inheritdoc/>
        /// </summary>
        protected override void OnInitCompleted()
        {
            if (this.DataHandlingAdapter == null)
            {
                this.SetDataHandlingAdapter(new FixedHeaderDataHandlingAdapter());
            }
        }

        internal void ChangeID(WaitSetID waitSetID)
        {
            ByteBlock block = new ByteBlock(1024).WriteObject(waitSetID);
            this.SocketSend(0, block.Buffer, 0, block.Len);
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
                            try
                            {
                                base.ResetID(waitSetID);
                            }
                            catch (Exception ex)
                            {
                                waitSetID.Status = 2;
                                waitSetID.Message = ex.Message;
                                this.ChangeID(waitSetID);
                            }
                            
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
                            byte[] data = new byte[byteBlock.Len - 2];
                            byteBlock.Position = 2;
                            byteBlock.Read(data);
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
                            byte[] data = byteBlock.ReadBytesPackage();
                            this.ReceivedChannelData(id, -3, data);
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
                    {
                        try
                        {
                            this.OnHeartbeat();
                        }
                        catch (Exception ex)
                        {
                            this.logger.Debug(LogType.Error, this, "在OnHeartbeat中发生错误。", ex);
                        }
                    }
                    break;

                default:
                    {
                        HandleProtocolData(procotol, byteBlock);
                        break;
                    }
            }
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
        }

        /// <summary>
        /// 在收到心跳
        /// </summary>
        protected virtual void OnHeartbeat()
        {
        }

        /// <summary>
        /// 重新设置ID
        /// </summary>
        /// <param name="waitSetID"></param>
        protected override void ResetID(WaitSetID waitSetID)
        {
            this.Service.ResetID(waitSetID);
        }

        private void ReceivedChannelData(int id, short type, byte[] data)
        {
            if (this.userChannels.TryGetValue(id, out Channel channel))
            {
                ChannelData channelData = new ChannelData();
                channelData.type = type;
                channelData.data = data;
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