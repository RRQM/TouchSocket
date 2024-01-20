using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace TouchSocket.Rpc.RateLimiting
{
    /// <summary>
    /// Rpc速率限定服务接口
    /// </summary>
    public interface IRateLimitService
    {
        /// <summary>
        /// 获取限流策略
        /// </summary>
        /// <param name="policyName"></param>
        /// <returns></returns>
        IRateLimiterPolicy GetRateLimiterPolicy(string policyName);
    }
}
