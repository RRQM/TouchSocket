using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Sockets;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// 基于Tcp协议的JsonRpc客户端。
    /// </summary>
    public interface ITcpJsonRpcClient:IJsonRpcClient,ITcpClient
    {
    }
}
