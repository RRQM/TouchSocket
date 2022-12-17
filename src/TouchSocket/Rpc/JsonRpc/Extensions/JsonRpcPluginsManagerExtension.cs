using TouchSocket.Rpc.JsonRpc;

namespace TouchSocket.Core
{
    /// <summary>
    /// JsonRpcPluginsManagerExtension
    /// </summary>
    public static class JsonRpcPluginsManagerExtension
    {
        /// <summary>
        /// 使用JsonRpc的插件。仅服务器可用。
        /// </summary>
        /// <param name="pluginsManager"></param>
        /// <returns></returns>
        public static JsonRpcParserPlugin UseJsonRpc(this IPluginsManager pluginsManager)
        {
            return pluginsManager.Add<JsonRpcParserPlugin>();
        }
    }
}
