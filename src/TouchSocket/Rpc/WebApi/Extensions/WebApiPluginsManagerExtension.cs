using TouchSocket.Rpc.WebApi;

namespace TouchSocket.Core
{
    /// <summary>
    /// WebApiPluginsManagerExtension
    /// </summary>
    public static class WebApiPluginsManagerExtension
    {
        /// <summary>
        /// 使用WebApi的插件。仅服务器可用。
        /// </summary>
        /// <param name="pluginsManager"></param>
        /// <returns></returns>
        public static WebApiParserPlugin UseWebApi(this IPluginsManager pluginsManager)
        {
            return pluginsManager.Add<WebApiParserPlugin>();
        }
    }
}
