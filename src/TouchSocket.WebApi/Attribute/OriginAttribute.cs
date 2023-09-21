//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：http://rrqm_home.gitee.io/touchsocket/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Threading.Tasks;
using TouchSocket.Rpc;

namespace TouchSocket.WebApi
{
    /// <summary>
    /// 跨域相关设置
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
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

        /// <inheritdoc/>
        public override async Task<InvokeResult> ExecutedAsync(ICallContext callContext, object[] parameters, InvokeResult invokeResult)
        {
            if (callContext is IHttpCallContext httpCallContext && httpCallContext.HttpContext != default)
            {
                httpCallContext.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", this.AllowOrigin);
                httpCallContext.HttpContext.Response.Headers.Add("Access-Control-Allow-Methods", this.AllowMethods);
                httpCallContext.HttpContext.Response.Headers.Add("Access-Control-Allow-Credentials", this.AllowCredentials.ToString().ToLower());
            }
            return await base.ExecutedAsync(callContext, parameters, invokeResult);
        }
    }
}