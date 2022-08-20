using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Http;

namespace TouchSocket.Rpc.WebApi
{
    /// <summary>
    /// 跨域相关设置
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class OriginAttribute : RpcActionFilterAttribute
    {
        /// <summary>
        /// 允许客户端携带验证信息
        /// </summary>
        public bool AllowCredentials { get; set; } = true;

        /// <summary>
        /// 允许跨域的方法。
        /// 默认为“PUT,POST,GET,DELETE,OPTIONS,HEAD,PATCH”
        /// </summary>
        public string AllowMethods { get; set; } = "PUT,POST,GET,DELETE,OPTIONS,HEAD,PATCH";

        /// <summary>
        /// 允许跨域的域名
        /// </summary>
        public string AllowOrigin { get; set; } = "*";

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="callContext"></param>
        /// <param name="invokeResult"></param>
        public override void Executed(ICallContext callContext, ref InvokeResult invokeResult)
        {
            if (callContext is IWebApiCallContext webApiCallContext)
            {
                webApiCallContext.HttpContext.Response.SetHeader("Access-Control-Allow-Origin", this.AllowOrigin);
                webApiCallContext.HttpContext.Response.SetHeader("Access-Control-Allow-Methods", this.AllowMethods);
                webApiCallContext.HttpContext.Response.SetHeader("Access-Control-Allow-Credentials", this.AllowCredentials.ToString().ToLower());
            }
            base.Executed(callContext, ref invokeResult);
        }
    }
}
