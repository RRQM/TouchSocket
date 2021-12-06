//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RRQMCore.Helper
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

        /// <summary>
        /// 按字符串分割
        /// </summary>
        /// <param name="str"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static string[] Split(this string str, string pattern)
        {
            return Regex.Split(str, pattern);
        }

        /// <summary>
        /// 只按最后一个匹配项分割
        /// </summary>
        /// <param name="str"></param>
        /// <param name="split"></param>
        /// <returns></returns>
        public static string[] SplitLast(this string str, char split)
        {
            List<string> s = new List<string>();
            int index = str.LastIndexOf(split);
            if (index > 0)
            {
                s.Add(str.Substring(0, index).Trim());
                s.Add(str.Substring(index + 1, str.Length - index - 1).Trim());
            }

            return s.ToArray();
        }

        /// <summary>
        /// 将字符串转换为指定类型
        /// </summary>
        /// <param name="str"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object ParseToType(this string str, Type type)
        {
            dynamic obj = null;
            if (type == RRQMReadonly.stringType)
            {
                obj = str;
            }
            else if (type == RRQMReadonly.byteType)
            {
                obj = byte.Parse(str);
            }
            else if (type == RRQMReadonly.boolType)
            {
                obj = bool.Parse(str);
            }
            else if (type == RRQMReadonly.shortType)
            {
                obj = short.Parse(str);
            }
            else if (type == RRQMReadonly.intType)
            {
                obj = int.Parse(str);
            }
            else if (type == RRQMReadonly.longType)
            {
                obj = long.Parse(str);
            }
            else if (type == RRQMReadonly.floatType)
            {
                obj = float.Parse(str);
            }
            else if (type == RRQMReadonly.doubleType)
            {
                obj = double.Parse(str);
            }
            else if (type == RRQMReadonly.decimalType)
            {
                obj = decimal.Parse(str);
            }
            else if (type == RRQMReadonly.dateTimeType)
            {
                obj = DateTime.Parse(str);
            }

            return obj;
        }

        /// <summary>
        /// 按格式填充
        /// </summary>
        /// <param name="str"></param>
        /// <param name="ps"></param>
        /// <returns></returns>
        public static string Format(this string str, params object[] ps)
        {
            return string.Format(str, ps);
        }
    }
}