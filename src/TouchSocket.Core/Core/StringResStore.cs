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
using System.Collections.Concurrent;
using System.ComponentModel;

namespace TouchSocket.Core
{
    /// <summary>
    /// 字符串资源字典
    /// </summary>
    public static class StringResStore
    {
        private static readonly ConcurrentDictionary<Enum, string> s_cache = new ConcurrentDictionary<Enum, string>();

        /// <summary>
        /// 获取资源字符
        /// </summary>
        /// <param name="enum">枚举值</param>
        /// <param name="objs">格式化字符串的参数</param>
        /// <returns>资源字符</returns>
        public static string GetDescription(this Enum @enum, params object[] objs)
        {
            // 尝试从缓存中获取枚举的描述
            if (s_cache.TryGetValue(@enum, out var str))
            {
                // 如果缓存中的描述为空字符串，则返回枚举的字符串表示形式，否则返回格式化后的描述
                return string.IsNullOrEmpty(str) ? @enum.ToString() : str.Format(objs);
            }

            // 尝试获取枚举值的DescriptionAttribute属性
            if (@enum.GetAttribute<DescriptionAttribute>() is DescriptionAttribute description)
            {
                // 获取DescriptionAttribute属性中的描述
                var res = description.Description;
                // 将枚举值和其描述添加到缓存中
                s_cache.TryAdd(@enum, res);
                // 如果描述不为空字符串，则返回格式化后的描述，否则返回枚举的字符串表示形式
                if (!string.IsNullOrEmpty(res))
                {
                    return objs.Length > 0 ? res.Format(objs) : res;
                }
            }

            // 如果无法获取描述，则返回枚举的字符串表示形式
            return @enum.ToString();
        }
    }
}