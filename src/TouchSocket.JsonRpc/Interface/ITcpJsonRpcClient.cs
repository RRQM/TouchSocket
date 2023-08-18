using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Sockets;

namespace TouchSocket.JsonRpc
{
    public interface ITcpJsonRpcClient:IJsonRpcClient,ITcpClient
    {
    }
}
