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
/// 流量控制器。
/// </summary>
public class FlowGate : Counter
{
    /// <summary>
    /// 最大值
    /// </summary>
    public long Maximum { get; set; } = long.MaxValue;

    /// <summary>
    /// 最长休眠周期。默认为5s.
    /// <para>当设置为5s时，假如设置的<see cref="Maximum"/>=10，而一次递增了100，则理应会休眠10s，但是会休眠5s。反之，如果设置1，则每秒周期都会清空。</para>
    /// </summary>
    public TimeSpan MaximumWaitTime { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// 检测等待
    /// </summary>
    public void AddCheckWait(long increment)
    {
        this.Increment(increment);
        if (this.m_count > this.Maximum)
        {
            Thread.Sleep(this.GetWaitTime());
        }
    }

    /// <summary>
    /// 异步添加并检查等待
    /// </summary>
    /// <param name="increment">要增加的值</param>
    /// <returns>任务延迟后的异步结果</returns>
    /// <remarks>
    /// 该方法主要用于限流，通过增加内部计数器并检查是否超过最大值，
    /// 如果超过，则根据设定的周期计算需要等待的时间，以Task.Delay的形式实现等待。
    /// </remarks>
    public async Task AddCheckWaitAsync(long increment)
    {
        this.Increment(increment);
        if (this.m_count > this.Maximum)
        {
            await Task.Delay(this.GetWaitTime()).ConfigureDefaultAwait();
        }
    }

    private TimeSpan GetWaitTime()
    {
        var elapsed = DateTimeOffset.UtcNow - this.LastIncrement;
        var ticks = (long)Math.Min((double)this.m_count / this.Maximum * this.Period.Ticks, this.MaximumWaitTime.Ticks);
        var waitTime = TimeSpan.FromTicks(ticks) - elapsed;
        if (elapsed >= this.Period || waitTime <= TimeSpan.Zero)
        {
            return TimeSpan.Zero;
        }

        // 延迟按毫秒向上取整，避免周期末尾不足一毫秒时持续放行。
        return TimeSpan.FromMilliseconds(Math.Ceiling(waitTime.TotalMilliseconds));
    }
}
