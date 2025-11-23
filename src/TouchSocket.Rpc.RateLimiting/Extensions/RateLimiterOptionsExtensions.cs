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

using System.Threading.RateLimiting;

namespace TouchSocket.Rpc.RateLimiting;


/// <summary>
/// 定义一个静态内部类，提供针对RateLimiter配置选项的扩展方法
/// </summary>
public static class RateLimiterOptionsExtensions
{
    /// <summary>
    /// 添加并发限流策略
    /// </summary>
    /// <param name="options">原有的RateLimiterOptions实例</param>
    /// <param name="policyName">限流策略的名称</param>
    /// <param name="configureOptions">用于配置ConcurrencyLimiterOptions的委托</param>
    /// <returns>返回修改后的RateLimiterOptions实例</returns>
    public static RateLimiterOptions AddConcurrencyLimiter(this RateLimiterOptions options, string policyName, Action<ConcurrencyLimiterOptions> configureOptions)
    {
        // 创建并发限流选项的实例
        var option = new ConcurrencyLimiterOptions();
        // 调用委托以配置并发限流选项
        configureOptions.Invoke(option);
        // 将配置好的并发限流策略添加到选项中
        options.AddPolicy(policyName, new ConcurrencyLimiterPolicy(option));
        // 返回更新后的限流选项实例
        return options;
    }

    /// <summary>
    /// 添加固定窗口限流策略
    /// </summary>
    /// <param name="options">限流器选项配置</param>
    /// <param name="policyName">策略名称</param>
    /// <param name="configureOptions">配置固定窗口限流策略的委托</param>
    /// <returns>返回更新后的限流器选项配置</returns>
    public static RateLimiterOptions AddFixedWindowLimiter(this RateLimiterOptions options, string policyName, Action<FixedWindowRateLimiterOptions> configureOptions)
    {
        // 创建固定窗口限流选项实例
        var option = new FixedWindowRateLimiterOptions();
        // 调用委托以配置固定窗口限流选项
        configureOptions.Invoke(option);
        // 添加配置好的固定窗口限流策略到选项配置中
        options.AddPolicy(policyName, new FixedWindowRateLimiterPolicy(option));
        // 返回更新后的限流器选项配置
        return options;
    }

    /// <summary>
    /// 添加滑动窗口限流策略
    /// </summary>
    /// <param name="options">RateLimiterOptions对象，用于配置限流策略</param>
    /// <param name="policyName">限流策略的名称</param>
    /// <param name="configureOptions">配置滑动窗口限流策略的委托</param>
    /// <returns>返回更新后的RateLimiterOptions对象</returns>
    public static RateLimiterOptions AddSlidingWindowLimiter(this RateLimiterOptions options, string policyName, Action<SlidingWindowRateLimiterOptions> configureOptions)
    {
        // 创建滑动窗口限流器选项实例
        var option = new SlidingWindowRateLimiterOptions();
        // 调用委托以配置滑动窗口限流器选项
        configureOptions.Invoke(option);
        // 添加滑动窗口限流策略到RateLimiterOptions对象中
        options.AddPolicy(policyName, new SlidingWindowLimiterPolicy(option));
        // 返回更新后的RateLimiterOptions对象
        return options;
    }

    /// <summary>
    /// 添加令牌桶限流策略
    /// </summary>
    /// <param name="options">限流器选项配置</param>
    /// <param name="policyName">策略名称</param>
    /// <param name="configureOptions">配置令牌桶选项的委托</param>
    /// <returns>返回更新后的限流器选项配置</returns>
    public static RateLimiterOptions AddTokenBucketLimiter(this RateLimiterOptions options, string policyName, Action<TokenBucketRateLimiterOptions> configureOptions)
    {
        // 创建令牌桶限流选项实例
        var option = new TokenBucketRateLimiterOptions();
        // 调用委托以配置令牌桶选项
        configureOptions.Invoke(option);
        // 使用令牌桶限流策略创建策略实例，并添加到限流器选项配置中
        options.AddPolicy(policyName, new TokenBucketLimiterPolicy(option));
        // 返回更新后的限流器选项配置
        return options;
    }
}