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
    /// 定义了UDP数据传输协议（DMTP）的实现类。
    /// 该类继承自UdpSessionBase并实现了IUdpDmtp接口，提供了基于UDP协议的数据传输机制。
    /// </summary>
    public partial class UdpDmtp : UdpSessionBase, IUdpDmtp
    {
        private readonly Timer m_timer;
        private readonly ConcurrentDictionary<EndPoint, UdpDmtpClient> m_udpDmtpClients = new ConcurrentDictionary<EndPoint, UdpDmtpClient>();

        /// <summary>
        /// UdpDmtp类的构造函数。
        /// 初始化一个定时器，用于定期清理不活跃的客户端连接。
        /// </summary>
        public UdpDmtp()
        {
            // 初始化定时器，每隔一段时间执行一次清理任务
            this.m_timer = new Timer((obj) =>
            {
                // 移除超过1分钟未活跃的客户端
                this.m_udpDmtpClients.RemoveWhen((kv) =>
                {
                    // 如果客户端最后一次活跃时间距现在超过1分钟，则认为该客户端不活跃
                    if (DateTime.UtcNow - kv.Value.LastActiveTime > TimeSpan.FromMinutes(1))
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
        /// 异步地通过终结点获取 <see cref="IUdpDmtpClient"/> 实例。
        /// </summary>
        /// <param name="endPoint">指定的终结点，用于创建 <see cref="IUdpDmtpClient"/> 实例。</param>
        /// <returns>返回一个任务，该任务的结果是 <see cref="IUdpDmtpClient"/> 实例。</returns>
        public async Task<IUdpDmtpClient> GetUdpDmtpClientAsync(EndPoint endPoint)
        {
            // 调用内部私有方法来获取 UDP DMTP 客户端实例，且不在调用上下文中等待结果。
            return await this.PrivateGetUdpDmtpClientAsync(endPoint).ConfigureAwait(false);
        }

        internal Task InternalSendAsync(EndPoint m_endPoint, ReadOnlyMemory<byte> memory)
        {
            return this.ProtectedSendAsync(m_endPoint, memory);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            this.m_timer.SafeDispose();
            this.m_udpDmtpClients.Clear();
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override void LoadConfig(TouchSocketConfig config)
        {
            base.LoadConfig(config);
        }

        /// <inheritdoc/>
        protected override async Task OnUdpReceived(UdpReceivedDataEventArgs e)
        {
            var client = await this.PrivateGetUdpDmtpClientAsync(e.EndPoint).ConfigureAwait(false);
            if (client == null)
            {
                return;
            }

            var message = DmtpMessage.CreateFrom(e.ByteBlock.Span);
            if (!await client.InputReceivedData(message).ConfigureAwait(false))
            {
                if (this.PluginManager.Enable)
                {
                    await this.PluginManager.RaiseAsync(typeof(IDmtpReceivedPlugin), this.Resolver, client, new DmtpMessageEventArgs(message)).ConfigureAwait(false);
                }
            }
        }

        private async Task<UdpDmtpClient> PrivateGetUdpDmtpClientAsync(EndPoint endPoint)
        {
            if (!this.m_udpDmtpClients.TryGetValue(endPoint, out var udpRpcActor))
            {
                udpRpcActor = new UdpDmtpClient(this, endPoint, this.Resolver.Resolve<ILog>())
                {
                    Client = this,
                };
                if (await udpRpcActor.CreatedAsync(this.PluginManager).ConfigureAwait(false))
                {
                    this.m_udpDmtpClients.TryAdd(endPoint, udpRpcActor);
                }
            }
            return udpRpcActor;
        }

        private UdpDmtpClient PrivateGetUdpDmtpClientAsync()
        {
            if (this.RemoteIPHost == null)
            {
                throw new ArgumentNullException(nameof(this.RemoteIPHost));
            }
            return this.PrivateGetUdpDmtpClientAsync(this.RemoteIPHost.EndPoint).GetFalseAwaitResult();
        }
    }
}