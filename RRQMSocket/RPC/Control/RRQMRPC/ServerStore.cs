using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// 服务仓库
    /// </summary>
    public class ServerStore
    {
        /// <summary>
        /// 服务键映射
        /// </summary>
        public Dictionary<string,MethodItem> tokenToMethodItem { get;internal set; }
    }
}
