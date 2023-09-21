using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Http.WebSockets
{
    /// <summary>
    /// IWebSocketHandshakedPlugin
    /// </summary>
    public interface IWebSocketHandshakedPlugin<in TClient> : IPlugin where TClient : IHttpClientBase
    {
        /// <summary>
        /// 表示完成握手后。
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnWebSocketHandshaked(TClient client, HttpContextEventArgs e);
    }

    /// <summary>
    /// IWebSocketHandshakedPlugin
    /// </summary>
    public interface IWebSocketHandshakedPlugin : IWebSocketHandshakedPlugin<IHttpClientBase>
    {
    }
}