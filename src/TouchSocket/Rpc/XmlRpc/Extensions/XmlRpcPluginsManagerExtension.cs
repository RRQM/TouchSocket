using TouchSocket.Rpc.XmlRpc;

namespace TouchSocket.Core
{
    /// <summary>
    /// XmlRpcPluginsManagerExtension
    /// </summary>
    public static class XmlRpcPluginsManagerExtension
    {
        /// <summary>
        /// 使用XmlRpc的插件。仅服务器可用。
        /// </summary>
        /// <param name="pluginsManager"></param>
        /// <returns></returns>
        public static XmlRpcParserPlugin UseXmlRpc(this IPluginsManager pluginsManager)
        {
            return pluginsManager.Add<XmlRpcParserPlugin>();
        }
    }
}
