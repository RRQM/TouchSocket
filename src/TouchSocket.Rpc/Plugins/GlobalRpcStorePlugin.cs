using System;
using TouchSocket.Core;

namespace TouchSocket.Rpc
{
    /// <summary>
    /// 全局Rpc仓库配置插件。
    /// </summary>
    public class GlobalRpcStorePlugin : PluginBase
    {
        private readonly RpcStore m_rpcStore;

        /// <summary>
        /// 全局Rpc仓库配置插件。
        /// </summary>
        /// <param name="container"></param>
        public GlobalRpcStorePlugin(IContainer container)
        {
            if (container.IsRegistered(typeof(RpcStore)))
            {
                this.m_rpcStore = container.Resolve<RpcStore>();
            }
            else
            {
                this.m_rpcStore = new RpcStore(container);
                container.RegisterSingleton<RpcStore>(this.m_rpcStore);
            }
        }

        /// <summary>
        /// 全局配置Rpc服务
        /// </summary>
        /// <param name="action"></param>
        public void ConfigureRpcStore(Action<RpcStore> action)
        {
            action?.Invoke(this.m_rpcStore);
        }
    }
}