using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Rpc
{
    /// <summary>
    /// 路由描述类
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RpcRouteAttribute : Attribute
    {
        /// <summary>
        /// 路由
        /// </summary>
        public string Route { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="route"></param>
        public RpcRouteAttribute(string route)
        {
            Route = route;
        }
    }
}
