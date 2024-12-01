using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.WebApi
{
    /// <summary>
    /// 表示 Web API 名称的特性。
    /// </summary>
    public abstract class WebApiNameAttribute : Attribute
    {
        /// <summary>
        /// 获取或设置 Web API 的名称。
        /// </summary>
        public string Name { get; set; }
    }
}
