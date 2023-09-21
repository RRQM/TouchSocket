using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.WebApi
{
    /// <summary>
    /// IWebApiPlugin
    /// </summary>
    public interface IWebApiPlugin<in TClient> : IPlugin where TClient : IWebApiClientBase
    {
        /// <summary>
        /// 在请求之前
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnRequest(TClient client, WebApiEventArgs e);

        /// <summary>
        /// 在收到响应之后
        /// </summary>
        /// <param name="client"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        Task OnResponse(TClient client, WebApiEventArgs e);
    }

    /// <summary>
    /// IWebApiPlugin
    /// </summary>
    public interface IWebApiPlugin : IWebApiPlugin<IWebApiClientBase>
    {
    }
}