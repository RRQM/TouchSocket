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
        private static readonly ConcurrentDictionary<Enum, string> m_cache = new ConcurrentDictionary<Enum, string>();

        /// <summary>
        /// 获取资源字符
        /// </summary>
        /// <param name="enum"></param>
        /// <param name="objs"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum @enum, params object[] objs)
        {
            if (m_cache.TryGetValue(@enum, out var str))
            {
                return string.IsNullOrEmpty(str) ? @enum.ToString() : str.Format(objs);
            }
            if (@enum.GetAttribute<DescriptionAttribute>() is DescriptionAttribute description)
            {
                var res = description.Description;
                m_cache.TryAdd(@enum, res);
                if (!string.IsNullOrEmpty(res))
                {
                    return objs.Length > 0 ? res.Format(objs) : res;
                }
            }
            return @enum.ToString();
        }
    }
}