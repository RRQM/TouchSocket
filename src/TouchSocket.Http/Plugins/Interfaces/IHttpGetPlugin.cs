using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Sockets;

namespace TouchSocket.Http
{
    /// <summary>
    /// IHttpGetPlugin
    /// </summary>
    public interface IHttpGetPlugin<in TClient>:IPlugin where TClient : IHttpSocketClient
    {
        /// <summary>
        /// 在收到Get时
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnHttpGet(TClient client, HttpContextEventArgs e);
    }

    /// <summary>
    /// IHttpGetPlugin
    /// </summary>
    public interface IHttpGetPlugin: IHttpGetPlugin<IHttpSocketClient>
    { 
    
    }
}
