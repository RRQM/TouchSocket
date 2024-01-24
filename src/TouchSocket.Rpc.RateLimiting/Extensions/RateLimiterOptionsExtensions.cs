//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://touchsocket.net/
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------

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