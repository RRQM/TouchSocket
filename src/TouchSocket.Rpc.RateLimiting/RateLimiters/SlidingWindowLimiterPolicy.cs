using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace TouchSocket.Rpc.RateLimiting
{
    internal sealed class SlidingWindowLimiterPolicy : RateLimiterPolicy
    {
        private readonly SlidingWindowRateLimiterOptions m_options;

        public SlidingWindowLimiterPolicy(SlidingWindowRateLimiterOptions options)
        {
            this.m_options = options;
        }

        protected override RateLimiter NewRateLimiter(MethodInfo method)
        {
            return new SlidingWindowRateLimiter(m_options);
        }
    }
}
