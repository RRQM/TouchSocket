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
using System.Collections.Generic;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// Http服务器辅助类
    /// </summary>
    public class HttpDmtpSocketClient : HttpSocketClient, IHttpDmtpSocketClient
    {
        internal Func<DmtpActor> m_internalOnRpcActorInit;
        private DmtpActor m_smtpActor;

        /// <inheritdoc/>
        public bool IsHandshaked => this.DmtpActor != null && this.DmtpActor.IsHandshaked;

        /// <inheritdoc/>
        public IDmtpActor DmtpActor { get => this.m_smtpActor; }

        /// <summary>
        /// 验证超时时间,默认为3000ms
        /// </summary>
        public int VerifyTimeout => this.Config.GetValue(DmtpConfigExtension.VerifyTimeoutProperty);

        /// <summary>
        /// 连接令箭
        /// </summary>
        public string VerifyToken => this.Config.GetValue(DmtpConfigExtension.VerifyTokenProperty);

        /// <inheritdoc/>
        public override void Close(string msg = "")
        {
            if (this.m_smtpActor == null)
            {
                base.Close(msg);
                return;
            }
            this.m_smtpActor.Close(true, msg);
        }

        #region ResetId

        /// <inheritdoc cref="IDmtpActor.ResetId(string)"/>
        public override void ResetId(string newId)
        {
            if (this.m_smtpActor == null)
            {
                base.ResetId(newId);
                return;
            }
            this.m_smtpActor.ResetId(newId);
        }

        ///<inheritdoc cref="IDmtpActor.ResetIdAsync(string)"/>
        public async Task ResetIdAsync(string newId)
        {
            if (this.m_smtpActor == null)
            {
                base.ResetId(newId);
                return;
            }
            await this.m_smtpActor.ResetIdAsync(newId);
        }

        #endregion ResetId

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            this.DmtpActor?.SafeDispose();
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override bool HandleReceivedData(ByteBlock byteBlock, IRequestInfo requestInfo)
        {
            if (this.Protocol == DmtpUtility.DmtpProtocol && requestInfo is DmtpMessage message)
            {
                if (!this.m_smtpActor.InputReceivedData(message))
                {
                    if (this.PluginsManager.Enable)
                    {
                        this.PluginsManager.Raise(nameof(IDmtpReceivedPlugin.OnDmtpReceived), this, new DmtpMessageEventArgs(message));
                    }
                }

                return false;
            }
            else
            {
                return base.HandleReceivedData(byteBlock, requestInfo);
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDisconnected(DisconnectEventArgs e)
        {
            this.DmtpActor?.Close(false, e.Message);
            base.OnDisconnected(e);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="request"></param>
        protected override void OnReceivedHttpRequest(HttpRequest request)
        {
            if (request.IsMethod(DmtpUtility.Dmtp) && request.IsUpgrade() &&
                string.Equals(request.Headers.Get(HttpHeaders.Upgrade), DmtpUtility.Dmtp, StringComparison.OrdinalIgnoreCase))
            {
                request.SafeDispose();

                this.SetRpcActor(this.m_internalOnRpcActorInit.Invoke());
                this.DefaultSend(new HttpResponse().SetStatus("101", "Switching Protocols").BuildAsBytes());
                return;
            }
            base.OnReceivedHttpRequest(request);
        }

        private void SetRpcActor(DmtpActor actor)
        {
            actor.Id = this.Id;
            actor.OnResetId = this.ThisOnResetId;
            actor.OutputSend = this.ThisDmtpActorOutputSend;
            actor.Client = this;
            actor.OnClose = this.OnDmtpActorClose;
            actor.OnRouting = this.OnDmtpActorRouting;
            actor.OnHandshaked = this.OnDmtpActorHandshaked;
            actor.OnHandshaking = this.OnDmtpActorHandshaking;
            actor.OnCreateChannel = this.OnDmtpActorCreateChannel;
            actor.Logger = this.Logger;
            this.m_smtpActor = actor;

            this.Protocol = DmtpUtility.DmtpProtocol;
            this.SetDataHandlingAdapter(new TcpDmtpAdapter());
        }

        private void ThisOnResetId(DmtpActor actor, WaitSetId waitSetId)
        {
            this.DirectResetId(waitSetId.NewId);
        }

        private void ThisDmtpActorOutputSend(DmtpActor actor, ArraySegment<byte>[] transferBytes)
        {
            base.Send(transferBytes);
        }

        #region 内部委托绑定

        private void OnDmtpActorClose(DmtpActor actor, string msg)
        {
            base.Close(msg);
        }

        private void OnDmtpActorCreateChannel(DmtpActor actor, CreateChannelEventArgs e)
        {
            this.OnCreateChannel(e);
            if (e.Handled)
            {
                return;
            }

            this.PluginsManager.Raise(nameof(IDmtpCreateChannelPlugin.OnCreateChannel), this, e);
        }

        private void OnDmtpActorHandshaked(DmtpActor actor, DmtpVerifyEventArgs e)
        {
            this.OnHandshaked(e);
            if (e.Handled)
            {
                return;
            }
            if (this.PluginsManager.Enable && this.PluginsManager.Raise(nameof(IDmtpHandshakedPlugin.OnDmtpHandshaked), this, e))
            {
                return;
            }
        }

        private void OnDmtpActorHandshaking(DmtpActor actor, DmtpVerifyEventArgs e)
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
            this.PluginsManager.Raise(nameof(IDmtpHandshakingPlugin.OnDmtpHandshaking), this, e);
        }

        private void OnDmtpActorRouting(DmtpActor actor, PackageRouterEventArgs e)
        {
            this.OnRouting(e);
            if (e.Handled)
            {
                return;
            }
            if (this.PluginsManager.Enable && this.PluginsManager.Raise(nameof(IDmtpRoutingPlugin.OnDmtpRouting), this, e))
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
        protected virtual void OnHandshaked(DmtpVerifyEventArgs e)
        {
        }

        /// <summary>
        /// 在验证Token时
        /// </summary>
        /// <param name="e">参数</param>
        protected virtual void OnHandshaking(DmtpVerifyEventArgs e)
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