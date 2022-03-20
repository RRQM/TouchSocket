//------------------------------------------------------------------------------
//  此代码版权（除特别声明或在RRQMCore.XREF命名空间的代码）归作者本人若汝棋茗所有
//  源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
//  CSDN博客：https://blog.csdn.net/qq_40374647
//  哔哩哔哩视频：https://space.bilibili.com/94253567
//  Gitee源代码仓库：https://gitee.com/RRQM_Home
//  Github源代码仓库：https://github.com/RRQM
//  API首页：https://www.yuque.com/eo2w71/rrqm
//  交流QQ群：234762506
//  感谢您的下载和使用
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace RRQMSocket.Http
{
    /// <summary>
    /// 枚举扩展类
    /// </summary>
    public static class EnumHelper
    {
        private static ConcurrentDictionary<Enum, string> _cache = new ConcurrentDictionary<Enum, string>();

        /// <summary>
        /// 获取DescriptionAttribute
        /// </summary>
        /// <param name="enum"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum @enum)
        {
            var result = string.Empty;

            if (@enum == null) return result;

            if (!_cache.TryGetValue(@enum, out result))
            {
                var typeInfo = @enum.GetType();

                var enumValues = typeInfo.GetEnumValues();

                foreach (var value in enumValues)
                {
                    if (@enum.Equals(value))
                    {
                        MemberInfo memberInfo = typeInfo.GetMember(value.ToString()).First();

                        result = memberInfo.GetCustomAttribute<DescriptionAttribute>().Description;
                    }
                }

                _cache.TryAdd(@enum, result);
            }

            return result;
        }

        /// <summary>
        /// 根据字符串获取枚举
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool GetEnum<T>(string str, out T result) where T : struct
        {
            return Enum.TryParse<T>(str, out result);
        }
    }
}