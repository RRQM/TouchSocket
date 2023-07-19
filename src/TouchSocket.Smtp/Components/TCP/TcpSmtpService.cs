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
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Smtp
{
    /// <summary>
    /// TcpSmtpService
    /// </summary>
    public class TcpSmtpService : TcpSmtpService<TcpSmtpSocketClient>
    {
    }

    /// <summary>
    /// TcpSmtpService泛型类型
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    public partial class TcpSmtpService<TClient> : TcpService<TClient>, ITcpSmtpService where TClient : TcpSmtpSocketClient, new()
    {
        #region 字段
        private bool m_allowRoute;
        private Func<string, ISmtpActor> m_findSmtpActor;
        #endregion 字段

        /// <summary>
        /// 连接令箭
        /// </summary>
        public string VerifyToken => this.Config?.GetValue(SmtpConfigExtension.VerifyTokenProperty);

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
        }

        /// <summary>
        /// 客户端请求连接
        /// </summary>
        /// <param name="socketClient"></param>
        /// <param name="e"></param>
        protected override void OnConnecting(TClient socketClient, ConnectingEventArgs e)
        {
            socketClient.SetSmtpActor(new SmtpActor(this.m_allowRoute)
            {
                Id = e.Id,
                OnFindSmtpActor = this.m_allowRoute ? (this.m_findSmtpActor ?? this.OnServiceFindSmtpActor) : null
            });
            base.OnConnecting(socketClient, e);
        }

        private ISmtpActor OnServiceFindSmtpActor(string id)
        {
            return this.TryGetSocketClient(id, out var client) ? client.SmtpActor : null;
        }
    }
}