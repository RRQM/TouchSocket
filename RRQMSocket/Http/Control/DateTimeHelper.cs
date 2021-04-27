using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RRQMSocket.Http
{
    public static class DateTimeHelper
    {
        static DateTime _dt;

        /// <summary>
        /// 时间工具类
        /// </summary>
        static DateTimeHelper()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    _dt = DateTime.Now;
                    Thread.Sleep(1);
                }
            }, TaskCreationOptions.LongRunning);
        }

        public static DateTime Now
        {
            get
            {
                if (_dt.Year == 1) _dt = DateTime.Now;
                return _dt;
            }
        }

        /// <summary>
        /// 将中国时间转换成UTC
        /// </summary>
        public static DateTime UtcNow
        {
            get
            {
                return DateTimeHelper.Now.AddHours(-8);
            }
        }

        public static string ToString(string format = "yyyy-MM-dd HH:mm:ss.fff")
        {
            return Now.ToString(format, CultureInfo.InvariantCulture);
        }

        public static string ToFString(this DateTime dt, string format = "yyyy-MM-dd HH:mm:ss.fff")
        {
            return dt.ToString(format, CultureInfo.InvariantCulture);
        }

        public static string ToGMTString(this DateTime dt, string v)
        {
            return dt.ToString("r", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 获取unix 时间戳
        /// </summary>
        /// <returns></returns>
        public static int GetUnixTick()
        {
            TimeSpan ts = DateTimeHelper.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();
            return Convert.ToInt32(ts.TotalSeconds);
        }

        /// <summary>
        /// 获取unix 时间戳
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static int ToUnixTick(this DateTime dateTime)
        {
            TimeSpan ts = dateTime - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime();
            return Convert.ToInt32(ts.TotalSeconds);
        }

        /// <summary>
        /// 将Unix时间戳转换成DateTime
        /// </summary>
        /// <param name="unixTick"></param>
        /// <returns></returns>
        public static DateTime ToDateTimeByUnixTick(this int unixTick)
        {
            var start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
            return start.AddSeconds(unixTick);
        }


    }
}
