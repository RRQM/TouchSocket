//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp.Rpc
{
    /// <summary>
    /// UDP Rpc解释器
    /// </summary>
    public partial class UdpDmtp : UdpSessionBase, IUdpDmtp
    {
        private readonly Timer m_timer;
        private readonly ConcurrentDictionary<EndPoint, UdpDmtpActor> m_udpRpcActors;

        /// <summary>
        /// 构造函数
        /// </summary>
        public UdpDmtp()
        {
            this.m_timer = new Timer((obj) =>
            {
                this.m_udpRpcActors.Remove((kv) =>
                {
                    if (--kv.Value.Tick < 0)
                    {
                        return true;
                    }
                    return false;
                });
            }, null, 1000 * 30, 1000 * 30);
            this.m_udpRpcActors = new ConcurrentDictionary<EndPoint, UdpDmtpActor>();
        }

        public IDmtpActor DmtpActor => throw new NotImplementedException();

        /// <inheritdoc/>
        public bool Ping(int timeout = 5000)
        {
            return this.GetUdpRpcActor().Ping(timeout);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            this.m_timer.SafeDispose();
            this.m_udpRpcActors.Clear();
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override void HandleReceivedData(EndPoint remoteEndPoint, ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            // this.GetUdpRpcActor(remoteEndPoint).InputReceivedData(byteBlock);
        }

        /// <inheritdoc/>
        protected override void LoadConfig(TouchSocketConfig config)
        {
            base.LoadConfig(config);
        }

        private UdpDmtpActor GetUdpRpcActor(EndPoint endPoint)
        {
            if (!this.m_udpRpcActors.TryGetValue(endPoint, out var udpRpcActor))
            {
                udpRpcActor = new UdpDmtpActor(this, endPoint, this.Container.Resolve<ILog>())
                {
                    Client = this,
                };
                this.m_udpRpcActors.TryAdd(endPoint, udpRpcActor);
            }
            udpRpcActor.Tick++;
            return udpRpcActor;
        }

        private UdpDmtpActor GetUdpRpcActor()
        {
            if (this.RemoteIPHost == null)
            {
                throw new ArgumentNullException(nameof(this.RemoteIPHost));
            }
            return this.GetUdpRpcActor(this.RemoteIPHost.EndPoint);
        }
    }
}