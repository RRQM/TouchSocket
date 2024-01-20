using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace TouchSocket.Rpc.RateLimiting
{
    internal class RateLimitService: IRateLimitService
    {
        private readonly Dictionary<string, IRateLimiterPolicy> m_rateLimiterPolicys;
        public RateLimitService(RateLimiterOptions options)
        {
            this.m_rateLimiterPolicys = options.RateLimiterPolicys;
        }

        public IRateLimiterPolicy GetRateLimiterPolicy(string policyName) 
        {
            if (this.m_rateLimiterPolicys.TryGetValue(policyName,out var rateLimiter))
            {
                return rateLimiter;
            }

            return null;
        }
    }
}
