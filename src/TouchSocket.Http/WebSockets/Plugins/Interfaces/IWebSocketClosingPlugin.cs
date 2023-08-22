using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Http.WebSockets
{
    /// <summary>
    /// IWebSocketClosingPlugin
    /// </summary>
    public interface IWebSocketClosingPlugin<in TClient> : IPlugin where TClient : IHttpClientBase
    {
        /// <summary>
        /// 表示收到断开连接报文。如果对方直接断开连接，此方法则不会触发。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        Task OnWebSocketClosing(TClient client, MsgPermitEventArgs e);
    }

    /// <summary>
    /// IWebSocketClosingPlugin
    /// </summary>
    public interface IWebSocketClosingPlugin : IWebSocketClosingPlugin<IHttpClientBase>
    {

    }
}
