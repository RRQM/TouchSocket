using RRQMCore.Run;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// 等待发现服务
    /// </summary>
    public class DiscoveryServiceWaitResult : WaitResult
    {
        /// <summary>
        /// 代理令箭
        /// </summary>
        public string PT { get; set; }

        /// <summary>
        /// 发现的服务方法。
        /// </summary>
        public MethodItem[] Methods { get; set; }
    }
}
