//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// 服务集合
    /// </summary>
    [DebuggerDisplay("{Count}")]
    public class ServerProviderCollection : IEnumerable<IServerProvider>, IEnumerable
    {
        /// <summary>
        /// 服务数量
        /// </summary>
        public int Count => this.servers.Count;

        private List<IServerProvider> servers = new List<IServerProvider>();

        internal void Add(IServerProvider serverProvider)
        {
            foreach (var server in this.servers)
            {
                if (serverProvider.GetType().FullName == server.GetType().FullName)
                {
                    throw new RpcException("相同类型的服务已添加");
                }
            }
            this.servers.Add(serverProvider);
        }

        internal void Remove(Type serverType)
        {
            foreach (var server in this.servers)
            {
                if (serverType.FullName == server.GetType().FullName)
                {
                    this.servers.Remove(server);
                    return;
                }
            }
        }

        /// <summary>
        /// 返回枚举
        /// </summary>
        /// <returns></returns>
        IEnumerator<IServerProvider> IEnumerable<IServerProvider>.GetEnumerator()
        {
            return this.servers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.servers.GetEnumerator();
        }
    }
}