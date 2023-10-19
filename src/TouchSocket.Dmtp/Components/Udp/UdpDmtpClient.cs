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
using System.Net;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp.Rpc
{
    /// <summary>
    /// UdpDmtp终端客户端
    /// </summary>
    internal sealed class UdpDmtpClient : DmtpActor, IUdpDmtpClient
    {
        private readonly EndPoint m_endPoint;
        private readonly UdpSessionBase m_udpSession;
        private IPluginsManager pluginsManager;

        /// <summary>
        /// UdpDmtp终端客户端
        /// </summary>
        /// <param name="udpSession"></param>
        /// <param name="endPoint"></param>
        /// <param name="logger"></param>
        public UdpDmtpClient(UdpSessionBase udpSession, EndPoint endPoint, ILog logger) : base(false, false)
        {
            this.Id = endPoint.ToString();
            this.OutputSend = this.RpcActorSend;
            this.OutputSendAsync = this.RpcActorSendAsync;
            this.CreatedChannel = this.OnDmtpActorCreatedChannel;
            this.m_udpSession = udpSession;
            this.m_endPoint = endPoint;
            this.Logger = logger;
            this.Client = this;
        }

        private Task OnDmtpActorCreatedChannel(DmtpActor actor, CreateChannelEventArgs e)
        {
            return this.pluginsManager.RaiseAsync(nameof(IDmtpCreateChannelPlugin.OnCreateChannel), this, e);
        }

        public bool Created(IPluginsManager pluginsManager)
        {
            this.pluginsManager = pluginsManager;
            var args = new DmtpVerifyEventArgs()
            {
                Id = this.Id,
                IsPermitOperation = true
            };
            pluginsManager.Raise(nameof(IDmtpHandshakingPlugin.OnDmtpHandshaking), this, args);

            if (args.IsPermitOperation == false)
            {
                return false;
            }

            this.IsHandshaked = true;

            args = new DmtpVerifyEventArgs()
            {
                Id = this.Id
            };
            pluginsManager.Raise(nameof(IDmtpHandshakedPlugin.OnDmtpHandshaked), this, args);

            return true;
        }

        /// <inheritdoc/>
        public IDmtpActor DmtpActor => this;

        /// <inheritdoc/>
        public EndPoint EndPoint => this.m_endPoint;

        /// <inheritdoc/>
        public IUdpSession UdpSession => this.m_udpSession;

        /// <summary>
        /// 不支持该操作
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="NotSupportedException">该客户端的Id为实际通信EndPoint值，所以不支持重置Id的操作。</exception>
        public override void ResetId(string id)
        {
            throw new NotSupportedException("该客户端的Id为实际通信EndPoint值，所以不支持重置Id的操作。");
        }

        /// <summary>
        /// 不支持该操作
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">该客户端的Id为实际通信EndPoint值，所以不支持重置Id的操作。</exception>
        public override Task ResetIdAsync(string id)
        {
            throw new NotSupportedException("该客户端的Id为实际通信EndPoint值，所以不支持重置Id的操作。");
        }

        private void RpcActorSend(DmtpActor actor, ArraySegment<byte>[] transferBytes)
        {
            this.m_udpSession.Send(this.m_endPoint, transferBytes);
        }

        private Task RpcActorSendAsync(DmtpActor actor, ArraySegment<byte>[] transferBytes)
        {
            return this.m_udpSession.SendAsync(this.m_endPoint, transferBytes);
        }
    }
}