using TouchSocket.Http.WebSockets;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// IWebSocketJsonRpcClient
    /// </summary>
    public interface IWebSocketJsonRpcClient : IWebSocketClient, IJsonRpcClient
    {
    }
}