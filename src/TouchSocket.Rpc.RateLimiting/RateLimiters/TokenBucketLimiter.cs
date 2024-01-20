using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace TouchSocket.Rpc.RateLimiting
{
    internal sealed class TokenBucketLimiterPolicy : RateLimiterPolicy
    {
        private readonly TokenBucketRateLimiterOptions m_options;

        public TokenBucketLimiterPolicy(TokenBucketRateLimiterOptions options)
        {
            this.m_options = options;
        }

        protected override RateLimiter NewRateLimiter(MethodInfo method)
        {
            return new TokenBucketRateLimiter(m_options);
        }
    }
}
