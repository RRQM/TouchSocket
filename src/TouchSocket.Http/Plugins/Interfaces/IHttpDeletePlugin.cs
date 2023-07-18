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
    /// IHttpDeletePlugin
    /// </summary>
    public interface IHttpDeletePlugin<in TClient> : IPlugin where TClient: IHttpSocketClient
    {
        /// <summary>
        /// 在收到Delete时
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnHttpDelete(TClient client, HttpContextEventArgs e);
    }

    /// <summary>
    /// IHttpDeletePlugin
    /// </summary>
    public interface IHttpDeletePlugin: IHttpDeletePlugin<IHttpSocketClient>
    { 
    
    }
}
