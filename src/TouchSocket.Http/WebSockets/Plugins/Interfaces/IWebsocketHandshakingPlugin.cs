using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Http.WebSockets
{
    /// <summary>
    /// IWebsocketHandshakingPlugin
    /// </summary>
    public interface IWebsocketHandshakingPlugin<in TClient> : IPlugin where TClient : IHttpClientBase
    {
        /// <summary>
        /// 表示在即将握手连接时。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnWebsocketHandshaking(TClient client, HttpContextEventArgs e);
    }

    /// <summary>
    /// IWebsocketHandshakingPlugin
    /// </summary>
    public interface IWebsocketHandshakingPlugin: IWebsocketHandshakingPlugin<IHttpClientBase>
    { 
    
    }
}
