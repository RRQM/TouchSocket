using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.WebApi
{
    /// <summary>
    /// 指定参数从查询字符串中获取。
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class FromQueryAttribute : WebApiNameAttribute
    {
    }
}
