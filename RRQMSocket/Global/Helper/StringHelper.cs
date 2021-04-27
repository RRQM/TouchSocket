using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace RRQMSocket
{
    /// <summary>
    /// 字符串扩展
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        /// 只按第一个匹配项分割
        /// </summary>
        /// <param name="str"></param>
        /// <param name="split"></param>
        /// <returns></returns>
        public static string[] SplitFirst(this string str, char split)
        {
            List<string> s = new List<string>();
            int index = str.IndexOf(split);
            if (index > 0)
            {
                s.Add(str.Substring(0, index).Trim());
                s.Add(str.Substring(index + 1, str.Length - index - 1).Trim());
            }

            return s.ToArray();
        }
    }
}
