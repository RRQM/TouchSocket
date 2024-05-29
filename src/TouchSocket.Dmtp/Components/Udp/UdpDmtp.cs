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
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// UdpDmtpService
    /// </summary>
    public partial class UdpDmtp : UdpSessionBase, IUdpDmtp
    {
        private readonly Timer m_timer;
        private readonly ConcurrentDictionary<EndPoint, UdpDmtpClient> m_udpDmtpClients = new ConcurrentDictionary<EndPoint, UdpDmtpClient>();

        /// <summary>
        /// 构造函数
        /// </summary>
        public UdpDmtp()
        {
            this.m_timer = new Timer((obj) =>
            {
                this.m_udpDmtpClients.RemoveWhen((kv) =>
                {
                    if (DateTime.Now - kv.Value.LastActiveTime > TimeSpan.FromMinutes(1))
                    {
                        return true;
                    }
                    return false;
                });
            }, null, 1000 * 10, 1000 * 10);
        }

        /// <inheritdoc/>
        public IDmtpActor DmtpActor => this.PrivateGetUdpDmtpClientAsync().DmtpActor;

        /// <summary>
        /// 通过终结点获取<see cref="IUdpDmtpClient"/>
        /// </summary>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        public async Task<IUdpDmtpClient> GetUdpDmtpClientAsync(EndPoint endPoint)
        {
            return await this.PrivateGetUdpDmtpClient(endPoint).ConfigureFalseAwait();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            this.m_timer.SafeDispose();
            this.m_udpDmtpClients.Clear();
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override async Task OnUdpReceived(UdpReceivedDataEventArgs e)
        {
            var client =await this.PrivateGetUdpDmtpClient(e.EndPoint).ConfigureFalseAwait();
            if (client == null)
            {
                return;
            }

            var message = DmtpMessage.CreateFrom(e.ByteBlock.Span);
            if (!await client.InputReceivedData(message).ConfigureFalseAwait())
            {
                if (this.PluginManager.Enable)
                {
                    await this.PluginManager.RaiseAsync(typeof(IDmtpReceivedPlugin), client, new DmtpMessageEventArgs(message)).ConfigureFalseAwait();
                }
            }
        }

        /// <inheritdoc/>
        protected override void LoadConfig(TouchSocketConfig config)
        {
            base.LoadConfig(config);
        }

        private async Task<UdpDmtpClient> PrivateGetUdpDmtpClient(EndPoint endPoint)
        {
            if (!this.m_udpDmtpClients.TryGetValue(endPoint, out var udpRpcActor))
            {
                udpRpcActor = new UdpDmtpClient(this, endPoint, this.Resolver.Resolve<ILog>())
                {
                    Client = this,
                };
                if (await udpRpcActor.CreatedAsync(this.PluginManager).ConfigureFalseAwait())
                {
                    this.m_udpDmtpClients.TryAdd(endPoint, udpRpcActor);
                }
            }
            return udpRpcActor;
        }

        private IUdpDmtpClient PrivateGetUdpDmtpClientAsync()
        {
            if (this.RemoteIPHost == null)
            {
                throw new ArgumentNullException(nameof(this.RemoteIPHost));
            }
            return this.PrivateGetUdpDmtpClient(this.RemoteIPHost.EndPoint).GetFalseAwaitResult();
        }

        //internal void InternalSend(EndPoint m_endPoint, ArraySegment<byte>[] transferBytes)
        //{
        //    this.ProtectedSend(m_endPoint, transferBytes);
        //}

        internal Task InternalSendAsync(EndPoint m_endPoint, ReadOnlyMemory<byte> memory)
        {
            return this.ProtectedSendAsync(m_endPoint, memory);
        }
    }
}