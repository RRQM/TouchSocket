using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Http.WebSockets
{
    /// <summary>
    /// IWebsocketHandshakedPlugin
    /// </summary>
    public interface IWebsocketHandshakedPlugin<in TClient> : IPlugin where TClient : IHttpClientBase
    {
        /// <summary>
        /// 表示完成握手后。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnWebsocketHandshaked(TClient client, HttpContextEventArgs e);
    }

    /// <summary>
    /// IWebsocketHandshakedPlugin
    /// </summary>
    public interface IWebsocketHandshakedPlugin : IWebsocketHandshakedPlugin<IHttpClientBase>
    { 
    
    }
}
