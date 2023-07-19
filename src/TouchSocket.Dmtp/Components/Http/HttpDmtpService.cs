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
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// HttpDmtpService
    /// </summary>
    public class HttpDmtpService : HttpDmtpService<HttpDmtpSocketClient>
    {
    }

    /// <summary>
    /// HttpDmtpService泛型类型
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    public partial class HttpDmtpService<TClient> : HttpService<TClient>, IHttpDmtpService where TClient : HttpDmtpSocketClient, new()
    {
        /// <summary>
        /// 连接令箭
        /// </summary>
        public string VerifyToken => this.Config.GetValue(DmtpConfigExtension.VerifyTokenProperty);

        #region 字段

        private bool m_allowRoute;
        private Func<string, IDmtpActor> m_findDmtpActor;

        #endregion 字段

        /// <inheritdoc/>
        protected override void LoadConfig(TouchSocketConfig config)
        {
            base.LoadConfig(config);
            if (this.Container.IsRegistered(typeof(IDmtpRouteService)))
            {
                this.m_allowRoute = true;
                this.m_findDmtpActor = this.Container.Resolve<IDmtpRouteService>().FindDmtpActor;
            }
        }

        /// <inheritdoc/>
        protected override void OnConnected(TClient socketClient, ConnectedEventArgs e)
        {
            socketClient.m_internalOnRpcActorInit = this.PrivateOnRpcActorInit;
            base.OnConnected(socketClient, e);
        }

        private IDmtpActor OnServiceFindDmtpActor(string id)
        {
            return this.TryGetSocketClient(id, out var client) ? client.DmtpActor : null;
        }

        private DmtpActor PrivateOnRpcActorInit()
        {
            return new DmtpActor(this.m_allowRoute)
            {
                OnFindDmtpActor = this.m_allowRoute ? (this.m_findDmtpActor ?? this.OnServiceFindDmtpActor) : null
            };
        }
    }
}