using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Http.WebSockets
{
    /// <summary>
    /// IWebsocketClosingPlugin
    /// </summary>
    public interface IWebsocketClosingPlugin<in TClient>:IPlugin where TClient: IHttpClientBase
    {
        /// <summary>
        /// 表示收到断开连接报文。如果对方直接断开连接，此方法则不会触发。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        Task OnWebsocketClosing(TClient client, MsgPermitEventArgs e);
    }

    /// <summary>
    /// IWebsocketClosingPlugin
    /// </summary>
    public interface IWebsocketClosingPlugin: IWebsocketClosingPlugin<IHttpClientBase>
    { 
    
    }
}
