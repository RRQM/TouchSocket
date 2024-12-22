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
using TouchSocket.Http;
using TouchSocket.Sockets;

namespace TouchSocket.Dmtp
{
    /// <summary>
    /// HttpDmtpService 类，继承自<see cref="HttpDmtpService{TClient}"/>，实现<see cref="IHttpDmtpService"/>接口。
    /// 该类提供基于HTTP协议的Dmtp服务，用于处理特定类型的会话客户端。
    /// </summary>
    public class HttpDmtpService : HttpDmtpService<HttpDmtpSessionClient>, IHttpDmtpService
    {
        /// <inheritdoc/>
        protected sealed override HttpDmtpSessionClient NewClient()
        {
            return new PrivateHttpDmtpSessionClient();
        }

        private class PrivateHttpDmtpSessionClient : HttpDmtpSessionClient
        {
        }
    }

    /// <summary>
    /// HttpDmtpService泛型类型
    /// </summary>
    /// <typeparam name="TClient">泛型参数，限定为<see cref="HttpDmtpSessionClient"/>的派生类型</typeparam>
    public abstract partial class HttpDmtpService<TClient> : HttpService<TClient>, IHttpDmtpService<TClient> where TClient : HttpDmtpSessionClient
    {
        /// <summary>
        /// 连接令箭
        /// </summary>
        public string VerifyToken => this.Config.GetValue(DmtpConfigExtension.DmtpOptionProperty).VerifyToken;

        #region 字段

        private bool m_allowRoute;
        private Func<string, Task<IDmtpActor>> m_findDmtpActor;

        #endregion 字段

        /// <inheritdoc/>
        protected override void ClientInitialized(TClient client)
        {
            base.ClientInitialized(client);
            client.m_internalOnRpcActorInit = this.PrivateOnRpcActorInit;
        }

        /// <inheritdoc/>
        protected override void LoadConfig(TouchSocketConfig config)
        {
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

        private SealedDmtpActor PrivateOnRpcActorInit()
        {
            return new SealedDmtpActor(this.m_allowRoute)
            {
                FindDmtpActor = this.FindDmtpActor
            };
        }
    }
}