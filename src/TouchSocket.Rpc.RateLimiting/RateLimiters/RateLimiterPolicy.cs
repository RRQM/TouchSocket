using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace TouchSocket.Rpc.RateLimiting
{
    internal abstract class RateLimiterPolicy : ConcurrentDictionary<MethodInfo, RateLimiter>, IRateLimiterPolicy
    {
        public RateLimiter GetRateLimiter(ICallContext callContext)
        {
            return this.GetOrAdd(callContext.MethodInstance.Info, a => this.NewRateLimiter(a));
        }

        protected abstract RateLimiter NewRateLimiter(MethodInfo method);
    }
}
