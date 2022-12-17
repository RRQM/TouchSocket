using TouchSocket.Http;

namespace TouchSocket.Core
{
    /// <summary>
    /// HttpPluginsManagerExtension
    /// </summary>
    public static class HttpPluginsManagerExtension
    {
        /// <summary>
        /// 默认的Http服务。为Http做兜底拦截。该插件应该最后添加。
        /// </summary>
        /// <param name="pluginsManager"></param>
        /// <returns></returns>
        public static DefaultHttpServicePlugin UseDefaultHttpServicePlugin(this IPluginsManager pluginsManager)
        {
            return pluginsManager.Add<DefaultHttpServicePlugin>();
        }
    }
}