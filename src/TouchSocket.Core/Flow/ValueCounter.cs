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
        /// <param name="value"></param>
        /// <returns>返回值表示当前递增的是否在一个周期内。</returns>
        public bool Increment(long value)
        {
            bool isPeriod;
            var dateTime = DateTime.UtcNow;
            if (dateTime - this.LastIncrement > this.Period)
            {
                this.OnPeriod?.Invoke(this.m_count);
                Interlocked.Exchange(ref this.m_count, 0);
                isPeriod = false;
                this.m_lastIncrement = dateTime;
            }
            else
            {
                isPeriod = true;
            }
            Interlocked.Add(ref this.m_count, value);
            return isPeriod;
        }

        /// <summary>
        /// 累计增加一个计数
        /// </summary>
        /// <returns></returns>
        public bool Increment()
        {
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