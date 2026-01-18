// ------------------------------------------------------------------------------
// 此代码版权（除特别声明或在XREF结尾的命名空间的代码）归作者本人若汝棋茗所有
// 源代码使用协议遵循本仓库的开源协议及附加协议，若本仓库没有设置，则按MIT开源协议授权
// CSDN博客：https://blog.csdn.net/qq_40374647
// 哔哩哔哩视频：https://space.bilibili.com/94253567
// Gitee源代码仓库：https://gitee.com/RRQM_Home
// Github源代码仓库：https://github.com/RRQM
// API首页：https://touchsocket.net/
// 交流QQ群：234762506
// 感谢您的下载和使用
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TouchSocket.Http;

/// <summary>
/// HTTP值集合扩展方法
/// </summary>
public static class HttpValuesExtension
{
    /// <summary>
    /// 确定集合是否包含指定的键及其关联的值。
    /// </summary>
    /// <param name="httpValues">HTTP值集合</param>
    /// <param name="key">要在集合中查找的键。不能为空。</param>
    /// <param name="value">要查找的、与指定键相关联的值。</param>
    /// <returns>若集合中存在包含指定键和对应值的元素，则返回<see langword="true"/>；否则返回<see langword="false"/></returns>
    public static bool Contains(this IHttpValues httpValues, string key, TextValues value)
    {
        return Contains(httpValues, key, value, StringComparison.Ordinal);
    }

    /// <summary>
    /// 确定集合是否包含指定的键及其关联的值。
    /// </summary>
    /// <param name="httpValues">HTTP值集合</param>
    /// <param name="key">要在集合中查找的键。不能为空。</param>
    /// <param name="value">要查找的、与指定键相关联的值。</param>
    /// <param name="ignoreCase">是否忽略大小写</param>
    /// <returns>若集合中存在包含指定键和对应值的元素，则返回<see langword="true"/>；否则返回<see langword="false"/></returns>
    public static bool Contains(this IHttpValues httpValues, string key, TextValues value, bool ignoreCase)
    {
        return Contains(httpValues, key, value, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
    }

    /// <summary>
    /// 确定集合是否包含指定的键及其关联的值。
    /// </summary>
    /// <param name="httpValues">HTTP值集合</param>
    /// <param name="key">要在集合中查找的键。不能为空。</param>
    /// <param name="value">要查找的、与指定键相关联的值。</param>
    /// <param name="comparison">字符串比较方式</param>
    /// <returns>若集合中存在包含指定键和对应值的元素，则返回<see langword="true"/>；否则返回<see langword="false"/></returns>
    public static bool Contains(this IHttpValues httpValues, string key, TextValues value, StringComparison comparison)
    {
        if (!httpValues.TryGetValue(key, out var val))
        {
            return false;
        }

        return val.Equals(value, comparison);
    }
}
