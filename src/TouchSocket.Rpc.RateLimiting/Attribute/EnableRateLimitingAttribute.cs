using System;
using System.Collections.Generic;
using System.Linq;
using TouchSocket.Core;
using System.Threading.Tasks;
using System.Threading.RateLimiting;

namespace TouchSocket.Rpc.RateLimiting
{
    /// <summary>
    /// 使用限流策略
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
    public sealed class EnableRateLimitingAttribute : RpcActionFilterAttribute
    {
        private RateLimitLease m_rateLimitLease;

        /// <summary>
        /// 策略名称
        /// </summary>
        public string PolicyName { get; }

        /// <summary>
        /// 使用限流策略
        /// </summary>
        /// <param name="policyName"></param>
        public EnableRateLimitingAttribute(string policyName)
        {
            this.PolicyName = policyName;
        }

        /// <inheritdoc/>
        public override async Task<InvokeResult> ExecutingAsync(ICallContext callContext, object[] parameters, InvokeResult invokeResult)
        {
            var rateLimitService = callContext.Resolver.Resolve<IRateLimitService>() ?? throw new Exception($"没有发现{typeof(IRateLimitService)}已注册的注册服务。");

            var rateLimiterPolicy = rateLimitService.GetRateLimiterPolicy(this.PolicyName) ?? throw new Exception($"没有找到名称为“{this.PolicyName}”的限流策略。");

            var rateLimiter = rateLimiterPolicy.GetRateLimiter(callContext);
            var rateLimitLease = await rateLimiter.AcquireAsync();
            if (rateLimitLease.IsAcquired)
            {
                this.m_rateLimitLease=rateLimitLease;
                return new InvokeResult() { Status = InvokeStatus.Ready };
            }
            else
            {
                return new InvokeResult() { Status = InvokeStatus.Exception, Message = "限制访问" };
            }
        }

        /// <inheritdoc/>
        public override Task<InvokeResult> ExecutedAsync(ICallContext callContext, object[] parameters, InvokeResult invokeResult)
        {
            this.m_rateLimitLease.SafeDispose();
            return Task.FromResult(invokeResult);
        }

        /// <inheritdoc/>
        public override Task<InvokeResult> ExecutExceptionAsync(ICallContext callContext, object[] parameters, InvokeResult invokeResult, Exception exception)
        {
            this.m_rateLimitLease.SafeDispose();
            return Task.FromResult(invokeResult);
        }
    }
}
