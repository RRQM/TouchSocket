using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Http;

namespace TouchSocket.JsonRpc
{
    /// <summary>
    /// IHttpJsonRpcClient
    /// </summary>
    public interface IHttpJsonRpcClient:IJsonRpcClient,IHttpClient
    {
    }
}
