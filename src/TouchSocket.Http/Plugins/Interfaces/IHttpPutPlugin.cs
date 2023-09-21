using System;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Http
{
    /// <summary>
    /// IHttpPutPlugin
    /// </summary>
    [Obsolete("该插件已被弃用，请考虑使用“IHttpPlugin”插件代替使用。本插件将在正式版发布时直接移除。", true)]
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
    [Obsolete("该插件已被弃用，请考虑使用“IHttpPlugin”插件代替使用。本插件将在正式版发布时直接移除。", true)]
    public interface IHttpPutPlugin : IHttpPutPlugin<IHttpSocketClient>
    {
    }
}