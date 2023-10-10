using TouchSocket.Http.WebSockets;
using TouchSocket.Rpc;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// IWebSocketJsonRpcClient
    /// </summary>
    public interface IWebSocketJsonRpcClient : IWebSocketClient, IJsonRpcClient, IRpcParser
    {
    }
}