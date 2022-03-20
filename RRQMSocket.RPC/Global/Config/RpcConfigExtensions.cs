using RRQMCore.Dependency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC
{
    /// <summary>
    /// Rpc配置项
    /// </summary>
    public static class RpcConfigExtensions 
    {
        /// <summary>
        /// 代理文件令箭, 所需类型<see cref="string"/>
        /// </summary>
        public static readonly DependencyProperty ProxyTokenProperty =
            DependencyProperty.Register("ProxyToken", typeof(string), typeof(RpcConfigExtensions), null);
    }
}
