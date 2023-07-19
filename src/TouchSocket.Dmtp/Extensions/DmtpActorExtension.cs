﻿using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// 适用于<see cref="IDmtpActor"/>的扩展。
    /// </summary>
    public static class DmtpActorExtension
    {
        #region Ping

        /// <inheritdoc cref="IDmtpActor.Ping(int)"/>
        public static bool Ping(this IDmtpActorObject client, int timeout = 5000)
        {
            return client.DmtpActor.Ping(timeout);
        }

        /// <inheritdoc cref="IDmtpActor.Ping(string,int)"/>
        public static bool Ping(this IDmtpActorObject client, string targetId, int timeout = 5000)
        {
            return client.DmtpActor.Ping(targetId, timeout);
        }

        /// <inheritdoc cref="IDmtpActor.PingAsync(int)"/>
        public static Task<bool> PingAsync(this IDmtpActorObject client, int timeout = 5000)
        {
            return client.DmtpActor.PingAsync(timeout);
        }

        /// <inheritdoc cref="IDmtpActor.PingAsync(string,int)"/>
        public static Task<bool> PingAsync(this IDmtpActorObject client, string targetId, int timeout = 5000)
        {
            return client.DmtpActor.PingAsync(targetId, timeout);
        }

        #endregion Ping

        #region IDmtpChannel

        public static bool ChannelExisted(this IDmtpActorObject client, int id)
        {
            return client.DmtpActor.ChannelExisted(id);
        }

        public static IDmtpChannel CreateChannel(this IDmtpActorObject client, Metadata metadata = default)
        {
            return client.DmtpActor.CreateChannel(metadata);
        }

        public static IDmtpChannel CreateChannel(this IDmtpActorObject client, int id, Metadata metadata = default)
        {
            return client.DmtpActor.CreateChannel(id,metadata);
        }

        public static IDmtpChannel CreateChannel(this IDmtpActorObject client, string targetId, int id, Metadata metadata = default)
        {
            return client.DmtpActor.CreateChannel(targetId, id,metadata);
        }

        public static IDmtpChannel CreateChannel(this IDmtpActorObject client, string targetId, Metadata metadata = default)
        {
            return client.DmtpActor.CreateChannel(targetId,metadata);
        }

        public static Task<IDmtpChannel> CreateChannelAsync(this IDmtpActorObject client, Metadata metadata = default)
        {
            return client.DmtpActor.CreateChannelAsync(metadata);
        }

        public static Task<IDmtpChannel> CreateChannelAsync(this IDmtpActorObject client, int id, Metadata metadata = default)
        {
            return client.DmtpActor.CreateChannelAsync(id,metadata);
        }

        public static Task<IDmtpChannel> CreateChannelAsync(this IDmtpActorObject client, string targetId, int id, Metadata metadata = default)
        {
            return client.DmtpActor.CreateChannelAsync(targetId, id,metadata);
        }

        public static Task<IDmtpChannel> CreateChannelAsync(this IDmtpActorObject client, string targetId, Metadata metadata = default)
        {
            return client.DmtpActor.CreateChannelAsync(targetId,metadata);
        }

        public static bool TrySubscribeChannel(this IDmtpActorObject client, int id, out IDmtpChannel channel)
        {
            return client.DmtpActor.TrySubscribeChannel(id, out channel);
        }

        #endregion IDmtpChannel

        #region 尝试发送

        /// <summary>
        /// 尝试发送
        /// </summary>
        /// <param name="client"></param>
        /// <param name="protocol"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static bool TrySend(this IDmtpActorObject client, ushort protocol, byte[] buffer, int offset, int length)
        {
            try
            {
                client.DmtpActor.Send(protocol, buffer, offset, length);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 尝试发送
        /// </summary>
        /// <param name="client"></param>
        /// <param name="protocol"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static bool TrySend(this IDmtpActorObject client, ushort protocol, byte[] buffer)
        {
            try
            {
                client.DmtpActor.Send(protocol, buffer, 0, buffer.Length);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 尝试发送
        /// </summary>
        /// <param name="client"></param>
        /// <param name="protocol"></param>
        /// <returns></returns>
        public static bool TrySend(this IDmtpActorObject client, ushort protocol)
        {
            try
            {
                client.DmtpActor.Send(protocol, TouchSocketCoreUtility.ZeroBytes, 0, 0);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 尝试发送
        /// </summary>
        /// <param name="client"></param>
        /// <param name="protocol"></param>
        /// <param name="byteBlock"></param>
        /// <returns></returns>
        public static bool TrySend(this IDmtpActorObject client, ushort protocol, ByteBlock byteBlock)
        {
            try
            {
                client.DmtpActor.Send(protocol, byteBlock);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion 尝试发送

        #region 尝试异步发送

        /// <summary>
        /// 尝试发送
        /// </summary>
        /// <param name="client"></param>
        /// <param name="protocol"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static async Task<bool> TrySendAsync(this IDmtpActorObject client, ushort protocol, byte[] buffer, int offset, int length)
        {
            try
            {
                await client.DmtpActor.SendAsync(protocol, buffer, offset, length);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 尝试发送
        /// </summary>
        /// <param name="client"></param>
        /// <param name="protocol"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static async Task<bool> TrySendAsync(this IDmtpActorObject client, ushort protocol, byte[] buffer)
        {
            try
            {
                await client.DmtpActor.SendAsync(protocol, buffer, 0, buffer.Length);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 尝试发送
        /// </summary>
        /// <param name="client"></param>
        /// <param name="protocol"></param>
        /// <returns></returns>
        public static async Task<bool> TrySendAsync(this IDmtpActorObject client, ushort protocol)
        {
            try
            {
                await client.DmtpActor.SendAsync(protocol, TouchSocketCoreUtility.ZeroBytes, 0, 0);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion 尝试异步发送

        #region 发送Package

        /// <summary>
        /// 发送<see cref="IPackage"/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="protocol">协议</param>
        /// <param name="package">包</param>
        /// <param name="maxSize">估计的包最大值，其作用是用于<see cref="ByteBlock"/>的申请。</param>
        public static void Send(this IDmtpActorObject client, ushort protocol, IPackage package, int maxSize)
        {
            using (var byteBlock = new ByteBlock(maxSize))
            {
                package.Package(byteBlock);
                client.DmtpActor.Send(protocol, byteBlock);
            }
        }

        /// <summary>
        /// 发送估计小于64K的<see cref="IPackage"/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="protocol">协议</param>
        /// <param name="package">包</param>
        public static void Send(this IDmtpActorObject client, ushort protocol, IPackage package)
        {
            Send(client, protocol, package, 1024 * 64);
        }

        /// <summary>
        /// 发送<see cref="IPackage"/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="protocol">协议</param>
        /// <param name="package">包</param>
        /// <param name="maxSize">估计的包最大值，其作用是用于<see cref="ByteBlock"/>的申请。</param>
        public static Task SendAsync(this IDmtpActorObject client, ushort protocol, IPackage package, int maxSize)
        {
            return Task.Run(() =>
            {
                Send(client, protocol, package, maxSize);
            });
        }

        /// <summary>
        /// 发送估计小于64K的<see cref="IPackage"/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="protocol">协议</param>
        /// <param name="package">包</param>
        public static Task SendAsync(this IDmtpActorObject client, ushort protocol, IPackage package)
        {
            return Task.Run(() =>
            {
                Send(client, protocol, package, 1024 * 64);
            });
        }

        #endregion 发送Package

        #region 尝试发送Package

        /// <summary>
        /// 发送<see cref="IPackage"/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="protocol">协议</param>
        /// <param name="package">包</param>
        /// <param name="maxSize">估计的包最大值，其作用是用于<see cref="ByteBlock"/>的申请。</param>
        public static bool TrySend(this IDmtpActorObject client, ushort protocol, IPackage package, int maxSize)
        {
            try
            {
                Send(client, protocol, package, maxSize);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 发送估计小于64K的<see cref="IPackage"/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="protocol">协议</param>
        /// <param name="package">包</param>
        public static bool TrySend(this IDmtpActorObject client, ushort protocol, IPackage package)
        {
            try
            {
                Send(client, protocol, package, 1024 * 64);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 发送<see cref="IPackage"/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="protocol">协议</param>
        /// <param name="package">包</param>
        /// <param name="maxSize">估计的包最大值，其作用是用于<see cref="ByteBlock"/>的申请。</param>
        public static async Task<bool> TrySendAsync(this IDmtpActorObject client, ushort protocol, IPackage package, int maxSize)
        {
            try
            {
                await SendAsync(client, protocol, package, maxSize);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 发送估计小于64K的<see cref="IPackage"/>
        /// </summary>
        /// <param name="client"></param>
        /// <param name="protocol">协议</param>
        /// <param name="package">包</param>
        public static async Task<bool> TrySendAsync(this IDmtpActorObject client, ushort protocol, IPackage package)
        {
            try
            {
                await SendAsync(client, protocol, package, 1024 * 64);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion 尝试发送Package

        #region 发送

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="client"></param>
        /// <param name="protocol"></param>
        /// <param name="buffer"></param>
        public static void Send(this IDmtpActorObject client, ushort protocol, byte[] buffer)
        {
            client.DmtpActor.Send(protocol, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="client"></param>
        /// <param name="protocol"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public static void Send(this IDmtpActorObject client, ushort protocol, byte[] buffer,int offset,int length)
        {
            client.DmtpActor.Send(protocol, buffer, offset, length);
        }

        /// <summary>
        ///  发送
        /// </summary>
        /// <param name="client"></param>
        /// <param name="protocol"></param>
        public static void Send(this IDmtpActorObject client, ushort protocol)
        {
            client.DmtpActor.Send(protocol, TouchSocketCoreUtility.ZeroBytes, 0, 0);
        }

        /// <summary>
        ///  发送
        /// </summary>
        /// <param name="client"></param>
        /// <param name="protocol"></param>
        /// <param name="byteBlock"></param>
        public static void Send(this IDmtpActorObject client, ushort protocol, ByteBlock byteBlock)
        {
            client.DmtpActor.Send(protocol, byteBlock.Buffer, 0, byteBlock.Len);
        }

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="client"></param>
        /// <param name="protocol"></param>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static Task SendAsync(this IDmtpActorObject client, ushort protocol, byte[] buffer)
        {
            return client.DmtpActor.SendAsync(protocol, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="client"></param>
        /// <param name="protocol"></param>
        /// <returns></returns>
        public static Task SendAsync(this IDmtpActorObject client, ushort protocol)
        {
            return client.DmtpActor.SendAsync(protocol, TouchSocketCoreUtility.ZeroBytes, 0, 0);
        }

        #endregion 发送

        /// <summary>
        /// 转为ResultCode
        /// </summary>
        /// <param name="channelStatus"></param>
        /// <returns></returns>
        public static ResultCode ToResultCode(this ChannelStatus channelStatus)
        {
            switch (channelStatus)
            {
                case ChannelStatus.Default:
                    return ResultCode.Default;

                case ChannelStatus.Overtime:
                    return ResultCode.Overtime;

                case ChannelStatus.Cancel:
                    return ResultCode.Canceled;

                case ChannelStatus.Completed:
                    return ResultCode.Success;

                case ChannelStatus.Moving:
                case ChannelStatus.Disposed:
                default:
                    return ResultCode.Error;
            }
        }
    }
}