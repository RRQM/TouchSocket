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
    /// IHttpPostPlugin
    /// </summary>
    public interface IHttpPostPlugin<in TClient> : IPlugin where TClient: IHttpSocketClient
    {
        /// <summary>
        /// 在收到Post时
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnHttpPost(TClient client, HttpContextEventArgs e);
    }

    /// <summary>
    /// IHttpPostPlugin
    /// </summary>
    public interface IHttpPostPlugin: IHttpPostPlugin<IHttpSocketClient>
    { 
    
    }
}
