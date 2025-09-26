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

namespace TouchSocket.Http;


/// <summary>
/// 表示HTTP头部的接口，继承自<see cref="IDictionary{TKey, TValue}"/>，用于管理HTTP头部键值对。
/// </summary>
public interface IHttpHeader : IDictionary<string, string>
{
    /// <summary>
    /// 获取指定键的Header值。
    /// </summary>
    /// <param name="key">要获取的Header键。</param>
    /// <returns>返回指定键的Header值，如果不存在则返回<see langword="null"/>。</returns>
    string Get(string key);

    /// <summary>
    /// 判断指定键的值是否等于指定值。
    /// </summary>
    /// <param name="key">要检查的键。</param>
    /// <param name="value">要匹配的值。</param>
    /// <param name="ignoreCase">是否忽略大小写，默认为<see langword="true"/>。</param>
    /// <returns>如果存在指定键且值匹配则返回<see langword="true"/>，否则返回<see langword="false"/>。</returns>
    bool Contains(string key, string value, bool ignoreCase = true);
}