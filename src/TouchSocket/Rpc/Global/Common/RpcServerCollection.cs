//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在TouchSocket.Core.XREF命名空间的代码）归作者本人若汝棋茗所有
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TouchSocket.Rpc
{
    /// <summary>
    /// 服务集合
    /// </summary>
    [DebuggerDisplay("{Count}")]
    public class RpcServerCollection : IEnumerable<IRpcServer>, IEnumerable
    {
        /// <summary>
        /// 服务数量
        /// </summary>
        public int Count => this.m_servers.Count;

        private readonly ConcurrentDictionary<Type, IRpcServer> m_servers = new ConcurrentDictionary<Type, IRpcServer>();

        /// <summary>
        /// 获取所有的注册类型。
        /// </summary>
        /// <returns></returns>
        public Type[] GetRegisterTypes()
        {
            return this.m_servers.Keys.ToArray();
        }

        internal void Add(Type type, IRpcServer serverProvider)
        {
            foreach (var server in this.m_servers.Keys)
            {
                if (type.FullName == server.GetType().FullName)
                {
                    throw new RpcException("相同类型的服务已添加");
                }
            }
            this.m_servers.TryAdd(type, serverProvider);
        }

        internal void Remove(Type serverType)
        {
            foreach (var server in this.m_servers.Keys)
            {
                if (serverType.FullName == server.FullName)
                {
                    this.m_servers.TryRemove(server, out _);
                    return;
                }
            }
        }

        /// <summary>
        /// 返回枚举
        /// </summary>
        /// <returns></returns>
        IEnumerator<IRpcServer> IEnumerable<IRpcServer>.GetEnumerator()
        {
            return this.m_servers.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.m_servers.Values.GetEnumerator();
        }
    }
}