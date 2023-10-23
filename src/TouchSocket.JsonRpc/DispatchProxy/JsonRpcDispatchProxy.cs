#if NET6_0_OR_GREATER
using TouchSocket.Rpc;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// JsonRpcDispatchProxy
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <typeparam name="TAttribute"></typeparam>
    public abstract class JsonRpcDispatchProxy<TClient, TAttribute> : RpcDispatchProxy<TClient, TAttribute> where TClient : IJsonRpcClient where TAttribute : JsonRpcAttribute
    {

    }

    /// <summary>
    /// JsonRpcDispatchProxy
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    public abstract class JsonRpcDispatchProxy<TClient> : JsonRpcDispatchProxy<TClient, JsonRpcAttribute> where TClient : IJsonRpcClient
    {

    }

    /// <summary>
    /// JsonRpcDispatchProxy
    /// </summary>
    public abstract class JsonRpcDispatchProxy : JsonRpcDispatchProxy<IJsonRpcClient>
    {

    }
}


#endif
