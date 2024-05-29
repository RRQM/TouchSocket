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

using System.Collections.Generic;
using System.Linq;

namespace TouchSocket.Sockets
{
    public abstract class ConnectableService<TClient> : ConnectableService, IConnectableService<TClient> where TClient : IClient, IIdClient
    {
        /// <inheritdoc/>
        public abstract IClientCollection<TClient> Clients { get; }

        /// <summary>
        /// 客户端实例初始化完成。
        /// </summary>
        /// <returns></returns>
        protected virtual void ClientInitialized(TClient client)
        {
        }

        /// <inheritdoc/>
        protected sealed override IEnumerable<IClient> GetClients()
        {
            return this.Clients.ToList().Cast<IClient>();
        }

        /// <summary>
        /// 创建客户端实例。
        /// </summary>
        /// <returns></returns>
        protected abstract TClient NewClient();
    }
}