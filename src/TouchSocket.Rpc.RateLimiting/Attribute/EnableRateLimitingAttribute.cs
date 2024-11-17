//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

using System;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using TouchSocket.Core;

namespace TouchSocket.Rpc.RateLimiting
{
    /// <summary>
    /// 使用限流策略
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
    public class EnableRateLimitingAttribute : RpcActionFilterAttribute
    {
        private RateLimitLease m_rateLimitLease;

        /// <summary>
        /// 策略名称
        /// </summary>
        public string PolicyName { get; }

        /// <summary>
        /// 构造函数：初始化EnableRateLimitingAttribute对象
        /// </summary>
        /// <param name="policyName">限流策略的名称</param>
        public EnableRateLimitingAttribute(string policyName)
        {
            this.PolicyName = policyName; // 设置限流策略名称
        }

        /// <inheritdoc/>
        public override async Task<InvokeResult> ExecutingAsync(ICallContext callContext, object[] parameters, InvokeResult invokeResult)
        {
            var rateLimitService = callContext.Resolver.Resolve<IRateLimitService>() ?? throw new Exception($"没有发现{typeof(IRateLimitService)}已注册的注册服务。");

            var rateLimiterPolicy = rateLimitService.GetRateLimiterPolicy(this.PolicyName) ?? throw new Exception($"没有找到名称为“{this.PolicyName}”的限流策略。");

            var rateLimiter = rateLimiterPolicy.GetRateLimiter(callContext);
            var rateLimitLease = await rateLimiter.AcquireAsync().ConfigureAwait(false);
            if (rateLimitLease.IsAcquired)
            {
                this.m_rateLimitLease = rateLimitLease;
                return new InvokeResult() { Status = InvokeStatus.Ready };
            }
            else
            {
                return new InvokeResult() { Status = InvokeStatus.Exception, Message = "限制访问" };
            }
        }

        /// <inheritdoc/>
        public override Task<InvokeResult> ExecutedAsync(ICallContext callContext, object[] parameters, InvokeResult invokeResult, Exception exception)
        {
            this.m_rateLimitLease.SafeDispose();
            return Task.FromResult(invokeResult);
        }
    }
}