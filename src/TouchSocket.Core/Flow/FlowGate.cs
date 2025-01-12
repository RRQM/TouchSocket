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
using System.Threading;
using System.Threading.Tasks;

namespace TouchSocket.Core
{
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
            if (this.Increment(increment))
            {
                if (this.m_count > this.Maximum)
                {
                    var time = (DateTime.UtcNow - this.LastIncrement);
                    var waitTime = this.Period - time <= TimeSpan.Zero ? TimeSpan.Zero : (this.GetBaseTime() - time);
                    waitTime = waitTime < TimeSpan.Zero ? TimeSpan.Zero : waitTime;
                    Thread.Sleep(waitTime);
                }
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
            // 尝试增加计数器，如果返回true，则表示增加成功，需要进一步处理
            if (this.Increment(increment))
            {
                // 如果当前计数超过设定的最大值
                if (this.m_count > this.Maximum)
                {
                    // 计算自上次增加以来的时间差
                    var time = (DateTime.UtcNow - this.LastIncrement);
                    // 计算还需要等待的时间，确保等待时间不为负
                    var waitTime = this.Period - time <= TimeSpan.Zero ? TimeSpan.Zero : (this.GetBaseTime() - time);
                    // 将等待时间小于0的情况调整为0
                    waitTime = waitTime < TimeSpan.Zero ? TimeSpan.Zero : waitTime;
                    // 异步延迟等待，不阻塞主线程
                    await Task.Delay(waitTime).ConfigureAwait(EasyTask.ContinueOnCapturedContext);
                }
            }
        }

        private TimeSpan GetBaseTime()
        {
            return TimeSpan.FromTicks(Math.Min((int)((double)this.m_count / this.Maximum * this.Period.Ticks), this.MaximumWaitTime.Ticks));
        }
    }
}