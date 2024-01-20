using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TouchSocket.Core;
using TouchSocket.Http;
using TouchSocket.Rpc;

namespace TouchSocket.WebApi
{
    /// <summary>
    /// 适用于WebApi的跨域特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
    public sealed class EnableCorsAttribute : RpcActionFilterAttribute
    {
        /// <summary>
        /// 适用于WebApi的跨域特性
        /// </summary>
        /// <param name="policyName">跨域策略名称</param>
        public EnableCorsAttribute(string policyName)
        {
            this.PolicyName = policyName;
        }

        /// <summary>
        /// 跨域策略名称
        /// </summary>
        public string PolicyName { get; set; }

        /// <inheritdoc/>
        public override async Task<InvokeResult> ExecutedAsync(ICallContext callContext, object[] parameters, InvokeResult invokeResult)
        {
            if (callContext is IHttpCallContext httpCallContext && httpCallContext.HttpContext != default)
            {
                var corsService = callContext.Resolver.Resolve<ICorsService>();
                var corsPolicy = corsService.GetPolicy(this.PolicyName) ?? throw new Exception($"没有找到名称为{this.PolicyName}的跨域策略。");

                corsPolicy.Apply(httpCallContext.HttpContext);
            }
            return await base.ExecutedAsync(callContext, parameters, invokeResult);
        }
    }
}