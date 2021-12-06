using RRQMSocket.RPC.RRQMRPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// 远程服务接口
    /// </summary>
    public interface IRemoteServer
    {
        /// <summary>
        /// 客户端
        /// </summary>
        IRpcClient Client { get; }
    }
}
