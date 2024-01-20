using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TouchSocket.Core;

namespace TouchSocket.Rpc.RateLimiting
{
    /// <summary>
    /// RateLimitingContainerExtension
    /// </summary>
    public static class RateLimitingContainerExtension
    {
        /// <summary>
        /// 向注册器中注册限流策略。
        /// </summary>
        /// <param name="registrator"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static IRegistrator AddRateLimiter(this IRegistrator registrator, Action<RateLimiterOptions> action)
        {
            var options = new RateLimiterOptions();
            action.Invoke(options);
            return registrator.RegisterSingleton<IRateLimitService>(new RateLimitService(options));
        }
    }
}
