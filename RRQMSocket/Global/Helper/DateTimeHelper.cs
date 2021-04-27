using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RRQMSocket
{
    /// <summary>
    /// 日期扩展
    /// </summary>
    public static class DateTimeHelper
    {
        /// <summary>
        /// 格林尼治标准时间
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static string ToGMTString(this DateTime dt, string v)
        {
            return dt.ToString("r", CultureInfo.InvariantCulture);
        }

    }
}
