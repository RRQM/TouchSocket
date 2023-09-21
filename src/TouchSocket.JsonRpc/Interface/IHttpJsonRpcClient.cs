using TouchSocket.Http;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// IHttpJsonRpcClient
    /// </summary>
    public interface IHttpJsonRpcClient : IJsonRpcClient, IHttpClient
    {
    }
}