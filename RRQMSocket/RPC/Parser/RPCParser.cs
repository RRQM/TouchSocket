using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// RPC解析器
    /// </summary>
    public abstract class RPCParser
    {
        /// <summary>
        /// 序列化转换器
        /// </summary>
        public SerializeConverter SerializeConverter { get; set; }

        internal void RRQMInitializeServers(ServerProviderCollection serverProviders)
        {
            InitializeServers(serverProviders);
        }

        /// <summary>
        /// 初始化服务
        /// </summary>
        /// <param name="serverProviders"></param>
        protected abstract void InitializeServers(ServerProviderCollection serverProviders);
    }
}
