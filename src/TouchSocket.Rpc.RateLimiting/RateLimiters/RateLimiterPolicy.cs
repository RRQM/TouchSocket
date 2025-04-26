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
using System.Collections.Concurrent;
using System.Threading.RateLimiting;
using TouchSocket.Core;

namespace TouchSocket.Rpc.RateLimiting;


/// <summary>
/// RateLimiter策略的抽象基类，用于定义针对特定分区键的限流策略。
/// </summary>
/// <typeparam name="TPartitionKey">分区键的类型，用于区分不同的数据分区。</typeparam>
public abstract class RateLimiterPolicy<TPartitionKey> : IRateLimiterPolicy
{
    private readonly ConcurrentDictionary<TPartitionKey, RateLimiterClass> m_rateLimiters = new ConcurrentDictionary<TPartitionKey, RateLimiterClass>();

    private TimeSpan m_maxLifetime = TimeSpan.FromMinutes(60);

    private ValueCounter m_valueCounter;

    /// <summary>
    /// 初始化RateLimiterPolicy类的实例。
    /// </summary>
    /// <remarks>
    /// 该构造函数主要负责初始化一个值计数器（ValueCounter），
    /// 并设置其周期（Period）和周期事件处理方法（OnPeriod）。
    /// 值计数器用于跟踪和控制请求的速率，以防止过载或滥用。
    /// </remarks>
    protected RateLimiterPolicy()
    {
        // 初始化一个值计数器实例，并设置其周期为最大生命周期（m_maxLifetime），
        // 同时指定周期事件处理方法为当前实例的OnPeriod方法。
        this.m_valueCounter = new ValueCounter()
        {
            Period = this.m_maxLifetime,
            OnPeriod = this.OnPeriod
        };
    }

    /// <summary>
    /// 最大生命周期限制。默认：60分钟。
    /// <para>
    /// 当一个限流器在设定时间内没被使用时，会被移除销毁。
    /// </para>
    /// </summary>
    public TimeSpan MaxLifetime
    {
        get => this.m_maxLifetime;
        set
        {
            this.m_maxLifetime = value;
            this.m_valueCounter.Period = TimeSpan.FromTicks(this.m_maxLifetime.Ticks / 10);
        }
    }

    /// <summary>
    /// 获取限流器
    /// </summary>
    /// <param name="callContext">调用上下文对象，用于确定限流器的分区键</param>
    /// <returns>返回一个限流器实例</returns>
    public RateLimiter GetRateLimiter(ICallContext callContext)
    {
        // 根据调用上下文获取分区键，用于区分不同的限流场景
        var partitionKey = this.GetPartitionKey(callContext);

        // 使用分区键从已存在的限流器映射中获取或添加一个新的限流器类
        var rateLimiterClass = this.m_rateLimiters.GetOrAdd(partitionKey, this.GetRateLimiterClass);

        // 更新限流器类的最后访问时间
        rateLimiterClass.DateTime = DateTimeOffset.UtcNow;

        // 增加值计数器，用于跟踪限流器的使用次数
        this.m_valueCounter.Increment();
        // 返回当前限流器实例
        return rateLimiterClass.RateLimiter;
    }

    /// <summary>
    /// 获取分区键
    /// </summary>
    /// <param name="callContext">调用上下文对象，可能包含获取分区键所需的信息</param>
    /// <returns>返回一个泛型分区键，具体类型由子类实现决定</returns>
    protected abstract TPartitionKey GetPartitionKey(ICallContext callContext);

    /// <summary>
    /// 创建一个新的限流器
    /// </summary>
    /// <param name="partitionKey">分区键，用于区分不同的限流对象</param>
    /// <returns>返回一个新的限流器实例</returns>
    protected abstract RateLimiter NewRateLimiter(TPartitionKey partitionKey);

    private RateLimiterClass GetRateLimiterClass(TPartitionKey partitionKey)
    {
        return new RateLimiterClass()
        {
            RateLimiter = this.NewRateLimiter(partitionKey),
            DateTime = DateTimeOffset.UtcNow
        };
    }

    private void OnPeriod(long obj)
    {
        this.m_rateLimiters.RemoveWhen((p) =>
        {
            if (DateTimeOffset.UtcNow - p.Value.DateTime > this.m_maxLifetime)
            {
                return true;
            }
            return false;
        });
    }

    #region Class

    private class RateLimiterClass
    {
        public DateTimeOffset DateTime { get; set; }
        public RateLimiter RateLimiter { get; set; }
    }

    #endregion Class
}