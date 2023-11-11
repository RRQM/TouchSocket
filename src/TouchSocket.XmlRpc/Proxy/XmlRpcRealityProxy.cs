#if NET45_OR_GREATER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Rpc;

namespace TouchSocket.XmlRpc
{
    /// <summary>
    /// XmlRpcRealityProxy
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TClient"></typeparam>
    /// <typeparam name="TAttribute"></typeparam>
    public abstract class XmlRpcRealityProxy<T, TClient, TAttribute> : RpcRealityProxy<T, TClient, TAttribute> where TClient : IXmlRpcClient where TAttribute : XmlRpcAttribute
    {

    }

    /// <summary>
    /// XmlRpcRealityProxy
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TClient"></typeparam>
    public abstract class XmlRpcRealityProxy<T, TClient> : XmlRpcRealityProxy<T, TClient, XmlRpcAttribute> where TClient : IXmlRpcClient
    {

    }

    /// <summary>
    /// XmlRpcRealityProxy
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class XmlRpcRealityProxy<T> : XmlRpcRealityProxy<T, IXmlRpcClient>
    {

    }
}

#endif
