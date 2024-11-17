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

namespace TouchSocket.Core
{
    /// <summary>
    /// 值类型计数器。
    /// </summary>
    public struct ValueCounter
    {
        /// <summary>
        /// 周期内的累计计数值。
        /// </summary>
        private long m_count;

        /// <summary>
        /// 最后一次递增时间
        /// </summary>
        private DateTime m_lastIncrement;

        /// <summary>
        /// 周期内的累计计数值。
        /// </summary>
        public readonly long Count => this.m_count;

        /// <summary>
        /// 最后一次递增时间
        /// </summary>
        public readonly DateTime LastIncrement => this.m_lastIncrement;

        /// <summary>
        /// 当达到一个周期时触发。
        /// </summary>
        public Action<long> OnPeriod { get; set; }

        /// <summary>
        /// 计数周期。
        /// </summary>
        public TimeSpan Period { get; set; }

        /// <summary>
        /// 累计增加计数
        /// </summary>
        /// <param name="value">要增加的值</param>
        /// <returns>返回值表示当前递增的是否在一个新的周期内。</returns>
        public bool Increment(long value)
        {
            // 用于判断是否在一个新的周期内
            bool isPeriod;

            // 获取当前时间
            var dateTime = DateTime.UtcNow;

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

            // 返回是否在周期内的标志
            return isPeriod;
        }

        /// <summary>
        /// 累计增加一个计数
        /// </summary>
        /// <returns>返回是否成功增加计数</returns>
        public bool Increment()
        {
            // 调用重载的Increment方法，增量为1
            return this.Increment(1);
        }

        /// <summary>
        /// 重置<see cref="Count"/>和<see cref="LastIncrement"/>
        /// </summary>
        public void Reset()
        {
            this.m_count = 0;
            this.m_lastIncrement = default;
        }
    }
}