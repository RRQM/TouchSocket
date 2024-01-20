using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace TouchSocket.Rpc.RateLimiting
{
    internal sealed class ConcurrencyLimiterPolicy : RateLimiterPolicy
    {
        private readonly ConcurrencyLimiterOptions m_options;

        public ConcurrencyLimiterPolicy(ConcurrencyLimiterOptions options)
        {
            this.m_options = options;
        }

        protected override RateLimiter NewRateLimiter(MethodInfo method)
        {
            return new ConcurrencyLimiter(m_options);
        }
    }
}
