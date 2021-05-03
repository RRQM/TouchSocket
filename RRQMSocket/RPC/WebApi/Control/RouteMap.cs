using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.WebApi
{
    /// <summary>
    /// 路由映射图
    /// </summary>
    public class RouteMap
    {
        internal RouteMap()
        {
            this.routeMap = new Dictionary<string, MethodInstance>();
        }
        private Dictionary<string, MethodInstance> routeMap;

        internal void Add(string routeUrl,MethodInstance methodInstance)
        {
            this.routeMap.Add(routeUrl, methodInstance);
        }

        /// <summary>
        /// 通过routeUrl获取函数实例
        /// </summary>
        /// <param name="routeUrl"></param>
        /// <param name="methodInstance"></param>
        /// <returns></returns>
        public bool TryGet(string routeUrl, out MethodInstance methodInstance)
        {
            if (this.routeMap.ContainsKey(routeUrl))
            {
                methodInstance = this.routeMap[routeUrl];
                return true;
            }
            methodInstance = null;
            return false;
        }
    }
}
