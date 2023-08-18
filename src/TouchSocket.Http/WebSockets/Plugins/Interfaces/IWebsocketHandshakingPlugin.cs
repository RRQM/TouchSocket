using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Http.WebSockets
{
    /// <summary>
    /// IWebSocketHandshakingPlugin
    /// </summary>
    public interface IWebSocketHandshakingPlugin<in TClient> : IPlugin where TClient : IHttpClientBase
    {
        /// <summary>
        /// 表示在即将握手连接时。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnWebSocketHandshaking(TClient client, HttpContextEventArgs e);
    }

    /// <summary>
    /// IWebSocketHandshakingPlugin
    /// </summary>
    public interface IWebSocketHandshakingPlugin : IWebSocketHandshakingPlugin<IHttpClientBase>
    {

    }
}
