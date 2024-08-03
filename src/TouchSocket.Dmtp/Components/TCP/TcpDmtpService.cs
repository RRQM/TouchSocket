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
    /// TcpDmtpService
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
    /// TcpDmtpService泛型类型
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
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

            if (this.Resolver.IsRegistered(typeof(IDmtpRouteService)))
            {
                this.m_allowRoute = true;
                this.m_findDmtpActor = this.Resolver.Resolve<IDmtpRouteService>().FindDmtpActor;
            }
        }

        private Task PrivateConnecting(TcpDmtpSessionClient socketClient, ConnectingEventArgs e)
        {
            socketClient.InternalSetDmtpActor(new SealedDmtpActor(this.m_allowRoute)
            {
                Id = e.Id,
                FindDmtpActor = this.FindDmtpActor
            });
            return EasyTask.CompletedTask;
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
    }
}