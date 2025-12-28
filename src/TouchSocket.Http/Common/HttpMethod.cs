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

using System.Diagnostics;

namespace TouchSocket.Http;

/// <summary>
/// 表示HTTP方法的结构体。
/// </summary>
[DebuggerDisplay("{m_value}")]
public readonly record struct HttpMethod
{
    private readonly string m_value;

    /// <summary>
    /// 表示HTTP GET方法。
    /// </summary>
    public static readonly HttpMethod Get = new HttpMethod("get");

    /// <summary>
    /// 表示HTTP POST方法。
    /// </summary>
    public static readonly HttpMethod Post = new HttpMethod("post");

    /// <summary>
    /// 表示HTTP PUT方法。
    /// </summary>
    public static readonly HttpMethod Put = new HttpMethod("put");

    /// <summary>
    /// 表示HTTP DELETE方法。
    /// </summary>
    public static readonly HttpMethod Delete = new HttpMethod("delete");

    /// <summary>
    /// 表示HTTP CONNECT方法。
    /// </summary>
    public static readonly HttpMethod Connect = new HttpMethod("connect");

    /// <summary>
    /// 表示HTTP OPTIONS方法。
    /// </summary>
    public static readonly HttpMethod Options = new HttpMethod("options");

    /// <summary>
    /// 初始化<see cref="HttpMethod"/>结构体的新实例。
    /// </summary>
    /// <param name="value">HTTP方法的字符串表示。</param>
    /// <exception cref="ArgumentNullException">当<paramref name="value"/>为<see langword="null"/>或空字符串时抛出。</exception>
    public HttpMethod(string value)
    {
        ThrowHelper.ThrowArgumentNullExceptionIfStringIsNullOrEmpty(value, nameof(value));
        this.m_value = value.ToUpper();
    }

    /// <summary>
    /// 将字符串隐式转换为<see cref="HttpMethod"/>。
    /// </summary>
    /// <param name="value">HTTP方法的字符串表示。</param>
    public static implicit operator HttpMethod(string value) => new(value);

    /// <summary>
    /// 返回HTTP方法的字符串表示。
    /// </summary>
    /// <returns>HTTP方法的字符串表示。</returns>
    public override string ToString()
    {
        return this.m_value;
    }
}