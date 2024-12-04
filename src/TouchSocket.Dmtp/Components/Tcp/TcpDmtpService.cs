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
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// TCP分布式消息传输服务类，继承自TcpDmtpService并实现ITcpDmtpService接口。
    /// 该类提供了基于TCP协议的分布式消息传输服务功能。
    /// </summary>
    public class TcpDmtpService : TcpDmtpService<TcpDmtpSessionClient>, ITcpDmtpService
    {
        /// <inheritdoc/>
        protected override sealed TcpDmtpSessionClient NewClient()
        {
            return new PrivateTcpDmtpSessionClient();
        }

        private class PrivateTcpDmtpSessionClient : TcpDmtpSessionClient
        {
        }
    }

    /// <summary>
    /// 抽象类<see cref="TcpDmtpService{TClient}"/>;为基于TCP协议的Dmtp服务提供基础实现。
    /// 它扩展了<see cref="TcpServiceBase{TClient}"/>;，并实现了<see cref="ITcpDmtpService{TClient}"/>接口。
    /// TClient必须是<see cref="TcpDmtpSessionClient"/>的派生类。
    /// </summary>
    /// <typeparam name="TClient">客户端会话类型，必须继承自<see cref="TcpDmtpSessionClient"/>。</typeparam>
    public abstract class TcpDmtpService<TClient> : TcpServiceBase<TClient>, ITcpDmtpService<TClient> where TClient : TcpDmtpSessionClient
    {
        #region 字段

        private bool m_allowRoute;
        private Func<string, Task<IDmtpActor>> m_findDmtpActor;

        #endregion 字段

        /// <summary>
        /// 连接令箭
        /// </summary>
        public string VerifyToken => this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).VerifyToken;

        /// <inheritdoc/>
        protected override void ClientInitialized(TClient client)
        {
            base.ClientInitialized(client);
            client.InternalSetAction(this.PrivateConnecting);
        }

        /// <inheritdoc/>
        protected override void LoadConfig(TouchSocketConfig config)
        {
            config.SetTcpDataHandlingAdapter(default);
            base.LoadConfig(config);

            var dmtpRouteService = this.Resolver.Resolve<IDmtpRouteService>();
            if (dmtpRouteService != null)
            {
                this.m_allowRoute = true;
                this.m_findDmtpActor = dmtpRouteService.FindDmtpActor;
            }
        }

        private async Task<IDmtpActor> FindDmtpActor(string id)
        {
            if (this.m_allowRoute)
            {
                if (this.m_findDmtpActor != null)
                {
                    return await this.m_findDmtpActor.Invoke(id).ConfigureAwait(false);
                }
                return this.TryGetClient(id, out var client) ? client.DmtpActor : null;
            }
            else
            {
                return null;
            }
        }

        private Task PrivateConnecting(TcpDmtpSessionClient sessionClient, ConnectingEventArgs e)
        {
            sessionClient.InternalSetDmtpActor(new SealedDmtpActor(this.m_allowRoute)
            {
                Id = e.Id,
                FindDmtpActor = this.FindDmtpActor
            });
            return EasyTask.CompletedTask;
        }
    }
}