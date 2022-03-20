using RRQMCore.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.RRQMRPC
{
    /// <summary>
    /// Rpc配置扩展
    /// </summary>
    public static class RRQMRPCConfigExtensions
    {
        #region TcpRpcClient

        /// <summary>
        /// 序列化转换器, 所需类型<see cref="RRQMRPC.SerializationSelector"/>
        /// </summary>
        public static readonly DependencyProperty SerializationSelectorProperty =
            DependencyProperty.Register("SerializationSelector", typeof(SerializationSelector), typeof(RRQMRPCConfigExtensions), new DefaultSerializationSelector());


        /// <summary>
        /// 代理文件令箭默认验证令箭。
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static RRQMConfig SetProxyToken(this RRQMConfig config, string value)
        {
            config.SetValue(RpcConfigExtensions.ProxyTokenProperty, value);
            return config;
        }

        /// <summary>
        /// 设置序列化转换器
        /// </summary>
        /// <param name="config"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static RRQMConfig SetSerializationSelector(this RRQMConfig config, SerializationSelector value)
        {
            config.SetValue(SerializationSelectorProperty, value);
            return config;
        }

        #endregion TcpRpcClient

        #region TcpRpcParser
        #endregion TcpRpcParser
    }
}
