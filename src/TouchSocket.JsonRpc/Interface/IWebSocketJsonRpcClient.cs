using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Http.WebSockets;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// IWebSocketJsonRpcClient
    /// </summary>
    public interface IWebSocketJsonRpcClient: IWebSocketClient, IJsonRpcClient
    {
    }
}
