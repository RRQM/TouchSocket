//------------------------------------------------------------------------------
//  此代码版权归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  源代码仓库：https://gitee.com/RRQM_Home
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// 服务集合
    /// </summary>
    [DebuggerDisplay("Count")]
    public class ServerProviderCollection:IEnumerable
    {
        /// <summary>
        /// 服务数量
        /// </summary>
        public int Count { get { return this.servers.Count; } }

        /// <summary>
        /// 唯一程序集
        /// </summary>
        public Assembly SingleAssembly { get; private set; }
        private List<ServerProvider> servers = new List<ServerProvider>();
        internal void Add(ServerProvider serverProvider)
        {
            if (this.SingleAssembly==null)
            {
                this.SingleAssembly = serverProvider.GetType().Assembly;
            }
            else if (SingleAssembly != serverProvider.GetType().Assembly)
            {
                throw new RRQMRPCException("所有的服务类必须声明在同一程序集内");
            }
            servers.Add(serverProvider);
        }

        /// <summary>
        /// 返回枚举
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return this.servers.GetEnumerator();
        }

        
    }
}
