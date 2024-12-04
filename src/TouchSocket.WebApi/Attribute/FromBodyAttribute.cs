using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.WebApi
{
    /// <summary>
    /// 指示参数的值应从 HTTP 请求正文中绑定。
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class FromBodyAttribute : Attribute
    {
    }
}
