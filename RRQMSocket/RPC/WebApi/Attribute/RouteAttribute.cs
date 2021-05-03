using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.WebApi
{
    /// <summary>
    /// 适用于WebApi的路由标记
    /// </summary>
    [AttributeUsage(AttributeTargets.Method|AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class RouteAttribute : RPCMethodAttribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public RouteAttribute()
        {
            
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="template"></param>
        public RouteAttribute(string template)
        {
            this.Template = template;
        }
        /// <summary>
        /// 路由模板
        /// </summary>
        public string Template { get;private set; }
    }
}
