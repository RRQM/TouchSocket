using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Http;

namespace TouchSocket.Rpc.XmlRpc
{
    /// <summary>
    /// IXmlRpcCallContext
    /// </summary>
    public interface IXmlRpcCallContext:ICallContext
    {
        /// <summary>
        /// XmlRpc的调用字符串。
        /// </summary>
        string XmlString { get; }

        /// <summary>
        /// 表明Http上下文。
        /// </summary>
        HttpContext HttpContext { get; }
    }
}
