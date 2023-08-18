using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Http
{
    /// <summary>
    /// IHttpPutPlugin
    /// </summary>
    public interface IHttpPutPlugin<in TClient> : IPlugin where TClient : IHttpSocketClient
    {
        /// <summary>
        /// 在收到Put时
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnHttpPut(TClient client, HttpContextEventArgs e);
    }

    /// <summary>
    /// IHttpPutPlugin
    /// </summary>
    public interface IHttpPutPlugin : IHttpPutPlugin<IHttpSocketClient>
    {

    }
}
