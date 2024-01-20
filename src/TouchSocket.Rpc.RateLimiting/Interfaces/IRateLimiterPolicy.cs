using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace TouchSocket.Rpc.RateLimiting
{
    /// <summary>
    /// IRateLimiterPolicy
    /// </summary>
    public interface IRateLimiterPolicy
    {
        /// <summary>
        /// GetRateLimiter
        /// </summary>
        /// <param name="callContext"></param>
        /// <returns></returns>
        RateLimiter GetRateLimiter(ICallContext callContext);
    }
}
