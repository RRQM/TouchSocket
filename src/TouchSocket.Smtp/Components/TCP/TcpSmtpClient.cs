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
using System.Threading;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Smtp
{
    /// <summary>
    /// TcpSmtpClient
    /// </summary>
    public partial class TcpSmtpClient : TcpClientBase, ITcpSmtpClient
    {
        /// <summary>
        /// TcpSmtpClient
        /// </summary>
        public TcpSmtpClient()
        {
            this.Protocol = SmtpUtility.SmtpProtocol;
        }
        /// <inheritdoc cref="ISmtpActor.Id"/>
        public string Id => this.SmtpActor.Id;

        #region 字段

        private bool m_allowRoute;
        private Func<string, ISmtpActor> m_findSmtpActor;
        private SmtpActor m_smtpActor;

        #endregion 字段

        /// <inheritdoc cref="ISmtpActor.IsHandshaked"/>
        public bool IsHandshaked => this.SmtpActor != null && this.SmtpActor.IsHandshaked;

        /// <inheritdoc/>
        public ISmtpActor SmtpActor { get => this.m_smtpActor; }

        /// <summary>
        /// 发送<see cref="ISmtpActor"/>关闭消息。
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public override void Close(string msg = "")
        {
            this.SmtpActor.Close(true, msg);
            base.Close(msg);
        }

        /// <summary>
        /// 建立Tcp连接，并且执行握手。
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public override ITcpClient Connect(int timeout = 5000)
        {
            lock (this.SyncRoot)
            {
                if (this.IsHandshaked)
                {
                    return this;
                }
                if (!this.CanSend)
                {
                    base.Connect(timeout);
                }

                this.m_smtpActor.Handshake(this.Config.GetValue(SmtpConfigExtension.VerifyTokenProperty),
                    this.Config.GetValue(SmtpConfigExtension.DefaultIdProperty), timeout, this.Config.GetValue(SmtpConfigExtension.MetadataProperty), CancellationToken.None);
                return this;
            }
        }

        #region ResetId
        ///<inheritdoc cref="ISmtpActor.ResetId(string)"/>
        public void ResetId(string id)
        {
            this.m_smtpActor.ResetId(id);
        }

        ///<inheritdoc cref="ISmtpActor.ResetIdAsync(string)"/>
        public Task ResetIdAsync(string newId)
        {
            return this.m_smtpActor.ResetIdAsync(newId);
        }
        #endregion

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
        protected override void LoadConfig(TouchSocketConfig config)
        {
            config.SetTcpDataHandlingAdapter(() => new TcpSmtpAdapter());
            base.LoadConfig(config);
            if (this.Container.IsRegistered(typeof(ISmtpRouteService)))
            {
                this.m_allowRoute = true;
                this.m_findSmtpActor = this.Container.Resolve<ISmtpRouteService>().FindSmtpActor;
            }
            this.m_smtpActor = new SmtpActor(this.m_allowRoute)
            {
                OutputSend = SmtpActorSend,
                OnRouting = OnSmtpActorRouting,
                OnHandshaking = this.OnSmtpActorHandshaking,
                OnHandshaked = OnSmtpActorHandshaked,
                OnClose = OnSmtpActorClose,
                Logger = this.Logger,
                Client = this,
                OnFindSmtpActor = this.m_findSmtpActor,
                OnCreateChannel = this.OnSmtpActorCreateChannel
            };
        }

        /// <inheritdoc/>
        protected override void OnDisconnected(DisconnectEventArgs e)
        {
            base.OnDisconnected(e);
            this.SmtpActor.Close(false, e.Message);
        }

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
            if (this.PluginsManager.Enable && this.PluginsManager.Raise(nameof(ISmtpHandshakedPlugin.OnSmtpHandshaked), this, e))
            {
                return;
            }
            this.OnHandshaked(e);
        }

        private void OnSmtpActorHandshaking(SmtpActor actor, SmtpVerifyEventArgs e)
        {
            this.OnHandshaking(e);
            if (e.Handled)
            {
                return;
            }
            this.PluginsManager.Raise(nameof(ISmtpHandshakingPlugin.OnSmtpHandshaking), this, e);
        }

        private void OnSmtpActorRouting(SmtpActor actor, PackageRouterEventArgs e)
        {
            if (this.PluginsManager.Enable && this.PluginsManager.Raise(nameof(ISmtpRoutingPlugin.OnSmtpRouting), this, e))
            {
                return;
            }
            this.OnRouting(e);
        }

        private void SmtpActorSend(SmtpActor actor, ArraySegment<byte>[] transferBytes)
        {
            base.Send(transferBytes);
        }

        #endregion 内部委托绑定

        #region 事件触发

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
        /// 即将握手连接时
        /// </summary>
        /// <param name="e">参数</param>
        protected virtual void OnHandshaking(SmtpVerifyEventArgs e)
        {
        }

        /// <summary>
        /// 当需要转发路由包时
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnRouting(PackageRouterEventArgs e)
        {
        }

        #endregion 事件触发
    }
}