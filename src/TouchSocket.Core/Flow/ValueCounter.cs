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

namespace TouchSocket.Core;

/// <summary>
/// 值类型计数器，提供基于时间周期的计数功能。
/// 支持线程安全的计数操作，可在达到指定周期时触发回调。
/// </summary>
/// <remarks>
/// 该计数器使用原子操作确保线程安全性，适用于高并发场景下的统计需求。
/// 计数器会跟踪周期内的累计计数和总计数，并在周期结束时自动重置周期计数。
/// </remarks>
public struct ValueCounter
{
    private long m_count;

    /// <summary>
    /// 最后一次递增时间。
    /// </summary>
    private DateTimeOffset m_lastIncrement;

    private long m_totalCount;

    /// <summary>
    /// 初始化 <see cref="ValueCounter"/> 结构的新实例。
    /// </summary>
    /// <param name="period">计数周期时间间隔。</param>
    /// <param name="onPeriod">当达到一个周期时的回调函数，参数为周期内的累计计数值。</param>
    public ValueCounter(TimeSpan period, Action<long> onPeriod) : this()
    {
        this.OnPeriod = onPeriod;
        this.Period = period;
    }

    /// <summary>
    /// 获取当前周期内的累计计数值。
    /// </summary>
    /// <value>返回当前周期内的计数值，当周期结束时会重置为0。</value>
    public readonly long Count => this.m_count;

    /// <summary>
    /// 获取最后一次递增的时间。
    /// </summary>
    /// <value>返回最后一次调用递增操作的UTC时间。</value>
    public readonly DateTimeOffset LastIncrement => this.m_lastIncrement;

    /// <summary>
    /// 获取或设置当达到一个周期时的回调函数。
    /// </summary>
    /// <value>当周期结束时执行的回调函数，参数为该周期内的累计计数值。如果为 <see langword="null"/>，则不执行任何操作。</value>
    public Action<long> OnPeriod { get; set; }

    /// <summary>
    /// 获取或设置计数周期的时间间隔。
    /// </summary>
    /// <value>计数器的周期时间间隔，用于判断是否进入新的计数周期。</value>
    public TimeSpan Period { get; set; }

    /// <summary>
    /// 获取从开始到现在的总计数值。
    /// </summary>
    /// <value>返回计数器的累计总数，包括所有周期的计数值，不会因周期重置而清零。</value>
    public readonly long TotalCount => this.m_totalCount;

    /// <summary>
    /// 以线程安全的方式增加指定的计数值。
    /// </summary>
    /// <param name="value">要增加的计数值。</param>
    /// <returns>如果当前递增在同一周期内，返回 <see langword="true"/>；如果进入了新的周期，返回 <see langword="false"/>。</returns>
    /// <remarks>
    /// 此方法会检查自上次递增以来是否超过了设定的周期时间。
    /// 如果超过周期时间，会触发 <see cref="OnPeriod"/> 回调并重置当前周期计数。
    /// 所有计数操作都使用原子操作确保线程安全。
    /// </remarks>
    public bool Increment(long value)
    {
        // 用于判断是否在一个新的周期内
        bool isPeriod;

        // 获取当前时间
        var dateTime = DateTimeOffset.UtcNow;

        // 判断自上次递增以来是否超过了设定的周期时间
        if (dateTime - this.LastIncrement > this.Period)
        {
            // 当周期结束时，调用周期结束的回调函数，并重置计数器
            this.OnPeriod?.Invoke(this.m_count);
            Interlocked.Exchange(ref this.m_count, 0);

            // 设置标志，表示不在周期内
            isPeriod = false;

            // 更新上次递增的时间为当前时间
            this.m_lastIncrement = dateTime;
        }
        else
        {
            // 设置标志，表示在周期内
            isPeriod = true;
        }

        // 原子性地增加计数器的值
        Interlocked.Add(ref this.m_count, value);
        Interlocked.Add(ref this.m_totalCount, value);

        // 返回是否在周期内的标志
        return isPeriod;
    }

    /// <summary>
    /// 以线程安全的方式增加1个计数。
    /// </summary>
    /// <returns>如果当前递增在同一周期内，返回 <see langword="true"/>；如果进入了新的周期，返回 <see langword="false"/>。</returns>
    /// <remarks>
    /// 这是 <see cref="Increment(long)"/> 方法的便捷重载，等效于调用 <c>Increment(1)</c>。
    /// </remarks>
    public bool Increment()
    {
        // 调用重载的Increment方法，增量为1
        return this.Increment(1);
    }

    /// <summary>
    /// 重置计数器的所有状态，包括当前周期计数、总计数和最后递增时间。
    /// </summary>
    /// <remarks>
    /// 此操作会将 <see cref="Count"/> 和 <see cref="TotalCount"/> 重置为0，
    /// 并将 <see cref="LastIncrement"/> 重置为默认值。
    /// 重置操作是线程安全的。
    /// </remarks>
    public void Reset()
    {
        Interlocked.Exchange(ref this.m_count, 0);
        Interlocked.Exchange(ref this.m_totalCount, 0);
        this.m_lastIncrement = default;
    }
}