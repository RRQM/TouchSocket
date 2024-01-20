using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.RateLimiting;

namespace TouchSocket.Rpc.RateLimiting
{
    /// <summary>
    /// RateLimiterOptions
    /// </summary>
    public class RateLimiterOptions
    {
        /// <summary>
        /// RateLimiterPolicys
        /// </summary>
        public Dictionary<string, IRateLimiterPolicy> RateLimiterPolicys { get; private set; } = new Dictionary<string, IRateLimiterPolicy>();

        /// <summary>
        /// 添加限流策略
        /// </summary>
        /// <param name="policyName"></param>
        /// <param name="rateLimiterPolicy"></param>
        /// <returns></returns>
        public RateLimiterOptions AddPolicy(string policyName, IRateLimiterPolicy rateLimiterPolicy)
        {
            this.RateLimiterPolicys.Add(policyName, rateLimiterPolicy);
            return this;
        }
    }
}
