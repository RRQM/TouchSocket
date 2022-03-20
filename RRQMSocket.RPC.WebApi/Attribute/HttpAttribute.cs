using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMSocket.RPC.WebApi
{

    /// <summary>
    /// Http函数
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public abstract class HttpMethodAttribute : RpcAttribute
    {

    }

    /// <summary>
    /// GET访问
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class HttpGetAttribute : HttpMethodAttribute
    {

    }

    /// <summary>
    /// GET访问
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class HttpPostAttribute : HttpMethodAttribute
    {
       
    }
}
