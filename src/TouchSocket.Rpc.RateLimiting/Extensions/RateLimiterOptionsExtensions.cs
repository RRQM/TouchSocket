using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace TouchSocket.Rpc.RateLimiting
{
    /// <summary>
    /// RateLimiterOptionsExtensions
    /// </summary>
    public static class RateLimiterOptionsExtensions
    {
        /// <summary>
        /// 添加并发限流策略
        /// </summary>
        /// <param name="options"></param>
        /// <param name="policyName"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static RateLimiterOptions AddConcurrencyLimiter(this RateLimiterOptions options, string policyName, Action<ConcurrencyLimiterOptions> configureOptions)
        {
            var option = new ConcurrencyLimiterOptions();
            configureOptions.Invoke(option);
            options.AddPolicy(policyName,new ConcurrencyLimiterPolicy(option));
            return options;
        }

        /// <summary>
        /// 添加固定窗口限流策略
        /// </summary>
        /// <param name="options"></param>
        /// <param name="policyName"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static RateLimiterOptions AddFixedWindowLimiter(this RateLimiterOptions options, string policyName, Action<FixedWindowRateLimiterOptions> configureOptions)
        {
            var option = new FixedWindowRateLimiterOptions();
            configureOptions.Invoke(option);
            options.AddPolicy(policyName, new FixedWindowRateLimiterPolicy(option));
            return options;
        }

        /// <summary>
        /// 添加滑动窗口限流策略
        /// </summary>
        /// <param name="options"></param>
        /// <param name="policyName"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static RateLimiterOptions AddSlidingWindowLimiter(this RateLimiterOptions options, string policyName, Action<SlidingWindowRateLimiterOptions> configureOptions)
        {
            var option = new SlidingWindowRateLimiterOptions();
            configureOptions.Invoke(option);
            options.AddPolicy(policyName, new SlidingWindowLimiterPolicy(option));
            return options;
        }

        /// <summary>
        /// 添加令牌桶限流策略
        /// </summary>
        /// <param name="options"></param>
        /// <param name="policyName"></param>
        /// <param name="configureOptions"></param>
        /// <returns></returns>
        public static RateLimiterOptions AddTokenBucketLimiter(this RateLimiterOptions options, string policyName, Action<TokenBucketRateLimiterOptions> configureOptions)
        {
            var option = new TokenBucketRateLimiterOptions();
            configureOptions.Invoke(option);
            options.AddPolicy(policyName, new TokenBucketLimiterPolicy(option));
            return options;
        }
    }
}
