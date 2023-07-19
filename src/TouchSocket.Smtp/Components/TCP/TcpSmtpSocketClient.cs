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
using System.Collections.Generic;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Smtp
{
    /// <summary>
    /// TcpSmtpSocketClient
    /// </summary>
    public partial class TcpSmtpSocketClient : SocketClient, ITcpSmtpSocketClient
    {
        private SmtpActor m_smtpActor;

        /// <summary>
        /// TcpSmtpSocketClient
        /// </summary>
        public TcpSmtpSocketClient()
        {
            this.Protocol = SmtpUtility.SmtpProtocol;
        }

        /// <inheritdoc cref="ISmtpActor.IsHandshaked"/>
        public bool IsHandshaked => this.SmtpActor != null && this.SmtpActor.IsHandshaked;

        /// <inheritdoc/>
        public ISmtpActor SmtpActor { get => this.m_smtpActor; }

        /// <summary>
        /// 验证超时时间,默认为3000ms
        /// </summary>
        public int VerifyTimeout => this.Config.GetValue(SmtpConfigExtension.VerifyTimeoutProperty);

        /// <summary>
        /// 连接令箭
        /// </summary>
        public string VerifyToken => this.Config.GetValue(SmtpConfigExtension.VerifyTokenProperty);

        #region 内部委托绑定

        private void OnSmtpActorClose(SmtpActor actor, string msg)
        {
            base.Close(msg);
        }

        private void OnSmtpActorCreateChannel(SmtpActor actor, CreateChannelEventArgs e)
        {
            this.OnCreateChannel(e);
            if (e.Handled)
            {
                return;
            }

            this.PluginsManager.Raise(nameof(ISmtpCreateChannelPlugin.OnCreateChannel), this, e);
        }

        private void OnSmtpActorHandshaked(SmtpActor actor, SmtpVerifyEventArgs e)
        {
            this.OnHandshaked(e);
            if (e.Handled)
            {
                return;
            }
            if (this.PluginsManager.Enable && this.PluginsManager.Raise(nameof(ISmtpHandshakedPlugin.OnSmtpHandshaked), this, e))
            {
                return;
            }
        }

        private void OnSmtpActorHandshaking(SmtpActor actor, SmtpVerifyEventArgs e)
        {
            if (e.Token == this.VerifyToken)
            {
                e.IsPermitOperation = true;
            }
            else
            {
                e.Message = "Token不受理";
            }

            this.OnHandshaking(e);
            if (e.Handled)
            {
                return;
            }
            this.PluginsManager.Raise(nameof(ISmtpHandshakingPlugin.OnSmtpHandshaking), this, e);
        }

        private void OnSmtpActorRouting(SmtpActor actor, PackageRouterEventArgs e)
        {
            this.OnRouting(e);
            if (e.Handled)
            {
                return;
            }
            if (this.PluginsManager.Enable && this.PluginsManager.Raise(nameof(ISmtpRoutingPlugin.OnSmtpRouting), this, e))
            {
                return;
            }
        }

        #endregion 内部委托绑定

        #region 事件

        /// <summary>
        /// 当创建通道
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnCreateChannel(CreateChannelEventArgs e)
        {
        }

        /// <summary>
        /// 在完成握手连接时
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnHandshaked(SmtpVerifyEventArgs e)
        {
        }

        /// <summary>
        /// 在验证Token时
        /// </summary>
        /// <param name="e">参数</param>
        protected virtual void OnHandshaking(SmtpVerifyEventArgs e)
        {
        }

        /// <summary>
        /// 在需要转发路由包时。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnRouting(PackageRouterEventArgs e)
        {
        }

        #endregion 事件

        /// <inheritdoc/>
        public override void Close(string msg = "")
        {
            this.m_smtpActor.Close(true, msg);
        }

        #region ResetId
        ///<inheritdoc cref="ISmtpActor.ResetId(string)"/>
        public override void ResetId(string id)
        {
            this.m_smtpActor.ResetId(id);
        }

        ///<inheritdoc cref="ISmtpActor.ResetIdAsync(string)"/>
        public Task ResetIdAsync(string newId)
        {
            return this.m_smtpActor.ResetIdAsync(newId);
        }
        #endregion

        internal void SetSmtpActor(SmtpActor actor)
        {
            actor.OnResetId = this.ThisOnResetId;
            actor.OutputSend = this.ThisSmtpActorOutputSend;
            actor.Client = this;
            actor.OnClose = this.OnSmtpActorClose;
            actor.OnRouting = this.OnSmtpActorRouting;
            actor.OnHandshaked = this.OnSmtpActorHandshaked;
            actor.OnHandshaking = this.OnSmtpActorHandshaking;
            actor.OnCreateChannel = this.OnSmtpActorCreateChannel;
            actor.Logger = this.Logger;
            this.m_smtpActor = actor;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            this.SmtpActor.SafeDispose();
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override bool HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            var message = (SmtpMessage)requestInfo;
            if (!this.m_smtpActor.InputReceivedData(message))
            {
                if (this.PluginsManager.Enable)
                {
                    this.PluginsManager.Raise(nameof(ISmtpReceivedPlugin.OnSmtpReceived), this, new SmtpMessageEventArgs(message));
                }
            }
            return false;
        }

        /// <inheritdoc/>
        protected override void OnConnected(ConnectedEventArgs e)
        {
            this.m_smtpActor.Id = this.Id;
            base.OnConnected(e);

            Task.Run(async () =>
            {
                await Task.Delay(this.VerifyTimeout);
                if (!this.IsHandshaked)
                {
                    this.Close("Handshak验证超时");
                }
            });
        }

        /// <inheritdoc/>
        protected override void OnDisconnected(DisconnectEventArgs e)
        {
            this.SmtpActor.Close(false, e.Message);
            base.OnDisconnected(e);
        }

        private void ThisOnResetId(SmtpActor rpcActor, WaitSetId waitSetId)
        {
            this.DirectResetId(waitSetId.NewId);
        }

        private void ThisSmtpActorOutputSend(SmtpActor actor, ArraySegment<byte>[] transferBytes)
        {
            base.Send(transferBytes);
        }

        #region 发送

        /// <summary>
        /// 不允许直接发送
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public override void Send(byte[] buffer, int offset, int length)
        {
            throw new Exception("不允许直接发送，请指定任意大于0的协议，然后发送。");
        }

        /// <summary>
        /// 不允许直接发送
        /// </summary>
        /// <param name="transferBytes"></param>
        public override void Send(IList<ArraySegment<byte>> transferBytes)
        {
            throw new Exception("不允许直接发送，请指定任意大于0的协议，然后发送。");
        }

        /// <summary>
        /// 不允许直接发送
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        public override Task SendAsync(byte[] buffer, int offset, int length)
        {
            throw new Exception("不允许直接发送，请指定任意大于0的协议，然后发送。");
        }

        /// <summary>
        /// 不允许直接发送
        /// </summary>
        /// <param name="transferBytes"></param>
        public override Task SendAsync(IList<ArraySegment<byte>> transferBytes)
        {
            throw new Exception("不允许直接发送，请指定任意大于0的协议，然后发送。");
        }

        #endregion 发送
    }
}