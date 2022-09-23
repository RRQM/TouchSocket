using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocketPro.Core
{
    /// <summary>
    /// DateExtensions
    /// </summary>
    public static class DateExtensions
    {
        private static readonly DateTime m_utc_time = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// 将时间转为毫秒级别的短整形
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ConvertTime(this in DateTime time)
        {
            return (uint)(Convert.ToInt64(time.Subtract(m_utc_time).TotalMilliseconds) & 0xffffffff);
        }

        private static readonly DateTimeOffset m_utc1970 = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        /// <summary>
        /// 将时间转为毫秒级别的短整形
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ConvertTime(this in DateTimeOffset time)
        {
            return (uint)(Convert.ToInt64(time.Subtract(m_utc1970).TotalMilliseconds) & 0xffffffff);
        }
    }
}
