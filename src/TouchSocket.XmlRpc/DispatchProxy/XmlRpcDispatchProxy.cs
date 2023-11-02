#if NET6_0_OR_GREATER || NET481_OR_GREATER
using TouchSocket.Rpc;

namespace TouchSocket.XmlRpc
{
    /// <summary>
    /// XmlRpcDispatchProxy
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <typeparam name="TAttribute"></typeparam>
    public abstract class XmlRpcDispatchProxy<TClient, TAttribute> : RpcDispatchProxy<TClient, TAttribute> where TClient : IXmlRpcClient where TAttribute : XmlRpcAttribute
    {

    }

    /// <summary>
    /// XmlRpcDispatchProxy
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    public abstract class XmlRpcDispatchProxy<TClient> : XmlRpcDispatchProxy<TClient, XmlRpcAttribute> where TClient : IXmlRpcClient
    {

    }

    /// <summary>
    /// XmlRpcDispatchProxy
    /// </summary>
    public abstract class XmlRpcDispatchProxy : XmlRpcDispatchProxy<IXmlRpcClient>
    {

    }
}


#endif
