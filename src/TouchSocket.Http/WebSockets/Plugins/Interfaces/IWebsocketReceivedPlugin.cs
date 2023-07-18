using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Http.WebSockets
{
    /// <summary>
    /// IWebsocketReceivedPlugin
    /// </summary>
    public interface IWebsocketReceivedPlugin<in TClient> : IPlugin where TClient : IHttpClientBase
    {
        /// <summary>
        /// 当收到WS数据时。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnWebsocketReceived(TClient client, WSDataFrameEventArgs e);

    }

    /// <summary>
    /// IWebsocketReceivedPlugin
    /// </summary>
    public interface IWebsocketReceivedPlugin: IWebsocketReceivedPlugin<IHttpClientBase>
    { 
    
    }
}
