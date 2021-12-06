using RRQMCore.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RRQMCore
{
    /// <summary>
    /// 字符串资源字典
    /// </summary>
    public static class StringResStore
    {
        /// <summary>
        /// 获取资源字符
        /// </summary>
        /// <param name="enum"></param>
        /// <returns></returns>
        public static string GetResString(this Enum @enum)
        {
            return Resource.ResourceManager.GetString(@enum.ToString());
        }

        /// <summary>
        /// 获取资源字符
        /// </summary>
        /// <param name="enum"></param>
        /// <param name="objs"></param>
        /// <returns></returns>
        public static string GetResString(this Enum @enum, params object[] objs)
        {
            return Resource.ResourceManager.GetString(@enum.ToString()).Format(objs);
        }
    }
}
