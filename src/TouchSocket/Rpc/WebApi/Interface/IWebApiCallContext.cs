using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Http;

namespace TouchSocket.Rpc.WebApi
{
    /// <summary>
    /// IWebApiCallContext
    /// </summary>
    public interface IWebApiCallContext:ICallContext
    {
        /// <summary>
        /// Http上下文
        /// </summary>
        HttpContext HttpContext { get;}
    }
}
