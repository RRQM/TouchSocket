using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Rpc.TouchRpc
{
    /// <summary>
    /// TouchRpc辅助类基类
    /// </summary>
    public interface IWSTouchRpcClientBase: ITouchRpc, IPluginObject, ISenderBase
    {
    }
}
