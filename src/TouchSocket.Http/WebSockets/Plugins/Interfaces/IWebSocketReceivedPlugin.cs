using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Http.WebSockets
{
    /// <summary>
    /// IWebSocketReceivedPlugin
    /// </summary>
    public interface IWebSocketReceivedPlugin<in TClient> : IPlugin where TClient : IHttpClientBase
    {
        /// <summary>
        /// 当收到WS数据时。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnWebSocketReceived(TClient client, WSDataFrameEventArgs e);

    }

    /// <summary>
    /// IWebSocketReceivedPlugin
    /// </summary>
    public interface IWebSocketReceivedPlugin : IWebSocketReceivedPlugin<IHttpClientBase>
    {

    }
}
