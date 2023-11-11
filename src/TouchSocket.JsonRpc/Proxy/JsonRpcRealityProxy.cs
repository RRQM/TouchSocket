#if NET45_OR_GREATER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Rpc;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// JsonRpcRealityProxy
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TClient"></typeparam>
    /// <typeparam name="TAttribute"></typeparam>
    public abstract class JsonRpcRealityProxy<T,TClient, TAttribute> : RpcRealityProxy<T,TClient, TAttribute> where TClient : IJsonRpcClient where TAttribute : JsonRpcAttribute
    {
       
    }

    /// <summary>
    /// JsonRpcRealityProxy
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TClient"></typeparam>
    public abstract class JsonRpcRealityProxy<T,TClient> : JsonRpcRealityProxy<T,TClient, JsonRpcAttribute> where TClient : IJsonRpcClient
    {
       
    }

    /// <summary>
    /// JsonRpcRealityProxy
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class JsonRpcRealityProxy<T> : JsonRpcRealityProxy<T,IJsonRpcClient>
    {
       
    }
}

#endif