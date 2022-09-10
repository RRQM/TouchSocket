//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/rrqm/touchsocket/index
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Collections.Concurrent;
using TouchSocket.Core;
using TouchSocket.Core.ByteManager;
using TouchSocket.Core.Run;
using TouchSocket.Resources;
using TouchSocket.Sockets;

namespace TouchSocket.Rpc.TouchRpc
{
    public partial class RpcActor
    {
        private readonly ConcurrentDictionary<int, Channel> m_userChannels;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool ChannelExisted(int id)
        {
            return this.m_userChannels.ContainsKey(id);
        }

        /// <summary>
        /// 创建通道
        /// </summary>
        /// <returns></returns>
        public Channel CreateChannel()
        {
            lock (this.SyncRoot)
            {
                while (true)
                {
                    int id = new object().GetHashCode();
                    if (!this.ChannelExisted(id))
                    {
                        return this.CreateChannel(id);
                    }
                }
            }
        }

        /// <summary>
        /// 创建通道
        /// </summary>
        /// <param name="id">指定ID</param>
        /// <returns></returns>
        public Channel CreateChannel(int id)
        {
            Channel channel = new Channel(this, null);

            if (this.m_userChannels.TryAdd(id, channel))
            {
                channel.SetID(id);
                this.SocketSend(TouchRpcUtility.P_100_CreateChannel_Request, TouchSocketBitConverter.Default.GetBytes(id));
                channel.SetUsing();
                return channel;
            }
            throw new Exception("指定ID已存在");
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="targetID"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public Channel CreateChannel(string targetID, int id)
        {
            if (string.IsNullOrEmpty(targetID))
            {
                throw new ArgumentException($"“{nameof(targetID)}”不能为 null 或空。", nameof(targetID));
            }

            if (this.ChannelExisted(id))
            {
                throw new Exception("指定ID已存在");
            }

            if (this.m_isService)
            {
                if (this.OnFindRpcActor?.Invoke(targetID) is RpcActor rpcActor)
                {
                    return rpcActor.CreateChannel();
                }
                throw new ClientNotFindException(TouchSocketRes.ClientNotFind.GetDescription());
            }

            ByteBlock byteBlock = new ByteBlock();
            WaitCreateChannel waitCreateChannel = new WaitCreateChannel()
            {
                RandomID = false,
                ChannelID = id,
                ClientID = targetID,
            };
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(waitCreateChannel);

            try
            {
                byteBlock.WriteObject(waitCreateChannel);
                this.SocketSend(TouchRpcUtility.P_110_CreateChannel_2C_Request, byteBlock);
                switch (waitData.Wait(5 * 1000))
                {
                    case WaitDataStatus.SetRunning:
                        {
                            WaitCreateChannel result = (WaitCreateChannel)waitData.WaitResult;
                            if (result.Status == 1)
                            {
                                Channel channel = new Channel(this, result.ClientID);
                                channel.SetID(result.ChannelID);
                                channel.SetUsing();
                                if (this.m_userChannels.TryAdd(result.ChannelID, channel))
                                {
                                    return channel;
                                }
                                throw new Exception(TouchSocketRes.UnknownError.GetDescription());
                            }
                            else
                            {
                                throw new Exception(result.Message);
                            }
                        }
                    case WaitDataStatus.Overtime:
                        {
                            throw new TimeoutException("等待结果超时");
                        }
                    default:
                        {
                            throw new Exception(TouchSocketRes.UnknownError.GetDescription());
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
        /// <param name="targetID"></param>
        /// <returns></returns>
        public Channel CreateChannel(string targetID)
        {
            if (string.IsNullOrEmpty(targetID))
            {
                throw new ArgumentException($"“{nameof(targetID)}”不能为 null 或空。", nameof(targetID));
            }

            if (this.m_isService)
            {
                if (this.OnFindRpcActor?.Invoke(targetID) is RpcActor rpcActor)
                {
                    return rpcActor.CreateChannel();
                }
                throw new ClientNotFindException(TouchSocketRes.ClientNotFind.GetDescription());
            }

            ByteBlock byteBlock = new ByteBlock();
            WaitCreateChannel waitCreateChannel = new WaitCreateChannel()
            {
                RandomID = true,
                ClientID = targetID,
            };
            WaitData<IWaitResult> waitData = this.WaitHandlePool.GetWaitData(waitCreateChannel);

            try
            {
                byteBlock.WriteObject(waitCreateChannel);
                this.SocketSend(TouchRpcUtility.P_110_CreateChannel_2C_Request, byteBlock);
                switch (waitData.Wait(5 * 1000))
                {
                    case WaitDataStatus.SetRunning:
                        {
                            WaitCreateChannel result = (WaitCreateChannel)waitData.WaitResult;
                            if (result.Status == 1)
                            {
                                Channel channel = new Channel(this, result.ClientID);
                                channel.SetID(result.ChannelID);
                                channel.SetUsing();
                                if (this.m_userChannels.TryAdd(result.ChannelID, channel))
                                {
                                    return channel;
                                }
                                throw new Exception(TouchSocketRes.UnknownError.GetDescription());
                            }
                            else
                            {
                                throw new Exception(result.Message);
                            }
                        }
                    case WaitDataStatus.Overtime:
                        {
                            throw new TimeoutException("等待结果超时");
                        }
                    default:
                        {
                            throw new Exception(TouchSocketRes.UnknownError.GetDescription());
                        }
                }
            }
            finally
            {
                this.WaitHandlePool.Destroy(waitData);
                byteBlock.Dispose();
            }
        }

        void IInternalRpc.RemoveChannel(int id)
        {
            this.m_userChannels.TryRemove(id, out _);
        }

        /// <summary>
        /// 订阅通道
        /// </summary>
        /// <param name="id"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        public bool TrySubscribeChannel(int id, out Channel channel)
        {
            if (this.m_userChannels.TryGetValue(id, out channel))
            {
                if (channel.Using)
                {
                    channel = null;
                    return false;
                }
                channel.SetUsing();
                return true;
            }
            return false;
        }

        private void ReceivedChannelData(int id, short type, ByteBlock byteBlock)
        {
            if (this.m_userChannels.TryGetValue(id, out Channel channel))
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
            if (!this.m_userChannels.ContainsKey(id))
            {
                Channel channel = new Channel(this, clientID);
                channel.SetID(id);
                if (!this.m_userChannels.TryAdd(id, channel))
                {
                    channel.Dispose();
                }
            }
        }
    }
}